using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using Microsoft.WindowsPhone.ImageUpdate.PkgGenCommon.Interfaces;
using Microsoft.WindowsPhone.ImageUpdate.Tools.Common;

namespace Microsoft.WindowsPhone.ImageUpdate.PkgGenCommon.BaseComponents.Internal
{
	public sealed class FailureActionsPkgObject
	{
		private const int SIZE_OF_SERVICE_FAILURE_ACTIONS_WOW64 = 20;

		[XmlAttribute("ResetPeriod")]
		public string ResetPeriod = "INFINITE";

		[XmlAttribute("RebootMessage")]
		public string RebootMsg;

		[XmlAttribute("Command")]
		public string Command;

		[XmlElement("Action")]
		public List<FailureAction> Actions;

		public FailureActionsPkgObject()
		{
			Actions = new List<FailureAction>();
		}

		public void Build(IPackageGenerator pkgGen)
		{
			int result = -1;
			if (ResetPeriod != null && !ResetPeriod.Equals("INFINITE", StringComparison.InvariantCulture))
			{
				if (!int.TryParse(ResetPeriod, out result))
				{
					throw new PkgGenException("Invalid ResetPeriod value '{0}' in Service object", ResetPeriod);
				}
			}
			else
			{
				result = -1;
			}
			if (Command != null)
			{
				pkgGen.AddRegValue("$(hklm.service)", "FailureCommand", RegValueType.String, Command);
			}
			if (RebootMsg != null)
			{
				pkgGen.AddRegValue("$(hklm.service)", "RebootMessage", RegValueType.String, RebootMsg);
			}
			List<byte> list = new List<byte>();
			if (Actions == null)
			{
				return;
			}
			list.AddRange(BitConverter.GetBytes(result));
			int value = ((RebootMsg != null) ? 1 : 0);
			list.AddRange(BitConverter.GetBytes(value));
			int value2 = ((Command != null) ? 1 : 0);
			list.AddRange(BitConverter.GetBytes(value2));
			list.AddRange(BitConverter.GetBytes(Actions.Count));
			list.AddRange(BitConverter.GetBytes(20));
			foreach (FailureAction action in Actions)
			{
				list.AddRange(BitConverter.GetBytes((int)action.Type));
				list.AddRange(BitConverter.GetBytes((int)action.Delay));
			}
			pkgGen.AddRegValue("$(hklm.service)", "FailureActions", RegValueType.Binary, BitConverter.ToString(list.ToArray()).Replace('-', ','));
		}
	}
}
