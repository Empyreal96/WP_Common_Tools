using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using Microsoft.WindowsPhone.ImageUpdate.Tools.Common;

namespace Microsoft.WindowsPhone.Imaging
{
	public class BcdElementObjectList : BcdElement
	{
		public List<Guid> ObjectList
		{
			get
			{
				List<Guid> list = new List<Guid>(base.MultiStringData.Count);
				for (int i = 0; i < base.MultiStringData.Count; i++)
				{
					Guid result = Guid.Empty;
					if (!Guid.TryParse(base.MultiStringData[i], out result))
					{
						throw new ImageStorageException($"{MethodBase.GetCurrentMethod().Name}: The string data isn't a valid Guid.");
					}
					list.Add(result);
				}
				return list;
			}
		}

		public BcdElementObjectList(string[] multiStringData, BcdElementDataType dataType)
			: base(dataType)
		{
			_multiStringData = new List<string>(multiStringData);
		}

		[CLSCompliant(false)]
		public override void LogInfo(IULogger logger, int indentLevel)
		{
			string text = new StringBuilder().Append(' ', indentLevel).ToString();
			base.LogInfo(logger, indentLevel);
			foreach (Guid @object in ObjectList)
			{
				logger.LogInfo(text + "Object ID: {{{0}}}", @object);
			}
		}
	}
}
