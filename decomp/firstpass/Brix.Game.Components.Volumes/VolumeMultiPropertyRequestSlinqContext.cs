using System;
using Brix.Engine;
using Smooth.Algebraics;
using Smooth.Slinq;
using Smooth.Slinq.Context;

namespace Brix.Game.Components.Volumes;

public struct VolumeMultiPropertyRequestSlinqContext
{
	private VolumeMultiPropertyRequest.Enumerator enumerator;

	private static readonly Mutator<XYZ, VolumeMultiPropertyRequestSlinqContext> skip = Skip;

	private static readonly Mutator<XYZ, VolumeMultiPropertyRequestSlinqContext> remove = Remove;

	private static readonly Mutator<XYZ, VolumeMultiPropertyRequestSlinqContext> dispose = Dispose;

	private VolumeMultiPropertyRequestSlinqContext(VolumeMultiPropertyRequest enumerable)
	{
		enumerator = enumerable.GetEnumerator();
	}

	public static Slinq<XYZ, VolumeMultiPropertyRequestSlinqContext> Slinq(VolumeMultiPropertyRequest request)
	{
		return new Slinq<XYZ, VolumeMultiPropertyRequestSlinqContext>(skip, remove, dispose, new VolumeMultiPropertyRequestSlinqContext(request));
	}

	private static void Skip(ref VolumeMultiPropertyRequestSlinqContext context, out Option<XYZ> next)
	{
		if (context.enumerator.MoveNext())
		{
			next = new Option<XYZ>(context.enumerator.Current);
			return;
		}
		next = default(Option<XYZ>);
		context.enumerator.Dispose();
	}

	private static void Remove(ref VolumeMultiPropertyRequestSlinqContext context, out Option<XYZ> next)
	{
		throw new NotSupportedException();
	}

	private static void Dispose(ref VolumeMultiPropertyRequestSlinqContext context, out Option<XYZ> next)
	{
		next = default(Option<XYZ>);
		context.enumerator.Dispose();
	}
}
