using System;
using System.Collections.Generic;

namespace Motus.Behavior;

public static class ParameteredSequenceBuilderExtender
{
	private static class Resolver<TBuilder, TBuilder2, P1> where TBuilder : IParameteredSequenceBuilder where TBuilder2 : IBuilderBase
	{
		public static TBuilder _Process(TBuilder b, Func<P1, TBuilder2> e)
		{
			Node node = DynamicNode.Make((P1 _p1, Func<P1, TBuilder2> _e) => _e(_p1).ToNode, e);
			b.AddNode(node);
			return b;
		}
	}

	private static class Resolver<TBuilder, TBuilder2, P1, P2> where TBuilder : IParameteredSequenceBuilder where TBuilder2 : IBuilderBase
	{
		public static TBuilder _Process(TBuilder b, Func<P1, P2, TBuilder2> e, P2 p2)
		{
			Node node = DynamicNode.Make((P1 _p1, P2 _p2, Func<P1, P2, TBuilder2> _e) => _e(_p1, _p2).ToNode, p2, e);
			b.AddNode(node);
			return b;
		}
	}

	private static class Resolver<TBuilder, TBuilder2, P1, P2, P3> where TBuilder : IParameteredSequenceBuilder where TBuilder2 : IBuilderBase
	{
		public static TBuilder _Process(TBuilder b, Func<P1, P2, P3, TBuilder2> e, P2 p2, P3 p3)
		{
			Node node = DynamicNode.Make((P1 _p1, P2 _p2, P3 _p3, Func<P1, P2, P3, TBuilder2> _e) => _e(_p1, _p2, _p3).ToNode, p2, p3, e);
			b.AddNode(node);
			return b;
		}
	}

	private static class Resolver<TBuilder, TBuilder2, P1, P2, P3, P4> where TBuilder : IParameteredSequenceBuilder where TBuilder2 : IBuilderBase
	{
		public static TBuilder _Process(TBuilder b, Func<P1, P2, P3, P4, TBuilder2> e, P2 p2, P3 p3, P4 p4)
		{
			Node node = DynamicNode.Make((P1 _p1, P2 _p2, P3 _p3, P4 _p4, Func<P1, P2, P3, P4, TBuilder2> _e) => _e(_p1, _p2, _p3, _p4).ToNode, p2, p3, p4, e);
			b.AddNode(node);
			return b;
		}
	}

	public static TBuilder Process<TBuilder, P1>(this TBuilder b, Action<P1> a) where TBuilder : IParameteredSequenceBuilder
	{
		b.AddNode(ActionNode.Make(a));
		return b;
	}

	public static TBuilder Process<TBuilder, P1, P2>(this TBuilder b, Action<P1, P2> a, P2 p2) where TBuilder : IParameteredSequenceBuilder
	{
		b.AddNode(ActionNode.Make(a, p2));
		return b;
	}

	public static TBuilder Process<TBuilder, P1, P2, P3>(this TBuilder b, Action<P1, P2, P3> a, P2 p2, P3 p3) where TBuilder : IParameteredSequenceBuilder
	{
		b.AddNode(ActionNode.Make(a, p2, p3));
		return b;
	}

	public static TBuilder Process<TBuilder, P1, P2, P3, P4>(this TBuilder b, Action<P1, P2, P3, P4> a, P2 p2, P3 p3, P4 p4) where TBuilder : IParameteredSequenceBuilder
	{
		b.AddNode(ActionNode.Make(a, p2, p3, p4));
		return b;
	}

	public static TBuilder Process<TBuilder, P1, TReturn>(this TBuilder b, Func<P1, TReturn> a) where TBuilder : IParameteredSequenceBuilder
	{
		b.AddNode(FuncNode.Make(a));
		return b;
	}

	public static TBuilder Process<TBuilder, P1, P2, TReturn>(this TBuilder b, Func<P1, P2, TReturn> a, P2 p2) where TBuilder : IParameteredSequenceBuilder
	{
		b.AddNode(FuncNode.Make(a, p2));
		return b;
	}

