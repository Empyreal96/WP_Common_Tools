using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml;
using System.Xml.Serialization;
using System.Xml.XPath;
using Microsoft.WindowsPhone.ImageUpdate.Tools.Common;

namespace Microsoft.WindowsPhone.Imaging
{
	public class ImageGeneratorParameters
	{
		[CLSCompliant(false)]
		public enum FFUHashAlgorithm : uint
		{
			Ffuhsha256 = 32780u
		}

		[CLSCompliant(false)]
		public const uint DefaultChunkSize = 256u;

		[CLSCompliant(false)]
		public const uint DevicePlatformIDSize = 192u;

		private const uint _OneKiloBtye = 1024u;

		private const uint _MinimumSectorSize = 512u;

		private const uint _MinimumSectorFreeCount = 8192u;

		private IULogger _logger;

		private const uint ALG_CLASS_HASH = 32768u;

		private const uint ALG_TYPE_ANY = 0u;

		private const uint ALG_SID_SHA_256 = 12u;

		private const uint CALG_SHA_256 = 32780u;

		public List<InputStore> Stores;

		public string[] DevicePlatformIDs;

		private uint _chunkSize = 256u;

		private uint _algid = 32780u;

		private uint _deviceLayoutVersion = 1u;

		public string Description { get; set; }

		public InputStore MainOSStore => Stores.FirstOrDefault((InputStore x) => x.IsMainOSStore());

		[CLSCompliant(false)]
		public uint ChunkSize
		{
			get
			{
				return _chunkSize;
			}
			set
			{
				_chunkSize = ((value != 0) ? value : 256u);
			}
		}

		[CLSCompliant(false)]
		public uint DefaultPartitionByteAlignment { get; set; }

		[CLSCompliant(false)]
		public uint VirtualHardDiskSectorSize { get; set; }

		[CLSCompliant(false)]
		public uint SectorSize { get; set; }

		[CLSCompliant(false)]
		public uint MinSectorCount { get; set; }

		[CLSCompliant(false)]
		public uint Algid
		{
			get
			{
				return _algid;
			}
			set
			{
				_algid = ((value != 0) ? value : 32780u);
			}
		}

		public uint DeviceLayoutVersion => _deviceLayoutVersion;

		public InputRules Rules { get; set; }

		public ImageGeneratorParameters()
		{
			Stores = new List<InputStore>();
		}

		public void Initialize(IULogger logger)
		{
			if (logger == null)
			{
				_logger = new IULogger();
			}
			else
			{
				_logger = logger;
			}
		}

		private bool VerifyPartitionSizes()
		{
			uint num = 0u;
			if (Stores == null)
			{
				return true;
			}
			InputPartition[] partitions = MainOSStore.Partitions;
			foreach (InputPartition inputPartition in partitions)
			{
				num = ((!inputPartition.UseAllSpace) ? (num + inputPartition.TotalSectors) : (num + 1));
			}
			if (num > MinSectorCount)
			{
				ulong num2 = (ulong)((long)num * (long)SectorSize) / 1024uL / 1024uL;
				ulong num3 = (ulong)((long)MinSectorCount * (long)SectorSize) / 1024uL / 1024uL;
				_logger.LogError($"ImageCommon!ImageGeneratorParameters::VerifyPartitionSizes: The total sectors used by all the partitions ({num} sectors/{num2} MB) is larger than the MinSectorCount ({MinSectorCount} sectors/{num3} MB). This means the image would not flash to a device with only {MinSectorCount} sectors/{num3} MB. Either remove image content, or increase MinSectorCount.");
				return false;
			}
			return true;
		}

