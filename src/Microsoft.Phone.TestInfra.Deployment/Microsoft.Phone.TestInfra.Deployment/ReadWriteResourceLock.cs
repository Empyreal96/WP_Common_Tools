using System;
using System.Security.Permissions;
using System.Threading;

namespace Microsoft.Phone.TestInfra.Deployment
{
	[HostProtection(SecurityAction.LinkDemand, MayLeakOnAbort = true)]
	[HostProtection(SecurityAction.LinkDemand, Synchronization = true, ExternalThreading = true)]
	public sealed class ReadWriteResourceLock : IDisposable
	{
		private static readonly int MaxReaders;

		private readonly object syncRoot;

		private Mutex mutex;

		private Semaphore semaphore;

		private volatile int readLockCount;

		private volatile int writeLockCount;

		private volatile bool disposed;

		public bool IsReadLockHeld => readLockCount > 0;

		public bool IsWriteLockHeld => writeLockCount > 0;

		static ReadWriteResourceLock()
		{
			MaxReaders = Settings.Default.MaxConcurrentReaders;
		}

		public ReadWriteResourceLock(string name)
		{
			if (string.IsNullOrEmpty(name))
			{
				throw new ArgumentNullException("name");
			}
			syncRoot = new object();
			mutex = new Mutex(false, "RWRL_M_" + name);
			semaphore = new Semaphore(MaxReaders, MaxReaders, "RWRL_S_" + name);
		}

		~ReadWriteResourceLock()
		{
			Dispose(false);
		}

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		public void AcquireReadLock()
		{
			AcquireReadLock(TimeoutHelper.InfiniteTimeSpan);
		}

		public void AcquireReadLock(int timeoutMilliseconds)
		{
			AcquireReadLock(TimeSpan.FromMilliseconds(timeoutMilliseconds));
		}

		public void AcquireReadLock(TimeSpan timeout)
		{
			PerformanceCounters.Instance.TimeWaitingOnReadlock.Start();
			try
			{
				lock (syncRoot)
				{
					VerifyNotDisposed();
					if (writeLockCount > 0)
					{
						return;
					}
					if (readLockCount > 0)
					{
						readLockCount++;
						return;
					}
					TimeoutHelper timeoutHelper = new TimeoutHelper(timeout);
					bool flag = false;
					try
					{
						mutex.Acquire(timeoutHelper);
						flag = true;
						semaphore.Acquire(timeoutHelper);
					}
					finally
					{
						if (flag)
						{
							mutex.ReleaseMutex();
						}
					}
					readLockCount++;
				}
			}
			finally
			{
				PerformanceCounters.Instance.TimeWaitingOnReadlock.Stop();
			}
		}

		public bool TryToAcquireReadLock()
		{
			try
			{
				AcquireReadLock(TimeSpan.Zero);
			}
			catch (TimeoutException)
			{
				return false;
			}
			return true;
		}

		public void ReleaseReadLock()
		{
			lock (syncRoot)
			{
				VerifyNotDisposed();
				if (readLockCount == 0 || writeLockCount > 0)
				{
					throw new SynchronizationLockException("The current application has not entered the lock in read mode.");
				}
				readLockCount--;
				if (readLockCount == 0)
				{
					semaphore.Release();
				}
			}
		}

		public void AcquireWriteLock()
		{
			AcquireWriteLock(TimeoutHelper.InfiniteTimeSpan);
		}

		public void AcquireWriteLock(int timeoutMilliseconds)
		{
			AcquireWriteLock(TimeSpan.FromMilliseconds(timeoutMilliseconds));
		}

		public void AcquireWriteLock(TimeSpan timeout)
		{
			PerformanceCounters.Instance.TimeWaitingOnWritelock.Start();
			try
			{
				lock (syncRoot)
				{
					VerifyNotDisposed();
					if (writeLockCount > 0)
					{
						writeLockCount++;
						return;
					}
					TimeoutHelper timeoutHelper = new TimeoutHelper(timeout);
					while (readLockCount > 0)
					{
						ReleaseReadLock();
					}
					bool flag = false;
					int i = 0;
					try
					{
						mutex.Acquire(timeoutHelper);
						flag = true;
						for (i = 0; i < MaxReaders; i++)
						{
							semaphore.Acquire(timeoutHelper);
						}
					}
					catch
					{
						if (i > 0)
						{
							semaphore.Release(i);
						}
						throw;
					}
					finally
					{
						if (flag)
						{
							mutex.ReleaseMutex();
						}
					}
					writeLockCount++;
				}
			}
			finally
			{
				PerformanceCounters.Instance.TimeWaitingOnWritelock.Stop();
			}
		}

		public bool TryToAcquireWriteLock()
		{
			try
			{
				AcquireWriteLock(TimeSpan.Zero);
			}
			catch (TimeoutException)
			{
				return false;
			}
			return true;
		}

		public void ReleaseWriteLock()
		{
			lock (syncRoot)
			{
				VerifyNotDisposed();
				if (writeLockCount == 0)
				{
					throw new SynchronizationLockException("The current application has not acquired write lock.");
				}
				writeLockCount--;
				if (writeLockCount == 0)
				{
					semaphore.Release(MaxReaders);
				}
			}
		}

		private void VerifyNotDisposed()
		{
			lock (syncRoot)
			{
				if (disposed)
				{
					throw new ObjectDisposedException("Current instance is already disposed");
				}
			}
		}

		private void Dispose(bool disposing)
		{
			lock (syncRoot)
			{
				if (!disposed)
				{
					if (writeLockCount > 0)
					{
						ReleaseWriteLock();
					}
					if (readLockCount > 0)
					{
						ReleaseReadLock();
					}
					if (mutex != null)
					{
						mutex.Dispose();
						mutex = null;
					}
					if (semaphore != null)
					{
						semaphore.Dispose();
						semaphore = null;
					}
					disposed = true;
				}
			}
		}
	}
}
