using System;
using System.Collections;
using System.Collections.Generic;

namespace Motus.Behavior;

public static class SequenceBuilderExtender
{
	private static class Resolver<TBuilder, TBuilder2> where TBuilder : ISequenceBuilder where TBuilder2 : IBuilderBase
	{
		public static TBuilder _Do(TBuilder b, Func<TBuilder2> e)
		{
			Node node = DynamicNode.Make((Func<TBuilder2> _e) => _e().ToNode, e);
			b.AddNode(node);
			return b;
		}
	}

	private static class Resolver<TBuilder, TBuilder2, P1> where TBuilder : ISequenceBuilder where TBuilder2 : IBuilderBase
	{
		public static TBuilder _Do(TBuilder b, Func<P1, TBuilder2> e, P1 p1)
		{
			Node node = DynamicNode.Make((Func<P1, TBuilder2> _e, P1 _p1) => _e(_p1).ToNode, e, p1);
			b.AddNode(node);
			return b;
		}
	}

	private static class Resolver<TBuilder, TBuilder2, P1, P2> where TBuilder : ISequenceBuilder where TBuilder2 : IBuilderBase
	{
		public static TBuilder _Do(TBuilder b, Func<P1, P2, TBuilder2> e, P1 p1, P2 p2)
		{
			Node node = DynamicNode.Make((Func<P1, P2, TBuilder2> _e, P1 _p1, P2 _p2) => _e(_p1, _p2).ToNode, e, p1, p2);
			b.AddNode(node);
			return b;
		}
	}

	private static class Resolver<TBuilder, TBuilder2, P1, P2, P3> where TBuilder : ISequenceBuilder where TBuilder2 : IBuilderBase
	{
		public static TBuilder _Do(TBuilder b, Func<P1, P2, P3, TBuilder2> e, P1 p1, P2 p2, P3 p3)
		{
			Node node = DynamicNode.Make((Func<P1, P2, P3, TBuilder2> _e, P1 _p1, P2 _p2, P3 _p3) => _e(_p1, _p2, _p3).ToNode, e, p1, p2, p3);
			b.AddNode(node);
			return b;
		}
	}

	private static class Resolver<TBuilder, TBuilder2, P1, P2, P3, P4> where TBuilder : ISequenceBuilder where TBuilder2 : IBuilderBase
	{
		public static TBuilder _Do(TBuilder t, Func<P1, P2, P3, P4, TBuilder2> e, P1 p1, P2 p2, P3 p3, P4 p4)
		{
			Node node = DynamicNode.Make((Func<P1, P2, P3, P4, TBuilder2> _e, P1 _p1, P2 _p2, P3 _p3, P4 _p4) => _e(_p1, _p2, _p3, _p4).ToNode, e, p1, p2, p3, p4);
			t.AddNode(node);
			return t;
		}
	}

	public static TBuilder Do<TBuilder>(this TBuilder b, Action a) where TBuilder : ISequenceBuilder
	{
		b.AddNode(ActionNode.Make(a));
		return b;
	}

	public static TBuilder Do<TBuilder, P1>(this TBuilder b, Action<P1> a, P1 p1) where TBuilder : ISequenceBuilder
	{
		b.AddNode(ActionNode.Make(a, p1));
		return b;
	}

	public static TBuilder Do<TBuilder, P1, P2>(this TBuilder b, Action<P1, P2> a, P1 p1, P2 p2) where TBuilder : ISequenceBuilder
	{
		b.AddNode(ActionNode.Make(a, p1, p2));
		return b;
	}

	public static TBuilder Do<TBuilder, P1, P2, P3>(this TBuilder b, Action<P1, P2, P3> a, P1 p1, P2 p2, P3 p3) where TBuilder : ISequenceBuilder
	{
		b.AddNode(ActionNode.Make(a, p1, p2, p3));
		return b;
	}

	public static TBuilder Do<TBuilder, P1, P2, P3, P4>(this TBuilder b, Action<P1, P2, P3, P4> a, P1 p1, P2 p2, P3 p3, P4 p4) where TBuilder : ISequenceBuilder
	{
		b.AddNode(ActionNode.Make(a, p1, p2, p3, p4));
		return b;
	}

