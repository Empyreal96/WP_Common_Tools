using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Windows.Forms;
using DeviceHealth;
using USB_Test_Infrastructure;

namespace UEFIUSBFnDevTester
{
	public class Form1 : Form
	{
		public class StringRedir : StringWriter
		{
			private delegate void SetTextCallback(string text);

			public RichTextBox outBox;

			public StringRedir(ref RichTextBox textBox)
			{
				outBox = textBox;
			}

			public override void WriteLine(string x)
			{
				if (outBox.InvokeRequired)
				{
					SetTextCallback method = WriteLine;
					outBox.Invoke(method, x);
					return;
				}
				if ("\f" == x)
				{
					outBox.Clear();
				}
				RichTextBox richTextBox = outBox;
				richTextBox.Text = richTextBox.Text + x + "\n";
				outBox.Refresh();
			}

			public override void Write(string x)
			{
				if (outBox.InvokeRequired)
				{
					SetTextCallback method = Write;
					outBox.Invoke(method, x);
					return;
				}
				if ("\f" == x)
				{
					outBox.Clear();
				}
				outBox.Text += x;
				outBox.Refresh();
			}
		}

		private delegate void DS(Button b);

		public DTSFUsbStream usb;

		public Tests test;

		public UsbConnectionManager cm;

		private string CurrentDevice;

		private ManualResetEvent inReset;

		private Queue<Button> jobQueue = new Queue<Button>();

		private StringRedir rtbredirect;

		private IContainer components;

		private Button MTUPerfButton;

		private Button MaxMTU;

		private Button Sweep512Button;

		private Button MaxPacketButton;

		private GroupBox groupBox1;

		private RichTextBox richTextBox1;

		private Button ResetButton;

		private Button ConnectionStatusButton;

		private Button Sweep1024Button;

		private Button MTULongRunButton;

		private Button CmdCancelButton;

		private Button OneGBTest;

		private Button ReconnectTest;

		private Button MaxTransferSweep;

		private Button OneGBOutOnly;

		private CheckBox checkBox1;

		private Button btnStartTest;

		private CheckBox checkBox10;

		private CheckBox checkBox9;

		private CheckBox checkBox8;

		private CheckBox checkBox7;

		private CheckBox checkBox6;

		private CheckBox checkBox5;

		private CheckBox checkBox4;

		private CheckBox checkBox3;

		private CheckBox checkBox2;

		private Button btnClearLogFrame;

		private static void DisconnectStatusDM(Button b)
		{
			b.BackColor = Color.Red;
			b.Text = "No connection";
		}

		private static void ConnectStatusDM(Button b)
		{
			b.BackColor = Color.Green;
			b.Text = "Connected";
		}

		public Form1()
		{
			InitializeComponent();
			inReset = new ManualResetEvent(true);
			cm = new UsbConnectionManager(delegate(string connectDeviceId)
			{
				Log.Info("ConnectDeviceId = {0}", connectDeviceId);
				try
				{
					usb = new DTSFUsbStream(connectDeviceId);
					test = new Tests();
					DS method = ConnectStatusDM;
					CurrentDevice = connectDeviceId;
					ConnectionStatusButton.Invoke(method, ConnectionStatusButton);
					inReset.Set();
				}
				catch (Exception)
				{
					Log.Info("Connection Failed: {0}", connectDeviceId);
					if (usb != null)
					{
						usb.Dispose();
					}
					if (test != null)
					{
						test = null;
					}
					DS method2 = DisconnectStatusDM;
					ConnectionStatusButton.Invoke(method2, ConnectionStatusButton);
				}
			}, delegate(string disconnectDeviceId)
			{
				Log.Info("DisonnectDeviceId = {0}", disconnectDeviceId);
				if (usb != null)
				{
					usb.Dispose();
					usb = null;
				}
				test = null;
				ConnectionStatusButton.BackColor = Color.Red;
				ConnectionStatusButton.Text = "No connection";
			});
			rtbredirect = new StringRedir(ref richTextBox1);
			Console.SetOut(rtbredirect);
		}

