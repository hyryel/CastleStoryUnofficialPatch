using System;
using Brix.Core;
using Brix.External.Signals;
using Brix.Game;
using Brix.Game.Components;
using Brix.Input.WorldEditor;
using Brix.NewUI.BreadBar;
using Brix.UI.Builder.Menu;
using Brix.Utils.Logging;
using Smooth.Algebraics;
using UnityEngine;

namespace Brix.Engine;

public class GameSignals : MonoBehaviour
{
	public abstract class GameSignalBase<TAction>
	{
		public abstract int _Count { get; }
	}

	public abstract class GameSignalBase<TSignal, TAction> : GameSignalBase<TAction>, GameSignalInterface where TSignal : IConnectableSignalBase<TAction>, IConnectableOnceSignalBase<TAction>, IClearableSignal, new()
	{
		public readonly TSignal _signal = new TSignal
		{
			MaximumConnections = 10000
		};

		private static readonly Action<GameSignalBase<TSignal, TAction>> _Clear = delegate(GameSignalBase<TSignal, TAction> self)
		{
			self._signal.Clear();
		};

		public override int _Count => _signal.InvokedCount;

		protected GameSignalBase()
		{
			SceneAwake.Connect(_Clear, this);
			SceneDestroyed.Connect(_Clear, this);
		}
	}

	public enum loadingContext
	{
		menu,
		ingame
	}

	public class GameSignal : GameSignalBase<Signal, Action>
	{
		private static class Utility
		{
			public static readonly Action<Action, Action<Exception>> _SafeInvoke = delegate(Action a, Action<Exception> r)
			{
				SafeInvoke(a, r);
			};
		}

		private static class Utility<T1>
		{
			public static readonly Action<Action<T1>, Action<Exception>, T1> _SafeInvoke = delegate(Action<T1> a, Action<Exception> r, T1 t1)
			{
				SafeInvoke(t1, a, r);
			};
		}

		private static class Utility<T1, T2>
		{
			public static readonly Action<Action<T1, T2>, Action<Exception>, T1, T2> _SafeInvoke = delegate(Action<T1, T2> a, Action<Exception> r, T1 t1, T2 t2)
			{
				SafeInvoke(t1, t2, a, r);
			};
		}

		private static class Utility<T1, T2, T3>
		{
			public static readonly Action<Action<T1, T2, T3>, Action<Exception>, T1, T2, T3> _SafeInvoke = delegate(Action<T1, T2, T3> a, Action<Exception> r, T1 t1, T2 t2, T3 t3)
			{
				SafeInvoke(t1, t2, t3, a, r);
			};
		}

		public SignalConnection _Connect(Action action, Action<Exception> redirectException = null)
		{
			return _signal.Connect(Utility._SafeInvoke, action, redirectException);
		}

		public static SignalConnection _Connect<T1>(GameSignal gs, Action<T1> action, T1 t1, Action<Exception> redirectException = null)
		{
			return gs._signal.Connect(Utility<T1>._SafeInvoke, action, redirectException, t1);
		}

		public static SignalConnection _Connect<T1, T2>(GameSignal gs, Action<T1, T2> action, T1 t1, T2 t2, Action<Exception> redirectException = null)
		{
			return gs._signal.Connect(Utility<T1, T2>._SafeInvoke, action, redirectException, t1, t2);
		}

		public static SignalConnection _Connect<T1, T2, T3>(GameSignal gs, Action<T1, T2, T3> action, T1 t1, T2 t2, T3 t3, Action<Exception> redirectException = null)
		{
			return gs._signal.Connect(Utility<T1, T2, T3>._SafeInvoke, action, redirectException, t1, t2, t3);
		}
	}

	public class GameSignal<T> : GameSignalBase<Signal<T>, Action<T>>
	{
	}

	public class GameSignal<T1, T2> : GameSignalBase<Signal<T1, T2>, Action<T1, T2>>
	{
	}

	public class GameSignal<T1, T2, T3> : GameSignalBase<Signal<T1, T2, T3>, Action<T1, T2, T3>>
	{
	}

	public class GameSignal<T1, T2, T3, T4> : GameSignalBase<Signal<T1, T2, T3, T4>, Action<T1, T2, T3, T4>>
	{
	}

	private static class Connector
	{
		public static SignalConnection Connect(GameSignal gs, Action action, Action<Exception> redirectException = null)
		{
			return gs._signal.Connect(delegate(Action a, Action<Exception> r)
			{
				SafeInvoke(a, r);
			}, action, redirectException);
		}

		public static SignalConnection Once(GameSignal gs, Action action, Action<Exception> redirectException = null)
		{
			return gs._signal.ConnectOnce(delegate(Action a, Action<Exception> r)
			{
				SafeInvoke(a, r);
			}, action, redirectException);
		}
	}

	private static class Connector<T1>
	{
		public static SignalConnection Connect(GameSignal gs, Action<T1> action, T1 t1, Action<Exception> redirectException = null)
		{
			return gs._signal.Connect(delegate(T1 _t1, Action<T1> a, Action<Exception> r)
			{
				SafeInvoke(_t1, a, r);
			}, t1, action, redirectException);
		}

