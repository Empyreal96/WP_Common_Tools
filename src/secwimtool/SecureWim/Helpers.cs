using System;
using System.Globalization;
using System.IO;
using System.Linq;

namespace SecureWim
{
	internal class Helpers
	{
		public static void WriteUintToStream(uint value, Stream s)
		{
			byte[] bytes = BitConverter.GetBytes(value);
			s.Write(bytes, 0, bytes.Length);
		}

		public static void SeekStreamToCatalogStart(Stream s)
		{
			uint catalogSize = GetCatalogSize(s);
			s.Seek(-((long)catalogSize + 4L), SeekOrigin.End);
		}

		public static uint GetCatalogSize(Stream s)
		{
			byte[] array = new byte[4];
			s.Seek(-array.Length, SeekOrigin.End);
			s.Read(array, 0, array.Length);
			return BitConverter.ToUInt32(array, 0);
		}

		public static bool HasSdiHeader(string sdiFile)
		{
			byte[] header = new byte[8] { 36, 83, 68, 73, 48, 48, 48, 49 };
			using (FileStream s = File.OpenRead(sdiFile))
			{
				return HasHeader(s, header);
			}
		}

		public static bool HasWimHeader(string wimFile)
		{
			byte[] header = new byte[8] { 77, 83, 87, 73, 77, 0, 0, 0 };
			using (FileStream s = File.OpenRead(wimFile))
			{
				return HasHeader(s, header);
			}
		}

		public static void AddPadding(Stream s, uint padding)
		{
			while (s.Length % (long)padding != 0L)
			{
				s.WriteByte(0);
			}
		}

		internal static Guid[] GetGuids(string[] idStrings)
		{
			Guid[] array = new Guid[idStrings.Length];
			for (int i = 0; i < idStrings.Length; i++)
			{
				if (idStrings[i].Length != 32)
				{
					throw new FormatException("Expected a raw byte string serial number.");
				}
				byte[] array2 = new byte[16];
				try
				{
					for (int j = 0; j < array2.Length; j++)
					{
						array2[j] = GetByte(idStrings[i].Substring(2 * j, 2));
					}
				}
				catch (FormatException ex)
				{
					throw new FormatException(ex.Message + " ID being parsed was: " + idStrings[i], ex);
				}
				array[i] = new Guid(array2);
			}
			return array;
		}

		private static byte GetByte(string byteString)
		{
			return byte.Parse(byteString, NumberStyles.HexNumber);
		}

		private static bool HasHeader(Stream s, byte[] header)
		{
			byte[] array = new byte[header.Length];
			if (s.Length < array.Length)
			{
				return false;
			}
			s.Seek(0L, SeekOrigin.Begin);
			s.Read(array, 0, array.Length);
			return header.SequenceEqual(array);
		}
	}
}
