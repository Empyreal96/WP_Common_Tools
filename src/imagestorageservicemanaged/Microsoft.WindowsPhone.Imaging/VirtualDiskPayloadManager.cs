using System;
using System.Collections.Generic;
using System.Reflection;
using Microsoft.WindowsPhone.ImageUpdate.Tools.Common;

namespace Microsoft.WindowsPhone.Imaging
{
	internal class VirtualDiskPayloadManager : IDisposable
	{
		private IULogger _logger;

		private List<Tuple<VirtualDiskPayloadGenerator, ImageStorage>> _generators;

		private ushort _storeHeaderVersion;

		private ushort _numOfStores;

		private bool _alreadyDisposed;

		public VirtualDiskPayloadManager(IULogger logger, ushort storeHeaderVersion, ushort numOfStores)
		{
			_logger = logger;
			_storeHeaderVersion = storeHeaderVersion;
			_numOfStores = numOfStores;
			_generators = new List<Tuple<VirtualDiskPayloadGenerator, ImageStorage>>();
		}

		public void AddStore(ImageStorage storage)
		{
			if (_storeHeaderVersion < 2 && _generators.Count > 1)
			{
				throw new ImageStorageException($"{MethodBase.GetCurrentMethod().Name}: Cannot add more than one store to a FFU using v1 store header.");
			}
			VirtualDiskPayloadGenerator item = new VirtualDiskPayloadGenerator(_logger, ImageConstants.PAYLOAD_BLOCK_SIZE, storage, _storeHeaderVersion, _numOfStores, (ushort)(_generators.Count + 1));
			_generators.Add(new Tuple<VirtualDiskPayloadGenerator, ImageStorage>(item, storage));
		}

		public void Write(IPayloadWrapper payloadWrapper)
		{
			long num = 0L;
			foreach (Tuple<VirtualDiskPayloadGenerator, ImageStorage> generator in _generators)
			{
				VirtualDiskPayloadGenerator item = generator.Item1;
				ImageStorage item2 = generator.Item2;
				item.GenerateStorePayload(item2);
				num += item.TotalSize;
			}
			payloadWrapper.InitializeWrapper(num);
			_generators.ForEach(delegate(Tuple<VirtualDiskPayloadGenerator, ImageStorage> t)
			{
				t.Item1.WriteMetadata(payloadWrapper);
			});
			_generators.ForEach(delegate(Tuple<VirtualDiskPayloadGenerator, ImageStorage> t)
			{
				t.Item1.WriteStorePayload(payloadWrapper);
			});
			payloadWrapper.FinalizeWrapper();
		}

		~VirtualDiskPayloadManager()
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
				_generators.ForEach(delegate(Tuple<VirtualDiskPayloadGenerator, ImageStorage> t)
				{
					t.Item1.Dispose();
				});
				_generators.Clear();
				_generators = null;
			}
			_alreadyDisposed = true;
		}
	}
}
