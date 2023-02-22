using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Threading;

namespace Microsoft.Mobile
{
	public class ProcessLauncher : IDisposable
	{
		private enum StreamIdentifier
		{
			StdOut,
			StdErr,
			Last
		}

		private Action<string> onErrorLine;

		private Action<string> onOutputLine;

		private Action<string> onInfraLine;

		private AutoResetEvent processExitedEvent = new AutoResetEvent(false);

		private string timeoutHandlerScriptPath = null;

		private CountdownEvent allStreamsClosed = new CountdownEvent(2);

		private bool[] streamsClosed = new bool[2];

		private bool timedOut = false;

		private bool attached = false;

		private bool captureOutput = false;

		private bool runToExitCalled = false;

		public Process Process { get; private set; }

		public ProcessStartInfo ProcessStartInfo { get; private set; }

		public bool IsRunning => Process != null && !Process.HasExited;

		public Action<Process> TimeoutHandler { get; set; }

		public bool IsDisposed { get; private set; }

		public ProcessLauncher(string fileName, string arguments)
			: this(fileName, arguments, null, null, null)
		{
		}

		public ProcessLauncher(string fileName, string arguments, Action<string> onErrorLine, Action<string> onOutputLine, Action<string> onInfraLine)
		{
			SetDefaultTimeoutHandler();
			this.onErrorLine = onErrorLine;
			this.onOutputLine = onOutputLine;
			this.onInfraLine = onInfraLine;
			ProcessStartInfo = new ProcessStartInfo
			{
				FileName = fileName,
				Arguments = arguments,
				UseShellExecute = false,
				WindowStyle = ProcessWindowStyle.Hidden,
				CreateNoWindow = true
			};
		}

		public void Start(bool runAttached = true)
		{
			CheckDisposed();
			if (Process != null)
			{
				throw new InvalidOperationException("The ProcessLauncher instance was already executed");
			}
			StartProcess(runAttached);
		}

		public void RunToExit()
		{
			RunToExit(-1);
		}

		public bool RunToExit(int timeoutMs)
		{
			using (AutoResetEvent cancelEvent = new AutoResetEvent(false))
			{
				return RunToExit(timeoutMs, cancelEvent);
			}
		}

		public bool RunToExit(WaitHandle cancelEvent)
		{
			return RunToExit(-1, cancelEvent);
		}

		public bool RunToExit(int timeoutMs, WaitHandle cancelEvent)
		{
			CheckDisposed();
			if (timeoutMs < -1)
			{
				throw new ArgumentOutOfRangeException("timeout cannot be less than -1");
			}
			if (timeoutMs == -1)
			{
				timeoutMs = int.MaxValue;
			}
			bool flag = false;
			if (Process == null)
			{
				bool runAttached = timeoutMs != 0;
				StartProcess(runAttached);
			}
			if (!attached)
			{
				return true;
			}
			try
			{
				runToExitCalled = true;
				switch (WaitHandle.WaitAny(new WaitHandle[2] { processExitedEvent, cancelEvent }, timeoutMs, false))
				{
				case 0:
					flag = true;
					break;
				case 1:
					SafeInvoke(onInfraLine, "Process execution was cancelled externally");
					break;
				case 258:
					timedOut = true;
					SafeInvoke(onInfraLine, "Process execution timed out");
					break;
				}
				if (flag && captureOutput && Process.HasExited)
				{
					int millisecondsTimeout = 5000;
					if (!allStreamsClosed.Wait(millisecondsTimeout))
					{
						for (int i = 0; i < streamsClosed.Length; i++)
						{
							if (!streamsClosed[i])
							{
								SafeInvoke(onInfraLine, $"Timed out waiting on stream close signal for {(StreamIdentifier)i}");
							}
						}
					}
				}
				return flag;
			}
			catch (Exception ex)
			{
				SafeInvoke(onInfraLine, "failed during process execution: " + ex);
				throw;
			}
			finally
			{
				CleanupProcessInstance(!flag);
			}
		}

		private void RunTimeoutHandler(Process dyingProcess)
		{
			string text = timeoutHandlerScriptPath ?? Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "TimeoutHandler.bat");
			if (File.Exists(text))
			{
				try
				{
					SafeInvoke(onInfraLine, "Executing handler script for PID={0} NAME={1} at \"{2}\"", dyingProcess.Id, dyingProcess.ProcessName, text);
					string lineHeader = $"[{Path.GetFileName(text)}]: ";
					string systemToolFullPath = GetSystemToolFullPath("cmd");
					string arguments = $"/c \"{text}\" {dyingProcess.Id} {dyingProcess.ProcessName}";
					Action<string> action = delegate(string m)
					{
						SafeInvoke(onErrorLine, lineHeader + m);
					};
					using (ProcessLauncher processLauncher = new ProcessLauncher(systemToolFullPath, arguments, action, delegate(string m)
					{
						SafeInvoke(onOutputLine, lineHeader + m);
					}, delegate(string m)
					{
						SafeInvoke(onInfraLine, lineHeader + m);
					}))
					{
						processLauncher.RunToExit();
						return;
					}
				}
				catch (Exception ex)
				{
					SafeInvoke(onInfraLine, "Exception occurred trying to run handler script.");
					SafeInvoke(onInfraLine, ex.ToString());
					return;
				}
			}
			SafeInvoke(onInfraLine, "No timeout handler script at \"{0}\", skipping.", text);
		}

