namespace Microsoft.WindowsPhone.Imaging
{
	public enum DeviceLayoutValidationError
	{
		Pass,
		UnknownInternalError,
		DeviceLayoutNotMsOwned,
		DeviceLayoutNotOEMOwned,
		DeviceLayoutNotProductionSigned,
		DeviceLayoutValidationManifestNotProductionSigned,
		DeviceLayoutAttributeMismatch,
		PartitionNotFound,
		PartitionPositionMismatch,
		PartitionAttributeValueMismatch,
		BackupPartitionNotFound,
		BackupPartitionSizeMismatch,
		PartitionInvalidName
	}
}
