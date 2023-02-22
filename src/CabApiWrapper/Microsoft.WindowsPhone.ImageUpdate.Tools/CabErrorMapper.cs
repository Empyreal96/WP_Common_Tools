using System;
using System.Collections.Generic;
using System.Reflection;

namespace Microsoft.WindowsPhone.ImageUpdate.Tools
{
	public class CabErrorMapper
	{
		private static readonly Lazy<CabErrorMapper> _instance = new Lazy<CabErrorMapper>(() => new CabErrorMapper());

		private const uint CABAPI_ERR_BASE = 2149089984u;

		private const uint CABAPI_ERR_FCI_BASE = 2149090000u;

		private const uint CABAPI_ERR_FDI_BASE = 2149090016u;

		public const uint E_CABAPI_NOT_CABINET = 2149089985u;

		public const string STR_E_CABAPI_NOT_CABINET = "Specified file is not a valid cabinet.";

		public const uint E_CABAPI_UNKNOWN_FILE = 2149089986u;

		public const string STR_E_CABAPI_UNKNOWN_FILE = "CAB extraction failed: One or more files in extraction list not found in cabinet.";

		public const uint E_CABAPI_FCI_OPEN_SRC = 2149090001u;

		public const string STR_E_CABAPI_FCI_OPEN_SRC = "CAB creation failed: Could not open source file.";

		public const uint E_CABAPI_FCI_READ_SRC = 2149090002u;

		public const string STR_E_CABAPI_FCI_READ_SRC = "CAB creation failed: Could not read source file.";

		public const uint E_CABAPI_FCI_ALLOC_FAIL = 2149090003u;

		public const string STR_E_CABAPI_FCI_ALLOC_FAIL = "CAB creation failed: FCI failed to allocate memory.";

		public const uint E_CABAPI_FCI_TEMP_FILE = 2149090004u;

		public const string STR_E_CABAPI_FCI_TEMP_FILE = "CAB creation failed: FCI failed to create temporary file.";

		public const uint E_CABAPI_FCI_BAD_COMPR_TYPE = 2149090005u;

		public const string STR_E_CABAPI_FCI_BAD_COMPR_TYPE = "CAB creation failed: Unknown compression type.";

		public const uint E_CABAPI_FCI_CAB_FILE = 2149090006u;

		public const string STR_E_CABAPI_FCI_CAB_FILE = "CAB creation failed: FCI failed to create cabinet file.";

		public const uint E_CABAPI_FCI_USER_ABORT = 2149090007u;

		public const string STR_E_CABAPI_FCI_USER_ABORT = "CAB creation failed: FCI aborted on user request.";

		public const uint E_CABAPI_FCI_MCI_FAIL = 2149090008u;

		public const string STR_E_CABAPI_FCI_MCI_FAIL = "CAB creation failed: FCI failed to compress data.";

		public const uint E_CABAPI_FCI_CAB_FORMAT_LIMIT = 2149090009u;

		public const string STR_E_CABAPI_FCI_CAB_FORMAT_LIMIT = "CAB creation failed: Data-size or file-count exceeded CAB format limits.";

		public const uint E_CABAPI_FCI_UNKNOWN = 2149090015u;

		public const string STR_E_CABAPI_FCI_UNKNOWN = "CAB creation failed: Unknown error.";

		public const uint E_CABAPI_FDI_CABINET_NOT_FOUND = 2149090017u;

		public const string STR_E_CABAPI_FDI_CABINET_NOT_FOUND = "CAB extract failed: Specified cabinet file not found.";

		public const uint E_CABAPI_FDI_NOT_A_CABINET = 2149090018u;

		public const string STR_E_CABAPI_FDI_NOT_A_CABINET = "CAB extract failed: Specified file is not a valid cabinet.";

		public const uint E_CABAPI_FDI_UNKNOWN_CABINET_VERSION = 2149090019u;

