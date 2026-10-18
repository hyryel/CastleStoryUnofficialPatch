using System;

namespace Brix.Assets;

public class SaveFileException : Exception
{
	public SaveFileException(string message)
		: base(message)
	{
	}

	public SaveFileException(string message, Exception inner)
		: base(message, inner)
	{
	}
}
