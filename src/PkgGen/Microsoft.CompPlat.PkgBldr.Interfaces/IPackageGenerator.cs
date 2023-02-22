namespace Microsoft.CompPlat.PkgBldr.Interfaces
{
	public interface IPackageGenerator
	{
		void Build(string projPath, string outputDir, bool compress);
	}
}
