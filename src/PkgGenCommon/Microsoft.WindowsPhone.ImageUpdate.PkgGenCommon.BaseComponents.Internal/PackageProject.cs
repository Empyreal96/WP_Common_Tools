using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Schema;
using System.Xml.Serialization;
using Microsoft.WindowsPhone.ImageUpdate.PkgCommon;
using Microsoft.WindowsPhone.ImageUpdate.PkgGenCommon.Interfaces;
using Microsoft.WindowsPhone.ImageUpdate.Tools.Common;

namespace Microsoft.WindowsPhone.ImageUpdate.PkgGenCommon.BaseComponents.Internal
{
	[XmlRoot(Namespace = "urn:Microsoft.WindowsPhone/PackageSchema.v8.00", ElementName = "Package")]
	public class PackageProject
	{
		[XmlRoot(Namespace = "urn:Microsoft.WindowsPhone/PackageSchema.v8.00", ElementName = "Field")]
		public class MetadataField
		{
			[XmlAttribute("Name")]
			public string Key { get; set; }

			[XmlText]
			public string Value { get; set; }
		}

		[XmlIgnore]
		private XmlValidator Validator;

		[XmlIgnore]
		private XDocument Document;

		[XmlIgnore]
		private XElement SecurityCapabilities;

		[XmlIgnore]
		private XElement SecurityAuthorization;

		[XmlIgnore]
		private List<PkgObject> Components;

		[XmlAttribute("Owner")]
		public string Owner { get; set; }

		[XmlAttribute("Component")]
		public string Component { get; set; }

		[XmlAttribute("SubComponent")]
		public string SubComponent { get; set; }

		[XmlIgnore]
		public string Name => PackageTools.BuildPackageName(Owner, Component, SubComponent);

		[XmlAttribute("OwnerType")]
		public OwnerType OwnerType { get; set; }

		[XmlAttribute("ReleaseType")]
		public ReleaseType ReleaseType { get; set; }

		[XmlAttribute("Platform")]
		public string Platform { get; set; }

		[XmlAttribute("Partition")]
		public string Partition { get; set; }

		[XmlAttribute("GroupingKey")]
		public string GroupingKey { get; set; }

		[XmlAttribute("Description")]
		public string Description { get; set; }

		[XmlAttribute("BinaryPartition")]
		public bool IsBinaryPartition { get; set; }

		[XmlElement("Macros")]
		public MacroTable MacroTable { get; set; }

		[XmlArray("CustomMetadata")]
		[XmlArrayItem(typeof(MetadataField), ElementName = "Field")]
		public List<MetadataField> CustomMetadata { get; }

		[XmlIgnore]
		public string ProjectFilePath { get; protected set; }

		public PackageProject()
		{
			IsBinaryPartition = false;
			Partition = PkgConstants.c_strMainOsPartition;
			ReleaseType = ReleaseType.Production;
			OwnerType = OwnerType.Microsoft;
			Components = new List<PkgObject>();
			CustomMetadata = new List<MetadataField>();
		}

		private void Preprocess(IMacroResolver macroResolver)
		{
			Owner = macroResolver.Resolve(Owner);
			Component = macroResolver.Resolve(Component);
			SubComponent = macroResolver.Resolve(SubComponent);
			Platform = macroResolver.Resolve(Platform);
			Partition = macroResolver.Resolve(Partition);
			GroupingKey = macroResolver.Resolve(GroupingKey);
			Description = macroResolver.Resolve(Description);
			if (string.IsNullOrEmpty(Partition))
			{
				Partition = PkgConstants.c_strMainOsPartition;
			}
			if (MacroTable != null)
			{
				macroResolver.Register(MacroTable.Values);
			}
			Components.ForEach(delegate(PkgObject x)
			{
				x.Preprocess(this, macroResolver);
			});
		}

