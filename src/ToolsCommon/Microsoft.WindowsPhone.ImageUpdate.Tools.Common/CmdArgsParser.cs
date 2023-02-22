using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml;
using System.Xml.Linq;

namespace Microsoft.WindowsPhone.ImageUpdate.Tools.Common
{
	public class CmdArgsParser
	{
		[AttributeUsage(AttributeTargets.Property)]
		public class CaseInsensitive : Attribute
		{
		}

		private const int HELP_INDENTATION = 12;

		public static T ParseArgs<T>(string[] args, params object[] configuration) where T : class, new()
		{
			return ParseArgs<T>(args.ToList(), configuration);
		}

		public static T ParseArgs<T>(List<string> args, params object[] configuration) where T : class, new()
		{
			List<CmdModes> list = new List<CmdModes>();
			foreach (object obj in configuration)
			{
				if (!(obj is CmdModes))
				{
					throw new ArgumentException($"A configuration argument was passed that was not of type \"CmdModes\". ARG={obj}");
				}
				list.Add((CmdModes)obj);
			}
			List<string> list2 = new List<string>();
			foreach (string arg in args)
			{
				if (arg.StartsWith("@", StringComparison.OrdinalIgnoreCase))
				{
					if (!File.Exists(arg.Substring(1)))
					{
						throw new FileNotFoundException($"Response file '{arg.Substring(1)}' could not be found.");
					}
					string[] array = File.ReadAllLines(arg.Substring(1));
					foreach (string item in array)
					{
						list2.Add(item);
					}
				}
				else
				{
					list2.Add(arg);
				}
			}
			args = list2;
			Type typeFromHandle = typeof(T);
			Dictionary<string, string> dictionary = ProcessCommandLine<T>(args, list);
			if (dictionary.ContainsKey("?"))
			{
				ParseUsage<T>(list);
				return null;
			}
			while (!list.Contains(CmdModes.DisableCFG) && dictionary.ContainsKey("cfg"))
			{
				string configLocation = dictionary["cfg"];
				dictionary.Remove("cfg");
				dictionary = ParseConfig(configLocation, dictionary, list);
			}
			MissingArguments<T>(dictionary, list);
			dictionary = ExtraArguments<T>(dictionary, list);
			T val = new T();
			foreach (KeyValuePair<string, string> commandEntry in dictionary)
			{
				PropertyInfo property = typeFromHandle.GetProperty(commandEntry.Key);
				Type type = Nullable.GetUnderlyingType(property.PropertyType) ?? property.PropertyType;
				if (type.IsGenericType)
				{
					if (type.GetGenericTypeDefinition() == typeof(List<>))
					{
						IList value = ReflectionListFactory(type.GetGenericArguments()[0], commandEntry.Value);
						property.SetValue(val, value, null);
						continue;
					}
					if (type.GetGenericTypeDefinition() == typeof(Dictionary<, >))
					{
						Type keyType = type.GetGenericArguments()[0];
						Type valueType = type.GetGenericArguments()[1];
						IDictionary value2 = ReflectionDictionaryFactory(keyType, valueType, commandEntry.Value);
						property.SetValue(val, value2, null);
						continue;
					}
					if (!(type.GetGenericTypeDefinition() == typeof(HashSet<>)))
					{
						throw new NotImplementedException($"CmdArgsParser does not support generic type '{type.Name}'.");
					}
					IEnumerable value3 = ReflectionSetFactory(type.GetGenericArguments()[0], commandEntry.Value, commandEntry.Key);
					property.SetValue(val, value3, null);
				}
				else if (type.IsEnum)
				{
					if (!Enum.GetNames(type).Any((string x) => x.Equals(commandEntry.Value, StringComparison.OrdinalIgnoreCase)))
					{
						string text = Enum.GetNames(type)[0];
						foreach (string item2 in Enum.GetNames(type).Skip(1))
						{
							text += $" {item2}";
						}
						throw new ArgumentException($"The value for \"{commandEntry.Key}\" is incorrect.\nValue: {commandEntry.Value}\nSupported Values: {text}");
					}
					property.SetValue(val, Convert.ChangeType(Enum.Parse(type, commandEntry.Value, true), type), null);
				}
				else
				{
					if (!TypeDescriptor.GetConverter(type).IsValid(commandEntry.Value))
					{
						throw new ArgumentException($"The value for \"{commandEntry.Key}\" is incorrect.\nValue '{commandEntry.Value}' cannot be converted to switch type '{type.Name}'.");
					}
					property.SetValue(val, Convert.ChangeType(commandEntry.Value, type), null);
				}
			}
			return val;
		}

