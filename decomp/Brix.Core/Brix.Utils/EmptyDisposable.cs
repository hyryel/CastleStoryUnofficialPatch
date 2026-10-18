using System;

namespace Brix.Utils;

internal class EmptyDisposable : IDisposable
{
	public static readonly EmptyDisposable Instance = new EmptyDisposable();

	public void Dispose()
	{
	}
}
