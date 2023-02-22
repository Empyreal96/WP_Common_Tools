using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;
using Microsoft.WindowsPhone.ImageUpdate.PkgCommon;
using Microsoft.WindowsPhone.ImageUpdate.PkgGenCommon;

namespace Microsoft.WindowsPhone.ImageUpdate.Customization.XML
{
	[XmlRoot(ElementName = "ImageCustomizations", Namespace = "http://schemas.microsoft.com/embedded/2004/10/ImageUpdate", IsNullable = false)]
	public class ImageCustomizations : IDefinedIn
	{
		public static MacroResolver environmentMacros;

		[XmlIgnore]
		public string DefinedInFile { get; set; }

		[XmlAttribute]
		public string Name { get; set; }

		[XmlAttribute]
		public string Description { get; set; }

		[XmlAttribute]
		public string Owner { get; set; }

		[XmlAttribute]
		public OwnerType OwnerType { get; set; }

		[XmlAttribute]
		public uint Priority { get; set; }

		[XmlArray(ElementName = "Imports")]
		[XmlArrayItem(ElementName = "Import", Type = typeof(Import), IsNullable = false)]
		public List<Import> Imports { get; set; }

		[XmlArray(ElementName = "Targets")]
		[XmlArrayItem(ElementName = "Target", Type = typeof(Target), IsNullable = false)]
		public List<Target> Targets { get; set; }

		[XmlElement(ElementName = "Static")]
		public StaticVariant StaticVariant { get; set; }

		[XmlElement(ElementName = "Variant")]
		public List<Variant> Variants { get; set; }

		public bool ShouldSerializePriority()
		{
			return Priority != 0;
		}

		public ImageCustomizations()
		{
			if (environmentMacros == null)
			{
				ImportEnvironmentToMacros();
			}
			Imports = new List<Import>();
			Targets = new List<Target>();
			StaticVariant = null;
			Variants = new List<Variant>();
		}

		public static IEnumerable<CustomizationError> ImportEnvironmentToMacros()
		{
			List<CustomizationError> list = new List<CustomizationError>();
			if (environmentMacros != null)
			{
				return list;
			}
			environmentMacros = new MacroResolver();
			foreach (DictionaryEntry environmentVariable in Environment.GetEnvironmentVariables())
			{
				try
				{
					environmentMacros.Register(environmentVariable.Key.ToString(), environmentVariable.Value.ToString());
				}
				catch (PkgGenException ex)
				{
					list.Add(new CustomizationError(CustomizationErrorSeverity.Warning, null, "Macro will be ignored : {0}", ex.Message));
				}
			}
			if (environmentMacros.Unregister(Strings.CurrentFileMacro))
			{
				list.Add(new CustomizationError(CustomizationErrorSeverity.Warning, null, Strings.CurrentFileDirOverride, Strings.CurrentFileMacro));
			}
			return list;
		}

		private void RefreshEnvironmentMacros()
		{
			environmentMacros = null;
			ImportEnvironmentToMacros();
		}

		public static string ExpandPath(string path)
		{
			return ExpandPath(path, false);
		}

		public static string ExpandPath(string path, bool generateError)
		{
			if (generateError)
			{
				return environmentMacros.Resolve(path, MacroResolveOptions.ErrorOnUnknownMacro);
			}
			return environmentMacros.Resolve(path, MacroResolveOptions.SkipOnUnknownMacro);
		}

		public static ImageCustomizations LoadFromPath(string filePath)
		{
			filePath = Path.GetFullPath(filePath);
			try
			{
				using (TextReader reader = File.OpenText(filePath))
				{
					ImageCustomizations imageCustomizations = LoadFromReader(reader);
					imageCustomizations.DefinedInFile = filePath;
					imageCustomizations.LinkChildrenToFile();
					imageCustomizations.SetApplicationVariantType();
					return imageCustomizations;
				}
			}
			catch (Exception innerException)
			{
				throw new CustomizationException($"Error with customization file '{filePath}'", innerException);
			}
		}

		public Target GetTargetWithId(string targetId)
		{
			return Targets.Where((Target x) => x.Id.Equals(targetId, StringComparison.Ordinal)).FirstOrNull();
		}

		public IEnumerable<Variant> GetVariantsWithTargetId(string targetId)
		{
			foreach (Variant variant in Variants)
			{
				if (variant.TargetRefs.Where((TargetRef x) => x.Id.Equals(targetId, StringComparison.Ordinal)).Count() > 0)
				{
					yield return variant;
				}
			}
		}

		public void Save(string path)
		{
			path = Path.GetFullPath(path);
			File.Delete(path);
			using (Stream stream = File.OpenWrite(path))
			{
				new XmlSerializer(typeof(ImageCustomizations)).Serialize(stream, this);
				DefinedInFile = path;
			}
		}