		public static void ParseUsage<T>(List<CmdModes> modes) where T : class, new()
		{
			if (modes.Contains(CmdModes.DisableUsage))
			{
				return;
			}
			int num = 12;
			Type typeFromHandle = typeof(T);
			PropertyInfo[] properties = typeFromHandle.GetProperties();
			T obj = new T();
			string text = $"Usage: {Path.GetFileName(Process.GetCurrentProcess().MainModule.FileName)}";
			object[] customAttributes = typeFromHandle.GetCustomAttributes(typeof(DescriptionAttribute), false);
			for (int i = 0; i < customAttributes.Length; i++)
			{
				DescriptionAttribute descriptionAttribute = (DescriptionAttribute)customAttributes[i];
				text = text + "\n" + descriptionAttribute.Description;
			}
			PropertyInfo[] array = properties;
			foreach (PropertyInfo propertyInfo in array)
			{
				string text2 = (propertyInfo.GetCustomAttributes(typeof(RequiredAttribute), false).Any() ? propertyInfo.Name : $"[{propertyInfo.Name}]");
				if (text2.Length + 1 > num)
				{
					num = text2.Length + 1;
				}
				text += $" {text2}";
			}
			if (!modes.Contains(CmdModes.DisableCFG))
			{
				text += " [cfg]";
			}
			array = properties;
			foreach (PropertyInfo propertyInfo2 in array)
			{
				string text3 = "";
				customAttributes = propertyInfo2.GetCustomAttributes(typeof(DescriptionAttribute), false);
				for (int j = 0; j < customAttributes.Length; j++)
				{
					text3 = ((DescriptionAttribute)customAttributes[j]).Description;
				}
				if (propertyInfo2.GetCustomAttributes(typeof(CaseInsensitive), false).Any())
				{
					text3 = text3 + "\n" + new string(' ', num + 3) + "Case Insensitive.";
				}
				string text4 = null;
				string text5 = null;
				if (!propertyInfo2.GetCustomAttributes(typeof(RequiredAttribute), false).Any())
				{
					text4 = $"[{propertyInfo2.Name}]";
					text5 = ((propertyInfo2.GetValue(obj, null) != null) ? propertyInfo2.GetValue(obj, null).ToString() : null);
				}
				else
				{
					text4 = propertyInfo2.Name;
				}
				string text6 = new string('·', num - text4.Length);
				string text7 = new string(' ', num);
				string text8 = $"  {text4}{text6} {text3}\n{text7}  ";
				Type type = Nullable.GetUnderlyingType(propertyInfo2.PropertyType) ?? propertyInfo2.PropertyType;
				if (type.IsGenericType)
				{
					if (type.GetGenericTypeDefinition() == typeof(List<>))
					{
						text8 += $" Values:<{type.GetGenericArguments()[0].Name}>, Multiple values seperated by ';'";
					}
					else if (type.GetGenericTypeDefinition() == typeof(Dictionary<, >))
					{
						text8 += $" Values:<{type.GetGenericArguments()[0].Name}={type.GetGenericArguments()[1].Name}>, Multiple values seperated by ';'";
					}
					else if (type.GetGenericTypeDefinition() == typeof(HashSet<>))
					{
						text8 += $" Values:<{type.GetGenericArguments()[0].Name}>, Multiple values seperated by ';'\n{text7}   Duplicates are not supported.";
					}
				}
				else if (!type.IsEnum)
				{
					text8 = ((type == typeof(bool)) ? (text8 + $" Values:<true | false>") : ((type == typeof(int)) ? (text8 + $" Values:<Number>") : ((!(type == typeof(float))) ? (text8 + $" Values:<Free Text>") : (text8 + $" Values:<Decimal>"))));
				}
				else
				{
					text8 += $" Values:<{Enum.GetNames(type)[0]}";
					foreach (string item in Enum.GetNames(type).Skip(1))
					{
						text8 += $" | {item}";
					}
					text8 += ">";
				}
				if (!propertyInfo2.GetCustomAttributes(typeof(RequiredAttribute), false).Any())
				{
					text8 = ((text5 != null) ? ((!(propertyInfo2.PropertyType == typeof(string))) ? (text8 + $" Default={text5}") : (text8 + $" Default=\"{text5}\"")) : (text8 + $" Default=NULL"));
				}
				text += $"\n{text8}";
			}
			if (!modes.Contains(CmdModes.DisableCFG))
			{
				string text9 = new string('·', num - "[cfg]".Length);
				string text10 = new string(' ', num);
				text += string.Format("\n  {0}{1} {2}\n{3}   {4}\n{3}   Values:<Free Text>", "[cfg]", text9, "A configuration file used to configure the enviornment.", text10, "If supplied the configuration file will override the command line. Used as named argument only.");
			}
			Console.WriteLine(text);
		}