		public void ButtonCallback(bool result, object obj)
		{
			if (obj != null)
			{
				if (result)
				{
					((Button)obj).BackColor = Color.Green;
				}
				else
				{
					((Button)obj).BackColor = Color.Red;
				}
				if (jobQueue.Count > 0)
				{
					jobQueue.Dequeue().PerformClick();
				}
				else if (!test.EnableLogHeaderFooter)
				{
					test.PrintLogFooter();
					test.EnableLogHeaderFooter = true;
					btnStartTest.Enabled = true;
				}
			}
		}

		private void Form1_Load(object sender, EventArgs e)
		{
			Text = "UEFI SimpleIO USB test " + Assembly.GetExecutingAssembly().GetName().Version.ToString();
			cm.Start();
		}

		private void MTUPerfButton_Click(object sender, EventArgs e)
		{
			if (test.EnableLogHeaderFooter)
			{
				test.resetGlobalResults();
			}
			if (test != null && test.TestRunning == 0)
			{
				test.TestCaseMaxPacketPerf(usb, ButtonCallback, MTUPerfButton);
			}
		}

		private void MaxMTU_Click(object sender, EventArgs e)
		{
			if (test.EnableLogHeaderFooter)
			{
				test.resetGlobalResults();
			}
			if (test != null && test.TestRunning == 0)
			{
				test.TestCaseMaxMTU(usb, ButtonCallback, MaxMTU);
			}
		}

		private void Sweep512Button_Click(object sender, EventArgs e)
		{
			if (test.EnableLogHeaderFooter)
			{
				test.resetGlobalResults();
			}
			if (test != null && test.TestRunning == 0)
			{
				test.TestCase512Sweep(usb, ButtonCallback, Sweep512Button);
			}
		}

		private void MaxPacketButton_Click(object sender, EventArgs e)
		{
			if (test.EnableLogHeaderFooter)
			{
				test.resetGlobalResults();
			}
			if (test != null && test.TestRunning == 0)
			{
				test.TestCaseMaxPacket(usb, ButtonCallback, MaxPacketButton);
			}
		}

		private void ResetButton_Click_Thread()
		{
			if (test != null)
			{
				test.TestCancel.Set();
			}
			Thread.Sleep(10);
			if (usb != null)
			{
				usb.Dispose();
				usb = null;
			}
			if (cm != null)
			{
				cm.Stop();
				cm.Start();
			}
			inReset.Set();
		}

		private void ResetButton_Click(object sender, EventArgs e)
		{
			try
			{
				if (inReset.WaitOne(0))
				{
					inReset.Reset();
					new Thread(ResetButton_Click_Thread).Start();
				}
				btnStartTest.Enabled = true;
			}
			catch
			{
			}
		}

		private void MTULongRunButton_Click(object sender, EventArgs e)
		{
			if (test.EnableLogHeaderFooter)
			{
				test.resetGlobalResults();
			}
			if (test != null && test.TestRunning == 0)
			{
				test.TestCaseMaxTransferLongRun(usb, ButtonCallback, MTULongRunButton);
			}
		}

		private void Sweep1024Button_Click(object sender, EventArgs e)
		{
			if (test.EnableLogHeaderFooter)
			{
				test.resetGlobalResults();
			}
			if (test != null && test.TestRunning == 0)
			{
				test.TestCase1024Sweep(usb, ButtonCallback, Sweep1024Button);
			}
		}

		private void CmdCancelButton_Click(object sender, EventArgs e)
		{
			if (test != null)
			{
				test.TestCancel.Set();
			}
		}

		private void OneGBTest_Click(object sender, EventArgs e)
		{
			if (test.EnableLogHeaderFooter)
			{
				test.resetGlobalResults();
			}
			if (test != null && test.TestRunning == 0)
			{
				test.TestCaseMaxPacket1G(usb, ButtonCallback, OneGBTest);
			}
		}

