using System;
using System.Diagnostics;
using System.Text;
using System.Threading;
using DeviceHealth;

namespace USB_Test_Infrastructure
{
	public class Tests
	{
		private enum Results
		{
			Pass,
			Fail,
			Skip,
			Abort
		}

		public delegate void TestCompleteCallback(bool result, object obj);

		public class TestData
		{
			public DTSFUsbStream usb;

			public TestCompleteCallback completionCallback;

			public object obj;
		}

		private static Random random = new Random();

		private const int MTU = 16376;

		private const int USB_FS_MAX_PACKET_SIZE = 64;

		private const int USB_HS_MAX_PACKET_SIZE = 512;

		private const int ONEGIG = 1073741824;

		public AutoResetEvent TestCancel;

		public int TestRunning;

		public bool EnableLogHeaderFooter = true;

		private int g_iPass;

		private int g_iSkip;

		private int g_iFail;

		private int g_iAbort;

		public Tests()
		{
			TestRunning = 0;
			TestCancel = new AutoResetEvent(false);
			resetGlobalResults();
		}

		public void resetGlobalResults()
		{
			g_iPass = 0;
			g_iSkip = 0;
			g_iFail = 0;
			g_iAbort = 0;
		}

		private static bool ValidateDataEquality(byte[] buffer1, int size1, byte[] buffer2, int size2)
		{
			int num = Math.Min(size1, size2);
			for (int i = 0; i < num; i++)
			{
				if (buffer1[i] != buffer2[i])
				{
					return false;
				}
			}
			return true;
		}

		public bool TestCaseMaxMTU(DTSFUsbStream usb, TestCompleteCallback completionCallback, object obj)
		{
			if (this != null)
			{
				TestData testData = new TestData();
				testData.usb = usb;
				testData.completionCallback = completionCallback;
				testData.obj = obj;
				new Thread(TestCaseMaxMTU).Start(testData);
			}
			return true;
		}

		private void TestCaseMaxMTU(object usbObj)
		{
			TestData testData = (TestData)usbObj;
			DTSFUsbStream usb = testData.usb;
			testData.completionCallback(TestCaseMaxMTU(usb), testData.obj);
		}

		public bool TestCaseMaxPacket(DTSFUsbStream usb, TestCompleteCallback completionCallback, object obj)
		{
			if (this != null)
			{
				TestData testData = new TestData();
				testData.usb = usb;
				testData.completionCallback = completionCallback;
				testData.obj = obj;
				new Thread(TestCaseMaxPacket).Start(testData);
			}
			return true;
		}

		private void TestCaseMaxPacket(object usbObj)
		{
			TestData testData = (TestData)usbObj;
			DTSFUsbStream usb = testData.usb;
			testData.completionCallback(TestCaseMaxPacket(usb), testData.obj);
		}

		public bool TestCaseMaxPacketPerf(DTSFUsbStream usb, TestCompleteCallback completionCallback, object obj)
		{
			if (this != null)
			{
				TestData testData = new TestData();
				testData.usb = usb;
				testData.completionCallback = completionCallback;
				testData.obj = obj;
				new Thread(TestCaseMaxPacketPerf).Start(testData);
			}
			return true;
		}

		private void TestCaseMaxPacketPerf(object usbObj)
		{
			TestData testData = (TestData)usbObj;
			DTSFUsbStream usb = testData.usb;
			testData.completionCallback(TestCaseMaxPacketPerf(usb), testData.obj);
		}

		public bool TestCaseMaxTransferSweep(DTSFUsbStream usb, TestCompleteCallback completionCallback, object obj)
		{
			if (this != null)
			{
				TestData testData = new TestData();
				testData.usb = usb;
				testData.completionCallback = completionCallback;
				testData.obj = obj;
				new Thread(TestCaseMaxTransferSweep).Start(testData);
			}
			return true;
		}

		private void TestCaseMaxTransferSweep(object usbObj)
		{
			TestData testData = (TestData)usbObj;
			DTSFUsbStream usb = testData.usb;
			testData.completionCallback(TestCaseMaxTransferSweep(usb), testData.obj);
		}

		public bool TestCase512Sweep(DTSFUsbStream usb, TestCompleteCallback completionCallback, object obj)
		{
			if (this != null)
			{
				TestData testData = new TestData();
				testData.usb = usb;
				testData.completionCallback = completionCallback;
				testData.obj = obj;
				new Thread(TestCase512Sweep).Start(testData);
			}
			return true;
		}

		private void TestCase512Sweep(object usbObj)
		{
			TestData testData = (TestData)usbObj;
			DTSFUsbStream usb = testData.usb;
			testData.completionCallback(TestCase512Sweep(usb), testData.obj);
		}

		public bool TestCase1024Sweep(DTSFUsbStream usb, TestCompleteCallback completionCallback, object obj)
		{
			if (this != null)
			{
				TestData testData = new TestData();
				testData.usb = usb;
				testData.completionCallback = completionCallback;
				testData.obj = obj;
				new Thread(TestCase1024Sweep).Start(testData);
			}
			return true;
		}

		private void TestCase1024Sweep(object usbObj)
		{
			TestData testData = (TestData)usbObj;
			DTSFUsbStream usb = testData.usb;
			testData.completionCallback(TestCase1024Sweep(usb), testData.obj);
		}

		public bool TestCaseMaxTransferLongRun(DTSFUsbStream usb, TestCompleteCallback completionCallback, object obj)
		{
			if (this != null)
			{
				TestData testData = new TestData();
				testData.usb = usb;
				testData.completionCallback = completionCallback;
				testData.obj = obj;
				new Thread(TestCaseMaxTransferLongRun).Start(testData);
			}
			return true;
		}

		private void TestCaseMaxTransferLongRun(object usbObj)
		{
			TestData testData = (TestData)usbObj;
			DTSFUsbStream usb = testData.usb;
			testData.completionCallback(TestCaseMaxTransferLongRun(usb), testData.obj);
		}

		public bool TestCaseMaxPacket1G(DTSFUsbStream usb, TestCompleteCallback completionCallback, object obj)
		{
			if (this != null)
			{
				TestData testData = new TestData();
				testData.usb = usb;
				testData.completionCallback = completionCallback;
				testData.obj = obj;
				new Thread(TestCaseMaxPacket1G).Start(testData);
			}
			return true;
		}

