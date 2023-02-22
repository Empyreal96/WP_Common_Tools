using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.WindowsPhone.ImageUpdate.Tools.Common;

namespace Microsoft.WindowsPhone.Imaging
{
	public class FullFlashUpdateImage
	{
		public struct SecurityHeader
		{
			private uint _byteCount;

			private uint _chunkSize;

			private uint _algid;

			private uint _catalogSize;

			private uint _hashTableSize;

			public uint ByteCount
			{
				get
				{
					return _byteCount;
				}
				set
				{
					_byteCount = value;
				}
			}

			public uint ChunkSize
			{
				get
				{
					return _chunkSize;
				}
				set
				{
					_chunkSize = value;
				}
			}

			public uint HashAlgorithmID
			{
				get
				{
					return _algid;
				}
				set
				{
					_algid = value;
				}
			}

			public uint CatalogSize
			{
				get
				{
					return _catalogSize;
				}
				set
				{
					_catalogSize = value;
				}
			}

			public uint HashTableSize
			{
				get
				{
					return _hashTableSize;
				}
				set
				{
					_hashTableSize = value;
				}
			}

			public static bool ValidateSignature(byte[] signature)
			{
				byte[] securitySignature = FullFlashUpdateHeaders.GetSecuritySignature();
				for (int i = 0; i < securitySignature.Length; i++)
				{
					if (signature[i] != securitySignature[i])
					{
						return false;
					}
				}
				return true;
			}
		}

		public struct ImageHeader
		{
			private uint _byteCount;

			private uint _manifestLength;

			private uint _chunkSize;

			public uint ByteCount
			{
				get
				{
					return _byteCount;
				}
				set
				{
					_byteCount = value;
				}
			}

			public uint ManifestLength
			{
				get
				{
					return _manifestLength;
				}
				set
				{
					_manifestLength = value;
				}
			}

			public uint ChunkSize
			{
				get
				{
					return _chunkSize;
				}
				set
				{
					_chunkSize = value;
				}
			}

			public static bool ValidateSignature(byte[] signature)
			{
				byte[] imageSignature = FullFlashUpdateHeaders.GetImageSignature();
				for (int i = 0; i < imageSignature.Length; i++)
				{
					if (signature[i] != imageSignature[i])
					{
						return false;
					}
				}
				return true;
			}
		}

		public class FullFlashUpdatePartition
		{
			private uint _sectorsInUse;

			private uint _totalSectors;

			private string _type;

			private string _id;

			private string _name;

			private FullFlashUpdateStore _store;

			private bool _useAllSpace;

			private string _fileSystem;

			private uint _byteAlignment;

			private uint _clusterSize;

			public string Name
			{
				get
				{
					return _name;
				}
				set
				{
					_name = value;
				}
			}

			public uint TotalSectors
			{
				get
				{
					return _totalSectors;
				}
				set
				{
					_totalSectors = value;
				}
			}

			public string PartitionType
			{
				get
				{
					return _type;
				}
				set
				{
					_type = value;
				}
			}

			public string PartitionId
			{
				get
				{
					return _id;
				}
				set
				{
					_id = value;
				}
			}

			public bool Bootable { get; set; }

			public bool ReadOnly { get; set; }

			public bool Hidden { get; set; }

			public bool AttachDriveLetter { get; set; }

			public string PrimaryPartition { get; set; }

			public bool Contiguous => true;

			public string FileSystem
			{
				get
				{
					return _fileSystem;
				}
				set
				{
					_fileSystem = value;
				}
			}

			public uint ByteAlignment
			{
				get
				{
					return _byteAlignment;
				}
				set
				{
					if (InputHelpers.IsPowerOfTwo(value))
					{
						_byteAlignment = value;
					}
				}
			}

			public uint ClusterSize
			{
				get
				{
					return _clusterSize;
				}
				set
				{
					if (InputHelpers.IsPowerOfTwo(value))
					{
						_clusterSize = value;
					}
				}
			}

			public uint LastUsedSector
			{
				get
				{
					if (_sectorsInUse != 0)
					{
						return _sectorsInUse - 1;
					}
					return 0u;
				}
			}

			public uint SectorsInUse
			{
				get
				{
					return _sectorsInUse;
				}
				set
				{
					_sectorsInUse = value;
				}
			}

			public bool UseAllSpace
			{
				get
				{
					return _useAllSpace;
				}
				set
				{
					_useAllSpace = value;
				}
			}

			public bool RequiredToFlash { get; set; }

			public uint SectorAlignment { get; set; }

			public void Initialize(uint usedSectors, uint totalSectors, string partitionType, string partitionId, string name, FullFlashUpdateStore store, bool useAllSpace)
			{
				Initialize(usedSectors, totalSectors, partitionType, partitionId, name, store, useAllSpace, false);
			}

			public void Initialize(uint usedSectors, uint totalSectors, string partitionType, string partitionId, string name, FullFlashUpdateStore store, bool useAllSpace, bool isDesktopImage)
			{
				_sectorsInUse = usedSectors;
				_totalSectors = totalSectors;
				_type = partitionType;
				_id = partitionId;
				_name = name;
				_store = store;
				_useAllSpace = useAllSpace;
				if (!isDesktopImage && _useAllSpace && !name.Equals("Data", StringComparison.InvariantCultureIgnoreCase))
				{
					throw new ImageCommonException($"ImageCommon!FullFlashUpdatePartition::Initialize: Partition {_name} cannot specify UseAllSpace.");
				}
				if (_totalSectors == uint.MaxValue)
				{
					throw new ImageCommonException("ImageCommon!FullFlashUpdatePartition::Initialize: Partition " + name + " is too large (" + _totalSectors + " sectors)");
				}
				ReadOnly = false;
				Bootable = false;
				Hidden = false;
				AttachDriveLetter = false;
				RequiredToFlash = false;
				SectorAlignment = 0u;
				_fileSystem = string.Empty;
				_byteAlignment = 0u;
				_clusterSize = 0u;
			}

