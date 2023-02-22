using System.Globalization;
using System.IO;

namespace Microsoft.WindowsPhone.ImageUpdate.InboxAppLib.Interfaces
{
	public sealed class InboxAppParameters
	{
		private string _packageBasePath = string.Empty;

		private string _licenseBasePath = string.Empty;

		private string _provXMLBasePath = string.Empty;

		private bool _infuseIntoDataPartition;

		private UpdateType _updateValue;

		private ProvXMLCategory _category;

		private string _workingBaseDir = string.Empty;

		public string PackageBasePath => _packageBasePath;

		public string LicenseBasePath => _licenseBasePath;

		public string ProvXMLBasePath => _provXMLBasePath;

		public bool InfuseIntoDataPartition => _infuseIntoDataPartition;

		public UpdateType UpdateValue => _updateValue;

		public ProvXMLCategory Category => _category;

		public string WorkingBaseDir => _workingBaseDir;

		public bool SkipSignatureValidation { get; set; }

		public InboxAppParameters(string packageBasePath, string licenseBasePath, string provXMLBasePath, bool infuseIntoDataPartition, UpdateType updateValue, ProvXMLCategory category, string workingBaseDir)
		{
			_packageBasePath = InboxAppUtils.ValidateFileOrDir(packageBasePath, false);
			_licenseBasePath = licenseBasePath;
			_provXMLBasePath = provXMLBasePath;
			_infuseIntoDataPartition = infuseIntoDataPartition;
			_updateValue = updateValue;
			_category = category;
			_workingBaseDir = workingBaseDir;
		}

		public InboxAppParameters(string packageBasePath, string licenseBasePath, string provXMLBasePath, bool infuseIntoDataPartition, UpdateType updateValue, ProvXMLCategory category)
			: this(packageBasePath, licenseBasePath, provXMLBasePath, infuseIntoDataPartition, updateValue, category, Path.Combine(Path.GetDirectoryName(Path.GetFullPath(packageBasePath)), Path.GetRandomFileName()))
		{
		}

		public override string ToString()
		{
			return string.Format(CultureInfo.InvariantCulture, "InboxApp Parameters: (PackageBasePath)=\"{0}\" (LicenseBasePath)=\"{1}\" (ProvXMLBasePath)=\"{2}\" InfuseIntoDataPartition=\"{3}\" UpdateType=\"{4}\" Category=\"{5}\"", _packageBasePath, _licenseBasePath, _provXMLBasePath, _infuseIntoDataPartition, _updateValue, _category);
		}
	}
}
