using System;
using System.Collections.Generic;
using Brix.Engine;
using Brix.Game;
using Brix.Game.AI;
using Brix.Game.Network;
using Brix.Game.Rules;
using Brix.UI.GameSelector;
using Brix.Utils;
using Brix.Utils.Collections;
using Smooth.Slinq;
using UnityEngine;

namespace Brix.UI;

public static class UIGameActor
{
	public static GameSignals.GameSignal SelectIdleProjectSignal = new GameSignals.GameSignal();

	public static void AssignCharacterToProject(Project project, Labor labor)
	{
		if (!project.Crew().Contains(labor))
		{
			DirectOrderProject.ChangeProject(labor, project, withSound: true);
		}
	}

	public static void MakeSelectionFreeAgents()
	{
		List<Labor> list = CollectionPool.RequestList<Labor>();
		UIGameObserver.bricktrons.CheckSelection().Slinq().AddTo(list);
		foreach (Labor item in list)
		{
			item.Autonomy = AutonomyStatus.FreeAgent;
		}
		list.Dispose();
	}

	public static void AssignSelectionToProject(Project project)
	{
		List<Labor> list = CollectionPool.RequestList<Labor>();
		UIGameObserver.bricktrons.CheckSelection().Slinq().AddTo(list);
		foreach (Labor item in list)
		{
			AssignCharacterToProject(project, item);
		}
		list.Dispose();
	}

	public static void SelectIdleProject()
	{
		GameSignals.Invoke(SelectIdleProjectSignal);
	}

	public static void SelectProjectMember(Project project, bool includeFixedAgents = true, bool includeFreeAgents = true, bool includeSoldiers = false)
	{
		List<GameObject> list = CollectionPool.RequestList<GameObject>();
		(from c in project.Crew().Slinq()
			where c.faction == User.LocalUser.faction
			select c).ForEach(delegate(Labor u, List<GameObject> list3)
		{
			list3.Add(u.gameObject);
		}, list);
		if (!includeFixedAgents || !includeFreeAgents || !includeSoldiers)
		{
			List<GameObject> list2 = list.Slinq().Where(delegate(GameObject i)
			{
				bool flag = i.GetComponent<Labor>().Autonomy == AutonomyStatus.FreeAgent;
				bool flag2 = i.GetComponent<Labor>().Occupation.CurrentOccupation == Occupation.Type.Builder;
				return ((flag && includeFreeAgents) || (!flag && includeFixedAgents)) && (flag2 || includeSoldiers);
			}).ToPooledList();
			list.Dispose();
			list = list2;
		}
		UIGameSelector.Select(list);
		list.Dispose();
	}

	public static void SelectByOccupation(Occupation.Type occupation, Faction faction, bool visibleOnly = false)
	{
		Project matchingProject = IdleProject.GetMatchingProject(faction);
		SelectProjectMembers(matchingProject, (Labor l) => l.Profession.InferedArchetype() == occupation, visibleOnly);
	}

	public static void SelectProjectMembers(Project project, Func<Labor, bool> test, bool visibleOnly = false)
	{
		List<Labor> list = project.Crew().Slinq().Where((Labor l, Func<Labor, bool> t) => t(l), test)
			.ToPooledList();
		if (visibleOnly)
		{
			list.RemoveAll((Labor l) => !UIGameObserver.bricktrons.GetScreenFlag(l));
		}
		List<GameObject> list2 = (from l in list.Slinq()
			select l.gameObject).ToPooledList();
		UIGameSelector.Select(list2);
		list.Dispose();
		list2.Dispose();
	}

	public static void SelectCoworkersOf(Labor labor, bool visibleOnly = false)
	{
		Occupation.Job job = labor.Occupation.GetJobFromToolbag();
		SelectProjectMembers(labor.Project, (Labor l) => l.Occupation.GetJobFromToolbag() == job, visibleOnly);
	}

	public static void DeleteCurrentProject()
	{
		if (!(UIGameObserver.projects.CurrentSelected == null) && TaskRules.Main.CanDeleteTasks && UIGameObserver.projects.CurrentSelected.CanBeDeleted)
		{
			UIGameObserver.projects.CurrentSelected.RequestDelete();
		}
	}

	public static void DeleteSelectedBlueprints()
	{
		CollectionPool.Request<GameObject>(out List<GameObject> list);
		UIGameSelector.GetSelectedOfType(ref list, SelectionType.Blueprint);
		UNetBlueprint component = User.LocalUser.GetComponent<UNetBlueprint>();
		list.Slinq().ForEach(delegate(GameObject b, UNetBlueprint p)
		{
			p.CallCmdDestroyBluePrint(b);
		}, component);
		list.Dispose();
	}

	public static void SplitSelectedBlueprints()
	{
		CollectionPool.Request<GameObject>(out List<GameObject> list);
		UIGameSelector.GetSelectedOfType(ref list, SelectionType.Blueprint);
		GameObject[] blueprints = list.ToArray();
		list.Dispose();
		User.LocalUser.GetComponent<UNetBlueprint>().CallCmdSendBlueprintsToNewProject(blueprints);
	}
}
