namespace Motus.Behavior;

public static class NodeExtenders
{
	public static Looper.Builder Loop(this Node b)
	{
		return Node.Loop(b);
	}

	public static Looper.Builder Loop(this StaticNode b)
	{
		return Node.Loop(b);
	}
}