		private void ReconnectTest_Click_Thread()
		{
			while (!test.TestCancel.WaitOne(0))
			{
				inReset.WaitOne();
				inReset.Reset();
				Log.Info("DisonnectDeviceId = {0}", CurrentDevice);
				if (usb != null)
				{
					usb.Dispose();
					usb = null;
				}
				if (cm != null)
				{
					cm.Stop();
					cm.Start();
				}
			}
		}

		private void ReconnectTest_Click(object sender, EventArgs e)
		{
			if (test.EnableLogHeaderFooter)
			{
				test.resetGlobalResults();
			}
			try
			{
				if (inReset.WaitOne(0))
				{
					new Thread(ReconnectTest_Click_Thread).Start();
				}
			}
			catch
			{
			}
		}

		private void MaxTransferSweep_Click(object sender, EventArgs e)
		{
			if (test.EnableLogHeaderFooter)
			{
				test.resetGlobalResults();
			}
			if (test != null && test.TestRunning == 0)
			{
				test.TestCaseMaxTransferSweep(usb, ButtonCallback, MaxTransferSweep);
			}
		}

		private void OneGBOutOnly_Click(object sender, EventArgs e)
		{
			if (test.EnableLogHeaderFooter)
			{
				test.resetGlobalResults();
			}
			if (test != null && test.TestRunning == 0)
			{
				test.TestCaseMaxPacket1GOutOnly(usb, ButtonCallback, OneGBOutOnly);
			}
		}

		private void richTextBox1_TextChanged(object sender, EventArgs e)
		{
			richTextBox1.SelectionStart = richTextBox1.Text.Length;
			richTextBox1.ScrollToCaret();
		}

		private void groupBox1_Enter(object sender, EventArgs e)
		{
		}

		private void btnStartTest_Click(object sender, EventArgs e)
		{
			if (!checkBox1.Checked && !checkBox2.Checked && !checkBox3.Checked && !checkBox4.Checked && !checkBox5.Checked && !checkBox6.Checked && !checkBox7.Checked && !checkBox8.Checked && !checkBox9.Checked && !checkBox10.Checked)
			{
				Log.Info("No tests were added to this run.");
				return;
			}
			btnStartTest.Enabled = false;
			test.resetGlobalResults();
			if (checkBox1.Checked)
			{
				jobQueue.Enqueue(MaxMTU);
			}
			if (checkBox2.Checked)
			{
				jobQueue.Enqueue(MaxPacketButton);
			}
			if (checkBox3.Checked)
			{
				jobQueue.Enqueue(Sweep512Button);
			}
			if (checkBox4.Checked)
			{
				jobQueue.Enqueue(Sweep1024Button);
			}
			if (checkBox5.Checked)
			{
				jobQueue.Enqueue(MTUPerfButton);
			}
			if (checkBox6.Checked)
			{
				jobQueue.Enqueue(MTULongRunButton);
			}
			if (checkBox7.Checked)
			{
				jobQueue.Enqueue(OneGBTest);
			}
			if (checkBox8.Checked)
			{
				jobQueue.Enqueue(ReconnectTest);
			}
			if (checkBox9.Checked)
			{
				jobQueue.Enqueue(MaxTransferSweep);
			}
			if (checkBox10.Checked)
			{
				jobQueue.Enqueue(OneGBOutOnly);
			}
			test.EnableLogHeaderFooter = false;
			test.PrintLogHeader(jobQueue.Count);
			jobQueue.Dequeue().PerformClick();
		}