		public static Dictionary<string, string> ParseConfig(string configLocation, Dictionary<string, string> cmds, List<CmdModes> modes)
		{
			if (string.IsNullOrEmpty(configLocation) || configLocation.Equals("true"))
			{
				throw new ArgumentNullException($"No configuration file was specified on switch \"cfg\". Please specify a config location with: /cfg:<location>");
			}
			if (!File.Exists(configLocation))
			{
				throw new FileNotFoundException($"Can't find configuration file {configLocation}.");
			}
			XDocument xDocument = XDocument.Load(configLocation, LoadOptions.SetLineInfo);
			if (!xDocument.Root.Name.LocalName.Equals("Configuration"))
			{
				throw new FormatException($"The format of the provided configuration file is incorrect. Root element was \"{xDocument.Root.Name.LocalName}\" instead of \"Configuration\". CFG:{configLocation}");
			}
			HashSet<string> hashSet = new HashSet<string>();
			List<string> list = new List<string>();
			foreach (XElement item in xDocument.Root.Elements())
			{
				string text = item.Name.LocalName.ToString();
				IXmlLineInfo xmlLineInfo = item;
				if (item.HasElements)
				{
					if (item.Attribute("name") == null)
					{
						throw new FormatException($"Container '{item.Name.LocalName}' is missing a 'name' attribute. Containers require a name attribute. Error at ({xmlLineInfo.LineNumber},{xmlLineInfo.LinePosition})");
					}
					text = item.Attribute("name").Value.ToString();
				}
				if (string.IsNullOrEmpty(item.Value.ToString()) && !item.HasElements)
				{
					throw new FormatException($"Key \"{item.Name.LocalName}\" does not have an associated value. Null values are not supported in the configuration file. Keys need to be in to format: <\"key\">value</\"key\">.");
				}
				string text2 = item.Value.ToString();
				if (hashSet.Contains(text))
				{
					list.Add($"Key: {text} at ({xmlLineInfo.LineNumber},{xmlLineInfo.LinePosition})");
					continue;
				}
				hashSet.Add(text);
				bool flag = false;
				if (item.HasElements)
				{
					flag = true;
					XElement xElement = item.Elements().First();
					if (item.Name.LocalName == "List" || item.Name.LocalName == "HashSet")
					{
						if (xElement.Name.LocalName != "Value")
						{
							throw new FormatException($"Element under Container '{item.Name.LocalName}' is not reconized. '{xElement.Name.LocalName}' is not supported. Use 'Value' for tags. Error at ({xmlLineInfo.LineNumber},{xmlLineInfo.LinePosition})");
						}
						text2 = $"{xElement.Value.ToString()}";
					}
					else
					{
						if (!(item.Name.LocalName == "Dictionary"))
						{
							throw new FormatException($"Container '{item.Name.LocalName}' is not reconized. Supported: 'List', 'HashSet', 'Dictionary'. Error at ({xmlLineInfo.LineNumber},{xmlLineInfo.LinePosition})");
						}
						if (xElement.Name.LocalName.ToString().Contains(' '))
						{
							throw new FormatException($"'{xElement.Name.LocalName}' is not allowed for use as a Key in a Dictionary. Spaces are not supported in Key names. Error at ({xmlLineInfo.LineNumber},{xmlLineInfo.LinePosition})");
						}
						text2 = $"{xElement.Name.LocalName.ToString()}={xElement.Value.ToString()}";
					}
					foreach (XElement item2 in item.Elements().Skip(1))
					{
						if (item.Name.LocalName == "List" || item.Name.LocalName == "HashSet")
						{
							if (item2.Name.LocalName != "Value")
							{
								throw new FormatException($"Element under Container '{item.Name.LocalName}' is not reconized. '{item2.Name.LocalName}' is not supported. Use 'Value' for tags. Error at ({xmlLineInfo.LineNumber},{xmlLineInfo.LinePosition})");
							}
							text2 += $";{item2.Value.ToString()}";
						}
						else
						{
							if (item2.Name.LocalName.ToString().Contains(' '))
							{
								throw new FormatException($"'{item2.Name.LocalName}' is not allowed for use as a Key in a Dictionary. Spaces are not supported in Key names. Error at ({xmlLineInfo.LineNumber},{xmlLineInfo.LinePosition})");
							}
							text2 += $";{item2.Name.LocalName.ToString()}={item2.Value.ToString()}";
						}
					}
				}
				if (cmds.ContainsKey(text))
				{
					if (modes.Contains(CmdModes.CFGOverride))
					{
						cmds[text] = text2;
					}
					else if (flag)
					{
						cmds[text] += $";{text2}";
					}
				}
				else
				{
					cmds.Add(text, text2);
				}
			}
			if (list.Any())
			{
				string text3 = list.First();
				foreach (string item3 in list.Skip(1))
				{
					text3 += $"\n{item3}";
				}
				throw new FormatException($"There were {list.Count()} duplicate entries in {configLocation}. Duplicate Keys:\n{text3}");
			}
			return cmds;
		}

