using System;
using System.Collections;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;

namespace Microsoft.MobileDevices.TestInfrastructure.UtilityLibrary.CommandLine
{
	public sealed class Parser
	{
		private class HelpArgument
		{
			[Argument(ArgumentType.AtMostOnce, ShortName = "?")]
			private bool help = false;

			[Argument(ArgumentType.AtMostOnce, ShortName = "h")]
			private bool help2 = false;

			public bool Help => help || help2;
		}

		private struct COORD
		{
			internal short x;

			internal short y;
		}

		private struct SMALL_RECT
		{
			internal short Left;

			internal short Top;

			internal short Right;

			internal short Bottom;
		}

		private struct CONSOLE_SCREEN_BUFFER_INFO
		{
			internal COORD dwSize;

			internal COORD dwCursorPosition;

			internal short wAttributes;

			internal SMALL_RECT srWindow;

			internal COORD dwMaximumWindowSize;
		}

		private struct ArgumentHelpStrings
		{
			public string syntax;

			public string help;

			public ArgumentHelpStrings(string syntax, string help)
			{
				this.syntax = syntax;
				this.help = help;
			}
		}

		private class Argument
		{
			private string longName;

			private string shortName;

			private string helpText;

			private bool isHidden;

			private bool hasHelpText;

			private bool explicitShortName;

			private object defaultValue;

			private bool seenValue;

			private FieldInfo field;

			private Type elementType;

			private ArgumentType flags;

			private ArrayList collectionValues;

			private ErrorReporter reporter;

			private bool isDefault;

			public Type ValueType => IsCollection ? elementType : Type;

			public string LongName => longName;

			public bool ExplicitShortName => explicitShortName;

			public string ShortName => shortName;

			public bool HasShortName => shortName != null;

			public bool IsHidden => isHidden;

			public bool HasHelpText => hasHelpText;

			public string HelpText => helpText;

			public object DefaultValue => defaultValue;

			public bool HasDefaultValue => null != defaultValue;

			public string FullHelpText
			{
				get
				{
					StringBuilder stringBuilder = new StringBuilder();
					if (HasHelpText)
					{
						stringBuilder.Append(HelpText);
					}
					if (HasDefaultValue)
					{
						if (stringBuilder.Length > 0)
						{
							stringBuilder.Append(" ");
						}
						stringBuilder.Append("Default value:'");
						AppendValue(stringBuilder, DefaultValue);
						stringBuilder.Append('\'');
					}
					if (HasShortName)
					{
						if (stringBuilder.Length > 0)
						{
							stringBuilder.Append(" ");
						}
						stringBuilder.Append("(short form /");
						stringBuilder.Append(ShortName);
						stringBuilder.Append(")");
					}
					return stringBuilder.ToString();
				}
			}

			public string SyntaxHelp
			{
				get
				{
					StringBuilder stringBuilder = new StringBuilder();
					if (IsDefault)
					{
						stringBuilder.Append("<");
						stringBuilder.Append(LongName);
						stringBuilder.Append(">");
					}
					else
					{
						stringBuilder.Append("/");
						stringBuilder.Append(LongName);
						Type valueType = ValueType;
						if (valueType == typeof(int))
						{
							stringBuilder.Append(":<int>");
						}
						else if (valueType == typeof(uint))
						{
							stringBuilder.Append(":<uint>");
						}
						else if (valueType == typeof(bool))
						{
							stringBuilder.Append("[+|-]");
						}
						else if (valueType == typeof(string))
						{
							stringBuilder.Append(":<string>");
						}
						else if (valueType == typeof(double))
						{
							stringBuilder.Append(":<double>");
						}
						else
						{
							stringBuilder.Append(":{");
							bool flag = true;
							FieldInfo[] fields = valueType.GetFields();
							foreach (FieldInfo fieldInfo in fields)
							{
								if (fieldInfo.IsStatic)
								{
									if (flag)
									{
										flag = false;
									}
									else
									{
										stringBuilder.Append('|');
									}
									stringBuilder.Append(fieldInfo.Name);
								}
							}
							stringBuilder.Append('}');
						}
					}
					return stringBuilder.ToString();
				}
			}

