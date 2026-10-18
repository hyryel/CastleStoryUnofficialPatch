using System;
using System.Collections.Generic;
using UnityEngine.Events;

namespace Brix.Game.Components.Tracking;

[Serializable]
public class TrackingEvent : UnityEvent<HashSet<Trackable>>
{
}
