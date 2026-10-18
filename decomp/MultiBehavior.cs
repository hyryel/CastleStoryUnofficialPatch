using System;
using System.Collections.Generic;
using Brix.Game.AI;
using Brix.Game.Components;
using Motus.Behavior;
using Smooth.Slinq;
using UnityEngine.Networking;

public class MultiBehavior : Behavior
{
	private List<Sequence> sequences = new List<Sequence>();

	protected override Node CreateTree()
	{
		return ToTree().FailWhen(FailException.Generic).Condition(GameComponentExtender.IsNullOrReleased, this).ToNode;
	}

	private Node ToTree()
	{
		if (NetworkServer.active)
		{
			return (StaticNode)Node.Parallel.ForEachIn(sequences, (Sequence seq) => seq.Create());
		}
		return Node.Forever;
	}

	protected void AddOrSetSequence(Node node, int tics, string name)
	{
		Sequence sequence = sequences.Slinq().Where((Sequence s, string n) => s.GetName() == n, name).FirstOrDefault();
		if (sequence == null)
		{
			sequence = new Sequence(name);
			sequences.Add(sequence);
		}
		sequence.Set(node as StaticNode, Node.WaitForTicks(tics));
	}

	protected void AddOrSetSequence(Action action, int tics, string name)
	{
		AddOrSetSequence(Node.Do(delegate(Action a)
		{
			a();
		}, action), tics, name);
	}
}
You are not using the latest version of the tool, please update.
Latest version is '11.1.0.9782' (yours is '9.1.0.7988')
