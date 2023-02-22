using System;

namespace Microsoft.WindowsPhone.Imaging
{
	public abstract class InputRule
	{
		public string Property { get; set; }

		public string Mode { get; set; }

		public char ModeCharacter
		{
			get
			{
				if (string.CompareOrdinal(Mode, "AFFIRMATIVE") == 0)
				{
					return 'A';
				}
				if (string.CompareOrdinal(Mode, "NEGATIVE") == 0)
				{
					return 'N';
				}
				if (string.CompareOrdinal(Mode, "OPTIONAL") == 0)
				{
					return 'O';
				}
				throw new ArgumentException("Mode");
			}
		}
	}
}