	public static TBuilder Process<TBuilder, P1, P2, P3, TReturn>(this TBuilder b, Func<P1, P2, P3, TReturn> a, P2 p2, P3 p3) where TBuilder : IParameteredSequenceBuilder
	{
		b.AddNode(FuncNode.Make(a, p2, p3));
		return b;
	}

	public static TBuilder Process<TBuilder, P1, P2, P3, P4, TReturn>(this TBuilder b, Func<P1, P2, P3, P4, TReturn> a, P2 p2, P3 p3, P4 p4) where TBuilder : IParameteredSequenceBuilder
	{
		b.AddNode(FuncNode.Make(a, p2, p3, p4));
		return b;
	}

	public static TBuilder Process<TBuilder, P1>(this TBuilder b, Func<P1, IEnumerator<ActivityProgress>> a) where TBuilder : IParameteredSequenceBuilder
	{
		b.AddNode(ActivityNode.Make(a));
		return b;
	}

	public static TBuilder Process<TBuilder, P1, P2>(this TBuilder b, Func<P1, P2, IEnumerator<ActivityProgress>> a, P2 p2) where TBuilder : IParameteredSequenceBuilder
	{
		b.AddNode(ActivityNode.Make(a, p2));
		return b;
	}

	public static TBuilder Process<TBuilder, P1, P2, P3>(this TBuilder b, Func<P1, P2, P3, IEnumerator<ActivityProgress>> a, P2 p2, P3 p3) where TBuilder : IParameteredSequenceBuilder
	{
		b.AddNode(ActivityNode.Make(a, p2, p3));
		return b;
	}

	public static TBuilder Process<TBuilder, P1, P2, P3, P4>(this TBuilder b, Func<P1, P2, P3, P4, IEnumerator<ActivityProgress>> a, P2 p2, P3 p3, P4 p4) where TBuilder : IParameteredSequenceBuilder
	{
		b.AddNode(ActivityNode.Make(a, p2, p3, p4));
		return b;
	}

	public static TBuilder Process<TBuilder, P1, TReturn>(this TBuilder b, Func<P1, IEnumerator<ComputationProgress<TReturn>>> a) where TBuilder : IParameteredSequenceBuilder
	{
		b.AddNode(ComputationNode.Make(a));
		return b;
	}

	public static TBuilder Process<TBuilder, P1, P2, TReturn>(this TBuilder b, Func<P1, P2, IEnumerator<ComputationProgress<TReturn>>> a, P2 p2) where TBuilder : IParameteredSequenceBuilder
	{
		b.AddNode(ComputationNode.Make(a, p2));
		return b;
	}

	public static TBuilder Process<TBuilder, P1, P2, P3, TReturn>(this TBuilder b, Func<P1, P2, P3, IEnumerator<ComputationProgress<TReturn>>> a, P2 p2, P3 p3) where TBuilder : IParameteredSequenceBuilder
	{
		b.AddNode(ComputationNode.Make(a, p2, p3));
		return b;
	}

	public static TBuilder Process<TBuilder, P1, P2, P3, P4, TReturn>(this TBuilder b, Func<P1, P2, P3, P4, IEnumerator<ComputationProgress<TReturn>>> a, P2 p2, P3 p3, P4 p4) where TBuilder : IParameteredSequenceBuilder
	{
		b.AddNode(ComputationNode.Make(a, p2, p3, p4));
		return b;
	}

	public static TBuilder Process<TBuilder, P1>(this TBuilder b, Func<P1, Node> e) where TBuilder : IParameteredSequenceBuilder
	{
		b.AddNode(DynamicNode.Make(e));
		return b;
	}

	public static TBuilder Process<TBuilder, P1, P2>(this TBuilder b, Func<P1, P2, Node> e, P2 p2) where TBuilder : IParameteredSequenceBuilder
	{
		b.AddNode(DynamicNode.Make(e, p2));
		return b;
	}

