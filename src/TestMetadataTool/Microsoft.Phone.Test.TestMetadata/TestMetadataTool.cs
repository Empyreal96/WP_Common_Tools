using System;
using System.Reflection;
using Microsoft.Phone.Test.TestMetadata.CommandLine;

namespace Microsoft.Phone.Test.TestMetadata
{
	internal class TestMetadataTool
	{
		private static int Main(string[] args)
		{
			PrintHeader();
			Command command = null;
			CommandFactory commandFactory = null;
			try
			{
				commandFactory = new CommandFactory(Assembly.GetExecutingAssembly(), new StandardOptionParser());
				using (command = commandFactory.Create(args))
				{
					command.Run();
					return 0;
				}
			}
			catch (UsageException ex)
			{
				if (command != null)
				{
					ex.Command.Specification.PrintFullUsage(Console.Out);
				}
				else if (commandFactory != null)
				{
					Log.Error(ex.Message);
					using (command = commandFactory.Create(new string[1] { "help" }))
					{
						command.Run();
					}
				}
				else
				{
					Log.Error(ex.Message);
				}
			}
			catch (Exception ex2)
			{
				Log.Error(ex2.Message);
				if (ex2.InnerException != null)
				{
					Log.Error(ex2.InnerException.Message);
				}
				Log.Message("{0}", ex2);
			}
			return 2;
		}

		private static void PrintHeader()
		{
			Console.WriteLine("");
			ConsoleColor foregroundColor = Console.ForegroundColor;
			Console.ForegroundColor = ConsoleColor.Cyan;
			Console.WriteLine("Test Metadata Tool.");
			Console.ForegroundColor = foregroundColor;
			Console.WriteLine("");
		}
	}
}
