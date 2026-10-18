using System.Collections.Generic;
using System.Runtime.InteropServices;
using Brix.Engine;
using Brix.Game;
using Brix.Legacy;
using Brix.Lua;
using Brix.Network;
using Brix.Utils.Unity;
using MoonSharp.Interpreter;
using UnityEngine;
using UnityEngine.Networking;

namespace Brix.UI.GameSelector;

public class UISelectable : NetworkGameComponent, PieMenuable, ISelectable
{
	[SyncVar]
	[DisplayRules(DisplayFlags.HideInEditMode)]
	[SerializeField]
	public bool SelectOnSpawn;

	[SerializeField]
	public string PieMenuTag;

	[HideInInspector]
	public Component target;

	[SerializeField]
	private Vector3 CircleTarget;

	[SerializeField]
	private bool Selectable;

	[SerializeField]
	private bool Hoverable;

	[SerializeField]
	public bool CanOnlyBeOne;

	[SerializeField]
	private SelectionType selectionType;

	[LuaDrivable("hk_OnSelected")]
	private static LuaHook<LuaDriver> lua_OnSelected = new LuaHook<LuaDriver>();

	[LuaDrivable("hk_OnUnselected")]
	private static LuaHook<LuaDriver> lua_OnUnselected = new LuaHook<LuaDriver>();

	[LuaDrivable("hk_OnHovered")]
	private static LuaHook<LuaDriver> lua_OnHovered = new LuaHook<LuaDriver>();

	[LuaDrivable("hk_OnUnhovered")]
	private static LuaHook<LuaDriver> lua_OnUnhovered = new LuaHook<LuaDriver>();

	private LuaDriver _ld;

	[SerializeField]
	private SelectableBehaviourInjector injector;

	[SerializeField]
	private bool useInjector;

	private SelectableBehaviour selectableBehaviour;

	public SelectionType SelectionType => selectionType;

	public bool NetworkSelectOnSpawn
	{
		get
		{
			return SelectOnSpawn;
		}
		[param: In]
		set
		{
			SetSyncVar(value, ref SelectOnSpawn, 1u);
		}
	}

	public static void RegisterClass(Script script)
	{
		Table table = new Table(script);
		table["hk_OnSelected"] = lua_OnSelected;
		table["hk_OnUnselected"] = lua_OnUnselected;
		table["hk_OnHovered"] = lua_OnHovered;
		table["hk_OnUnhovered"] = lua_OnUnhovered;
		script.Globals["UISelectable"] = table;
	}

	private void InitLuaDriver()
	{
		_ld = base.gameObject.GetComponent<LuaDriver>();
	}

	public void SetSelectableParent(UISelectable s)
	{
		selectableBehaviour = s.selectableBehaviour;
	}

	public bool CirclePositionOverride(out Vector3 pos)
	{
		pos = CircleTarget;
		return CircleTarget != Vector3.zero;
	}

	public void ResetSelectableParent()
	{
		if (useInjector)
		{
			injector.Inject(ref selectableBehaviour, this, Selectable, Hoverable, SelectionType);
		}
		else
		{
			selectableBehaviour = new SelectableBaseBehaviour(this, Selectable, Hoverable, SelectionType);
		}
	}

	private void InitBehaviour()
	{
		ResetSelectableParent();
	}

	public void OnSelect()
	{
		selectableBehaviour.OnSelect();
		if (selectionType != SelectionType.None)
		{
			UIGameSelector.instance.OnSelectedDico[selectionType].Invoke(this);
		}
		GetComponent<ISelectObserver>()?.OnSelect();
		if (_ld != null)
		{
			LuaHookBase.InvokeFromCLR(lua_OnSelected, _ld);
		}
	}

	public void OnUnselect()
	{
		GetComponent<IUnselectObserver>()?.OnUnselect();
		if (selectionType != SelectionType.None)
		{
			UIGameSelector.instance.OnUnselectedDico[selectionType].Invoke(this);
		}
		selectableBehaviour.OnUnselect();
		if (_ld != null)
		{
			LuaHookBase.InvokeFromCLR(lua_OnUnselected, _ld);
		}
	}