			public void ToCategory(ManifestCategory category)
			{
				category.Clean();
				category["Name"] = _name;
				category["Type"] = _type;
				if (!string.IsNullOrEmpty(_id))
				{
					category["Id"] = _id;
				}
				category["Primary"] = PrimaryPartition;
				if (!string.IsNullOrEmpty(_fileSystem))
				{
					category["FileSystem"] = _fileSystem;
				}
				if (ReadOnly)
				{
					category["ReadOnly"] = ReadOnly.ToString(CultureInfo.InvariantCulture);
				}
				if (Hidden)
				{
					category["Hidden"] = Hidden.ToString(CultureInfo.InvariantCulture);
				}
				if (AttachDriveLetter)
				{
					category["AttachDriveLetter"] = AttachDriveLetter.ToString(CultureInfo.InvariantCulture);
				}
				if (Bootable)
				{
					category["Bootable"] = Bootable.ToString(CultureInfo.InvariantCulture);
				}
				if (_useAllSpace)
				{
					category["UseAllSpace"] = "true";
				}
				else
				{
					category["TotalSectors"] = _totalSectors.ToString(CultureInfo.InvariantCulture);
					category["UsedSectors"] = _sectorsInUse.ToString(CultureInfo.InvariantCulture);
				}
				if (_byteAlignment != 0)
				{
					category["ByteAlignment"] = _byteAlignment.ToString(CultureInfo.InvariantCulture);
				}
				if (_clusterSize != 0)
				{
					category["ClusterSize"] = _clusterSize.ToString(CultureInfo.InvariantCulture);
				}
				if (SectorAlignment != 0)
				{
					category["SectorAlignment"] = SectorAlignment.ToString(CultureInfo.InvariantCulture);
				}
				if (RequiredToFlash)
				{
					category["RequiredToFlash"] = RequiredToFlash.ToString(CultureInfo.InvariantCulture);
				}
			}

			public override string ToString()
			{
				return Name;
			}
		}

		public class FullFlashUpdateStore : IDisposable
		{
			private List<FullFlashUpdatePartition> _partitions = new List<FullFlashUpdatePartition>();

			private FullFlashUpdateImage _image;

			private string _storeId;

			private bool _isMainOSStore;

			private string _devicePath;

			private bool _onlyAllocateDefinedGptEntries;

			private uint _minSectorCount;

			private uint _sectorSize;

			private uint _sectorsUsed;

			private string _tempBackingStoreFile = string.Empty;

			private string _tempBackingStorePath = string.Empty;

			private bool _alreadyDisposed;

			public FullFlashUpdateImage Image => _image;

			public string Id => _storeId;

			public bool IsMainOSStore => _isMainOSStore;

			public string DevicePath => _devicePath;

			public bool OnlyAllocateDefinedGptEntries => _onlyAllocateDefinedGptEntries;

			public uint SectorCount
			{
				get
				{
					return _minSectorCount;
				}
				set
				{
					_minSectorCount = value;
				}
			}

			public uint MinSectorCount
			{
				get
				{
					return _minSectorCount;
				}
				set
				{
					_minSectorCount = value;
				}
			}

			public uint SectorSize
			{
				get
				{
					return _sectorSize;
				}
				set
				{
					_sectorSize = value;
				}
			}

			public int PartitionCount => _partitions.Count;

			public List<FullFlashUpdatePartition> Partitions => _partitions;

			public FullFlashUpdatePartition this[string name]
			{
				get
				{
					foreach (FullFlashUpdatePartition partition in _partitions)
					{
						if (string.CompareOrdinal(partition.Name, name) == 0)
						{
							return partition;
						}
					}
					return null;
				}
			}

			public string BackingFile => _tempBackingStoreFile;

			~FullFlashUpdateStore()
			{
				Dispose(false);
			}

			public void Dispose()
			{
				Dispose(true);
				GC.SuppressFinalize(this);
			}

			protected virtual void Dispose(bool isDisposing)
			{
				if (_alreadyDisposed)
				{
					return;
				}
				if (isDisposing)
				{
					_partitions = null;
				}
				if (File.Exists(_tempBackingStoreFile))
				{
					try
					{
						File.Delete(_tempBackingStoreFile);
					}
					catch (Exception ex)
					{
						Console.WriteLine("Warning: ImageCommon!Dispose: Failed to delete temporary backing store '" + _tempBackingStoreFile + "' with exception: " + ex.Message);
					}
				}
				if (Directory.Exists(_tempBackingStorePath))
				{
					try
					{
						Directory.Delete(_tempBackingStorePath, true);
					}
					catch (Exception ex2)
					{
						Console.WriteLine("Warning: ImageCommon!Dispose: Failed to delete temporary backing store directory '" + _tempBackingStorePath + "' with exception: " + ex2.Message);
					}
				}
				_alreadyDisposed = true;
			}

