using System.Collections.Generic;
using Brix.External.Signals;
using Smooth.Algebraics;

namespace Brix.UI.Builder.Menu;

public interface ISignalConnector<TLife> where TLife : struct
{
	Dictionary<TLife, Tuple<List<LifeSignalConnectionBase>, Signal>> SignalMap { get; set; }

	Signal OnClearSignal { get; }
}