	public void OnHover()
	{
		GetComponent<IHoverObserver>()?.OnHover();
		if (selectionType != SelectionType.None)
		{
			UIGameSelector.instance.OnHoveredDico[selectionType].Invoke(this);
		}
		selectableBehaviour.OnHover();
		if (_ld != null)
		{
			LuaHookBase.InvokeFromCLR(lua_OnHovered, _ld);
		}
	}

	public void OnUnhover()
	{
		GetComponent<IUnhoverObserver>()?.OnUnhover();
		if (selectionType != SelectionType.None)
		{
			UIGameSelector.instance.OnUnhoveredDico[selectionType].Invoke(this);
		}
		selectableBehaviour.OnUnhover();
		if (_ld != null)
		{
			LuaHookBase.InvokeFromCLR(lua_OnUnhovered, _ld);
		}
	}

	public bool IsSelectable()
	{
		return selectableBehaviour.IsSelectable();
	}

	public bool IsHoverable()
	{
		return selectableBehaviour.IsHoverable();
	}

	public void OnMultiSelect(List<UISelectable> selectables)
	{
		selectableBehaviour.OnMultiSelect(selectables);
	}

	public override bool CheckTag(string tag)
	{
		if (PieMenuTag.Contains(tag))
		{
			return true;
		}
		return target is PieMenuable pieMenuable && pieMenuable.CheckTag(tag);
	}

	public override void Initialize()
	{
		base.Initialize();
		InitBehaviour();
		InitLuaDriver();
	}

	public override void OnRequested()
	{
		base.OnRequested();
		UIGameSelector.Register(this);
	}

	public override void ResetComponent()
	{
		UIGameSelector.Unregister(this);
		base.ResetComponent();
	}

	public override void OnStartClient()
	{
		if (SelectOnSpawn && (bool)base.affiliation && base.affiliation.UserCreator == User.LocalUser.gameObject && IsSelectable())
		{
			Neo.Relais(delegate
			{
				UIGameSelector.AddToSelection(base.gameObject);
			}, "add to selection");
		}
	}

	public UISelectable CopyTo(GameObject go, SelectionType newType, string pieMenuTag)
	{
		return AddComponentTo(go, Selectable, Hoverable, newType, pieMenuTag);
	}

	private static UISelectable AddComponentTo(GameObject go, bool selectable, bool hoverable, SelectionType type, string pieMenuTag)
	{
		UISelectable uISelectable = go.AddComponent<UISelectable>();
		uISelectable.Selectable = selectable;
		uISelectable.Hoverable = hoverable;
		uISelectable.selectionType = type;
		uISelectable.PieMenuTag = pieMenuTag;
		return uISelectable;
	}

	private void UNetVersion()
	{
	}

	public override bool OnSerialize(NetworkWriter writer, bool forceAll)
	{
		bool flag = base.OnSerialize(writer, forceAll);
		if (forceAll)
		{
			writer.Write(SelectOnSpawn);
			return true;
		}
		bool flag2 = false;
		if ((base.syncVarDirtyBits & 1) != 0)
		{
			if (!flag2)
			{
				writer.WritePackedUInt32(base.syncVarDirtyBits);
				flag2 = true;
			}
			writer.Write(SelectOnSpawn);
		}
		if (!flag2)
		{
			writer.WritePackedUInt32(base.syncVarDirtyBits);
		}
		return flag2 || flag;
	}

	public override void OnDeserialize(NetworkReader reader, bool initialState)
	{
		base.OnDeserialize(reader, initialState);
		if (initialState)
		{
			SelectOnSpawn = reader.ReadBoolean();
			return;
		}
		int num = (int)reader.ReadPackedUInt32();
		if ((num & 1) != 0)
		{
			SelectOnSpawn = reader.ReadBoolean();
		}
	}
}