		private void btnClearLogFrame_Click(object sender, EventArgs e)
		{
			richTextBox1.Text = "";
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing && components != null)
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		private void InitializeComponent()
		{
			this.MTUPerfButton = new System.Windows.Forms.Button();
			this.MaxMTU = new System.Windows.Forms.Button();
			this.Sweep512Button = new System.Windows.Forms.Button();
			this.MaxPacketButton = new System.Windows.Forms.Button();
			this.groupBox1 = new System.Windows.Forms.GroupBox();
			this.btnClearLogFrame = new System.Windows.Forms.Button();
			this.btnStartTest = new System.Windows.Forms.Button();
			this.checkBox10 = new System.Windows.Forms.CheckBox();
			this.checkBox9 = new System.Windows.Forms.CheckBox();
			this.checkBox8 = new System.Windows.Forms.CheckBox();
			this.checkBox7 = new System.Windows.Forms.CheckBox();
			this.checkBox6 = new System.Windows.Forms.CheckBox();
			this.checkBox5 = new System.Windows.Forms.CheckBox();
			this.checkBox4 = new System.Windows.Forms.CheckBox();
			this.checkBox3 = new System.Windows.Forms.CheckBox();
			this.checkBox2 = new System.Windows.Forms.CheckBox();
			this.checkBox1 = new System.Windows.Forms.CheckBox();
			this.OneGBOutOnly = new System.Windows.Forms.Button();
			this.MaxTransferSweep = new System.Windows.Forms.Button();
			this.ReconnectTest = new System.Windows.Forms.Button();
			this.OneGBTest = new System.Windows.Forms.Button();
			this.CmdCancelButton = new System.Windows.Forms.Button();
			this.Sweep1024Button = new System.Windows.Forms.Button();
			this.MTULongRunButton = new System.Windows.Forms.Button();
			this.ConnectionStatusButton = new System.Windows.Forms.Button();
			this.ResetButton = new System.Windows.Forms.Button();
			this.richTextBox1 = new System.Windows.Forms.RichTextBox();
			this.groupBox1.SuspendLayout();
			base.SuspendLayout();
			this.MTUPerfButton.Location = new System.Drawing.Point(19, 153);
			this.MTUPerfButton.Name = "MTUPerfButton";
			this.MTUPerfButton.Size = new System.Drawing.Size(112, 23);
			this.MTUPerfButton.TabIndex = 0;
			this.MTUPerfButton.Text = "MTU Perf test";
			this.MTUPerfButton.UseVisualStyleBackColor = true;
			this.MTUPerfButton.Click += new System.EventHandler(MTUPerfButton_Click);
			this.MaxMTU.Location = new System.Drawing.Point(18, 37);
			this.MaxMTU.Name = "MaxMTU";
			this.MaxMTU.Size = new System.Drawing.Size(112, 23);
			this.MaxMTU.TabIndex = 1;
			this.MaxMTU.Text = "Single MTU test";
			this.MaxMTU.UseVisualStyleBackColor = true;
			this.MaxMTU.Click += new System.EventHandler(MaxMTU_Click);
			this.Sweep512Button.Location = new System.Drawing.Point(18, 95);
			this.Sweep512Button.Name = "Sweep512Button";
			this.Sweep512Button.Size = new System.Drawing.Size(112, 23);
			this.Sweep512Button.TabIndex = 2;
			this.Sweep512Button.Text = "512 byte sweep test";
			this.Sweep512Button.UseVisualStyleBackColor = true;
			this.Sweep512Button.Click += new System.EventHandler(Sweep512Button_Click);
			this.MaxPacketButton.Location = new System.Drawing.Point(18, 66);
			this.MaxPacketButton.Name = "MaxPacketButton";
			this.MaxPacketButton.Size = new System.Drawing.Size(112, 23);
			this.MaxPacketButton.TabIndex = 3;
			this.MaxPacketButton.Text = "Max Packet test";
			this.MaxPacketButton.UseVisualStyleBackColor = true;
			this.MaxPacketButton.Click += new System.EventHandler(MaxPacketButton_Click);
			this.groupBox1.Controls.Add(this.btnClearLogFrame);
			this.groupBox1.Controls.Add(this.btnStartTest);
			this.groupBox1.Controls.Add(this.ConnectionStatusButton);
			this.groupBox1.Controls.Add(this.checkBox10);
			this.groupBox1.Controls.Add(this.checkBox9);
			this.groupBox1.Controls.Add(this.checkBox8);
			this.groupBox1.Controls.Add(this.checkBox7);
			this.groupBox1.Controls.Add(this.checkBox6);
			this.groupBox1.Controls.Add(this.checkBox5);
			this.groupBox1.Controls.Add(this.checkBox4);
			this.groupBox1.Controls.Add(this.checkBox3);
			this.groupBox1.Controls.Add(this.checkBox2);
			this.groupBox1.Controls.Add(this.checkBox1);
			this.groupBox1.Controls.Add(this.OneGBOutOnly);
			this.groupBox1.Controls.Add(this.MaxTransferSweep);
			this.groupBox1.Controls.Add(this.ReconnectTest);
			this.groupBox1.Controls.Add(this.OneGBTest);
			this.groupBox1.Controls.Add(this.CmdCancelButton);
			this.groupBox1.Controls.Add(this.Sweep1024Button);
			this.groupBox1.Controls.Add(this.MTULongRunButton);
			this.groupBox1.Controls.Add(this.ResetButton);
			this.groupBox1.Controls.Add(this.MaxMTU);
			this.groupBox1.Controls.Add(this.Sweep512Button);
			this.groupBox1.Controls.Add(this.MaxPacketButton);
			this.groupBox1.Controls.Add(this.MTUPerfButton);
			this.groupBox1.Location = new System.Drawing.Point(12, 12);
			this.groupBox1.Name = "groupBox1";
			this.groupBox1.Size = new System.Drawing.Size(200, 475);
			this.groupBox1.TabIndex = 4;
			this.groupBox1.TabStop = false;
			this.groupBox1.Text = "UEFI USBFn Loopback tests";
			this.groupBox1.Enter += new System.EventHandler(groupBox1_Enter);
			this.btnClearLogFrame.Location = new System.Drawing.Point(19, 357);
			this.btnClearLogFrame.Name = "btnClearLogFrame";
			this.btnClearLogFrame.Size = new System.Drawing.Size(80, 25);
			this.btnClearLogFrame.TabIndex = 24;
			this.btnClearLogFrame.Text = "Clear";
			this.btnClearLogFrame.UseVisualStyleBackColor = true;
			this.btnClearLogFrame.Click += new System.EventHandler(btnClearLogFrame_Click);
			this.btnStartTest.Location = new System.Drawing.Point(19, 328);
			this.btnStartTest.Name = "btnStartTest";
			this.btnStartTest.Size = new System.Drawing.Size(80, 25);
			this.btnStartTest.TabIndex = 23;
			this.btnStartTest.Text = "Start Test";
			this.btnStartTest.UseVisualStyleBackColor = true;
			this.btnStartTest.Click += new System.EventHandler(btnStartTest_Click);
			this.checkBox10.AutoSize = true;
			this.checkBox10.Location = new System.Drawing.Point(136, 303);
			this.checkBox10.Name = "checkBox10";
			this.checkBox10.Size = new System.Drawing.Size(45, 17);
			this.checkBox10.TabIndex = 22;
			this.checkBox10.Text = "Add";
			this.checkBox10.UseVisualStyleBackColor = true;
			this.checkBox10.Visible = false;
			this.checkBox9.AutoSize = true;
			this.checkBox9.Location = new System.Drawing.Point(136, 273);
			this.checkBox9.Name = "checkBox9";
			this.checkBox9.Size = new System.Drawing.Size(45, 17);
			this.checkBox9.TabIndex = 21;
			this.checkBox9.Text = "Add";
			this.checkBox9.UseVisualStyleBackColor = true;
			this.checkBox8.AutoSize = true;
			this.checkBox8.Location = new System.Drawing.Point(136, 244);
			this.checkBox8.Name = "checkBox8";
			this.checkBox8.Size = new System.Drawing.Size(45, 17);
			this.checkBox8.TabIndex = 20;
			this.checkBox8.Text = "Add";
			this.checkBox8.UseVisualStyleBackColor = true;
			this.checkBox7.AutoSize = true;
			this.checkBox7.Location = new System.Drawing.Point(136, 215);
			this.checkBox7.Name = "checkBox7";
			this.checkBox7.Size = new System.Drawing.Size(45, 17);
			this.checkBox7.TabIndex = 19;
			this.checkBox7.Text = "Add";
			this.checkBox7.UseVisualStyleBackColor = true;
			this.checkBox6.AutoSize = true;
			this.checkBox6.Location = new System.Drawing.Point(136, 186);
			this.checkBox6.Name = "checkBox6";
			this.checkBox6.Size = new System.Drawing.Size(45, 17);
			this.checkBox6.TabIndex = 18;
			this.checkBox6.Text = "Add";
			this.checkBox6.UseVisualStyleBackColor = true;
			this.checkBox5.AutoSize = true;
			this.checkBox5.Location = new System.Drawing.Point(136, 157);
			this.checkBox5.Name = "checkBox5";
			this.checkBox5.Size = new System.Drawing.Size(45, 17);
			this.checkBox5.TabIndex = 17;
			this.checkBox5.Text = "Add";
			this.checkBox5.UseVisualStyleBackColor = true;
			this.checkBox4.AutoSize = true;
			this.checkBox4.Location = new System.Drawing.Point(136, 128);
			this.checkBox4.Name = "checkBox4";
			this.checkBox4.Size = new System.Drawing.Size(45, 17);
			this.checkBox4.TabIndex = 16;
			this.checkBox4.Text = "Add";
			this.checkBox4.UseVisualStyleBackColor = true;
			this.checkBox3.AutoSize = true;
			this.checkBox3.Location = new System.Drawing.Point(136, 99);
			this.checkBox3.Name = "checkBox3";
			this.checkBox3.Size = new System.Drawing.Size(45, 17);
			this.checkBox3.TabIndex = 15;
			this.checkBox3.Text = "Add";
			this.checkBox3.UseVisualStyleBackColor = true;
			this.checkBox2.AutoSize = true;
			this.checkBox2.Location = new System.Drawing.Point(136, 70);
			this.checkBox2.Name = "checkBox2";
			this.checkBox2.Size = new System.Drawing.Size(45, 17);
			this.checkBox2.TabIndex = 14;
			this.checkBox2.Text = "Add";
			this.checkBox2.UseVisualStyleBackColor = true;
			this.checkBox1.AutoSize = true;
			this.checkBox1.Location = new System.Drawing.Point(136, 41);
			this.checkBox1.Name = "checkBox1";
			this.checkBox1.Size = new System.Drawing.Size(45, 17);
			this.checkBox1.TabIndex = 13;
			this.checkBox1.Text = "Add";
			this.checkBox1.UseVisualStyleBackColor = true;
			this.OneGBOutOnly.Location = new System.Drawing.Point(19, 299);
			this.OneGBOutOnly.Name = "OneGBOutOnly";
			this.OneGBOutOnly.Size = new System.Drawing.Size(110, 23);
			this.OneGBOutOnly.TabIndex = 12;
			this.OneGBOutOnly.Text = "1GB Out Only";
			this.OneGBOutOnly.UseVisualStyleBackColor = true;
			this.OneGBOutOnly.Visible = false;
			this.OneGBOutOnly.Click += new System.EventHandler(OneGBOutOnly_Click);
			this.MaxTransferSweep.Location = new System.Drawing.Point(19, 269);
			this.MaxTransferSweep.Name = "MaxTransferSweep";
			this.MaxTransferSweep.Size = new System.Drawing.Size(110, 23);
			this.MaxTransferSweep.TabIndex = 11;
			this.MaxTransferSweep.Text = "MTU Sweep";
			this.MaxTransferSweep.UseVisualStyleBackColor = true;
			this.MaxTransferSweep.Click += new System.EventHandler(MaxTransferSweep_Click);
			this.ReconnectTest.Location = new System.Drawing.Point(18, 240);
			this.ReconnectTest.Name = "ReconnectTest";
			this.ReconnectTest.Size = new System.Drawing.Size(111, 23);
			this.ReconnectTest.TabIndex = 10;
			this.ReconnectTest.Text = "Connection test";
			this.ReconnectTest.UseVisualStyleBackColor = true;
			this.ReconnectTest.Click += new System.EventHandler(ReconnectTest_Click);
			this.OneGBTest.Location = new System.Drawing.Point(18, 211);
			this.OneGBTest.Name = "OneGBTest";
			this.OneGBTest.Size = new System.Drawing.Size(111, 23);
			this.OneGBTest.TabIndex = 9;
			this.OneGBTest.Text = "1GB Throughput Test";
			this.OneGBTest.UseVisualStyleBackColor = true;
			this.OneGBTest.Click += new System.EventHandler(OneGBTest_Click);
			this.CmdCancelButton.Location = new System.Drawing.Point(101, 328);
			this.CmdCancelButton.Name = "CmdCancelButton";
			this.CmdCancelButton.Size = new System.Drawing.Size(80, 25);
			this.CmdCancelButton.TabIndex = 8;
			this.CmdCancelButton.Text = "Stop Test";
			this.CmdCancelButton.UseVisualStyleBackColor = true;
			this.CmdCancelButton.Click += new System.EventHandler(CmdCancelButton_Click);
			this.Sweep1024Button.Location = new System.Drawing.Point(19, 124);
			this.Sweep1024Button.Name = "Sweep1024Button";
			this.Sweep1024Button.Size = new System.Drawing.Size(111, 23);
			this.Sweep1024Button.TabIndex = 7;
			this.Sweep1024Button.Text = "1024 byte sweep test";
			this.Sweep1024Button.UseVisualStyleBackColor = true;
			this.Sweep1024Button.Click += new System.EventHandler(Sweep1024Button_Click);
			this.MTULongRunButton.Location = new System.Drawing.Point(18, 182);
			this.MTULongRunButton.Name = "MTULongRunButton";
			this.MTULongRunButton.Size = new System.Drawing.Size(111, 23);
			this.MTULongRunButton.TabIndex = 6;
			this.MTULongRunButton.Text = "MTU Long Run";
			this.MTULongRunButton.UseVisualStyleBackColor = true;
			this.MTULongRunButton.Click += new System.EventHandler(MTULongRunButton_Click);
			this.ConnectionStatusButton.BackColor = System.Drawing.Color.Red;
			this.ConnectionStatusButton.Location = new System.Drawing.Point(19, 388);
			this.ConnectionStatusButton.Name = "ConnectionStatusButton";
			this.ConnectionStatusButton.Size = new System.Drawing.Size(162, 81);
			this.ConnectionStatusButton.TabIndex = 5;
			this.ConnectionStatusButton.Text = "No Connection";
			this.ConnectionStatusButton.UseVisualStyleBackColor = false;
			this.ResetButton.Location = new System.Drawing.Point(101, 357);
			this.ResetButton.Name = "ResetButton";
			this.ResetButton.Size = new System.Drawing.Size(80, 25);
			this.ResetButton.TabIndex = 4;
			this.ResetButton.Text = "Reset";
			this.ResetButton.UseVisualStyleBackColor = true;
			this.ResetButton.Click += new System.EventHandler(ResetButton_Click);
			this.richTextBox1.Location = new System.Drawing.Point(218, 12);
			this.richTextBox1.Name = "richTextBox1";
			this.richTextBox1.ReadOnly = true;
			this.richTextBox1.ScrollBars = System.Windows.Forms.RichTextBoxScrollBars.ForcedVertical;
			this.richTextBox1.ShowSelectionMargin = true;
			this.richTextBox1.Size = new System.Drawing.Size(584, 475);
			this.richTextBox1.TabIndex = 5;
			this.richTextBox1.Text = "";
			this.richTextBox1.TextChanged += new System.EventHandler(richTextBox1_TextChanged);
			base.AutoScaleDimensions = new System.Drawing.SizeF(6f, 13f);
			base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			base.ClientSize = new System.Drawing.Size(810, 495);
			base.Controls.Add(this.richTextBox1);
			base.Controls.Add(this.groupBox1);
			base.FormBorderStyle = System.Windows.Forms.FormBorderStyle.Fixed3D;
			base.MaximizeBox = false;
			base.Name = "Form1";
			this.Text = "UEFI SimpleIO USB test";
			base.Load += new System.EventHandler(Form1_Load);
			this.groupBox1.ResumeLayout(false);
			this.groupBox1.PerformLayout();
			base.ResumeLayout(false);
		}
	}
}
