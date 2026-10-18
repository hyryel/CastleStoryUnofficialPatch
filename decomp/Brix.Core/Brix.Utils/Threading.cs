using System.Diagnostics;
using System.Reflection;
using System.Threading;

namespace Brix.Utils;

public static class Threading
{
	public static Thread MainThread;

	public static bool IsOnMainThread
	{
		get
		{
			if (Thread.CurrentThread == MainThread)
			{
				return true;
			}
			if (MainThread == null && IsTopMethodOfStackEntryPointOfAssembly())
			{
				MainThread = Thread.CurrentThread;
				return true;
			}
			return false;
		}
	}

	private static bool IsTopMethodOfStackEntryPointOfAssembly()
	{
		if (Thread.CurrentThread.GetApartmentState() == ApartmentState.STA && !Thread.CurrentThread.IsBackground && !Thread.CurrentThread.IsThreadPoolThread && Thread.CurrentThread.IsAlive)
		{
			MethodInfo entryPoint = Assembly.GetEntryAssembly().EntryPoint;
			StackTrace stackTrace = new StackTrace();
			StackFrame[] frames = stackTrace.GetFrames();
			for (int num = frames.Length - 1; num >= 0; num--)
			{
				MethodBase method = frames[num].GetMethod();
				if (entryPoint == method)
				{
					return true;
				}
			}
		}
		return false;
	}
}
