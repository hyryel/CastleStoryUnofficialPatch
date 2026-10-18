using System;
using Brix.Engine;
using Brix.External.Signals;
using Brix.Game.Components;
using Brix.Game.Semantique;
using Motus.Behavior;
using UnityEngine;

namespace Brix.Game.AI.Nodes;

public static class WorkNodesWrappers
{
	public delegate Node NodeWrappingFunc<T>(Node node, T goal, Labor labor);

	private static readonly Action<Labor> ToggleOnEatGarnotte = delegate(Labor l)
	{
		l.eater.Eat();
	};

	private static readonly Action<Labor> StopToggleGarnotte = delegate(Labor l)
	{
		if (!l.IsNullOrReleased())
		{
			l.eater.StarveIn(3f);
		}
	};

	private static readonly Action<GameObject, Color, Signal<GameObject>> HighlightGO = delegate(GameObject go, Color color, Signal<GameObject> s)
	{
		Highlighter.highlight.Object(go, color, s);
	};

	private static readonly Action<Signal<GameObject>> StopHighlight = delegate(Signal<GameObject> s)
	{
		s.Invoke(null);
	};

	private static readonly Action<XYZ, Color, Signal<GameObject>> HighlightAt = delegate(XYZ pos, Color color, Signal<GameObject> s)
	{
		Highlighter.highlight.At(pos, color, s);
	};

	public static Node EatGarnotteWhileWorking<T>(Node node, T component, Labor labor)
	{
		return (StaticNode)Node.Sequence.Label("EatGarnotteWhileWorking Sequence").Do(ToggleOnEatGarnotte, labor).Label("ToggleOnEatGarnotte")
			.Do((StaticNode)node)
			.Finally(StopToggleGarnotte, labor);
	}

	public static Node HighlightTargetRed<T>(Node node, T component, Labor labor) where T : GameComponent
	{
		return _HighlightTarget(node, component, Color.red);
	}

	public static Node HighlightTargetYellow<T>(Node node, T component, Labor labor) where T : GameComponent
	{
		return _HighlightTarget(node, component, new Color(1f, 0.77f, 0.04f));
	}

	public static Node HighlightTargetBlue<T>(Node node, T component, Labor labor) where T : GameComponent
	{
		return _HighlightTarget(node, component, Color.blue);
	}

	public static Node HighlightAtRed<T>(Node node, T component, Labor labor) where T : GameComponent
	{
		return _HighlightAt(node, component.voxel.position, Color.red);
	}

	public static Node HighlightAtYellow<T>(Node node, T component, Labor labor) where T : GameComponent
	{
		return _HighlightAt(node, component.voxel.position, new Color(1f, 0.77f, 0.04f));
	}

	public static Node HighlightAtBlue<T>(Node node, T component, Labor labor) where T : GameComponent
	{
		return _HighlightAt(node, component.voxel.position, Color.blue);
	}

	private static Node _HighlightTarget<T>(Node node, T component, Color color) where T : GameComponent
	{
		Signal<GameObject> signal = new Signal<GameObject>();
		return (StaticNode)Node.Sequence.Label("HighlightTarget Sequence").Do(HighlightGO, component.gameObject, color, signal).Label("HighlightAt")
			.Do(node as StaticNode)
			.Finally(StopHighlight, signal);
	}

	private static Node _HighlightAt(Node node, XYZ pos, Color color)
	{
		Signal<GameObject> signal = new Signal<GameObject>();
		return (StaticNode)Node.Sequence.Label("HighlightTarget Sequence").Do(HighlightAt, pos, color, signal).Label("HighlightAt")
			.Do(node as StaticNode)
			.Finally(StopHighlight, signal);
	}

	public static Node GoStoreYourCrapFirst(Node node, Labor labor, Profile acceptedStuff)
	{
		return GoStoreYourCrapFirst((StaticNode)node, labor, acceptedStuff.description);
	}

	public static StaticNode GoStoreYourCrapFirst(StaticNode node, Labor labor, Description acceptedStuff)
	{
		return Node.Selector.Label("GoStoreYourCrapFirst Selector").When(CarriesUselessStuff, labor, acceptedStuff).Label("When CarriesUselessStuff")
			.Do(delegate(Labor l)
			{
				Order.Transfer.GoStore.Send(l);
			}, labor)
			.Label("Send GoStore Order")
			.Else()
			.Label("When doesn't CarryUselessStuff")
			.Do(node);
	}

	private static bool CarriesUselessStuff(Labor labor, Description acceptedStuff)
	{
		return labor.recepteur.CarriesSomethingElseThan(acceptedStuff);
	}

	public static StaticNode IfEnoughTimeElapsed(StaticNode node, Labor labor, float delay, Labor.DecisionSource source)
	{
		return Node.Selector.Label("IfEnoughTimeElapsed").If((Labor l, Labor.DecisionSource s, float d) => l.TimeSinceLastOrder(s) < d, labor, source, delay).Label("When NotEnoughTimeElapsed")
			.Do(Node.Empty)
			.Else()
			.Label("When EnoughTimeHasElapsed")
			.Do(node);
	}
}
