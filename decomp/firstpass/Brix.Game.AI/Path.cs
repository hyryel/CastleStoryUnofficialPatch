using System;
using System.Collections.Generic;
using Brix.Engine;
using UnityEngine;

namespace Brix.Game.AI;

public class Path : IPath
{
	public XYZ start;

	public List<WalkNode> nodes;

	public Func<XYZ, bool> presenceTest;

	public Location destinationLocation;

	private static Func<XYZ, bool> DefaultPresenceTest = (XYZ p) => true;

	public bool IsEmpty => nodes.Count == 0 || nodes[0] == null;

	public Func<XYZ, bool> PresenceTest => presenceTest;

	public IEnumerator<WalkNode> NodesEnumerator => nodes.GetEnumerator();

	public XYZ Start => start;

	public Location DestinationLocation => destinationLocation;

	public Path(List<WalkNode> nodes, Func<XYZ, bool> presenceTest = null)
		: this((nodes.Count <= 0) ? Vector3.zero : nodes[0].Position, null, nodes, presenceTest)
	{
	}

	public Path(Vector3 start, List<WalkNode> nodes, Func<XYZ, bool> presenceTest = null)
		: this(start, null, nodes, presenceTest)
	{
	}

	public Path(Vector3 start, Location destinationLocation, List<WalkNode> nodes, Func<XYZ, bool> presenceTest = null)
	{
		this.start = XYZ.FromVector3(start);
		this.destinationLocation = destinationLocation;
		this.nodes = nodes;
		this.presenceTest = presenceTest ?? DefaultPresenceTest;
	}
}
