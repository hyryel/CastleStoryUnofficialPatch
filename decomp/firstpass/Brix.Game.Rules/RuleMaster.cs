using System;
using System.Collections.Generic;
using Brix.Lua;
using Brix.Network.Util;
using MoonSharp.Interpreter;
using Smooth.Slinq;
using UnityEngine;

namespace Brix.Game.Rules;

[Serializable]
[SerializerTarget(new string[] { "save" })]
public class RuleMaster : MonoBehaviour
{
	private static List<RuleMaster> _masters = new List<RuleMaster>();

	[SerializeField]
	private int _priority;

	[SerializeField]
	public bool _drawGizmo;

	[LuaDrivable("priority")]
	public int Priority
	{
		get
		{
			return _priority;
		}
		set
		{
			if (value < 0)
			{
				Debug.LogError("ERROR: Priority of rule zone cannot be negative.");
			}
			else
			{
				_priority = value;
			}
		}
	}

	private void OnDrawGizmos()
	{
	}

	public bool ContainsVector(Vector3 v3)
	{
		if (_priority == -1)
		{
			return true;
		}
		return new Bounds(base.gameObject.transform.position, base.gameObject.transform.localScale).Contains(v3);
	}

	public T GetHighestPriorityRulesAt<T>(Vector3 v3) where T : class
	{
		if (_masters.Count == 0)
		{
			return (T)null;
		}
		foreach (RuleMaster master in _masters)
		{
			if (master.ContainsVector(v3))
			{
				T val = (T)(object)master.GetComponent(typeof(T));
				if (val != null)
				{
					return val;
				}
			}
		}
		return (T)null;
	}

	private void Awake()
	{
		if (_priority != -1)
		{
			_masters.Add(this);
			_ = from x in _masters.Slinq()
				orderby x._priority descending
				select x;
		}
	}

	private void OnDestroy()
	{
		if (_priority != -1)
		{
			_masters.Remove(this);
		}
	}

	[LuaDrivable("AddTerrainRules")]
	private static DynValue lua_AddTerrainRules(ScriptExecutionContext context, CallbackArguments args)
	{
		GameObject gameObject = args.AsGameObject(0, "AddTerrainRules");
		return DynValue.FromObject(context.OwnerScript, gameObject.AddComponent<TerrainRules>());
	}

	[LuaDrivable("AddCameraRules")]
	private static DynValue lua_AddCameraRules(ScriptExecutionContext context, CallbackArguments args)
	{
		GameObject gameObject = args.AsGameObject(0, "AddCameraRules");
		return DynValue.FromObject(context.OwnerScript, gameObject.AddComponent<CameraRules>());
	}
}
