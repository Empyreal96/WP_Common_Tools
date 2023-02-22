using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml.Serialization;
using Microsoft.WindowsPhone.ImageUpdate.PkgCommon;
using Microsoft.WindowsPhone.ImageUpdate.Tools.Common;

namespace Microsoft.WindowsPhone.FeatureAPI
{
	[XmlType(Namespace = "http://schemas.microsoft.com/embedded/2004/10/ImageUpdate")]
	[XmlRoot(ElementName = "ImagingEditions", Namespace = "http://schemas.microsoft.com/embedded/2004/10/ImageUpdate", IsNullable = false)]
	public class ImagingEditions
	{
		private static readonly object _lockEditionsXML = new object();

		private static ImagingEditions _editionsXML = null;

		[XmlArrayItem(ElementName = "Edition", Type = typeof(Edition), IsNullable = false)]
		[XmlArray("Editions")]
		public List<Edition> EditionList;

		public static List<Edition> Editions
		{
			get
			{
				if (_editionsXML == null)
				{
					lock (_lockEditionsXML)
					{
						if (_editionsXML == null)
						{
							_editionsXML = ValidateEditions(new IULogger());
						}
					}
				}
				return _editionsXML.EditionList;
			}
		}

		public static Edition GetProductEdition(string product)
		{
			Edition result = null;
			foreach (Edition edition in Editions)
			{
				if (product.Equals(edition.Name, StringComparison.OrdinalIgnoreCase) || product.Equals(edition.AlternateName, StringComparison.OrdinalIgnoreCase))
				{
					return edition;
				}
			}
			return result;
		}

		public static List<CpuId> GetWowGuestCpuTypes(List<string> fmFiles, CpuId cpuType)
		{
			List<CpuId> result = new List<CpuId>();
			List<string> fmfilenamesOnly = fmFiles.Select((string fm) => Path.GetFileName(fm)).ToList();
			Edition edition = Editions.FirstOrDefault((Edition ed) => ed.CoreFeatureManifestPackages.Any((EditionPackage fm) => fmfilenamesOnly.Contains(Path.GetFileName(fm.FMDevicePath), StringComparer.OrdinalIgnoreCase)));
			if (edition != null)
			{
				result = edition.GetSupportedWowCpuTypes(cpuType);
			}
			return result;
		}

		public static ImagingEditions ValidateEditions(IULogger logger)
		{
			XsdValidator xsdValidator = new XsdValidator();
			ImagingEditions imagingEditions = new ImagingEditions();
			try
			{
				Assembly executingAssembly = Assembly.GetExecutingAssembly();
				string[] manifestResourceNames = executingAssembly.GetManifestResourceNames();
				string text = string.Empty;
				string text2 = string.Empty;
				string text3 = "ImagingEditions.xml";
				string text4 = "ImagingEditions.xsd";
				string[] array = manifestResourceNames;
				foreach (string text5 in array)
				{
					if (text5.Contains(text3))
					{
						text = text5;
					}
					else if (text5.Contains(text4))
					{
						text2 = text5;
					}
				}
				if (string.IsNullOrEmpty(text))
				{
					throw new XsdValidatorException("FeatureAPI!ValidateEditions: XML resource was not found: " + text3);
				}
				if (string.IsNullOrEmpty(text2))
				{
					throw new XsdValidatorException("FeatureAPI!ValidateEditions: XSD resource was not found: " + text4);
				}
				using (Stream stream = executingAssembly.GetManifestResourceStream(text))
				{
					if (stream == null)
					{
						throw new XsdValidatorException("FeatureAPI!ValidateEditions: Failed to load the editions file: " + text);
					}
					using (Stream stream2 = executingAssembly.GetManifestResourceStream(text2))
					{
						if (stream2 == null)
						{
							throw new XsdValidatorException("FeatureAPI!ValidateEditions: Failed to load the embeded schema file: " + text2);
						}
						xsdValidator.ValidateXsd(stream2, stream, text3, logger);
						stream.Seek(0L, SeekOrigin.Begin);
						XmlSerializer xmlSerializer = new XmlSerializer(typeof(ImagingEditions));
						try
						{
							return (ImagingEditions)xmlSerializer.Deserialize(stream);
						}
						catch (Exception innerException)
						{
							throw new FeatureAPIException("FeatureAPI!ValidateEditions: Unable to parse editions XML file.", innerException);
						}
					}
				}
			}
			catch (XsdValidatorException innerException2)
			{
				throw new FeatureAPIException("FeatureAPI!ValidateEditions: Unable to validate editions XSD.", innerException2);
			}
		}
	}
}
