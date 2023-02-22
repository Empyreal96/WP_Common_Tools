using System;
using Microsoft.CompPlat.PkgBldr.Base.Security;
using Microsoft.CompPlat.PkgBldr.Base.Tools;
using Microsoft.CompPlat.PkgBldr.Tools;

namespace Microsoft.CompPlat.PkgBldr.Base
{
	public class Config
	{
		public MacroResolver Macros;

		public GlobalSecurity GlobalSecurity;

		public BuildPass Pass;

		public object arg;

		public ExitStatus ExitStatus;

		public Build build;

		public Bld Bld;

		public PkgBldrCmd pkgBldrArgs;

		private string _input;

		private string _output;

		private bool _autoGenerateOutput;

		private bool _generateCab;

		private bool _proccessInf;

		public bool ProcessInf
		{
			get
			{
				return _proccessInf;
			}
			set
			{
				_proccessInf = value;
			}
		}

		public bool GenerateCab
		{
			get
			{
				return _generateCab;
			}
			set
			{
				_generateCab = value;
			}
		}

		public ConversionType Convert { get; set; }

		public string Input
		{
			get
			{
				return _input;
			}
			set
			{
				if (value != null)
				{
					_input = LongPath.GetFullPath(value.TrimEnd('\\'));
					if (!LongPathFile.Exists(_input))
					{
						throw new PkgGenException("Input file does not exist");
					}
				}
				else
				{
					_input = null;
				}
			}
		}

		public string Output
		{
			get
			{
				return _output;
			}
			set
			{
				if (value != null)
				{
					string fullPath = LongPath.GetFullPath(value.TrimEnd('\\'));
					if (fullPath.ToLowerInvariant().EndsWith(".man", StringComparison.OrdinalIgnoreCase) || fullPath.ToLowerInvariant().EndsWith(".xml", StringComparison.OrdinalIgnoreCase))
					{
						_autoGenerateOutput = false;
						_output = fullPath;
						fullPath = LongPath.GetDirectoryName(fullPath);
					}
					else
					{
						_autoGenerateOutput = true;
						_output = fullPath;
					}
				}
				else
				{
					_output = null;
					_autoGenerateOutput = false;
				}
			}
		}

		public bool AutoGenerateOutput => _autoGenerateOutput;

		public IDeploymentLogger Logger { get; set; }

		public bool Diagnostic { get; set; }
	}
}
