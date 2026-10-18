using System;
using System.Collections.Generic;
using Brix.Utils;
using Smooth.Slinq;
using Smooth.Slinq.Context;
using UnityEngine;

namespace Brix.Game.AI;

public class ProjectDatabase
{
	public static Dictionary<Faction, ProjectDatabase> All = new Dictionary<Faction, ProjectDatabase>();

	private Dictionary<Type, HashSet<IOperable<Labor>>> _projectDatabase = new Dictionary<Type, HashSet<IOperable<Labor>>>();

	private Dictionary<Type, int> _workDatabase = new Dictionary<Type, int>();

	public static void ClearAll()
	{
		foreach (KeyValuePair<Faction, ProjectDatabase> item in All)
		{
			item.Value._projectDatabase.Clear();
			item.Value._workDatabase.Clear();
		}
	}

	public static ProjectDatabase For(Faction o)
	{
		return All.GetValueOrCreate(o, () => new ProjectDatabase());
	}

	public ProjectDatabase Register(IOperable<Labor> project)
	{
		HashSet<IOperable<Labor>> valueOrCreate = _projectDatabase.GetValueOrCreate(project.GetType(), () => new HashSet<IOperable<Labor>>());
		project.OnReleaseOrDestroySignal.ConnectOnce(delegate(GameObject g, HashSet<IOperable<Labor>> s, IOperable<Labor> p)
		{
			s.Remove(p);
		}, valueOrCreate, project);
		valueOrCreate.Add(project);
		return this;
	}

	public void AddWorker(IOperable<Labor> project)
	{
		if (!_workDatabase.ContainsKey(project.GetType()))
		{
			_workDatabase.Add(project.GetType(), 0);
		}
		_workDatabase[project.GetType()]++;
	}

	public void RemoveWorker(IOperable<Labor> project, int qte = 1)
	{
		if (_workDatabase.ContainsKey(project.GetType()))
		{
			_workDatabase[project.GetType()] -= qte;
		}
	}

	public Slinq<IOperable<Labor>, SelectSlinqContext<IOperable<Labor>, HashSetSlinqContext<IOperable<Labor>>, KeyValuePair<Type, HashSet<IOperable<Labor>>>, DictionaryContext<Type, HashSet<IOperable<Labor>>>>> Projects()
	{
		return _projectDatabase.Slinq().SelectMany((KeyValuePair<Type, HashSet<IOperable<Labor>>> kv) => kv.Value.Slinq());
	}

	public void AvailableProjects(List<IOperable<Labor>> projects, Labor labor)
	{
		Projects().Where(Available).AddTo(projects);
	}

	private static bool Available(IOperable<Labor> p)
	{
		return p.IsStillAlive && p.PriorityBias != ProjectPriority.Bias.Locked && !(p is IdleProject);
	}

	public int WorkerInTaskSimilarTasksTo(IOperable<Labor> project)
	{
		_workDatabase.TryGetValue(project.GetType(), out var value);
		return value;
	}
}