			public void Initialize(FullFlashUpdateImage image, string storeId, bool isMainOSStore, string devicePath, bool onlyAllocateDefinedGptEntries, uint minSectorCount, uint sectorSize)
			{
				_tempBackingStorePath = BuildPaths.GetImagingTempPath(Directory.GetCurrentDirectory());
				Directory.CreateDirectory(_tempBackingStorePath);
				_tempBackingStoreFile = FileUtils.GetTempFile(_tempBackingStorePath) + "FFUBackingStore";
				_image = image;
				_storeId = storeId;
				_isMainOSStore = isMainOSStore;
				_devicePath = devicePath;
				_onlyAllocateDefinedGptEntries = onlyAllocateDefinedGptEntries;
				_minSectorCount = minSectorCount;
				_sectorSize = sectorSize;
				_sectorsUsed = 0u;
			}

			public void AddPartition(FullFlashUpdatePartition partition)
			{
				if (this[partition.Name] != null)
				{
					throw new ImageCommonException("ImageCommon!FullFlashUpdateStore::AddPartition: Two partitions in a store have the same name (" + partition.Name + ").");
				}
				if (_isMainOSStore)
				{
					if (_minSectorCount != 0 && partition.TotalSectors > _minSectorCount)
					{
						throw new ImageCommonException("ImageCommon!FullFlashUpdateStore::AddPartition: The partition " + partition.Name + " is too large for the store.");
					}
					if (partition.UseAllSpace)
					{
						foreach (FullFlashUpdatePartition partition2 in _partitions)
						{
							if (partition2.UseAllSpace)
							{
								throw new ImageCommonException("ImageCommon!FullFlashUpdateStore::AddPartition: Two partitions in the same store have the UseAllSpace flag set.");
							}
						}
					}
					else if (partition.SectorsInUse > partition.TotalSectors)
					{
						throw new ImageCommonException("ImageCommon!FullFlashUpdateStore::AddPartition: The partition data is invalid.  There are more used sectors (" + partition.SectorsInUse + ") than total sectors (" + partition.TotalSectors + ") for partition:" + partition.Name);
					}
					if (_minSectorCount != 0)
					{
						if (partition.UseAllSpace)
						{
							_sectorsUsed++;
						}
						else
						{
							_sectorsUsed += partition.TotalSectors;
						}
						if (_sectorsUsed > _minSectorCount)
						{
							throw new ImageCommonException("ImageCommon!FullFlashUpdateStore::AddPartition: Partition (" + partition.Name + ") on the Store does not fit. SectorsUsed = " + _sectorsUsed + " > MinSectorCount = " + _minSectorCount);
						}
					}
				}
				_partitions.Add(partition);
			}

			internal void AddPartition(ManifestCategory category)
			{
				uint usedSectors = 0u;
				uint num = 0u;
				string partitionType = category["Type"];
				string text = category["Name"];
				string partitionId = category["Id"];
				bool flag = false;
				if (_isMainOSStore)
				{
					if (category["UsedSectors"] != null)
					{
						usedSectors = uint.Parse(category["UsedSectors"], CultureInfo.InvariantCulture);
					}
					if (category["TotalSectors"] != null)
					{
						num = uint.Parse(category["TotalSectors"], CultureInfo.InvariantCulture);
					}
					if (category["UseAllSpace"] != null)
					{
						flag = bool.Parse(category["UseAllSpace"]);
					}
					if (!flag && num == 0)
					{
						throw new ImageCommonException($"ImageCommon!FullFlashUpdateImage::AddPartition: The partition category for partition {text} must contain either a 'TotalSectors' or 'UseAllSpace' key/value pair.");
					}
					if (flag && num != 0)
					{
						throw new ImageCommonException($"ImageCommon!FullFlashUpdateImage::AddPartition: The partition category for partition {text} cannot contain both a 'TotalSectors' and a 'UseAllSpace' key/value pair.");
					}
				}
				FullFlashUpdatePartition fullFlashUpdatePartition = new FullFlashUpdatePartition();
				fullFlashUpdatePartition.Initialize(usedSectors, num, partitionType, partitionId, text, this, flag);
				if (category["Hidden"] != null)
				{
					fullFlashUpdatePartition.Hidden = bool.Parse(category["Hidden"]);
				}
				if (category["AttachDriveLetter"] != null)
				{
					fullFlashUpdatePartition.AttachDriveLetter = bool.Parse(category["AttachDriveLetter"]);
				}
				if (category["ReadOnly"] != null)
				{
					fullFlashUpdatePartition.ReadOnly = bool.Parse(category["ReadOnly"]);
				}
				if (category["Bootable"] != null)
				{
					fullFlashUpdatePartition.Bootable = bool.Parse(category["Bootable"]);
				}
				if (category["FileSystem"] != null)
				{
					fullFlashUpdatePartition.FileSystem = category["FileSystem"];
				}
				fullFlashUpdatePartition.PrimaryPartition = category["Primary"];
				if (category["ByteAlignment"] != null)
				{
					fullFlashUpdatePartition.ByteAlignment = uint.Parse(category["ByteAlignment"], CultureInfo.InvariantCulture);
				}
				if (category["ClusterSize"] != null)
				{
					fullFlashUpdatePartition.ClusterSize = uint.Parse(category["ClusterSize"], CultureInfo.InvariantCulture);
				}
				if (category["SectorAlignment"] != null)
				{
					fullFlashUpdatePartition.SectorAlignment = uint.Parse(category["SectorAlignment"], CultureInfo.InvariantCulture);
				}
				if (category["RequiredToFlash"] != null)
				{
					fullFlashUpdatePartition.RequiredToFlash = bool.Parse(category["RequiredToFlash"]);
				}
				AddPartition(fullFlashUpdatePartition);
			}