		private void TestCaseMaxPacket1G(object usbObj)
		{
			TestData testData = (TestData)usbObj;
			DTSFUsbStream usb = testData.usb;
			testData.completionCallback(TestCaseMaxPacket1G(usb), testData.obj);
		}

		public bool TestCaseMaxPacket1GOutOnly(DTSFUsbStream usb, TestCompleteCallback completionCallback, object obj)
		{
			if (this != null)
			{
				TestData testData = new TestData();
				testData.usb = usb;
				testData.completionCallback = completionCallback;
				testData.obj = obj;
				new Thread(TestCaseMaxPacket1GOutOnly).Start(testData);
			}
			return true;
		}

		private void TestCaseMaxPacket1GOutOnly(object usbObj)
		{
			TestData testData = (TestData)usbObj;
			DTSFUsbStream usb = testData.usb;
			testData.completionCallback(TestCaseMaxPacket1GOutOnly(usb), testData.obj);
		}

		public void PrintLogHeader(int numOfTests)
		{
			Log.Info("Tux: Generating WTTLog filename (use -t option to specify)");
			Log.Info("Tux: Attempting Tux's Directory");
			Log.Info("<TESTGROUP>");
			Log.Info("*** ==================================================================");
			Log.Info("*** SUITE INFORMATION");
			Log.Info("***");
			Log.Info("*** Suite Name:        N/A (built on the fly)");
			Log.Info("*** Suite Description: N/A");
			Log.Info("*** Number of Tests:   {0}", numOfTests);
			Log.Info("*** ==================================================================");
			Log.Info("");
			Log.Info("*** ==================================================================");
			Log.Info("*** SYSTEM INFORMATION");
			Log.Info("***");
			Log.Info("*** Date and Time:          11/02/2011  7:58 PM (Wednesday)");
			Log.Info("***");
			Log.Info("*** Computer Name:          \"mobilecore-258\"");
			Log.Info("*** OS Version:             6.02");
			Log.Info("*** Build Number:           8141");
			Log.Info("*** Platform ID:            2 \"Windows NT\"");
			Log.Info("*** Version String:         \"\"");
			Log.Info("***");
			Log.Info("*** Processor Type:         0x00000000 (0) \"Unknown\"");
			Log.Info("*** Processor Architecture: 0x0005     (5) \"ARM\"");
			Log.Info("*** Page Size:              0x00001000 (4,096)");
			Log.Info("*** Minimum App Address:    0x00010000 (65,536)");
			Log.Info("*** Maximum App Address:    0xBFFEFFFF (3,221,159,935)");
			Log.Info("*** Active Processor Mask:  0x00000003");
			Log.Info("*** Number Of Processors:   2");
			Log.Info("*** Allocation Granularity: 0x00010000 (65,536)");
			Log.Info("*** Processor Level:        0x002D     (45)");
			Log.Info("*** Processor Revision:     0x0002     (2)");
			Log.Info("*** ==================================================================");
			Log.Info("");
			Log.Info("*** ==================================================================");
			Log.Info("*** MEMORY INFO");
			Log.Info("***");
			Log.Info("*** Memory Total:   423,452,672 bytes");
			Log.Info("*** Memory Used:    266,821,632 bytes");
			Log.Info("*** Memory Free:    156,631,040 bytes");
			Log.Info("*** ==================================================================");
			Log.Info("");
		}

		public void PrintLogFooter()
		{
			PrintLogFooter(g_iPass, g_iFail, g_iSkip, g_iAbort);
		}

		public void PrintLogFooter(int iPass, int iFail, int iSkip, int iAbort)
		{
			Log.Info("*** ==================================================================");
			Log.Info("*** MEMORY INFO");
			Log.Info("***");
			Log.Info("*** Memory Total:   423,452,672 bytes");
			Log.Info("*** Memory Used:    275,075,072 bytes");
			Log.Info("*** Memory Free:    148,377,600 bytes");
			Log.Info("*** ==================================================================");
			Log.Info("");
			Log.Info("*** ==================================================================");
			Log.Info("*** SUITE SUMMARY");
			Log.Info("***");
			Log.Info("*** Passed:          {0}", iPass);
			Log.Info("*** Failed:          {0}", iFail);
			Log.Info("*** Skipped:         {0}", iSkip);
			Log.Info("*** Aborted:         {0}", iAbort);
			Log.Info("*** -------- ---------");
			Log.Info("*** Total:           {0}", iPass + iFail + iSkip + iAbort);
			Log.Info("***");
			Log.Info("*** Cumulative Test Execution Time: 0:01:00.000");
			Log.Info("*** Total Tux Suite Execution Time: 0:01:00.001");
			Log.Info("*** ==================================================================");
			Log.Info("</TESTGROUP>");
			Log.Info("@@@@@@{0}", iFail + iAbort);
		}

		private void PrintTestHeader(string sTestName, int iTestID)
		{
			Log.Info("<TESTCASE ID={0}>", iTestID);
			Log.Info("*** vvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvv");
			Log.Info("*** TEST STARTING");
			Log.Info("***");
			Log.Info("*** Test Name:      {0}", sTestName);
			Log.Info("*** Test ID:        {0}", iTestID);
			Log.Info("*** Library Path:   siostress.dll");
			Log.Info("*** Command Line:");
			Log.Info("*** Random Seed:    0");
			Log.Info("*** Thread Count:   0");
			Log.Info("*** vvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvv");
		}

		private void PrintTestFooter(string sTestName, int iTestID, Results result)
		{
			Log.Info("*** ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^");
			Log.Info("*** TEST COMPLETED");
			Log.Info("***");
			Log.Info("*** Test Name:      {0}", sTestName);
			Log.Info("*** Test ID:        {0}", iTestID);
			Log.Info("*** Library Path:   siostress.dll");
			Log.Info("*** Command Line:");
			switch (result)
			{
			case Results.Pass:
				Log.Info("*** Result:         Passed");
				break;
			case Results.Fail:
				Log.Info("*** Result:         Failed");
				break;
			case Results.Skip:
				Log.Info("*** Result:         Skipped");
				break;
			case Results.Abort:
				Log.Info("*** Result:         Aborted");
				break;
			}
			Log.Info("*** Random Seed:    0");
			Log.Info("*** Thread Count:   1");
			Log.Info("*** Execution Time: 0:00:01.000");
			Log.Info("*** ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^");
			Log.Info("");
			switch (result)
			{
			case Results.Pass:
				Log.Info("</TESTCASE RESULT=\"PASSED\">");
				break;
			case Results.Fail:
				Log.Info("</TESTCASE RESULT=\"FAILED\">");
				break;
			case Results.Skip:
				Log.Info("</TESTCASE RESULT=\"SKIPPED\">");
				break;
			case Results.Abort:
				Log.Info("</TESTCASE RESULT=\"ABORTED\">");
				break;
			}
		}

