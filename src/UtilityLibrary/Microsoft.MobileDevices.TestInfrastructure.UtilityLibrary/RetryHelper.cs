using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace Microsoft.MobileDevices.TestInfrastructure.UtilityLibrary
{
	public static class RetryHelper
	{
		public delegate void OnFailureCallback(Exception error, int numAttempts, int maxAttempts);

		public delegate bool IsRetryableExceptionCallback(Exception e);

		private const double MaxSleepOffsetAdjustmentPercent = 0.15;

		private static readonly Random RandomGenerator = new Random();

		public static void Retry(Action action, int retryCount, TimeSpan retryDelay)
		{
			Retry(action, retryCount, retryDelay, new Type[1] { typeof(Exception) });
		}

		public static void Retry(Action action, int retryCount, TimeSpan retryDelay, IEnumerable<Type> retryableExceptions)
		{
			if (action == null)
			{
				throw new ArgumentNullException("action");
			}
			Retry(delegate
			{
				action();
				return true;
			}, retryCount, retryDelay, retryableExceptions);
		}

		public static T Retry<T>(Func<T> func, int retryCount, TimeSpan retryDelay)
		{
			return Retry(func, retryCount, retryDelay, new Type[1] { typeof(Exception) });
		}

		public static T Retry<T>(Func<T> func, int retryCount, TimeSpan retryDelay, IEnumerable<Type> retryableExceptions, OnFailureCallback onFailure = null)
		{
			if (retryableExceptions == null || !retryableExceptions.Any())
			{
				throw new ArgumentNullException("retryableExceptions");
			}
			if (retryableExceptions.Any((Type re) => !typeof(Exception).IsAssignableFrom(re)))
			{
				throw new ArgumentException("Retryable exceptions list contains element(s) that are not exception types");
			}
			return Retry(func, retryCount, retryDelay, (Exception e) => DefaultIsRetryableException(retryableExceptions, e), onFailure);
		}

		public static void Retry(Action action, int retryCount, TimeSpan retryDelay, IEnumerable<Type> retryableExceptions, OnFailureCallback onFailure = null)
		{
			Retry(delegate
			{
				action();
				return true;
			}, retryCount, retryDelay, retryableExceptions, onFailure);
		}

		public static T Retry<T>(Func<T> func, int retryCount, TimeSpan retryDelay, IsRetryableExceptionCallback isRetryableException, OnFailureCallback onFailure = null)
		{
			if (func == null)
			{
				throw new ArgumentNullException("func");
			}
			if (retryCount < 0)
			{
				throw new ArgumentOutOfRangeException("retryCount", retryCount, "Retry count is negative");
			}
			if (retryDelay < TimeSpan.Zero)
			{
				throw new ArgumentOutOfRangeException("retryDelay", retryDelay, "Retry delay is negative");
			}
			if (isRetryableException == null)
			{
				throw new ArgumentNullException("isRetryableException");
			}
			T result = default(T);
			for (int i = 0; i <= retryCount; i++)
			{
				try
				{
					result = func();
					return result;
				}
				catch (Exception ex)
				{
					if (!isRetryableException(ex))
					{
						throw;
					}
					onFailure?.Invoke(ex, i + 1, retryCount + 1);
					if (i >= retryCount)
					{
						throw;
					}
					Thread.Sleep(MakeRandomSleepInterval(retryDelay));
				}
			}
			return result;
		}

		public static void Retry(Action action, int retryCount, TimeSpan retryDelay, IsRetryableExceptionCallback isRetryableException, OnFailureCallback onFailure = null)
		{
			Retry(delegate
			{
				action();
				return true;
			}, retryCount, retryDelay, isRetryableException, onFailure);
		}

		public static T KeepTrying<T>(Func<T> func, TimeSpan timeout, TimeSpan retryDelay, TimeSpan? initialDelay = null, IsRetryableExceptionCallback isRetryableException = null, OnFailureCallback onFailure = null)
		{
			if (func == null)
			{
				throw new ArgumentNullException("func");
			}
			if (isRetryableException == null)
			{
				isRetryableException = AllowAnyException;
			}
			if (initialDelay.HasValue)
			{
				Thread.Sleep(initialDelay.Value);
			}
			DateTime now = DateTime.Now;
			int num = 0;
			while (true)
			{
				bool flag = true;
				try
				{
					num++;
					return func();
				}
				catch (Exception ex)
				{
					if (!isRetryableException(ex))
					{
						throw;
					}
					onFailure?.Invoke(ex, num, -1);
					if (DateTime.Now - now >= timeout)
					{
						throw;
					}
				}
				Thread.Sleep(retryDelay);
			}
		}

		public static void KeepTrying(Action action, TimeSpan timeout, TimeSpan retryDelay, TimeSpan? initialDelay = null, IsRetryableExceptionCallback isRetryableException = null, OnFailureCallback onFailure = null)
		{
			KeepTrying(delegate
			{
				action();
				return true;
			}, timeout, retryDelay, initialDelay, isRetryableException, onFailure);
		}

		private static bool DefaultIsRetryableException(IEnumerable<Type> retryableExceptions, Exception e)
		{
			if (!retryableExceptions.Any((Type re) => re.IsInstanceOfType(e)))
			{
				AggregateException ex = e as AggregateException;
				if (ex == null || !ex.InnerExceptions.Any((Exception ie) => retryableExceptions.Any((Type re) => re.IsInstanceOfType(ie))))
				{
					return false;
				}
			}
			return true;
		}

		private static bool AllowAnyException(Exception e)
		{
			return true;
		}

		private static TimeSpan MakeRandomSleepInterval(TimeSpan baseInterval)
		{
			double totalMilliseconds = baseInterval.TotalMilliseconds;
			double num = 1.0 + (RandomGenerator.NextDouble() - 0.5) * 0.15 * 2.0;
			double value = Math.Max(totalMilliseconds * num, 0.0);
			return TimeSpan.FromMilliseconds(value);
		}
	}
}