			public void TransferLocation(Stream sourceStream, Stream destinationStream)
			{
				byte[] array = new byte[1048576];
				long num = sourceStream.Length - sourceStream.Position;
				while (num > 0)
				{
					int num2 = (int)Math.Min(num, array.Length);
					sourceStream.Read(array, 0, num2);
					destinationStream.Write(array, 0, num2);
					num -= num2;
				}
			}

			public void ToCategory(ManifestCategory category)
			{
				category["SectorSize"] = _sectorSize.ToString(CultureInfo.InvariantCulture);
				if (_minSectorCount != 0)
				{
					category["MinSectorCount"] = _minSectorCount.ToString(CultureInfo.InvariantCulture);
				}
				if (!string.IsNullOrEmpty(_storeId))
				{
					category["StoreId"] = _storeId;
				}
				category["IsMainOSStore"] = _isMainOSStore.ToString(CultureInfo.InvariantCulture);
				if (!string.IsNullOrEmpty(_devicePath))
				{
					category["DevicePath"] = _devicePath;
				}
				category["OnlyAllocateDefinedGptEntries"] = _onlyAllocateDefinedGptEntries.ToString(CultureInfo.InvariantCulture);
			}
		}

		public class ManifestCategory
		{
			private string _name = string.Empty;

			private string _category = string.Empty;

			private int _maxKeySize;

			private Hashtable _keyValues = new Hashtable();

			public string this[string name]
			{
				get
				{
					return (string)_keyValues[name];
				}
				set
				{
					if (_keyValues.ContainsKey(name))
					{
						_keyValues[name] = value;
						return;
					}
					if (name.Length > _maxKeySize)
					{
						_maxKeySize = name.Length;
					}
					_keyValues.Add(name, value);
				}
			}

			public string Category
			{
				get
				{
					return _category;
				}
				set
				{
					_category = value;
				}
			}

			public string Name => _name;

			public void RemoveNameValue(string name)
			{
				if (_keyValues.ContainsKey(name))
				{
					_keyValues.Remove(name);
				}
			}

			public ManifestCategory(string name)
			{
				_name = name;
			}

			public ManifestCategory(string name, string categoryValue)
			{
				_name = name;
				_category = categoryValue;
			}

			public void WriteToStream(Stream targetStream)
			{
				ASCIIEncoding aSCIIEncoding = new ASCIIEncoding();
				byte[] bytes = aSCIIEncoding.GetBytes("[" + _category + "]\r\n");
				targetStream.Write(bytes, 0, bytes.Count());
				foreach (DictionaryEntry keyValue in _keyValues)
				{
					string text = keyValue.Key as string;
					bytes = aSCIIEncoding.GetBytes(text);
					targetStream.Write(bytes, 0, bytes.Count());
					for (int i = 0; i < _maxKeySize + 1 - text.Length; i++)
					{
						targetStream.Write(aSCIIEncoding.GetBytes(" "), 0, 1);
					}
					bytes = aSCIIEncoding.GetBytes(string.Concat("= ", _keyValues[text], "\r\n"));
					targetStream.Write(bytes, 0, bytes.Count());
				}
				targetStream.Write(aSCIIEncoding.GetBytes("\r\n"), 0, 2);
			}

			public void WriteToFile(TextWriter targetStream)
			{
				targetStream.WriteLine("[{0}]", _category);
				foreach (DictionaryEntry keyValue in _keyValues)
				{
					string text = keyValue.Key as string;
					targetStream.Write("{0}", text);
					for (int i = 0; i < _maxKeySize + 1 - text.Length; i++)
					{
						targetStream.Write(" ");
					}
					targetStream.WriteLine("= {0}", _keyValues[text]);
				}
				targetStream.WriteLine("");
			}

			public void Clean()
			{
				_keyValues.Clear();
			}
		}

		public class FullFlashUpdateManifest
		{
			private ArrayList _categories = new ArrayList(20);

			private FullFlashUpdateImage _image;

			public ManifestCategory this[string categoryName]
			{
				get
				{
					foreach (ManifestCategory category in _categories)
					{
						if (string.Compare(category.Name, categoryName, StringComparison.Ordinal) == 0)
						{
							return category;
						}
					}
					return null;
				}
			}

			public uint Length
			{
				get
				{
					MemoryStream memoryStream = new MemoryStream();
					WriteToStream(memoryStream);
					return (uint)memoryStream.Position;
				}
			}

			public FullFlashUpdateManifest(FullFlashUpdateImage image)
			{
				_image = image;
			}