		public bool TestCaseMaxMTU(DTSFUsbStream usb)
		{
			byte[] array = new byte[16376];
			byte[] array2 = new byte[16376];
			int num = 0;
			int num2 = 0;
			long num3 = 0L;
			long num4 = 0L;
			TimeSpan timeSpan = default(TimeSpan);
			TimeSpan timeSpan2 = default(TimeSpan);
			Stopwatch stopwatch = new Stopwatch();
			Stopwatch stopwatch2 = new Stopwatch();
			bool flag = true;
			int num5 = 0;
			TestRunning = 1;
			if (EnableLogHeaderFooter)
			{
				PrintLogHeader(1);
			}
			PrintTestHeader("Single transmission max MTU", TestRunning);
			int num6 = 16376;
			FillBuffer(array2, num);
			try
			{
				stopwatch.Start();
				usb.Write(array2, 0, num6);
				stopwatch.Stop();
				Log.Verbose("INFO: [{0}]: TX {1} bytes", num, num6);
				if (num6 % 512 == 0)
				{
					stopwatch.Start();
					usb.Write(array2, 0, 0);
					stopwatch.Stop();
					Log.Verbose("INFO: [{0}]: TX ZLP", num, num6);
				}
				Array.Clear(array, 0, array.Length);
				stopwatch2.Start();
				int num7 = usb.Read(array, 0, array.Length);
				stopwatch2.Stop();
				timeSpan2 = timeSpan2.Add(stopwatch2.Elapsed);
				num4 += num7;
				stopwatch2.Reset();
				num3 += num6;
				timeSpan = timeSpan.Add(stopwatch.Elapsed);
				stopwatch.Reset();
				num++;
				if (num7 == num6 && ValidateDataEquality(array, num7, array2, num6))
				{
					num2++;
					Log.Info("*PASS*: [{0}]: Received {1} bytes as expected", num, num7);
				}
				else
				{
					Log.Error("#FAIL#: [{0}]: Received {1} bytes, expected {2} bytes", num, num7, num6);
					Console.WriteLine("#FAIL#: [{0}]: Received {1} bytes, expected {2} bytes", num, num7, num6);
					StringBuilder stringBuilder = new StringBuilder(Encoding.ASCII.GetString(array2), 0, num6, num6);
					StringBuilder stringBuilder2 = new StringBuilder(Encoding.ASCII.GetString(array), 0, num7, num7);
					Log.Error("#FAIL#: [{0}]: TX: {1})", num, stringBuilder.ToString());
					Log.Error("#FAIL#: [{0}]: RX: {1})", num, stringBuilder2.ToString());
					flag = false;
				}
			}
			catch (Exception e)
			{
				ExceptionSpew(e);
				flag = false;
				num5 = 1;
				g_iAbort++;
			}
			PerfSpew(num3, timeSpan.TotalSeconds, num4, timeSpan2.TotalSeconds);
			if (num5 > 0)
			{
				PrintTestFooter("Single transmission max MTU", TestRunning, Results.Abort);
			}
			else if (!flag)
			{
				PrintTestFooter("Single transmission max MTU", TestRunning, Results.Fail);
				g_iFail++;
			}
			else if (num2 > 0 && num - num2 == 0)
			{
				PrintTestFooter("Single transmission max MTU", TestRunning, Results.Pass);
				g_iPass++;
			}
			else if (num2 == 0 && num == 0)
			{
				PrintTestFooter("Single transmission max MTU", TestRunning, Results.Skip);
				g_iSkip++;
			}
			else
			{
				PrintTestFooter("Single transmission max MTU", TestRunning, Results.Fail);
				g_iFail++;
			}
			if (EnableLogHeaderFooter)
			{
				PrintLogFooter();
			}
			TestRunning = 0;
			return flag;
		}

		public bool TestCaseMaxPacket(DTSFUsbStream usb)
		{
			byte[] array = new byte[16376];
			byte[] array2 = new byte[16376];
			int num = 0;
			int num2 = 0;
			long num3 = 0L;
			long num4 = 0L;
			TimeSpan timeSpan = default(TimeSpan);
			TimeSpan timeSpan2 = default(TimeSpan);
			Stopwatch stopwatch = new Stopwatch();
			Stopwatch stopwatch2 = new Stopwatch();
			int num5 = 0;
			bool flag = true;
			TestRunning = 2;
			if (EnableLogHeaderFooter)
			{
				PrintLogHeader(1);
			}
			PrintTestHeader("Max Packet 512 transmission", TestRunning);
			int num6 = 512;
			FillBuffer(array2, num);
			try
			{
				stopwatch.Start();
				usb.Write(array2, 0, num6);
				stopwatch.Stop();
				Log.Verbose("INFO: [{0}]: TX {1} bytes", num, num6);
				if (num6 % 512 == 0)
				{
					stopwatch.Start();
					usb.Write(array2, 0, 0);
					stopwatch.Stop();
					Log.Verbose("INFO: [{0}]: TX ZLP", num, num6);
				}
				Array.Clear(array, 0, array.Length);
				stopwatch2.Start();
				int num7 = usb.Read(array, 0, array.Length);
				stopwatch2.Stop();
				timeSpan2 = timeSpan2.Add(stopwatch2.Elapsed);
				num4 += num7;
				stopwatch2.Reset();
				num3 += num6;
				timeSpan = timeSpan.Add(stopwatch.Elapsed);
				stopwatch.Reset();
				num++;
				Log.Trace("INFO: [{0}]: Received {1} bytes, expected {2} bytes", num, num7, num6);
				if (num7 == num6 && ValidateDataEquality(array, num7, array2, num6))
				{
					num2++;
					Log.Trace("*PASS*: [{0}]: Received {1} bytes as expected", num, num7);
					Console.WriteLine("*PASS*: [{0}]: Received {1} bytes as expected", num, num7);
				}
				else
				{
					Log.Error("#FAIL#: [{0}]: Received {1} bytes, expected {2} bytes", num, num7, num6);
					Console.WriteLine("#FAIL#: [{0}]: Received {1} bytes, expected {2} bytes", num, num7, num6);
					StringBuilder stringBuilder = new StringBuilder(Encoding.ASCII.GetString(array2), 0, num6, num6);
					StringBuilder stringBuilder2 = new StringBuilder(Encoding.ASCII.GetString(array), 0, num7, num7);
					Log.Error("#FAIL#: [{0}]: TX: {1})", num, stringBuilder.ToString());
					Log.Error("#FAIL#: [{0}]: RX: {1})", num, stringBuilder2.ToString());
					flag = false;
				}
			}
			catch (Exception e)
			{
				ExceptionSpew(e);
				flag = false;
				num5 = 1;
				g_iAbort++;
			}
			Log.Info("*Pass*: {0}, Fail: {1}, Rate: {2}%", num2, num - num2, (double)num2 / (double)num * 100.0);
			PerfSpew(num3, timeSpan.TotalSeconds, num4, timeSpan2.TotalSeconds);
			if (num5 > 0)
			{
				PrintTestFooter("Max Packet 512 transmission", TestRunning, Results.Abort);
			}
			else if (!flag)
			{
				PrintTestFooter("Max Packet 512 transmission", TestRunning, Results.Fail);
				g_iFail++;
			}
			else if (num2 > 0 && num - num2 == 0)
			{
				PrintTestFooter("Max Packet 512 transmission", TestRunning, Results.Pass);
				g_iPass++;
			}
			else if (num2 == 0 && num == 0)
			{
				PrintTestFooter("Max Packet 512 transmission", TestRunning, Results.Skip);
				g_iSkip++;
			}
			else
			{
				PrintTestFooter("Max Packet 512 transmission", TestRunning, Results.Fail);
				g_iFail++;
			}
			if (EnableLogHeaderFooter)
			{
				PrintLogFooter();
			}
			TestRunning = 0;
			return flag;
		}