	public static TBuilder Do<TBuilder, TReturn>(this TBuilder b, Func<TReturn> a) where TBuilder : ISequenceBuilder
	{
		b.AddNode(FuncNode.Make(a));
		return b;
	}

	public static TBuilder Do<TBuilder, P1, TReturn>(this TBuilder b, Func<P1, TReturn> a, P1 p1) where TBuilder : ISequenceBuilder
	{
		b.AddNode(FuncNode.Make(a, p1));
		return b;
	}

	public static TBuilder Do<TBuilder, P1, P2, TReturn>(this TBuilder b, Func<P1, P2, TReturn> a, P1 p1, P2 p2) where TBuilder : ISequenceBuilder
	{
		b.AddNode(FuncNode.Make(a, p1, p2));
		return b;
	}

	public static TBuilder Do<TBuilder, P1, P2, P3, TReturn>(this TBuilder b, Func<P1, P2, P3, TReturn> a, P1 p1, P2 p2, P3 p3) where TBuilder : ISequenceBuilder
	{
		b.AddNode(FuncNode.Make(a, p1, p2, p3));
		return b;
	}

	public static TBuilder Do<TBuilder, P1, P2, P3, P4, TReturn>(this TBuilder b, Func<P1, P2, P3, P4, TReturn> a, P1 p1, P2 p2, P3 p3, P4 p4) where TBuilder : ISequenceBuilder
	{
		b.AddNode(FuncNode.Make(a, p1, p2, p3, p4));
		return b;
	}

	public static TBuilder Do<TBuilder>(this TBuilder b, Func<IEnumerator<ActivityProgress>> f) where TBuilder : ISequenceBuilder
	{
		StaticNode node = ActivityNode.Make(f);
		b.AddNode(node);
		return b;
	}

	public static TBuilder Do<TBuilder, P1>(this TBuilder b, Func<P1, IEnumerator<ActivityProgress>> f, P1 p1) where TBuilder : ISequenceBuilder
	{
		StaticNode node = ActivityNode.Make(f, p1);
		b.AddNode(node);
		return b;
	}

	public static TBuilder Do<TBuilder, P1, P2>(this TBuilder b, Func<P1, P2, IEnumerator<ActivityProgress>> f, P1 p1, P2 p2) where TBuilder : ISequenceBuilder
	{
		StaticNode node = ActivityNode.Make(f, p1, p2);
		b.AddNode(node);
		return b;
	}

	public static TBuilder Do<TBuilder, P1, P2, P3>(this TBuilder b, Func<P1, P2, P3, IEnumerator<ActivityProgress>> f, P1 p1, P2 p2, P3 p3) where TBuilder : ISequenceBuilder
	{
		StaticNode node = ActivityNode.Make(f, p1, p2, p3);
		b.AddNode(node);
		return b;
	}

	public static TBuilder Do<TBuilder, P1, P2, P3, P4>(this TBuilder b, Func<P1, P2, P3, P4, IEnumerator<ActivityProgress>> f, P1 p1, P2 p2, P3 p3, P4 p4) where TBuilder : ISequenceBuilder
	{
		StaticNode node = ActivityNode.Make(f, p1, p2, p3, p4);
		b.AddNode(node);
		return b;
	}

	public static TBuilder Do<TBuilder, TResult>(this TBuilder b, Func<IEnumerator<ComputationProgress<TResult>>> f) where TBuilder : ISequenceBuilder
	{
		Node node = ComputationNode.Make(f);
		b.AddNode(node);
		return b;
	}

	public static TBuilder Do<TBuilder, P1, TResult>(this TBuilder b, Func<P1, IEnumerator<ComputationProgress<TResult>>> f, P1 p1) where TBuilder : ISequenceBuilder
	{
		Node node = ComputationNode.Make(f, p1);
		b.AddNode(node);
		return b;
	}

