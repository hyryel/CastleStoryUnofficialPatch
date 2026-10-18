using System.Collections.Generic;
using Brix.External.Signals;

namespace Brix.UI.Builder.Menu;

public interface ITargetSignalConnector<TTarget, TLife> where TLife : struct
{
	Dictionary<TLife, KeyValuePair<List<TargetLifeSignalConnectionBase<TTarget>>, Signal>> TargetSignalMap { get; set; }

	Signal OnClearSignal { get; }
}
