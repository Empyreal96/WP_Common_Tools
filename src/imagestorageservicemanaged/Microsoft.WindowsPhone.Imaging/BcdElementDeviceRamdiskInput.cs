namespace Microsoft.WindowsPhone.Imaging
{
	public class BcdElementDeviceRamdiskInput
	{
		private BcdElementDeviceInput _parent;

		public BcdElementDeviceInput ParentDevice
		{
			get
			{
				return _parent;
			}
			set
			{
				if (value.DeviceType == DeviceTypeChoice.RamdiskDevice)
				{
					throw new ImageStorageException("A RamDisk's parent device cannot be another ramdisk.");
				}
				_parent = value;
			}
		}

		public string FilePath { get; set; }

		public bool RamdiskOptions { get; set; }
	}
}