	public static TBuilder Do<TBuilder, P1, P2, TResult>(this TBuilder b, Func<P1, P2, IEnumerator<ComputationProgress<TResult>>> f, P1 p1, P2 p2) where TBuilder : ISequenceBuilder
	{
		Node node = ComputationNode.Make(f, p1, p2);
		b.AddNode(node);
		return b;
	}

	public static TBuilder Do<TBuilder, P1, P2, P3, TResult>(this TBuilder b, Func<P1, P2, P3, IEnumerator<ComputationProgress<TResult>>> f, P1 p1, P2 p2, P3 p3) where TBuilder : ISequenceBuilder
	{
		Node node = ComputationNode.Make(f, p1, p2, p3);
		b.AddNode(node);
		return b;
	}

	public static TBuilder Do<TBuilder, P1, P2, P3, P4, TResult>(this TBuilder b, Func<P1, P2, P3, P4, IEnumerator<ComputationProgress<TResult>>> f, P1 p1, P2 p2, P3 p3, P4 p4) where TBuilder : ISequenceBuilder
	{
		Node node = ComputationNode.Make(f, p1, p2, p3, p4);
		b.AddNode(node);
		return b;
	}

	public static TBuilder Do<TBuilder, TBuilderParam>(this TBuilder b, TBuilderParam n) where TBuilder : ISequenceBuilder where TBuilderParam : IBuilderBase
	{
		b.AddNode(n.ToNode);
		return b;
	}

	public static TBuilder Do<TBuilder>(this TBuilder b, StaticNode n) where TBuilder : ISequenceBuilder
	{
		b.AddNode(n);
		return b;
	}

	public static TBuilder Do<TBuilder>(this TBuilder b, Func<Node> e) where TBuilder : ISequenceBuilder
	{
		b.AddNode(DynamicNode.Make(e));
		return b;
	}

	public static TBuilder Do<TBuilder, P1>(this TBuilder b, Func<P1, Node> e, P1 p1) where TBuilder : ISequenceBuilder
	{
		b.AddNode(DynamicNode.Make(e, p1));
		return b;
	}

	public static TBuilder Do<TBuilder, P1, P2>(this TBuilder b, Func<P1, P2, Node> e, P1 p1, P2 p2) where TBuilder : ISequenceBuilder
	{
		b.AddNode(DynamicNode.Make(e, p1, p2));
		return b;
	}

	public static TBuilder Do<TBuilder, P1, P2, P3>(this TBuilder b, Func<P1, P2, P3, Node> e, P1 p1, P2 p2, P3 p3) where TBuilder : ISequenceBuilder
	{
		b.AddNode(DynamicNode.Make(e, p1, p2, p3));
		return b;
	}

	public static TBuilder Do<TBuilder, P1, P2, P3, P4>(this TBuilder b, Func<P1, P2, P3, P4, Node> e, P1 p1, P2 p2, P3 p3, P4 p4) where TBuilder : ISequenceBuilder
	{
		b.AddNode(DynamicNode.Make(e, p1, p2, p3, p4));
		return b;
	}

	public static TBuilder Do<TBuilder>(this TBuilder b, Func<StaticNode> e) where TBuilder : ISequenceBuilder
	{
		b.AddNode(DynamicNode.Make(e));
		return b;
	}

	public static TBuilder Do<TBuilder, P1>(this TBuilder b, Func<P1, StaticNode> e, P1 p1) where TBuilder : ISequenceBuilder
	{
		b.AddNode(DynamicNode.Make(e, p1));
		return b;
	}

	public static TBuilder Do<TBuilder, P1, P2>(this TBuilder b, Func<P1, P2, StaticNode> e, P1 p1, P2 p2) where TBuilder : ISequenceBuilder
	{
		b.AddNode(DynamicNode.Make(e, p1, p2));
		return b;
	}

	public static TBuilder Do<TBuilder, P1, P2, P3>(this TBuilder b, Func<P1, P2, P3, StaticNode> e, P1 p1, P2 p2, P3 p3) where TBuilder : ISequenceBuilder
	{
		b.AddNode(DynamicNode.Make(e, p1, p2, p3));
		return b;
	}

