using Brix.Engine;
using Brix.External.Signals;
using Brix.Game.AI;
using Brix.Utils.Signals;
using UnityEngine;

public class TargetHighlightDriver : MonoBehaviour
{
	[SerializeField]
	private Sprite Sword;

	[SerializeField]
	private Sprite Bow;

	private Signal Destroyed = new Signal();

	private void ConnectToSignals()
	{
		UIGameObserver.bricktrons.ChangeTarget.Connect(OnTargetChanged).DisconnectWhen(Destroyed);
		UIGameObserver.bricktrons.EnterSelection.Connect(OnSelected).DisconnectWhen(Destroyed);
		UIGameObserver.bricktrons.ExitSelection.Connect(OnDeselected).DisconnectWhen(Destroyed);
		UIGameObserver.bricktrons.ExitCreation.Connect(OnRemove).DisconnectWhen(Destroyed);
	}

	private void OnRemove(Labor labor)
	{
		RemoveHighlight(labor);
	}

	private void OnDeselected(Labor labor)
	{
		RemoveHighlight(labor);
	}

	private void OnSelected(Labor labor)
	{
		AddHiglight(labor);
	}

	private void OnTargetChanged(Labor labor)
	{
		RemovePreviousHighlight(labor);
		if (UIGameObserver.bricktrons.GetSelectionFlag(labor))
		{
			AddHiglight(labor);
		}
	}

	private Sprite GetAttackIcon(Labor attacker)
	{
		if (attacker != null && attacker.Profession.InferedArchetype() == Occupation.Type.Archer)
		{
			return Bow;
		}
		return Sword;
	}

	private void RemovePreviousHighlight(Labor labor)
	{
		if (!labor.affiliation.IsOwnedByMe)
		{
			return;
		}
		GameObject gameObject = UIGameObserver.bricktrons.PreviousTargetOf(labor);
		if (!(gameObject == null))
		{
			OverheadDisplayDriver component = gameObject.GetComponent<OverheadDisplayDriver>();
			if (component != null)
			{
				component.HighlightTarget(GetAttackIcon(labor), value: false);
			}
		}
	}

	private void AddHiglight(Labor labor)
	{
		if (!labor.affiliation.IsOwnedByMe)
		{
			return;
		}
		GameObject gameObject = UIGameObserver.bricktrons.TargetOf(labor);
		if (!(gameObject == null))
		{
			OverheadDisplayDriver component = gameObject.GetComponent<OverheadDisplayDriver>();
			if (component != null)
			{
				component.HighlightTarget(GetAttackIcon(labor), value: true);
			}
		}
	}

	private void RemoveHighlight(Labor labor)
	{
		if (!labor.affiliation.IsOwnedByMe)
		{
			return;
		}
		GameObject gameObject = UIGameObserver.bricktrons.TargetOf(labor);
		if (!(gameObject == null))
		{
			OverheadDisplayDriver component = gameObject.GetComponent<OverheadDisplayDriver>();
			if (component != null)
			{
				component.HighlightTarget(GetAttackIcon(labor), value: false);
			}
		}
	}

	private void OnDestroy()
	{
		Destroyed.Invoke();
	}

	private void Start()
	{
		GameSignals.LateConnect(UIGameObserver.IsReady, ConnectToSignals);
	}
}
