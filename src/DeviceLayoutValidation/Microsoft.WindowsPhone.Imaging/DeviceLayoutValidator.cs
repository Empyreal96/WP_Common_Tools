using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography.X509Certificates;
using System.Text.RegularExpressions;
using System.Xml.Serialization;
using Microsoft.WindowsPhone.FeatureAPI;
using Microsoft.WindowsPhone.ImageUpdate.PkgCommon;
using Microsoft.WindowsPhone.ImageUpdate.Tools.Common;

namespace Microsoft.WindowsPhone.Imaging
{
	public class DeviceLayoutValidator
	{
		private enum FileSignatureCertificateType
		{
			None,
			NonMicrosoftCertificate,
			MicrosoftTestCertificate,
			MicrosoftFlightCertificate,
			MicrosoftProductionCertificate
		}

		private const string ValidationAnyValueAllowed = "*ANY*";

		private const string BackupPartitionPlaceHolderName = "BACKUP_*";

		private const string ValidateRuleGetValueFromParent = "*PARENT*";

		private const string ValidateRuleAcceptDefaultPrefix = "allow_default_plus_";

		private const string UpdateTypeValueCritical = "Critical";

		private const string BackupPartitionsBookEndPosition = "BACKUP_BOOKEND";

		private const string BackupPartitionNamePrefix = "BACKUP_";

		private OEMInput _oemInput;

		private IULogger _logger;

		private string _tempDirectoryPath;

		private List<string> _deviceLayoutValidationXmlFiles = new List<string>();

		private const string DeviceLayoutValidationSchema = "DeviceLayoutValidation.xsd";

		private const string DeviceLayout = "DeviceLayout.xml";

		private const string DeviceLayoutValidation = "DeviceLayoutValidation.xml";

		private const string ReleaseTypeProduction = "Production";

		private const string ReleaseTypeTest = "Test";

		private static Dictionary<string, FileSignatureCertificateType> _certPublicKeys = new Dictionary<string, FileSignatureCertificateType>();

		public void Initialize(IPkgInfo ReferencePkg, OEMInput OemInput, IULogger Logger, string TempDirectoryPath)
		{
			_logger = Logger;
			_oemInput = OemInput;
			_tempDirectoryPath = TempDirectoryPath;
			DeviceLayoutValidatorExpressionEvaluator.Initialize(OemInput, Logger);
			ReadDeviceLayoutValidationManifest();
		}

		private void ReadDeviceLayoutValidationManifest()
		{
			string[] manifestResourceNames = Assembly.GetExecutingAssembly().GetManifestResourceNames();
			foreach (string text in manifestResourceNames)
			{
				if (Regex.IsMatch(text, "^Microsoft\\..+\\.DeviceLayoutValidation.xml$", RegexOptions.IgnoreCase))
				{
					_deviceLayoutValidationXmlFiles.Add(text);
				}
			}
			_logger.LogInfo("DeviceLayoutValidation: Successfully read the Device Layout Validation Manifest - FileCount -> {0}", _deviceLayoutValidationXmlFiles.Count);
		}

		private static int GetScopeExpressionSpecificationLevel(string scope)
		{
			int num = 0;
			string[] array = scope.Split(':');
			foreach (string text in array)
			{
				num = 10 * num;
				if (!text.Equals(".*") && !text.Equals(".+"))
				{
					num++;
				}
			}
			return num;
		}

		private string GetDeviceLayoutValidationFileInScope(string versionTag)
		{
			string result = null;
			string text = null;
			string text2 = _oemInput.CPUType + ":" + _oemInput.SV + ":" + _oemInput.SOC + ":" + _oemInput.Device + ":" + _oemInput.ReleaseType;
			string[] array = versionTag.Split('.', ':');
			foreach (string text3 in array)
			{
				text2 = text2 + ":" + text3;
			}
			Assembly executingAssembly = Assembly.GetExecutingAssembly();
			if (_deviceLayoutValidationXmlFiles != null)
			{
				XmlSerializer xmlSerializer = new XmlSerializer(typeof(DeviceLayoutValidationScope));
				int num = 0;
				foreach (string deviceLayoutValidationXmlFile in _deviceLayoutValidationXmlFiles)
				{
					DeviceLayoutValidationScope deviceLayoutValidationScope;
					using (Stream stream = executingAssembly.GetManifestResourceStream(deviceLayoutValidationXmlFile))
					{
						if (stream == null)
						{
							throw new DeviceLayoutValidationException(DeviceLayoutValidationError.UnknownInternalError, "DeviceLayoutValidation:GetDeviceLayoutValidationFileInScope - unable to open stream to resource : " + deviceLayoutValidationXmlFile);
						}
						try
						{
							deviceLayoutValidationScope = (DeviceLayoutValidationScope)xmlSerializer.Deserialize(stream);
						}
						catch (Exception inner)
						{
							throw new PackageException(inner, "DeviceLayoutValidation: ValidateDeviceLayout Unable to parse Device Layout XML.");
						}
					}
					bool flag = Regex.IsMatch(text2, deviceLayoutValidationScope.Scope, RegexOptions.IgnoreCase);
					if (flag)
					{
						if (deviceLayoutValidationScope.ExcludedScopes != null)
						{
							array = deviceLayoutValidationScope.ExcludedScopes;
							foreach (string text4 in array)
							{
								if (Regex.IsMatch(text2, text4, RegexOptions.IgnoreCase))
								{
									_logger.LogInfo("DeviceLayoutValidation: Validation file [{0}] matched for scope -> [{1}->{2} but was excluded by [{3}]", deviceLayoutValidationXmlFile, text2, deviceLayoutValidationScope.Scope, text4);
									flag = false;
									break;
								}
							}
							if (flag)
							{
								_logger.LogInfo("DeviceLayoutValidation: Validation file [{0}] matched for scope -> [{1}->{2}] and did not match any exclusions", deviceLayoutValidationXmlFile, text2, deviceLayoutValidationScope.Scope);
							}
						}
						else
						{
							_logger.LogInfo("DeviceLayoutValidation: Validation file [{0}] matched for scope -> [{1}->{2}] - no exclusions", deviceLayoutValidationXmlFile, text2, deviceLayoutValidationScope.Scope);
						}
					}
					if (flag)
					{
						int scopeExpressionSpecificationLevel = GetScopeExpressionSpecificationLevel(deviceLayoutValidationScope.Scope);
						_logger.LogInfo("DeviceLayoutValidation: Validation file [{0}] was in for scope -> [{1}->{2}:{3}]", deviceLayoutValidationXmlFile, text2, deviceLayoutValidationScope.Scope, scopeExpressionSpecificationLevel);
						if (scopeExpressionSpecificationLevel > num)
						{
							_logger.LogInfo("DeviceLayoutValidation: Validation file [{0}] was picked for scope -> [{1}->{2}]", deviceLayoutValidationXmlFile, text2, deviceLayoutValidationScope.Scope);
							text = deviceLayoutValidationXmlFile;
							num = scopeExpressionSpecificationLevel;
						}
					}
					else
					{
						_logger.LogDebug("DeviceLayoutValidation: Validation file [{0}] was NOT in for scope -> [{1}->{2}]", deviceLayoutValidationXmlFile, text2, deviceLayoutValidationScope.Scope);
					}
				}
			}
			if (text == null)
			{
				_logger.LogInfo("DeviceLayoutValidation: could not find validation file scope -> [{0}]", text2);
				return result;
			}
			_logger.LogInfo("DeviceLayoutValidation: Validation file [{0}] was picked for scope -> [{1}]", text, text2);
			result = Path.Combine(_tempDirectoryPath, text);
			using (Stream stream2 = executingAssembly.GetManifestResourceStream(text))
			{
				try
				{
					using (FileStream destination = new FileStream(result, FileMode.Create, FileAccess.Write))
					{
						stream2.CopyTo(destination);
						return result;
					}
				}
				catch (Exception inner2)
				{
					throw new PackageException(inner2, "DeviceLayoutValidation: ValidateDeviceLayout Unable to write Device Layout XML.");
				}
			}
		}

