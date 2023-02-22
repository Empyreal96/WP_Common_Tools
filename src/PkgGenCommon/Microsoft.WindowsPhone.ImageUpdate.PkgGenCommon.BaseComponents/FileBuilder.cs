using System;
using System.IO;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using Microsoft.WindowsPhone.ImageUpdate.PkgCommon;
using Microsoft.WindowsPhone.ImageUpdate.PkgGenCommon.BaseComponents.Internal;

namespace Microsoft.WindowsPhone.ImageUpdate.PkgGenCommon.BaseComponents
{
	public sealed class FileBuilder : FileBuilder<PkgFile, FileBuilder>
	{
		internal FileBuilder(XElement element)
			: base(element)
		{
		}

		internal FileBuilder(string source, string destinationDir)
			: base(source, destinationDir)
		{
		}
	}
	public abstract class FileBuilder<T, V> where T : PkgFile, new() where V : FileBuilder<T, V>
	{
		protected T pkgObject;

		internal FileBuilder(XElement fileElement)
		{
			pkgObject = fileElement.FromXElement<T>();
		}

		internal FileBuilder(string source, string destinationDir)
		{
			pkgObject = new T();
			pkgObject.SourcePath = source;
			pkgObject.DestinationDir = destinationDir;
		}

		public V SetAttributes(string value)
		{
			value = new Regex("\\s+", RegexOptions.Compiled).Replace(value, ",");
			FileAttributes result;
			if (!Enum.TryParse<FileAttributes>(value, out result))
			{
				throw new ArgumentException("Argument cannot be parsed.");
			}
			return SetAttributes(result);
		}

		public V SetAttributes(FileAttributes attributes)
		{
			if ((attributes & ~PkgConstants.c_validAttributes) != 0)
			{
				FileAttributes c_validAttributes = PkgConstants.c_validAttributes;
				throw new ArgumentException($"Valid attributes for packaging are: {c_validAttributes.ToString()}", "attributes");
			}
			pkgObject.Attributes = attributes;
			return (V)this;
		}

		public V SetDestinationDir(string destinationDir)
		{
			pkgObject.DestinationDir = destinationDir;
			return (V)this;
		}

		public V SetName(string name)
		{
			pkgObject.Name = name;
			return (V)this;
		}

		internal virtual T ToPkgObject()
		{
			return pkgObject;
		}
	}
}
