using System;
using Brix.Engine;
using Brix.Lua;
using Brix.Network.Util;
using MoonSharp.Interpreter;
using Newtonsoft.Json;
using UnityEngine;

namespace Brix.Game.Rules;

[Serializable]
[JsonObject(MemberSerialization.OptIn)]
[RequireComponent(typeof(RuleMaster))]
[SerializerTarget(new string[] { "save" })]
public class TaskRules : MonoBehaviour
{
	private CompCache<RuleMaster> _ruleMaster;

	public GameSignals.GameSignal<TaskType> OnAllowedTasksChanged = new GameSignals.GameSignal<TaskType>();

	private static TaskRules _main;

	[EnumFlag]
	[SerializeField]
	private TaskType _allowedTasks = TaskType.All;

	[JsonProperty]
	[SerializeField]
	private bool _canDeleteTasks = true;

	public static TaskRules Main => _main;

	[LuaDrivable("allowedTasks")]
	public TaskType AllowedTasks
	{
		get
		{
			return _allowedTasks;
		}
		set
		{
			_allowedTasks = value;
			GameSignals.Invoke(OnAllowedTasksChanged, _allowedTasks);
		}
	}

	[LuaDrivable("canDeleteTask")]
	public bool CanDeleteTasks
	{
		get
		{
			return _canDeleteTasks;
		}
		set
		{
			_canDeleteTasks = value;
		}
	}

	private void Awake()
	{
		_ruleMaster = new CompCache<RuleMaster>(base.gameObject);
		if (_ruleMaster.Get.Priority == -1)
		{
			_main = this;
		}
	}

	public void SetAllowedTasks(Table rules)
	{
		TaskType taskType = TaskType.None;
		foreach (DynValue value in rules.Values)
		{
			if (value.IsNotNil())
			{
				TaskType taskType2 = value.ToEnum<TaskType>();
				if (taskType2 != TaskType.None)
				{
					taskType |= taskType2;
				}
			}
		}
		AllowedTasks = taskType;
	}

	[LuaDrivable("SetAllowedTasks")]
	private static DynValue lua_SetAllowedTasks(ScriptExecutionContext context, CallbackArguments args)
	{
		TaskRules taskRules = args.AsComponent<TaskRules>(0, "SetAllowedTasks");
		Table allowedTasks = args.AsTable(1, "SetAllowedTasks");
		taskRules.SetAllowedTasks(allowedTasks);
		return DynValue.Void;
	}
}
