using System.Collections.Generic;
using System.Xml.Linq;
using Microsoft.WindowsPhone.ImageUpdate.PkgGenCommon.BaseComponents.Internal;

namespace Microsoft.WindowsPhone.ImageUpdate.PkgGenCommon.BaseComponents
{
	public sealed class FileGroupBuilder : FilterGroupBuilder<FileGroup, FileGroupBuilder>
	{
		private List<FileBuilder> files;

		public FileGroupBuilder()
		{
			files = new List<FileBuilder>();
		}

		public FileBuilder AddFile(XElement fileElement)
		{
			FileBuilder fileBuilder = new FileBuilder(fileElement);
			files.Add(fileBuilder);
			return fileBuilder;
		}

		public FileBuilder AddFile(string source, string destinationDir)
		{
			FileBuilder fileBuilder = new FileBuilder(source, destinationDir);
			files.Add(fileBuilder);
			return fileBuilder;
		}

		public FileBuilder AddFile(string source)
		{
			return AddFile(source, "$(runtime.default)");
		}

		public override FileGroup ToPkgObject()
		{
			filterGroup.Files.Clear();
			files.ForEach(delegate(FileBuilder x)
			{
				filterGroup.Files.Add(x.ToPkgObject());
			});
			return base.ToPkgObject();
		}
	}
}
