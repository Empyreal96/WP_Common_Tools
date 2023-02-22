using System;
using System.Globalization;
using System.IO;
using System.Xml.Linq;
using Microsoft.WindowsPhone.ImageUpdate.InboxAppLib.Interfaces;
using Microsoft.WindowsPhone.ImageUpdate.Tools.Common;

namespace Microsoft.WindowsPhone.ImageUpdate.InboxAppLib
{
	public abstract class ProvXMLBase : IInboxProvXML
	{
		protected InboxAppParameters _parameters;

		protected string _provXMLDestinationPath = string.Empty;

		protected string _updateProvXMLDestinationPath = string.Empty;

		protected string _licenseDestinationPath = string.Empty;

		protected string _packageHash = string.Empty;

		protected XDocument _document;

		public string ProvXMLDestinationPath => _provXMLDestinationPath;

		public string UpdateProvXMLDestinationPath => _updateProvXMLDestinationPath;

		public string LicenseDestinationPath => _licenseDestinationPath;

		public ProvXMLCategory Category => _parameters.Category;

		public string DependencyHash
		{
			get
			{
				return _packageHash;
			}
			set
			{
				_packageHash = value;
			}
		}

		protected ProvXMLBase(InboxAppParameters parameters)
		{
			if (parameters == null)
			{
				throw new ArgumentNullException("parameters", "INTERNAL ERROR: The parameters passed into the ProvXMLBase constructor is null!");
			}
			_parameters = parameters;
			InboxAppUtils.ValidateFileOrDir(_parameters.ProvXMLBasePath, false);
			ValidateFileNameDetails();
		}

		public abstract void ReadProvXML();

		public void Save(string outputBasePath)
		{
			_document.Save(outputBasePath);
		}

		public abstract void Update(string installDestinationPath, string licenseFileDestinationPath);

		protected void ValidateFileNameDetails()
		{
			if (!_parameters.ProvXMLBasePath.Contains(".provxml"))
			{
				string message = string.Format(CultureInfo.InvariantCulture, "The provxml filename \"{0}\" does not match the expected format '{1}*{2}'.", new object[3] { _parameters.ProvXMLBasePath, "MPAP_", ".provxml" });
				LogUtil.Error(message);
				throw new NotSupportedException(message);
			}
		}

		protected string GetMxipFileDestinationPath(string provxmlPBath, ProvXMLCategory category, UpdateType updateValue, IInboxAppManifest manifest)
		{
			string result = string.Empty;
			string fileName = Path.GetFileName(provxmlPBath);
			switch (category)
			{
			case ProvXMLCategory.Microsoft:
			case ProvXMLCategory.Test:
				result = ((manifest == null || !(manifest is AppManifestAppxBase) || !((AppManifestAppxBase)manifest).IsFramework) ? string.Format(CultureInfo.InvariantCulture, "$(runtime.updateProvxmlMS)\\mxipupdate{0}", new object[1] { fileName.CleanFileNameForUpdate(updateValue == UpdateType.UpdateEarly) }) : string.Format(CultureInfo.InvariantCulture, "$(runtime.updateProvxmlMS)\\appframework{0}", new object[1] { fileName.CleanFileNameForUpdate(false) }));
				break;
			case ProvXMLCategory.OEM:
				result = string.Format(CultureInfo.InvariantCulture, "$(runtime.updateProvxmlOEM)\\mxipupdate{0}", new object[1] { fileName.CleanFileNameForUpdate(updateValue == UpdateType.UpdateEarly) });
				break;
			}
			return result;
		}

		protected abstract string DetermineProvXMLDestinationPath();

		protected abstract string DetermineLicenseDestinationPath();
	}
}
