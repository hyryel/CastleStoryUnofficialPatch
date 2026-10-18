using System;
using System.Collections.Generic;
using Brix.Utils.Logging;
using MoonSharp.Interpreter;

namespace Brix.UI.Builder.Menu;

public static class LuaIdUtility
{
	public abstract class Entry
	{
		public abstract string Name { get; }
	}

	public sealed class MenuEntry : Entry
	{
		public IMenu Menu { get; private set; }

		public override string Name => Menu.FullName;

		public MenuEntry(IMenu menu)
		{
			Menu = menu;
		}
	}

	public sealed class PanelEntry : Entry
	{
		public IPanel Panel { get; private set; }

		public override string Name => Panel.FullName;

		public PanelEntry(IPanel panel)
		{
			Panel = panel;
		}
	}

	public sealed class WidgetEntry : Entry
	{
		public IWidget Widget;

		public IPanel Panel;

		private readonly string _widgetName;

		public override string Name => $"{Panel.FullName}.{Widget.HierarchyName}[\"{_widgetName}\"]";

		public WidgetEntry(IWidget widget, IPanel panel, string widgetName)
		{
			Widget = widget;
			Panel = panel;
			_widgetName = widgetName;
		}
	}

	public static class EnumStringMapper<T> where T : struct
	{
		public static T[] Values;

		public static string[] Names;

		public static Dictionary<string, T> NameValueMap;

		static EnumStringMapper()
		{
			Values = Enum.GetValues(typeof(T)) as T[];
			Names = Enum.GetNames(typeof(T));
			NameValueMap = new Dictionary<string, T>();
			for (int i = 0; i < Values.Length; i++)
			{
				NameValueMap.Add(Names[i], Values[i]);
			}
		}
	}

	private static BrixLogger logger = new BrixLogger(typeof(LuaIdUtility).FullName);

	private static long _id;

	private static readonly Dictionary<long, Entry> EntryMap = new Dictionary<long, Entry>();

	private static readonly List<DynValue> TmpPanelIds = new List<DynValue>();

	public static bool TryGetEntryFromEntryId<TEntry>(long entryId, out TEntry typedEntry) where TEntry : Entry
	{
		Entry entry = GetEntry(entryId);
		typedEntry = entry as TEntry;
		return typedEntry != null;
	}

	public static Entry GetEntry(long id)
	{
		return EntryMap[id];
	}

	public static void Reset()
	{
		_id = 0L;
		EntryMap.Clear();
	}

	public static DynValue RegisterPanel(IPanel panel, params string[] widgetNames)
	{
		if (panel.LuaId != 0)
		{
			throw new Exception("Non-zero lua id on added panel.");
		}
		_id++;
		panel.LuaId = _id;
		PanelEntry value = new PanelEntry(panel);
		EntryMap.Add(_id, value);
		TmpPanelIds.Clear();
		TmpPanelIds.Add(DynValue.NewNumber(_id));
		return DynValue.NewTuple(TmpPanelIds.ToArray());
	}

	public static long RegisterMenu(IMenu menu)
	{
		if (menu.LuaId != 0)
		{
			return menu.LuaId;
		}
		_id++;
		menu.LuaId = _id;
		MenuEntry value = new MenuEntry(menu);
		EntryMap.Add(_id, value);
		return _id;
	}

	public static void UnregisterMenu(IMenu menu)
	{
		if (menu.LuaId != 0)
		{
			menu.LuaId = 0L;
			EntryMap.Remove(_id);
		}
	}

	public static void OutputLog()
	{
		string text = "Lua IDs:\n";
		foreach (long key in EntryMap.Keys)
		{
			text = $"{text}[{key.ToString()}] {EntryMap[key].Name}\n";
		}
		logger.Info(text);
	}
}
