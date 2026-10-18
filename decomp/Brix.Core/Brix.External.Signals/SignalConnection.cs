using Brix.Utils.Collections;

namespace Brix.External.Signals;

public struct SignalConnection
{
	internal readonly IDisconnectableSignal Signal;

	internal readonly StructLinkedListNode Node;

	public static readonly SignalConnection Empty = new SignalConnection(null, StructLinkedListNode.Empty);

	public bool IsConnected => Signal != null && Signal.IsConnected(this);

	public override int GetHashCode()
	{
		return (((Signal != null) ? Signal.GetHashCode() : 0) * 397) ^ Node.GetHashCode();
	}

	public bool Equals(SignalConnection other)
	{
		return object.Equals(Signal, other.Signal) && object.Equals(Node, other.Node);
	}

	public override bool Equals(object obj)
	{
		if (obj == null)
		{
			return false;
		}
		return obj is SignalConnection && Equals((SignalConnection)obj);
	}

	public static bool operator ==(SignalConnection left, SignalConnection right)
	{
		return left.Equals(right);
	}

	public static bool operator !=(SignalConnection left, SignalConnection right)
	{
		return !left.Equals(right);
	}

	internal SignalConnection(IDisconnectableSignal signal, StructLinkedListNode node)
	{
		Signal = signal;
		Node = node;
	}

	public bool Disconnect()
	{
		return Signal != null && Signal.Disconnect(this);
	}
}
