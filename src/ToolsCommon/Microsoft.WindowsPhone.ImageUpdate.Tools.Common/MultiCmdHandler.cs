using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Microsoft.WindowsPhone.ImageUpdate.Tools.Common
{
	public class MultiCmdHandler
	{
		private string appName = new FileInfo(Environment.GetCommandLineArgs()[0]).Name;

		private Dictionary<string, CmdHandler> _allHandlers = new Dictionary<string, CmdHandler>(StringComparer.OrdinalIgnoreCase);

		private void ShowUsage()
		{
			LogUtil.Message("Usage: {0} <command> <parameters>", appName);
			LogUtil.Message("\t available command:");
			foreach (KeyValuePair<string, CmdHandler> allHandler in _allHandlers)
			{
				LogUtil.Message("\t\t{0}:{1}", allHandler.Value.Command, allHandler.Value.Description);
			}
			LogUtil.Message("\t Run {0} <command> /? to check command line parameters for each command", appName);
		}

		public void AddCmdHandler(CmdHandler cmdHandler)
		{
			_allHandlers.Add(cmdHandler.Command, cmdHandler);
		}

		public int Run(string[] args)
		{
			if (args.Length < 1)
			{
				ShowUsage();
				return -1;
			}
			int result = -1;
			string cmdParams = ((args.Length > 1) ? string.Join(" ", args.Skip(1)) : string.Empty);
			CmdHandler value = null;
			if (!_allHandlers.TryGetValue(args[0], out value))
			{
				ShowUsage();
			}
			else
			{
				result = value.Execute(cmdParams, appName);
			}
			return result;
		}
	}
}
