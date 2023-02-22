using System;
using System.Collections;
using System.IO;
using System.Xml;

namespace WPTCEditorTool
{
	internal class TCDEPS
	{
		public readonly string localPackagesRootPath = ProgramFilesX86() + "\\Windows Phone Blue Test Central Test Content\\Packages\\PreBuilt";

		private readonly string depsType = "*dep.xml";

		private ArrayList jobLog = new ArrayList();

		public void AddTags(string sDepXml)
		{
			XmlDocument xmlDocument = new XmlDocument();
			jobLog.Add("Loading doc: " + sDepXml);
			xmlDocument.Load(sDepXml);
			XmlNodeList xmlNodeList = xmlDocument.SelectNodes("/Required/RemoteFile");
			if (xmlNodeList.Count <= 0)
			{
				return;
			}
			int num = 0;
			foreach (XmlNode item in xmlNodeList)
			{
				XmlAttribute xmlAttribute = item.Attributes["Tags"];
				if (xmlAttribute == null)
				{
					XmlAttribute xmlAttribute2 = xmlDocument.CreateAttribute("Tags");
					xmlAttribute2.Value = "FOAllJobs";
					item.Attributes.Append(xmlAttribute2);
					num++;
				}
			}
			jobLog.Add(num + " tags were added for " + sDepXml);
			xmlDocument.Save(sDepXml);
		}

		public void AddTagsLocalInstall(string xmlName)
		{
			string filePath = GetFilePath(xmlName, localPackagesRootPath);
			if (filePath == null)
			{
				Console.WriteLine("ERROR: File not found: " + xmlName);
			}
			else
			{
				AddTags(filePath);
			}
		}

		public void AddTagsFullInstall(string packagesRootPath)
		{
			if (string.IsNullOrEmpty(packagesRootPath))
			{
				packagesRootPath = localPackagesRootPath;
			}
			if (!Directory.Exists(packagesRootPath))
			{
				throw new Exception("Path does not exist: " + packagesRootPath + ". Please ensure that Test Central is installed.");
			}
			DirectoryInfo directoryInfo = new DirectoryInfo(packagesRootPath);
			FileInfo[] files = directoryInfo.GetFiles(depsType, SearchOption.AllDirectories);
			FileInfo[] array = files;
			foreach (FileInfo fileInfo in array)
			{
				AddTags(fileInfo.FullName);
			}
		}

		private static string ProgramFilesX86()
		{
			if (Directory.Exists("C:\\Program Files (x86)"))
			{
				return "C:\\Program Files (x86)";
			}
			return "C:\\Program Files";
		}

		public string GetFilePath(string fileName, string rootPath)
		{
			if (!Directory.Exists(rootPath))
			{
				Console.WriteLine("ERROR: Path does not exist: " + rootPath + ". Please ensure that Test Central is installed.");
				return null;
			}
			string[] directories = Directory.GetDirectories(rootPath);
			foreach (string path in directories)
			{
				string[] files = Directory.GetFiles(path);
				foreach (string text in files)
				{
					if (text.ToLower().Contains(fileName.ToLower()))
					{
						return text;
					}
				}
			}
			return null;
		}

		public void PrintGeneralLog()
		{
			string path = Path.Combine(Directory.GetCurrentDirectory(), "WPTCEditorLog.txt");
			FileStream fileStream = File.Create(path);
			fileStream.Close();
			foreach (string item in jobLog)
			{
				using (StreamWriter streamWriter = File.AppendText(path))
				{
					streamWriter.WriteLine(item);
				}
			}
		}
	}
}
