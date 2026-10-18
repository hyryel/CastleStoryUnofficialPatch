using System;

namespace Brix.Utils.Closures;

public interface IClosure<TDelegate> : IDisposable
{
	TDelegate Delegate { get; }
}
