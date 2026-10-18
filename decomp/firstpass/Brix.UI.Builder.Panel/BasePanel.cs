using System;
using System.Collections.Generic;
using System.Linq;
using Brix.External.Factories;
using Brix.UI.Builder.Menu;
using Brix.Utils.Logging;
using MoonSharp.Interpreter;
using UnityEngine;

namespace Brix.UI.Builder.Panel;

public abstract class BasePanel : IPanel, IRefresh, IInstance, ILuaId, IHierarchyName
{
	private RectTransform _trs;

	private string _suffixName;

	protected static BrixLogger logger = new BrixLogger(typeof(BasePanel).FullName);

	private bool _ignoresWidgets;

	protected BrixLogger Logger = new BrixLogger(typeof(BasePanel).FullName);

	public long LuaId { get; set; }

	public bool HasInstance => BaseInstance != null;

	private string MenuTypeName
	{
		get
		{
			if (ParentMenu == null)
			{
				return "{?}";
			}
			string text = ParentMenu.GetType().ToString();
			text = (text.Contains(".") ? text.Split('.').Last() : text);
			return "{" + text + "}";
		}
	}

	private string EditorName
	{
		get
		{
			string text = ((SuffixName != null) ? (" " + SuffixName) : string.Empty);
			return "(" + AssetKey.Factory + "." + AssetKey.Name + ") " + MenuTypeName + text;
		}
	}

	public string Name
	{
		get
		{
			string text = GetType().ToString().Split('.').Last();
			string text2 = text;
			return text2 + "(" + AssetKey.Factory + "." + AssetKey.Name + ")";
		}
	}

	public string HierarchyName
	{
		get
		{
			string text = ((ParentPanel != null) ? (ParentPanel.HierarchyName + " > ") : string.Empty);
			return text + Name;
		}
	}

	public string FullName => MenuTypeName + " " + HierarchyName;

	public string SuffixName
	{
		get
		{
			return _suffixName ?? string.Empty;
		}
		set
		{
			_suffixName = value;
		}
	}

	public IMenu ParentMenu { get; set; }

	public ILayoutPanel ParentPanel { get; set; }

	public virtual IWidget DefaultWidget { get; set; }

	public virtual bool LuaDriven { get; private set; }

	public Dictionary<string, IWidget> LuaWidget { get; set; }

	public bool IgnoresWidgets
	{
		get
		{
			return _ignoresWidgets;
		}
		private set
		{
			_ignoresWidgets = value;
		}
	}

	public virtual List<BasePanel> Builders_2 => new List<BasePanel>();

	public BasePanelComponent BaseInstance { get; private set; }

	protected virtual Factory.AssetKey AssetKey => null;

	public string ProfilingName => AssetKey.Name;

	protected BasePanel(bool luaDriven = false)
	{
		SetLuaDriven(luaDriven);
		LuaWidget = new Dictionary<string, IWidget>();
	}

	public RectTransform GetTrs()
	{
		return _trs;
	}

	public RectTransform GetParentLayoutTrs()
	{
		return _trs.parent.transform as RectTransform;
	}

	public abstract void Reset();

	public abstract bool Refresh();

	public void Request(bool skipIfInstanced)
	{
		if (skipIfInstanced && BaseInstance != null)
		{
			return;
		}
		Release();
		if (!(AssetKey == null))
		{
			GameObject gameObject = Factory.Instantiate(AssetKey);
			if (!(gameObject == null))
			{
				gameObject.name = EditorName;
				BaseInstance = gameObject.GetComponent<BasePanelComponent>();
				_trs = gameObject.transform as RectTransform;
			}
		}
	}

	public void ParentToTrs(RectTransform bin)
	{
		BaseInstance.transform.SetParent(bin, worldPositionStays: false);
	}

	public void Release()
	{
		if (!(BaseInstance == null))
		{
			UnityEngine.Object.Destroy(BaseInstance.gameObject);
			BaseInstance = null;
		}
	}

	private void SetLuaDriven(bool luaDriven)
	{
		LuaDriven = luaDriven;
	}

	public virtual void Lua_SetDefaultHandle(string handleKey, Closure closure)
	{
	}

	public virtual void Lua_ClearDefaultHandle(string handleKey)
	{
	}

	public virtual void Lua_ClearDefaultHandles(string[] handleKeys)
	{
	}

	public virtual void Lua_ClearAllDefaultHandles()
	{
	}

	public virtual void TrySetLuaWidgets()
	{
		logger.Debug("BasePanel.TrySetLuaWidgets");
	}

	public IPanel IgnoreWidgets()
	{
		IgnoresWidgets = true;
		return this;
	}
}
public abstract class BasePanel<TPanelComponent> : BasePanel where TPanelComponent : BasePanelComponent
{
	private readonly Action<TPanelComponent> _onReset;

	private readonly Action<TPanelComponent> _onRefresh;

	public TPanelComponent Instance => base.BaseInstance as TPanelComponent;

	protected BasePanel()
	{
	}

	protected BasePanel(Action<TPanelComponent> onReset, Action<TPanelComponent> onRefresh, bool luaDriven = false)
		: base(luaDriven)
	{
		_onReset = onReset;
		_onRefresh = onRefresh;
	}

	public override void Reset()
	{
		if (_onReset != null)
		{
			_onReset(Instance);
		}
	}

	public override bool Refresh()
	{
		if (_onRefresh != null)
		{
			_onRefresh(Instance);
		}
		return true;
	}
}