			public bool IsRequired => 0 != (flags & ArgumentType.Required);

			public bool SeenValue => seenValue;

			public bool AllowMultiple => 0 != (flags & ArgumentType.Multiple);

			public bool Unique => 0 != (flags & ArgumentType.Unique);

			public Type Type => field.FieldType;

			public bool IsCollection => IsCollectionType(Type);

			public bool IsDefault => isDefault;

			public Argument(ArgumentAttribute attribute, FieldInfo field, ErrorReporter reporter)
			{
				longName = LongName(attribute, field);
				explicitShortName = ExplicitShortName(attribute);
				shortName = ShortName(attribute, field);
				hasHelpText = HasHelpText(attribute);
				helpText = HelpText(attribute, field);
				defaultValue = DefaultValue(attribute, field);
				isHidden = IsHidden(attribute);
				elementType = ElementType(field);
				flags = Flags(attribute, field);
				this.field = field;
				seenValue = false;
				this.reporter = reporter;
				isDefault = attribute != null && attribute is DefaultArgumentAttribute;
				if (IsCollection)
				{
					collectionValues = new ArrayList();
				}
			}

			public bool Finish(object destination)
			{
				if (!SeenValue && HasDefaultValue)
				{
					field.SetValue(destination, DefaultValue);
				}
				if (IsCollection)
				{
					field.SetValue(destination, collectionValues.ToArray(elementType));
				}
				return ReportMissingRequiredArgument();
			}

			private bool ReportMissingRequiredArgument()
			{
				if (IsRequired && !SeenValue)
				{
					if (IsDefault)
					{
						reporter($"Missing required argument '<{LongName}>'.");
					}
					else
					{
						reporter($"Missing required argument '/{LongName}'.");
					}
					return true;
				}
				return false;
			}

			private void ReportDuplicateArgumentValue(string value)
			{
				reporter($"Duplicate '{LongName}' argument '{value}'");
			}

			public bool SetValue(string value, object destination)
			{
				if (SeenValue && !AllowMultiple)
				{
					reporter($"Duplicate '{LongName}' argument");
					return false;
				}
				seenValue = true;
				object value2;
				if (!ParseValue(ValueType, value, out value2))
				{
					return false;
				}
				if (IsCollection)
				{
					if (Unique && collectionValues.Contains(value2))
					{
						ReportDuplicateArgumentValue(value);
						return false;
					}
					collectionValues.Add(value2);
				}
				else
				{
					field.SetValue(destination, value2);
				}
				return true;
			}

			private void ReportBadArgumentValue(string value)
			{
				reporter($"'{value}' is not a valid value for the '{LongName}' command line option");
			}

			private bool ParseValue(Type type, string stringData, out object value)
			{
				if ((stringData != null || type == typeof(bool)) && (stringData == null || stringData.Length > 0))
				{
					try
					{
						if (type == typeof(string))
						{
							value = stringData;
							return true;
						}
						if (!(type == typeof(bool)))
						{
							if (type == typeof(int))
							{
								value = int.Parse(stringData);
								return true;
							}
							if (type == typeof(uint))
							{
								value = int.Parse(stringData);
								return true;
							}
							if (type == typeof(double))
							{
								value = double.Parse(stringData);
								return true;
							}
							value = Enum.Parse(type, stringData, true);
							return true;
						}
						if (stringData == null || stringData == "+")
						{
							value = true;
							return true;
						}
						if (stringData == "-")
						{
							value = false;
							return true;
						}
					}
					catch
					{
					}
				}
				ReportBadArgumentValue(stringData);
				value = null;
				return false;
			}

