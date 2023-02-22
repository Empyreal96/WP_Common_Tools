using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace Microsoft.CompPlat.PkgBldr.Base.Tools
{
	public class PkgBldrCmd
	{
		[Description("Full path to input file : .wm.xml, .pkg.xml, .man")]
		public string project { get; set; }

		[Description("Output directory or file.")]
		public string output { get; set; }

		[Description("Version string in the form of <major>.<minor>.<qfe>.<build>")]
		public string version { get; set; }

		[Description("CPU type. Values: (x86|arm|arm64|amd64)")]
		public CpuType cpu { get; set; }

		[Description("Supported language identifier list, separated by ';'")]
		public string languages { get; set; }

		[Description("Supported resolution identifier list, separated by ';'")]
		public string resolutions { get; set; }

		[Description("Additional variables used in the project file,syntax:<name>=<value>;<name>=<value>;....")]
		public string variables { get; set; }

		[Description("Enable debug output.")]
		public bool diagnostic { get; set; }

		[Description("Path to write the windows manifest schema.")]
		public string wmxsd { get; set; }

		[Description("The type of conversion operation to perform. Values: (wm2csi|csi2wm|pkg2csi|pkg2wm|csi2pkg|pkg2cab)")]
		public ConversionType convert { get; set; }

		[Description("Supported product identifier")]
		public string product { get; set; }

		[Description("Output directory for guest packages")]
		public string wowdir { get; set; }

		[Description("HostOnly, GuestOnly, or Both")]
		public WowBuildType wowbuild { get; set; }

		[Description("Use NtverpUtils to get the Windows product version")]
		public bool usentverp { get; set; }

		[Description("Process the driver's INF like the build does")]
		public bool processInf { get; set; }

		[Description("Generate CAB(s) when using wm.xml")]
		public bool makecab { get; set; }

		[Description("Only log warnings and errors")]
		public bool quiet { get; set; }

		[Description("Building TOC files instead of the actual package")]
		public bool toc { get; set; }

		[Description("Compressing the generated package.")]
		public bool compress { get; set; }

		[Description("File with globally defined variables.")]
		public string config { get; set; }

		[Description("Build type string Values: (fre|chk)")]
		public BuildType build { get; set; }

		[Description("Path to write the auto-generated windows phone manifest schema")]
		public string xsd { get; set; }

		[Description("The package is for onecore products, this sets nohives = true for BSP’s")]
		public bool onecore { get; set; }

		[Description("Indicates whether or not this package has no hive dependency")]
		public bool nohives { get; set; }

		[Description("Location of SdxRoot.")]
		public string sdxRoot { get; set; }

		[Description("Location of RazzleToolPath.")]
		public string razzleToolPath { get; set; }

		[Description("Location of RazzleDataPath")]
		public string razzleDataPath { get; set; }

		[Description("Location of _NTTREE")]
		public string nttree { get; set; }

		[Description("Location of build.nttree")]
		public string buildNttree { get; set; }

		[Description("Location of capabilityList.cfg")]
		public string capabilityListCfg { get; set; }

		[Description("_BuildBranch")]
		public string buildBranch { get; set; }

		[Description("Location of manifest_sddl.txt")]
		public string manifestSddlTxt { get; set; }

		[Description("Location of NTDEV_LSSettings.lsconfig")]
		public string ntdevLssettingsLsconfig { get; set; }

		[Description("Location of bldnump.h")]
		public string bldNumpH { get; set; }

		[Description("Location of cmiv2.dll")]
		public string cmiV2Dll { get; set; }

		[Description("Path to PkgBldr.CSI.Xsd.")]
		public string csiXsdPath { get; set; }

		[Description("Path to PkgBldr.PKG.Xsd.")]
		public string pkgXsdPath { get; set; }

		[Description("Path to PkgBldr.Shared.Xsd.")]
		public string sharedXsdPath { get; set; }

		[Description("Path to PkgBldr.WM.Xsd.")]
		public string wmXsdPath { get; set; }

		[Description("Directories containing tools needed by spkggen.exe")]
		public string spkgGenToolDirs { get; set; }

		[Description("Generate JSON's in the specifed depot when conerting from pkg.xml to wm.xml")]
		public string json { get; set; }

		[Description("Dictionary of tool paths needed by pkggen.exe")]
		public Dictionary<string, string> toolPaths { get; set; }

		public bool isRazzleEnv
		{
			get
			{
				bool result = true;
				if (razzleDataPath.Equals("%RazzleDataPath%", StringComparison.OrdinalIgnoreCase))
				{
					result = !Environment.ExpandEnvironmentVariables(razzleDataPath).Equals(razzleDataPath);
				}
				return result;
			}
		}

		public PkgBldrCmd()
		{
			output = ".";
			version = "1.0.0.0";
			cpu = CpuType.arm;
			convert = ConversionType.pkg2cab;
			product = "windows";
			wowbuild = WowBuildType.Both;
			usentverp = false;
			processInf = false;
			makecab = false;
			quiet = false;
			toc = false;
			compress = false;
			diagnostic = false;
			build = BuildType.fre;
			nohives = false;
			sdxRoot = "%sdxroot%";
			razzleToolPath = "%RazzleToolPath%";
			razzleDataPath = "%RazzleDataPath%";
			nttree = "%_nttree%";
			buildNttree = "%build.nttree%";
			capabilityListCfg = "%RazzleToolPath%\\managed\\v4.0\\capabilitylist.cfg";
			buildBranch = "%_BuildBranch%";
			manifestSddlTxt = "%RazzleToolPath%\\manifest_sddl.txt";
			ntdevLssettingsLsconfig = "% RazzleToolPath %\\locstudio\\NTDEV_LSSettings.lsconfig";
			bldNumpH = "%PUBLIC_ROOT%\\sdk\\inc\\bldnump.h";
			cmiV2Dll = "%RazzleToolPath%\\x86\\cmiv2.dll";
			csiXsdPath = "%RazzleToolPath%\\managed\\v4.0\\PkgBldr.CSI.Xsd";
			pkgXsdPath = "%RazzleToolPath%\\managed\\v4.0\\PkgBldr.PKG.Xsd";
			sharedXsdPath = "%RazzleToolPath%\\managed\\v4.0\\PkgBldr.Shared.Xsd";
			wmXsdPath = "%RazzleToolPath%\\managed\\v4.0\\PkgBldr.WM.Xsd";
			spkgGenToolDirs = "%RazzleToolPath%\\x86";
			toolPaths = new Dictionary<string, string>
			{
				{ "pkgresolvepartial", "%RazzleToolPath%\\x86\\pkgresolvepartial.exe" },
				{ "genddf", "%RazzleToolPath%\\x86\\genddf.exe" },
				{ "makecab", "%RazzleToolPath%\\x86\\MakeCab.exe" },
				{ "updcat", "%RazzleToolPath%\\x86\\updcat.exe" },
				{ "sign", "%RazzleToolPath%\\sign.cmd" },
				{ "perl", "%RazzleToolPath%\\perl\\bin\\perl.exe" },
				{ "spkggen", "%RazzleToolPath%\\managed\\v4.0\\spkggen.exe" },
				{ "cmd", "cmd.exe" },
				{ "unitext", "unitext.exe" },
				{ "prodfilt", "prodfilt.exe" },
				{ "stampinf", "stampinf.exe" },
				{ "infutil", "infutil.exe" },
				{ "binplace", "%RazzleToolPath%\\x86\\binplace.exe" },
				{ "urtrun", "%RazzleToolPath%\\urtrun.cmd" },
				{ "reformatmanifest", "%RazzleToolPath%\\reformatmanifest.cmd" },
				{ "cmimanifest", "%RazzleToolPath\\cmi-manifest.pl" },
				{ "filemanager", "%RazzleToolPath%\\x86\\filemanager.exe" },
				{ "keyform", "%RazzleToolPath%\\x86\\keyform.exe" },
				{ "cl", "%RazzleToolPath%\\DEV12\\x32\\x86\\cl.exe" }
			};
		}
	}
}