		public static SignalConnection Connect(GameSignal<T1> gs, Action<T1> action, Action<Exception> redirectException = null)
		{
			return gs._signal.Connect(delegate(T1 t1, Action<T1> a, Action<Exception> r)
			{
				SafeInvoke(t1, a, r);
			}, action, redirectException);
		}

		public static SignalConnection Once(GameSignal gs, Action<T1> action, T1 t1, Action<Exception> redirectException = null)
		{
			return gs._signal.ConnectOnce(delegate(T1 _t1, Action<T1> a, Action<Exception> r)
			{
				SafeInvoke(_t1, a, r);
			}, t1, action, redirectException);
		}

		public static SignalConnection Once(GameSignal<T1> gs, Action<T1> action, Action<Exception> redirectException = null)
		{
			return gs._signal.ConnectOnce(delegate(T1 t1, Action<T1> a, Action<Exception> r)
			{
				SafeInvoke(t1, a, r);
			}, action, redirectException);
		}
	}

	private static class Connector<T1, T2>
	{
		public static SignalConnection Connect(GameSignal gs, Action<T1, T2> action, T1 t1, T2 t2, Action<Exception> redirectException = null)
		{
			return gs._signal.Connect(delegate(T1 _t1, T2 _t2, Action<T1, T2> a, Action<Exception> r)
			{
				SafeInvoke(_t1, _t2, a, r);
			}, t1, t2, action, redirectException);
		}

		public static SignalConnection Connect(GameSignal<T1> gs, Action<T1, T2> action, T2 t2, Action<Exception> redirectException = null)
		{
			return gs._signal.Connect(delegate(T1 _t1, T2 _t2, Action<T1, T2> a, Action<Exception> r)
			{
				SafeInvoke(_t1, _t2, a, r);
			}, t2, action, redirectException);
		}

		public static SignalConnection Connect(GameSignal<T1, T2> gs, Action<T1, T2> action, Action<Exception> redirectException = null)
		{
			return gs._signal.Connect(delegate(T1 t1, T2 t2, Action<T1, T2> a, Action<Exception> r)
			{
				SafeInvoke(t1, t2, a, r);
			}, action, redirectException);
		}

		public static SignalConnection Once(GameSignal gs, Action<T1, T2> action, T1 t1, T2 t2, Action<Exception> redirectException = null)
		{
			return gs._signal.ConnectOnce(delegate(T1 _t1, T2 _t2, Action<T1, T2> a, Action<Exception> r)
			{
				SafeInvoke(_t1, _t2, a, r);
			}, t1, t2, action, redirectException);
		}

		public static SignalConnection Once(GameSignal<T1> gs, Action<T1, T2> action, T2 t2, Action<Exception> redirectException = null)
		{
			return gs._signal.ConnectOnce(delegate(T1 _t1, T2 _t2, Action<T1, T2> a, Action<Exception> r)
			{
				SafeInvoke(_t1, _t2, a, r);
			}, t2, action, redirectException);
		}

		public static SignalConnection Once(GameSignal<T1, T2> gs, Action<T1, T2> action, Action<Exception> redirectException = null)
		{
			return gs._signal.ConnectOnce(delegate(T1 t1, T2 t2, Action<T1, T2> a, Action<Exception> r)
			{
				SafeInvoke(t1, t2, a, r);
			}, action, redirectException);
		}
	}

	private static class Connector<T1, T2, T3>
	{
		public static SignalConnection Connect(GameSignal gs, Action<T1, T2, T3> action, T1 t1, T2 t2, T3 t3, Action<Exception> redirectException = null)
		{
			return gs._signal.Connect(delegate(T1 _t1, T2 _t2, T3 _t3, Action<T1, T2, T3> a, Action<Exception> r)
			{
				SafeInvoke(_t1, _t2, _t3, a, r);
			}, t1, t2, t3, action, redirectException);
		}

		public static SignalConnection Connect(GameSignal<T1> gs, Action<T1, T2, T3> action, T2 t2, T3 t3, Action<Exception> redirectException = null)
		{
			return gs._signal.Connect(delegate(T1 _t1, T2 _t2, T3 _t3, Action<T1, T2, T3> a, Action<Exception> r)
			{
				SafeInvoke(_t1, _t2, _t3, a, r);
			}, t2, t3, action, redirectException);
		}

		public static SignalConnection Connect(GameSignal<T1, T2> gs, Action<T1, T2, T3> action, T3 t3, Action<Exception> redirectException = null)
		{
			return gs._signal.Connect(delegate(T1 _t1, T2 _t2, T3 _t3, Action<T1, T2, T3> a, Action<Exception> r)
			{
				SafeInvoke(_t1, _t2, _t3, a, r);
			}, t3, action, redirectException);
		}

		public static SignalConnection Connect(GameSignal<T1, T2, T3> gs, Action<T1, T2, T3> action, Action<Exception> redirectException = null)
		{
			return gs._signal.Connect(delegate(T1 t1, T2 t2, T3 t3, Action<T1, T2, T3> a, Action<Exception> r)
			{
				SafeInvoke(t1, t2, t3, a, r);
			}, action, redirectException);
		}