			private void AppendValue(StringBuilder builder, object value)
			{
				if (value is string || value is int || value is uint || value.GetType().IsEnum)
				{
					builder.Append(value.ToString());
					return;
				}
				if (value is bool)
				{
					builder.Append(((bool)value) ? "+" : "-");
					return;
				}
				bool flag = true;
				foreach (object item in (Array)value)
				{
					if (!flag)
					{
						builder.Append(", ");
					}
					AppendValue(builder, item);
					flag = false;
				}
			}

			public void ClearShortName()
			{
				shortName = null;
			}
		}

		public const string NewLine = "\r\n";

		private const int STD_OUTPUT_HANDLE = -11;

		private const int spaceBeforeParam = 2;

		private const int minParameterSpecsForResponseFile = 2;

		private ArrayList arguments;

		private Hashtable argumentMap;

		private ArrayList defaultArguments;

		private int usedDefaults;

		private ErrorReporter reporter;

		public bool HasDefaultArgument => defaultArguments.Count != 0;

		private Parser()
		{
		}

		public static bool ParseArgumentsWithUsage(string[] arguments, object destination)
		{
			if (ParseHelp(arguments) || !ParseArguments(arguments, destination))
			{
				PrintUsage(destination);
				return false;
			}
			return true;
		}

		public static void PrintUsage(object destination)
		{
			PrintUsage(destination, Console.Out.WriteLine);
		}

		public static void PrintUsage(object destination, ErrorReporter reporter)
		{
			IArgumentHolder argumentHolder = destination as IArgumentHolder;
			if (argumentHolder != null)
			{
				reporter(argumentHolder.GetUsageString());
			}
			reporter(ArgumentsUsage(destination.GetType()));
		}

		public static bool ParseArguments(string[] arguments, object destination)
		{
			return ParseArguments(arguments, destination, Console.Error.WriteLine);
		}

		public static bool ParseArguments(string[] arguments, object destination, ErrorReporter reporter)
		{
			Parser parser = new Parser(destination.GetType(), reporter);
			return parser.Parse(arguments, destination);
		}

		private static void NullErrorReporter(string message)
		{
		}

		public static bool ParseHelp(string[] args)
		{
			Parser parser = new Parser(typeof(HelpArgument), NullErrorReporter);
			HelpArgument helpArgument = new HelpArgument();
			parser.Parse(args, helpArgument);
			return helpArgument.Help;
		}

		public static string ArgumentsUsage(Type argumentType)
		{
			int num = GetConsoleWindowWidth();
			if (num == 0)
			{
				num = 80;
			}
			return ArgumentsUsage(argumentType, num);
		}

		public static string ArgumentsUsage(Type argumentType, int columns)
		{
			return new Parser(argumentType, null).GetUsageString(columns);
		}

		[DllImport("kernel32.dll", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Auto, SetLastError = true)]
		private static extern int GetStdHandle(int nStdHandle);

		[DllImport("kernel32.dll", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Auto, SetLastError = true)]
		private static extern int GetConsoleScreenBufferInfo(int hConsoleOutput, ref CONSOLE_SCREEN_BUFFER_INFO lpConsoleScreenBufferInfo);

		public static int GetConsoleWindowWidth()
		{
			CONSOLE_SCREEN_BUFFER_INFO lpConsoleScreenBufferInfo = default(CONSOLE_SCREEN_BUFFER_INFO);
			int consoleScreenBufferInfo = GetConsoleScreenBufferInfo(GetStdHandle(-11), ref lpConsoleScreenBufferInfo);
			return lpConsoleScreenBufferInfo.dwSize.x;
		}

		public static int IndexOf(StringBuilder text, char value, int startIndex)
		{
			for (int i = startIndex; i < text.Length; i++)
			{
				if (text[i] == value)
				{
					return i;
				}
			}
			return -1;
		}

		public static int LastIndexOf(StringBuilder text, char value, int startIndex)
		{
			for (int num = Math.Min(startIndex, text.Length - 1); num >= 0; num--)
			{
				if (text[num] == value)
				{
					return num;
				}
			}
			return -1;
		}

