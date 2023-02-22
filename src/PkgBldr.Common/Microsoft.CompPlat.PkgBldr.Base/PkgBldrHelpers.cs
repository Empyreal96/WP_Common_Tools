using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Xml.Linq;
using Microsoft.CompPlat.PkgBldr.Tools;
using Microsoft.Win32.SafeHandles;

namespace Microsoft.CompPlat.PkgBldr.Base
{
	public static class PkgBldrHelpers
	{
		public static XElement AddIfNotFound(XElement parent, string name)
		{
			XElement xElement = parent.Element(parent.Name.Namespace + name);
			if (xElement == null)
			{
				xElement = new XElement(parent.Name.Namespace + name);
				parent.Add(xElement);
			}
			return xElement;
		}

		public static void SetDefaultNameSpace(XElement Parent, XNamespace NameSpace)
		{
			if (Parent == null)
			{
				return;
			}
			foreach (XElement item in Parent.DescendantsAndSelf())
			{
				item.Name = NameSpace + item.Name.LocalName;
			}
		}

		public static void ReplaceDefaultNameSpace(ref XElement parent, XNamespace oldNameSpace, XNamespace newNameSpace)
		{
			if (parent == null)
			{
				return;
			}
			foreach (XElement item in parent.DescendantsAndSelf())
			{
				if (item.Name.Namespace.NamespaceName == oldNameSpace.NamespaceName)
				{
					item.Name = newNameSpace + item.Name.LocalName;
				}
			}
		}

		public static string GetAttributeValue(XElement element, string attributeName)
		{
			return element.Attribute(attributeName)?.Value;
		}

		public static XElement GetFirstDecendant(XElement Parent, XName Name)
		{
			IEnumerable<XElement> source = Parent.Descendants(Name);
			if (source.Count() == 0)
			{
				return null;
			}
			return source.First();
		}

		public static XElement FindMatchingAttribute(XElement Parent, string ElementName, string AttributeName, string AttributeValue)
		{
			XElement result = null;
			IEnumerable<XElement> enumerable = from el in Parent.Descendants(Parent.Name.Namespace + ElementName)
				where el.Attribute(AttributeName).Value.Equals(AttributeValue, StringComparison.OrdinalIgnoreCase)
				select el;
			if (enumerable != null && enumerable.Count() > 0)
			{
				result = enumerable.First();
			}
			return result;
		}

		public static IEnumerable<XElement> FindMatchingAttributes(XElement Parent, string ElementName, string AttributeName)
		{
			return from el in Parent.Descendants(Parent.Name.Namespace + ElementName)
				where el.Attribute(AttributeName) != null
				select el;
		}

		public static XDocument XDocumentLoadFromLongPath(string path)
		{
			if (!LongPathFile.Exists(path))
			{
				throw new PkgGenException("Can't find path {0}", path);
			}
			SafeFileHandle safeFileHandle = CreateFile(path, 1u, 3u, IntPtr.Zero, 3u, 0u, IntPtr.Zero);
			XDocument result = XDocument.Load(new FileStream(safeFileHandle, FileAccess.Read));
			safeFileHandle.Close();
			return result;
		}

		public static void XDocumentSaveToLongPath(XDocument document, string path)
		{
			string directoryName = LongPath.GetDirectoryName(path);
			if (!LongPathDirectory.Exists(directoryName))
			{
				throw new PkgGenException("Can't find path {0}", directoryName);
			}
			SafeFileHandle safeFileHandle = CreateFile(path, 2u, 3u, IntPtr.Zero, 2u, 0u, IntPtr.Zero);
			FileStream stream = new FileStream(safeFileHandle, FileAccess.Write);
			document.Save(stream);
			safeFileHandle.Close();
		}

		[DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
		private static extern SafeFileHandle CreateFile(string fileName, uint desiredAccess, uint shareMode, IntPtr securityAttributes, uint creationDisposition, uint flagsAndAttributes, IntPtr templateFile);
	}
}