		public bool VerifyInputParameters()
		{
			Dictionary<string, string> dictionary = new Dictionary<string, string>();
			if (uint.MaxValue / ChunkSize < 1024)
			{
				_logger.LogError("ImageCommon!ImageGeneratorParameters::VerifyInputParameters: The chunk size is specified in Kilobytes and the total size must be under 4GB.");
				return false;
			}
			if (SectorSize < 512)
			{
				_logger.LogError("ImageCommon!ImageGeneratorParameters::VerifyInputParameters: The sector size must be at least 512 bytes: {0} bytes.", SectorSize);
				return false;
			}
			if (!InputHelpers.IsPowerOfTwo(SectorSize))
			{
				_logger.LogError("ImageCommon!ImageGeneratorParameters::VerifyInputParameters: The sector size must be a multiple of 2: {0} bytes.", SectorSize);
				return false;
			}
			if (ChunkSize * 1024 < SectorSize)
			{
				_logger.LogError("ImageCommon!ImageGeneratorParameters::VerifyInputParameters: The chunk size is specified in Kilobytes and the total size must be under larger the sector size: {0} bytes.", SectorSize);
				return false;
			}
			if (ChunkSize * 1024 % SectorSize != 0)
			{
				_logger.LogError("ImageCommon!ImageGeneratorParameters::VerifyInputParameters: The chunk size is specified in Kilobytes and must be divisible by the sector size: {0}.", SectorSize);
				return false;
			}
			if (DefaultPartitionByteAlignment != 0 && !InputHelpers.IsPowerOfTwo(DefaultPartitionByteAlignment))
			{
				_logger.LogError("ImageCommon!ImageGeneratorParameters::VerifyInputParameters: The default partition byte alignment must be a multiple of 2: {0} bytes.", DefaultPartitionByteAlignment);
				return false;
			}
			if (Stores == null || Stores.Count == 0)
			{
				_logger.LogError("ImageCommon!ImageGeneratorParameters::VerifyInputParameters: For Generating a FFU image, at least one store must be specified.");
				return false;
			}
			if (Stores.Count((InputStore x) => x.IsMainOSStore()) != 1)
			{
				_logger.LogError("ImageCommon!ImageGeneratorParameters::VerifyInputParameters: For Generating a FFU image, one and only one of the stores must be MainOS.");
				return false;
			}
			if (MainOSStore.Partitions == null || MainOSStore.Partitions.Count() == 0)
			{
				_logger.LogError("ImageCommon!ImageGeneratorParameters::VerifyInputParameters: For Generating a FFU image, at least one partition must be specified.");
				return false;
			}
			if (SectorSize == 0)
			{
				_logger.LogError("ImageCommon!ImageGeneratorParameters::VerifyInputParameters: The SectorSize cannot be 0. Please provide a valid SectorSize.");
				return false;
			}
			if (ChunkSize == 0)
			{
				_logger.LogError("ImageCommon!ImageGeneratorParameters::VerifyInputParameters: The ChunkSize cannot be 0. Please provide a valid ChunkSize between 1-1024.");
				return false;
			}
			if (ChunkSize < 1 || ChunkSize > 1024)
			{
				_logger.LogError("ImageCommon!ImageGeneratorParameters::VerifyInputParameters: The ChunkSize must between 1-1024.");
				return false;
			}
			int num = 0;
			if (DevicePlatformIDs != null)
			{
				string[] devicePlatformIDs = DevicePlatformIDs;
				foreach (string text in devicePlatformIDs)
				{
					num += text.Length + 1;
				}
			}
			if ((long)num > 191L)
			{
				_logger.LogError("ImageCommon!ImageGeneratorParameters::VerifyInputParameters: parameter DevicePlatformID larger than {0}.", 192u.ToString());
				return false;
			}
			InputPartition[] partitions;
			foreach (InputStore store in Stores)
			{
				partitions = store.Partitions;
				foreach (InputPartition inputPartition in partitions)
				{
					if (dictionary.ContainsKey(inputPartition.Name))
					{
						_logger.LogError("ImageCommon!ImageGeneratorParameters::VerifyInputParameters: A partition '" + inputPartition.Name + "' is defined twice in the DeviceLayout.");
						return false;
					}
					dictionary.Add(inputPartition.Name, "Partitions");
				}
			}
			InputPartition inputPartition2 = null;
			partitions = MainOSStore.Partitions;
			foreach (InputPartition inputPartition3 in partitions)
			{
				if (inputPartition2 != null)
				{
					_logger.LogError("ImageCommon!ImageGeneratorParameters::VerifyInputParameters: Partitions that specify UseAllSpace must be at the end.  See partition '{0}' and '{1}' for conflict.", inputPartition2.Name, inputPartition3.Name);
					return false;
				}
				if (inputPartition3.UseAllSpace)
				{
					inputPartition2 = inputPartition3;
					if (inputPartition3.TotalSectors != 0)
					{
						_logger.LogError("ImageCommon!ImageGeneratorParameters::VerifyInputParameters: A partition cannot use all available space and have total sectors set.  See partition " + inputPartition3.Name);
						return false;
					}
				}
				if (inputPartition3.ByteAlignment != 0)
				{
					if (inputPartition3.SingleSectorAlignment && inputPartition3.ByteAlignment != SectorSize)
					{
						_logger.LogError("ImageCommon!ImageGeneratorParameters::VerifyInputParameters: Partition '{0}' has both a byte alignment and SingleSectorAlignment set.", inputPartition3.Name);
						return false;
					}
					if (!InputHelpers.IsPowerOfTwo(inputPartition3.ByteAlignment))
					{
						_logger.LogError("ImageCommon!ImageGeneratorParameters::VerifyInputParameters: The byte alignment for partition '{0}' must be a multiple of 2: {1} bytes.", inputPartition3.Name, inputPartition3.ByteAlignment);
						return false;
					}
				}
				if (inputPartition3.SingleSectorAlignment)
				{
					inputPartition3.ByteAlignment = SectorSize;
				}
				if (!string.IsNullOrEmpty(inputPartition3.PrimaryPartition) && FindPartition(inputPartition3.PrimaryPartition) == null)
				{
					_logger.LogError($"ImageCommon!ImageGeneratorParameters::VerifyInputParameters: The primary partition for partition '{inputPartition3.Name}' is not found Primary: '{inputPartition3.PrimaryPartition}'.");
					return false;
				}
			}
			if (!VerifyPartitionSizes())
			{
				return false;
			}
			return true;
		}

