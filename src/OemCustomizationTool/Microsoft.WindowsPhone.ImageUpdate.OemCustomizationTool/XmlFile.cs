using System;
using System.IO;
using System.Xml.Linq;

namespace Microsoft.WindowsPhone.ImageUpdate.OemCustomizationTool
{
	internal class XmlFile
	{
		private string filename;

		private string schema;

		public string Filename
		{
			get
			{
				return filename;
			}
			set
			{
				filename = value;
			}
		}

		public string Schema
		{
			get
			{
				return schema;
			}
			set
			{
				schema = value;
			}
		}

		public XmlFile(string file, string sch)
		{
			filename = file;
			schema = sch;
		}

		public bool Validate()
		{
			return XmlFileHandler.ValidateSchema(XElement.Load(Filename), Schema);
		}

		public void ExpandFilePath()
		{
			try
			{
				filename = Environment.ExpandEnvironmentVariables(filename);
				if (schema == Settings.CustomizationSchema)
				{
					string text = Path.GetDirectoryName(filename);
					string fileName = Path.GetFileName(filename);
					if (string.IsNullOrEmpty(text))
					{
						text = Settings.CustomizationIncludeDirectory;
					}
					filename = Path.Combine(text, fileName);
				}
			}
			catch
			{
				TraceLogger.LogMessage(TraceLevel.Info, $"Couldn't get directory/filename. File='{filename}', schema='{schema}'. Ignoring.");
			}
		}
	}
}