	public static TBuilder Do<TBuilder, P1, P2, P3, P4>(this TBuilder b, Func<P1, P2, P3, P4, StaticNode> e, P1 p1, P2 p2, P3 p3, P4 p4) where TBuilder : ISequenceBuilder
	{
		b.AddNode(DynamicNode.Make(e, p1, p2, p3, p4));
		return b;
	}

	[Obsolete]
	public static TBuilder Do<TBuilder>(this TBuilder b, Func<IEnumerator> f) where TBuilder : ISequenceBuilder
	{
		return b;
	}

	[Obsolete]
	public static TBuilder Do<TBuilder, P1>(this TBuilder b, Func<P1, IEnumerator> f, P1 p1) where TBuilder : ISequenceBuilder
	{
		return b;
	}

	[Obsolete]
	public static TBuilder Do<TBuilder, P1, P2>(this TBuilder b, Func<P1, P2, IEnumerator> f, P1 p1, P2 p2) where TBuilder : ISequenceBuilder
	{
		return b;
	}

	[Obsolete]
	public static TBuilder Do<TBuilder, P1, P2, P3>(this TBuilder b, Func<P1, P2, P3, IEnumerator> f, P1 p1, P2 p2, P3 p3) where TBuilder : ISequenceBuilder
	{
		return b;
	}

	[Obsolete]
	public static TBuilder Do<TBuilder, P1, P2, P3, P4>(this TBuilder b, Func<P1, P2, P3, P4, IEnumerator> f, P1 p1, P2 p2, P3 p3, P4 p4) where TBuilder : ISequenceBuilder
	{
		return b;
	}

	[Obsolete]
	public static TBuilder Do<TBuilder>(this TBuilder b, Func<IEnumerable<ActivityProgress>> f) where TBuilder : ISequenceBuilder
	{
		return b;
	}

	[Obsolete]
	public static TBuilder Do<TBuilder, P1>(this TBuilder b, Func<P1, IEnumerable<ActivityProgress>> f, P1 p1) where TBuilder : ISequenceBuilder
	{
		return b;
	}

	[Obsolete]
	public static TBuilder Do<TBuilder, P1, P2>(this TBuilder b, Func<P1, P2, IEnumerable<ActivityProgress>> f, P1 p1, P2 p2) where TBuilder : ISequenceBuilder
	{
		return b;
	}

	[Obsolete]
	public static TBuilder Do<TBuilder, P1, P2, P3>(this TBuilder b, Func<P1, P2, P3, IEnumerable<ActivityProgress>> f, P1 p1, P2 p2, P3 p3) where TBuilder : ISequenceBuilder
	{
		return b;
	}

	[Obsolete]
	public static TBuilder Do<TBuilder, P1, P2, P3, P4>(this TBuilder b, Func<P1, P2, P3, P4, IEnumerable<ActivityProgress>> f, P1 p1, P2 p2, P3 p3, P4 p4) where TBuilder : ISequenceBuilder
	{
		return b;
	}

	[Obsolete]
	public static TBuilder Do<TBuilder, TReturn>(this TBuilder b, Func<IEnumerable<ComputationProgress<TReturn>>> f) where TBuilder : ISequenceBuilder
	{
		return b;
	}

	[Obsolete]
	public static TBuilder Do<TBuilder, P1, TReturn>(this TBuilder b, Func<P1, IEnumerable<ComputationProgress<TReturn>>> f, P1 p1) where TBuilder : ISequenceBuilder
	{
		return b;
	}

	[Obsolete]
	public static TBuilder Do<TBuilder, P1, P2, TReturn>(this TBuilder b, Func<P1, P2, IEnumerable<ComputationProgress<TReturn>>> f, P1 p1, P2 p2) where TBuilder : ISequenceBuilder
	{
		return b;
	}

	[Obsolete]
	public static TBuilder Do<TBuilder, P1, P2, P3, TReturn>(this TBuilder b, Func<P1, P2, P3, IEnumerable<ComputationProgress<TReturn>>> f, P1 p1, P2 p2, P3 p3) where TBuilder : ISequenceBuilder
	{
		return b;
	}

