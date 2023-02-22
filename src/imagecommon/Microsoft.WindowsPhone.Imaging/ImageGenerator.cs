using System;
using System.Linq;
using System.Text;
using Microsoft.WindowsPhone.ImageUpdate.Tools.Common;

namespace Microsoft.WindowsPhone.Imaging
{
	public class ImageGenerator
	{
		private IULogger _logger;

		private ImageGeneratorParameters _parameters;

		private bool _isDesktopImage;

		private const string c_RulesVersion = "1.0";

		public void Initialize(ImageGeneratorParameters parameters, IULogger logger)
		{
			Initialize(parameters, logger, false);
		}

		public void Initialize(ImageGeneratorParameters parameters, IULogger logger, bool isDesktopImage)
		{
			_logger = logger;
			if (logger == null)
			{
				_logger = new IULogger();
			}
			_parameters = parameters;
			_isDesktopImage = isDesktopImage;
		}

		public FullFlashUpdateImage CreateFFU()
		{
			FullFlashUpdateImage fullFlashUpdateImage = new FullFlashUpdateImage();
			if (_parameters == null)
			{
				throw new ImageCommonException("ImageCommon!ImageGenerator::CreateFFU: ImageGenerator has not been initialized.");
			}
			try
			{
				_parameters.VerifyInputParameters();
				fullFlashUpdateImage.Initialize();
				fullFlashUpdateImage.Description = _parameters.Description;
				fullFlashUpdateImage.DevicePlatformIDs = _parameters.DevicePlatformIDs.ToList();
				fullFlashUpdateImage.ChunkSize = _parameters.ChunkSize;
				fullFlashUpdateImage.HashAlgorithmID = _parameters.Algid;
				fullFlashUpdateImage.DefaultPartitionAlignmentInBytes = _parameters.DefaultPartitionByteAlignment;
			}
			catch (Exception innerException)
			{
				throw new ImageCommonException("ImageCommon!ImageGenerator::CreateFFU: Failed to Initialize FFU: ", innerException);
			}
			if (_parameters.Rules != null)
			{
				StringBuilder stringBuilder = new StringBuilder();
				InputIntegerRule[] integerRules = _parameters.Rules.IntegerRules;
				foreach (InputIntegerRule inputIntegerRule in integerRules)
				{
					if ((inputIntegerRule.Min.HasValue || inputIntegerRule.Max.HasValue) && inputIntegerRule.Values != null && inputIntegerRule.Values.Length != 0)
					{
						throw new ImageCommonException("ImageCommon!ImageGenerator::CreateFFU: Cannot specify both min/max value and list at the same time");
					}
					if (!inputIntegerRule.Property.All(char.IsLetterOrDigit))
					{
						throw new ImageCommonException("ImageCommon!ImageGenerator::CreateFFU: Only alphanumerics are allowed for the rule property");
					}
					if (inputIntegerRule.Min.HasValue || inputIntegerRule.Max.HasValue)
					{
						if (inputIntegerRule.Min.HasValue && inputIntegerRule.Max.HasValue && inputIntegerRule.Min > inputIntegerRule.Max)
						{
							throw new ImageCommonException("ImageCommon!ImageGenerator::CreateFFU: Invalid min/max integer rule");
						}
						stringBuilder.AppendFormat("{0}={1}<{2},{3}>;", inputIntegerRule.Property, inputIntegerRule.ModeCharacter, (!inputIntegerRule.Min.HasValue) ? string.Empty : inputIntegerRule.Min.ToString(), (!inputIntegerRule.Max.HasValue) ? string.Empty : inputIntegerRule.Max.ToString());
						continue;
					}
					stringBuilder.AppendFormat("{0}={1}[{2}", inputIntegerRule.Property, inputIntegerRule.ModeCharacter, inputIntegerRule.Values[0]);
					foreach (ulong item in inputIntegerRule.Values.Skip(1))
					{
						stringBuilder.AppendFormat(",{0}", item);
					}
					stringBuilder.Append("];");
				}
				UnicodeEncoding unicodeEncoding = new UnicodeEncoding();
				InputStringRule[] stringRules = _parameters.Rules.StringRules;
				foreach (InputStringRule inputStringRule in stringRules)
				{
					if (!inputStringRule.Property.All(char.IsLetterOrDigit))
					{
						throw new ImageCommonException("ImageCommon!ImageGenerator::CreateFFU: Only alphanumerics are allowed for the rule property");
					}
					stringBuilder.AppendFormat("{0}={1}", inputStringRule.Property, inputStringRule.ModeCharacter);
					stringBuilder.Append("{");
					stringBuilder.Append(Convert.ToBase64String(unicodeEncoding.GetBytes(inputStringRule.Values[0])));
					foreach (string item2 in inputStringRule.Values.Skip(1))
					{
						stringBuilder.AppendFormat(",{0}", Convert.ToBase64String(unicodeEncoding.GetBytes(item2)));
					}
					stringBuilder.Append("};");
				}
				fullFlashUpdateImage.RulesVersion = "1.0";
				fullFlashUpdateImage.RulesData = stringBuilder.ToString();
			}
			try
			{
				foreach (InputStore store in _parameters.Stores)
				{
					FullFlashUpdateImage.FullFlashUpdateStore fullFlashUpdateStore = new FullFlashUpdateImage.FullFlashUpdateStore();
					uint minSectorCount = _parameters.MinSectorCount;
					if (!store.IsMainOSStore())
					{
						minSectorCount = store.SizeInSectors;
					}
					fullFlashUpdateStore.Initialize(fullFlashUpdateImage, store.Id, store.IsMainOSStore(), store.DevicePath, store.OnlyAllocateDefinedGptEntries, minSectorCount, _parameters.SectorSize);
					InputPartition[] partitions = store.Partitions;
					foreach (InputPartition inputPartition in partitions)
					{
						FullFlashUpdateImage.FullFlashUpdatePartition fullFlashUpdatePartition = new FullFlashUpdateImage.FullFlashUpdatePartition();
						fullFlashUpdatePartition.Initialize(0u, inputPartition.TotalSectors, inputPartition.Type, inputPartition.Id, inputPartition.Name, fullFlashUpdateStore, inputPartition.UseAllSpace, _isDesktopImage);
						fullFlashUpdatePartition.FileSystem = inputPartition.FileSystem;
						fullFlashUpdatePartition.Bootable = inputPartition.Bootable;
						fullFlashUpdatePartition.ReadOnly = inputPartition.ReadOnly;
						fullFlashUpdatePartition.Hidden = inputPartition.Hidden;
						fullFlashUpdatePartition.AttachDriveLetter = inputPartition.AttachDriveLetter;
						fullFlashUpdatePartition.PrimaryPartition = inputPartition.PrimaryPartition;
						fullFlashUpdatePartition.RequiredToFlash = inputPartition.RequiredToFlash;
						fullFlashUpdatePartition.ByteAlignment = inputPartition.ByteAlignment;
						fullFlashUpdatePartition.ClusterSize = inputPartition.ClusterSize;
						fullFlashUpdateStore.AddPartition(fullFlashUpdatePartition);
						if (!store.IsMainOSStore() && inputPartition.ByteAlignment == 0)
						{
							fullFlashUpdatePartition.ByteAlignment = fullFlashUpdateImage.ChunkSize * 1024;
						}
					}
					fullFlashUpdateImage.AddStore(fullFlashUpdateStore);
				}
				return fullFlashUpdateImage;
			}
			catch (Exception innerException2)
			{
				throw new ImageCommonException("ImageCommon!ImageGenerator::CreateFFU: Failed to add partitions to FFU: ", innerException2);
			}
		}
	}
}