		private bool ValidateDeviceLayoutValue(bool BoolValue, string Comparator, string ComparatorDefault = "false")
		{
			if (string.IsNullOrEmpty(Comparator))
			{
				Comparator = ComparatorDefault;
			}
			if (Comparator.Equals("*ANY*", StringComparison.OrdinalIgnoreCase))
			{
				return true;
			}
			bool result;
			if (bool.TryParse(Comparator, out result))
			{
				return result == BoolValue;
			}
			if (Comparator.StartsWith("allow_default_plus_", StringComparison.OrdinalIgnoreCase))
			{
				if (!BoolValue)
				{
					_logger.LogInfo("DeviceLayoutValidation: ValidateDeviceLayoutValue succeeded since [{0}] - allows the default value !!!", Comparator);
					return true;
				}
				Comparator = Comparator.Substring("allow_default_plus_".Length);
			}
			if (DeviceLayoutValidatorExpressionEvaluator.EvaluateBooleanExpression(BoolValue.ToString(), Comparator))
			{
				return true;
			}
			return false;
		}

		private bool ValidateDeviceLayoutValue(string StringValue, string Comparator, string ComparatorDefault = "")
		{
			if (Comparator == null)
			{
				Comparator = ComparatorDefault;
			}
			if (Comparator.Equals("*ANY*", StringComparison.OrdinalIgnoreCase))
			{
				return true;
			}
			if (StringValue == null)
			{
				if (string.IsNullOrEmpty(Comparator))
				{
					return true;
				}
				if (Comparator.StartsWith("allow_default_plus_", StringComparison.OrdinalIgnoreCase))
				{
					_logger.LogInfo("DeviceLayoutValidation: ValidateDeviceLayoutValue succeeded since [{0}] - allows the default value !!!", Comparator);
					return true;
				}
				_logger.LogError("DeviceLayoutValidation: ValidateDeviceLayoutValue Failed in validation of field value [{0}] but field was empty or not specified as required !!!", Comparator);
				return false;
			}
			if (StringValue.Equals(Comparator, StringComparison.Ordinal))
			{
				return true;
			}
			if (string.IsNullOrEmpty(Comparator))
			{
				_logger.LogError("DeviceLayoutValidation: ValidateDeviceLayoutValue Failed in validation of field value [{0}] - field was NOT empty as required !!!", StringValue);
				return false;
			}
			if (Comparator.StartsWith("allow_default_plus_", StringComparison.OrdinalIgnoreCase))
			{
				if (StringValue == "")
				{
					_logger.LogInfo("DeviceLayoutValidation: ValidateDeviceLayoutValue succeeded since [{0}] - allows the default value !!!", Comparator);
					return true;
				}
				Comparator = Comparator.Substring("allow_default_plus_".Length);
			}
			DeviceLayoutValidatorExpressionEvaluator.EvaluateBooleanExpression(StringValue, Comparator);
			return true;
		}

		private bool ValidateDeviceLayoutValue(uint UintValue, string Comparator, string ComparatorDefault = "0")
		{
			if (string.IsNullOrEmpty(Comparator))
			{
				Comparator = ComparatorDefault;
			}
			if (Comparator.Equals("*ANY*", StringComparison.OrdinalIgnoreCase))
			{
				return true;
			}
			uint Value;
			if (StringToUint(Comparator, out Value))
			{
				return Value == UintValue;
			}
			if (Comparator.StartsWith("allow_default_plus_", StringComparison.OrdinalIgnoreCase))
			{
				if (UintValue == 0)
				{
					_logger.LogInfo("DeviceLayoutValidation: ValidateDeviceLayoutValue succeeded since [{0}] - allows the default value !!!", Comparator);
					return true;
				}
				Comparator = Comparator.Substring("allow_default_plus_".Length);
			}
			if (DeviceLayoutValidatorExpressionEvaluator.EvaluateBooleanExpression(UintValue.ToString(), Comparator))
			{
				return true;
			}
			return false;
		}

		private bool GetParentDeviceLayoutValue(string ValidationRuleValue, bool ParentValue, ref bool OriginalValue)
		{
			bool result = false;
			if (!string.IsNullOrEmpty(ValidationRuleValue))
			{
				if (ValidationRuleValue.Equals("*PARENT*", StringComparison.OrdinalIgnoreCase))
				{
					OriginalValue = ParentValue;
					result = true;
				}
				else
				{
					result = bool.TryParse(ValidationRuleValue, out OriginalValue);
				}
			}
			return result;
		}

		private bool GetParentDeviceLayoutValue(string ValidationRuleValue, string ParentValue, ref string OriginalValue)
		{
			bool result = false;
			if (!string.IsNullOrEmpty(ValidationRuleValue))
			{
				result = true;
				if (ValidationRuleValue.Equals("*PARENT*", StringComparison.OrdinalIgnoreCase))
				{
					OriginalValue = ParentValue;
				}
				else
				{
					OriginalValue = ValidationRuleValue;
				}
			}
			return result;
		}

		private bool GetParentDeviceLayoutValue(string ValidationRuleValue, uint ParentValue, ref uint OriginalValue)
		{
			bool result = false;
			if (!string.IsNullOrEmpty(ValidationRuleValue))
			{
				if (ValidationRuleValue.Equals("*PARENT*", StringComparison.OrdinalIgnoreCase))
				{
					OriginalValue = ParentValue;
					result = true;
				}
				else
				{
					result = StringToUint(ValidationRuleValue, out OriginalValue);
				}
			}
			return result;
		}

