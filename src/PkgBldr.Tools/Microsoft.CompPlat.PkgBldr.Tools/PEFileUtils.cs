using System.IO;
using System.Runtime.InteropServices;

namespace Microsoft.CompPlat.PkgBldr.Tools
{
	public static class PEFileUtils
	{
		[StructLayout(LayoutKind.Explicit)]
		public struct IMAGE_DOS_HEADER
		{
			[FieldOffset(60)]
			public int e_lfanew;
		}

		public struct IMAGE_FILE_HEADER
		{
			public ushort Machine;

			public ushort NumberOfSections;

			public ulong TimeDateStamp;

			public ulong PointerToSymbolTable;

			public ulong NumberOfSymbols;

			public ushort SizeOfOptionalHeader;

			public ushort Characteristics;
		}

		public struct IMAGE_DATA_DIRECTORY
		{
			public uint VirtualAddress;

			public uint Size;
		}

		public struct IMAGE_OPTIONAL_HEADER32
		{
			public ushort Magic;

			public byte MajorLinkerVersion;

			public byte MinorLinkerVersion;

			public uint SizeOfCode;

			public uint SizeOfInitializedData;

			public uint SizeOfUninitializedData;

			public uint AddressOfEntryPoint;

			public uint BaseOfCode;

			public uint BaseOfData;

			public uint ImageBase;

			public uint SectionAlignment;

			public uint FileAlignment;

			public ushort MajorOperatingSystemVersion;

			public ushort MinorOperatingSystemVersion;

			public ushort MajorImageVersion;

			public ushort MinorImageVersion;

			public ushort MajorSubsystemVersion;

			public ushort MinorSubsystemVersion;

			public uint Win32VersionValue;

			public uint SizeOfImage;

			public uint SizeOfHeaders;

			public uint CheckSum;

			public ushort Subsystem;

			public ushort DllCharacteristics;

			public uint SizeOfStackReserve;

			public uint SizeOfStackCommit;

			public uint SizeOfHeapReserve;

			public uint SizeOfHeapCommit;

			public uint LoaderFlags;

			public uint NumberOfRvaAndSizes;
		}

		public struct IMAGE_OPTIONAL_HEADER64
		{
			public ushort Magic;

			public byte MajorLinkerVersion;

			public byte MinorLinkerVersion;

			public uint SizeOfCode;

			public uint SizeOfInitializedData;

			public uint SizeOfUninitializedData;

			public uint AddressOfEntryPoint;

			public uint BaseOfCode;

			public ulong ImageBase;

			public uint SectionAlignment;

			public uint FileAlignment;

			public ushort MajorOperatingSystemVersion;

			public ushort MinorOperatingSystemVersion;

			public ushort MajorImageVersion;

			public ushort MinorImageVersion;

			public ushort MajorSubsystemVersion;

			public ushort MinorSubsystemVersion;

			public uint Win32VersionValue;

			public uint SizeOfImage;

			public uint SizeOfHeaders;

			public uint CheckSum;

			public ushort Subsystem;

			public ushort DllCharacteristics;

			public ulong SizeOfStackReserve;

			public ulong SizeOfStackCommit;

			public ulong SizeOfHeapReserve;

			public ulong SizeOfHeapCommit;

			public uint LoaderFlags;

			public uint NumberOfRvaAndSizes;
		}

		public struct IMAGE_NT_HEADERS32
		{
			public uint Signature;

			public IMAGE_FILE_HEADER FileHeader;

			public IMAGE_OPTIONAL_HEADER32 OptionalHeader;
		}

		public struct IMAGE_NT_HEADERS64
		{
			public uint Signature;

			public IMAGE_FILE_HEADER FileHeader;

			public IMAGE_OPTIONAL_HEADER64 OptionalHeader;
		}

		private static uint c_iPESignature = 4660u;

		private static T ReadStruct<T>(BinaryReader br)
		{
			GCHandle gCHandle = GCHandle.Alloc(br.ReadBytes(Marshal.SizeOf(typeof(T))), GCHandleType.Pinned);
			T result = (T)Marshal.PtrToStructure(gCHandle.AddrOfPinnedObject(), typeof(T));
			gCHandle.Free();
			return result;
		}

		public static bool IsPE(string path)
		{
			using (BinaryReader binaryReader = new BinaryReader(LongPathFile.OpenRead(path)))
			{
				if (binaryReader.BaseStream.Length < Marshal.SizeOf(typeof(IMAGE_DOS_HEADER)))
				{
					return false;
				}
				IMAGE_DOS_HEADER iMAGE_DOS_HEADER = ReadStruct<IMAGE_DOS_HEADER>(binaryReader);
				if (iMAGE_DOS_HEADER.e_lfanew < Marshal.SizeOf(typeof(IMAGE_DOS_HEADER)))
				{
					return false;
				}
				if (iMAGE_DOS_HEADER.e_lfanew > int.MaxValue - Marshal.SizeOf(typeof(IMAGE_NT_HEADERS32)))
				{
					return false;
				}
				if (binaryReader.BaseStream.Length < iMAGE_DOS_HEADER.e_lfanew + Marshal.SizeOf(typeof(IMAGE_NT_HEADERS32)))
				{
					return false;
				}
				binaryReader.BaseStream.Seek(iMAGE_DOS_HEADER.e_lfanew, SeekOrigin.Begin);
				if (binaryReader.ReadUInt32() != c_iPESignature)
				{
					return false;
				}
				ReadStruct<IMAGE_FILE_HEADER>(binaryReader);
				return true;
			}
		}
	}
}