		public bool TestCaseMaxPacketPerf(DTSFUsbStream usb)
		{
			double num = 0.0;
			double num2 = 0.0;
			byte[] array = new byte[16376];
			byte[] array2 = new byte[16376];
			int num3 = 0;
			int num4 = 0;
			long num5 = 0L;
			long num6 = 0L;
			TimeSpan timeSpan = default(TimeSpan);
			TimeSpan timeSpan2 = default(TimeSpan);
			Stopwatch stopwatch = new Stopwatch();
			Stopwatch stopwatch2 = new Stopwatch();
			int num7 = 0;
			bool flag = true;
			TestRunning = 3;
			if (EnableLogHeaderFooter)
			{
				PrintLogHeader(1);
			}
			PrintTestHeader("Max Packet perf", TestRunning);
			FillBuffer(array2, num3);
			while (1000 > num3)
			{
				int num8 = 16376;
				try
				{
					stopwatch.Start();
					usb.Write(array2, 0, num8);
					stopwatch.Stop();
					if (num8 % 512 == 0)
					{
						stopwatch.Start();
						usb.Write(array2, 0, 0);
						stopwatch.Stop();
					}
					Array.Clear(array, 0, array.Length);
					stopwatch2.Start();
					int num9 = usb.Read(array, 0, array.Length);
					stopwatch2.Stop();
					timeSpan2 = timeSpan2.Add(stopwatch2.Elapsed);
					num6 += num9;
					stopwatch2.Reset();
					num5 += num8;
					timeSpan = timeSpan.Add(stopwatch.Elapsed);
					stopwatch.Reset();
					num3++;
					if (num9 == num8 && ValidateDataEquality(array, num9, array2, num8))
					{
						num4++;
					}
					else
					{
						Log.Error("#FAIL#: [{0}]: Received {1} bytes, expected {2} bytes", num3, num9, num8);
						Console.WriteLine("#FAIL#: [{0}]: Received {1} bytes, expected {2} bytes", num3, num9, num8);
						StringBuilder stringBuilder = new StringBuilder(Encoding.ASCII.GetString(array2), 0, num8, num8);
						StringBuilder stringBuilder2 = new StringBuilder(Encoding.ASCII.GetString(array), 0, num9, num9);
						Log.Error("#FAIL#: [{0}]: TX: {1})", num3, stringBuilder.ToString());
						Log.Error("#FAIL#: [{0}]: RX: {1})", num3, stringBuilder2.ToString());
						flag = false;
					}
				}
				catch (Exception e)
				{
					ExceptionSpew(e);
					flag = false;
					num7 = 1;
					g_iAbort++;
					break;
				}
				if (TestCancel.WaitOne(0))
				{
					flag = false;
					num7 = 1;
					g_iAbort++;
					break;
				}
			}
			Log.Info("*Pass*: {0}, Fail: {1}, Rate: {2}%", num4, num3 - num4, (double)num4 / (double)num3 * 100.0);
			PerfSpew(num5, timeSpan.TotalSeconds, num6, timeSpan2.TotalSeconds);
			num = (double)num5 / timeSpan.TotalSeconds / 1000.0;
			num2 = (double)num6 / timeSpan2.TotalSeconds / 1000.0;
			if (3000.0 > num || 3000.0 > num2)
			{
				Log.Error("#FAIL#: Total throughput too low. Transfer rates must be OVER 3000.0 KBps.");
				flag = false;
			}
			if (num7 > 0)
			{
				PrintTestFooter("Max Packet perf", TestRunning, Results.Abort);
			}
			else if (!flag)
			{
				PrintTestFooter("Max Packet perf", TestRunning, Results.Fail);
				g_iFail++;
			}
			else if (num4 > 0 && num3 - num4 == 0)
			{
				PrintTestFooter("Max Packet perf", TestRunning, Results.Pass);
				g_iPass++;
			}
			else if (num4 == 0 && num3 == 0)
			{
				PrintTestFooter("Max Packet perf", TestRunning, Results.Skip);
				g_iSkip++;
			}
			else
			{
				PrintTestFooter("Max Packet perf", TestRunning, Results.Fail);
				g_iFail++;
			}
			if (EnableLogHeaderFooter)
			{
				PrintLogFooter();
			}
			TestRunning = 0;
			return flag;
		}