		public static bool StringToUint(string ValueAsString, out uint Value)
		{
			bool result = true;
			if (ValueAsString.StartsWith("0x"))
			{
				if (!uint.TryParse(ValueAsString.Substring(2, ValueAsString.Length - 2), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out Value))
				{
					result = false;
				}
			}
			else if (!uint.TryParse(ValueAsString, NumberStyles.Integer, CultureInfo.InvariantCulture, out Value))
			{
				result = false;
			}
			return result;
		}

		private bool ValidateDeviceLayoutFields(InputPartition Partition, InputValidationPartition ValidationPartition, uint LayoutSectorSize, uint RulesSectorSize)
		{
			bool result = true;
			if (LayoutSectorSize != RulesSectorSize)
			{
				_logger.LogInfo("DeviceLayoutValidation: ValidateDeviceLayoutFields LayoutSectorSize [{0}] does not match RulesSectorSize [{1}] - ALL sector sizes will be scaled by [{0}/{1}] before checking of rules !!!", LayoutSectorSize, RulesSectorSize);
			}
			if (!ValidateDeviceLayoutValue(Partition.Type, ValidationPartition.Type))
			{
				result = false;
				_logger.LogError("DeviceLayoutValidation: ValidateDeviceLayoutFields Failed in validation of partition [{0}] value for field [Type] mismatched : found [{1}] -> expected [{2}] !!!", ValidationPartition.Name, Partition.Type, ValidationPartition.Type);
			}
			if (!ValidateDeviceLayoutValue(Partition.Bootable, ValidationPartition.Bootable))
			{
				result = false;
				_logger.LogError("DeviceLayoutValidation: ValidateDeviceLayoutFields Failed in validation of partition [{0}] value for field [Bootable] mismatched : found [{1}] -> expected [{2}] !!!", ValidationPartition.Name, Partition.Bootable, ValidationPartition.Bootable);
			}
			if (!ValidateDeviceLayoutValue(Partition.ReadOnly, ValidationPartition.ReadOnly))
			{
				result = false;
				_logger.LogError("DeviceLayoutValidation: ValidateDeviceLayoutFields Failed in validation of partition [{0}] value for field [ReadOnly] mismatched : found [{1}] -> expected [{2}] !!!", ValidationPartition.Name, Partition.ReadOnly, ValidationPartition.ReadOnly);
			}
			if (!ValidateDeviceLayoutValue(Partition.Hidden, ValidationPartition.Hidden))
			{
				result = false;
				_logger.LogError("DeviceLayoutValidation: ValidateDeviceLayoutFields Failed in validation of partition [{0}] value for field [Hidden] mismatched : found [{1}] -> expected [{2}] !!!", ValidationPartition.Name, Partition.Hidden, ValidationPartition.Hidden);
			}
			if (!ValidateDeviceLayoutValue(Partition.AttachDriveLetter, ValidationPartition.AttachDriveLetter))
			{
				result = false;
				_logger.LogError("DeviceLayoutValidation: ValidateDeviceLayoutFields Failed in validation of partition [{0}] value for field [AttachDriveLetter] mismatched : found [{1}] -> expected [{2}] !!!", ValidationPartition.Name, Partition.AttachDriveLetter, ValidationPartition.AttachDriveLetter);
			}
			if (!ValidateDeviceLayoutValue(Partition.UseAllSpace, ValidationPartition.UseAllSpace))
			{
				result = false;
				_logger.LogError("DeviceLayoutValidation: ValidateDeviceLayoutFields Failed in validation of partition [{0}] value for field [UseAllSpace] mismatched : found [{1}] -> expected [{2}] !!!", ValidationPartition.Name, Partition.UseAllSpace, ValidationPartition.UseAllSpace);
			}
			if (!ValidateDeviceLayoutValue(Partition.TotalSectors * LayoutSectorSize / RulesSectorSize, ValidationPartition.TotalSectors))
			{
				result = false;
				_logger.LogError("DeviceLayoutValidation: ValidateDeviceLayoutFields Failed in validation of partition [{0}] value for field [TotalSectors] mismatched : found [{1}], scaled_to[{2}] -> expected [{3}] !!!", ValidationPartition.Name, Partition.TotalSectors, Partition.TotalSectors * LayoutSectorSize / RulesSectorSize, ValidationPartition.TotalSectors);
			}
			if (!ValidateDeviceLayoutValue(Partition.MinFreeSectors * LayoutSectorSize / RulesSectorSize, ValidationPartition.MinFreeSectors))
			{
				result = false;
				_logger.LogError("DeviceLayoutValidation: ValidateDeviceLayoutFields Failed in validation of partition [{0}] value for field [MinFreeSectors] mismatched : found [{1}], scaled_to[{2}] -> expected [{3}] !!!", ValidationPartition.Name, Partition.MinFreeSectors, Partition.MinFreeSectors * LayoutSectorSize / RulesSectorSize, ValidationPartition.MinFreeSectors);
			}
			if (!ValidateDeviceLayoutValue(Partition.RequiresCompression, ValidationPartition.RequiresCompression))
			{
				result = false;
				_logger.LogError("DeviceLayoutValidation: ValidateDeviceLayoutFields Failed in validation of partition [{0}] value for field [RequiresCompression] mismatched : found [{1}] -> expected [{2}] !!!", ValidationPartition.Name, Partition.RequiresCompression, ValidationPartition.RequiresCompression);
			}
			if (!ValidateDeviceLayoutValue(Partition.FileSystem, ValidationPartition.FileSystem))
			{
				result = false;
				_logger.LogError("DeviceLayoutValidation: ValidateDeviceLayoutFields Failed in validation of partition [{0}] value for field [FileSystem] mismatched : found [{1}] -> expected [{2}] !!!", ValidationPartition.Name, Partition.FileSystem, ValidationPartition.FileSystem);
			}
			if (!ValidateDeviceLayoutValue(Partition.RequiredToFlash, ValidationPartition.RequiredToFlash))
			{
				result = false;
				_logger.LogError("DeviceLayoutValidation: ValidateDeviceLayoutFields Failed in validation of partition [{0}] value for field [RequiredToFlash] mismatched : found [{1}] -> expected [{2}] !!!", ValidationPartition.Name, Partition.RequiredToFlash, ValidationPartition.RequiredToFlash);
			}
			if (!ValidateDeviceLayoutValue(Partition.PrimaryPartition, ValidationPartition.PrimaryPartition, Partition.Name))
			{
				result = false;
				_logger.LogError("DeviceLayoutValidation: ValidateDeviceLayoutFields Failed in validation of partition [{0}] value for field [PrimaryPartition] mismatched : found [{1}] -> expected [{2}] !!!", ValidationPartition.Name, Partition.PrimaryPartition, ValidationPartition.PrimaryPartition);
			}
			if (!ValidateDeviceLayoutValue(Partition.SingleSectorAlignment, ValidationPartition.SingleSectorAlignment))
			{
				result = false;
				_logger.LogError("DeviceLayoutValidation: ValidateDeviceLayoutFields Failed in validation of partition [{0}] value for field [SingleSectorAlignment] mismatched : found [{1}] -> expected [{2}] !!!", ValidationPartition.Name, Partition.SingleSectorAlignment, ValidationPartition.SingleSectorAlignment);
			}
			if (!ValidateDeviceLayoutValue(Partition.ByteAlignment, ValidationPartition.ByteAlignment))
			{
				result = false;
				_logger.LogError("DeviceLayoutValidation: ValidateDeviceLayoutFields Failed in validation of partition [{0}] value for field [ByteAlignment] mismatched : found [{1}] -> expected [{2}] !!!", ValidationPartition.Name, Partition.ByteAlignment, ValidationPartition.ByteAlignment);
			}
			if (!ValidateDeviceLayoutValue(Partition.ClusterSize, ValidationPartition.ClusterSize))
			{
				result = false;
				_logger.LogError("DeviceLayoutValidation: ValidateDeviceLayoutFields Failed in validation of partition [{0}] value for field [ClusterSize] mismatched : found [{1}] -> expected [{2}] !!!", ValidationPartition.Name, Partition.ClusterSize, ValidationPartition.ClusterSize);
			}
			return result;
		}