		public static SignalConnection Once(GameSignal gs, Action<T1, T2, T3> action, T1 t1, T2 t2, T3 t3, Action<Exception> redirectException = null)
		{
			return gs._signal.ConnectOnce(delegate(T1 _t1, T2 _t2, T3 _t3, Action<T1, T2, T3> a, Action<Exception> r)
			{
				SafeInvoke(_t1, _t2, _t3, a, r);
			}, t1, t2, t3, action, redirectException);
		}

		public static SignalConnection Once(GameSignal<T1> gs, Action<T1, T2, T3> action, T2 t2, T3 t3, Action<Exception> redirectException = null)
		{
			return gs._signal.ConnectOnce(delegate(T1 _t1, T2 _t2, T3 _t3, Action<T1, T2, T3> a, Action<Exception> r)
			{
				SafeInvoke(_t1, _t2, _t3, a, r);
			}, t2, t3, action, redirectException);
		}

		public static SignalConnection Once(GameSignal<T1, T2> gs, Action<T1, T2, T3> action, T3 t3, Action<Exception> redirectException = null)
		{
			return gs._signal.ConnectOnce(delegate(T1 _t1, T2 _t2, T3 _t3, Action<T1, T2, T3> a, Action<Exception> r)
			{
				SafeInvoke(_t1, _t2, _t3, a, r);
			}, t3, action, redirectException);
		}

		public static SignalConnection Once(GameSignal<T1, T2, T3> gs, Action<T1, T2, T3> action, Action<Exception> redirectException = null)
		{
			return gs._signal.ConnectOnce(delegate(T1 t1, T2 t2, T3 t3, Action<T1, T2, T3> a, Action<Exception> r)
			{
				SafeInvoke(t1, t2, t3, a, r);
			}, action, redirectException);
		}
	}

	private static class Connector<T1, T2, T3, T4>
	{
		public static SignalConnection Connect(GameSignal gs, Action<T1, T2, T3, T4> action, T1 t1, T2 t2, T3 t3, T4 t4, Action<Exception> redirectException = null)
		{
			return gs._signal.Connect(delegate(T1 _t1, T2 _t2, T3 _t3, T4 _t4, Tuple<Action<T1, T2, T3, T4>, Action<Exception>> tuple)
			{
				SafeInvoke(_t1, _t2, _t3, _t4, tuple._1, tuple._2);
			}, t1, t2, t3, t4, Tuple.Create(action, redirectException));
		}

		public static SignalConnection Connect(GameSignal<T1> gs, Action<T1, T2, T3, T4> action, T2 t2, T3 t3, T4 t4, Action<Exception> redirectException = null)
		{
			return gs._signal.Connect(delegate(T1 _t1, T2 _t2, T3 _t3, T4 _t4, Tuple<Action<T1, T2, T3, T4>, Action<Exception>> tuple)
			{
				SafeInvoke(_t1, _t2, _t3, _t4, tuple._1, tuple._2);
			}, t2, t3, t4, Tuple.Create(action, redirectException));
		}

		public static SignalConnection Connect(GameSignal<T1, T2> gs, Action<T1, T2, T3, T4> action, T3 t3, T4 t4, Action<Exception> redirectException = null)
		{
			return gs._signal.Connect(delegate(T1 _t1, T2 _t2, T3 _t3, T4 _t4, Tuple<Action<T1, T2, T3, T4>, Action<Exception>> tuple)
			{
				SafeInvoke(_t1, _t2, _t3, _t4, tuple._1, tuple._2);
			}, t3, t4, Tuple.Create(action, redirectException));
		}

		public static SignalConnection Connect(GameSignal<T1, T2, T3> gs, Action<T1, T2, T3, T4> action, T4 t4, Action<Exception> redirectException = null)
		{
			return gs._signal.Connect(delegate(T1 _t1, T2 _t2, T3 _t3, T4 _t4, Tuple<Action<T1, T2, T3, T4>, Action<Exception>> tuple)
			{
				SafeInvoke(_t1, _t2, _t3, _t4, tuple._1, tuple._2);
			}, t4, Tuple.Create(action, redirectException));
		}

		public static SignalConnection Connect(GameSignal<T1, T2, T3, T4> gs, Action<T1, T2, T3, T4> action, Action<Exception> redirectException = null)
		{
			return gs._signal.Connect(delegate(T1 t1, T2 t2, T3 t3, T4 t4, Tuple<Action<T1, T2, T3, T4>, Action<Exception>> t)
			{
				SafeInvoke(t1, t2, t3, t4, t._1, t._2);
			}, Tuple.Create(action, redirectException));
		}

		public static SignalConnection Once(GameSignal gs, Action<T1, T2, T3, T4> action, T1 t1, T2 t2, T3 t3, T4 t4, Action<Exception> redirectException = null)
		{
			return gs._signal.ConnectOnce(delegate(T1 _t1, T2 _t2, T3 _t3, T4 _t4, Tuple<Action<T1, T2, T3, T4>, Action<Exception>> tuple)
			{
				SafeInvoke(_t1, _t2, _t3, _t4, tuple._1, tuple._2);
			}, t1, t2, t3, t4, Tuple.Create(action, redirectException));
		}

