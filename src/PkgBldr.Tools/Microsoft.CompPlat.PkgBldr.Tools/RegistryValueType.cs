namespace Microsoft.CompPlat.PkgBldr.Tools
{
	public enum RegistryValueType : uint
	{
		None,
		String,
		ExpandString,
		Binary,
		DWord,
		DWordBigEndian,
		Link,
		MultiString,
		RegResourceList,
		RegFullResourceDescriptor,
		RegResourceRequirementsList,
		QWord
	}
}