		private void ValidateDeviceLayout(DeviceLayoutInput XmlDeviceLayout, DeviceLayoutValidationInput XmlDeviceLayoutValidation, out bool deviceLayoutHasChanged)
		{
			bool flag = true;
			int num = 1;
			InputValidationPartition inputValidationPartition = null;
			List<InputPartition> list = new List<InputPartition>(XmlDeviceLayout.Partitions);
			deviceLayoutHasChanged = false;
			DeviceLayoutValidationError deviceLayoutValidationError = DeviceLayoutValidationError.Pass;
			_logger.LogInfo("DeviceLayoutValidation: Validating device layout attributes");
			if (!ValidateDeviceLayoutValue(XmlDeviceLayout.SectorSize, XmlDeviceLayoutValidation.SectorSize))
			{
				flag = false;
				if (deviceLayoutValidationError == DeviceLayoutValidationError.Pass)
				{
					deviceLayoutValidationError = DeviceLayoutValidationError.DeviceLayoutAttributeMismatch;
				}
				_logger.LogError("DeviceLayoutValidation: ValidateDeviceLayout Failed in validation of SectorSize : found [{0}] -> expected [{1}]", XmlDeviceLayout.SectorSize, XmlDeviceLayoutValidation.SectorSize);
			}
			if (!ValidateDeviceLayoutValue(XmlDeviceLayout.ChunkSize, XmlDeviceLayoutValidation.ChunkSize))
			{
				flag = false;
				if (deviceLayoutValidationError == DeviceLayoutValidationError.Pass)
				{
					deviceLayoutValidationError = DeviceLayoutValidationError.DeviceLayoutAttributeMismatch;
				}
				_logger.LogError("DeviceLayoutValidation: ValidateDeviceLayout Failed in validation of ChunkSize : found [{0}] -> expected [{1}]", XmlDeviceLayout.ChunkSize, XmlDeviceLayoutValidation.ChunkSize);
			}
			if (!ValidateDeviceLayoutValue(XmlDeviceLayout.DefaultPartitionByteAlignment, XmlDeviceLayoutValidation.DefaultPartitionByteAlignment))
			{
				flag = false;
				if (deviceLayoutValidationError == DeviceLayoutValidationError.Pass)
				{
					deviceLayoutValidationError = DeviceLayoutValidationError.DeviceLayoutAttributeMismatch;
				}
				_logger.LogError("DeviceLayoutValidation: ValidateDeviceLayout Failed in validation of DefaultPartitionByteAlignment : found [{0}] -> expected [{1}]", XmlDeviceLayout.DefaultPartitionByteAlignment, XmlDeviceLayoutValidation.DefaultPartitionByteAlignment);
			}
			if (XmlDeviceLayoutValidation.Partitions != null)
			{
				_logger.LogInfo("DeviceLayoutValidation: Validating partition existence and positioning");
				InputValidationPartition[] partitions = XmlDeviceLayoutValidation.Partitions;
				InputPartition[] partitions2;
				foreach (InputValidationPartition inputValidationPartition2 in partitions)
				{
					InputPartition inputPartition = null;
					int num2 = 0;
					partitions2 = XmlDeviceLayout.Partitions;
					foreach (InputPartition inputPartition2 in partitions2)
					{
						num2++;
						if (inputPartition2.Name.Equals(inputValidationPartition2.Name, StringComparison.OrdinalIgnoreCase))
						{
							inputPartition = inputPartition2;
							break;
						}
					}
					if (inputPartition == null)
					{
						if (inputValidationPartition2.Name.Equals("BACKUP_*", StringComparison.OrdinalIgnoreCase))
						{
							inputValidationPartition = inputValidationPartition2;
							continue;
						}
						bool result = false;
						bool.TryParse(inputValidationPartition2.Optional, out result);
						if (!result)
						{
							flag = false;
							if (deviceLayoutValidationError == DeviceLayoutValidationError.Pass)
							{
								deviceLayoutValidationError = DeviceLayoutValidationError.PartitionNotFound;
							}
							_logger.LogError("DeviceLayoutValidation: ValidateDeviceLayout Failed in validation of partition [{0}]  - NOT FOUND !!!", inputValidationPartition2.Name);
						}
						else
						{
							_logger.LogInfo("DeviceLayoutValidation: ValidateDeviceLayout optional partition [{0}]  - NOT FOUND", inputValidationPartition2.Name);
						}
						continue;
					}
					if (inputPartition.Name.Equals("BACKUP_*", StringComparison.OrdinalIgnoreCase))
					{
						flag = false;
						if (deviceLayoutValidationError == DeviceLayoutValidationError.Pass)
						{
							deviceLayoutValidationError = DeviceLayoutValidationError.PartitionInvalidName;
						}
						_logger.LogError("DeviceLayoutValidation: ValidateDeviceLayout Failed in validation of partition [{0}]  - INVALID NAME !!!", inputValidationPartition2.Name);
						continue;
					}
					if (inputValidationPartition2.Position != null)
					{
						int result2 = 0;
						int.TryParse(inputValidationPartition2.Position, out result2);
						if (result2 == 0)
						{
							if (inputValidationPartition2.Position.Equals("BACKUP_BOOKEND", StringComparison.OrdinalIgnoreCase))
							{
								num = num2;
							}
						}
						else if (result2 > 0)
						{
							if (result2 != num2)
							{
								flag = false;
								if (deviceLayoutValidationError == DeviceLayoutValidationError.Pass)
								{
									deviceLayoutValidationError = DeviceLayoutValidationError.PartitionPositionMismatch;
								}
								_logger.LogError("DeviceLayoutValidation: ValidateDeviceLayout Failed in validation of partition [{0}]  - POSITION MISMATCH - position is [{1}] but was expecting [{2}] !!!", inputValidationPartition2.Name, num2, inputValidationPartition2.Position);
							}
							else
							{
								_logger.LogInfo("DeviceLayoutValidation: Validated correct position for partition -> " + inputValidationPartition2.Name);
							}
						}
						else
						{
							int num3 = num2 - XmlDeviceLayout.Partitions.Length - 1;
							if (result2 != num3)
							{
								flag = false;
								if (deviceLayoutValidationError == DeviceLayoutValidationError.Pass)
								{
									deviceLayoutValidationError = DeviceLayoutValidationError.PartitionPositionMismatch;
								}
								_logger.LogError("DeviceLayoutValidation: ValidateDeviceLayout Failed in validation of partition [{0}]  - POSITION MISMATCH - position is [{1}] but was expecting [{2}] !!!", inputValidationPartition2.Name, num3, inputValidationPartition2.Position);
							}
							else
							{
								_logger.LogInfo("DeviceLayoutValidation: Correct position for partition -> " + inputValidationPartition2.Name);
							}
						}
					}
					_logger.LogInfo("DeviceLayoutValidation: Validating attributes for partition -> " + inputValidationPartition2.Name);
					if (!ValidateDeviceLayoutFields(inputPartition, inputValidationPartition2, XmlDeviceLayout.SectorSize, XmlDeviceLayoutValidation.RulesSectorSize))
					{
						flag = false;
						if (deviceLayoutValidationError == DeviceLayoutValidationError.Pass)
						{
							deviceLayoutValidationError = DeviceLayoutValidationError.PartitionAttributeValueMismatch;
						}
					}
				}
				if (inputValidationPartition != null)
				{
					_logger.LogInfo("DeviceLayoutValidation: creating BACKUP partitions for partitions marked with UpdateType = 'Critical'");
					partitions2 = XmlDeviceLayout.Partitions;
					foreach (InputPartition inputPartition3 in partitions2)
					{
						if (inputPartition3.Name.StartsWith("BACKUP_", StringComparison.OrdinalIgnoreCase))
						{
							continue;
						}
						string backupPartitionName2 = "BACKUP_" + inputPartition3.Name;
						InputPartition inputPartition4 = Array.Find(XmlDeviceLayout.Partitions, (InputPartition p) => p.Name.Equals(backupPartitionName2, StringComparison.OrdinalIgnoreCase));
						string text = inputPartition3.UpdateType ?? "Normal";
						if (inputPartition4 != null)
						{
							continue;
						}
						_logger.LogInfo("DeviceLayoutValidation: Partition [{0}] has no backup - UpdateType is [{1}]", inputPartition3.Name, text);
						if (text.Equals("Critical", StringComparison.OrdinalIgnoreCase))
						{
							string OriginalValue = "";
							uint OriginalValue2 = 0u;
							bool OriginalValue3 = false;
							inputPartition4 = new InputPartition();
							inputPartition4.Name = "BACKUP_" + inputPartition3.Name;
							_logger.LogInfo("DeviceLayoutValidation: Creating new backup partition : [{0}] for partition : [{1}]", inputPartition4.Name, inputPartition3.Name);
							if (GetParentDeviceLayoutValue(inputValidationPartition.Type, inputPartition3.Type, ref OriginalValue))
							{
								inputPartition4.Type = OriginalValue;
							}
							if (GetParentDeviceLayoutValue(inputValidationPartition.ReadOnly, inputPartition3.ReadOnly, ref OriginalValue3))
							{
								inputPartition4.ReadOnly = OriginalValue3;
							}
							if (GetParentDeviceLayoutValue(inputValidationPartition.AttachDriveLetter, inputPartition3.AttachDriveLetter, ref OriginalValue3))
							{
								inputPartition4.AttachDriveLetter = OriginalValue3;
							}
							if (GetParentDeviceLayoutValue(inputValidationPartition.Hidden, inputPartition3.Hidden, ref OriginalValue3))
							{
								inputPartition4.Hidden = OriginalValue3;
							}
							if (GetParentDeviceLayoutValue(inputValidationPartition.Bootable, inputPartition3.Bootable, ref OriginalValue3))
							{
								inputPartition4.Bootable = OriginalValue3;
							}
							if (GetParentDeviceLayoutValue(inputValidationPartition.TotalSectors, inputPartition3.TotalSectors, ref OriginalValue2))
							{
								inputPartition4.TotalSectors = OriginalValue2;
							}
							if (GetParentDeviceLayoutValue(inputValidationPartition.MinFreeSectors, inputPartition3.MinFreeSectors, ref OriginalValue2))
							{
								inputPartition4.MinFreeSectors = OriginalValue2;
							}
							if (GetParentDeviceLayoutValue(inputValidationPartition.UseAllSpace, inputPartition3.UseAllSpace, ref OriginalValue3))
							{
								inputPartition4.UseAllSpace = OriginalValue3;
							}
							if (GetParentDeviceLayoutValue(inputValidationPartition.FileSystem, inputPartition3.FileSystem, ref OriginalValue))
							{
								inputPartition4.FileSystem = OriginalValue;
							}
							if (GetParentDeviceLayoutValue(inputValidationPartition.UpdateType, inputPartition3.UpdateType, ref OriginalValue))
							{
								inputPartition4.UpdateType = OriginalValue;
							}
							if (GetParentDeviceLayoutValue(inputValidationPartition.Compressed, inputPartition3.Compressed, ref OriginalValue3))
							{
								inputPartition4.Compressed = OriginalValue3;
							}
							if (GetParentDeviceLayoutValue(inputValidationPartition.RequiredToFlash, inputPartition3.RequiredToFlash, ref OriginalValue3))
							{
								inputPartition4.RequiredToFlash = OriginalValue3;
							}
							if (GetParentDeviceLayoutValue(inputValidationPartition.SingleSectorAlignment, inputPartition3.SingleSectorAlignment, ref OriginalValue3))
							{
								inputPartition4.SingleSectorAlignment = OriginalValue3;
							}
							if (GetParentDeviceLayoutValue(inputValidationPartition.ByteAlignment, inputPartition3.ByteAlignment, ref OriginalValue2))
							{
								inputPartition4.ByteAlignment = OriginalValue2;
							}
							if (GetParentDeviceLayoutValue(inputValidationPartition.ClusterSize, inputPartition3.ClusterSize, ref OriginalValue2))
							{
								inputPartition4.ClusterSize = OriginalValue2;
							}
							list.Insert(num - 1, inputPartition4);
							num++;
						}
					}
					if (list.Count > XmlDeviceLayout.Partitions.Count())
					{
						_logger.LogInfo("DeviceLayoutValidation: [{0}] new backup partitions created", list.Count - XmlDeviceLayout.Partitions.Count());
						XmlDeviceLayout.Partitions = list.ToArray();
						deviceLayoutHasChanged = true;
					}
				}
				_logger.LogInfo("DeviceLayoutValidation: Validating partition BACKUP rules");
				partitions2 = XmlDeviceLayout.Partitions;
				foreach (InputPartition inputPartition5 in partitions2)
				{
					if (inputPartition5.Name.StartsWith("BACKUP_", StringComparison.OrdinalIgnoreCase))
					{
						continue;
					}
					string backupPartitionName = "BACKUP_" + inputPartition5.Name;
					InputPartition inputPartition6 = Array.Find(XmlDeviceLayout.Partitions, (InputPartition p) => p.Name.Equals(backupPartitionName, StringComparison.OrdinalIgnoreCase));
					string text2 = inputPartition5.UpdateType ?? "Normal";
					if (inputPartition6 == null)
					{
						_logger.LogInfo("DeviceLayoutValidation: Partition [{0}] has no backup - UpdateType is [{1}]", inputPartition5.Name, text2);
						if (text2.Equals("Critical", StringComparison.OrdinalIgnoreCase))
						{
							flag = false;
							if (deviceLayoutValidationError == DeviceLayoutValidationError.Pass)
							{
								deviceLayoutValidationError = DeviceLayoutValidationError.BackupPartitionNotFound;
							}
							_logger.LogError("DeviceLayoutValidation: ValidateDeviceLayout Failed in validation of partition [{0}]  - partition with UpdateType critical does not have a backup partition !!!", inputPartition5.Name);
						}
						continue;
					}
					_logger.LogInfo("DeviceLayoutValidation: Partition [{0}] has backup - UpdateType is [{1}]", inputPartition5.Name, text2);
					if (inputPartition5.TotalSectors != inputPartition6.TotalSectors)
					{
						flag = false;
						if (deviceLayoutValidationError == DeviceLayoutValidationError.Pass)
						{
							deviceLayoutValidationError = DeviceLayoutValidationError.BackupPartitionSizeMismatch;
						}
						_logger.LogError("DeviceLayoutValidation: ValidateDeviceLayout Failed in validation of partition [{0}] and [{1}]  - partition has a different size (TotalSectors) [{2}] from its backup [{3}] !!!", inputPartition5.Name, inputPartition6.Name, inputPartition5.TotalSectors, inputPartition6.TotalSectors);
					}
				}
			}
			_logger.LogInfo("DeviceLayoutValidation: Validating completed - results => [{0}]", flag);
			if (!flag)
			{
				throw new DeviceLayoutValidationException(deviceLayoutValidationError, "DeviceLayoutValidation: ValidateDeviceLayout DeviceLayout does not comply with Microsoft rules for the SOC");
			}
		}

