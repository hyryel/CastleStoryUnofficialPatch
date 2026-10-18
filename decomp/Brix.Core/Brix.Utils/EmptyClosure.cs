using System;
using Brix.Utils.Closures;

namespace Brix.Utils;

internal class EmptyClosure : IClosure<IAction>, IDisposable
{
	public static IAction EmptyAction = Closure.Make(delegate
	{
	});

	public static readonly EmptyClosure Instance = new EmptyClosure();

	public IAction Delegate => EmptyAction;

	public void Dispose()
	{
	}
}
