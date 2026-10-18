using System;
using System.Collections.Generic;
using Brix.Engine;

namespace Brix.Game.AI;

public interface IPath
{
	bool IsEmpty { get; }

	IEnumerator<WalkNode> NodesEnumerator { get; }

	Func<XYZ, bool> PresenceTest { get; }

	XYZ Start { get; }

	Location DestinationLocation { get; }
}
