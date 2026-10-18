using System;

namespace Motus.Behavior;

public class FailException : Exception
{
	public static FailException Generic = new FailException();

	public FailException()
	{
	}

	public FailException(string msg)
		: base(msg)
	{
	}
}
