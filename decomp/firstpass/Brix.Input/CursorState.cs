using System.Collections.Generic;
using Brix.External.Signals;
using Brix.Game.AI.Nodes;
using Brix.Utils.Signals;
using Smooth.Slinq;

namespace Brix.Input;

public class CursorState
{
	public CursorIcon icon;

	private CursorController controller;

	private List<UtilityNodes.BoolFunc> conditions = new List<UtilityNodes.BoolFunc>();

	private List<UtilityNodes.BoolFunc> filterConditions = new List<UtilityNodes.BoolFunc>();

	public bool Dirty { get; set; }

	public CursorState(CursorController controller)
	{
		this.controller = controller;
	}

	public bool Check()
	{
		if (filterConditions.Slinq().All((UtilityNodes.BoolFunc c) => !c()) && conditions.Slinq().Any((UtilityNodes.BoolFunc c) => c()))
		{
			Apply();
			return true;
		}
		if (controller.currentState == this)
		{
			controller.CheckAllExcept(this);
		}
		return false;
	}

	public void Apply()
	{
		controller.currentState = this;
		SetIcon();
	}

	protected virtual void SetIcon()
	{
		Curseur.SetCurseur(icon);
	}

	public CursorState UseWhen(UtilityNodes.BoolFunc condition)
	{
		conditions.Add(condition);
		return this;
	}

	public CursorState DontUseWhen(UtilityNodes.BoolFunc condition)
	{
		filterConditions.Add(condition);
		return this;
	}

	public void RunUpdate()
	{
		Check();
		Dirty = false;
	}

	public CursorState UpdateOn<T1, T2>(Signal<T1, T2> signal)
	{
		signal.Connect(delegate
		{
			_makeThingsDirty();
		}).DisconnectWhen(controller.Cleared);
		return this;
	}

	public CursorState UpdateOn<T>(Signal<T> signal)
	{
		signal.Connect(delegate
		{
			_makeThingsDirty();
		}).DisconnectWhen(controller.Cleared);
		return this;
	}

	public CursorState UpdateOn(Signal signal)
	{
		signal.Connect(delegate
		{
			_makeThingsDirty();
		}).DisconnectWhen(controller.Cleared);
		return this;
	}

	private void _makeThingsDirty()
	{
		Dirty = true;
		controller.UpdateOnNextFrame = true;
	}
}
