using System.Runtime.InteropServices;

namespace Microsoft.Windows.Flashing.Platform
{
	public abstract class GenericProgress
	{
		public abstract void RegisterProgress([In] uint Progress);

		public GenericProgress()
		{
		}
	}
}
