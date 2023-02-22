using System;
using System.Runtime.Serialization;

namespace _003CCrtImplementationDetails_003E
{
	[Serializable]
	internal class ModuleLoadExceptionHandlerException : ModuleLoadException
	{
		private const string formatString = "\n{0}: {1}\n--- Start of primary exception ---\n{2}\n--- End of primary exception ---\n\n--- Start of nested exception ---\n{3}\n--- End of nested exception ---\n";

		private Exception _003Cbacking_store_003ENestedException;

		public Exception NestedException
		{
			get
			{
				return _003Cbacking_store_003ENestedException;
			}
			set
			{
				_003Cbacking_store_003ENestedException = value;
			}
		}

		protected ModuleLoadExceptionHandlerException(SerializationInfo info, StreamingContext context)
			: base(info, context)
		{
			NestedException = (Exception)info.GetValue("NestedException", typeof(Exception));
		}

		public ModuleLoadExceptionHandlerException(string message, Exception innerException, Exception nestedException)
			: base(message, innerException)
		{
			NestedException = nestedException;
		}

		public override string ToString()
		{
			string text = ((InnerException == null) ? string.Empty : InnerException.ToString());
			string text2 = ((NestedException == null) ? string.Empty : NestedException.ToString());
			object[] array = new object[4]
			{
				GetType(),
				null,
				null,
				null
			};
			string text3 = ((Message == null) ? string.Empty : Message);
			array[1] = text3;
			string text4 = ((text == null) ? string.Empty : text);
			array[2] = text4;
			string text5 = ((text2 == null) ? string.Empty : text2);
			array[3] = text5;
			return string.Format("\n{0}: {1}\n--- Start of primary exception ---\n{2}\n--- End of primary exception ---\n\n--- Start of nested exception ---\n{3}\n--- End of nested exception ---\n", array);
		}

		public override void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			base.GetObjectData(info, context);
			info.AddValue("NestedException", NestedException, typeof(Exception));
		}
	}
}
