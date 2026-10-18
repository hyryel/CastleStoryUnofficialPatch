using System;

namespace Brix.External.Signals;

public interface IWrapper<TTuple, TAction>
{
	Action<TTuple> Wrap(TAction action);

	TAction Unwrap(Action<TTuple> wrappedAction);
}