		private void Validate()
		{
			if (IsBinaryPartition)
			{
				if (Components == null || Components.Count != 1 || !(Components[0] is BinaryPartitionPkgObject))
				{
					throw new PkgGenProjectException(ProjectFilePath, "BinaryPartition package requires exactly one BinaryPartition object");
				}
				if (Partition.Equals(PkgConstants.c_strMainOsPartition))
				{
					throw new PkgGenProjectException(ProjectFilePath, "A package in MainOS partition can't be binary partition");
				}
			}
			else if (Components != null && Components.OfType<BinaryPartitionPkgObject>().Count() != 0)
			{
				throw new PkgGenProjectException(ProjectFilePath, "BinaryPartition object can only be included in a package with BinaryPartition attribute set");
			}
			if (OwnerType != OwnerType.Microsoft && Platform == null)
			{
				throw new PkgGenProjectException(ProjectFilePath, "Platform needs to be specified for all non-Microsoft packages");
			}
		}

		private static void OnUnknownAttribute(object sender, XmlAttributeEventArgs e)
		{
			LogUtil.Diagnostic("Unknown attribute {0} at line {1}", e.Attr.Name, e.LineNumber);
		}

		private static void OnUnknownElement(object sender, XmlElementEventArgs e)
		{
			LogUtil.Diagnostic("Unknown element {0} at line {1}", e.Element.Name, e.LineNumber);
		}

		private static void OnUnknownNode(object sender, XmlNodeEventArgs e)
		{
			LogUtil.Diagnostic("Unknown node {0} at line {1}", e.Name, e.LineNumber);
		}

		private static void OnUnreferencedObject(object sender, UnreferencedObjectEventArgs e)
		{
			LogUtil.Diagnostic("Unreferenced object {0}", e.UnreferencedId);
		}

		public void Build(IPackageGenerator pkgGen)
		{
			try
			{
				Components.ForEach(delegate(PkgObject x)
				{
					x.Build(pkgGen);
				});
				BuildCustomMetadata(pkgGen);
			}
			catch (Exception ex)
			{
				throw new PkgGenProjectException(ex, ProjectFilePath, ex.Message);
			}
		}

		public void AddToCapabilities(XElement element)
		{
			try
			{
				SecurityCapabilities.Add(element);
				Validator.Validate(element);
			}
			catch (Exception ex)
			{
				if (element.Parent != null)
				{
					element.Remove();
				}
				throw ex;
			}
		}

		public void AddToAuthorization(XElement element)
		{
			try
			{
				SecurityAuthorization.Add(element);
				Validator.Validate(element);
			}
			catch (Exception ex)
			{
				if (element.Parent != null)
				{
					element.Remove();
				}
				throw ex;
			}
		}

		public XmlDocument ToXmlDocument()
		{
			XmlDocument xmlDocument = new XmlDocument();
			using (XmlReader reader = Document.CreateReader())
			{
				xmlDocument.Load(reader);
				return xmlDocument;
			}
		}

		private void BuildCustomMetadata(IPackageGenerator pkgGen)
		{
			if (CustomMetadata == null || CustomMetadata.Count <= 0)
			{
				return;
			}
			string path = PackageTools.BuildPackageName(Owner, Component, SubComponent) + PkgConstants.c_strCustomMetadataExtension;
			string text = Path.Combine(pkgGen.TempDirectory, path);
			string devicePath = Path.Combine(PkgConstants.c_strCustomMetadataDeviceFolder, path);
			XNamespace xNamespace = "urn:Microsoft.WindowsPhone/PackageSchema.v8.00";
			XElement xElement = new XElement(xNamespace + "CustomMetadata");
			foreach (MetadataField customMetadatum in CustomMetadata)
			{
				string value = pkgGen.MacroResolver.Resolve(customMetadatum.Key);
				string text2 = pkgGen.MacroResolver.Resolve(customMetadatum.Value);
				XElement content = new XElement(xNamespace + "Field", new XAttribute("Name", value), text2);
				xElement.Add(content);
			}
			new XDocument(xElement).Save(text);
			pkgGen.AddFile(text, devicePath, PkgConstants.c_defaultAttributes);
		}

