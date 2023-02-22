using System;
using System.Xml.Linq;

namespace Microsoft.WindowsPhone.ImageUpdate.PkgGenCommon.BaseComponents
{
	public class PkgXmlException : PkgGenException
	{
		public XElement XmlElement { get; set; }

		public PkgXmlException(XElement failedElement, string message, params object[] args)
			: this(null, failedElement, message, args)
		{
		}

		public PkgXmlException(Exception innerException, XElement failedElement, string message, params object[] args)
			: base(innerException, message, args)
		{
			XmlElement = failedElement;
		}
	}
}