			public FullFlashUpdateManifest(FullFlashUpdateImage image, StreamReader manifestStream)
			{
				Regex regex = new Regex("^\\s*\\[(?<category>[^\\]]+)\\]\\s*$");
				Regex regex2 = new Regex("^\\s*(?<key>[^=\\s]+)\\s*=\\s*(?<value>.*)(\\s*$)");
				Match match = null;
				_image = image;
				ManifestCategory manifestCategory = null;
				while (!manifestStream.EndOfStream)
				{
					string text = manifestStream.ReadLine();
					if (regex.IsMatch(text))
					{
						match = null;
						string value = regex.Match(text).Groups["category"].Value;
						ProcessCategory(manifestCategory);
						manifestCategory = null;
						if (string.Compare(value, "Store", StringComparison.Ordinal) == 0)
						{
							manifestCategory = new ManifestCategory("Store", "Store");
						}
						else if (string.Compare(value, "Partition", StringComparison.Ordinal) == 0)
						{
							manifestCategory = new ManifestCategory("Partition", "Partition");
						}
						else
						{
							manifestCategory = AddCategory(value, value);
						}
					}
					else if (manifestCategory != null && regex2.IsMatch(text))
					{
						match = null;
						Match match2 = regex2.Match(text);
						manifestCategory[match2.Groups["key"].Value] = match2.Groups["value"].Value;
						if (match2.Groups["key"].ToString() == "Description")
						{
							match = match2;
						}
					}
					else if (match != null)
					{
						ManifestCategory manifestCategory2 = manifestCategory;
						string value2 = match.Groups["key"].Value;
						manifestCategory2[value2] = manifestCategory2[value2] + Environment.NewLine + text;
					}
				}
				ProcessCategory(manifestCategory);
				manifestCategory = null;
			}

			private void ProcessCategory(ManifestCategory category)
			{
				if (category != null)
				{
					if (string.CompareOrdinal(category.Name, "Store") == 0)
					{
						_image.AddStore(category);
						category = null;
					}
					else if (string.CompareOrdinal(category.Name, "Partition") == 0)
					{
						_image.Stores.Last().AddPartition(category);
						category = null;
					}
				}
			}

			public ManifestCategory AddCategory(string name, string categoryValue)
			{
				if (this[name] != null)
				{
					throw new ImageCommonException("ImageCommon!FullFlashUpdateManifest::AddCategory: Cannot add duplicate categories to a manifest.");
				}
				ManifestCategory manifestCategory = new ManifestCategory(name, categoryValue);
				_categories.Add(manifestCategory);
				return manifestCategory;
			}

			public void RemoveCategory(string name)
			{
				if (this[name] != null)
				{
					ManifestCategory obj = this[name];
					_categories.Remove(obj);
				}
			}

			public void WriteToStream(Stream targetStream)
			{
				foreach (ManifestCategory category in _categories)
				{
					category.WriteToStream(targetStream);
				}
				foreach (FullFlashUpdateStore store in _image.Stores)
				{
					ManifestCategory manifestCategory = new ManifestCategory("Store", "Store");
					store.ToCategory(manifestCategory);
					manifestCategory.WriteToStream(targetStream);
					foreach (FullFlashUpdatePartition partition in store.Partitions)
					{
						ManifestCategory manifestCategory2 = new ManifestCategory("Partition", "Partition");
						partition.ToCategory(manifestCategory2);
						manifestCategory2.WriteToStream(targetStream);
					}
				}
			}

			public void WriteToFile(string fileName)
			{
				try
				{
					if (File.Exists(fileName))
					{
						File.Delete(fileName);
					}
				}
				catch (Exception innerException)
				{
					throw new ImageCommonException("ImageCommon!FullFlashUpdateManifest::WriteToFile: Unable to delete the existing image file", innerException);
				}
				StreamWriter streamWriter = File.CreateText(fileName);
				WriteToStream(streamWriter.BaseStream);
				streamWriter.Close();
			}
		}

		private FullFlashUpdateManifest _manifest;

		private const uint _OneKiloByte = 1024u;

		private const string _version = "2.0";

		private const uint _DefaultPartitionByteAlignment = 65536u;

		private List<FullFlashUpdateStore> _stores = new List<FullFlashUpdateStore>();

		private string _imagePath;

		private long _payloadOffset;

		private ImageHeader _imageHeader;

		private SecurityHeader _securityHeader;

		private byte[] _catalogData;

		private byte[] _hashTableData;

		private uint _defaultPartititionByteAlignment = 65536u;

		public static readonly uint PartitionTypeMbr = 0u;

		public static readonly uint PartitionTypeGpt = 1u;

		public SecurityHeader GetSecureHeader => _securityHeader;

		public static int SecureHeaderSize => Marshal.SizeOf((object)default(SecurityHeader));

		public byte[] CatalogData
		{
			get
			{
				return _catalogData;
			}
			set
			{
				_catalogData = value;
			}
		}

		public byte[] HashTableData
		{
			get
			{
				return _hashTableData;
			}
			set
			{
				_hashTableData = value;
			}
		}

		public ImageHeader GetImageHeader => _imageHeader;

		public static int ImageHeaderSize => Marshal.SizeOf((object)default(ImageHeader));

		public uint ChunkSize
		{
			get
			{
				return _imageHeader.ChunkSize;
			}
			set
			{
				_imageHeader.ChunkSize = value;
			}
		}

		public uint ChunkSizeInBytes => ChunkSize * 1024;

		public uint HashAlgorithmID
		{
			get
			{
				return _securityHeader.HashAlgorithmID;
			}
			set
			{
				_securityHeader.HashAlgorithmID = value;
			}
		}

		public uint ManifestLength
		{
			get
			{
				return _imageHeader.ManifestLength;
			}
			set
			{
				_imageHeader.ManifestLength = value;
			}
		}

		public int StoreCount => _stores.Count;

