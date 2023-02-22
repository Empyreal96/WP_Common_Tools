namespace Microsoft.CompPlat.PkgBldr.Tools
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