		public bool TestCase512Sweep(DTSFUsbStream usb)
		{
			byte[] array = new byte[16376];
			byte[] array2 = new byte[16376];
			int num = 0;
			int num2 = 0;
			long num3 = 0L;
			long num4 = 0L;
			TimeSpan timeSpan = default(TimeSpan);
			TimeSpan timeSpan2 = default(TimeSpan);
			Stopwatch stopwatch = new Stopwatch();
			Stopwatch stopwatch2 = new Stopwatch();
			int num5 = 0;
			bool flag = true;
			TestRunning = 4;
			if (EnableLogHeaderFooter)
			{
				PrintLogHeader(1);
			}
			PrintTestHeader("1-512 sweep transmission", TestRunning);
			while (512 > num)
			{
				int num6 = num + 1;
				FillBuffer(array2, num);
				try
				{
					stopwatch.Start();
					usb.Write(array2, 0, num6);
					stopwatch.Stop();
					Log.Verbose("INFO: [{0}]: TX {1} bytes", num, num6);
					if (num6 % 512 == 0)
					{
						stopwatch.Start();
						usb.Write(array2, 0, 0);
						stopwatch.Stop();
						Log.Verbose("INFO: [{0}]: TX ZLP", num, num6);
					}
					Array.Clear(array, 0, array.Length);
					stopwatch2.Start();
					int num7 = usb.Read(array, 0, array.Length);
					stopwatch2.Stop();
					timeSpan2 = timeSpan2.Add(stopwatch2.Elapsed);
					num4 += num7;
					stopwatch2.Reset();
					num3 += num6;
					timeSpan = timeSpan.Add(stopwatch.Elapsed);
					stopwatch.Reset();
					num++;
					Log.Trace("INFO: [{0}]: Received {1} bytes, expected {2} bytes", num, num7, num6);
					if (num7 == num6 && ValidateDataEquality(array, num7, array2, num6))
					{
						num2++;
						Log.Trace("*PASS*: [{0}]: Received {1} bytes as expected", num, num7);
						Console.WriteLine("*PASS*: [{0}]: Received {1} bytes as expected", num, num7);
					}
					else
					{
						Log.Error("#FAIL#: [{0}]: Received {1} bytes, expected {2} bytes", num, num7, num6);
						Console.WriteLine("#FAIL#: [{0}]: Received {1} bytes, expected {2} bytes", num, num7, num6);
						StringBuilder stringBuilder = new StringBuilder(Encoding.ASCII.GetString(array2), 0, num6, num6);
						StringBuilder stringBuilder2 = new StringBuilder(Encoding.ASCII.GetString(array), 0, num7, num7);
						Log.Error("#FAIL#: [{0}]: TX: {1})", num, stringBuilder.ToString());
						Log.Error("#FAIL#: [{0}]: RX: {1})", num, stringBuilder2.ToString());
						flag = false;
					}
				}
				catch (Exception e)
				{
					ExceptionSpew(e);
					flag = false;
					num5 = 1;
					g_iAbort++;
					break;
				}
				if (TestCancel.WaitOne(0))
				{
					flag = false;
					num5 = 1;
					g_iAbort++;
					break;
				}
			}
			Log.Info("*Pass*: {0}, Fail: {1}, Rate: {2}%", num2, num - num2, (double)num2 / (double)num * 100.0);
			PerfSpew(num3, timeSpan.TotalSeconds, num4, timeSpan2.TotalSeconds);
			if (num5 > 0)
			{
				PrintTestFooter("1-512 sweep transmission", TestRunning, Results.Abort);
			}
			else if (!flag)
			{
				PrintTestFooter("1-512 sweep transmission", TestRunning, Results.Fail);
				g_iFail++;
			}
			else if (num2 > 0 && num - num2 == 0)
			{
				PrintTestFooter("1-512 sweep transmission", TestRunning, Results.Pass);
				g_iPass++;
			}
			else if (num2 == 0 && num == 0)
			{
				PrintTestFooter("1-512 sweep transmission", TestRunning, Results.Skip);
				g_iSkip++;
			}
			else
			{
				PrintTestFooter("1-512 sweep transmission", TestRunning, Results.Fail);
				g_iFail++;
			}
			if (EnableLogHeaderFooter)
			{
				PrintLogFooter();
			}
			TestRunning = 0;
			return flag;
		}

		public bool TestCase1024Sweep(DTSFUsbStream usb)
		{
			byte[] array = new byte[16376];
			byte[] array2 = new byte[16376];
			int num = 0;
			int num2 = 0;
			long num3 = 0L;
			long num4 = 0L;
			TimeSpan timeSpan = default(TimeSpan);
			TimeSpan timeSpan2 = default(TimeSpan);
			Stopwatch stopwatch = new Stopwatch();
			Stopwatch stopwatch2 = new Stopwatch();
			int num5 = 0;
			bool flag = true;
			TestRunning = 5;
			if (EnableLogHeaderFooter)
			{
				PrintLogHeader(1);
			}
			PrintTestHeader("513-1024 sweep transmission", TestRunning);
			while (512 > num)
			{
				int num6 = 512 + num + 1;
				FillBuffer(array2, num);
				try
				{
					stopwatch.Start();
					usb.Write(array2, 0, num6);
					stopwatch.Stop();
					Log.Verbose("INFO: [{0}]: TX {1} bytes", num, num6);
					if (num6 % 512 == 0)
					{
						stopwatch.Start();
						usb.Write(array2, 0, 0);
						stopwatch.Stop();
						Log.Verbose("INFO: [{0}]: TX ZLP", num, num6);
					}
					Array.Clear(array, 0, array.Length);
					stopwatch2.Start();
					int num7 = usb.Read(array, 0, array.Length);
					stopwatch2.Stop();
					timeSpan2 = timeSpan2.Add(stopwatch2.Elapsed);
					num4 += num7;
					stopwatch2.Reset();
					num3 += num6;
					timeSpan = timeSpan.Add(stopwatch.Elapsed);
					stopwatch.Reset();
					num++;
					Log.Trace("INFO: [{0}]: Received {1} bytes, expected {2} bytes", num, num7, num6);
					if (num7 == num6 && ValidateDataEquality(array, num7, array2, num6))
					{
						num2++;
						Log.Info("*PASS*: [{0}]: Received {1} bytes as expected", num, num7);
					}
					else
					{
						Log.Error("#FAIL#: [{0}]: Received {1} bytes, expected {2} bytes", num, num7, num6);
						Console.WriteLine("#FAIL#: [{0}]: Received {1} bytes, expected {2} bytes", num, num7, num6);
						StringBuilder stringBuilder = new StringBuilder(Encoding.ASCII.GetString(array2), 0, num6, num6);
						StringBuilder stringBuilder2 = new StringBuilder(Encoding.ASCII.GetString(array), 0, num7, num7);
						Log.Error("#FAIL#: [{0}]: TX: {1})", num, stringBuilder.ToString());
						Log.Error("#FAIL#: [{0}]: RX: {1})", num, stringBuilder2.ToString());
						flag = false;
					}
				}
				catch (Exception e)
				{
					ExceptionSpew(e);
					flag = false;
					num5 = 1;
					g_iAbort++;
					break;
				}
				if (TestCancel.WaitOne(0))
				{
					flag = false;
					num5 = 1;
					g_iAbort++;
					break;
				}
			}
			Log.Info("*Pass*: {0}, Fail: {1}, Rate: {2}%", num2, num - num2, (double)num2 / (double)num * 100.0);
			PerfSpew(num3, timeSpan.TotalSeconds, num4, timeSpan2.TotalSeconds);
			if (num5 > 0)
			{
				PrintTestFooter("513-1024 sweep transmission", TestRunning, Results.Abort);
			}
			else if (!flag)
			{
				PrintTestFooter("513-1024 sweep transmission", TestRunning, Results.Fail);
				g_iFail++;
			}
			else if (num2 > 0 && num - num2 == 0)
			{
				PrintTestFooter("513-1024 sweep transmission", TestRunning, Results.Pass);
				g_iPass++;
			}
			else if (num2 == 0 && num == 0)
			{
				PrintTestFooter("513-1024 sweep transmission", TestRunning, Results.Skip);
				g_iSkip++;
			}
			else
			{
				PrintTestFooter("513-1024 sweep transmission", TestRunning, Results.Fail);
				g_iFail++;
			}
			if (EnableLogHeaderFooter)
			{
				PrintLogFooter();
			}
			TestRunning = 0;
			return flag;
		}