		public Parser(Type argumentSpecification, ErrorReporter reporter)
		{
			this.reporter = reporter;
			arguments = new ArrayList();
			defaultArguments = new ArrayList();
			argumentMap = new Hashtable();
			FieldInfo[] fields = argumentSpecification.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
			foreach (FieldInfo fieldInfo in fields)
			{
				if (fieldInfo.IsStatic || fieldInfo.IsInitOnly || fieldInfo.IsLiteral)
				{
					continue;
				}
				ArgumentAttribute attribute = GetAttribute(fieldInfo);
				Argument value = new Argument(attribute, fieldInfo, reporter);
				if (attribute is DefaultArgumentAttribute)
				{
					if (defaultArguments.Count != 0)
					{
					}
					defaultArguments.Add(value);
				}
				else
				{
					arguments.Add(value);
				}
			}
			foreach (Argument argument2 in arguments)
			{
				argumentMap[argument2.LongName] = argument2;
				if (argument2.ExplicitShortName)
				{
					if (argument2.ShortName != null && argument2.ShortName.Length > 0)
					{
						argumentMap[argument2.ShortName] = argument2;
					}
					else
					{
						argument2.ClearShortName();
					}
				}
			}
			foreach (Argument argument3 in arguments)
			{
				if (!argument3.ExplicitShortName)
				{
					if (argument3.ShortName != null && argument3.ShortName.Length > 0 && !argumentMap.ContainsKey(argument3.ShortName))
					{
						argumentMap[argument3.ShortName] = argument3;
					}
					else
					{
						argument3.ClearShortName();
					}
				}
			}
		}

		private static ArgumentAttribute GetAttribute(FieldInfo field)
		{
			object[] customAttributes = field.GetCustomAttributes(typeof(ArgumentAttribute), false);
			if (customAttributes.Length == 1)
			{
				return (ArgumentAttribute)customAttributes[0];
			}
			return null;
		}

		private void ReportUnrecognizedArgument(string argument)
		{
			reporter($"Unrecognized command line argument '{argument}'");
		}

		private bool ParseArgumentList(string[] args, object destination)
		{
			bool flag = false;
			if (args != null)
			{
				foreach (string text in args)
				{
					if (text.Length <= 0)
					{
						continue;
					}
					switch (text[0])
					{
					case '-':
					case '/':
					{
						int num = text.IndexOfAny(new char[3] { ':', '+', '-' }, 1);
						string text2 = text.Substring(1, (num == -1) ? (text.Length - 1) : (num - 1));
						string value = ((text2.Length + 1 == text.Length) ? null : ((text.Length <= 1 + text2.Length || text[1 + text2.Length] != ':') ? text.Substring(text2.Length + 1) : text.Substring(text2.Length + 2)));
						Argument argument = (Argument)argumentMap[text2];
						if (argument == null)
						{
							ReportUnrecognizedArgument(text);
							flag = true;
						}
						else
						{
							flag |= !argument.SetValue(value, destination);
						}
						continue;
					}
					case '@':
					{
						string[] args2;
						flag |= LexFileArguments(text.Substring(1), out args2);
						flag |= ParseArgumentList(args2, destination);
						continue;
					}
					}
					if (defaultArguments.Count != 0)
					{
						flag |= !((Argument)defaultArguments[usedDefaults]).SetValue(text, destination);
						if (usedDefaults + 1 < defaultArguments.Count)
						{
							usedDefaults++;
						}
					}
					else
					{
						ReportUnrecognizedArgument(text);
						flag = true;
					}
				}
			}
			return flag;
		}

		public bool Parse(string[] args, object destination)
		{
			bool flag = ParseArgumentList(args, destination);
			foreach (Argument argument2 in arguments)
			{
				flag |= argument2.Finish(destination);
			}
			foreach (Argument defaultArgument in defaultArguments)
			{
				flag |= defaultArgument.Finish(destination);
			}
			return !flag;
		}

