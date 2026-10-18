using System;
using RakNet;

namespace Network;

public class MatchAddress : SystemAddress, IEquatable<MatchAddress>
{
	public MatchAddress(string str, ushort port)
		: base(str, port)
	{
	}

	public bool Equals(MatchAddress other)
	{
		if (object.ReferenceEquals(this, other))
		{
			return true;
		}
		if ((object)other == null)
		{
			return false;
		}
		return Equals((SystemAddress)other);
	}
}