		public List<FullFlashUpdateStore> Stores => _stores;

		public uint ImageStyle
		{
			get
			{
				bool flag = true;
				if (Stores[0].Partitions != null && Stores[0].Partitions.Count() > 0)
				{
					flag = IsGPTPartitionType(Stores[0].Partitions[0].PartitionType);
				}
				if (!flag)
				{
					return PartitionTypeMbr;
				}
				return PartitionTypeGpt;
			}
		}

		public FullFlashUpdatePartition this[string name]
		{
			get
			{
				foreach (FullFlashUpdateStore store in Stores)
				{
					foreach (FullFlashUpdatePartition partition in store.Partitions)
					{
						if (string.CompareOrdinal(partition.Name, name) == 0)
						{
							return partition;
						}
					}
				}
				return null;
			}
		}

		public uint StartOfImageHeader
		{
			get
			{
				uint num = 0u;
				if (_manifest != null)
				{
					num += FullFlashUpdateHeaders.SecurityHeaderSize;
					num += _securityHeader.CatalogSize;
					num += _securityHeader.HashTableSize;
				}
				return num;
			}
		}

		public FullFlashUpdateManifest GetManifest => _manifest;

		public uint DefaultPartitionAlignmentInBytes
		{
			get
			{
				return _defaultPartititionByteAlignment;
			}
			set
			{
				if (InputHelpers.IsPowerOfTwo(value))
				{
					_defaultPartititionByteAlignment = value;
				}
			}
		}

		public uint SecurityPadding
		{
			get
			{
				uint num = 1024u;
				if (_imageHeader.ChunkSize != 0)
				{
					num *= _imageHeader.ChunkSize;
				}
				else
				{
					if (_securityHeader.ChunkSize == 0)
					{
						throw new ImageCommonException("ImageCommon!FullFlashUpdateImage::SecurityPadding: Neither the of the headers have been initialized with a chunk size.");
					}
					num *= _securityHeader.ChunkSize;
				}
				return CalculateAlignment((uint)((int)FullFlashUpdateHeaders.SecurityHeaderSize + ((CatalogData != null) ? CatalogData.Length : 0) + ((HashTableData != null) ? HashTableData.Length : 0)), num);
			}
		}

		public string Description
		{
			get
			{
				if (_manifest != null && _manifest["FullFlash"] != null && _manifest["FullFlash"]["Description"] != null)
				{
					return _manifest["FullFlash"]["Description"];
				}
				return "";
			}
			set
			{
				if (_manifest != null)
				{
					if (_manifest["FullFlash"] == null)
					{
						_manifest.AddCategory("FullFlash", "FullFlash");
					}
					_manifest["FullFlash"]["Description"] = value;
				}
			}
		}

		public List<string> DevicePlatformIDs
		{
			get
			{
				List<string> list = new List<string>();
				if (_manifest == null || _manifest["FullFlash"] == null)
				{
					return list;
				}
				int num = 0;
				num = 0;
				while (true)
				{
					string name = $"DevicePlatformId{num}";
					if (_manifest["FullFlash"][name] == null)
					{
						break;
					}
					num++;
				}
				for (int i = 0; i < num; i++)
				{
					string name2 = $"DevicePlatformId{i}";
					list.Add(_manifest["FullFlash"][name2]);
				}
				return list;
			}
			set
			{
				if (_manifest != null)
				{
					if (_manifest["FullFlash"] == null)
					{
						_manifest.AddCategory("FullFlash", "FullFlash");
					}
					for (int i = 0; i < value.Count; i++)
					{
						string name = $"DevicePlatformId{i}";
						_manifest["FullFlash"][name] = value[i];
					}
				}
			}
		}

		public string Version
		{
			get
			{
				if (_manifest != null && _manifest["FullFlash"] != null && _manifest["FullFlash"]["Version"] != null)
				{
					return _manifest["FullFlash"]["Version"];
				}
				return string.Empty;
			}
			private set
			{
				if (_manifest != null)
				{
					if (_manifest["FullFlash"] == null)
					{
						_manifest.AddCategory("FullFlash", "FullFlash");
					}
					_manifest["FullFlash"]["Version"] = value;
				}
			}
		}

		public string OSVersion
		{
			get
			{
				if (_manifest != null && _manifest["FullFlash"] != null && _manifest["FullFlash"]["OSVersion"] != null)
				{
					return _manifest["FullFlash"]["OSVersion"];
				}
				return string.Empty;
			}
			set
			{
				if (_manifest != null)
				{
					if (_manifest["FullFlash"] == null)
					{
						_manifest.AddCategory("FullFlash", "FullFlash");
					}
					_manifest["FullFlash"]["OSVersion"] = value;
				}
			}
		}

		public string CanFlashToRemovableMedia
		{
			get
			{
				if (_manifest != null && _manifest["FullFlash"] != null && _manifest["FullFlash"]["CanFlashToRemovableMedia"] != null)
				{
					return _manifest["FullFlash"]["CanFlashToRemovableMedia"];
				}
				return string.Empty;
			}
			set
			{
				if (_manifest != null)
				{
					if (_manifest["FullFlash"] == null)
					{
						_manifest.AddCategory("FullFlash", "FullFlash");
					}
					_manifest["FullFlash"]["CanFlashToRemovableMedia"] = value;
				}
			}
		}