		private static void MissingArguments<T>(Dictionary<string, string> commandTable, List<CmdModes> modes) where T : class, new()
		{
			Type typeFromHandle = typeof(T);
			List<string> list = new List<string>();
			PropertyInfo[] properties = typeFromHandle.GetProperties();
			foreach (PropertyInfo propertyInfo in properties)
			{
				if (propertyInfo.GetCustomAttributes(typeof(RequiredAttribute), false).Any() && !commandTable.ContainsKey(propertyInfo.Name))
				{
					list.Add(propertyInfo.Name);
				}
			}
			if (!list.Any())
			{
				return;
			}
			ParseUsage<T>(modes);
			string text = $"\"{list.First()}\"";
			foreach (string item in list.Skip(1))
			{
				text += $", \"{item}\"";
			}
			throw new ArgumentNullException(string.Format("Required argument {0} {1} not specified.", text, (list.Count > 1) ? "were" : "was"));
		}

		private static Dictionary<string, string> ExtraArguments<T>(Dictionary<string, string> commandTable, List<CmdModes> modes) where T : class, new()
		{
			Type typeFromHandle = typeof(T);
			List<string> list = new List<string>();
			Dictionary<string, string> dictionary = new Dictionary<string, string>();
			foreach (KeyValuePair<string, string> item in commandTable)
			{
				if (typeFromHandle.GetProperty(item.Key) == null)
				{
					bool flag = true;
					if (modes.Contains(CmdModes.LegacySwitchFormat))
					{
						PropertyInfo[] properties = typeFromHandle.GetProperties();
						foreach (PropertyInfo propertyInfo in properties)
						{
							if (propertyInfo.Name.StartsWith(item.Key, StringComparison.InvariantCultureIgnoreCase))
							{
								dictionary[propertyInfo.Name] = item.Value;
								flag = false;
								break;
							}
						}
					}
					if (flag)
					{
						list.Add(item.Key);
					}
				}
				else
				{
					dictionary[item.Key] = item.Value;
				}
			}
			if (list.Any())
			{
				ParseUsage<T>(modes);
				string text = $"\"{list.First()}\"";
				foreach (string item2 in list.Skip(1))
				{
					text += $", \"{item2}\"";
				}
				throw new ArgumentOutOfRangeException(string.Format("Unknown argument {0} {1} provided.", text, (list.Count > 1) ? "were" : "was"));
			}
			return dictionary;
		}

		private static Dictionary<string, string> ProcessCommandLine<T>(List<string> args, List<CmdModes> modes) where T : class, new()
		{
			Type typeFromHandle = typeof(T);
			Dictionary<string, string> dictionary = new Dictionary<string, string>();
			bool flag = false;
			foreach (string arg in args)
			{
				string value = "true";
				bool flag2 = false;
				if (modes.Contains(CmdModes.LegacySwitchFormat))
				{
					if (arg.First() == '-')
					{
						flag2 = true;
						value = "false";
					}
					else if (arg.First() == '+')
					{
						flag2 = true;
					}
				}
				else if (arg.First() == '-' || arg.First() == '+')
				{
					throw new FormatException($"Argument {dictionary.Count + 1} is in the wrong format. Legacy arguments are not supported. ARG={arg}");
				}
				string text;
				if (arg.First() != '/' && !flag2)
				{
					if (flag)
					{
						throw new FormatException($"Argument {dictionary.Count + 1} is in the wrong format. After a named argument all arguments must be named. ARG={arg}");
					}
					if (dictionary.Count >= typeFromHandle.GetProperties().Count())
					{
						throw new ArgumentException($"To many positional arguments supplied. Amount allowed: {typeFromHandle.GetProperties().Count()}\nOffending Argument: {arg}");
					}
					text = typeFromHandle.GetProperties()[dictionary.Count].Name;
					value = arg;
				}
				else
				{
					flag = true;
					text = ((arg.IndexOf(":", StringComparison.OrdinalIgnoreCase) != -1) ? arg.Substring(1, arg.IndexOf(":", StringComparison.OrdinalIgnoreCase) - 1) : arg.Substring(1));
					if (arg.IndexOf(":", StringComparison.OrdinalIgnoreCase) != -1)
					{
						value = arg.Substring(arg.IndexOf(":", StringComparison.OrdinalIgnoreCase) + 1);
					}
					else
					{
						PropertyInfo propertyInfo = typeFromHandle.GetProperty(text);
						if (propertyInfo == null)
						{
							propertyInfo = typeFromHandle.GetProperty(text, BindingFlags.IgnoreCase | BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public);
							if (propertyInfo != null)
							{
								if (propertyInfo.GetCustomAttributes(typeof(CaseInsensitive), false).Any())
								{
									text = propertyInfo.Name;
								}
								else
								{
									propertyInfo = null;
								}
							}
						}
						if (propertyInfo != null)
						{
							Type type = Nullable.GetUnderlyingType(propertyInfo.PropertyType) ?? propertyInfo.PropertyType;
							if (!type.IsAssignableFrom(typeof(bool)))
							{
								throw new ArgumentException($"{text} was used as a 'Boolean' switch however the switch type is '{type.Name}'.");
							}
						}
					}
				}
				if (dictionary.ContainsKey(text))
				{
					throw new FormatException($"Argument {text} has already been declared. ARG={arg}");
				}
				dictionary.Add(text, value);
			}
			return dictionary;
		}

