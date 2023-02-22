using System;
using System.IO;
using System.Reflection;
using System.Xml.Serialization;

namespace Microsoft.WindowsPhone.Imaging
{
	public class BcdElementDeviceInput
	{
		[XmlIgnore]
		public DeviceTypeChoice DeviceType;

		[XmlChoiceIdentifier("DeviceType")]
		[XmlElement("GPTDevice", typeof(BcdElementDeviceGptInput))]
		[XmlElement("MBRDevice", typeof(BcdElementDeviceMbrInput))]
		[XmlElement("RamdiskDevice", typeof(BcdElementDeviceRamdiskInput))]
		public object DeviceValue { get; set; }

		public void SaveAsRegFile(TextWriter writer, string elementName)
		{
			switch (DeviceType)
			{
			case DeviceTypeChoice.MBRDevice:
			{
				BcdElementDevice bcdElementDevice2 = BcdElementDevice.CreateBaseBootDevice();
				byte[] array2 = new byte[bcdElementDevice2.BinarySize];
				MemoryStream memoryStream2 = new MemoryStream(array2);
				try
				{
					bcdElementDevice2.WriteToStream(memoryStream2);
				}
				finally
				{
					memoryStream2.Flush();
					memoryStream2 = null;
				}
				BcdElementValueTypeInput.WriteByteArray(writer, elementName, "\"Element\"=hex:", array2);
				writer.WriteLine();
				array2 = null;
				break;
			}
			case DeviceTypeChoice.RamdiskDevice:
			{
				BcdElementDeviceRamdiskInput bcdElementDeviceRamdiskInput = DeviceValue as BcdElementDeviceRamdiskInput;
				BcdElementBootDevice bcdElementBootDevice = null;
				switch (bcdElementDeviceRamdiskInput.ParentDevice.DeviceType)
				{
				case DeviceTypeChoice.GPTDevice:
					bcdElementBootDevice = BcdElementDeviceGptInput.CreateGptBootDevice(bcdElementDeviceRamdiskInput.ParentDevice.DeviceValue as BcdElementDeviceGptInput).BootDevice;
					break;
				case DeviceTypeChoice.MBRDevice:
					bcdElementBootDevice = BcdElementBootDevice.CreateBaseBootDevice();
					break;
				default:
					throw new ImageStorageException($"{MethodBase.GetCurrentMethod().Name}: The given Ramdisk parent type is not supported.");
				}
				BcdElementDevice bcdElementDevice3 = BcdElementDevice.CreateBaseRamdiskDevice(bcdElementDeviceRamdiskInput.FilePath, bcdElementBootDevice);
				MemoryStream memoryStream3 = new MemoryStream();
				byte[] array3;
				try
				{
					bcdElementDevice3.WriteToStream(memoryStream3);
					array3 = new byte[memoryStream3.ToArray().Length];
					Array.Copy(memoryStream3.ToArray(), array3, memoryStream3.Length);
				}
				finally
				{
					memoryStream3.Flush();
					memoryStream3 = null;
				}
				BcdElementValueTypeInput.WriteByteArray(writer, elementName, "\"Element\"=hex:", array3);
				writer.WriteLine();
				array3 = null;
				break;
			}
			case DeviceTypeChoice.GPTDevice:
			{
				BcdElementDevice bcdElementDevice = BcdElementDeviceGptInput.CreateGptBootDevice(DeviceValue as BcdElementDeviceGptInput);
				byte[] array = new byte[bcdElementDevice.BinarySize];
				MemoryStream memoryStream = new MemoryStream(array);
				try
				{
					bcdElementDevice.WriteToStream(memoryStream);
				}
				finally
				{
					memoryStream.Flush();
					memoryStream = null;
				}
				BcdElementValueTypeInput.WriteByteArray(writer, elementName, "\"Element\"=hex:", array);
				writer.WriteLine();
				array = null;
				break;
			}
			default:
				throw new ImageStorageException($"{MethodBase.GetCurrentMethod().Name}: Unsupported partition type: {DeviceType}.");
			}
		}

		public void SaveAsRegData(BcdRegData bcdRegData, string path)
		{
			switch (DeviceType)
			{
			case DeviceTypeChoice.MBRDevice:
			{
				BcdElementDevice bcdElementDevice2 = BcdElementDevice.CreateBaseBootDevice();
				byte[] array2 = new byte[bcdElementDevice2.BinarySize];
				MemoryStream memoryStream2 = new MemoryStream(array2);
				try
				{
					bcdElementDevice2.WriteToStream(memoryStream2);
				}
				finally
				{
					memoryStream2.Flush();
					memoryStream2 = null;
				}
				BcdElementValueTypeInput.WriteByteArray(bcdRegData, path, array2);
				array2 = null;
				break;
			}
			case DeviceTypeChoice.RamdiskDevice:
			{
				BcdElementDeviceRamdiskInput bcdElementDeviceRamdiskInput = DeviceValue as BcdElementDeviceRamdiskInput;
				BcdElementBootDevice bcdElementBootDevice = null;
				switch (bcdElementDeviceRamdiskInput.ParentDevice.DeviceType)
				{
				case DeviceTypeChoice.GPTDevice:
					bcdElementBootDevice = BcdElementDeviceGptInput.CreateGptBootDevice(bcdElementDeviceRamdiskInput.ParentDevice.DeviceValue as BcdElementDeviceGptInput).BootDevice;
					break;
				case DeviceTypeChoice.MBRDevice:
					bcdElementBootDevice = BcdElementBootDevice.CreateBaseBootDevice();
					break;
				default:
					throw new ImageStorageException($"{MethodBase.GetCurrentMethod().Name}: The given Ramdisk parent type is not supported.");
				}
				BcdElementDevice bcdElementDevice3 = BcdElementDevice.CreateBaseRamdiskDevice(bcdElementDeviceRamdiskInput.FilePath, bcdElementBootDevice);
				MemoryStream memoryStream3 = new MemoryStream();
				byte[] array3;
				try
				{
					bcdElementDevice3.WriteToStream(memoryStream3);
					array3 = new byte[memoryStream3.ToArray().Length];
					Array.Copy(memoryStream3.ToArray(), array3, memoryStream3.Length);
				}
				finally
				{
					memoryStream3.Flush();
					memoryStream3 = null;
				}
				BcdElementValueTypeInput.WriteByteArray(bcdRegData, path, array3);
				array3 = null;
				break;
			}
			case DeviceTypeChoice.GPTDevice:
			{
				BcdElementDevice bcdElementDevice = BcdElementDeviceGptInput.CreateGptBootDevice(DeviceValue as BcdElementDeviceGptInput);
				byte[] array = new byte[bcdElementDevice.BinarySize];
				MemoryStream memoryStream = new MemoryStream(array);
				try
				{
					bcdElementDevice.WriteToStream(memoryStream);
				}
				finally
				{
					memoryStream.Flush();
					memoryStream = null;
				}
				BcdElementValueTypeInput.WriteByteArray(bcdRegData, path, array);
				array = null;
				break;
			}
			default:
				throw new ImageStorageException($"{MethodBase.GetCurrentMethod().Name}: Unsupported partition type: {DeviceType}.");
			}
		}
	}
}