		public bool TestCaseMaxTransferSweep(DTSFUsbStream usb)
		{
			byte[] array = new byte[16376];
			byte[] array2 = new byte[16376];
			int num = 0;
			int num2 = 0;
			long num3 = 0L;
			long num4 = 0L;
			TimeSpan timeSpan = default(TimeSpan);
			TimeSpan timeSpan2 = default(TimeSpan);
			Stopwatch stopwatch = new Stopwatch();
			Stopwatch stopwatch2 = new Stopwatch();
			int num5 = 0;
			bool flag = true;
			TestRunning = 9;
			if (EnableLogHeaderFooter)
			{
				PrintLogHeader(1);
			}
			PrintTestHeader("1-MTU sweep transmission", TestRunning);
			while (16376 > num)
			{
				int num6 = num + 1;
				FillBuffer(array2, num);
				try
				{
					stopwatch.Start();
					usb.Write(array2, 0, num6);
					stopwatch.Stop();
					Log.Verbose("INFO: [{0}]: TX {1} bytes", num, num6);
					if (num6 % 512 == 0)
					{
						stopwatch.Start();
						usb.Write(array2, 0, 0);
						stopwatch.Stop();
						Log.Verbose("INFO: [{0}]: TX ZLP", num, num6);
					}
					Array.Clear(array, 0, array.Length);
					stopwatch2.Start();
					int num7 = usb.Read(array, 0, array.Length);
					stopwatch2.Stop();
					timeSpan2 = timeSpan2.Add(stopwatch2.Elapsed);
					num4 += num7;
					stopwatch2.Reset();
					num3 += num6;
					timeSpan = timeSpan.Add(stopwatch.Elapsed);
					stopwatch.Reset();
					num++;
					Log.Trace("INFO: [{0}]: Received {1} bytes, expected {2} bytes", num, num7, num6);
					if (num7 == num6 && ValidateDataEquality(array, num7, array2, num6))
					{
						num2++;
						Log.Info("*PASS*: [{0}]: Received {1} bytes as expected.  RX:{2} == TX:{3}", num, num7, array[0], array2[0]);
					}
					else
					{
						Log.Error("#FAIL#: [{0}]: Received {1} bytes, expected {2} bytes", num, num7, num6);
						Console.WriteLine("#FAIL#: [{0}]: Received {1} bytes, expected {2} bytes", num, num7, num6);
						StringBuilder stringBuilder = new StringBuilder(Encoding.ASCII.GetString(array2), 0, num6, num6);
						StringBuilder stringBuilder2 = new StringBuilder(Encoding.ASCII.GetString(array), 0, num7, num7);
						Log.Error("#FAIL#: [{0}]: TX: {1})", num, stringBuilder.ToString());
						Log.Error("#FAIL#: [{0}]: RX: {1})", num, stringBuilder2.ToString());
						flag = false;
					}
				}
				catch (Exception e)
				{
					ExceptionSpew(e);
					flag = false;
					num5 = 1;
					g_iAbort++;
					break;
				}
				if (TestCancel.WaitOne(0))
				{
					flag = false;
					num5 = 1;
					g_iAbort++;
					break;
				}
			}
			Log.Info("*Pass*: {0}, Fail: {1}, Rate: {2}%", num2, num - num2, (double)num2 / (double)num * 100.0);
			PerfSpew(num3, timeSpan.TotalSeconds, num4, timeSpan2.TotalSeconds);
			if (num5 > 0)
			{
				PrintTestFooter("1-MTU sweep transmission", TestRunning, Results.Abort);
			}
			else if (!flag)
			{
				PrintTestFooter("1-MTU sweep transmission", TestRunning, Results.Fail);
				g_iFail++;
			}
			else if (num2 > 0 && num - num2 == 0)
			{
				PrintTestFooter("1-MTU sweep transmission", TestRunning, Results.Pass);
				g_iPass++;
			}
			else if (num2 == 0 && num == 0)
			{
				PrintTestFooter("1-MTU sweep transmission", TestRunning, Results.Skip);
				g_iSkip++;
			}
			else
			{
				PrintTestFooter("1-MTU sweep transmission", TestRunning, Results.Fail);
				g_iFail++;
			}
			if (EnableLogHeaderFooter)
			{
				PrintLogFooter();
			}
			TestRunning = 0;
			return flag;
		}

