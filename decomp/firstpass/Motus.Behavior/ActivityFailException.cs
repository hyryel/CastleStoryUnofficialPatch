namespace Motus.Behavior;

public class ActivityFailException : FailException
{
	public ActivityFailException()
	{
	}

	public ActivityFailException(string msg)
		: base(msg)
	{
	}
}
