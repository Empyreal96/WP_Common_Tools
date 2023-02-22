using System;
using System.Text.RegularExpressions;

namespace SecureWim
{
	public class SecureWimTool
	{
		public static int Main(string[] arguments)
		{
			return Parse(arguments).Run();
		}

		private static IToolCommand Parse(string[] arguments)
		{
			string[] commands = new string[4] { "?", "build", "extractcat", "replacecat" };
			UsagePrinter.UsageDelegate[] array = new UsagePrinter.UsageDelegate[4]
			{
				new UsageCommand(commands).PrintUsage,
				BuildCommand.PrintUsage,
				ExtractCommand.PrintUsage,
				ReplaceCommand.PrintUsage
			};
			Func<string[], IToolCommand>[] array2 = new Func<string[], IToolCommand>[4]
			{
				(string[] a) => new UsageCommand(commands),
				(string[] a) => new BuildCommand(a),
				(string[] a) => new ExtractCommand(a),
				(string[] a) => new ReplaceCommand(a)
			};
			string text = string.Empty;
			if (arguments.Length != 0)
			{
				text = arguments[0];
			}
			try
			{
				Regex regex = new Regex("^[/-]\\?$", RegexOptions.Compiled);
				for (int i = 0; i < commands.Length; i++)
				{
					string pattern = "^[/-]" + commands[i] + "$";
					if (Regex.IsMatch(text, pattern, RegexOptions.IgnoreCase) || regex.IsMatch(text))
					{
						if (arguments.Length == 2 && regex.IsMatch(arguments[1]))
						{
							return new UsagePrinter(array[i]);
						}
						return array2[i](arguments);
					}
				}
				throw new ArgParseException($"Unrecognized command: \"{text}\"", array[0]);
			}
			catch (ArgParseException ex)
			{
				return new UsagePrinter(ex.Message, ex.PrintUsage);
			}
		}
	}
}
