using System;
using System.Collections.Generic;
using System.Globalization;

namespace Microsoft.WindowsPhone.Security.SecurityPolicyCompiler
{
	public class AccessRightHelper
	{
		public enum AccessType : uint
		{
			DELETE = 65536u,
			READ_CONTROL = 131072u,
			WRITE_DAC = 262144u,
			WRITE_OWNER = 524288u,
			SYNCHRONIZE = 1048576u,
			STANDARD_RIGHTS_REQUIRED = 983040u,
			STANDARD_RIGHTS_READ = 131072u,
			STANDARD_RIGHTS_WRITE = 131072u,
			STANDARD_RIGHTS_EXECUTE = 131072u,
			STANDARD_RIGHTS_ALL = 2031616u,
			SPECIFIC_RIGHTS_ALL = 65535u,
			ACCESS_SYSTEM_SECURITY = 16777216u,
			GENERIC_READ = 2147483648u,
			GENERIC_WRITE = 1073741824u,
			GENERIC_EXECUTE = 536870912u,
			GENERIC_ALL = 268435456u
		}

		public enum SyncObjectsAccessType : uint
		{
			EVENT_MODIFY_STATE = 2u,
			EVENT_ALL_ACCESS = 2031619u,
			MUTANT_QUERY_STATE = 1u,
			MUTANT_ALL_ACCESS = 2031617u,
			SEMAPHORE_QUERY_STATE = 1u,
			SEMAPHORE_MODIFY_STATE = 2u,
			SEMAPHORE_ALL_ACCESS = 2031619u,
			TIMER_QUERY_STATE = 1u,
			TIMER_MODIFY_STATE = 2u,
			TIMER_ALL_ACCESS = 2031619u,
			WNF_STATE_SUBSCRIBE = 1u,
			WNF_STATE_PUBLISH = 2u,
			WNF_STATE_CROSS_SCOPE_ACCESS = 16u,
			PORT_CONNECT = 1u,
			PORT_ALL_ACCESS = 2031617u,
			JOB_OBJECT_ASSIGN_PROCESS = 1u,
			JOB_OBJECT_SET_ATTRIBUTES = 2u,
			JOB_OBJECT_QUERY = 4u,
			JOB_OBJECT_TERMINATE = 8u,
			JOB_OBJECT_SET_SECURITY_ATTRIBUTES = 16u,
			JOB_OBJECT_ALL_ACCESS = 2031647u,
			FILE_MAP_QUERY = 1u,
			FILE_MAP_WRITE = 2u,
			FILE_MAP_READ = 4u,
			FILE_MAP_EXECUTE = 32u,
			FILE_MAP_ALL_ACCESS = 983055u
		}

		public enum FileAccessType : uint
		{
			FILE_READ_DATA = 1u,
			FILE_WRITE_DATA = 2u,
			FILE_APPEND_DATA = 4u,
			FILE_READ_EA = 8u,
			FILE_WRITE_EA = 16u,
			FILE_EXECUTE = 32u,
			FILE_READ_ATTRIBUTES = 128u,
			FILE_WRITE_ATTRIBUTES = 256u,
			FILE_GENERIC_READ = 1179785u,
			FILE_GENERIC_WRITE = 1179926u,
			FILE_GENERIC_EXECUTE = 1179808u,
			FILE_ALL_ACCESS = 2032127u
		}

		public enum RegistryAccessType : uint
		{
			KEY_QUERY_VALUE = 1u,
			KEY_SET_VALUE = 2u,
			KEY_CREATE_SUB_KEY = 4u,
			KEY_ENUMERATE_SUB_KEYS = 8u,
			KEY_NOTIFY = 16u,
			KEY_CREATE_LINK = 32u,
			KEY_READ = 131097u,
			KEY_EXECUTE = 131097u,
			KEY_WRITE = 131078u,
			KEY_ALL_ACCESS = 983103u
		}

		public enum ServiceAccessType : uint
		{
			SERVICE_QUERY_CONFIG = 1u,
			SERVICE_CHANGE_CONFIG = 2u,
			SERVICE_QUERY_STATUS = 4u,
			SERVICE_ENUMERATE_DEPENDENTS = 8u,
			SERVICE_START = 16u,
			SERVICE_STOP = 32u,
			SERVICE_PAUSE_CONTINUE = 64u,
			SERVICE_INTERROGATE = 128u,
			SERVICE_USER_DEFINED_CONTROL = 256u,
			SERVICE_ALL_ACCESS = 983551u
		}

		public struct GenericMapping
		{
			public readonly uint GenericRead;

			public readonly uint GenericWrite;

			public readonly uint GenericExecute;

			public readonly uint GenericAll;

			public GenericMapping(uint GenericRead, uint GenericWrite, uint GenericExecute, uint GenericAll)
			{
				this.GenericRead = GenericRead;
				this.GenericWrite = GenericWrite;
				this.GenericExecute = GenericExecute;
				this.GenericAll = GenericAll;
			}
		}