	public static TBuilder Process<TBuilder, P1, P2, P3>(this TBuilder b, Func<P1, P2, P3, Node> e, P2 p2, P3 p3) where TBuilder : IParameteredSequenceBuilder
	{
		b.AddNode(DynamicNode.Make(e, p2, p3));
		return b;
	}

	public static TBuilder Process<TBuilder, P1, P2, P3, P4>(this TBuilder b, Func<P1, P2, P3, P4, Node> e, P2 p2, P3 p3, P4 p4) where TBuilder : IParameteredSequenceBuilder
	{
		b.AddNode(DynamicNode.Make(e, p2, p3, p4));
		return b;
	}

	public static TBuilder Process<TBuilder, P1>(this TBuilder b, Func<P1, StaticNode> e) where TBuilder : IParameteredSequenceBuilder
	{
		b.AddNode(DynamicNode.Make(e));
		return b;
	}

	public static TBuilder Process<TBuilder, P1, P2>(this TBuilder b, Func<P1, P2, StaticNode> e, P2 p2) where TBuilder : IParameteredSequenceBuilder
	{
		b.AddNode(DynamicNode.Make(e, p2));
		return b;
	}

	public static TBuilder Process<TBuilder, P1, P2, P3>(this TBuilder b, Func<P1, P2, P3, StaticNode> e, P2 p2, P3 p3) where TBuilder : IParameteredSequenceBuilder
	{
		b.AddNode(DynamicNode.Make(e, p2, p3));
		return b;
	}

	public static TBuilder Process<TBuilder, P1, P2, P3, P4>(this TBuilder b, Func<P1, P2, P3, P4, StaticNode> e, P2 p2, P3 p3, P4 p4) where TBuilder : IParameteredSequenceBuilder
	{
		b.AddNode(DynamicNode.Make(e, p2, p3, p4));
		return b;
	}

	public static TBuilder Process<TBuilder, P1>(this TBuilder b, Func<P1, Sequencer.Builder> e) where TBuilder : IParameteredSequenceBuilder
	{
		return Resolver<TBuilder, Sequencer.Builder, P1>._Process(b, e);
	}

	public static TBuilder Process<TBuilder, P1>(this TBuilder b, Func<P1, ParallelSequencer.Builder> e) where TBuilder : IParameteredSequenceBuilder
	{
		return Resolver<TBuilder, ParallelSequencer.Builder, P1>._Process(b, e);
	}

	public static TBuilder Process<TBuilder, P1>(this TBuilder b, Func<P1, Brancher.Builder> e) where TBuilder : IParameteredSequenceBuilder
	{
		return Resolver<TBuilder, Brancher.Builder, P1>._Process(b, e);
	}

	public static TBuilder Process<TBuilder, P1>(this TBuilder b, Func<P1, Trier.Builder> e) where TBuilder : IParameteredSequenceBuilder
	{
		return Resolver<TBuilder, Trier.Builder, P1>._Process(b, e);
	}

	public static TBuilder Process<TBuilder, P1>(this TBuilder b, Func<P1, Catcher.Builder> e) where TBuilder : IParameteredSequenceBuilder
	{
		return Resolver<TBuilder, Catcher.Builder, P1>._Process(b, e);
	}

	public static TBuilder Process<TBuilder, P1>(this TBuilder b, Func<P1, TimedSequencer.Builder> e) where TBuilder : IParameteredSequenceBuilder
	{
		return Resolver<TBuilder, TimedSequencer.Builder, P1>._Process(b, e);
	}

	public static TBuilder Process<TBuilder, TItem, P1>(this TBuilder b, Func<P1, ForEacher.Builder<TItem>> e) where TBuilder : IParameteredSequenceBuilder
	{
		return Resolver<TBuilder, ForEacher.Builder<TItem>, P1>._Process(b, e);
	}