		public static SignalConnection Once(GameSignal<T1> gs, Action<T1, T2, T3, T4> action, T2 t2, T3 t3, T4 t4, Action<Exception> redirectException = null)
		{
			return gs._signal.ConnectOnce(delegate(T1 _t1, T2 _t2, T3 _t3, T4 _t4, Tuple<Action<T1, T2, T3, T4>, Action<Exception>> tuple)
			{
				SafeInvoke(_t1, _t2, _t3, _t4, tuple._1, tuple._2);
			}, t2, t3, t4, Tuple.Create(action, redirectException));
		}

		public static SignalConnection Once(GameSignal<T1, T2> gs, Action<T1, T2, T3, T4> action, T3 t3, T4 t4, Action<Exception> redirectException = null)
		{
			return gs._signal.ConnectOnce(delegate(T1 _t1, T2 _t2, T3 _t3, T4 _t4, Tuple<Action<T1, T2, T3, T4>, Action<Exception>> tuple)
			{
				SafeInvoke(_t1, _t2, _t3, _t4, tuple._1, tuple._2);
			}, t3, t4, Tuple.Create(action, redirectException));
		}

		public static SignalConnection Once(GameSignal<T1, T2, T3> gs, Action<T1, T2, T3, T4> action, T4 t4, Action<Exception> redirectException = null)
		{
			return gs._signal.ConnectOnce(delegate(T1 _t1, T2 _t2, T3 _t3, T4 _t4, Tuple<Action<T1, T2, T3, T4>, Action<Exception>> tuple)
			{
				SafeInvoke(_t1, _t2, _t3, _t4, tuple._1, tuple._2);
			}, t4, Tuple.Create(action, redirectException));
		}

		public static SignalConnection Once(GameSignal<T1, T2, T3, T4> gs, Action<T1, T2, T3, T4> action, Action<Exception> redirectException = null)
		{
			return gs._signal.ConnectOnce(delegate(T1 t1, T2 t2, T3 t3, T4 t4, Tuple<Action<T1, T2, T3, T4>, Action<Exception>> t)
			{
				SafeInvoke(t1, t2, t3, t4, t._1, t._2);
			}, Tuple.Create(action, redirectException));
		}
	}

	private static BrixLogger logger = new BrixLogger(typeof(GameSignals).FullName);

	public static readonly Signal SceneAwake = new Signal
	{
		MaximumConnections = 20000
	};

	private static readonly Signal SceneDestroyed = new Signal
	{
		MaximumConnections = 20000
	};

	public static GameSignal<loadingContext> LoadingGame = new GameSignal<loadingContext>();

	public static GameSignal LoadingScreenAnimStart = new GameSignal();

	public static GameSignal LoadingScreenAnimEnd = new GameSignal();

	public static GameSignal<string> StartError = new GameSignal<string>();

	public static GameSignal SaveUpdateNeeded = new GameSignal();

	public static GameSignal<int> SaveUpdateSuccess = new GameSignal<int>();

	public static GameSignal<int> SaveUpdateFailed = new GameSignal<int>();

	public static GameSignal<int> UserAcceptedUpdate = new GameSignal<int>();

	public static GameSignal NetworkWelcomed = new GameSignal();

	public static GameSignal NetworkStarted = new GameSignal();

	public static GameSignal ServerProcessStarted = new GameSignal();

	public static GameSignal ServerProcessEnded = new GameSignal();

	public static GameSignal<string> ServerProcessFailed = new GameSignal<string>();

	public static GameSignal<string> ServerConnectionFailed = new GameSignal<string>();

	public static GameSignal ServerConnectionSuccess = new GameSignal();

	public static GameSignal<string> LobbyPostJoinError = new GameSignal<string>();

	public static GameSignal UPnPSuceeded = new GameSignal();

	public static GameSignal UPnPFailed = new GameSignal();

	public static GameSignal LoadingStarted = new GameSignal();

	public static GameSignal GamemodeLoaded = new GameSignal();

	public static GameSignal GameObjectsLoaded = new GameSignal();

	public static GameSignal BeforeTerrainLoaded = new GameSignal();

	public static GameSignal TerrainLoaded = new GameSignal();

	public static GameSignal LevelLoaded = new GameSignal();

	public static GameSignal LevelFirstLoaded = new GameSignal();

	public static GameSignal LoadGamemode = new GameSignal();

	public static GameSignal LevelSaveLoaded = new GameSignal();

	public static GameSignal TerrainReady = new GameSignal();

	public static GameSignal SoundEngineReady = new GameSignal();

	public static GameSignal LevelReady = new GameSignal();

	public static GameSignal ReLoadGameCommand = new GameSignal();

	public static GameSignal LevelFirstReady = new GameSignal();

	public static GameSignal LevelSaveReady = new GameSignal();

	public static GameSignal DisclaimerAcknowledged = new GameSignal();

	public static GameSignal PhysicsReady = new GameSignal();

	public static GameSignal WaitingForPlayers = new GameSignal();

	public static GameSignal LeaveGame = new GameSignal();

