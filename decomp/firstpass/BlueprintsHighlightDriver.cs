using System.Collections.Generic;
using Brix.Engine;
using Brix.Game.Components;
using Brix.Input;
using Brix.NewUI.Tooltips;
using Brix.UI.GameSelector;
using Brix.Utils.Signals;
using UnityEngine;

public class BlueprintsHighlightDriver : GameComponent
{
	[SerializeField]
	public Color OnHoverColor;

	[SerializeField]
	public Color OnSelectedColor;

	private Dictionary<Blueprint, int> highlightRequests = new Dictionary<Blueprint, int>();

	protected override void Start()
	{
		base.Start();
		GameSignals.LateConnect(UIGameObserver.IsReady, ConnectToSignals);
	}

	private void ConnectToSignals()
	{
		UIGameObserver.blueprints.Selected.Connect(OnObjectSelected).DisconnectWhen(base.OnReleaseOrDestroySignal);
		UIGameObserver.blueprints.Unselected.Connect(OnObjectUnselected).DisconnectWhen(base.OnReleaseOrDestroySignal);
		UIGameObserver.blueprints.Removed.Connect(OnObjectUnselected).DisconnectWhen(base.OnReleaseOrDestroySignal);
		UIGameObserver.blueprints.Hovered.Connect(OnObjectHovered).DisconnectWhen(base.OnReleaseOrDestroySignal);
		UIGameObserver.blueprints.EndHovered.Connect(OnObjectUnhovered).DisconnectWhen(base.OnReleaseOrDestroySignal);
		base.OnReleaseOrDestroySignal.ConnectOnce(delegate
		{
			RemoveAllHighlights();
		});
	}

	private void Highlight(Blueprint b)
	{
		RemoveHighlight(b);
		int value = Highlighter.highlight.At(b.volume.blueprint.Slinq().FirstOrDefault(), OnSelectedColor);
		highlightRequests.Add(b, value);
	}

	private void RemoveHighlight(Blueprint b)
	{
		highlightRequests.TryGetValue(b, out var value);
		Highlighter.remove.Request(value);
		highlightRequests.Remove(b);
	}

	private void RemoveAllHighlights()
	{
		foreach (KeyValuePair<Blueprint, int> highlightRequest in highlightRequests)
		{
			Highlighter.remove.Request(highlightRequest.Value);
		}
		highlightRequests.Clear();
	}

	private void OnObjectSelected(Blueprint blueprint)
	{
		Highlight(blueprint);
	}

	private void OnObjectUnselected(Blueprint blueprint)
	{
		RemoveHighlight(blueprint);
	}

	private void OnObjectUnhovered(Blueprint blueprint)
	{
		CursorTooltip.instance.Hide();
		if (!UIGameSelector.IsSelected(blueprint.gameObject))
		{
			RemoveHighlight(blueprint);
		}
	}

	private void OnObjectHovered(Blueprint blueprint)
	{
		if (!UIGameSelector.IsSelected(blueprint.gameObject))
		{
			if (!SelectionBox.active && blueprint.recepteur.HasComplexRecipe() && !blueprint.IsAnti)
			{
				CursorTooltip.instance.ShowBlueprintRequirement(blueprint);
			}
			Highlight(blueprint);
		}
	}
}