	public static TBuilder Process<TBuilder, P1, P2>(this TBuilder b, Func<P1, P2, Sequencer.Builder> e, P2 p2) where TBuilder : IParameteredSequenceBuilder
	{
		return Resolver<TBuilder, Sequencer.Builder, P1, P2>._Process(b, e, p2);
	}

	public static TBuilder Process<TBuilder, P1, P2>(this TBuilder b, Func<P1, P2, ParallelSequencer.Builder> e, P2 p2) where TBuilder : IParameteredSequenceBuilder
	{
		return Resolver<TBuilder, ParallelSequencer.Builder, P1, P2>._Process(b, e, p2);
	}

	public static TBuilder Process<TBuilder, P1, P2>(this TBuilder b, Func<P1, P2, Brancher.Builder> e, P2 p2) where TBuilder : IParameteredSequenceBuilder
	{
		return Resolver<TBuilder, Brancher.Builder, P1, P2>._Process(b, e, p2);
	}

	public static TBuilder Process<TBuilder, P1, P2>(this TBuilder b, Func<P1, P2, Trier.Builder> e, P2 p2) where TBuilder : IParameteredSequenceBuilder
	{
		return Resolver<TBuilder, Trier.Builder, P1, P2>._Process(b, e, p2);
	}

	public static TBuilder Process<TBuilder, P1, P2>(this TBuilder b, Func<P1, P2, Catcher.Builder> e, P2 p2) where TBuilder : IParameteredSequenceBuilder
	{
		return Resolver<TBuilder, Catcher.Builder, P1, P2>._Process(b, e, p2);
	}

	public static TBuilder Process<TBuilder, P1, P2>(this TBuilder b, Func<P1, P2, TimedSequencer.Builder> e, P2 p2) where TBuilder : IParameteredSequenceBuilder
	{
		return Resolver<TBuilder, TimedSequencer.Builder, P1, P2>._Process(b, e, p2);
	}

	public static TBuilder Process<TBuilder, TItem, P1, P2>(this TBuilder b, Func<P1, P2, ForEacher.Builder<TItem>> e, P2 p2) where TBuilder : IParameteredSequenceBuilder
	{
		return Resolver<TBuilder, ForEacher.Builder<TItem>, P1, P2>._Process(b, e, p2);
	}

	public static TBuilder Process<TBuilder, P1, P2, P3>(this TBuilder b, Func<P1, P2, P3, Sequencer.Builder> e, P2 p2, P3 p3) where TBuilder : IParameteredSequenceBuilder
	{
		return Resolver<TBuilder, Sequencer.Builder, P1, P2, P3>._Process(b, e, p2, p3);
	}

	public static TBuilder Process<TBuilder, P1, P2, P3>(this TBuilder b, Func<P1, P2, P3, ParallelSequencer.Builder> e, P2 p2, P3 p3) where TBuilder : IParameteredSequenceBuilder
	{
		return Resolver<TBuilder, ParallelSequencer.Builder, P1, P2, P3>._Process(b, e, p2, p3);
	}

	public static TBuilder Process<TBuilder, P1, P2, P3>(this TBuilder b, Func<P1, P2, P3, Brancher.Builder> e, P2 p2, P3 p3) where TBuilder : IParameteredSequenceBuilder
	{
		return Resolver<TBuilder, Brancher.Builder, P1, P2, P3>._Process(b, e, p2, p3);
	}

	public static TBuilder Process<TBuilder, P1, P2, P3>(this TBuilder b, Func<P1, P2, P3, Trier.Builder> e, P2 p2, P3 p3) where TBuilder : IParameteredSequenceBuilder
	{
		return Resolver<TBuilder, Trier.Builder, P1, P2, P3>._Process(b, e, p2, p3);
	}