		public string GetUsageString(int screenWidth)
		{
			ArgumentHelpStrings[] allHelpStrings = GetAllHelpStrings();
			int num = 0;
			ArgumentHelpStrings[] array = allHelpStrings;
			for (int i = 0; i < array.Length; i++)
			{
				ArgumentHelpStrings argumentHelpStrings = array[i];
				num = Math.Max(num, argumentHelpStrings.syntax.Length);
			}
			int num2 = 4 + num + 2;
			screenWidth = Math.Max(screenWidth, 19);
			int num3 = ((screenWidth >= num2 + 10) ? num2 : 9);
			StringBuilder stringBuilder = new StringBuilder();
			array = allHelpStrings;
			for (int i = 0; i < array.Length; i++)
			{
				ArgumentHelpStrings argumentHelpStrings2 = array[i];
				stringBuilder.Append(' ', 4);
				int num4 = 4;
				int length = argumentHelpStrings2.syntax.Length;
				stringBuilder.Append(argumentHelpStrings2.syntax);
				num4 += length;
				if (num4 >= num3)
				{
					stringBuilder.Append("\n");
					num4 = 4;
				}
				int num5 = screenWidth - num3;
				int j = 0;
				while (j < argumentHelpStrings2.help.Length)
				{
					stringBuilder.Append(' ', num3 - num4);
					num4 = num3;
					int num6 = j + num5;
					if (num6 >= argumentHelpStrings2.help.Length)
					{
						num6 = argumentHelpStrings2.help.Length;
					}
					else
					{
						num6 = argumentHelpStrings2.help.LastIndexOf(' ', num6 - 1, Math.Min(num6 - j, num5));
						if (num6 <= j)
						{
							num6 = j + num5;
						}
					}
					stringBuilder.Append(argumentHelpStrings2.help, j, num6 - j);
					j = num6;
					AddNewLine("\n", stringBuilder, ref num4);
					for (; j < argumentHelpStrings2.help.Length && argumentHelpStrings2.help[j] == ' '; j++)
					{
					}
				}
				if (argumentHelpStrings2.help.Length == 0)
				{
					stringBuilder.Append("\n");
				}
			}
			return stringBuilder.ToString();
		}

		private static void AddNewLine(string newLine, StringBuilder builder, ref int currentColumn)
		{
			builder.Append(newLine);
			currentColumn = 0;
		}

		private ArgumentHelpStrings[] GetAllHelpStrings()
		{
			int num = NumberOfParametersToDisplay();
			ArgumentHelpStrings[] array = new ArgumentHelpStrings[num];
			int num2 = 0;
			foreach (Argument argument2 in arguments)
			{
				if (!argument2.IsHidden)
				{
					ref ArgumentHelpStrings reference = ref array[num2];
					reference = GetHelpStrings(argument2);
					num2++;
				}
			}
			foreach (Argument defaultArgument in defaultArguments)
			{
				ref ArgumentHelpStrings reference2 = ref array[num2];
				reference2 = GetHelpStrings(defaultArgument);
				num2++;
			}
			if (num > 2)
			{
				ref ArgumentHelpStrings reference3 = ref array[num2++];
				reference3 = new ArgumentHelpStrings("@<file>", "Read additional options from response file");
			}
			return array;
		}

		private static ArgumentHelpStrings GetHelpStrings(Argument arg)
		{
			return new ArgumentHelpStrings(arg.SyntaxHelp, arg.FullHelpText);
		}

		private int NumberOfParametersToDisplay()
		{
			int num = arguments.Count + defaultArguments.Count;
			if (num > 2)
			{
				num++;
			}
			return num;
		}

