using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using Microsoft.WindowsPhone.ImageUpdate.Tools.Common;

namespace Microsoft.SecureBoot
{
	public class PlatformManifest
	{
		public enum ImageReleaseType
		{
			Retail,
			Test
		}

		private Guid Id;

		private string BuildString;

		private IList<byte[]> Entries = new List<byte[]>();

		private IList<string> TextEntries = new List<string>();

		private const string DebugIDString = "DebugFileId";

		private static byte[] DebugIDHash = GetUnicodeHash("DebugFileId");

		public const int PACKAGE_MANIFEST_V1_HASH_LENGTH = 32;

		public ImageReleaseType ImageType { get; set; }

		private static byte[] GetHash(string input)
		{
			return SHA256.Create().ComputeHash(Encoding.UTF8.GetBytes(input));
		}

		private static byte[] GetUnicodeHash(string input)
		{
			return SHA256.Create().ComputeHash(Encoding.Unicode.GetBytes(input));
		}

		public PlatformManifest(Guid id, string bs)
		{
			Id = id;
			BuildString = bs;
			ImageType = ImageReleaseType.Retail;
		}

		public void AddRawEntry(byte[] NewEntry)
		{
			if (NewEntry.Length != 32)
			{
				throw new Exception("PlatformManifest!AddRawEntry: Invalid Entry Length for Platform Manifest");
			}
			if (ImageType == ImageReleaseType.Retail && NewEntry.SequenceEqual(DebugIDHash))
			{
				throw new InvalidDebugIDException("PlatformManitest!AddRawEntry: Debug ID not allowed when generating a Retail image.");
			}
			Entries.Add(NewEntry);
		}

		public void AddStringEntry(string NewEntry)
		{
			AddRawEntry(GetHash(NewEntry));
			TextEntries.Add(NewEntry);
		}

		public void AddBinaryFromSignInfo(string SigninfoFile)
		{
			SignInfo signInfo = SignInfo.LoadFromFile(SigninfoFile);
			AddRawEntry(Convert.FromBase64String(signInfo.BinaryIdHash));
			TextEntries.Add($"SignInfo: {Path.GetFileName(SigninfoFile)}  BinaryIdHash={signInfo.BinaryIdHash}");
		}

		public void WriteRawPlatformManifestToFile(string OutputPath, string TextFilePath)
		{
			if (ImageType == ImageReleaseType.Test)
			{
				AddRawEntry(DebugIDHash);
			}
			BinaryWriter binaryWriter = new BinaryWriter(File.Open(OutputPath, FileMode.Create), Encoding.Unicode);
			StreamWriter streamWriter = File.CreateText(TextFilePath);
			char[] array = new char[128];
			char[] array2 = BuildString.ToCharArray();
			int num = 0;
			char[] array3 = array2;
			foreach (char c in array3)
			{
				array[num++] = c;
				if (num > array.Length)
				{
					break;
				}
			}
			binaryWriter.Write(1718446928u);
			binaryWriter.Write((ushort)1);
			binaryWriter.Write(Id.ToByteArray());
			binaryWriter.Write(1u);
			binaryWriter.Write(array);
			binaryWriter.Write((uint)Entries.Count);
			foreach (byte[] entry in Entries)
			{
				if (entry.Length != 32)
				{
					throw new Exception("PlatformManifest!WriteToFile: Invalid hash length");
				}
				binaryWriter.Write(entry);
			}
			foreach (string textEntry in TextEntries)
			{
				streamWriter.WriteLine(textEntry);
			}
			binaryWriter.Close();
			streamWriter.Close();
		}

		public void SignFileP7(string Filename)
		{
			int num = 0;
			if (!File.Exists(Environment.ExpandEnvironmentVariables("%RazzleToolPath%\\urtrun.cmd")))
			{
				throw new Exception("Unable to find urtrun.cmd!");
			}
			try
			{
				num = CommonUtils.RunProcess("%COMSPEC%", $"/C %RazzleToolPath%\\urtrun.cmd 4.0 signer.exe authenticode -s PlatformManifest -f \"{Filename}\"");
			}
			catch (Exception innerException)
			{
				throw new Exception($"Failed to sign the Platform Manifest with commandline signer.cmd authenticode -s PlatformManifest -f \"{Filename}\"", innerException);
			}
			if (num != 0)
			{
				throw new Exception($"Failed to sign the Platform Manifest with commandline signer.cmd authenticode -s PlatformManifest -f \"{Filename}\", exit code {num}");
			}
		}

		public void WriteToFile(string OutputPath)
		{
			string tempFile = FileUtils.GetTempFile();
			WriteRawPlatformManifestToFile(tempFile, OutputPath + ".txt");
			SignFileP7(tempFile);
			if (File.Exists(OutputPath))
			{
				File.Delete(OutputPath);
			}
			File.Move(tempFile + ".p7", OutputPath);
			File.Delete(tempFile);
		}
	}
}
