using System.Collections.Generic;
using System.IO;
using Microsoft.WindowsPhone.ImageUpdate.PkgCommon;
using Microsoft.WindowsPhone.ImageUpdate.PkgGenCommon.BaseComponents;
using Microsoft.WindowsPhone.ImageUpdate.PkgGenCommon.BaseComponents.Internal;
using Microsoft.WindowsPhone.ImageUpdate.Tools.Common;

namespace Microsoft.WindowsPhone.ImageUpdate.PkgGenCommon.Interfaces
{
	public interface IPackageGenerator
	{
		ISecurityPolicyCompiler PolicyCompiler { get; }

		XmlValidator XmlValidator { get; }

		BuildPass BuildPass { get; }

		CpuId CPU { get; }

		IMacroResolver MacroResolver { get; }

		string TempDirectory { get; }

		string ToolPaths { get; }

		IEnumerable<SatelliteId> GetSatelliteValues(SatelliteType type);

		void AddRegMultiSzSegment(string keyName, string valueName, params string[] valueSegments);

		void AddRegValue(string keyName, string valueName, RegValueType valueType, string value, SatelliteId satelliteId);

		void AddRegValue(string keyName, string valueName, RegValueType valueType, string value);

		void AddRegExpandValue(string keyName, string valueName, string value);

		void AddRegKey(string keyName, SatelliteId satelliteId);

		void AddRegKey(string keyName);

		void AddFile(string sourcePath, string devicePath, FileAttributes attributes, SatelliteId satelliteId, string embedSignCategory = "None");

		void AddFile(string sourcePath, string devicePath, FileAttributes attributes);

		void AddCertificate(string sourcePath);

		void AddBinaryPartition(string sourcePath);

		void AddBCDStore(string sourcePath);

		RegGroup ImportRegistry(string sourcePath);

		void Build(string projPath, string outputDir, bool compress);
	}
}