		public const string STR_E_CABAPI_FDI_UNKNOWN_CABINET_VERSION = "CAB extract failed: Specified cabinet has an unknown cabinet version number.";

		public const uint E_CABAPI_FDI_CORRUPT_CABINET = 2149090020u;

		public const string STR_E_CABAPI_FDI_CORRUPT_CABINET = "CAB extract failed: Specified cabinet is corrupt.";

		public const uint E_CABAPI_FDI_ALLOC_FAIL = 2149090021u;

		public const string STR_E_CABAPI_FDI_ALLOC_FAIL = "CAB extract failed: FDI failed to allocate memory.";

		public const uint E_CABAPI_FDI_BAD_COMPR_TYPE = 2149090022u;

		public const string STR_E_CABAPI_FDI_BAD_COMPR_TYPE = "CAB extract failed: Unknown compression type used in cabinet folder.";

		public const uint E_CABAPI_FDI_MDI_FAIL = 2149090023u;

		public const string STR_E_CABAPI_FDI_MDI_FAIL = "CAB extract failed: FDI failed to decompress data from cabinet file.";

		public const uint E_CABAPI_FDI_TARGET_FILE = 2149090024u;

		public const string STR_E_CABAPI_FDI_TARGET_FILE = "CAB extract failed: Failure writing to target file.";

		public const uint E_CABAPI_FDI_RESERVE_MISMATCH = 2149090025u;

		public const string STR_E_CABAPI_FDI_RESERVE_MISMATCH = "CAB extract failed: The cabinets within a set do not have the same RESERVE sizes.";

		public const uint E_CABAPI_FDI_WRONG_CABINET = 2149090026u;

		public const string STR_E_CABAPI_FDI_WRONG_CABINET = "CAB extract failed: The cabinet returned by fdintNEXT_CABINET is incorrect.";

		public const uint E_CABAPI_FDI_USER_ABORT = 2149090027u;

		public const string STR_E_CABAPI_FDI_USER_ABORT = "CAB extract failed: FDI aborted on user request.";

		public const uint E_CABAPI_FDI_UNKNOWN = 2149090031u;

		public const string STR_E_CABAPI_FDI_UNKNOWN = "CAB extract failed: Unknown error.";

		public const string STR_UNKNOWN_ERROR = "CAB operation failed: Unknown error.";

		private Dictionary<uint, string> _map = new Dictionary<uint, string>();

		private const string STR_CABERROR_PREFIX = "E_CABAPI";

		private const string STR_PREFIX = "STR_";

		private const string STR_CABERRORMSG_PREFIX = "STR_E_CABAPI";

		public static CabErrorMapper Instance => _instance.Value;

		private CabErrorMapper()
		{
			Dictionary<string, uint> dictionary = new Dictionary<string, uint>();
			Dictionary<string, string> dictionary2 = new Dictionary<string, string>();
			FieldInfo[] fields = GetType().GetFields(BindingFlags.Static | BindingFlags.Public);
			foreach (FieldInfo fieldInfo in fields)
			{
				if (fieldInfo.Name.StartsWith("E_CABAPI", StringComparison.OrdinalIgnoreCase))
				{
					dictionary[fieldInfo.Name] = (uint)fieldInfo.GetValue(this);
				}
				else if (fieldInfo.Name.StartsWith("STR_E_CABAPI", StringComparison.OrdinalIgnoreCase))
				{
					dictionary2[fieldInfo.Name] = fieldInfo.GetValue(this) as string;
				}
			}
			foreach (string key2 in dictionary.Keys)
			{
				string key = "STR_" + key2;
				if (dictionary2.ContainsKey(key))
				{
					_map[dictionary[key2]] = dictionary2[key];
				}
			}
		}

		public string MapError(uint hr)
		{
			string empty = string.Empty;
			if (_map != null && _map.ContainsKey(hr))
			{
				return _map[hr];
			}
			return string.Format("CAB operation failed: Unknown error.", hr);
		}
	}
}