		public void SetDefaultTimeoutHandler(string timeoutHandlerScriptPath = null)
		{
			TimeoutHandler = RunTimeoutHandler;
			this.timeoutHandlerScriptPath = timeoutHandlerScriptPath;
		}

		private void StartProcess(bool runAttached)
		{
			attached = runAttached;
			captureOutput = attached && (onOutputLine != null || onErrorLine != null);
			StartProcess(attached, captureOutput);
		}

		private void StartProcess(bool enableEvents, bool captureOutput)
		{
			try
			{
				Process = new Process
				{
					StartInfo = ProcessStartInfo,
					EnableRaisingEvents = enableEvents
				};
				if (captureOutput)
				{
					if (onOutputLine != null)
					{
						ProcessStartInfo.RedirectStandardOutput = true;
						Process.OutputDataReceived += OnOutputDataReceived;
					}
					else
					{
						SignalStreamCloseEvent(StreamIdentifier.StdOut);
					}
					if (onErrorLine != null)
					{
						ProcessStartInfo.RedirectStandardError = true;
						Process.ErrorDataReceived += OnErrorDataReceived;
					}
					else
					{
						SignalStreamCloseEvent(StreamIdentifier.StdErr);
					}
				}
				if (enableEvents)
				{
					Process.Exited += OnProcessExited;
				}
				Process.Start();
				if (captureOutput)
				{
					BeginReadingProcessOutput();
				}
			}
			catch (Exception ex)
			{
				SafeInvoke(onInfraLine, "failed before process execution: " + ex);
				throw;
			}
		}

		private void CleanupProcessInstance(bool forceTermination)
		{
			if (Process != null)
			{
				Process.Exited -= OnProcessExited;
				if (onOutputLine != null)
				{
					Process.OutputDataReceived -= OnOutputDataReceived;
				}
				if (onErrorLine != null)
				{
					Process.ErrorDataReceived -= OnErrorDataReceived;
				}
			}
			if (forceTermination)
			{
				ForceCleanup();
			}
		}

		private void BeginReadingProcessOutput()
		{
			if (onOutputLine != null)
			{
				Process.BeginOutputReadLine();
			}
			if (onErrorLine != null)
			{
				Process.BeginErrorReadLine();
			}
		}

		private void OnProcessExited(object sender, EventArgs e)
		{
			processExitedEvent.Set();
		}

		private void OnErrorDataReceived(object sender, DataReceivedEventArgs e)
		{
			if (e.Data != null)
			{
				SafeInvoke(onErrorLine, e.Data);
			}
			else
			{
				SignalStreamCloseEvent(StreamIdentifier.StdErr);
			}
		}

		private void OnOutputDataReceived(object sender, DataReceivedEventArgs e)
		{
			if (e.Data != null)
			{
				SafeInvoke(onOutputLine, e.Data);
			}
			else
			{
				SignalStreamCloseEvent(StreamIdentifier.StdOut);
			}
		}

		public void ForceCleanup()
		{
			if (IsDisposed)
			{
				return;
			}
			try
			{
				if (Process == null || Process.HasExited)
				{
					return;
				}
				Process.CloseMainWindow();
				if (Process.HasExited)
				{
					return;
				}
			}
			catch (Exception ex)
			{
				SafeInvoke(onInfraLine, "failed during process cleanup: " + ex);
			}
			try
			{
				Process.KillProgeny(delegate(Process p)
				{
					if (timedOut && TimeoutHandler != null)
					{
						TimeoutHandler(p);
					}
				});
			}
			catch (Exception ex2)
			{
				SafeInvoke(onInfraLine, "failed to kill process: " + ex2);
			}
		}

		private void CheckDisposed()
		{
			if (IsDisposed)
			{
				throw new ObjectDisposedException("Microsoft.Mobile.ProcessLauncher");
			}
		}

		private void SafeInvoke(Action<string> action, string arg, params object[] formatArgs)
		{
			action?.Invoke((formatArgs.Length == 0) ? arg : string.Format(arg, formatArgs));
		}

		private void SignalStreamCloseEvent(StreamIdentifier streamId)
		{
			allStreamsClosed.Signal();
			streamsClosed[(int)streamId] = true;
		}

		private static string GetSystemToolFullPath(string tool)
		{
			return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.System), tool);
		}

		public void Dispose()
		{
			bool flag = true;
			if (!IsDisposed)
			{
				if (Process != null)
				{
					if (attached && !runToExitCalled)
					{
						flag = false;
					}
					Process.Dispose();
					Process = null;
				}
				IsDisposed = true;
			}
			if (!flag)
			{
				throw new InvalidOperationException("A ProcessLauncher instance is now being disposed, which was used to launch a waitable process without ever calling RunToExit().  Either make it asynchronous, or call RunToExit().");
			}
		}
	}
}