		public ImageCustomizations GetMergedCustomizations(out IEnumerable<CustomizationError> errors)
		{
			List<CustomizationError> list = new List<CustomizationError>();
			List<ImageCustomizations> list2 = new List<ImageCustomizations>();
			list2.Add(this);
			list2.AddRange(GetLinkedCustomizations());
			ImageCustomizations imageCustomizations = new ImageCustomizations();
			IEnumerable<IGrouping<uint, ImageCustomizations>> source = from x in list2
				group x by x.Priority;
			foreach (IGrouping<uint, ImageCustomizations> item in source.OrderBy((IGrouping<uint, ImageCustomizations> x) => x.Key).Reverse())
			{
				if (item.Key == 0)
				{
					continue;
				}
				ImageCustomizations imageCustomizations2 = new ImageCustomizations();
				foreach (ImageCustomizations item2 in item)
				{
					list.AddRange(imageCustomizations2.Merge(item2));
				}
				list.AddRange(imageCustomizations.Merge(imageCustomizations2, true));
			}
			IGrouping<uint, ImageCustomizations> grouping = source.SingleOrDefault((IGrouping<uint, ImageCustomizations> x) => x.Key == 0);
			if (grouping != null)
			{
				foreach (ImageCustomizations item3 in grouping)
				{
					list.AddRange(imageCustomizations.Merge(item3));
				}
			}
			imageCustomizations.Name = Name;
			imageCustomizations.Description = Description;
			imageCustomizations.Owner = Owner;
			imageCustomizations.OwnerType = OwnerType;
			imageCustomizations.DefinedInFile = DefinedInFile;
			errors = list;
			return imageCustomizations;
		}

		private IEnumerable<ImageCustomizations> GetLinkedCustomizations()
		{
			List<ImageCustomizations> list = new List<ImageCustomizations>();
			foreach (Import import in Imports)
			{
				environmentMacros.BeginLocal();
				ImageCustomizations imageCustomizations;
				try
				{
					environmentMacros.Register(Strings.CurrentFileMacro, Path.GetDirectoryName(DefinedInFile));
					IEnumerable<CustomizationError> source = CustomContentGenerator.VerifyImportSource(import);
					if (source.Any((CustomizationError x) => x.Severity.Equals(CustomizationErrorSeverity.Error)))
					{
						throw new CustomizationException(source.First().Message);
					}
					imageCustomizations = LoadFromPath(import.ExpandedSourcePath);
				}
				finally
				{
					environmentMacros.EndLocal();
				}
				list.Add(imageCustomizations);
				list.AddRange(imageCustomizations.GetLinkedCustomizations());
			}
			return list;
		}

		public IEnumerable<CustomizationError> Merge(ImageCustomizations customizations)
		{
			return Merge(customizations, false);
		}

		public IEnumerable<CustomizationError> Merge(ImageCustomizations customizations, bool allowOverride)
		{
			if (customizations == null)
			{
				throw new ArgumentNullException("customizations");
			}
			List<CustomizationError> list = new List<CustomizationError>();
			IEnumerable<Target> source = customizations.Targets.Concat(Targets);
			foreach (IGrouping<string, Target> item2 in from x in source
				group x by x.Id into grp
				where grp.Count() > 1
				select grp)
			{
				CustomizationError item = new CustomizationError((!allowOverride) ? CustomizationErrorSeverity.Error : CustomizationErrorSeverity.Warning, item2, Strings.DuplicateTargets, item2.Key);
				list.Add(item);
			}
			Targets = source.DistinctBy((Target x) => x.Id).ToList();
			if (StaticVariant == null)
			{
				StaticVariant = customizations.StaticVariant;
			}
			else if (customizations.StaticVariant != null)
			{
				IEnumerable<CustomizationError> collection = StaticVariant.Merge(customizations.StaticVariant, allowOverride);
				list.AddRange(collection);
			}
			foreach (Variant variant2 in customizations.Variants)
			{
				string id = variant2.TargetRefs.First().Id;
				Variant variant = GetVariantsWithTargetId(id).FirstOrDefault();
				if (variant != null)
				{
					IEnumerable<CustomizationError> collection2 = variant.Merge(variant2, allowOverride);
					list.AddRange(collection2);
				}
				else
				{
					Variants.Add(variant2);
				}
			}
			return list;
		}

		public bool ShouldSerializeImports()
		{
			return Imports.Count > 0;
		}

		public bool ShouldSerializeTargets()
		{
			return Targets.Count > 0;
		}

		private void LinkChildrenToFile()
		{
			foreach (Target target in Targets)
			{
				target.DefinedInFile = DefinedInFile;
			}
			if (StaticVariant != null)
			{
				StaticVariant.LinkToFile(this);
			}
			foreach (Variant variant in Variants)
			{
				variant.LinkToFile(this);
			}
		}

		private void SetApplicationVariantType()
		{
			foreach (Application item in Variants.SelectMany((Variant x) => x.ApplicationGroups).SelectMany((Applications x) => x.Items))
			{
				item.StaticApp = false;
			}
			if (StaticVariant == null)
			{
				return;
			}
			foreach (Application item2 in StaticVariant.ApplicationGroups.SelectMany((Applications x) => x.Items))
			{
				item2.StaticApp = true;
			}
		}

		private static ImageCustomizations LoadFromReader(TextReader reader)
		{
			Assembly executingAssembly = Assembly.GetExecutingAssembly();
			executingAssembly.GetManifestResourceNames();
			using (XmlReader reader2 = XmlReader.Create(executingAssembly.GetManifestResourceStream("Customization.xsd")))
			{
				XmlSchema schema = XmlSchema.Read(reader2, null);
				XmlReaderSettings xmlReaderSettings = new XmlReaderSettings();
				xmlReaderSettings.Schemas.Add(schema);
				xmlReaderSettings.ValidationType = ValidationType.Schema;
				XmlReader xmlReader = XmlReader.Create(reader, xmlReaderSettings);
				return (ImageCustomizations)new XmlSerializer(typeof(ImageCustomizations)).Deserialize(xmlReader);
			}
		}
	}
}
