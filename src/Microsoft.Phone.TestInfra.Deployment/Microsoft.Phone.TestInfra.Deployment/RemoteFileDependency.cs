using System;
using System.Xml.Serialization;

namespace Microsoft.Phone.TestInfra.Deployment
{
	public class RemoteFileDependency : Dependency
	{
		[XmlAttribute]
		public string SourcePath { get; set; }

		[XmlAttribute]
		public string Source { get; set; }

		[XmlAttribute]
		public string DestinationPath { get; set; }

		[XmlAttribute]
		public string Destination { get; set; }

		[XmlAttribute]
		public string Tags { get; set; }

		public override bool Equals(object obj)
		{
			if (obj == null)
			{
				return false;
			}
			if (obj.GetType() != GetType())
			{
				return false;
			}
			return Equals(obj as RemoteFileDependency);
		}

		public bool Equals(RemoteFileDependency other)
		{
			bool flag = false;
			if (!string.IsNullOrEmpty(Tags))
			{
				flag = Tags.Equals(other.Tags, StringComparison.OrdinalIgnoreCase);
			}
			else if (string.IsNullOrEmpty(other.Tags))
			{
				flag = true;
			}
			return SourcePath.Equals(other.SourcePath, StringComparison.OrdinalIgnoreCase) && Source.Equals(other.Source, StringComparison.OrdinalIgnoreCase) && DestinationPath.Equals(other.DestinationPath, StringComparison.OrdinalIgnoreCase) && Destination.Equals(other.Destination, StringComparison.OrdinalIgnoreCase) && flag;
		}

		public override int GetHashCode()
		{
			int hashCode = string.Empty.GetHashCode();
			if (!string.IsNullOrEmpty(Tags))
			{
				hashCode = Tags.GetHashCode();
			}
			return SourcePath.GetHashCode() ^ Source.GetHashCode() ^ DestinationPath.GetHashCode() ^ Destination.GetHashCode() ^ hashCode;
		}
	}
}
