using System.Collections.Generic;
using Brix.External.Signals;
using Brix.Game.Components;
using Brix.Utils.Logging;
using UnityEngine;

namespace Brix.Game.AI.Behaviors;

public class AnimationEventBoard : UpdatableGameComponent<AnimationEventBoard>
{
	private class ExhaustedEvent
	{
		public AnimationEvent Event;

		public float ReviveTime;
	}

	private static BrixLogger logger = new BrixLogger(typeof(AnimationEventBoard).FullName);

	public Signal<AnimationEvent> OnEvent = new Signal<AnimationEvent>();

	private List<ExhaustedEvent> ExhaustionList;

	protected override void Start()
	{
		base.Start();
		ExhaustionList = new List<ExhaustedEvent>();
	}

	protected override void BrixUpdate()
	{
		ExhaustionList.RemoveAll((ExhaustedEvent evt) => evt.ReviveTime <= Time.timeSinceLevelLoad);
	}

	private void NewEvent(Object o)
	{
		AnimationEvent evt = o as AnimationEvent;
		if (evt == null)
		{
			logger.Warn("an animation event was sent without data, probably a data error in a clip");
		}
		else if (ExhaustionList.Find((ExhaustedEvent e) => e.Event == evt) == null)
		{
			OnEvent.Invoke(evt);
			if (evt.SleepDelay > 0f)
			{
				ExhaustionList.Add(new ExhaustedEvent
				{
					Event = evt,
					ReviveTime = Time.timeSinceLevelLoad + evt.SleepDelay
				});
			}
		}
	}
}