		private void ReadDeviceLayoutXmlFile(string DeviceLayoutXMLFile, ref DeviceLayoutInput xmlDeviceLayout, ref DeviceLayoutInputv2 xmlDeviceLayoutv2)
		{
			XsdValidator xsdValidator = new XsdValidator();
			try
			{
				using (Stream xsdStream = ImageGeneratorParameters.GetDeviceLayoutXSD(DeviceLayoutXMLFile))
				{
					xsdValidator.ValidateXsd(xsdStream, DeviceLayoutXMLFile, _logger);
				}
			}
			catch (XsdValidatorException inner)
			{
				throw new PackageException(inner, "DeviceLayoutValidation: ValidateDeviceLayout Unable to validate Device Layout XSD.");
			}
			_logger.LogInfo("DeviceLayoutValidation: Successfully validated the Device Layout XML");
			if (ImageGeneratorParameters.IsDeviceLayoutV2(DeviceLayoutXMLFile))
			{
				XmlSerializer xmlSerializer = new XmlSerializer(typeof(DeviceLayoutInputv2));
				using (StreamReader textReader = new StreamReader(DeviceLayoutXMLFile))
				{
					try
					{
						xmlDeviceLayoutv2 = (DeviceLayoutInputv2)xmlSerializer.Deserialize(textReader);
					}
					catch (Exception inner2)
					{
						throw new PackageException(inner2, "DeviceLayoutValidation: ValidateDeviceLayout Unable to parse Device Layout XML.");
					}
				}
				if (xmlDeviceLayoutv2.VersionTag == null || xmlDeviceLayoutv2.VersionTag.Equals(""))
				{
					xmlDeviceLayoutv2.VersionTag = "WindowsMobile.10.0000";
				}
				return;
			}
			XmlSerializer xmlSerializer2 = new XmlSerializer(typeof(DeviceLayoutInput));
			using (StreamReader textReader2 = new StreamReader(DeviceLayoutXMLFile))
			{
				try
				{
					xmlDeviceLayout = (DeviceLayoutInput)xmlSerializer2.Deserialize(textReader2);
				}
				catch (Exception inner3)
				{
					throw new PackageException(inner3, "DeviceLayoutValidation: ValidateDeviceLayout Unable to parse Device Layout XML.");
				}
			}
			if (xmlDeviceLayout.VersionTag == null || xmlDeviceLayout.VersionTag.Equals(""))
			{
				xmlDeviceLayout.VersionTag = "WindowsMobile.10.0000";
			}
		}

