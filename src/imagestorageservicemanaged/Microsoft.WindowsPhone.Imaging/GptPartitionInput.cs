using System;
using System.Globalization;
using System.Reflection;
using System.Xml.Serialization;

namespace Microsoft.WindowsPhone.Imaging
{
	public class GptPartitionInput
	{
		[XmlChoiceIdentifier("PartitionSpecifier")]
		[XmlElement("Name", typeof(string))]
		[XmlElement("Id", typeof(string))]
		public object DataType { get; set; }

		[XmlIgnore]
		public GptPartitionTypeChoice PartitionSpecifier { get; set; }

		[XmlIgnore]
		public Guid PartitionId
		{
			get
			{
				Guid empty = Guid.Empty;
				if (PartitionSpecifier == GptPartitionTypeChoice.Id)
				{
					empty = new Guid(DataType as string);
				}
				else
				{
					string text = DataType as string;
					if (string.Compare(ImageConstants.MAINOS_PARTITION_NAME, text, true, CultureInfo.InvariantCulture) == 0)
					{
						empty = ImageConstants.MAINOS_PARTITION_ID;
					}
					else if (string.Compare(ImageConstants.SYSTEM_PARTITION_NAME, text, true, CultureInfo.InvariantCulture) == 0)
					{
						empty = ImageConstants.SYSTEM_PARTITION_ID;
					}
					else
					{
						if (string.Compare(ImageConstants.MMOS_PARTITION_NAME, text, true, CultureInfo.InvariantCulture) != 0)
						{
							throw new ImageStorageException($"{MethodBase.GetCurrentMethod().Name}: The partition name {text} is not currently supported.");
						}
						empty = ImageConstants.MMOS_PARTITION_ID;
					}
				}
				return empty;
			}
		}
	}
}