		public bool TestCaseMaxTransferLongRun(DTSFUsbStream usb)
		{
			byte[] array = new byte[16376];
			byte[] array2 = new byte[16376];
			int num = 0;
			int num2 = 0;
			long num3 = 0L;
			long num4 = 0L;
			TimeSpan timeSpan = default(TimeSpan);
			TimeSpan timeSpan2 = default(TimeSpan);
			Stopwatch stopwatch = new Stopwatch();
			Stopwatch stopwatch2 = new Stopwatch();
			int num5 = 0;
			bool flag = true;
			TestRunning = 6;
			if (EnableLogHeaderFooter)
			{
				PrintLogHeader(1);
			}
			PrintTestHeader("Max Transfer Long Run", TestRunning);
			FillBuffer(array2, num);
			while (true)
			{
				int num6 = 16376;
				try
				{
					stopwatch.Start();
					usb.Write(array2, 0, num6);
					stopwatch.Stop();
					if (num6 % 512 == 0)
					{
						stopwatch.Start();
						usb.Write(array2, 0, 0);
						stopwatch.Stop();
					}
					Array.Clear(array, 0, array.Length);
					stopwatch2.Start();
					int num7 = usb.Read(array, 0, array.Length);
					stopwatch2.Stop();
					timeSpan2 = timeSpan2.Add(stopwatch2.Elapsed);
					num4 += num7;
					num3 += num6;
					timeSpan = timeSpan.Add(stopwatch.Elapsed);
					num++;
					if (num7 == num6 && ValidateDataEquality(array, num7, array2, num6))
					{
						num2++;
						Console.WriteLine("\f");
						Log.Info("*PASS*: [{0}]: Received {1} bytes as expected", num, num7);
					}
					else
					{
						Log.Error("#FAIL#: [{0}]: Received {1} bytes, expected {2} bytes", num, num7, num6);
						Console.WriteLine("#FAIL#: [{0}]: Received {1} bytes, expected {2} bytes", num, num7, num6);
						StringBuilder stringBuilder = new StringBuilder(Encoding.ASCII.GetString(array2), 0, num6, num6);
						StringBuilder stringBuilder2 = new StringBuilder(Encoding.ASCII.GetString(array), 0, num7, num7);
						Log.Error("#FAIL#: [{0}]: TX: {1})", num, stringBuilder.ToString());
						Log.Error("#FAIL#: [{0}]: RX: {1})", num, stringBuilder2.ToString());
						flag = false;
					}
					PerfSpew(num6, stopwatch.Elapsed.TotalSeconds, num7, stopwatch2.Elapsed.TotalSeconds);
					stopwatch2.Reset();
					stopwatch.Reset();
				}
				catch (Exception e)
				{
					ExceptionSpew(e);
					flag = false;
					num5 = 1;
					g_iAbort++;
					break;
				}
				if (TestCancel.WaitOne(0))
				{
					flag = true;
					num5 = 1;
					g_iAbort++;
					break;
				}
			}
			Log.Info("*Pass*: {0}, Fail: {1}, Rate: {2}%", num2, num - num2, (double)num2 / (double)num * 100.0);
			PerfSpew(num3, timeSpan.TotalSeconds, num4, timeSpan2.TotalSeconds);
			if (num5 > 0)
			{
				PrintTestFooter("Max Transfer Long Run", TestRunning, Results.Abort);
			}
			else if (!flag)
			{
				PrintTestFooter("Max Transfer Long Run", TestRunning, Results.Fail);
				g_iFail++;
			}
			else if (num2 > 0 && num - num2 == 0)
			{
				PrintTestFooter("Max Transfer Long Run", TestRunning, Results.Pass);
				g_iPass++;
			}
			else if (num2 == 0 && num == 0)
			{
				PrintTestFooter("Max Transfer Long Run", TestRunning, Results.Skip);
				g_iSkip++;
			}
			else
			{
				PrintTestFooter("Max Transfer Long Run", TestRunning, Results.Fail);
				g_iFail++;
			}
			if (EnableLogHeaderFooter)
			{
				PrintLogFooter();
			}
			TestRunning = 0;
			return flag;
		}

		public bool TestCaseMaxPacket1GOutOnly(DTSFUsbStream usb)
		{
			double num = 0.0;
			double num2 = 0.0;
			byte[] array = new byte[16376];
			byte[] buffer = new byte[16376];
			int num3 = 0;
			int num4 = 0;
			long num5 = 0L;
			long num6 = 0L;
			TimeSpan timeSpan = default(TimeSpan);
			TimeSpan timeSpan2 = default(TimeSpan);
			Stopwatch stopwatch = new Stopwatch();
			new Stopwatch();
			int num7 = 0;
			bool flag = true;
			TestRunning = 7;
			if (EnableLogHeaderFooter)
			{
				PrintLogHeader(1);
			}
			PrintTestHeader("1GB perf", TestRunning);
			FillBuffer(buffer, num3);
			while (65569 > num3)
			{
				int num8 = 16376;
				try
				{
					stopwatch.Start();
					usb.Write(buffer, 0, num8);
					stopwatch.Stop();
					if (num8 % 512 == 0)
					{
						stopwatch.Start();
						usb.Write(buffer, 0, 0);
						stopwatch.Stop();
					}
					num5 += num8;
					timeSpan = timeSpan.Add(stopwatch.Elapsed);
					stopwatch.Reset();
					num3++;
					if (num3 % 65 == 0)
					{
						Console.Write(".");
					}
					if (num3 % 6556 == 0)
					{
						Console.WriteLine("");
						PerfSpew(num5, timeSpan.TotalSeconds, num6, timeSpan2.TotalSeconds);
					}
				}
				catch (Exception e)
				{
					ExceptionSpew(e);
					flag = false;
					num7 = 1;
					g_iAbort++;
					break;
				}
				if (TestCancel.WaitOne(0))
				{
					flag = false;
					num7 = 1;
					g_iAbort++;
					break;
				}
			}
			Log.Info("*Pass*: {0}, Fail: {1}, Rate: {2}%", num4, num3 - num4, (double)num4 / (double)num3 * 100.0);
			PerfSpew(num5, timeSpan.TotalSeconds, 0L, 0.0);
			num = (double)num5 / timeSpan.TotalSeconds / 1000.0;
			num2 = (double)num6 / timeSpan2.TotalSeconds / 1000.0;
			if (3000.0 > num || 3000.0 > num2)
			{
				Log.Error("#WARNING#: Low throughput. Transfer rates should be OVER 3000.0 KBps.");
			}
			if (num7 > 0)
			{
				PrintTestFooter("1GB perf", TestRunning, Results.Abort);
			}
			else if (!flag)
			{
				PrintTestFooter("1GB perf", TestRunning, Results.Fail);
				g_iFail++;
			}
			else if (num4 > 0 && num3 - num4 == 0)
			{
				PrintTestFooter("1GB perf", TestRunning, Results.Pass);
				g_iPass++;
			}
			else if (num4 == 0 && num3 == 0)
			{
				PrintTestFooter("1GB perf", TestRunning, Results.Skip);
				g_iSkip++;
			}
			else
			{
				PrintTestFooter("1GB perf", TestRunning, Results.Fail);
				g_iFail++;
			}
			if (EnableLogHeaderFooter)
			{
				PrintLogFooter();
			}
			TestRunning = 0;
			return flag;
		}

