using System;
using System.Collections.Generic;
using Microsoft.WindowsPhone.Imaging;

namespace Microsoft.WindowsPhone.ImageUpdate.Tools.Images
{
	public class WPStore : FullFlashUpdateImage.FullFlashUpdateStore
	{
		private WPImage _image;

		public new int PartitionCount
		{
			get
			{
				if (_image == null)
				{
					return 0;
				}
				return _image.PartitionCount;
			}
		}

		public new List<WPPartition> Partitions
		{
			get
			{
				if (_image == null)
				{
					return null;
				}
				return _image.Partitions;
			}
		}

		public WPStore(WPImage image)
		{
			_image = image;
		}

		public void Initialize(FullFlashUpdateImage.FullFlashUpdateStore store)
		{
			Initialize(store.MinSectorCount, store.SectorSize);
		}

		[CLSCompliant(false)]
		public void Initialize(uint minSectorCount, uint sectorSize)
		{
			base.MinSectorCount = minSectorCount;
			base.SectorSize = sectorSize;
		}
	}
}