		private InputPartition FindPartition(string PartitionName)
		{
			Func<InputPartition, bool> func = default(Func<InputPartition, bool>);
			foreach (InputStore store in Stores)
			{
				InputPartition[] partitions = store.Partitions;
				Func<InputPartition, bool> func2 = func;
				if (func2 == null)
				{
					func2 = (func = (InputPartition x) => x.Name.Equals(PartitionName, StringComparison.OrdinalIgnoreCase));
				}
				IEnumerable<InputPartition> source = partitions.Where(func2);
				if (source.ToArray().Length != 0)
				{
					return source.First();
				}
			}
			return null;
		}

		public static bool IsDeviceLayoutV2(string DeviceLayoutXMLFile)
		{
			XPathNavigator xPathNavigator = new XPathDocument(DeviceLayoutXMLFile).CreateNavigator();
			xPathNavigator.MoveToFollowing(XPathNodeType.Element);
			return xPathNavigator.GetNamespacesInScope(XmlNamespaceScope.All).Values.Any((string x) => string.CompareOrdinal(x, "http://schemas.microsoft.com/embedded/2004/10/ImageUpdate/v2") == 0);
		}

		public static Stream GetDeviceLayoutXSD(string deviceLayoutXMLFile)
		{
			if (IsDeviceLayoutV2(deviceLayoutXMLFile))
			{
				return GetXSDStream(DevicePaths.DeviceLayoutSchema2);
			}
			return GetXSDStream(DevicePaths.DeviceLayoutSchema);
		}

		public static Stream GetOEMDevicePlatformXSD()
		{
			return GetXSDStream(DevicePaths.OEMDevicePlatformSchema);
		}

		public static Stream GetXSDStream(string xsdID)
		{
			Assembly executingAssembly = Assembly.GetExecutingAssembly();
			string[] manifestResourceNames = executingAssembly.GetManifestResourceNames();
			string text = string.Empty;
			string[] array = manifestResourceNames;
			foreach (string text2 in array)
			{
				if (text2.Contains(xsdID))
				{
					text = text2;
					break;
				}
			}
			if (string.IsNullOrEmpty(text))
			{
				throw new ImageCommonException("ImageCommon!ImageGeneratorParameters::GetXSDStream: XSD resource was not found: " + xsdID);
			}
			return executingAssembly.GetManifestResourceStream(text);
		}

