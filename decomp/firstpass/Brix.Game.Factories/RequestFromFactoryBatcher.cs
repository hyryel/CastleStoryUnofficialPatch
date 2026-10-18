using System.Collections.Generic;
using Brix.Game.Components;
using Smooth.Slinq;
using UnityEngine;
using UnityEngine.Events;

namespace Brix.Game.Factories;

internal class RequestFromFactoryBatcher : GameComponent
{
	public UnityEvent RequestComplete;

	[SerializeField]
	private List<RequestOnInitialize> children;

	[ContextMenu("FindChildren")]
	private void FindChildren()
	{
		children = new List<RequestOnInitialize>();
		GetComponentsInChildren<RequestOnInitialize>(includeInactive: true).Slinq().AddTo(children);
	}

	public override void OnRequested()
	{
		base.OnRequested();
		foreach (RequestOnInitialize child in children)
		{
			child.Request();
		}
		RequestComplete.Invoke();
	}
}
