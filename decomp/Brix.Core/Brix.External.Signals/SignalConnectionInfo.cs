using System;

namespace Brix.External.Signals;

internal struct SignalConnectionInfo<TAction> where TAction : class, IDisposable
{
	public readonly TAction Action;

	public readonly int BirthFrame;

	public readonly bool AutoDisconnect;

	public SignalConnectionInfo(TAction action, int birthFrame, bool autoDisconnect)
	{
		Action = action;
		BirthFrame = birthFrame;
		AutoDisconnect = autoDisconnect;
	}
}
