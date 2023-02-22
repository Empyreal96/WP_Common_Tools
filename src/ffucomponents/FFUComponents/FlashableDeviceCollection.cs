using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;

namespace FFUComponents
{
	[ComVisible(false)]
	public class FlashableDeviceCollection : IEnumerator
	{
		private List<FlashableDevice> theList;

		private IEnumerator<FlashableDevice> theEnum;

		public object Current => theEnum.Current;

		public FlashableDeviceCollection(ICollection<IFFUDevice> aColl)
		{
			theList = new List<FlashableDevice>();
			for (int i = 0; i < aColl.Count; i++)
			{
				theList.Add(new FlashableDevice(aColl.ElementAt(i)));
			}
			theEnum = theList.GetEnumerator();
		}

		public bool MoveNext()
		{
			return theEnum.MoveNext();
		}

		public void Reset()
		{
			theEnum.Reset();
		}
	}
}
