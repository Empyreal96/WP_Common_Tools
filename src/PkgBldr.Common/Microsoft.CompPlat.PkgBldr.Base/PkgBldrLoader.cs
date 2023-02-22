using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.IO;
using System.Xml.Linq;
using System.Xml.Schema;
using Microsoft.CompPlat.PkgBldr.Base.Tools;
using Microsoft.CompPlat.PkgBldr.Interfaces;
using Microsoft.CompPlat.PkgBldr.Tools;

namespace Microsoft.CompPlat.PkgBldr.Base
{
	public class PkgBldrLoader
	{
		private Dictionary<string, IPkgPlugin> m_plugins;

		private Dictionary<string, IPkgPlugin> m_wmPlugins;

		private PluginType m_pluginType;

		private SchemaSet m_csiSchemaValidator;

		private SchemaSet m_wmSchemaValidator;

		private SchemaSet m_pkgSchemaValidator;

		private IDeploymentLogger m_logger;

		private PkgBldrCmd m_pkgBldrArgs;

		private string m_csiXsdPath;

		private string m_pkgXsdPath;

		private string m_sharedXsdPath;

		private string m_wmXsdPath;

		public Dictionary<string, IPkgPlugin> Plugins => m_plugins;

		public PkgBldrLoader(PluginType pluginType, PkgBldrCmd pkgBldrArgs, IDeploymentLogger logger = null)
		{
			if (pkgBldrArgs == null)
			{
				throw new ArgumentNullException("pkgBldrArgs");
			}
			m_logger = logger ?? new Logger();
			m_pkgBldrArgs = pkgBldrArgs;
			m_csiXsdPath = Environment.ExpandEnvironmentVariables(pkgBldrArgs.csiXsdPath);
			m_pkgXsdPath = Environment.ExpandEnvironmentVariables(pkgBldrArgs.pkgXsdPath);
			m_sharedXsdPath = Environment.ExpandEnvironmentVariables(pkgBldrArgs.sharedXsdPath);
			m_wmXsdPath = Environment.ExpandEnvironmentVariables(pkgBldrArgs.wmXsdPath);
			m_pluginType = pluginType;
			LoadPlugins();
		}

		public List<XmlSchema> WmSchemaSet()
		{
			return m_wmSchemaValidator.GetMergedSchemas();
		}

		public List<XmlSchema> CsiSchemaSet()
		{
			return m_csiSchemaValidator.GetMergedSchemas();
		}

		private void LoadPlugins()
		{
			m_plugins = null;
			m_wmPlugins = null;
			m_plugins = LoadPackagePlugins(m_pluginType);
			if (m_pluginType != PluginType.PkgFilter)
			{
				if (m_pluginType == PluginType.WmToCsi)
				{
					m_wmPlugins = m_plugins;
				}
				else
				{
					m_wmPlugins = LoadPackagePlugins(PluginType.WmToCsi);
				}
				switch (m_pluginType)
				{
				case PluginType.CsiToCsi:
					m_csiSchemaValidator = null;
					LoadCsiSchemas();
					break;
				case PluginType.WmToCsi:
				case PluginType.CsiToWm:
					m_wmSchemaValidator = null;
					m_csiSchemaValidator = null;
					LoadWmSchemas();
					LoadCsiSchemas();
					break;
				case PluginType.CsiToCab:
					m_csiSchemaValidator = null;
					LoadCsiSchemas();
					break;
				case PluginType.PkgToWm:
					m_pkgSchemaValidator = null;
					m_wmSchemaValidator = null;
					LoadPkgSchemas();
					LoadWmSchemas();
					break;
				case PluginType.Csi2Pkg:
					m_csiSchemaValidator = null;
					m_pkgSchemaValidator = null;
					LoadCsiSchemas();
					LoadPkgSchemas();
					break;
				default:
					throw new PkgGenException("stream loading not supported for this plugin type");
				}
			}
		}

		private void LoadWmSchemas()
		{
			m_wmSchemaValidator = new SchemaSet(m_logger, m_pkgBldrArgs);
			m_wmSchemaValidator.LoadSchemasFromPlugins(m_wmPlugins.Values);
		}

		private void LoadCsiSchemas()
		{
			m_csiSchemaValidator = new SchemaSet(m_logger);
			List<string> list = new List<string>();
			string[] files = LongPathDirectory.GetFiles(m_csiXsdPath, "*.xsd");
			foreach (string path in files)
			{
				list.Add(LongPath.GetFullPath(path));
			}
			files = LongPathDirectory.GetFiles(m_sharedXsdPath, "*.xsd");
			foreach (string path2 in files)
			{
				list.Add(LongPath.GetFullPath(path2));
			}
			m_csiSchemaValidator.LoadSchemasFromFiles(list);
		}