	[Obsolete]
	public static TBuilder Do<TBuilder, P1, P2, P3, P4, TReturn>(this TBuilder b, Func<P1, P2, P3, P4, IEnumerable<ComputationProgress<TReturn>>> f, P1 p1, P2 p2, P3 p3, P4 p4) where TBuilder : ISequenceBuilder
	{
		return b;
	}

	public static TBuilder Do<TBuilder>(this TBuilder b, Func<Sequencer.Builder> e) where TBuilder : ISequenceBuilder
	{
		return Resolver<TBuilder, Sequencer.Builder>._Do(b, e);
	}

	public static TBuilder Do<TBuilder>(this TBuilder b, Func<ParallelSequencer.Builder> e) where TBuilder : ISequenceBuilder
	{
		return Resolver<TBuilder, ParallelSequencer.Builder>._Do(b, e);
	}

	public static TBuilder Do<TBuilder>(this TBuilder b, Func<Brancher.Builder> e) where TBuilder : ISequenceBuilder
	{
		return Resolver<TBuilder, Brancher.Builder>._Do(b, e);
	}

	public static TBuilder Do<TBuilder>(this TBuilder b, Func<Trier.Builder> e) where TBuilder : ISequenceBuilder
	{
		return Resolver<TBuilder, Trier.Builder>._Do(b, e);
	}

	public static TBuilder Do<TBuilder>(this TBuilder b, Func<Catcher.Builder> e) where TBuilder : ISequenceBuilder
	{
		return Resolver<TBuilder, Catcher.Builder>._Do(b, e);
	}

	public static TBuilder Do<TBuilder>(this TBuilder b, Func<TimedSequencer.Builder> e) where TBuilder : ISequenceBuilder
	{
		return Resolver<TBuilder, TimedSequencer.Builder>._Do(b, e);
	}

	public static TBuilder Do<TBuilder, TItem>(this TBuilder b, Func<ForEacher.Builder<TItem>> e) where TBuilder : ISequenceBuilder
	{
		return Resolver<TBuilder, ForEacher.Builder<TItem>>._Do(b, e);
	}

	public static TBuilder Do<TBuilder, P1>(this TBuilder b, Func<P1, Sequencer.Builder> e, P1 p1) where TBuilder : ISequenceBuilder
	{
		return Resolver<TBuilder, Sequencer.Builder, P1>._Do(b, e, p1);
	}

	public static TBuilder Do<TBuilder, P1>(this TBuilder b, Func<P1, ParallelSequencer.Builder> e, P1 p1) where TBuilder : ISequenceBuilder
	{
		return Resolver<TBuilder, ParallelSequencer.Builder, P1>._Do(b, e, p1);
	}

	public static TBuilder Do<TBuilder, P1>(this TBuilder b, Func<P1, Brancher.Builder> e, P1 p1) where TBuilder : ISequenceBuilder
	{
		return Resolver<TBuilder, Brancher.Builder, P1>._Do(b, e, p1);
	}

	public static TBuilder Do<TBuilder, P1>(this TBuilder b, Func<P1, Trier.Builder> e, P1 p1) where TBuilder : ISequenceBuilder
	{
		return Resolver<TBuilder, Trier.Builder, P1>._Do(b, e, p1);
	}

	public static TBuilder Do<TBuilder, P1>(this TBuilder b, Func<P1, Catcher.Builder> e, P1 p1) where TBuilder : ISequenceBuilder
	{
		return Resolver<TBuilder, Catcher.Builder, P1>._Do(b, e, p1);
	}

	public static TBuilder Do<TBuilder, P1>(this TBuilder b, Func<P1, TimedSequencer.Builder> e, P1 p1) where TBuilder : ISequenceBuilder
	{
		return Resolver<TBuilder, TimedSequencer.Builder, P1>._Do(b, e, p1);
	}

	public static TBuilder Do<TBuilder, TItem, P1>(this TBuilder b, Func<P1, ForEacher.Builder<TItem>> e, P1 p1) where TBuilder : ISequenceBuilder
	{
		return Resolver<TBuilder, ForEacher.Builder<TItem>, P1>._Do(b, e, p1);
	}

