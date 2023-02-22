using System.Collections.Generic;
using Microsoft.WindowsPhone.ImageUpdate.Tools.Common;

namespace Microsoft.WindowsPhone.ImageUpdate.PkgGenCommon.BaseComponents.Internal
{
	public static class CertStoreBuilder
	{
		public static bool Build(IEnumerable<string> certs, string output)
		{
			List<byte> list = new List<byte>();
			foreach (string cert in certs)
			{
				if (!LongPathFile.Exists(cert))
				{
					throw new PkgGenException("Certificate file '{0}' doens't exist", cert);
				}
				list.AddRange(LongPathFile.ReadAllBytes(cert));
			}
			if (list.Count > 0)
			{
				LongPathFile.WriteAllBytes(output, list.ToArray());
				return true;
			}
			return false;
		}
	}
}
