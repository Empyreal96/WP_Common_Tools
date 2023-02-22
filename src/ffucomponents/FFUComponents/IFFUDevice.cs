using System;

namespace FFUComponents
{
	public interface IFFUDevice : IDisposable
	{
		string DeviceFriendlyName { get; }

		Guid DeviceUniqueID { get; }

		Guid SerialNumber { get; }

		string DeviceType { get; }

		event EventHandler<ProgressEventArgs> ProgressEvent;

		void FlashFFUFile(string ffuFilePath);

		void FlashFFUFile(string ffuFilePath, bool optimize);

		bool WriteWim(string wimPath);

		bool EndTransfer();

		bool SkipTransfer();

		bool Reboot();

		bool EnterMassStorage();

		bool ClearIdOverride();

		bool GetDiskInfo(out uint blockSize, out ulong lastBlock);

		void ReadDisk(ulong diskOffset, byte[] buffer, int offset, int count);

		void WriteDisk(ulong diskOffset, byte[] buffer, int offset, int count);

		uint SetBootMode(uint bootMode, string profileName);

		string GetServicingLogs(string logFolderPath);

		string GetFlashingLogs(string logFolderPath);

		void QueryDeviceUnlockId(out byte[] unlockId, out byte[] oemId, out byte[] platformId);

		void RelockDeviceUnlockId();

		uint[] QueryUnlockTokenFiles();

		void WriteUnlockTokenFile(uint unlockTokenId, byte[] fileData);

		bool QueryBitlockerState();

		void Unlock(uint tokenId, string tokenFilePath, string pin);

		byte[] GetDeviceProperties();
	}
}
