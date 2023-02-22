using System;
using System.Collections;
using System.Runtime.CompilerServices;
using System.Runtime.ConstrainedExecution;
using System.Threading;

namespace _003CCrtImplementationDetails_003E
{
	internal class ModuleUninitializer : Stack
	{
		private static object @lock = new object();

		internal static ModuleUninitializer _ModuleUninitializer = new ModuleUninitializer();

		internal void AddHandler(EventHandler handler)
		{
			bool flag = false;
			RuntimeHelpers.PrepareConstrainedRegions();
			try
			{
				RuntimeHelpers.PrepareConstrainedRegions();
				try
				{
				}
				finally
				{
					Monitor.Enter(@lock);
					flag = true;
				}
				RuntimeHelpers.PrepareDelegate(handler);
				Push(handler);
			}
			finally
			{
				if (flag)
				{
					Monitor.Exit(@lock);
				}
			}
		}

		private ModuleUninitializer()
		{
			EventHandler value = SingletonDomainUnload;
			AppDomain.CurrentDomain.DomainUnload += value;
			AppDomain.CurrentDomain.ProcessExit += value;
		}

		[PrePrepareMethod]
		private void SingletonDomainUnload(object source, EventArgs arguments)
		{
			bool flag = false;
			RuntimeHelpers.PrepareConstrainedRegions();
			try
			{
				RuntimeHelpers.PrepareConstrainedRegions();
				try
				{
				}
				finally
				{
					Monitor.Enter(@lock);
					flag = true;
				}
				IEnumerator enumerator = GetEnumerator();
				try
				{
					while (enumerator.MoveNext())
					{
						((EventHandler)enumerator.Current)(source, arguments);
					}
				}
				finally
				{
					IEnumerator enumerator2 = enumerator;
					(enumerator as IDisposable)?.Dispose();
				}
			}
			finally
			{
				if (flag)
				{
					Monitor.Exit(@lock);
				}
			}
		}
	}
}