		private void ValidateDeviceLayout(DeviceLayoutInput XmlDeviceLayout, string DeviceLayoutXMLFile, string DeviceLayoutValidationXMLFile)
		{
			DeviceLayoutValidationInput xmlDeviceLayoutValidation = null;
			bool deviceLayoutHasChanged = false;
			_logger.LogInfo("DeviceLayoutValidation: Successfully validated the Device Layout Validation XML");
			XmlValidator xmlValidator = new XmlValidator();
			try
			{
				xmlValidator.ValidateXmlAndAddDefaults("DeviceLayoutValidation.xsd", DeviceLayoutValidationXMLFile, _logger);
			}
			catch (XmlValidatorException inner)
			{
				throw new PackageException(inner, "DeviceLayoutValidation: ValidateDeviceLayout Unable to validate Device Layout Validation - XSD.");
			}
			XmlSerializer xmlSerializer = new XmlSerializer(typeof(DeviceLayoutValidationInput));
			using (StreamReader textReader = new StreamReader(DeviceLayoutValidationXMLFile))
			{
				try
				{
					xmlDeviceLayoutValidation = (DeviceLayoutValidationInput)xmlSerializer.Deserialize(textReader);
				}
				catch (Exception inner2)
				{
					throw new PackageException(inner2, "DeviceLayoutValidation: ValidateDeviceLayout Unable to parse Device Layout XML.");
				}
			}
			ValidateDeviceLayout(XmlDeviceLayout, xmlDeviceLayoutValidation, out deviceLayoutHasChanged);
			if (!deviceLayoutHasChanged)
			{
				return;
			}
			_logger.LogInfo("DeviceLayoutValidation: Device layout has changed - regenerating file [" + DeviceLayoutXMLFile + "]");
			XmlSerializer xmlSerializer2 = new XmlSerializer(typeof(DeviceLayoutInput));
			using (StreamWriter textWriter = new StreamWriter(DeviceLayoutXMLFile))
			{
				try
				{
					xmlSerializer2.Serialize(textWriter, XmlDeviceLayout);
				}
				catch (Exception inner3)
				{
					throw new PackageException(inner3, "DeviceLayoutValidation: ValidateDeviceLayout Unable to write Device Layout XML.");
				}
			}
		}