		public static IList ReflectionListFactory(Type contentType, string dataSource)
		{
			IList list = (IList)Activator.CreateInstance(typeof(List<>).MakeGenericType(contentType));
			string[] array = dataSource.Split(';');
			foreach (string value in array)
			{
				if (contentType.IsEnum)
				{
					list.Add(Convert.ChangeType(Enum.Parse(contentType, value), contentType));
				}
				else
				{
					list.Add(Convert.ChangeType(value, contentType));
				}
			}
			return list;
		}

		public static IDictionary ReflectionDictionaryFactory(Type keyType, Type valueType, string dataSource)
		{
			IDictionary dictionary = (IDictionary)Activator.CreateInstance(typeof(Dictionary<, >).MakeGenericType(keyType, valueType));
			HashSet<object> hashSet = new HashSet<object>();
			string[] array = dataSource.Split(';');
			foreach (string text in array)
			{
				string[] array2 = text.Split('=');
				if (array2.Count() != 2)
				{
					throw new FormatException($"The format of a dictionary argument is incorrect. The format is 'key=value'. Offending value: '{text}'");
				}
				object obj = ((!keyType.IsEnum) ? Convert.ChangeType(array2[0], keyType) : Convert.ChangeType(Enum.Parse(keyType, array2[0], true), keyType));
				if (!hashSet.Contains(obj))
				{
					hashSet.Add(obj);
					object value = ((!valueType.IsEnum) ? Convert.ChangeType(array2[1], valueType) : Convert.ChangeType(Enum.Parse(valueType, array2[1], true), valueType));
					dictionary.Add(obj, value);
				}
			}
			return dictionary;
		}

		public static IEnumerable ReflectionSetFactory(Type contentType, string dataSource, string name)
		{
			Type type = typeof(HashSet<>).MakeGenericType(contentType);
			IEnumerable enumerable = (IEnumerable)Activator.CreateInstance(type);
			MethodInfo method = type.GetMethod("Add");
			MethodInfo method2 = type.GetMethod("Contains");
			List<string> list = new List<string>();
			string[] array = dataSource.Split(';');
			foreach (string value in array)
			{
				if (contentType.IsEnum)
				{
					object obj = Convert.ChangeType(Enum.Parse(contentType, value), contentType);
					if ((bool)method2.Invoke(enumerable, new object[1] { obj }))
					{
						list.Add(obj.ToString());
						continue;
					}
					method.Invoke(enumerable, new object[1] { obj });
				}
				else
				{
					object obj2 = Convert.ChangeType(value, contentType);
					if ((bool)method2.Invoke(enumerable, new object[1] { obj2 }))
					{
						list.Add(obj2.ToString());
						continue;
					}
					method.Invoke(enumerable, new object[1] { obj2 });
				}
			}
			if (list.Any())
			{
				string text = $"\"{list.First()}\"";
				foreach (string item in list.Skip(1))
				{
					text += $", \"{item}\"";
				}
				throw new FormatException(string.Format("HashSet '{0}' had {1} duplicate value{2}. Duplicates: {3}", name, list.Count(), (list.Count() > 1) ? "s" : "", text));
			}
			return enumerable;
		}
	}
}
