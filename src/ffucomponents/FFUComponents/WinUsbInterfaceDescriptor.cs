namespace FFUComponents
{
	internal struct WinUsbInterfaceDescriptor
	{
		public byte Length;

		public byte DescriptorType;

		public byte InterfaceNumber;

		public byte AlternateSetting;

		public byte NumEndpoints;

		public byte InterfaceClass;

		public byte InterfaceSubClass;

		public byte InterfaceProtocol;

		public byte Interface;
	}
}