	public static GameSignal GameOver = new GameSignal();

	public static GameSignal QuitThroughMenu = new GameSignal();

	public static GameSignal LoadError = new GameSignal();

	public static GameSignal AdditiveMapLoaded = new GameSignal();

	public static GameSignal LevelUnloaded = new GameSignal();

	public static GameSignal GameModeTimersStarted = new GameSignal();

	public static GameSignal LoadUI = new GameSignal();

	public static GameSignal GameSaved = new GameSignal();

	public static GameSignal<IJoueur> PlayerJoined = new GameSignal<IJoueur>();

	public static GameSignal<User> PlayerLost = new GameSignal<User>();

	public static GameSignal<GameObject, GameObject> UnitDied = new GameSignal<GameObject, GameObject>();

	public static GameSignal<IJoueur> PlayerWin = new GameSignal<IJoueur>();

	public static GameSignal<IJoueur> PlayerLeaveGame = new GameSignal<IJoueur>();

	public static GameSignal<int> BlockPlaced = new GameSignal<int>();

	public static GameSignal<XYZ> TreeLoaded = new GameSignal<XYZ>();

	public static GameSignal<StartingPoint> StartingPointLoaded = new GameSignal<StartingPoint>();

	public static GameSignal<GameObject> GameObjectLoaded = new GameSignal<GameObject>();

	public static GameSignal<XYZ> TerrainChanged = new GameSignal<XYZ>();

	public static GameSignal<XYZ> BlockTypeChanged = new GameSignal<XYZ>();

	public static GameSignal<XYZ> VoxelDamageChanged = new GameSignal<XYZ>();

	public static GameSignal<XYZ> TreeChanged = new GameSignal<XYZ>();

	public static GameSignal<XYZ> PlantChanged = new GameSignal<XYZ>();

	public static GameSignal<XYZ> BouldersChanged = new GameSignal<XYZ>();

	public static GameSignal<XYZ> MineralsChanged = new GameSignal<XYZ>();

	public static GameSignal BricktronSpawned = new GameSignal();

	public static GameSignal PathfindingDirectRequest = new GameSignal();

	public static GameSignal PathfindingSolvedRequest = new GameSignal();

	public static GameSignal PathfindingUnsolvedRequest = new GameSignal();

	public static GameSignal PathfindingInterruptedRequest = new GameSignal();

	public static GameSignal PathfindingRequest = new GameSignal();

	public static GameSignal DeselectAll = new GameSignal();

	public static GameSignal<ListeSelection> GetSelected = new GameSignal<ListeSelection>();

	public static GameSignal<ListeSelection> GetHovered = new GameSignal<ListeSelection>();

	public static GameSignal LoadingProgress = new GameSignal();

	public static GameSignal<int> LoadingProgressLeap = new GameSignal<int>();

	public static GameSignal<int> MaxLoadingProgress = new GameSignal<int>();

	public static GameSignal<IVoxel, bool, bool> VoxelDestroyed = new GameSignal<IVoxel, bool, bool>();

	public static GameSignal<XYZ> VoxelChanged = new GameSignal<XYZ>();

	public static GameSignal<BlockData> BeforeBlockRemoved = new GameSignal<BlockData>();

	public static GameSignal<XYZ, Quaternion, BlockInfo> AfterBlockRemoved = new GameSignal<XYZ, Quaternion, BlockInfo>();

	public static GameSignal<XYZ> TerrainRemoved = new GameSignal<XYZ>();

	public static GameSignal<Vector3> TerrainRemovedLua = new GameSignal<Vector3>();

	public static GameSignal<bool> OnEditorModeChanged = new GameSignal<bool>();

	public static GameSignal<bool> SetAudioDebug = new GameSignal<bool>();

	public static GameSignal<string, string, string, Action> OnDisplayOkDialog = new GameSignal<string, string, string, Action>();

	public static GameSignal<OkDialogContainerPopulator.GenericDialog> OnDisplayGenericDialog = new GameSignal<OkDialogContainerPopulator.GenericDialog>();

	public static GameSignal<int> OnDisplayDialogRadioSelect = new GameSignal<int>();

	public static GameSignal<bool> OnDisplayDialogFlag = new GameSignal<bool>();

	public static GameSignal<string> OnDisplayDialogUserName = new GameSignal<string>();

	public static GameSignal<string> OnDisplayDialogPassName = new GameSignal<string>();

	public static GameSignal TwitchReconnect = new GameSignal();

	public static GameSignal<OkDialogContainerPopulator.GenericDialog[]> OnOpenDialog = new GameSignal<OkDialogContainerPopulator.GenericDialog[]>();

	public static GameSignal<string, string, string[], Action[]> OnDisplayDialog = new GameSignal<string, string, string[], Action[]>();

	public static GameSignal<OkDialogContainerPopulator.GenericDialog> OnDisplayStartDialog = new GameSignal<OkDialogContainerPopulator.GenericDialog>();

	public static GameSignal OnCloseDialog = new GameSignal();

	public static GameSignal<string, float> OnAutoFadeDialog = new GameSignal<string, float>();

	public static GameSignal OnMenuToggleRequest = new GameSignal();

