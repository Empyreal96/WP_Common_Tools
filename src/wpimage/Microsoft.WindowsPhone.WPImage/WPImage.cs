using System;
using System.Globalization;
using System.Linq;

namespace Microsoft.WindowsPhone.WPImage
{
	internal class WPImage
	{
		internal const string ModuleName = "WPImage.exe";

		private IWPImageCommand[] _commands = new IWPImageCommand[4]
		{
			new MountCommand(),
			new DismountCommand(),
			new RemoveIdCommand(),
			new DisplayIdCommand()
		};

		private IWPImageCommand _command;

		private static void Main(string[] args)
		{
			new WPImage().Run(args);
		}

		private void Run(string[] args)
		{
			ParseArgs(args);
			if (_command != null)
			{
				_command.Run();
			}
		}

		private void ParseArgs(string[] args)
		{
			if (args.Length < 1)
			{
				PrintUsage();
				return;
			}
			IWPImageCommand[] commands = _commands;
			foreach (IWPImageCommand iWPImageCommand in commands)
			{
				if (string.Compare(args[0], iWPImageCommand.Name, true, CultureInfo.InvariantCulture) == 0)
				{
					_command = iWPImageCommand;
					break;
				}
			}
			if (_command == null)
			{
				PrintUsage();
			}
			else if (!_command.ParseArgs(args.Skip(1).ToArray()))
			{
				_command.PrintUsage();
				_command = null;
			}
		}

		private void PrintUsage()
		{
			Console.WriteLine("WPImage.exe command [command_parameters]");
			Console.WriteLine("Commands:");
			IWPImageCommand[] commands = _commands;
			foreach (IWPImageCommand iWPImageCommand in commands)
			{
				Console.WriteLine("  {0}", iWPImageCommand.Name);
			}
		}

		public static void NullLog(string unused, object[] notused)
		{
		}
	}
}