		private bool LexFileArguments(string fileName, out string[] arguments)
		{
			string text = null;
			try
			{
				using (FileStream stream = new FileStream(fileName, FileMode.Open, FileAccess.Read))
				{
					text = new StreamReader(stream).ReadToEnd();
				}
			}
			catch (Exception ex)
			{
				reporter($"Error: Can't open command line argument file '{fileName}' : '{ex.Message}'");
				arguments = null;
				return false;
			}
			bool result = false;
			ArrayList arrayList = new ArrayList();
			StringBuilder stringBuilder = new StringBuilder();
			bool flag = false;
			int i = 0;
			try
			{
				while (true)
				{
					bool flag2 = true;
					for (; char.IsWhiteSpace(text[i]); i++)
					{
					}
					if (text[i] == '#')
					{
						for (i++; text[i] != '\n'; i++)
						{
						}
						continue;
					}
					do
					{
						if (text[i] == '\\')
						{
							int num = 1;
							i++;
							while (i == text.Length && text[i] == '\\')
							{
								num++;
							}
							if (i == text.Length || text[i] != '"')
							{
								stringBuilder.Append('\\', num);
								continue;
							}
							stringBuilder.Append('\\', num >> 1);
							if (0 != (num & 1))
							{
								stringBuilder.Append('"');
							}
							else
							{
								flag = !flag;
							}
						}
						else if (text[i] == '"')
						{
							flag = !flag;
							i++;
						}
						else
						{
							stringBuilder.Append(text[i]);
							i++;
						}
					}
					while (!char.IsWhiteSpace(text[i]) || flag);
					arrayList.Add(stringBuilder.ToString());
					stringBuilder.Length = 0;
				}
			}
			catch (IndexOutOfRangeException)
			{
				if (flag)
				{
					reporter($"Error: Unbalanced '\"' in command line argument file '{fileName}'");
					result = true;
				}
				else if (stringBuilder.Length > 0)
				{
					arrayList.Add(stringBuilder.ToString());
				}
			}
			arguments = (string[])arrayList.ToArray(typeof(string));
			return result;
		}

		private static string LongName(ArgumentAttribute attribute, FieldInfo field)
		{
			return (attribute == null || attribute.DefaultLongName) ? field.Name : attribute.LongName;
		}

		private static string ShortName(ArgumentAttribute attribute, FieldInfo field)
		{
			if (attribute is DefaultArgumentAttribute)
			{
				return null;
			}
			if (!ExplicitShortName(attribute))
			{
				return LongName(attribute, field).Substring(0, 1);
			}
			return attribute.ShortName;
		}

		private static string HelpText(ArgumentAttribute attribute, FieldInfo field)
		{
			return attribute?.HelpText;
		}

		private static bool IsHidden(ArgumentAttribute attribute)
		{
			return attribute != null && (attribute.Type & ArgumentType.Hidden) == ArgumentType.Hidden;
		}

		private static bool HasHelpText(ArgumentAttribute attribute)
		{
			return attribute?.HasHelpText ?? false;
		}

		private static bool ExplicitShortName(ArgumentAttribute attribute)
		{
			return attribute != null && !attribute.DefaultShortName;
		}

		private static object DefaultValue(ArgumentAttribute attribute, FieldInfo field)
		{
			return (attribute == null || !attribute.HasDefaultValue) ? null : attribute.DefaultValue;
		}

		private static Type ElementType(FieldInfo field)
		{
			if (IsCollectionType(field.FieldType))
			{
				return field.FieldType.GetElementType();
			}
			return null;
		}

		private static ArgumentType Flags(ArgumentAttribute attribute, FieldInfo field)
		{
			if (attribute != null)
			{
				return attribute.Type;
			}
			if (IsCollectionType(field.FieldType))
			{
				return ArgumentType.MultipleUnique;
			}
			return ArgumentType.AtMostOnce;
		}

		private static bool IsCollectionType(Type type)
		{
			return type.IsArray;
		}

		private static bool IsValidElementType(Type type)
		{
			return type != null && (type == typeof(int) || type == typeof(uint) || type == typeof(double) || type == typeof(string) || type == typeof(bool) || type.IsEnum);
		}
	}
}