		public void ProcessInputXML(string deviceLayoutXMLFile, string oemDevicePlatformXMLFile)
		{
			OEMDevicePlatformInput oEMDevicePlatformInput = null;
			XsdValidator xsdValidator = new XsdValidator();
			try
			{
				using (Stream xsdStream = GetDeviceLayoutXSD(deviceLayoutXMLFile))
				{
					xsdValidator.ValidateXsd(xsdStream, deviceLayoutXMLFile, _logger);
				}
			}
			catch (XsdValidatorException innerException)
			{
				throw new ImageCommonException("ImageCommon!ImageGeneratorParameters::ProcessInputXML: Unable to validate Device Layout XSD.", innerException);
			}
			_logger.LogInfo("ImageCommon: Successfully validated the Device Layout XML");
			if (IsDeviceLayoutV2(deviceLayoutXMLFile))
			{
				InitializeV2DeviceLayout(deviceLayoutXMLFile);
			}
			else
			{
				InitializeV1DeviceLayout(deviceLayoutXMLFile);
			}
			xsdValidator = new XsdValidator();
			try
			{
				using (Stream xsdStream2 = GetOEMDevicePlatformXSD())
				{
					xsdValidator.ValidateXsd(xsdStream2, oemDevicePlatformXMLFile, _logger);
				}
			}
			catch (XsdValidatorException innerException2)
			{
				throw new ImageCommonException("ImageCommon!ImageGeneratorParameters::ProcessInputXML: Unable to validate OEM Device Platform XSD.", innerException2);
			}
			_logger.LogInfo("ImageCommon: Successfully validated the OEM Device Platform XML");
			XmlSerializer xmlSerializer = new XmlSerializer(typeof(OEMDevicePlatformInput));
			using (StreamReader textReader = new StreamReader(oemDevicePlatformXMLFile))
			{
				try
				{
					oEMDevicePlatformInput = (OEMDevicePlatformInput)xmlSerializer.Deserialize(textReader);
				}
				catch (Exception innerException3)
				{
					throw new ImageCommonException("ImageCommon!ImageGeneratorParameters::ProcessInputXML: Unable to parse OEM Device Platform XML.", innerException3);
				}
			}
			try
			{
				DevicePlatformIDs = oEMDevicePlatformInput.DevicePlatformIDs;
				MinSectorCount = oEMDevicePlatformInput.MinSectorCount;
				foreach (InputStore store in Stores)
				{
					foreach (InputPartition item in store.Partitions.Where((InputPartition x) => !string.IsNullOrEmpty(x.FileSystem) && x.FileSystem.Equals("NTFS", StringComparison.OrdinalIgnoreCase)))
					{
						item.Compressed = true;
					}
				}
				string[] array = oEMDevicePlatformInput.UncompressedPartitions ?? new string[0];
				foreach (string text in array)
				{
					InputPartition inputPartition = FindPartition(text);
					if (inputPartition == null)
					{
						throw new ImageCommonException("Partition " + text + " was marked in the OEMDevicePlatform as uncompressed, but the partition doesn't exist in the device layout. Please ensure the spelling of the partition is correct in OEMDevicePlatform and that the partition is defined in the OEMDeviceLayout.");
					}
					_logger.LogInfo("ImageCommon: Marking partition " + text + " uncompressed as requested by device plaform.");
					inputPartition.Compressed = false;
				}
				AddSectorsToMainOs(oEMDevicePlatformInput.AdditionalMainOSFreeSectorsRequest, oEMDevicePlatformInput.MainOSRTCDataReservedSectors);
				if (oEMDevicePlatformInput.MMOSPartitionTotalSectorsOverride != 0)
				{
					InputPartition inputPartition2 = FindPartition("MMOS");
					if (inputPartition2 == null)
					{
						throw new ImageCommonException("ImageCommon!ImageGeneratorParameters::ProcessInputXML: The OEM Device Platform XML specifies that the MMOS should have total sectors set but no MMOS partition was found.");
					}
					inputPartition2.TotalSectors = oEMDevicePlatformInput.MMOSPartitionTotalSectorsOverride;
				}
				if (oEMDevicePlatformInput.Rules != null)
				{
					Rules = oEMDevicePlatformInput.Rules;
				}
			}
			catch (ImageCommonException)
			{
				throw;
			}
			catch (Exception innerException4)
			{
				throw new ImageCommonException("ImageCommon!ImageGeneratorParameters::ProcessInputXML: There was a problem parsing the OEM Device Platform input", innerException4);
			}
		}

