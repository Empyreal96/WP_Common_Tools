using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Text;
using System.Threading;

namespace Microsoft.Windows.ImageTools
{
	public class ProgressReporter
	{
		private int width;

		private Stopwatch stopwatch;

		private int summaryCount;

		private Queue<Tuple<double, long>> progressPoints;

		private const double OneMegabyte = 1048576.0;

		public ProgressReporter()
		{
			width = Console.WindowWidth;
			stopwatch = Stopwatch.StartNew();
			summaryCount = 0;
			progressPoints = new Queue<Tuple<double, long>>();
		}

		public string CreateProgressDisplay(long position, long totalLength)
		{
			StringBuilder stringBuilder = new StringBuilder(2 * width);
			if (position == totalLength)
			{
				stopwatch.Stop();
				if (Interlocked.Add(ref summaryCount, 1) == 1)
				{
					double num = (double)totalLength / 1048576.0;
					stringBuilder.AppendFormat(Resources.TRANSFER_STATISTICS, num, stopwatch.Elapsed.TotalSeconds, num / stopwatch.Elapsed.TotalSeconds);
				}
				else
				{
					stringBuilder.Clear();
				}
			}
			else
			{
				double num2 = (double)position / (double)totalLength;
				if (num2 > 1.0)
				{
					num2 = 1.0;
				}
				int num3 = (int)Math.Floor(50.0 * num2);
				for (int i = 0; i < width; i++)
				{
					stringBuilder.Append('\b');
				}
				stringBuilder.Append('[');
				for (int j = 0; j < num3; j++)
				{
					stringBuilder.Append('=');
				}
				if (num3 < 50)
				{
					stringBuilder.Append('>');
					num3++;
				}
				for (int k = num3; k < 50; k++)
				{
					stringBuilder.Append(' ');
				}
				stringBuilder.Append("]  ");
				stringBuilder.AppendFormat("{0:0.00%}", num2);
				stringBuilder.AppendFormat(" {0}", GetSpeedString(position));
				for (int l = stringBuilder.Length; l < 2 * width - 1; l++)
				{
					stringBuilder.Append(' ');
				}
			}
			return stringBuilder.ToString();
		}

		private string GetSpeedString(long position)
		{
			string result = string.Empty;
			Tuple<double, long> item = new Tuple<double, long>(stopwatch.Elapsed.TotalSeconds, position);
			progressPoints.Enqueue(item);
			if (progressPoints.Count >= 8)
			{
				result = GetSpeedFromPoints(progressPoints.ToArray());
				progressPoints.Dequeue();
			}
			return result;
		}

		private string GetSpeedFromPoints(Tuple<double, long>[] points)
		{
			double num = 0.0;
			for (int i = 1; i < points.Length; i++)
			{
				double num2 = (double)(points[i].Item2 - points[i - 1].Item2) / 1048576.0;
				double num3 = points[i].Item1 - points[i - 1].Item1;
				num += num2 / num3 / (double)(points.Length - 1);
			}
			return string.Format(CultureInfo.CurrentCulture, Resources.FORMAT_SPEED, new object[1] { num });
		}
	}
}
