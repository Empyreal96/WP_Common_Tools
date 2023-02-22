using System;
using System.IO;
using System.Text.RegularExpressions;

namespace Microsoft.MobileDevices.TestInfrastructure.UtilityLibrary.Facades
{
	public class FileNames
	{
		public static string QualifyCommandLine(string commandLine, string rootPath)
		{
			string arguments;
			string command;
			SplitCommandFromCommandLine(commandLine, out command, out arguments);
			command = Path.Combine(rootPath, command);
			return JoinCommandWithArguments(command, arguments);
		}

		public static void SplitCommandFromCommandLine(string commandLine, out string command, out string arguments)
		{
			commandLine = commandLine.TrimStart(' ');
			if (commandLine[0] != '"')
			{
				int num = commandLine.IndexOf(' ');
				if (num != -1)
				{
					command = commandLine.Substring(0, num);
					arguments = commandLine.Substring(num + 1);
				}
				else
				{
					command = commandLine;
					arguments = null;
				}
				return;
			}
			Regex regex = new Regex("^(?<command>(?:[^\"\\s]*\"[^\"]*\"[^\"\\s]*)+)(?:\\s(?<arguments>.*)|$)");
			Match match = regex.Match(commandLine);
			if (match.Success)
			{
				command = match.Groups["command"].Value;
				arguments = match.Groups["arguments"].Value;
				command = command.Replace("\"", "");
				return;
			}
			throw new ArgumentException("Command doesn't use a recognizable grammar.  Expected: \"quoted command\" arg1 ...");
		}

		public static string JoinCommandWithArguments(string command, string arguments)
		{
			string text = QuoteFileNameIfNeeded(command);
			if (arguments != null)
			{
				text = text + ' ' + arguments;
			}
			return text;
		}

		public static string QuoteFileNameIfNeeded(string fileName)
		{
			string result = fileName;
			if (Regex.Match(fileName, "^[^\"].*\\s.*[^\"]$").Success)
			{
				result = '"' + fileName + '"';
			}
			return result;
		}
	}
}
