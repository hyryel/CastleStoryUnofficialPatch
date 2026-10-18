using Brix.Engine;
using Brix.Game;
using Brix.Game.AI;
using Brix.Game.AI.Damage;
using Brix.Game.Components;
using Brix.UI.GameSelector;
using Brix.Utils.Factories;
using Brix.Utils.Signals;
using UnityEngine;

public abstract class CharacterHighlightDriver : GameComponent
{
	protected enum ShowStatus
	{
		show,
		remove,
		linger
	}

	protected enum Change
	{
		damage,
		none,
		heal
	}

	protected abstract UIGameObserver.Bricktron observer { get; }

	protected abstract bool testFog { get; }

	protected abstract SelectionCircle.MainColors mainColor { get; }

	protected override void Start()
	{
		base.Start();
		GameSignals.LateConnect(UIGameObserver.IsReady, ConnectToSignals);
	}

	private void ConnectToSignals()
	{
		observer.EnterCreation.Connect(ConnectBricktronSignals).DisconnectWhen(base.OnReleaseOrDestroySignal);
		observer.EnterHover.Connect(RefreshCircle).DisconnectWhen(base.OnReleaseOrDestroySignal);
		observer.ExitHover.Connect(RefreshCircle).DisconnectWhen(base.OnReleaseOrDestroySignal);
		observer.EnterSelection.Connect(RefreshCircle).DisconnectWhen(base.OnReleaseOrDestroySignal);
		observer.ExitSelection.Connect(RefreshCircle).DisconnectWhen(base.OnReleaseOrDestroySignal);
	}

	private void ConnectBricktronSignals(Labor l)
	{
		BricktronDamageReceiver bricktronDamageReceiver = l.damageReceiver as BricktronDamageReceiver;
		bricktronDamageReceiver.HPChangedSignal.Connect(delegate(float previous, float next)
		{
			OnHpChanged(l, ComputeChange(previous, next));
		}).DisconnectWhen(l.DestroyedSignal);
		bricktronDamageReceiver.StaminaChangedSignal.Connect(delegate(float previous, float next)
		{
			OnStaminaChanged(l, ComputeChange(previous, next));
		}).DisconnectWhen(l.DestroyedSignal);
		l.StateUpdated.Connect(delegate(Labor _l)
		{
			RefreshCircle(_l);
		}, l).DisconnectWhen(l.DestroyedSignal);
		l.OnReleaseOrDestroySignal.ConnectOnce(delegate(GameObject go)
		{
			SelectionCircleDispenser.Instance.Remove(go);
		});
	}

	private Change ComputeChange(float previous, float next)
	{
		return (!(previous > next)) ? ((previous == next) ? Change.none : Change.heal) : Change.damage;
	}

	private void OnHpChanged(Labor labor, Change hp)
	{
		RefreshCircle(labor, hp, Change.none);
	}

	private void OnStaminaChanged(Labor labor, Change stamina)
	{
		RefreshCircle(labor, Change.none, stamina);
	}

	protected virtual ShowStatus ShouldShow(Labor labor, Change hp, Change stamina)
	{
		if (testFog && !UIGameObserver.fogObjects.IsVisible(labor.vision))
		{
			return ShowStatus.remove;
		}
		if (!LocalFactionVision.ObjectIsVisibleForFaction(labor.gameObject, User.LocalUser.faction))
		{
			return ShowStatus.remove;
		}
		if (UIGameSelector.IsFocused(labor.gameObject))
		{
			return ShowStatus.show;
		}
		if (labor.WithComponent((BricktronDamageReceiver d) => d.IsDamaged || !d.FullStamina, _default: false))
		{
			return ShowStatus.show;
		}
		if (hp == Change.heal || stamina == Change.heal)
		{
			return ShowStatus.linger;
		}
		return ShowStatus.remove;
	}

	private void RefreshCircle(Labor labor)
	{
		RefreshCircle(labor, Change.none, Change.none);
	}

	private void RefreshCircle(Labor labor, Change hp, Change stamina)
	{
		switch (ShouldShow(labor, hp, stamina))
		{
		case ShowStatus.show:
			GetCircle(labor, hp, stamina);
			break;
		case ShowStatus.remove:
			SelectionCircleDispenser.Instance.Remove(labor.gameObject);
			break;
		case ShowStatus.linger:
			SelectionCircleDispenser.Instance.Remove(labor.gameObject, 1f);
			break;
		}
	}

	private void GetCircle(Labor labor, Change hp, Change stamina)
	{
		SelectionCircle selectionCircle = SelectionCircleDispenser.Instance.AddOrGet(labor.gameObject);
		selectionCircle.AdaptToBricktron(labor);
		selectionCircle.MainColor(mainColor);
		selectionCircle.FlashHp(hp == Change.damage);
		selectionCircle.FlashStamina(stamina == Change.damage);
	}
}
