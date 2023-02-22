using System.Xml.Linq;

namespace Microsoft.CompPlat.PkgBldr.Base
{
	public class PKG
	{
		private string _partition;

		private string _ownerType;

		private string _component;

		private string _subComponent;

		private string _releaseType;

		public XElement Root;

		public string OwnerType
		{
			get
			{
				return _ownerType;
			}
			set
			{
				_ownerType = value;
			}
		}

		public string Partition
		{
			get
			{
				return _partition;
			}
			set
			{
				_partition = value;
			}
		}

		public string Component
		{
			get
			{
				return _component;
			}
			set
			{
				_component = value;
			}
		}

		public string SubComponent
		{
			get
			{
				return _subComponent;
			}
			set
			{
				_subComponent = value;
			}
		}

		public string ReleaseType
		{
			get
			{
				return _releaseType;
			}
			set
			{
				_releaseType = value;
			}
		}
	}
}