		private FileSignatureCertificateType GetFileSignatureCertificateType(string FilePath)
		{
			X509Certificate2 x509Certificate = null;
			FileSignatureCertificateType value = FileSignatureCertificateType.None;
			try
			{
				x509Certificate = new X509Certificate2(FilePath);
				if (x509Certificate != null)
				{
					if (!string.IsNullOrEmpty(x509Certificate.Subject))
					{
						if (!_certPublicKeys.TryGetValue(x509Certificate.Thumbprint, out value))
						{
							value = FileSignatureCertificateType.NonMicrosoftCertificate;
							X509Chain x509Chain = new X509Chain(true);
							x509Chain.Build(x509Certificate);
							X509ChainElementEnumerator enumerator = x509Chain.ChainElements.GetEnumerator();
							while (enumerator.MoveNext())
							{
								X509ChainElement current = enumerator.Current;
								if (string.Compare("3B1EFD3A66EA28B16697394703A72CA340A05BD5", current.Certificate.Thumbprint, true) == 0)
								{
									value = FileSignatureCertificateType.MicrosoftProductionCertificate;
									break;
								}
								if (string.Compare("9E594333273339A97051B0F82E86F266B917EDB3", current.Certificate.Thumbprint, true) == 0)
								{
									value = FileSignatureCertificateType.MicrosoftFlightCertificate;
									break;
								}
								if (string.Compare("5f444a6740b7ca2434c7a5925222c2339ee0f1b7", current.Certificate.Thumbprint, true) == 0)
								{
									value = FileSignatureCertificateType.MicrosoftFlightCertificate;
									break;
								}
								if (string.Compare("8A334AA8052DD244A647306A76B8178FA215F344", current.Certificate.Thumbprint, true) == 0)
								{
									value = FileSignatureCertificateType.MicrosoftTestCertificate;
									break;
								}
							}
							enumerator = x509Chain.ChainElements.GetEnumerator();
							while (enumerator.MoveNext())
							{
								X509ChainElement current2 = enumerator.Current;
								_certPublicKeys[current2.Certificate.Thumbprint] = value;
							}
							return value;
						}
						return value;
					}
					return value;
				}
				return value;
			}
			catch
			{
				return FileSignatureCertificateType.None;
			}
		}

		private void ValidateDeviceLayoutPackageIsMicrosoftOwned(IPkgInfo ReferencePkg, IPkgInfo Pkg, string PkgName, string PkgPath)
		{
			if (_oemInput.CPUType.Equals(FeatureManifest.CPUType_ARM, StringComparison.OrdinalIgnoreCase) || _oemInput.CPUType.Equals(FeatureManifest.CPUType_ARM64, StringComparison.OrdinalIgnoreCase))
			{
				if (Pkg.OwnerType != OwnerType.Microsoft)
				{
					throw new DeviceLayoutValidationException(DeviceLayoutValidationError.DeviceLayoutNotMsOwned, string.Concat("DeviceLayoutValidation: ValidateDeviceLayoutPackageIsMicrosoftOwned Package Owner '", Pkg.OwnerType, "' must be a Microsoft owned package.  '", PkgPath, "' device package is not."));
				}
				if (_oemInput.ReleaseType.Equals("Production", StringComparison.OrdinalIgnoreCase))
				{
					if (!string.Equals(_oemInput.BuildType, OEMInput.BuildType_FRE, StringComparison.OrdinalIgnoreCase))
					{
						throw new DeviceLayoutValidationException(DeviceLayoutValidationError.DeviceLayoutNotProductionSigned, "DeviceLayoutValidation: ValidateDeviceLayoutPackageIsMicrosoftOwned: The BuildType '" + _oemInput.BuildType + "' in the OEM Input file is not valid.  Please use 'fre' for Retail images.");
					}
					if (Pkg.ReleaseType != ReleaseType.Production || Pkg.BuildType != BuildType.Retail)
					{
						throw new DeviceLayoutValidationException(DeviceLayoutValidationError.DeviceLayoutNotProductionSigned, "DeviceLayoutValidation: ValidateDeviceLayoutPackageIsMicrosoftOwned: The Package BuildType must be 'Production' and the package release type must be 'Retail'");
					}
					IFileEntry fileEntry = Pkg.Files.Where((IFileEntry file) => file.FileType == FileType.Catalog).First();
					string text = Path.Combine(_tempDirectoryPath, "tempref.cat");
					string text2 = Path.Combine(_tempDirectoryPath, "temp.cat");
					try
					{
						Pkg.ExtractFile(fileEntry.DevicePath, text2, true);
						FileSignatureCertificateType fileSignatureCertificateType = GetFileSignatureCertificateType(text2);
						switch (fileSignatureCertificateType)
						{
						default:
						{
							_logger.LogInfo("DeviceLayoutValidation: ValidateDeviceLayoutPackageIsMicrosoftOwned - '" + PkgName + " 'is signed by " + fileSignatureCertificateType.ToString() + " - checking that the reference package also is.'");
							IFileEntry fileEntry2 = ReferencePkg.Files.Where((IFileEntry file) => file.FileType == FileType.Catalog).First();
							ReferencePkg.ExtractFile(fileEntry2.DevicePath, text, true);
							FileSignatureCertificateType fileSignatureCertificateType2 = GetFileSignatureCertificateType(text);
							if (fileSignatureCertificateType2 != fileSignatureCertificateType)
							{
								if (fileSignatureCertificateType2 == FileSignatureCertificateType.None || fileSignatureCertificateType2 == FileSignatureCertificateType.NonMicrosoftCertificate)
								{
									throw new DeviceLayoutValidationException(DeviceLayoutValidationError.UnknownInternalError, "DeviceLayoutValidation: ValidateDeviceLayoutPackageIsMicrosoftOwned: Unable to validate if the Package '" + PkgName + "' is Microsoft signed - since the reference package provided has an invalid certificate '" + fileSignatureCertificateType2.ToString() + "'");
								}
								throw new DeviceLayoutValidationException(DeviceLayoutValidationError.DeviceLayoutValidationManifestNotProductionSigned, "DeviceLayoutValidation: ValidateDeviceLayoutPackageIsMicrosoftOwned: The DeviceLayoutValidationManifest package '" + PkgName + "' signature (" + fileSignatureCertificateType.ToString() + ") does not match the signature type of the reference package (" + fileSignatureCertificateType2.ToString() + ".");
							}
							break;
						}
						case FileSignatureCertificateType.None:
						case FileSignatureCertificateType.NonMicrosoftCertificate:
							throw new DeviceLayoutValidationException(DeviceLayoutValidationError.DeviceLayoutNotProductionSigned, "DeviceLayoutValidation: ValidateDeviceLayoutPackageIsMicrosoftOwned: The Package '" + PkgName + "' is NOT Microsoft signed");
						case FileSignatureCertificateType.MicrosoftFlightCertificate:
						case FileSignatureCertificateType.MicrosoftProductionCertificate:
							break;
						}
						_logger.LogInfo("DeviceLayoutValidation: ValidateDeviceLayoutPackageIsMicrosoftOwned - Success Package '" + PkgName + " 'is signed correctly.");
						return;
					}
					finally
					{
						File.Delete(text2);
						File.Delete(text);
					}
				}
				_logger.LogInfo("DeviceLayoutValidation: ValidateDeviceLayoutPackageIsMicrosoftOwned - ReleaseType '" + _oemInput.ReleaseType.ToString() + "' is not production - ignoring certificate check.");
			}
			else
			{
				_logger.LogInfo("DeviceLayoutValidation: ValidateDeviceLayoutPackageIsMicrosoftOwned - CPUType '" + _oemInput.CPUType.ToString() + "' is not ARM based - ignoring certificate check.");
			}
		}