	public static TBuilder Do<TBuilder, P1, P2>(this TBuilder b, Func<P1, P2, Sequencer.Builder> e, P1 p1, P2 p2) where TBuilder : ISequenceBuilder
	{
		return Resolver<TBuilder, Sequencer.Builder, P1, P2>._Do(b, e, p1, p2);
	}

	public static TBuilder Do<TBuilder, P1, P2>(this TBuilder b, Func<P1, P2, ParallelSequencer.Builder> e, P1 p1, P2 p2) where TBuilder : ISequenceBuilder
	{
		return Resolver<TBuilder, ParallelSequencer.Builder, P1, P2>._Do(b, e, p1, p2);
	}

	public static TBuilder Do<TBuilder, P1, P2>(this TBuilder b, Func<P1, P2, Brancher.Builder> e, P1 p1, P2 p2) where TBuilder : ISequenceBuilder
	{
		return Resolver<TBuilder, Brancher.Builder, P1, P2>._Do(b, e, p1, p2);
	}

	public static TBuilder Do<TBuilder, P1, P2>(this TBuilder b, Func<P1, P2, Trier.Builder> e, P1 p1, P2 p2) where TBuilder : ISequenceBuilder
	{
		return Resolver<TBuilder, Trier.Builder, P1, P2>._Do(b, e, p1, p2);
	}

	public static TBuilder Do<TBuilder, P1, P2>(this TBuilder b, Func<P1, P2, Catcher.Builder> e, P1 p1, P2 p2) where TBuilder : ISequenceBuilder
	{
		return Resolver<TBuilder, Catcher.Builder, P1, P2>._Do(b, e, p1, p2);
	}

	public static TBuilder Do<TBuilder, P1, P2>(this TBuilder b, Func<P1, P2, TimedSequencer.Builder> e, P1 p1, P2 p2) where TBuilder : ISequenceBuilder
	{
		return Resolver<TBuilder, TimedSequencer.Builder, P1, P2>._Do(b, e, p1, p2);
	}

	public static TBuilder Do<TBuilder, TItem, P1, P2>(this TBuilder b, Func<P1, P2, ForEacher.Builder<TItem>> e, P1 p1, P2 p2) where TBuilder : ISequenceBuilder
	{
		return Resolver<TBuilder, ForEacher.Builder<TItem>, P1, P2>._Do(b, e, p1, p2);
	}

	public static TBuilder Do<TBuilder, P1, P2, P3>(this TBuilder b, Func<P1, P2, P3, Sequencer.Builder> e, P1 p1, P2 p2, P3 p3) where TBuilder : ISequenceBuilder
	{
		return Resolver<TBuilder, Sequencer.Builder, P1, P2, P3>._Do(b, e, p1, p2, p3);
	}

	public static TBuilder Do<TBuilder, P1, P2, P3>(this TBuilder b, Func<P1, P2, P3, ParallelSequencer.Builder> e, P1 p1, P2 p2, P3 p3) where TBuilder : ISequenceBuilder
	{
		return Resolver<TBuilder, ParallelSequencer.Builder, P1, P2, P3>._Do(b, e, p1, p2, p3);
	}

	public static TBuilder Do<TBuilder, P1, P2, P3>(this TBuilder b, Func<P1, P2, P3, Brancher.Builder> e, P1 p1, P2 p2, P3 p3) where TBuilder : ISequenceBuilder
	{
		return Resolver<TBuilder, Brancher.Builder, P1, P2, P3>._Do(b, e, p1, p2, p3);
	}

	public static TBuilder Do<TBuilder, P1, P2, P3>(this TBuilder b, Func<P1, P2, P3, Trier.Builder> e, P1 p1, P2 p2, P3 p3) where TBuilder : ISequenceBuilder
	{
		return Resolver<TBuilder, Trier.Builder, P1, P2, P3>._Do(b, e, p1, p2, p3);
	}