		private void InitializeV1DeviceLayout(string DeviceLayoutXMLFile)
		{
			XmlDocument xmlDocument = new XmlDocument();
			try
			{
				xmlDocument.Load(DeviceLayoutXMLFile);
			}
			catch (Exception innerException)
			{
				throw new ImageCommonException("ImageCommon!ImageGeneratorParameters::InitializeV1DeviceLayout: Unable to validate Device Layout XSD.", innerException);
			}
			DeviceLayoutInput deviceLayoutInput = null;
			using (StreamReader textReader = new StreamReader(DeviceLayoutXMLFile))
			{
				try
				{
					deviceLayoutInput = (DeviceLayoutInput)new XmlSerializer(typeof(DeviceLayoutInput)).Deserialize(textReader);
				}
				catch (Exception innerException2)
				{
					throw new ImageCommonException("ImageCommon!ImageGeneratorParameters::InitializeV1DeviceLayout: Unable to parse Device Layout XML.", innerException2);
				}
			}
			try
			{
				InputStore inputStore = new InputStore("MainOSStore");
				if (deviceLayoutInput.Partitions != null)
				{
					inputStore.Partitions = deviceLayoutInput.Partitions;
				}
				SectorSize = deviceLayoutInput.SectorSize;
				ChunkSize = deviceLayoutInput.ChunkSize;
				VirtualHardDiskSectorSize = deviceLayoutInput.SectorSize;
				DefaultPartitionByteAlignment = deviceLayoutInput.DefaultPartitionByteAlignment;
				InputPartition[] partitions = inputStore.Partitions;
				foreach (InputPartition inputPartition in partitions)
				{
					if (inputPartition.MinFreeSectors != 0)
					{
						if (inputPartition.TotalSectors != 0 || inputPartition.UseAllSpace)
						{
							throw new ImageCommonException("ImageCommon!ImageGeneratorParameters::InitializeV1DeviceLayout: MinFreeSectors cannot be set for partition '" + inputPartition.Name + "' when either TotalSectors or UseAllSpace is set.");
						}
						if (inputPartition.MinFreeSectors < 8192)
						{
							throw new ImageCommonException("ImageCommon!ImageGeneratorParameters::InitializeV1DeviceLayout: MinFreeSectors cannot be set for partition '" + inputPartition.Name + "' less than " + 8192u + " sectors.");
						}
					}
					if (inputPartition.GeneratedFileOverheadSectors != 0 && inputPartition.MinFreeSectors == 0)
					{
						throw new ImageCommonException("ImageCommon!ImageGeneratorParameters::InitializeV1DeviceLayout: GeneratedFileOverheadSectors cannot be set for partition '" + inputPartition.Name + "' without MinFreeSectors being set.");
					}
				}
				Stores.Add(inputStore);
			}
			catch (ImageCommonException)
			{
				throw;
			}
			catch (Exception innerException3)
			{
				throw new ImageCommonException("ImageCommon!ImageGeneratorParameters::InitializeV1DeviceLayout: There was a problem parsing the Device Layout input", innerException3);
			}
		}