		private static readonly Dictionary<string, GenericMapping> genericAccessTable = new Dictionary<string, GenericMapping>(StringComparer.OrdinalIgnoreCase)
		{
			{
				"Mutex",
				new GenericMapping(131073u, 131072u, 1179648u, 2031617u)
			},
			{
				"Semaphore",
				new GenericMapping(131073u, 131074u, 1179648u, 2031619u)
			},
			{
				"WaitableTimer",
				new GenericMapping(131073u, 131074u, 1179648u, 2031619u)
			},
			{
				"AlpcPort",
				new GenericMapping(131073u, 65537u, 0u, 2031617u)
			},
			{
				"Rpc",
				new GenericMapping(2147483648u, 1073741824u, 536870912u, 268435456u)
			},
			{
				"Private",
				new GenericMapping(2147483648u, 1073741824u, 536870912u, 268435456u)
			},
			{
				"Template",
				new GenericMapping(2147483648u, 1073741824u, 536870912u, 268435456u)
			},
			{
				"WNF",
				new GenericMapping(1179649u, 2u, 2031616u, 2031619u)
			},
			{
				"SDRegValue",
				new GenericMapping(2147483648u, 1073741824u, 536870912u, 268435456u)
			},
			{
				"File",
				new GenericMapping(1179785u, 1179926u, 1179808u, 2032127u)
			},
			{
				"Directory",
				new GenericMapping(1179785u, 1179926u, 1179808u, 2032127u)
			},
			{
				"RegKey",
				new GenericMapping(131097u, 131078u, 131097u, 983103u)
			},
			{
				"ServiceAccess",
				new GenericMapping(131213u, 131074u, 131440u, 983551u)
			}
		};

		private static readonly Dictionary<string, uint> accessRightsTable = new Dictionary<string, uint>(StringComparer.OrdinalIgnoreCase)
		{
			{ "CC", 1u },
			{ "DC", 2u },
			{ "LC", 4u },
			{ "SW", 8u },
			{ "RP", 16u },
			{ "WP", 32u },
			{ "DT", 64u },
			{ "LO", 128u },
			{ "CR", 256u },
			{ "SD", 65536u },
			{ "RC", 131072u },
			{ "WD", 262144u },
			{ "WO", 524288u },
			{ "GA", 268435456u },
			{ "GR", 2147483648u },
			{ "GW", 1073741824u },
			{ "GX", 536870912u },
			{ "FA", 2032127u },
			{ "FR", 1179785u },
			{ "FW", 1179926u },
			{ "FX", 1179808u },
			{ "KA", 983103u },
			{ "KR", 131097u },
			{ "KW", 131078u },
			{ "KX", 131097u }
		};

		public static GenericMapping? GetGenericMapping(string resourceType)
		{
			if (genericAccessTable.ContainsKey(resourceType))
			{
				return genericAccessTable[resourceType];
			}
			return null;
		}

		public static uint MergeAccessRight(string accessRightsString, string resourcePath, string resourceType)
		{
			uint num = 0u;
			int num2 = accessRightsString.IndexOf("0X", StringComparison.OrdinalIgnoreCase);
			if (num2 >= 0)
			{
				if (num2 > 0 || accessRightsString.Length > 10)
				{
					throw new PolicyCompilerInternalException(string.Format(GlobalVariables.Culture, "The rule on resource '{0}' has the access rights '{1}' which has mixed Hex format and string format of access right.", new object[2] { resourcePath, accessRightsString }));
				}
				try
				{
					num = uint.Parse(accessRightsString.Substring(2), NumberStyles.HexNumber, GlobalVariables.Culture);
				}
				catch (FormatException originalException)
				{
					throw new PolicyCompilerInternalException(string.Format(GlobalVariables.Culture, "The rule on resource '{0}' has the access rights '{1}' which can't be converted to a integer.", new object[2] { resourcePath, accessRightsString }), originalException);
				}
			}
			else
			{
				for (int i = 0; i < accessRightsString.Length; i += 2)
				{
					string text = accessRightsString.Substring(i, 2);
					if (accessRightsTable.ContainsKey(text))
					{
						num |= accessRightsTable[text];
						continue;
					}
					throw new PolicyCompilerInternalException(string.Format(GlobalVariables.Culture, "The rule on resource '{0}' has the access rights '{1}' which has a invalid access right '{2}'.", new object[3] { resourcePath, accessRightsString, text }));
				}
			}
			GenericMapping? genericMapping = GetGenericMapping(resourceType);
			if (genericMapping.HasValue)
			{
				if ((num & 0x80000000u) != 0)
				{
					num |= genericMapping.Value.GenericRead;
				}
				if ((num & 0x40000000u) != 0)
				{
					num |= genericMapping.Value.GenericWrite;
				}
				if ((num & 0x20000000u) != 0)
				{
					num |= genericMapping.Value.GenericExecute;
				}
				if ((num & 0x10000000u) != 0)
				{
					num |= genericMapping.Value.GenericAll;
				}
			}
			return num;
		}
	}
}
