using System;
using Microsoft.CompPlat.PkgBldr.Base.Tools;

namespace Microsoft.CompPlat.PkgBldr.Base
{
	public class Bld
	{
		private CpuType _arch;

		private string _hostArch;

		private string _guestArch;

		private string _version;

		private string _product;

		private bool _isPhoneBuild;

		private string _jsonDepot;

		public string Lang;

		public string Resolution;

		public bool IsGuest;

		public MacroResolver BuildMacros;

		public CSI CSI = new CSI();

		public PKG PKG = new PKG();

		public WM WM = new WM();

		public bool IsPhoneBuild => _isPhoneBuild;

		public CpuType Arch
		{
			get
			{
				return _arch;
			}
			set
			{
				_arch = value;
				switch (value)
				{
				case CpuType.x86:
				case CpuType.amd64:
					HostArch = "amd64";
					GuestArch = "x86";
					break;
				case CpuType.arm:
				case CpuType.arm64:
					HostArch = "arm64";
					GuestArch = "arm";
					break;
				}
			}
		}

		public string ArchAsString
		{
			get
			{
				switch (_arch)
				{
				case CpuType.x86:
					return "x86";
				case CpuType.amd64:
					return "amd64";
				case CpuType.arm:
					return "arm";
				case CpuType.arm64:
					return "arm64";
				default:
					return null;
				}
			}
		}

		public string GuestArch
		{
			get
			{
				return _guestArch;
			}
			set
			{
				if (value != null)
				{
					_guestArch = value.ToLowerInvariant();
					string guestArch = _guestArch;
					if (!(guestArch == "x86") && !(guestArch == "arm"))
					{
						throw new PkgGenException("Invalid arch = {0}", value.ToLowerInvariant());
					}
					return;
				}
				throw new PkgGenException("GuestArch cannot be null");
			}
		}

		public string HostArch
		{
			get
			{
				return _hostArch;
			}
			set
			{
				if (value != null)
				{
					_hostArch = value.ToLowerInvariant();
					string hostArch = _hostArch;
					if (!(hostArch == "amd64") && !(hostArch == "arm64"))
					{
						throw new PkgGenException("Invalid arch = {0}", value.ToLowerInvariant());
					}
					return;
				}
				throw new PkgGenException("HostArch cannot be null");
			}
		}

		public string Product
		{
			get
			{
				return _product;
			}
			set
			{
				if (value != null)
				{
					_product = value.ToLowerInvariant();
				}
				else
				{
					_product = null;
				}
			}
		}

		public string Version
		{
			get
			{
				return _version;
			}
			set
			{
				if (value != null)
				{
					_version = value.ToLowerInvariant();
				}
				else
				{
					_version = null;
				}
			}
		}

		public string JsonDepot
		{
			get
			{
				return _jsonDepot;
			}
			set
			{
				if (value != null)
				{
					_jsonDepot = value.ToLowerInvariant();
				}
				else
				{
					_jsonDepot = null;
				}
			}
		}

		public Bld()
		{
			if (!Environment.ExpandEnvironmentVariables("%_WINPHONEROOT%").StartsWith("%", StringComparison.OrdinalIgnoreCase))
			{
				_isPhoneBuild = true;
			}
		}
	}
}
