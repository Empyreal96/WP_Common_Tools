using System;

namespace Microsoft.WindowsPhone.Imaging
{
	public class BcdElementDeviceGptInput
	{
		public string DiskId { get; set; }

		public GptPartitionInput Partition { get; set; }

		public static BcdElementDevice CreateGptBootDevice(BcdElementDeviceGptInput inputValue)
		{
			BcdElementDevice bcdElementDevice = BcdElementDevice.CreateBaseBootDevice();
			Guid empty = Guid.Empty;
			Guid empty2 = Guid.Empty;
			try
			{
				empty = new Guid(inputValue.DiskId);
				empty2 = inputValue.Partition.PartitionId;
			}
			catch (Exception innerException)
			{
				throw new ImageStorageException("Unable to parse the GPTDevice value.", innerException);
			}
			PartitionIdentifierEx identifier = PartitionIdentifierEx.CreateSimpleGpt(empty, empty2);
			bcdElementDevice.ReplaceBootDeviceIdentifier(identifier);
			return bcdElementDevice;
		}
	}
}
