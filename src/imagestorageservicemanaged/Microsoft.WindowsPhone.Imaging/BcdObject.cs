using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using Microsoft.WindowsPhone.ImageUpdate.Tools.Common;

namespace Microsoft.WindowsPhone.Imaging
{
	public class BcdObject
	{
		private List<BcdElement> _elements = new List<BcdElement>();

		public string Name { get; set; }

		public Guid Id { get; set; }

		[CLSCompliant(false)]
		public uint Type { get; private set; }

		public List<BcdElement> Elements => _elements;

		public BcdObject(string objectName)
		{
			Name = objectName;
			try
			{
				Id = new Guid(objectName);
			}
			catch (Exception innerException)
			{
				throw new ImageStorageException($"{MethodBase.GetCurrentMethod().Name}: Object '{objectName}' isn't a valid ID.", innerException);
			}
		}

		[CLSCompliant(false)]
		public BcdObject(Guid objectId, uint dataType)
		{
			Id = objectId;
			Name = $"{{{objectId}}}";
			Type = dataType;
		}

		public void ReadFromRegistry(OfflineRegistryHandle objectKey)
		{
			OfflineRegistryHandle offlineRegistryHandle = null;
			OfflineRegistryHandle offlineRegistryHandle2 = null;
			try
			{
				offlineRegistryHandle = objectKey.OpenSubKey("Description");
				uint type = (uint)offlineRegistryHandle.GetValue("Type", 0);
				Type = type;
			}
			catch (ImageStorageException)
			{
				throw;
			}
			catch (Exception innerException)
			{
				throw new ImageStorageException($"{MethodBase.GetCurrentMethod().Name}: There was a problem accessing the Description key for object {Name}", innerException);
			}
			finally
			{
				if (offlineRegistryHandle != null)
				{
					offlineRegistryHandle.Close();
					offlineRegistryHandle = null;
				}
			}
			try
			{
				offlineRegistryHandle2 = objectKey.OpenSubKey("Elements");
				string[] subKeyNames = offlineRegistryHandle2.GetSubKeyNames();
				foreach (string text in subKeyNames)
				{
					OfflineRegistryHandle offlineRegistryHandle3 = null;
					try
					{
						offlineRegistryHandle3 = offlineRegistryHandle2.OpenSubKey(text);
						Elements.Add(BcdElement.CreateElement(offlineRegistryHandle3));
					}
					catch (Exception innerException2)
					{
						throw new ImageStorageException($"{MethodBase.GetCurrentMethod().Name}: There was a problem accessing element {text} for object {{{Name}}}", innerException2);
					}
					finally
					{
						if (offlineRegistryHandle3 != null)
						{
							offlineRegistryHandle3.Close();
							offlineRegistryHandle3 = null;
						}
					}
				}
			}
			catch (ImageStorageException)
			{
				throw;
			}
			catch (Exception innerException3)
			{
				throw new ImageStorageException($"{MethodBase.GetCurrentMethod().Name}: There was a problem accessing the Elements key for object {Name}", innerException3);
			}
			finally
			{
				offlineRegistryHandle2.Close();
				offlineRegistryHandle2 = null;
			}
		}

		public void AddElement(BcdElement element)
		{
			foreach (BcdElement element2 in Elements)
			{
				if (element2.DataType == element.DataType)
				{
					throw new ImageStorageException($"{MethodBase.GetCurrentMethod().Name}: A bcd element with the given datatype already exists.");
				}
			}
			Elements.Add(element);
		}

		public Guid GetDefaultObjectId()
		{
			Guid empty = Guid.Empty;
			foreach (BcdElement element in Elements)
			{
				if (element.DataType.Equals(BcdElementDataTypes.DefaultObject))
				{
					BcdElementObject bcdElementObject = element as BcdElementObject;
					if (bcdElementObject != null)
					{
						return bcdElementObject.ElementObject;
					}
				}
			}
			return empty;
		}

		[CLSCompliant(false)]
		public void LogInfo(IULogger logger, int indentLevel)
		{
			string text = new StringBuilder().Append(' ', indentLevel).ToString();
			logger.LogInfo(text + "BCD Object: {{{0}}}", Id);
			if (BcdObjects.BootObjectList.ContainsKey(Id))
			{
				logger.LogInfo(text + "Friendly Name: {0}", BcdObjects.BootObjectList[Id].Name);
			}
			logger.LogInfo("");
			foreach (BcdElement element in Elements)
			{
				element.LogInfo(logger, checked(indentLevel + 2));
				logger.LogInfo("");
			}
		}

		public override string ToString()
		{
			if (!string.IsNullOrEmpty(Name))
			{
				return Name;
			}
			return "Unnamed BcdObject";
		}
	}
}