		public string AntiTheftVersion
		{
			get
			{
				if (_manifest != null && _manifest["FullFlash"] != null && _manifest["FullFlash"]["AntiTheftVersion"] != null)
				{
					return _manifest["FullFlash"]["AntiTheftVersion"];
				}
				return string.Empty;
			}
			set
			{
				if (_manifest != null)
				{
					if (_manifest["FullFlash"] == null)
					{
						_manifest.AddCategory("FullFlash", "FullFlash");
					}
					_manifest["FullFlash"]["AntiTheftVersion"] = value;
				}
			}
		}

		public string RulesVersion
		{
			get
			{
				if (_manifest != null && _manifest["FullFlash"] != null && _manifest["FullFlash"]["RulesVersion"] != null)
				{
					return _manifest["FullFlash"]["RulesVersion"];
				}
				return string.Empty;
			}
			set
			{
				if (_manifest != null)
				{
					if (_manifest["FullFlash"] == null)
					{
						_manifest.AddCategory("FullFlash", "FullFlash");
					}
					_manifest["FullFlash"]["RulesVersion"] = value;
				}
			}
		}

		public string RulesData
		{
			get
			{
				if (_manifest != null && _manifest["FullFlash"] != null && _manifest["FullFlash"]["RulesData"] != null)
				{
					return _manifest["FullFlash"]["RulesData"];
				}
				return string.Empty;
			}
			set
			{
				if (_manifest != null)
				{
					if (_manifest["FullFlash"] == null)
					{
						_manifest.AddCategory("FullFlash", "FullFlash");
					}
					_manifest["FullFlash"]["RulesData"] = value;
				}
			}
		}

		public void Initialize(string imagePath)
		{
			uint num = 0u;
			byte[] array = null;
			if (!File.Exists(imagePath))
			{
				throw new ImageCommonException("ImageCommon!FullFlashUpdateImage::Initialize: The FFU file '" + imagePath + "' does not exist.");
			}
			_imagePath = Path.GetFullPath(imagePath);
			using (FileStream fileStream = GetImageStream())
			{
				using (BinaryReader binaryReader = new BinaryReader(fileStream))
				{
					num = binaryReader.ReadUInt32();
					array = binaryReader.ReadBytes(12);
					if (num != FullFlashUpdateHeaders.SecurityHeaderSize || !SecurityHeader.ValidateSignature(array))
					{
						throw new ImageCommonException("ImageCommon!FullFlashUpdateImage::Initialize: Unable to load image because the security header is invalid.");
					}
					_securityHeader.ByteCount = num;
					_securityHeader.ChunkSize = binaryReader.ReadUInt32();
					_securityHeader.HashAlgorithmID = binaryReader.ReadUInt32();
					_securityHeader.CatalogSize = binaryReader.ReadUInt32();
					_securityHeader.HashTableSize = binaryReader.ReadUInt32();
					_catalogData = binaryReader.ReadBytes((int)_securityHeader.CatalogSize);
					_hashTableData = binaryReader.ReadBytes((int)_securityHeader.HashTableSize);
					binaryReader.ReadBytes((int)SecurityPadding);
					num = binaryReader.ReadUInt32();
					array = binaryReader.ReadBytes(12);
					if (num == FullFlashUpdateHeaders.ImageHeaderSize && ImageHeader.ValidateSignature(array))
					{
						_imageHeader.ByteCount = num;
						_imageHeader.ManifestLength = binaryReader.ReadUInt32();
						_imageHeader.ChunkSize = binaryReader.ReadUInt32();
						StreamReader streamReader = new StreamReader(new MemoryStream(binaryReader.ReadBytes((int)_imageHeader.ManifestLength)), Encoding.ASCII);
						try
						{
							_manifest = new FullFlashUpdateManifest(this, streamReader);
						}
						finally
						{
							streamReader.Close();
							streamReader = null;
						}
						ValidateManifest();
						if (_imageHeader.ChunkSize != 0)
						{
							binaryReader.ReadBytes((int)CalculateAlignment((uint)fileStream.Position, _imageHeader.ChunkSize * 1024));
						}
						_payloadOffset = fileStream.Position;
						return;
					}
					throw new ImageCommonException("ImageCommon!FullFlashUpdateImage::Initialize: Unable to load image because the image header is invalid.");
				}
			}
		}

		public FileStream GetImageStream()
		{
			FileStream fileStream = File.OpenRead(_imagePath);
			fileStream.Position = _payloadOffset;
			return fileStream;
		}

		public void AddStore(FullFlashUpdateStore store)
		{
			if (store == null)
			{
				throw new ArgumentNullException("store");
			}
			_stores.Add(store);
		}

		private void AddStore(ManifestCategory category)
		{
			uint sectorSize = uint.Parse(category["SectorSize"], CultureInfo.InvariantCulture);
			uint minSectorCount = 0u;
			if (category["MinSectorCount"] != null)
			{
				minSectorCount = uint.Parse(category["MinSectorCount"], CultureInfo.InvariantCulture);
			}
			string storeId = null;
			if (category["StoreId"] != null)
			{
				storeId = category["StoreId"];
			}
			bool isMainOSStore = true;
			if (category["IsMainOSStore"] != null)
			{
				isMainOSStore = bool.Parse(category["IsMainOSStore"]);
			}
			string devicePath = null;
			if (category["DevicePath"] != null)
			{
				devicePath = category["DevicePath"];
			}
			bool onlyAllocateDefinedGptEntries = false;
			if (category["OnlyAllocateDefinedGptEntries"] != null)
			{
				onlyAllocateDefinedGptEntries = bool.Parse(category["OnlyAllocateDefinedGptEntries"]);
			}
			FullFlashUpdateStore fullFlashUpdateStore = new FullFlashUpdateStore();
			fullFlashUpdateStore.Initialize(this, storeId, isMainOSStore, devicePath, onlyAllocateDefinedGptEntries, minSectorCount, sectorSize);
			_stores.Add(fullFlashUpdateStore);
		}