	public static TBuilder Do<TBuilder, P1, P2, P3>(this TBuilder b, Func<P1, P2, P3, Catcher.Builder> e, P1 p1, P2 p2, P3 p3) where TBuilder : ISequenceBuilder
	{
		return Resolver<TBuilder, Catcher.Builder, P1, P2, P3>._Do(b, e, p1, p2, p3);
	}

	public static TBuilder Do<TBuilder, P1, P2, P3>(this TBuilder b, Func<P1, P2, P3, TimedSequencer.Builder> e, P1 p1, P2 p2, P3 p3) where TBuilder : ISequenceBuilder
	{
		return Resolver<TBuilder, TimedSequencer.Builder, P1, P2, P3>._Do(b, e, p1, p2, p3);
	}

	public static TBuilder Do<TBuilder, TItem, P1, P2, P3>(this TBuilder b, Func<P1, P2, P3, ForEacher.Builder<TItem>> e, P1 p1, P2 p2, P3 p3) where TBuilder : ISequenceBuilder
	{
		return Resolver<TBuilder, ForEacher.Builder<TItem>, P1, P2, P3>._Do(b, e, p1, p2, p3);
	}

	public static TBuilder Do<TBuilder, P1, P2, P3, P4>(this TBuilder b, Func<P1, P2, P3, P4, Sequencer.Builder> e, P1 p1, P2 p2, P3 p3, P4 p4) where TBuilder : ISequenceBuilder
	{
		return Resolver<TBuilder, Sequencer.Builder, P1, P2, P3, P4>._Do(b, e, p1, p2, p3, p4);
	}

	public static TBuilder Do<TBuilder, P1, P2, P3, P4>(this TBuilder b, Func<P1, P2, P3, P4, ParallelSequencer.Builder> e, P1 p1, P2 p2, P3 p3, P4 p4) where TBuilder : ISequenceBuilder
	{
		return Resolver<TBuilder, ParallelSequencer.Builder, P1, P2, P3, P4>._Do(b, e, p1, p2, p3, p4);
	}

	public static TBuilder Do<TBuilder, P1, P2, P3, P4>(this TBuilder b, Func<P1, P2, P3, P4, Brancher.Builder> e, P1 p1, P2 p2, P3 p3, P4 p4) where TBuilder : ISequenceBuilder
	{
		return Resolver<TBuilder, Brancher.Builder, P1, P2, P3, P4>._Do(b, e, p1, p2, p3, p4);
	}

	public static TBuilder Do<TBuilder, P1, P2, P3, P4>(this TBuilder b, Func<P1, P2, P3, P4, Trier.Builder> e, P1 p1, P2 p2, P3 p3, P4 p4) where TBuilder : ISequenceBuilder
	{
		return Resolver<TBuilder, Trier.Builder, P1, P2, P3, P4>._Do(b, e, p1, p2, p3, p4);
	}

	public static TBuilder Do<TBuilder, P1, P2, P3, P4>(this TBuilder b, Func<P1, P2, P3, P4, Catcher.Builder> e, P1 p1, P2 p2, P3 p3, P4 p4) where TBuilder : ISequenceBuilder
	{
		return Resolver<TBuilder, Catcher.Builder, P1, P2, P3, P4>._Do(b, e, p1, p2, p3, p4);
	}

	public static TBuilder Do<TBuilder, P1, P2, P3, P4>(this TBuilder b, Func<P1, P2, P3, P4, TimedSequencer.Builder> e, P1 p1, P2 p2, P3 p3, P4 p4) where TBuilder : ISequenceBuilder
	{
		return Resolver<TBuilder, TimedSequencer.Builder, P1, P2, P3, P4>._Do(b, e, p1, p2, p3, p4);
	}

	public static TBuilder Do<TBuilder, TItem, P1, P2, P3, P4>(this TBuilder b, Func<P1, P2, P3, P4, ForEacher.Builder<TItem>> e, P1 p1, P2 p2, P3 p3, P4 p4) where TBuilder : ISequenceBuilder
	{
		return Resolver<TBuilder, ForEacher.Builder<TItem>, P1, P2, P3, P4>._Do(b, e, p1, p2, p3, p4);
	}
}