		private void InitializeV2DeviceLayout(string DeviceLayoutXMLFile)
		{
			XmlDocument xmlDocument = new XmlDocument();
			try
			{
				xmlDocument.Load(DeviceLayoutXMLFile);
			}
			catch (Exception innerException)
			{
				throw new ImageCommonException("ImageCommon!ImageGeneratorParameters::InitializeV2DeviceLayout: Unable to validate Device Layout XSD.", innerException);
			}
			DeviceLayoutInputv2 deviceLayoutInputv = null;
			using (StreamReader textReader = new StreamReader(DeviceLayoutXMLFile))
			{
				try
				{
					deviceLayoutInputv = (DeviceLayoutInputv2)new XmlSerializer(typeof(DeviceLayoutInputv2)).Deserialize(textReader);
				}
				catch (Exception innerException2)
				{
					throw new ImageCommonException("ImageCommon!ImageGeneratorParameters::InitializeV2DeviceLayout: Unable to parse Device Layout XML.", innerException2);
				}
			}
			try
			{
				if (deviceLayoutInputv.Stores != null)
				{
					Stores = new List<InputStore>(deviceLayoutInputv.Stores);
				}
				SectorSize = deviceLayoutInputv.SectorSize;
				ChunkSize = deviceLayoutInputv.ChunkSize;
				VirtualHardDiskSectorSize = deviceLayoutInputv.SectorSize;
				DefaultPartitionByteAlignment = deviceLayoutInputv.DefaultPartitionByteAlignment;
				foreach (InputStore store in Stores)
				{
					if (store.IsMainOSStore())
					{
						if (store.SizeInSectors != 0)
						{
							throw new ImageCommonException("ImageCommon!ImageGeneratorParameters::InitializeV2DeviceLayout: SizeInSector cannot be set for MainOS store.'");
						}
					}
					else
					{
						if (string.IsNullOrEmpty(store.Id))
						{
							throw new ImageCommonException("ImageCommon!ImageGeneratorParameters::InitializeV2DeviceLayout: Id needs to be set for individual stores.'");
						}
						if (store.SizeInSectors == 0)
						{
							throw new ImageCommonException("ImageCommon!ImageGeneratorParameters::InitializeV2DeviceLayout: SizeInSector needs to be set for non-MainOS store '" + store.Id + "'.");
						}
						if ((ulong)((long)store.SizeInSectors * (long)SectorSize) < 3145728uL)
						{
							throw new ImageCommonException("ImageCommon!ImageGeneratorParameters::InitializeV2DeviceLayout: Minimum size of a store '" + store.Id + "' must be 3MB or larger.");
						}
					}
					if (string.IsNullOrEmpty(store.StoreType))
					{
						throw new ImageCommonException("ImageCommon!ImageGeneratorParameters::InitializeV2DeviceLayout: StoreType needs to be set for store '" + store.Id + "'.");
					}
					if (string.IsNullOrEmpty(store.DevicePath))
					{
						throw new ImageCommonException("ImageCommon!ImageGeneratorParameters::InitializeV2DeviceLayout: DevicePath needs to be set for store '" + store.Id + "'.");
					}
					if (store.OnlyAllocateDefinedGptEntries && store.Partitions.Count() > 32)
					{
						throw new ImageCommonException("ImageCommon!ImageGeneratorParameters::InitializeV2DeviceLayout: Cannot use shortened GPT as it has more than 32 partitions for store '" + store.Id + "'.");
					}
					InputPartition[] partitions = store.Partitions;
					foreach (InputPartition inputPartition in partitions)
					{
						if (inputPartition.MinFreeSectors != 0)
						{
							if (inputPartition.TotalSectors != 0 || inputPartition.UseAllSpace)
							{
								throw new ImageCommonException("ImageCommon!ImageGeneratorParameters::InitializeV2DeviceLayout: MinFreeSectors cannot be set for partition '" + inputPartition.Name + "' when either TotalSectors or UseAllSpace is set.");
							}
							if (inputPartition.MinFreeSectors < 8192)
							{
								throw new ImageCommonException("ImageCommon!ImageGeneratorParameters::InitializeV2DeviceLayout: MinFreeSectors cannot be set for partition '" + inputPartition.Name + "' less than " + 8192u + " sectors.");
							}
						}
						if (inputPartition.GeneratedFileOverheadSectors != 0 && inputPartition.MinFreeSectors == 0)
						{
							throw new ImageCommonException("ImageCommon!ImageGeneratorParameters::InitializeV2DeviceLayout: GeneratedFileOverheadSectors cannot be set for partition '" + inputPartition.Name + "' without MinFreeSectors being set.");
						}
					}
				}
			}
			catch (ImageCommonException)
			{
				throw;
			}
			catch (Exception innerException3)
			{
				throw new ImageCommonException("ImageCommon!ImageGeneratorParameters::InitializeV2DeviceLayout: There was a problem parsing the Device Layout input", innerException3);
			}
			_deviceLayoutVersion = 2u;
		}

		private void AddSectorsToMainOs(uint additionalFreeSectors, uint runtimeConfigurationDataSectors)
		{
			InputPartition inputPartition = FindPartition("MainOS");
			if (inputPartition == null)
			{
				throw new ImageCommonException("ImageCommon!ImageGeneratorParameters::AddSectorsToMainOs: No MainOS partition found for additional free sectors.");
			}
			if ((additionalFreeSectors != 0 || runtimeConfigurationDataSectors != 0) && inputPartition.MinFreeSectors == 0)
			{
				throw new ImageCommonException("ImageCommon!ImageGeneratorParameters::AddSectorsToMainOs: The OEM Device Platform XML specifies that the MainOS should have additional free sectors but the MainOS partition is not using MinFreeSectors.");
			}
			if (runtimeConfigurationDataSectors > 104857600u / SectorSize)
			{
				throw new ImageCommonException("ImageCommon!ImageGeneratorParameters::AddSectorsToMainOs: Runtime configuration data reservation is limited to 100MB. Please reduce the number of sectors requested in 'MainOSMVDataReservedSectors' in the OEM device platform input.");
			}
			if (additionalFreeSectors != 0)
			{
				_logger.LogInfo("OEM device platform input requested {0} additional free sectors in the MainOS partition.", additionalFreeSectors);
			}
			if (runtimeConfigurationDataSectors != 0)
			{
				_logger.LogInfo("OEM device platform input requested {0} additional sectors for runtime configuration data be reserved in the MainOS partition.", runtimeConfigurationDataSectors);
			}
			inputPartition.MinFreeSectors += additionalFreeSectors;
			inputPartition.MinFreeSectors += runtimeConfigurationDataSectors;
		}
	}
}