	public static GameSignal<Container> OnMenuApply = new GameSignal<Container>();

	public static GameSignal AfterChangeMap = new GameSignal();

	public static GameSignal<Crumb> OnMenuBread = new GameSignal<Crumb>();

	public static GameSignal<SectionBread> OnMenuBreadChange = new GameSignal<SectionBread>();

	public static GameSignal<ButtonBread[]> OnMenuBreadAddBtn = new GameSignal<ButtonBread[]>();

	public static GameSignal OnMenuBreadClearBtns = new GameSignal();

	public static GameSignal<BrushComponent> OnSelectBrushByType = new GameSignal<BrushComponent>();

	public static GameSignal<int> OnSelectBrush = new GameSignal<int>();

	public static GameSignal OnLoadBrushSettings = new GameSignal();

	public static GameSignal<Type> OnMenuOpen = new GameSignal<Type>();

	public static GameSignal<Type> OnMenuClose = new GameSignal<Type>();

	public static GameSignal<Type> OnMenuToggle = new GameSignal<Type>();

	public static GameSignal OnTerrainEdited = new GameSignal();

	private static readonly Action<Exception> DefaultRedirectException = delegate(Exception e)
	{
		logger.Error(e);
	};

	private void Awake()
	{
		SceneAwake.Invoke();
	}

	private void OnDestroy()
	{
		Invoke(LevelUnloaded);
		SceneDestroyed.Invoke();
	}

	public static GameSignal CreateStaticSignal()
	{
		return new GameSignal();
	}

	public static GameSignal<T> CreateStaticSignal<T>()
	{
		return new GameSignal<T>();
	}

	public static GameSignal<T1, T2> CreateStaticSignal<T1, T2>()
	{
		return new GameSignal<T1, T2>();
	}

	public static GameSignal<T1, T2, T3> CreateStaticSignal<T1, T2, T3>()
	{
		return new GameSignal<T1, T2, T3>();
	}

	public static GameSignal<T1, T2, T3, T4> CreateStaticSignal<T1, T2, T3, T4>()
	{
		return new GameSignal<T1, T2, T3, T4>();
	}

	public static void Invoke(GameSignal gs)
	{
		gs._signal.Invoke();
	}

	public static void Invoke<T>(GameSignal<T> gs, T t)
	{
		gs._signal.Invoke(t);
	}

	public static void Invoke<T1, T2>(GameSignal<T1, T2> gs, T1 t1, T2 t2)
	{
		gs._signal.Invoke(t1, t2);
	}

	public static void Invoke<T1, T2, T3>(GameSignal<T1, T2, T3> gs, T1 t1, T2 t2, T3 t3)
	{
		gs._signal.Invoke(t1, t2, t3);
	}

	public static void Invoke<T1, T2, T3, T4>(GameSignal<T1, T2, T3, T4> gs, T1 t1, T2 t2, T3 t3, T4 t4)
	{
		gs._signal.Invoke(t1, t2, t3, t4);
	}

	public static SignalConnection Connect(GameSignal gs, Action action, Action<Exception> redirectException = null)
	{
		return Connector.Connect(gs, action, redirectException);
	}

	public static SignalConnection Connect<T1>(GameSignal<T1> gs, Action<T1> action, Action<Exception> redirectException = null)
	{
		return Connector<T1>.Connect(gs, action, redirectException);
	}

	public static SignalConnection Connect<T1, T2>(GameSignal<T1, T2> gs, Action<T1, T2> action, Action<Exception> redirectException = null)
	{
		return Connector<T1, T2>.Connect(gs, action, redirectException);
	}

	public static SignalConnection Connect<T1, T2, T3>(GameSignal<T1, T2, T3> gs, Action<T1, T2, T3> action, Action<Exception> redirectException = null)
	{
		return Connector<T1, T2, T3>.Connect(gs, action, redirectException);
	}

	public static SignalConnection Connect<T1, T2, T3, T4>(GameSignal<T1, T2, T3, T4> gs, Action<T1, T2, T3, T4> action, Action<Exception> redirectException = null)
	{
		return Connector<T1, T2, T3, T4>.Connect(gs, action, redirectException);
	}

	public static SignalConnection Once(GameSignal gs, Action action, Action<Exception> redirectException = null)
	{
		return Connector.Once(gs, action, redirectException);
	}

	public static SignalConnection Once<T1>(GameSignal<T1> gs, Action<T1> action, Action<Exception> redirectException = null)
	{
		return Connector<T1>.Once(gs, action, redirectException);
	}

	public static SignalConnection Once<T1, T2>(GameSignal<T1, T2> gs, Action<T1, T2> action, Action<Exception> redirectException = null)
	{
		return Connector<T1, T2>.Once(gs, action, redirectException);
	}

	public static SignalConnection Once<T1, T2, T3>(GameSignal<T1, T2, T3> gs, Action<T1, T2, T3> action, Action<Exception> redirectException = null)
	{
		return Connector<T1, T2, T3>.Once(gs, action, redirectException);
	}

