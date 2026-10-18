using System;
using Brix.Utils.Closures;
using UnityEngine;

namespace Motus.Behavior;

public static class BuilderBaseExtender
{
	public static Looper.Builder Loop<TBuilder>(this TBuilder b) where TBuilder : IBuilderBase
	{
		return Node.Loop(b.ToNode);
	}

	public static Sharer.Builder AsSharedNode<TBuilder>(this TBuilder b) where TBuilder : IBuilderBase
	{
		return Node.Share(b.ToNode);
	}

	public static Shielder.Builder ShieldNode<TBuilder>(this TBuilder b) where TBuilder : IBuilderBase
	{
		return Node.Shield(b.ToNode);
	}

	public static Shielder.Builder ShieldNode(this StaticNode b)
	{
		return Node.Shield(b);
	}

	public static TBuilder Label<TBuilder>(this TBuilder b, string label) where TBuilder : IBuilderBase
	{
		b.Label(label, null);
		return b;
	}

	public static TBuilder Label<TBuilder>(this TBuilder b, string label, UnityEngine.Object context) where TBuilder : IBuilderBase
	{
		b.Label(label, context);
		return b;
	}

	public static TBuilder Finally<TBuilder>(this TBuilder b, Action a) where TBuilder : IBuilderBase
	{
		b.SetFinalize(a.Make().Pooled2());
		return b;
	}

	public static TBuilder Finally<TBuilder, P1>(this TBuilder b, Action<P1> a, P1 p1) where TBuilder : IBuilderBase
	{
		b.SetFinalize(a.Make().Close(p1).Pooled2());
		return b;
	}

	public static TBuilder Finally<TBuilder, P1, P2>(this TBuilder b, Action<P1, P2> a, P1 p1, P2 p2) where TBuilder : IBuilderBase
	{
		b.SetFinalize(a.Make().Close(p2).Close(p1)
			.Pooled2());
		return b;
	}

	public static TBuilder Finally<TBuilder, P1, P2, P3>(this TBuilder b, Action<P1, P2, P3> a, P1 p1, P2 p2, P3 p3) where TBuilder : IBuilderBase
	{
		b.SetFinalize(a.Make().Close(p3).Close(p2)
			.Close(p1)
			.Pooled2());
		return b;
	}

	public static TBuilder Finally<TBuilder, P1, P2, P3, P4>(this TBuilder b, Action<P1, P2, P3, P4> a, P1 p1, P2 p2, P3 p3, P4 p4) where TBuilder : IBuilderBase
	{
		b.SetFinalize(a.Make().Close(p4).Close(p3)
			.Close(p2)
			.Close(p1)
			.Pooled2());
		return b;
	}
}
