using System;
using System.Diagnostics;
using System.Threading;

namespace Microsoft.Phone.TestInfra.Deployment
{
	public class PerformanceCounters
	{
		private static readonly Lazy<PerformanceCounters> LazyInstance = new Lazy<PerformanceCounters>(() => new PerformanceCounters());

		private int numFilesFound;

		private int cacheMisses;

		private int cacheHits;

		private int filesCopiedToCache;

		private int filesCopiedFromCache;

		private int filesCopiedFromSource;

		public static PerformanceCounters Instance => LazyInstance.Value;

		public Stopwatch TimeCopyingToCache { get; private set; }

		public Stopwatch TimeCopyingToDest { get; private set; }

		public Stopwatch TimeWaitingOnWritelock { get; private set; }

		public Stopwatch TimeWaitingOnReadlock { get; private set; }

		public Stopwatch TimeWaitingOnNetThrottle { get; private set; }

		public Stopwatch TimeWaitingOnLocalThrottle { get; private set; }

		public Stopwatch TimeSpentPurging { get; private set; }

		public int NumFilesFound => numFilesFound;

		public int CacheMisses => cacheMisses;

		public int CacheHits => cacheHits;

		public int FilesCopiedToCache => filesCopiedToCache;

		public int FilesCopiedFromCache => filesCopiedFromCache;

		public int FilesCopiedFromSource => filesCopiedFromSource;

		private PerformanceCounters()
		{
			TimeCopyingToCache = new Stopwatch();
			TimeCopyingToDest = new Stopwatch();
			TimeSpentPurging = new Stopwatch();
			TimeWaitingOnWritelock = new Stopwatch();
			TimeWaitingOnReadlock = new Stopwatch();
			TimeWaitingOnNetThrottle = new Stopwatch();
			TimeWaitingOnLocalThrottle = new Stopwatch();
		}

		public void Reset()
		{
			TimeCopyingToCache.Reset();
			TimeCopyingToDest.Reset();
			TimeSpentPurging.Reset();
			TimeWaitingOnWritelock.Reset();
			TimeWaitingOnReadlock.Reset();
			TimeWaitingOnNetThrottle.Reset();
			TimeWaitingOnLocalThrottle.Reset();
			numFilesFound = 0;
			cacheMisses = 0;
			cacheHits = 0;
			filesCopiedToCache = 0;
			filesCopiedFromCache = 0;
			filesCopiedFromSource = 0;
		}

		public void AddNumFilesFound(int value)
		{
			Interlocked.Add(ref numFilesFound, value);
		}

		public void AddCacheMisses(int value)
		{
			Interlocked.Add(ref cacheMisses, value);
		}

		public void AddCacheHits(int value)
		{
			Interlocked.Add(ref cacheHits, value);
		}

		public void AddFilesCopiedToCache(int value)
		{
			Interlocked.Add(ref filesCopiedToCache, value);
		}

		public void AddFilesCopiedFromCache(int value)
		{
			Interlocked.Add(ref filesCopiedFromCache, value);
		}

		public void AddFilesCopiedFromSource(int value)
		{
			Interlocked.Add(ref filesCopiedFromSource, value);
		}
	}
}
