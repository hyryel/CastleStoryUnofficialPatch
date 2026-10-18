namespace Brix.Engine.FlatWorld;

internal static class StepIntExtender
{
	internal static bool Contains(this int i, Step step)
	{
		return (i & (1 << (int)step)) != 0;
	}

	internal static T Select<T>(this int i, T a, object b, Step step)
	{
		T result = a;
		if (i.Contains(step))
		{
			result = (T)b;
		}
		return result;
	}
}
