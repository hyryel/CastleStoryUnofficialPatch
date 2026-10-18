using Brix.Utils.Logging;
using UnityEngine;

namespace Brix.Game.Network;

public static class IObjectTrackerExtender
{
	private static BrixLogger logger = new BrixLogger(typeof(IObjectTrackerExtender).FullName);

	public static void AddObject(this IObjectTracker tracker, GameObject item)
	{
		Transform transform = tracker.ContainerFor(item);
		if (transform == null)
		{
			logger.ErrorFormat("Cannot find container for {0} in {1} object tracker", item, tracker.gameObject);
		}
		else
		{
			item.transform.SetParent(transform);
		}
	}

	public static void ObjectTracked(this IObjectTracker tracker, GameObject item)
	{
		if (item.transform.parent != tracker.ContainerFor(item))
		{
			tracker.AddObject(item);
		}
		tracker.OnObjectTracked(item);
	}
}
