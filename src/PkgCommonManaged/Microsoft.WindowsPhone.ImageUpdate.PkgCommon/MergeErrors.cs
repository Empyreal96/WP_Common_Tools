using System.Collections.Generic;
using System.Threading;
using Microsoft.WindowsPhone.ImageUpdate.Tools.Common;

namespace Microsoft.WindowsPhone.ImageUpdate.PkgCommon
{
	internal class MergeErrors
	{
		private static Dictionary<int, MergeErrors> _instances = new Dictionary<int, MergeErrors>();

		private List<string> _errors = new List<string>();

		public bool HasError => _errors.Count != 0;

		public static MergeErrors Instance
		{
			get
			{
				lock (_instances)
				{
					MergeErrors value = null;
					if (!_instances.TryGetValue(Thread.CurrentThread.ManagedThreadId, out value))
					{
						value = new MergeErrors();
						_instances.Add(Thread.CurrentThread.ManagedThreadId, value);
					}
					return value;
				}
			}
		}

		public void Add(string msg)
		{
			_errors.Add(msg);
		}

		public void Add(string format, params object[] args)
		{
			_errors.Add(string.Format(format, args));
		}

		public void CheckResult()
		{
			if (!HasError)
			{
				return;
			}
			foreach (string error in _errors)
			{
				LogUtil.Error(error);
			}
			throw new PackageException("Error occured during merging");
		}

		public static void Clear()
		{
			lock (_instances)
			{
				_instances.Remove(Thread.CurrentThread.ManagedThreadId);
			}
		}
	}
}
