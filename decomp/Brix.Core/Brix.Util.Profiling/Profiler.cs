namespace Brix.Util.Profiling;

internal static class Profiler
{
	public static IProfiler Null = new NullProfiler();

	public static IProfiler Game = new UnityLogProfiler("Game");

	public static IProfiler Engine = new UnityProfiler("Engine");
}
