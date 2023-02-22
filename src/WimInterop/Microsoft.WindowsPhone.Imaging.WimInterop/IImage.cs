namespace Microsoft.WindowsPhone.Imaging.WimInterop
{
	public interface IImage
	{
		string MountedPath { get; }

		void Apply(string pathToMountTo);

		void Mount(string pathToMountTo, bool isReadOnly);

		void DismountImage();

		void DismountImage(bool saveChanges);
	}
}
