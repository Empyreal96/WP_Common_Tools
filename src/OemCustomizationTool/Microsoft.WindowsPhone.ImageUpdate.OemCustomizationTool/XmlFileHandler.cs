using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Schema;

namespace Microsoft.WindowsPhone.ImageUpdate.OemCustomizationTool
{
	internal class XmlFileHandler
	{
		private static List<XmlFile> mergeFileList;

		private static List<XmlFile> xmlFiles;

		public static XDocument LoadXmlDoc(ref List<XmlFile> files)
		{
			if (files == null || files.Count <= 0)
			{
				return null;
			}
			xmlFiles = files;
			if (mergeFileList != null)
			{
				mergeFileList.Clear();
			}
			try
			{
				GenerateMergeFileList();
				MergeFiles();
				XDocument xDocument = XDocument.Load(Settings.MergeFilePath);
				TraceLogger.LogMessage(TraceLevel.Info, "Merged File Contents: ");
				TraceLogger.LogMessage(TraceLevel.Info, Environment.NewLine + xDocument.ToString());
				files.Clear();
				files.AddRange(mergeFileList);
				return xDocument;
			}
			catch (Exception ex)
			{
				TraceLogger.LogMessage(TraceLevel.Warn, "There was a failure in loading xml documents.");
				TraceLogger.LogMessage(TraceLevel.Info, ex.ToString());
			}
			return null;
		}

		private static void GenerateMergeFileList()
		{
			if (mergeFileList == null)
			{
				mergeFileList = new List<XmlFile>();
			}
			foreach (XmlFile xmlFile in xmlFiles)
			{
				AddIncludesToMergeFileList(xmlFile);
				if (xmlFile.Validate())
				{
					mergeFileList.Add(xmlFile);
				}
				else
				{
					TraceLogger.LogMessage(TraceLevel.Warn, "Skipping merge of " + xmlFile.Filename);
				}
			}
		}

		private static void AddIncludesToMergeFileList(XmlFile file)
		{
			try
			{
				foreach (XElement item in XDocument.Load(file.Filename).Elements().Descendants())
				{
					if (!(item.Name.LocalName == "include"))
					{
						continue;
					}
					XmlFile xmlFile = new XmlFile(item.Attribute("href").Value, file.Schema);
					TraceLogger.LogMessage(TraceLevel.Info, "FOUND INCLUDE: " + xmlFile.Filename);
					xmlFile.ExpandFilePath();
					if (File.Exists(xmlFile.Filename))
					{
						TraceLogger.LogMessage(TraceLevel.Info, $"Adding include file from local filesystem: {xmlFile.Filename} to merge list.");
						AddIncludesToMergeFileList(xmlFile);
						if (xmlFile.Validate())
						{
							mergeFileList.Add(xmlFile);
						}
						else
						{
							TraceLogger.LogMessage(TraceLevel.Warn, "Skipping merge of " + xmlFile.Filename);
						}
					}
					else if (Uri.IsWellFormedUriString(xmlFile.Filename, UriKind.RelativeOrAbsolute))
					{
						TraceLogger.LogMessage(TraceLevel.Info, $"Adding include file from remote uri: {xmlFile.Filename} to merge list.");
						AddIncludesToMergeFileList(xmlFile);
						if (xmlFile.Validate())
						{
							mergeFileList.Add(xmlFile);
						}
						else
						{
							TraceLogger.LogMessage(TraceLevel.Warn, "Skipping merge of " + xmlFile.Filename);
						}
					}
					else
					{
						TraceLogger.LogMessage(TraceLevel.Warn, $"The include file {xmlFile.Filename} specified was not found. This file will be skipped during merge.");
					}
				}
			}
			catch (Exception ex)
			{
				TraceLogger.LogMessage(TraceLevel.Warn, $"Error encountered when processing file {file.Filename}.");
				TraceLogger.LogMessage(TraceLevel.Info, ex.ToString());
				if (ex.InnerException != null)
				{
					TraceLogger.LogMessage(TraceLevel.Info, ex.InnerException.ToString());
				}
			}
		}

		private static void MergeFiles()
		{
			XmlTextReader xmlTextReader = null;
			DataSet dataSet = new DataSet();
			dataSet.Locale = CultureInfo.InvariantCulture;
			DataSet dataSet2 = new DataSet();
			dataSet2.Locale = CultureInfo.InvariantCulture;
			try
			{
				foreach (XmlFile mergeFile in mergeFileList)
				{
					TraceLogger.LogMessage(TraceLevel.Info, "Merging: " + mergeFile.Filename);
					xmlTextReader = new XmlTextReader(mergeFile.Filename);
					Stream manifestResourceStream = Assembly.GetExecutingAssembly().GetManifestResourceStream(mergeFile.Schema);
					if (manifestResourceStream == null)
					{
						throw new SystemException("Failed to load the embedded schema file: " + mergeFile.Schema);
					}
					using (XmlTextReader reader = new XmlTextReader(manifestResourceStream))
					{
						dataSet2.ReadXmlSchema(reader);
					}
					dataSet2.ReadXml(xmlTextReader);
					dataSet.Merge(dataSet2);
					dataSet2.Clear();
				}
				dataSet.WriteXml(Settings.MergeFilePath);
			}
			catch (Exception ex)
			{
				TraceLogger.LogMessage(TraceLevel.Error, "Exception: " + ex.ToString());
			}
		}

		public static bool ValidateSchema(XElement doc, string schemaFilename)
		{
			try
			{
				Stream manifestResourceStream = Assembly.GetExecutingAssembly().GetManifestResourceStream(schemaFilename);
				if (manifestResourceStream == null)
				{
					throw new SystemException("Failed to load the embedded schema file: " + schemaFilename);
				}
				ValidationEventHandler validationEventHandler = ValidationEventHandler;
				XmlSchema schema = null;
				using (XmlTextReader reader = new XmlTextReader(manifestResourceStream))
				{
					schema = XmlSchema.Read(reader, validationEventHandler);
				}
				XmlReaderSettings xmlReaderSettings = new XmlReaderSettings();
				xmlReaderSettings.Schemas.Add(schema);
				xmlReaderSettings.ValidationType = ValidationType.Schema;
				xmlReaderSettings.ValidationFlags |= XmlSchemaValidationFlags.ReportValidationWarnings;
				XmlReader reader2 = XmlReader.Create(doc.CreateReader(), xmlReaderSettings);
				XmlDocument xmlDocument = new XmlDocument();
				xmlDocument.Load(reader2);
				xmlDocument.Validate(validationEventHandler);
				return true;
			}
			catch (Exception ex)
			{
				TraceLogger.LogMessage(TraceLevel.Warn, "There are schema validation errors in:" + schemaFilename + Environment.NewLine + ex.ToString());
				return false;
			}
		}

		private static void ValidationEventHandler(object sender, ValidationEventArgs e)
		{
			switch (e.Severity)
			{
			case XmlSeverityType.Error:
				Console.WriteLine("Error: {0}", e.Message);
				break;
			case XmlSeverityType.Warning:
				Console.WriteLine("Warning: {0}", e.Message);
				break;
			}
		}
	}
}
