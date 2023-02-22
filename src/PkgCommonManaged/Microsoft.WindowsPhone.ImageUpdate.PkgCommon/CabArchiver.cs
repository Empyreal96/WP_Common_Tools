using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.WindowsPhone.ImageUpdate.Tools;
using Microsoft.WindowsPhone.ImageUpdate.Tools.Common;

namespace Microsoft.WindowsPhone.ImageUpdate.PkgCommon
{
	public class CabArchiver
	{
		private List<KeyValuePair<string, string>> filesInCab = new List<KeyValuePair<string, string>>();

		public void AddFile(string destination, string source)
		{
			AddFileAtIndex(filesInCab.Count, destination, source);
		}

		public void AddFileToFront(string destination, string source)
		{
			AddFileAtIndex(0, destination, source);
		}

		private void AddFileAtIndex(int index, string destination, string source)
		{
			if (string.IsNullOrEmpty(source) || string.IsNullOrEmpty(destination))
			{
				throw new PackageException("Both source and destination must be set");
			}
			filesInCab.Insert(index, new KeyValuePair<string, string>(destination, source));
		}

		public void Save(string cabPath, CompressionType compressionType)
		{
			string tempDirectory = FileUtils.GetTempDirectory();
			try
			{
				CabApiWrapper.CreateCabSelected(cabPath, filesInCab.Select((KeyValuePair<string, string> x) => LongPath.GetFullPathUNC(x.Value)).ToArray(), filesInCab.Select((KeyValuePair<string, string> x) => x.Key.TrimStart('\\')).ToArray(), tempDirectory, compressionType);
			}
			catch (Exception innerException)
			{
				throw new PackageException(innerException, "Failed to save cab to {0}", cabPath);
			}
			finally
			{
				FileUtils.DeleteTree(tempDirectory);
			}
		}
	}
}