		public static PackageProject Load(string projPath, Dictionary<string, IPkgPlugin> plugins, IPackageGenerator packageGenerator)
		{
			PackageProject packageProject = null;
			try
			{
				using (XmlReader projXmlReader = XmlReader.Create(projPath))
				{
					packageProject = Load(projXmlReader, plugins, packageGenerator);
					packageProject.ProjectFilePath = projPath;
				}
			}
			catch (PkgXmlException ex)
			{
				IXmlLineInfo xmlElement = ex.XmlElement;
				if (xmlElement.HasLineInfo())
				{
					throw new PkgGenProjectException(projPath, xmlElement.LineNumber, xmlElement.LinePosition, ex.MessageTrace);
				}
				throw new PkgGenProjectException(projPath, ex.MessageTrace);
			}
			catch (XmlSchemaValidationException ex2)
			{
				throw new PkgGenProjectException(ex2, projPath, ex2.LineNumber, ex2.LinePosition, ex2.Message);
			}
			catch (XmlException innerException)
			{
				throw new PkgGenProjectException(innerException, projPath, "Failed to load XML file.");
			}
			packageProject.Preprocess(packageGenerator.MacroResolver);
			packageProject.Validate();
			return packageProject;
		}

		private static PackageProject Load(XmlReader projXmlReader, Dictionary<string, IPkgPlugin> plugins, IPackageGenerator packageGenerator)
		{
			packageGenerator.MacroResolver.BeginLocal();
			try
			{
				XDocument xDocument = XDocument.Load(projXmlReader, LoadOptions.SetLineInfo);
				packageGenerator.XmlValidator.Validate(xDocument);
				XmlSerializer xmlSerializer = new XmlSerializer(typeof(PackageProject));
				PackageProject packageProject = null;
				using (XmlReader xmlReader = xDocument.CreateReader())
				{
					packageProject = (PackageProject)xmlSerializer.Deserialize(xmlReader);
					packageProject.Validator = packageGenerator.XmlValidator;
					packageProject.Document = xDocument;
					packageProject.SecurityCapabilities = xDocument.Root.LocalElement("Capabilities") ?? new XElement("Capabilities");
					packageProject.SecurityAuthorization = xDocument.Root.LocalElement("Authorization") ?? new XElement("Authorization");
				}
				if (packageProject.MacroTable != null)
				{
					packageGenerator.MacroResolver.Register(packageProject.MacroTable.Values);
				}
				XElement xElement = xDocument.Root.LocalElement("Components");
				if (xElement != null)
				{
					XElement xElement2 = new XElement(xElement);
					xElement2.RemoveNodes();
					xElement.ReplaceWith(xElement2);
					packageProject.Validator.Validate(xElement2);
					IEnumerable<IGrouping<string, XElement>> enumerable = from element in xElement.Elements()
						group element by element.Name.LocalName;
					PackageWrapper packageGenerator2 = new PackageWrapper(packageGenerator, packageProject);
					foreach (IGrouping<string, XElement> item in enumerable)
					{
						IPkgPlugin value = null;
						if (plugins.TryGetValue(item.Key, out value))
						{
							value.ValidateEntries(packageGenerator2, new List<XElement>(item));
							continue;
						}
						throw new PkgXmlException(item.First(), "Unknown component '{0}' used.", item.Key);
					}
					foreach (IGrouping<string, XElement> item2 in enumerable)
					{
						IPkgPlugin pkgPlugin = plugins[item2.Key];
						IEnumerable<PkgObject> enumerable2 = pkgPlugin.ProcessEntries(packageGenerator2, item2);
						if (enumerable2 != null)
						{
							packageProject.Components.AddRange(enumerable2);
						}
						if (pkgPlugin.UseSecurityCompilerPassthrough)
						{
							xElement2.Add(item2);
						}
						else
						{
							if (enumerable2 == null)
							{
								continue;
							}
							foreach (PkgObject item3 in enumerable2)
							{
								XElement xElement3 = item3.ToXElement();
								try
								{
									xElement2.Add(xElement3);
									packageProject.Validator.Validate(xElement3);
								}
								catch (XmlSchemaValidationException ex)
								{
									throw new PkgGenProjectException(ex, packageProject.ProjectFilePath, "Plugin '{0}' returned a component with invalid resulting XML: {1} \n{2}", pkgPlugin.Name, ex.Message, xElement3.ToString());
								}
							}
						}
					}
				}
				return packageProject;
			}
			catch (InvalidOperationException ex2)
			{
				if (ex2.InnerException != null)
				{
					throw ex2.InnerException;
				}
				throw;
			}
			finally
			{
				packageGenerator.MacroResolver.EndLocal();
			}
		}
	}
}
