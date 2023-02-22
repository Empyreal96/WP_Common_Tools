namespace Microsoft.WindowsPhone.WPImage
{
	internal interface IWPImageCommand
	{
		string Name { get; }

		bool ParseArgs(string[] args);

		void PrintUsage();

		void Run();
	}
}
