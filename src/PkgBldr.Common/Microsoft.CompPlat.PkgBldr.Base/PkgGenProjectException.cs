using System;
using System.Globalization;
using Microsoft.CompPlat.PkgBldr.Tools;

namespace Microsoft.CompPlat.PkgBldr.Base
{
	public class PkgGenProjectException : PkgGenException
	{
		public string ProjectPath { get; private set; }

		public int LineNumber { get; private set; }

		public int LinePosition { get; private set; }

		public bool HasLineInfo { get; private set; }

		public override string Message
		{
			get
			{
				if (HasLineInfo)
				{
					return string.Format(CultureInfo.InvariantCulture, "{0}({1},{2}): {3}", LongPath.GetFileName(ProjectPath), LineNumber, LinePosition, base.Message);
				}
				return string.Format(CultureInfo.InvariantCulture, "{0}: {1}", new object[2]
				{
					LongPath.GetFileName(ProjectPath),
					base.Message
				});
			}
		}

		public PkgGenProjectException(Exception innerException, string projectPath, string msg, params object[] args)
			: base(innerException, msg, args)
		{
			ProjectPath = projectPath;
			HasLineInfo = false;
			LineNumber = -1;
			LinePosition = -1;
		}

		public PkgGenProjectException(string projectPath, string msg, params object[] args)
			: this(null, projectPath, msg, args)
		{
		}

		public PkgGenProjectException(Exception innerException, string projectPath, int lineNumber, int linePosition, string msg, params object[] args)
			: base(innerException, msg, args)
		{
			ProjectPath = projectPath;
			HasLineInfo = true;
			LineNumber = lineNumber;
			LinePosition = linePosition;
		}

		public PkgGenProjectException(string projectPath, int lineNumber, int linePosition, string msg, params object[] args)
			: this(null, projectPath, lineNumber, linePosition, msg, args)
		{
		}
	}
}
