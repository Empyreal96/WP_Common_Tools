using System;

namespace SecureWim
{
	internal class UsageCommand : IToolCommand
	{
		private string[] supportedCommands;

		public UsageCommand(string[] commands)
		{
			supportedCommands = commands;
		}

		public int Run()
		{
			PrintUsage();
			return 0;
		}

		public void PrintUsage()
		{
			Console.WriteLine("Usage:");
			Console.WriteLine("secwimtool <command> <arguments>\n");
			Console.WriteLine("Commands:");
			string[] array = supportedCommands;
			foreach (string text in array)
			{
				Console.WriteLine("-".PadLeft(4) + text);
			}
			Console.WriteLine();
			Console.WriteLine("run secwimtool <command> -? for per-command help.");
		}
	}
}
