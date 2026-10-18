using System;
using System.Collections.Generic;
using Brix.Lua;
using Brix.NewUI.UserSettings_;
using MoonSharp.Interpreter;
using MoonSharp.Interpreter.Interop;

public static class LuiGameObserver
{
	private static class Game
	{
		public static class Options
		{
			public static class Sound
			{
				public static class Mute
				{
					public static readonly Func<bool> Get = () => _userSettings.audio.volumeMute;

					public static readonly Action<bool> Set = delegate(bool v)
					{
						_userSettings.audio.volumeMute = v;
						LuaHookBase.InvokeFromCLR(OnSet);
					};

					public static readonly LuaHook OnSet = new LuaHook();
				}

				public static class VolumeMenu
				{
					public static readonly Func<float> Get = () => _userSettings.audio.volumeUI;

					public static readonly Action<float> Set = delegate(float v)
					{
						_userSettings.audio.volumeUI = v;
						LuaHookBase.InvokeFromCLR(OnSet);
					};

					public static readonly LuaHook OnSet = new LuaHook();
				}

				public static class VolumeSound
				{
					public static readonly Func<float> Get = () => _userSettings.audio.volumeSon;

					public static readonly Action<float> Set = delegate(float v)
					{
						_userSettings.audio.volumeSon = v;
						LuaHookBase.InvokeFromCLR(OnSet);
					};

					public static readonly LuaHook OnSet = new LuaHook();
				}

				public static class VolumeAmbiance
				{
					public static readonly Func<float> Get = () => _userSettings.audio.volumeAmbiance;

					public static readonly Action<float> Set = delegate(float v)
					{
						_userSettings.audio.volumeAmbiance = v;
						LuaHookBase.InvokeFromCLR(OnSet);
					};

					public static readonly LuaHook OnSet = new LuaHook();
				}
			}

			private static UserSettings _userSettings;

			public static void Load()
			{
				_userSettings = UserSettingsReader.Read();
			}

			public static void Save()
			{
				UserSettingsReader.Save(_userSettings);
			}

			public static void Apply()
			{
				UserSettingManager.SetSettingIntoGame(_userSettings);
			}
		}
	}

	private class DataBuilder
	{
		private Table _rootTable;

		private Table _parentTable;

		private readonly List<string> _tableNames = new List<string>();

		public DataBuilder this[string name]
		{
			get
			{
				AddTableName(name);
				return this;
			}
		}

		public DataBuilder(Script s)
		{
			InitRootTable(s.Globals);
			ResetParentTable();
		}

		private void InitRootTable(Table t)
		{
			_rootTable = t;
		}

		private void ResetParentTable()
		{
			_parentTable = _rootTable;
		}

		private void AddTable(string name)
		{
			if (_parentTable.Get(name).Equals(DynValue.Nil))
			{
				Table value = new Table(_rootTable.OwnerScript);
				_parentTable[name] = value;
			}
			_parentTable = _parentTable[name] as Table;
		}

		private void AddTables(bool skipLast = false)
		{
			for (int i = 0; i < _tableNames.Count; i++)
			{
				if (!skipLast || i != _tableNames.Count - 1)
				{
					string name = _tableNames[i];
					AddTable(name);
				}
			}
		}

		private void AddTableName(string name)
		{
			_tableNames.Add(name);
		}

		private void ClearTableNames()
		{
			_tableNames.Clear();
		}

		private void ResetTables()
		{
			ResetParentTable();
			ClearTableNames();
		}

		public void New()
		{
			AddTables();
			ResetTables();
		}

		public void New(Action v)
		{
			AddTables(skipLast: true);
			ResetTables();
		}

		public void New<T>(Action<T> v)
		{
			AddTables(skipLast: true);
			ResetTables();
		}

		public void New<T>(Func<T> v)
		{
			AddTables(skipLast: true);
			ResetTables();
		}

		public void New(LuaHookBase v)
		{
			AddTables(skipLast: true);
			ResetTables();
		}
	}

	private const string __LuiGame = "LuiGame";

	private const string __Options = "Options";

	private const string __Sound = "Sound";

	private const string __Mute = "Mute";

	private const string __VolumeMenu = "VolumeMenu";

	private const string __Load = "Load";

	private const string __Save = "Save";

	private const string __Apply = "Apply";

	private const string __Get = "Get";

	private const string __Set = "Set";

	private const string __OnSet = "hk_OnSet";

	[MoonSharpVisible(false)]
	public static void RegisterClass(Script s)
	{
		DataBuilder dataBuilder = new DataBuilder(s);
		dataBuilder["LuiGame"].New();
		dataBuilder["LuiGame"]["Options"].New();
		dataBuilder["LuiGame"]["Options"]["Load"].New(Game.Options.Load);
		dataBuilder["LuiGame"]["Options"]["Save"].New(Game.Options.Save);
		dataBuilder["LuiGame"]["Options"]["Apply"].New(Game.Options.Apply);
		dataBuilder["LuiGame"]["Options"]["Sound"].New();
		dataBuilder["LuiGame"]["Options"]["Sound"]["Mute"].New();
		dataBuilder["LuiGame"]["Options"]["Sound"]["Mute"]["Get"].New(Game.Options.Sound.Mute.Get);
		dataBuilder["LuiGame"]["Options"]["Sound"]["Mute"]["Set"].New(Game.Options.Sound.Mute.Set);
		dataBuilder["LuiGame"]["Options"]["Sound"]["Mute"]["hk_OnSet"].New(Game.Options.Sound.Mute.OnSet);
		dataBuilder["LuiGame"]["Options"]["Sound"]["VolumeMenu"].New();
		dataBuilder["LuiGame"]["Options"]["Sound"]["VolumeMenu"]["Get"].New(Game.Options.Sound.VolumeMenu.Get);
		dataBuilder["LuiGame"]["Options"]["Sound"]["VolumeMenu"]["Set"].New(Game.Options.Sound.VolumeMenu.Set);
		dataBuilder["LuiGame"]["Options"]["Sound"]["VolumeMenu"]["hk_OnSet"].New(Game.Options.Sound.VolumeMenu.OnSet);
	}
}
