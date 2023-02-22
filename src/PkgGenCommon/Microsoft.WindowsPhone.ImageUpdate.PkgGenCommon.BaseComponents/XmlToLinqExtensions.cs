using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using System.Xml.Serialization;

namespace Microsoft.WindowsPhone.ImageUpdate.PkgGenCommon.BaseComponents
{
	public static class XmlToLinqExtensions
	{
		public delegate void WithEntityDelegate<T>(T entity) where T : XObject;

		public static XElement ToXElement<T>(this T pkgObject) where T : PkgObject
		{
			using (MemoryStream memoryStream = new MemoryStream())
			{
				using (TextWriter textWriter = new StreamWriter(memoryStream))
				{
					new XmlSerializer(pkgObject.GetType()).Serialize(textWriter, pkgObject);
					return XElement.Parse(Encoding.UTF8.GetString(memoryStream.ToArray()));
				}
			}
		}

		public static T FromXElement<T>(this XElement element)
		{
			using (MemoryStream stream = new MemoryStream(Encoding.UTF8.GetBytes(element.ToString())))
			{
				return (T)new XmlSerializer(typeof(T)).Deserialize(stream);
			}
		}

		public static bool WithLocalAttribute<T>(this T source, string localName, WithEntityDelegate<XAttribute> del) where T : XElement
		{
			if (source.LocalAttribute(localName) != null)
			{
				del(source.LocalAttribute(localName));
				return true;
			}
			return false;
		}

		public static XAttribute LocalAttribute<T>(this T source, string localName) where T : XElement
		{
			IEnumerable<XAttribute> enumerable = from a in source.Attributes()
				where a.Name.LocalName == localName
				select a;
			if (enumerable != null && enumerable.Count() > 0)
			{
				return enumerable.First();
			}
			return null;
		}

		public static XElement LocalElement<T>(this T source, string localName) where T : XContainer
		{
			IEnumerable<XElement> enumerable = from e in source.Elements()
				where e.Name.LocalName == localName
				select e;
			if (enumerable != null && enumerable.Count() > 0)
			{
				return enumerable.First();
			}
			return null;
		}

		public static IEnumerable<XElement> LocalElements<T>(this T source, string localName) where T : XContainer
		{
			IEnumerable<XElement> enumerable = from e in source.Elements()
				where e.Name.LocalName == localName
				select e;
			return enumerable ?? new List<XElement>();
		}

		public static IEnumerable<XElement> LocalDescendants<T>(this T source, string localName) where T : XContainer
		{
			IEnumerable<XElement> enumerable = from e in source.Descendants()
				where e.Name.LocalName == localName
				select e;
			return enumerable ?? new List<XElement>();
		}
	}
}
