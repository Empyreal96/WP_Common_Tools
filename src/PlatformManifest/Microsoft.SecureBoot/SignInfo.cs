using System.IO;
using System.Runtime.Serialization;

namespace Microsoft.SecureBoot
{
	[DataContract(Namespace = "http://schemas.datacontract.org/2004/07/Microsoft.Build.Signing")]
	internal class SignInfo
	{
		[DataMember]
		public string BinaryID { get; set; }

		[DataMember]
		public string BinaryIdHash { get; set; }

		[DataMember]
		public string CodeAuthorization { get; set; }

		public static SignInfo LoadFromFile(string SignInfoPath)
		{
			using (FileStream stream = new FileStream(SignInfoPath, FileMode.Open, FileAccess.Read))
			{
				return (SignInfo)new DataContractSerializer(typeof(SignInfo)).ReadObject((Stream)stream);
			}
		}
	}
}