	public static TBuilder Process<TBuilder, P1, P2, P3>(this TBuilder b, Func<P1, P2, P3, Catcher.Builder> e, P2 p2, P3 p3) where TBuilder : IParameteredSequenceBuilder
	{
		return Resolver<TBuilder, Catcher.Builder, P1, P2, P3>._Process(b, e, p2, p3);
	}

	public static TBuilder Process<TBuilder, P1, P2, P3>(this TBuilder b, Func<P1, P2, P3, TimedSequencer.Builder> e, P2 p2, P3 p3) where TBuilder : IParameteredSequenceBuilder
	{
		return Resolver<TBuilder, TimedSequencer.Builder, P1, P2, P3>._Process(b, e, p2, p3);
	}

	public static TBuilder Process<TBuilder, TItem, P1, P2, P3>(this TBuilder b, Func<P1, P2, P3, ForEacher.Builder<TItem>> e, P2 p2, P3 p3) where TBuilder : IParameteredSequenceBuilder
	{
		return Resolver<TBuilder, ForEacher.Builder<TItem>, P1, P2, P3>._Process(b, e, p2, p3);
	}

	public static TBuilder Process<TBuilder, P1, P2, P3, P4>(this TBuilder b, Func<P1, P2, P3, P4, Sequencer.Builder> e, P2 p2, P3 p3, P4 p4) where TBuilder : IParameteredSequenceBuilder
	{
		return Resolver<TBuilder, Sequencer.Builder, P1, P2, P3, P4>._Process(b, e, p2, p3, p4);
	}

	public static TBuilder Process<TBuilder, P1, P2, P3, P4>(this TBuilder b, Func<P1, P2, P3, P4, ParallelSequencer.Builder> e, P2 p2, P3 p3, P4 p4) where TBuilder : IParameteredSequenceBuilder
	{
		return Resolver<TBuilder, ParallelSequencer.Builder, P1, P2, P3, P4>._Process(b, e, p2, p3, p4);
	}

	public static TBuilder Process<TBuilder, P1, P2, P3, P4>(this TBuilder b, Func<P1, P2, P3, P4, Brancher.Builder> e, P2 p2, P3 p3, P4 p4) where TBuilder : IParameteredSequenceBuilder
	{
		return Resolver<TBuilder, Brancher.Builder, P1, P2, P3, P4>._Process(b, e, p2, p3, p4);
	}

	public static TBuilder Process<TBuilder, P1, P2, P3, P4>(this TBuilder b, Func<P1, P2, P3, P4, Trier.Builder> e, P2 p2, P3 p3, P4 p4) where TBuilder : IParameteredSequenceBuilder
	{
		return Resolver<TBuilder, Trier.Builder, P1, P2, P3, P4>._Process(b, e, p2, p3, p4);
	}

	public static TBuilder Process<TBuilder, P1, P2, P3, P4>(this TBuilder b, Func<P1, P2, P3, P4, Catcher.Builder> e, P2 p2, P3 p3, P4 p4) where TBuilder : IParameteredSequenceBuilder
	{
		return Resolver<TBuilder, Catcher.Builder, P1, P2, P3, P4>._Process(b, e, p2, p3, p4);
	}

	public static TBuilder Process<TBuilder, P1, P2, P3, P4>(this TBuilder b, Func<P1, P2, P3, P4, TimedSequencer.Builder> e, P2 p2, P3 p3, P4 p4) where TBuilder : IParameteredSequenceBuilder
	{
		return Resolver<TBuilder, TimedSequencer.Builder, P1, P2, P3, P4>._Process(b, e, p2, p3, p4);
	}

	public static TBuilder Process<TBuilder, TItem, P1, P2, P3, P4>(this TBuilder b, Func<P1, P2, P3, P4, ForEacher.Builder<TItem>> e, P2 p2, P3 p3, P4 p4) where TBuilder : IParameteredSequenceBuilder
	{
		return Resolver<TBuilder, ForEacher.Builder<TItem>, P1, P2, P3, P4>._Process(b, e, p2, p3, p4);
	}
}
