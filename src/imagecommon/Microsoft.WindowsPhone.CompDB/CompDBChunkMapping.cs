using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Xml.Serialization;
using Microsoft.WindowsPhone.ImageUpdate.Tools.Common;
using Microsoft.WindowsPhone.Imaging;

namespace Microsoft.WindowsPhone.CompDB
{
	[XmlType(Namespace = "http://schemas.microsoft.com/embedded/2004/10/ImageUpdate")]
	[XmlRoot(ElementName = "CompDBChunkMapping", Namespace = "http://schemas.microsoft.com/embedded/2004/10/ImageUpdate", IsNullable = false)]
	public class CompDBChunkMapping
	{
		public static string c_LangVariable = "$(Lang)";

		[XmlArrayItem(ElementName = "Mapping", Type = typeof(CompDBChunkMapItem), IsNullable = false)]
		[XmlArray]
		public List<CompDBChunkMapItem> ChunkMappings = new List<CompDBChunkMapItem>();

		[XmlIgnore]
		public List<string> Languages;

		private Dictionary<string, CompDBChunkMapItem> _lookupTable = new Dictionary<string, CompDBChunkMapItem>(StringComparer.OrdinalIgnoreCase);

		public CompDBChunkMapItem FindChunk(string path)
		{
			CompDBChunkMapItem result = null;
			foreach (string key in _lookupTable.Keys)
			{
				if (path.StartsWith(key, StringComparison.OrdinalIgnoreCase))
				{
					return _lookupTable[key];
				}
			}
			return result;
		}

		public static CompDBChunkMapping ValidateAndLoad(string xmlFile, List<string> languages, IULogger logger)
		{
			CompDBChunkMapping compDBChunkMapping = new CompDBChunkMapping();
			string text = string.Empty;
			string compDBChunkMappingSchema = BuildPaths.CompDBChunkMappingSchema;
			Assembly executingAssembly = Assembly.GetExecutingAssembly();
			string[] manifestResourceNames = executingAssembly.GetManifestResourceNames();
			foreach (string text2 in manifestResourceNames)
			{
				if (text2.Contains(compDBChunkMappingSchema))
				{
					text = text2;
					break;
				}
			}
			if (string.IsNullOrEmpty(text))
			{
				throw new ImageCommonException("ImageCommon!CompDBChunkMapping::ValidateAndLoad: XSD resource was not found: " + compDBChunkMappingSchema);
			}
			using (Stream xsdStream = executingAssembly.GetManifestResourceStream(text))
			{
				XsdValidator xsdValidator = new XsdValidator();
				try
				{
					xsdValidator.ValidateXsd(xsdStream, xmlFile, logger);
				}
				catch (XsdValidatorException innerException)
				{
					throw new ImageCommonException("ImageCommon!CompDBChunkMapping::ValidateAndLoad: Unable to validate CompDB Chunk Mapping XSD for file '" + xmlFile + "'.", innerException);
				}
			}
			logger.LogInfo("ImageCommon: Successfully validated the CompDB Chunk Mappingt XML: {0}", xmlFile);
			TextReader textReader = new StreamReader(LongPathFile.OpenRead(xmlFile));
			XmlSerializer xmlSerializer = new XmlSerializer(typeof(CompDBChunkMapping));
			try
			{
				compDBChunkMapping = (CompDBChunkMapping)xmlSerializer.Deserialize(textReader);
			}
			catch (Exception innerException2)
			{
				throw new ImageCommonException("ImageCommon!CompDBChunkMapping::ValidateAndLoad: Unable to parse CompDB Chunk Mapping XML file '" + xmlFile + "'.", innerException2);
			}
			finally
			{
				textReader.Close();
			}
			compDBChunkMapping.Languages = languages;
			compDBChunkMapping.LoadLookUpTable();
			return compDBChunkMapping;
		}

		private void LoadLookUpTable()
		{
			foreach (CompDBChunkMapItem chunkMapping in ChunkMappings)
			{
				if (chunkMapping.Path.ToUpper(CultureInfo.InvariantCulture).Contains(c_LangVariable.ToUpper(CultureInfo.InvariantCulture)))
				{
					foreach (string language in Languages)
					{
						CompDBChunkMapItem compDBChunkMapItem = new CompDBChunkMapItem(chunkMapping);
						compDBChunkMapItem.ChunkName = compDBChunkMapItem.ChunkName.Replace(c_LangVariable, language, StringComparison.OrdinalIgnoreCase);
						compDBChunkMapItem.Path = compDBChunkMapItem.Path.Replace(c_LangVariable, language, StringComparison.OrdinalIgnoreCase);
						_lookupTable[compDBChunkMapItem.Path] = compDBChunkMapItem;
					}
				}
				else
				{
					_lookupTable[chunkMapping.Path] = chunkMapping;
				}
			}
		}

		public void WriteToFile(string xmlFile)
		{
			TextWriter textWriter = new StreamWriter(LongPathFile.OpenWrite(xmlFile));
			XmlSerializer xmlSerializer = new XmlSerializer(typeof(CompDBChunkMapping));
			try
			{
				xmlSerializer.Serialize(textWriter, this);
			}
			catch (Exception innerException)
			{
				throw new ImageCommonException("CompDBChunkMapping!WriteToFile: Unable to write CompDB Chunk Mapping XML file '" + xmlFile + "'", innerException);
			}
			finally
			{
				textWriter.Close();
			}
		}
	}
}
