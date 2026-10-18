using System;

namespace Brix.Network;

public class ReseauContext : IDisposable
{
	public bool isLocal()
	{
		return true;
	}

	public void Dispose()
	{
	}
}