		public static bool IsGPTPartitionType(string partitionType)
		{
			Guid result;
			return Guid.TryParse(partitionType, out result);
		}

		public void DisplayImageInformation(IULogger logger)
		{
			foreach (string devicePlatformID in DevicePlatformIDs)
			{
				logger.LogInfo("\tDevice Platform ID: {0}", devicePlatformID);
			}
			logger.LogInfo("\tChunk Size: 0x{0:X}", ChunkSize);
			logger.LogInfo(" ");
			foreach (FullFlashUpdateStore store in Stores)
			{
				logger.LogInfo("Store");
				logger.LogInfo("\tSector Size: 0x{0:X}", store.SectorSize);
				logger.LogInfo("\tID: {0}", store.Id);
				logger.LogInfo("\tDevice Path: {0}", store.DevicePath);
				logger.LogInfo("\tContains MainOS: {0}", store.IsMainOSStore);
				if (store.IsMainOSStore)
				{
					logger.LogInfo("\tMinimum Sector Count: 0x{0:X}", store.SectorCount);
				}
				logger.LogInfo(" ");
				logger.LogInfo("There are {0} partitions in the store.", store.Partitions.Count);
				logger.LogInfo(" ");
				foreach (FullFlashUpdatePartition partition in store.Partitions)
				{
					logger.LogInfo("\tPartition");
					logger.LogInfo("\t\tName: {0}", partition.Name);
					logger.LogInfo("\t\tPartition Type: {0}", partition.PartitionType);
					logger.LogInfo("\t\tTotal Sectors: 0x{0:X}", partition.TotalSectors);
					logger.LogInfo("\t\tSectors In Use: 0x{0:X}", partition.SectorsInUse);
					logger.LogInfo(" ");
				}
			}
		}

		private uint CalculateAlignment(uint currentPosition, uint blockSize)
		{
			uint result = 0u;
			uint num = currentPosition % blockSize;
			if (num != 0)
			{
				result = blockSize - num;
			}
			return result;
		}

		public byte[] GetSecurityHeader(byte[] catalogData, byte[] hashData)
		{
			MemoryStream memoryStream = new MemoryStream();
			BinaryWriter binaryWriter = new BinaryWriter(memoryStream);
			binaryWriter.Write(FullFlashUpdateHeaders.SecurityHeaderSize);
			binaryWriter.Write(FullFlashUpdateHeaders.GetSecuritySignature());
			binaryWriter.Write(ChunkSize);
			binaryWriter.Write(HashAlgorithmID);
			binaryWriter.Write(catalogData.Length);
			binaryWriter.Write(hashData.Length);
			binaryWriter.Write(catalogData);
			binaryWriter.Write(hashData);
			binaryWriter.Flush();
			if (memoryStream.Length % (long)ChunkSizeInBytes != 0L)
			{
				long num = ChunkSizeInBytes - memoryStream.Length % (long)ChunkSizeInBytes;
				memoryStream.SetLength(memoryStream.Length + num);
			}
			return memoryStream.ToArray();
		}

		public byte[] GetManifestRegion()
		{
			MemoryStream memoryStream = new MemoryStream();
			BinaryWriter binaryWriter = new BinaryWriter(memoryStream);
			binaryWriter.Write(FullFlashUpdateHeaders.ImageHeaderSize);
			binaryWriter.Write(FullFlashUpdateHeaders.GetImageSignature());
			binaryWriter.Write(_manifest.Length);
			binaryWriter.Write(ChunkSize);
			binaryWriter.Flush();
			_manifest.WriteToStream(memoryStream);
			if (memoryStream.Length % (long)ChunkSizeInBytes != 0L)
			{
				long num = ChunkSizeInBytes - memoryStream.Length % (long)ChunkSizeInBytes;
				memoryStream.SetLength(memoryStream.Length + num);
			}
			return memoryStream.ToArray();
		}

		public void WriteManifest(Stream stream)
		{
			_manifest.WriteToStream(stream);
		}

		private void ValidateManifest()
		{
			string text = null;
			if (_manifest["FullFlash"] == null)
			{
				throw new ImageCommonException("ImageCommon!FullFlashUpdateImage::ValidateManifest: Missing 'FullFlash' or 'Image' category in the manifest");
			}
			text = _manifest["FullFlash"]["Version"];
			if (text == null)
			{
				throw new ImageCommonException("ImageCommon!FullFlashUpdateImage::ValidateManifest: Missing 'Version' name/value pair in the 'FullFlash' category.");
			}
			if (!text.Equals("2.0", StringComparison.OrdinalIgnoreCase))
			{
				throw new ImageCommonException("ImageCommon!FullFlashUpdateImage::ValidateManifest: 'Version' value (" + text + ") does not match current version of 2.0.");
			}
		}

		public void Initialize()
		{
			_manifest = new FullFlashUpdateManifest(this);
			Version = "2.0";
		}
	}
}