	public static SignalConnection Once<T1, T2, T3, T4>(GameSignal<T1, T2, T3, T4> gs, Action<T1, T2, T3, T4> action, Action<Exception> redirectException = null)
	{
		return Connector<T1, T2, T3, T4>.Once(gs, action, redirectException);
	}

	public static SignalConnection Connect<T1>(GameSignal gs, Action<T1> action, T1 t1, Action<Exception> redirectException = null)
	{
		return Connector<T1>.Connect(gs, action, t1, redirectException);
	}

	public static SignalConnection Connect<T1, T2>(GameSignal gs, Action<T1, T2> action, T1 t1, T2 t2, Action<Exception> redirectException = null)
	{
		return Connector<T1, T2>.Connect(gs, action, t1, t2, redirectException);
	}

	public static SignalConnection Connect<T1, T2, T3>(GameSignal gs, Action<T1, T2, T3> action, T1 t1, T2 t2, T3 t3, Action<Exception> redirectException = null)
	{
		return Connector<T1, T2, T3>.Connect(gs, action, t1, t2, t3, redirectException);
	}

	public static SignalConnection Connect<T1, T2, T3, T4>(GameSignal gs, Action<T1, T2, T3, T4> action, T1 t1, T2 t2, T3 t3, T4 t4, Action<Exception> redirectException = null)
	{
		return Connector<T1, T2, T3, T4>.Connect(gs, action, t1, t2, t3, t4, redirectException);
	}

	public static SignalConnection Connect<T1, T2>(GameSignal<T1> gs, Action<T1, T2> action, T2 t2, Action<Exception> redirectException = null)
	{
		return Connector<T1, T2>.Connect(gs, action, t2, redirectException);
	}

	public static SignalConnection Connect<T1, T2, T3>(GameSignal<T1> gs, Action<T1, T2, T3> action, T2 t2, T3 t3, Action<Exception> redirectException = null)
	{
		return Connector<T1, T2, T3>.Connect(gs, action, t2, t3, redirectException);
	}

	public static SignalConnection Connect<T1, T2, T3, T4>(GameSignal<T1> gs, Action<T1, T2, T3, T4> action, T2 t2, T3 t3, T4 t4, Action<Exception> redirectException = null)
	{
		return Connector<T1, T2, T3, T4>.Connect(gs, action, t2, t3, t4, redirectException);
	}

	public static SignalConnection Connect<T1, T2, T3>(GameSignal<T1, T2> gs, Action<T1, T2, T3> action, T3 t3, Action<Exception> redirectException = null)
	{
		return Connector<T1, T2, T3>.Connect(gs, action, t3, redirectException);
	}

	public static SignalConnection Connect<T1, T2, T3, T4>(GameSignal<T1, T2> gs, Action<T1, T2, T3, T4> action, T3 t3, T4 t4, Action<Exception> redirectException = null)
	{
		return Connector<T1, T2, T3, T4>.Connect(gs, action, t3, t4, redirectException);
	}

	public static SignalConnection Connect<T1, T2, T3, T4>(GameSignal<T1, T2, T3> gs, Action<T1, T2, T3, T4> action, T4 t4, Action<Exception> redirectException = null)
	{
		return Connector<T1, T2, T3, T4>.Connect(gs, action, t4, redirectException);
	}

	public static SignalConnection Once<T1>(GameSignal gs, Action<T1> action, T1 t1, Action<Exception> redirectException = null)
	{
		return Connector<T1>.Once(gs, action, t1, redirectException);
	}

	public static SignalConnection Once<T1, T2>(GameSignal gs, Action<T1, T2> action, T1 t1, T2 t2, Action<Exception> redirectException = null)
	{
		return Connector<T1, T2>.Once(gs, action, t1, t2, redirectException);
	}

	public static SignalConnection Once<T1, T2, T3>(GameSignal gs, Action<T1, T2, T3> action, T1 t1, T2 t2, T3 t3, Action<Exception> redirectException = null)
	{
		return Connector<T1, T2, T3>.Once(gs, action, t1, t2, t3, redirectException);
	}

	public static SignalConnection Once<T1, T2, T3, T4>(GameSignal gs, Action<T1, T2, T3, T4> action, T1 t1, T2 t2, T3 t3, T4 t4, Action<Exception> redirectException = null)
	{
		return Connector<T1, T2, T3, T4>.Once(gs, action, t1, t2, t3, t4, redirectException);
	}

	public static SignalConnection Once<T1, T2>(GameSignal<T1> gs, Action<T1, T2> action, T2 t2, Action<Exception> redirectException = null)
	{
		return Connector<T1, T2>.Once(gs, action, t2, redirectException);
	}

	public static SignalConnection Once<T1, T2, T3>(GameSignal<T1> gs, Action<T1, T2, T3> action, T2 t2, T3 t3, Action<Exception> redirectException = null)
	{
		return Connector<T1, T2, T3>.Once(gs, action, t2, t3, redirectException);
	}

	public static SignalConnection Once<T1, T2, T3, T4>(GameSignal<T1> gs, Action<T1, T2, T3, T4> action, T2 t2, T3 t3, T4 t4, Action<Exception> redirectException = null)
	{
		return Connector<T1, T2, T3, T4>.Once(gs, action, t2, t3, t4, redirectException);
	}

