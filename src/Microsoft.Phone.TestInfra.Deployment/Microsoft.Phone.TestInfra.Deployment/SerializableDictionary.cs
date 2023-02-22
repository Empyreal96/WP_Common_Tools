using System;
using System.Collections.Generic;
using System.IO;
using System.Web.Script.Serialization;

namespace Microsoft.Phone.TestInfra.Deployment
{
	public class SerializableDictionary<TKey, TValue> : Dictionary<TKey, TValue>
	{
		private static int MaxJsonLength = 20971520;

		public void SerializeToFile(string file)
		{
			if (string.IsNullOrEmpty(file))
			{
				throw new ArgumentException("cannot be null or empty.", "file");
			}
			string directoryName = Path.GetDirectoryName(Path.GetFullPath(file));
			if (!Directory.Exists(directoryName))
			{
				Directory.CreateDirectory(directoryName);
			}
			JavaScriptSerializer javaScriptSerializer = new JavaScriptSerializer();
			javaScriptSerializer.MaxJsonLength = MaxJsonLength;
			string contents = javaScriptSerializer.Serialize(this);
			File.WriteAllText(file, contents);
		}

		public static SerializableDictionary<TKey, TValue> DeserializeFile(string file)
		{
			if (string.IsNullOrEmpty(file))
			{
				throw new ArgumentException("cannot be null or empty.", "file");
			}
			if (!File.Exists(file))
			{
				throw new FileNotFoundException(file);
			}
			JavaScriptSerializer javaScriptSerializer = new JavaScriptSerializer();
			javaScriptSerializer.MaxJsonLength = MaxJsonLength;
			object obj = javaScriptSerializer.Deserialize(File.ReadAllText(file), typeof(SerializableDictionary<TKey, TValue>));
			if (!(obj is SerializableDictionary<TKey, TValue>))
			{
				throw new InvalidDataException($"File {file} cannot be deserialized to an obj of type {typeof(SerializableDictionary<TKey, TValue>).Name}");
			}
			return obj as SerializableDictionary<TKey, TValue>;
		}
	}
}