		public bool TestCaseMaxPacket1G(DTSFUsbStream usb)
		{
			double num = 0.0;
			double num2 = 0.0;
			byte[] array = new byte[16376];
			byte[] array2 = new byte[16376];
			int num3 = 0;
			int num4 = 0;
			long num5 = 0L;
			long num6 = 0L;
			TimeSpan timeSpan = default(TimeSpan);
			TimeSpan timeSpan2 = default(TimeSpan);
			Stopwatch stopwatch = new Stopwatch();
			Stopwatch stopwatch2 = new Stopwatch();
			int num7 = 0;
			bool flag = true;
			TestRunning = 7;
			if (EnableLogHeaderFooter)
			{
				PrintLogHeader(1);
			}
			PrintTestHeader("1GB perf", TestRunning);
			FillBuffer(array2, num3);
			while (65569 > num3)
			{
				int num8 = 16376;
				try
				{
					stopwatch.Start();
					usb.Write(array2, 0, num8);
					stopwatch.Stop();
					if (num8 % 512 == 0)
					{
						stopwatch.Start();
						usb.Write(array2, 0, 0);
						stopwatch.Stop();
					}
					Array.Clear(array, 0, array.Length);
					stopwatch2.Start();
					int num9 = usb.Read(array, 0, array.Length);
					stopwatch2.Stop();
					timeSpan2 = timeSpan2.Add(stopwatch2.Elapsed);
					num6 += num9;
					stopwatch2.Reset();
					num5 += num8;
					timeSpan = timeSpan.Add(stopwatch.Elapsed);
					stopwatch.Reset();
					num3++;
					if (num3 % 65 == 0)
					{
						Console.Write(".");
					}
					if (num3 % 6556 == 0)
					{
						Console.WriteLine("");
						PerfSpew(num5, timeSpan.TotalSeconds, num6, timeSpan2.TotalSeconds);
					}
					if (num9 == num8 && ValidateDataEquality(array, num9, array2, num8))
					{
						num4++;
					}
					else
					{
						Log.Error("#FAIL#: [{0}]: Received {1} bytes, expected {2} bytes", num3, num9, num8);
						Console.WriteLine("#FAIL#: [{0}]: Received {1} bytes, expected {2} bytes", num3, num9, num8);
						StringBuilder stringBuilder = new StringBuilder(Encoding.ASCII.GetString(array2), 0, num8, num8);
						StringBuilder stringBuilder2 = new StringBuilder(Encoding.ASCII.GetString(array), 0, num9, num9);
						Log.Error("#FAIL#: [{0}]: TX: {1})", num3, stringBuilder.ToString());
						Log.Error("#FAIL#: [{0}]: RX: {1})", num3, stringBuilder2.ToString());
						flag = false;
					}
				}
				catch (Exception e)
				{
					ExceptionSpew(e);
					flag = false;
					num7 = 1;
					g_iAbort++;
					break;
				}
				if (TestCancel.WaitOne(0))
				{
					flag = false;
					num7 = 1;
					g_iAbort++;
					break;
				}
			}
			Log.Info("*Pass*: {0}, Fail: {1}, Rate: {2}%", num4, num3 - num4, (double)num4 / (double)num3 * 100.0);
			PerfSpew(num5, timeSpan.TotalSeconds, num6, timeSpan2.TotalSeconds);
			num = (double)num5 / timeSpan.TotalSeconds / 1000.0;
			num2 = (double)num6 / timeSpan2.TotalSeconds / 1000.0;
			if (3000.0 > num || 3000.0 > num2)
			{
				Log.Error("#WARNING#: Low throughput. Transfer rates should be OVER 3000.0 KBps.");
			}
			if (num7 > 0)
			{
				PrintTestFooter("1GB perf", TestRunning, Results.Abort);
			}
			else if (!flag)
			{
				PrintTestFooter("1GB perf", TestRunning, Results.Fail);
				g_iFail++;
			}
			else if (num4 > 0 && num3 - num4 == 0)
			{
				PrintTestFooter("1GB perf", TestRunning, Results.Pass);
				g_iPass++;
			}
			else if (num4 == 0 && num3 == 0)
			{
				PrintTestFooter("1GB perf", TestRunning, Results.Skip);
				g_iSkip++;
			}
			else
			{
				PrintTestFooter("1GB perf", TestRunning, Results.Fail);
				g_iFail++;
			}
			if (EnableLogHeaderFooter)
			{
				PrintLogFooter();
			}
			TestRunning = 0;
			return flag;
		}

		private void PerfSpew(long txSize, double TxTotalSeconds, long rxSize, double RxTotalSeconds)
		{
			double num = (double)txSize / TxTotalSeconds / 1000.0;
			double num2 = (double)rxSize / RxTotalSeconds / 1000.0;
			Log.Info("===============================================================================");
			Log.Info("TX: {0} bytes in {1} Seconds ({2} KBps)", txSize, TxTotalSeconds, num);
			Log.Info("RX: {0} bytes in {1} Seconds ({2} KBps)", rxSize, RxTotalSeconds, num2);
			Log.Info("===============================================================================");
		}

		private void FillBuffer(byte[] Buffer, int seed)
		{
			for (int i = 0; i < Buffer.Length; i++)
			{
				Buffer[i] = Convert.ToByte(seed % 127 + 49);
			}
		}

		private void ExceptionSpew(Exception e)
		{
			Log.Info("===============================================================================");
			Log.Info("==============================Exception!=======================================");
			Log.Info("{0}", e);
			Log.Info("===============================================================================");
		}
	}
}