	public static SignalConnection Once<T1, T2, T3>(GameSignal<T1, T2> gs, Action<T1, T2, T3> action, T3 t3, Action<Exception> redirectException = null)
	{
		return Connector<T1, T2, T3>.Once(gs, action, t3, redirectException);
	}

	public static SignalConnection Once<T1, T2, T3, T4>(GameSignal<T1, T2> gs, Action<T1, T2, T3, T4> action, T3 t3, T4 t4, Action<Exception> redirectException = null)
	{
		return Connector<T1, T2, T3, T4>.Once(gs, action, t3, t4, redirectException);
	}

	public static SignalConnection Once<T1, T2, T3, T4>(GameSignal<T1, T2, T3> gs, Action<T1, T2, T3, T4> action, T4 t4, Action<Exception> redirectException = null)
	{
		return Connector<T1, T2, T3, T4>.Once(gs, action, t4, redirectException);
	}

	public static SignalConnection LateConnect(GameSignal gs, Action action, Action<Exception> redirectException = null)
	{
		if (gs._Count > 0)
		{
			SafeInvoke(action, redirectException);
			return SignalConnection.Empty;
		}
		return Connect(gs, action, redirectException);
	}

	public static SignalConnection LateConnect<T1>(GameSignal gs, Action<T1> action, T1 a, Action<Exception> redirectException = null)
	{
		if (gs._Count > 0)
		{
			SafeInvoke(a, action, redirectException);
			return SignalConnection.Empty;
		}
		return Connect(gs, action, a, redirectException);
	}

	public static SignalConnection LateConnect<T1, T2>(GameSignal gs, Action<T1, T2> action, T1 a, T2 b, Action<Exception> redirectException = null)
	{
		if (gs._Count > 0)
		{
			SafeInvoke(a, b, action, redirectException);
			return SignalConnection.Empty;
		}
		return Connect(gs, action, a, b, redirectException);
	}

	public static SignalConnection LateConnectOnce(GameSignal gs, Action action, Action<Exception> redirectException = null)
	{
		if (gs._Count > 0)
		{
			SafeInvoke(action, redirectException);
			return SignalConnection.Empty;
		}
		return Once(gs, action, redirectException);
	}

	public static SignalConnection LateConnectOnce<T1>(GameSignal gs, Action<T1> action, T1 a, Action<Exception> redirectException = null)
	{
		if (gs._Count > 0)
		{
			SafeInvoke(a, action, redirectException);
			return SignalConnection.Empty;
		}
		return Once(gs, action, a, redirectException);
	}

	public static SignalConnection LateConnectOnce<T1, T2>(GameSignal gs, Action<T1, T2> action, T1 a, T2 b, Action<Exception> redirectException = null)
	{
		if (gs._Count > 0)
		{
			SafeInvoke(a, b, action, redirectException);
			return SignalConnection.Empty;
		}
		return Once(gs, action, a, b, redirectException);
	}

	public static int Count<TAction>(GameSignalBase<TAction> gs)
	{
		return gs._Count;
	}

	public static bool Received<TAction>(GameSignalBase<TAction> gs)
	{
		return gs._Count > 0;
	}

	public static void DisableUntil(GameSignal gs, MonoBehaviour m, bool lateConnect = true)
	{
		if ((lateConnect && gs._Count > 0) || m == null)
		{
			return;
		}
		m.enabled = false;
		gs._signal.ConnectOnce(delegate(MonoBehaviour aM)
		{
			if (aM != null)
			{
				aM.enabled = true;
			}
		}, m);
	}

	private static void SafeInvoke(Action a, Action<Exception> redirectException = null)
	{
		try
		{
			a();
		}
		catch (Exception obj)
		{
			(redirectException ?? DefaultRedirectException)(obj);
		}
	}

	private static void SafeInvoke<T1>(T1 t1, Action<T1> a, Action<Exception> redirectException = null)
	{
		try
		{
			a(t1);
		}
		catch (Exception obj)
		{
			(redirectException ?? DefaultRedirectException)(obj);
		}
	}

	private static void SafeInvoke<T1, T2>(T1 t1, T2 t2, Action<T1, T2> a, Action<Exception> redirectException = null)
	{
		try
		{
			a(t1, t2);
		}
		catch (Exception obj)
		{
			(redirectException ?? DefaultRedirectException)(obj);
		}
	}

	private static void SafeInvoke<T1, T2, T3>(T1 t1, T2 t2, T3 t3, Action<T1, T2, T3> a, Action<Exception> redirectException = null)
	{
		try
		{
			a(t1, t2, t3);
		}
		catch (Exception obj)
		{
			(redirectException ?? DefaultRedirectException)(obj);
		}
	}

	private static void SafeInvoke<T1, T2, T3, T4>(T1 t1, T2 t2, T3 t3, T4 t4, Action<T1, T2, T3, T4> a, Action<Exception> redirectException = null)
	{
		try
		{
			a(t1, t2, t3, t4);
		}
		catch (Exception obj)
		{
			(redirectException ?? DefaultRedirectException)(obj);
		}
	}
}
