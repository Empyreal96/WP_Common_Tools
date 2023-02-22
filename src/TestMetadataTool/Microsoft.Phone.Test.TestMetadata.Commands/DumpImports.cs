using System;
using System.Globalization;
using System.IO;
using Microsoft.Phone.Test.TestMetadata.CommandLine;
using Microsoft.Phone.Test.TestMetadata.Helper;
using Microsoft.Tools.IO;

namespace Microsoft.Phone.Test.TestMetadata.Commands
{
	[Command("imports", BriefUsage = "testmetadatatool.exe imports -b <binary> -o <output file>", BriefDescription = "Dumps the imports for a given binary.", GeneralInformation = "This command dumps the imports for a given test binary.", AllowNoNameOptions = true)]
	[CommandAlias("imp")]
	internal class DumpImports : CommandBase
	{
		[Option("b", OptionValueType.ValueRequired, Description = "-{0} <binary name>: Full path to the binary.")]
		public string Binary { get; set; }

		[Option("o", OptionValueType.ValueOptional, Description = "-{0} <output file name>: output file path.")]
		public string Out { get; set; }

		protected override void RunImplementation()
		{
			ValidateArguments();
			TextWriter textWriter = null;
			TextWriter textWriter2 = null;
			try
			{
				if (Out != null)
				{
					textWriter = new StreamWriter(Out);
					textWriter2 = Console.Out;
					Console.SetOut(textWriter);
				}
				foreach (PortableExecutableDependency item in BinaryFile.GetDependency(Binary))
				{
					Console.WriteLine("{0} [{1}]", item.Name, item.Type);
				}
			}
			finally
			{
				if (textWriter2 != null)
				{
					Console.SetOut(textWriter2);
				}
				textWriter?.Dispose();
			}
		}

		private void ValidateArguments()
		{
			if (string.IsNullOrEmpty(Binary))
			{
				PrintError($"Binary (-b) not specified.\n");
			}
			if (!LongPathFile.Exists(Binary))
			{
				PrintError(string.Format(CultureInfo.InvariantCulture, "Binary {0} does not exist.", new object[1] { Binary }));
			}
		}
	}
}
