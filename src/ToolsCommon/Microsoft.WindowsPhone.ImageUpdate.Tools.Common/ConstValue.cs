namespace Microsoft.WindowsPhone.ImageUpdate.Tools.Common
{
	public class ConstValue<T>
	{
		public T Value { get; private set; }

		public ConstValue(T value)
		{
			Value = value;
		}
	}
}
