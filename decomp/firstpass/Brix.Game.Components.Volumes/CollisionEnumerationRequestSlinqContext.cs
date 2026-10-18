using System;
using Brix.Game.Components.Tracking;
using Smooth.Algebraics;
using Smooth.Slinq;
using Smooth.Slinq.Context;

namespace Brix.Game.Components.Volumes;

public struct CollisionEnumerationRequestSlinqContext
{
	private TrackingRegistry.TrackingRequest.Enumerator enumerator;

	private static readonly Mutator<TrackingRegistry.TrackingEntry, CollisionEnumerationRequestSlinqContext> skip = Skip;

	private static readonly Mutator<TrackingRegistry.TrackingEntry, CollisionEnumerationRequestSlinqContext> remove = Remove;

	private static readonly Mutator<TrackingRegistry.TrackingEntry, CollisionEnumerationRequestSlinqContext> dispose = Dispose;

	private CollisionEnumerationRequestSlinqContext(TrackingRegistry.TrackingRequest enumerable)
	{
		enumerator = enumerable.GetEnumerator();
	}

	public static Slinq<TrackingRegistry.TrackingEntry, CollisionEnumerationRequestSlinqContext> Slinq(TrackingRegistry.TrackingRequest request)
	{
		return new Slinq<TrackingRegistry.TrackingEntry, CollisionEnumerationRequestSlinqContext>(skip, remove, dispose, new CollisionEnumerationRequestSlinqContext(request));
	}

	private static void Skip(ref CollisionEnumerationRequestSlinqContext context, out Option<TrackingRegistry.TrackingEntry> next)
	{
		if (context.enumerator.MoveNext())
		{
			next = new Option<TrackingRegistry.TrackingEntry>(context.enumerator.Current);
			return;
		}
		next = default(Option<TrackingRegistry.TrackingEntry>);
		context.enumerator.Dispose();
	}

	private static void Remove(ref CollisionEnumerationRequestSlinqContext context, out Option<TrackingRegistry.TrackingEntry> next)
	{
		throw new NotSupportedException();
	}

	private static void Dispose(ref CollisionEnumerationRequestSlinqContext context, out Option<TrackingRegistry.TrackingEntry> next)
	{
		next = default(Option<TrackingRegistry.TrackingEntry>);
		context.enumerator.Dispose();
	}
}