		private void LoadPkgSchemas()
		{
			m_pkgSchemaValidator = new SchemaSet(m_logger);
			List<string> list = new List<string>();
			string[] files = Directory.GetFiles(m_pkgXsdPath, "*.xsd");
			foreach (string item in files)
			{
				list.Add(item);
			}
			m_pkgSchemaValidator.LoadSchemasFromFiles(list);
		}

		public void ValidateInput(XDocument xmlDoc)
		{
			switch (m_pluginType)
			{
			case PluginType.CsiToWm:
			case PluginType.CsiToCsi:
			case PluginType.Csi2Pkg:
			case PluginType.CsiToCab:
				m_csiSchemaValidator.ValidateXmlDoc(xmlDoc);
				break;
			case PluginType.PkgToWm:
				m_pkgSchemaValidator.ValidateXmlDoc(xmlDoc);
				break;
			case PluginType.WmToCsi:
				m_wmSchemaValidator.ValidateXmlDoc(xmlDoc);
				break;
			default:
				throw new PkgGenException("invalid plugin type, can't validate input schema");
			case PluginType.PkgFilter:
				break;
			}
		}

		public void ValidateOutput(XDocument xmlDoc)
		{
			switch (m_pluginType)
			{
			case PluginType.WmToCsi:
			case PluginType.CsiToCsi:
				m_csiSchemaValidator.ValidateXmlDoc(xmlDoc);
				break;
			case PluginType.Csi2Pkg:
				m_pkgSchemaValidator.ValidateXmlDoc(xmlDoc);
				break;
			case PluginType.CsiToWm:
			case PluginType.PkgToWm:
				m_wmSchemaValidator.ValidateXmlDoc(xmlDoc);
				break;
			default:
				throw new PkgGenException("invalid plugin type, can't validate output schema");
			case PluginType.CsiToCab:
			case PluginType.PkgFilter:
				break;
			}
		}

		private Dictionary<string, IPkgPlugin> LoadPackagePlugins(PluginType pType)
		{
			CompositionContainer compositionContainer = null;
			try
			{
				string directoryName = LongPath.GetDirectoryName(typeof(IPkgPlugin).Assembly.Location);
				AggregateCatalog aggregateCatalog = new AggregateCatalog();
				if (pType == PluginType.WmToCsi)
				{
					aggregateCatalog.Catalogs.Add(new AssemblyCatalog(typeof(IPkgPlugin).Assembly));
				}
				string searchPattern = null;
				switch (pType)
				{
				case PluginType.WmToCsi:
					searchPattern = "PkgBldr.Plugin.WmToCsi.*.dll";
					break;
				case PluginType.CsiToWm:
					searchPattern = "PkgBldr.Plugin.CsiToWm.*.dll";
					break;
				case PluginType.CsiToCsi:
					searchPattern = "PkgBldr.Plugin.CsiToCsi.*.dll";
					break;
				case PluginType.PkgToWm:
				case PluginType.PkgFilter:
					searchPattern = "PkgBldr.Plugin.PkgToWm.*.dll";
					break;
				case PluginType.Csi2Pkg:
					searchPattern = "PkgBldr.Plugin.CsiToPkg.*.dll";
					break;
				case PluginType.CsiToCab:
					searchPattern = "PkgBldr.Plugin.CsiToCab.*.dll";
					break;
				}
				if (LongPathDirectory.Exists(directoryName))
				{
					aggregateCatalog.Catalogs.Add(new DirectoryCatalog(directoryName, searchPattern));
				}
				CompositionBatch batch = new CompositionBatch();
				compositionContainer = new CompositionContainer(aggregateCatalog);
				compositionContainer.Compose(batch);
			}
			catch (CompositionException innerException)
			{
				throw new PkgGenException(innerException, "Failed to load package plugins.");
			}
			Dictionary<string, IPkgPlugin> dictionary = new Dictionary<string, IPkgPlugin>(StringComparer.OrdinalIgnoreCase);
			foreach (IPkgPlugin exportedValue in compositionContainer.GetExportedValues<IPkgPlugin>())
			{
				if (string.IsNullOrEmpty(exportedValue.XmlElementName))
				{
					throw new PkgGenException("Failed to load package plugin '{0}'. Invalid XmlElementName.", exportedValue.Name);
				}
				try
				{
					dictionary.Add(exportedValue.XmlElementName, exportedValue);
				}
				catch (ArgumentException innerException2)
				{
					string fileName = LongPath.GetFileName(exportedValue.GetType().Assembly.Location);
					IPkgPlugin pkgPlugin = dictionary[exportedValue.XmlElementName];
					string fileName2 = LongPath.GetFileName(pkgPlugin.GetType().Assembly.Location);
					throw new PkgGenException(innerException2, "Failed to load package plugin '{0}' ({1}). Uses a duplicate XmlElementName with '{2}' ({3}).", exportedValue.Name, fileName, pkgPlugin.Name, fileName2);
				}
			}
			return dictionary;
		}
	}
}