		private void ValidateDeviceLayoutPackageIsOEMOwned(IPkgInfo Pkg, string PkgName, string PkgPath)
		{
			if (_oemInput.CPUType.Equals(FeatureManifest.CPUType_ARM, StringComparison.OrdinalIgnoreCase) || _oemInput.CPUType.Equals(FeatureManifest.CPUType_ARM64, StringComparison.OrdinalIgnoreCase))
			{
				if (Pkg.OwnerType != OwnerType.OEM)
				{
					_logger.LogInfo("DeviceLayoutValidation: ValidateDeviceLayoutPackageIsOEMOwned - Package '" + PkgName + " 'should be contained in a OEM owned package.  '" + PkgPath + "' device package is not.");
				}
				if (_oemInput.ReleaseType.Equals("Production", StringComparison.OrdinalIgnoreCase))
				{
					if (!string.Equals(_oemInput.BuildType, OEMInput.BuildType_FRE, StringComparison.OrdinalIgnoreCase))
					{
						throw new DeviceLayoutValidationException(DeviceLayoutValidationError.DeviceLayoutNotProductionSigned, "DeviceLayoutValidation: ValidateDeviceLayoutPackageIsOEMOwned: The BuildType '" + _oemInput.BuildType + "' in the OEM Input file is not valid.  Please use 'fre' for Retail images.");
					}
					if (Pkg.ReleaseType != ReleaseType.Production || Pkg.BuildType != BuildType.Retail)
					{
						throw new DeviceLayoutValidationException(DeviceLayoutValidationError.DeviceLayoutNotProductionSigned, "DeviceLayoutValidation: ValidateDeviceLayoutPackageIsOEMOwned: The Package BuildType must be 'Production' and the package release type must be 'Retail'");
					}
					IFileEntry fileEntry = Pkg.Files.Where((IFileEntry file) => file.FileType == FileType.Catalog).First();
					string text = Path.Combine(_tempDirectoryPath, "temp.cat");
					try
					{
						Pkg.ExtractFile(fileEntry.DevicePath, text, true);
						switch (GetFileSignatureCertificateType(text))
						{
						case FileSignatureCertificateType.None:
							throw new DeviceLayoutValidationException(DeviceLayoutValidationError.DeviceLayoutNotProductionSigned, "DeviceLayoutValidation: ValidateDeviceLayoutPackageIsOEMOwned: The Package '" + PkgName + "' is not signed");
						case FileSignatureCertificateType.NonMicrosoftCertificate:
							return;
						}
						_logger.LogInfo("DeviceLayoutValidation: ValidateDeviceLayoutPackageIsOEMOwned - Package '" + PkgName + " 'is signed by Microsoft but needs to be OEM signed.'");
						return;
					}
					finally
					{
						File.Delete(text);
					}
				}
				_logger.LogInfo("DeviceLayoutValidation: ValidateDeviceLayoutPackageIsOEMOwned - ReleaseType '" + _oemInput.ReleaseType.ToString() + "' is not production - ignoring certificate check.");
			}
			else
			{
				_logger.LogInfo("DeviceLayoutValidation: ValidateDeviceLayoutPackageIsOEMOwned - CPUType '" + _oemInput.CPUType.ToString() + "' is not ARM based - ignoring certificate check.");
			}
		}

		public void ValidateDeviceLayout(IPkgInfo ReferencePkg, IPkgInfo PkgDeviceLayout, string DeviceLayoutPackagePath, string DeviceLayoutXmlFilePath)
		{
			DeviceLayoutInput xmlDeviceLayout = null;
			DeviceLayoutInputv2 xmlDeviceLayoutv = null;
			if (_oemInput == null)
			{
				throw new ImageCommonException("DeviceLayoutValidation: DeviceLayoutValidator not initialized");
			}
			ReadDeviceLayoutXmlFile(DeviceLayoutXmlFilePath, ref xmlDeviceLayout, ref xmlDeviceLayoutv);
			string versionTag = ((xmlDeviceLayoutv == null) ? xmlDeviceLayout.VersionTag : xmlDeviceLayoutv.VersionTag);
			string deviceLayoutValidationFileInScope = GetDeviceLayoutValidationFileInScope(versionTag);
			if (string.IsNullOrEmpty(deviceLayoutValidationFileInScope))
			{
				_logger.LogInfo("DeviceLayoutValidation: SOC '" + _oemInput.SOC + "' is not opened - reverting to validating Microsoft ownership of DeviceLayout Package");
				ValidateDeviceLayoutPackageIsMicrosoftOwned(ReferencePkg, PkgDeviceLayout, "DeviceLayout", DeviceLayoutPackagePath);
				return;
			}
			_logger.LogInfo("DeviceLayoutValidation: SOC '" + _oemInput.SOC + "' is open. Using '" + deviceLayoutValidationFileInScope + "' to validate  DeviceLayout Package");
			ValidateDeviceLayoutPackageIsOEMOwned(PkgDeviceLayout, "DeviceLayout", DeviceLayoutPackagePath);
			if (xmlDeviceLayout != null)
			{
				ValidateDeviceLayout(xmlDeviceLayout, DeviceLayoutXmlFilePath, deviceLayoutValidationFileInScope);
			}
		}
	}
}
