using System;
using Brix.Game;
using Brix.Game.AI;
using Brix.Game.Network;
using Brix.Utils.Factories;
using UnityEngine;
using UnityEngine.Networking;

namespace Brix.UI.GameSelector;

[Serializable]
public class SelectableProjectBehaviour : SelectableBaseBehaviour
{
	[SerializeField]
	private Project project;

	public SelectableProjectBehaviour(UISelectable selectableComponent, bool selectable, bool hoverable, SelectionType type)
		: base(selectableComponent, selectable, hoverable, type)
	{
	}

	public override void OnSelect()
	{
		UIGameSelector.UnselectAll(SelectionType.Project, project.gameObject);
		UIGameSelector.UnselectAll(SelectionType.Crafting);
		if (project is IdleProject)
		{
			UIGameSelector.UnselectEverything(project.gameObject);
		}
		project.WithComponent(delegate(BuildGoalProvider p)
		{
			p.ToggleSharedMaterialFocus(value: true);
		});
		project.OnSelect();
	}

	public override void OnUnselect()
	{
		base.OnUnselect();
		if (NetworkClient.active)
		{
			User.LocalUser.GetComponent<UNetGroupe>().CallCmdCommit(project.gameObject);
		}
		if (project is BuildProject)
		{
			project.GetComponent<BuildGoalProvider>().ToggleSharedMaterialFocus(value: false);
		}
		UIGameSelector.UnselectAll(SelectionType.Storage);
		project.OnUnselect();
	}
}
