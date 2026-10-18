using System;
using System.Collections.Generic;
using System.Diagnostics;
using Brix.Engine;
using Brix.Game.Components;
using Brix.Lua;
using Brix.Network.Util;
using Brix.Utils;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Serialization;

namespace Brix.Game;

[SerializerTarget(new string[] { "save" })]
public class GameClock : MonoBehaviour
{
	public class ClockSignal
	{
		protected IUpdatableComponent[] UpdateList = Empty.Array<IUpdatableComponent>();

		protected int Count;

		private readonly Dictionary<IUpdatableComponent, int> _updateIndices = new Dictionary<IUpdatableComponent, int>();

		public int ConnectionCount => Count;

		public bool Disconnect(IUpdatableComponent updatable)
		{
			if (_updateIndices.TryGetValue(updatable, out var value))
			{
				int num = --Count;
				if (value != num)
				{
					IUpdatableComponent updatableComponent = UpdateList[num];
					UpdateList[value] = updatableComponent;
					_updateIndices[updatableComponent] = value;
				}
				UpdateList[num] = null;
				_updateIndices.Remove(updatable);
				return true;
			}
			return false;
		}

		public Connection Connect(IUpdatableComponent updatable)
		{
			int num = Count++;
			if (num >= UpdateList.Length)
			{
				Array.Resize(ref UpdateList, Mathf.NextPowerOfTwo((Count * 3 + 1) / 2));
			}
			UpdateList[num] = updatable;
			_updateIndices[updatable] = num;
			return new Connection(updatable, this);
		}
	}

	public class UpdateSignal : ClockSignal
	{
		public void SafeInvoke()
		{
			for (int i = 0; i < Count; i++)
			{
				IUpdatableComponent updatableComponent = UpdateList[i];
				try
				{
					updatableComponent.BrixUpdate();
				}
				catch (Exception e)
				{
					updatableComponent.OnUpdateError(e);
				}
				finally
				{
				}
			}
		}
	}

	public class FixedUpdateSignal : ClockSignal
	{
		public void UnorderedSafeInvoke()
		{
			for (int i = 0; i < Count; i++)
			{
				IUpdatableComponent updatableComponent = UpdateList[i];
				try
				{
					updatableComponent.BrixFixedUpdate();
				}
				catch (Exception e)
				{
					updatableComponent.OnFixedUpdateError(e);
				}
				finally
				{
				}
			}
		}
	}

	public class LateUpdateSignal : ClockSignal
	{
		public void SafeInvoke()
		{
			for (int i = 0; i < Count; i++)
			{
				IUpdatableComponent updatableComponent = UpdateList[i];
				try
				{
					updatableComponent.BrixLateUpdate();
				}
				catch (Exception e)
				{
					updatableComponent.OnLateUpdateError(e);
				}
				finally
				{
				}
			}
		}
	}

	public class SlowUpdateSignal : ClockSignal
	{
		public void SafeInvoke()
		{
			for (int i = 0; i < Count; i++)
			{
				IUpdatableComponent updatableComponent = UpdateList[i];
				try
				{
					updatableComponent.BrixSlowUpdate();
				}
				catch (Exception e)
				{
					updatableComponent.OnSlowUpdateError(e);
				}
				finally
				{
				}
			}
		}
	}

	public struct Connection
	{
		public static readonly Connection Empty = default(Connection);

		private IUpdatableComponent _updatable;

		private ClockSignal _signal;

		public Connection(IUpdatableComponent updatable, ClockSignal signal)
		{
			_updatable = updatable;
			_signal = signal;
		}

		public bool Disconnect()
		{
			if (_signal == null)
			{
				return false;
			}
			bool result = _signal.Disconnect(_updatable);
			_signal = null;
			_updatable = null;
			return result;
		}
	}

	private static GameClock instance;

	public static WaitForEndOfFrame WaitForEndOfFrame = null;

	public static long TicksPerFrame = (long)(0.0166 * (double)Stopwatch.Frequency);

	protected readonly UpdateSignal UpdateSig = new UpdateSignal();

	protected readonly FixedUpdateSignal FixedUpdateSig = new FixedUpdateSignal();

	protected readonly LateUpdateSignal LateUpdateSig = new LateUpdateSignal();

	protected readonly SlowUpdateSignal SlowUpdateSig_50 = new SlowUpdateSignal();

	protected readonly SlowUpdateSignal SlowUpdateSig_100 = new SlowUpdateSignal();

	protected readonly SlowUpdateSignal SlowUpdateSig_200 = new SlowUpdateSignal();

	protected readonly SlowUpdateSignal SlowUpdateSig_500 = new SlowUpdateSignal();

	protected readonly SlowUpdateSignal SlowUpdateSig_1s = new SlowUpdateSignal();

	protected readonly SlowUpdateSignal SlowUnscaledSig_50 = new SlowUpdateSignal();

	protected readonly SlowUpdateSignal SlowUnscaledSig_100 = new SlowUpdateSignal();

	protected readonly SlowUpdateSignal SlowUnscaledSig_200 = new SlowUpdateSignal();

	protected readonly SlowUpdateSignal SlowUnscaledSig_500 = new SlowUpdateSignal();

	protected readonly SlowUpdateSignal SlowUnscaledSig_1s = new SlowUpdateSignal();

	[JsonProperty]
	private SerializableTimestamp slowTimestamp_50;

	[JsonProperty]
	private SerializableTimestamp slowTimestamp_100;

	[JsonProperty]
	private SerializableTimestamp slowTimestamp_200;

	[JsonProperty]
	private SerializableTimestamp slowTimestamp_500;

	[JsonProperty]
	private SerializableTimestamp slowTimestamp_1s;

	[JsonProperty]
	private SerializableTimestamp slowUnscaledTimestamp_50;

	[JsonProperty]
	private SerializableTimestamp slowUnscaledTimestamp_100;

	[JsonProperty]
	private SerializableTimestamp slowUnscaledTimestamp_200;

	[JsonProperty]
	private SerializableTimestamp slowUnscaledTimestamp_500;

	[JsonProperty]
	private SerializableTimestamp slowUnscaledTimestamp_1s;

	[JsonProperty]
	[FormerlySerializedAs("timestamp")]
	private SerializableTimestamp gameModeTimestamp;

	[JsonProperty]
	private bool gameModeStarted;

	[JsonProperty]
	private int _updateIndex;

	public static bool Exists => instance != null;

	[LuaDrivable("age")]
	public static float Age
	{
		get
		{
			instance.gameModeTimestamp = instance.gameModeTimestamp ?? new SerializableTimestamp();
			return instance.gameModeTimestamp.Age;
		}
		set
		{
			instance.gameModeTimestamp = instance.gameModeTimestamp ?? new SerializableTimestamp();
			instance.gameModeTimestamp.Age = value;
		}
	}

	[LuaDrivable("timeSinceGameStart")]
	public static float TimeSinceGameStart => (!instance.gameModeStarted) ? 0f : instance.gameModeTimestamp.Delta;

	[LuaDrivable("formatedTimeSinceGameStart")]
	private static string lua_formatedTimeSinceGameStart
	{
		get
		{
			int num = Mathf.FloorToInt(TimeSinceGameStart);
			return $"{num / 60}:{num % 60:D2}";
		}
	}

	public static int UpdateIndex => instance._updateIndex;

	public static Connection AddOnFixedUpdate(IUpdatableComponent updatable)
	{
		if (instance == null)
		{
			return Connection.Empty;
		}
		return instance.FixedUpdateSig.Connect(updatable);
	}

	public static Connection AddOnUpdate(IUpdatableComponent updatable)
	{
		if (instance == null)
		{
			return Connection.Empty;
		}
		return instance.UpdateSig.Connect(updatable);
	}

	public static Connection AddOnLateUpdate(IUpdatableComponent updatable)
	{
		if (instance == null)
		{
			return Connection.Empty;
		}
		return instance.LateUpdateSig.Connect(updatable);
	}

	private static ClockSignal AddOnSlowUpdate(float updateRate = 0.1f, bool unscaled = false)
	{
		if (instance == null)
		{
			return null;
		}
		if (updateRate < 0.075f)
		{
			return (!unscaled) ? instance.SlowUpdateSig_50 : instance.SlowUnscaledSig_50;
		}
		if (updateRate < 0.15f)
		{
			return (!unscaled) ? instance.SlowUpdateSig_100 : instance.SlowUnscaledSig_100;
		}
		if (updateRate < 0.35f)
		{
			return (!unscaled) ? instance.SlowUpdateSig_200 : instance.SlowUnscaledSig_200;
		}
		if (updateRate < 0.75f)
		{
			return (!unscaled) ? instance.SlowUpdateSig_500 : instance.SlowUnscaledSig_500;
		}
		return (!unscaled) ? instance.SlowUpdateSig_1s : instance.SlowUnscaledSig_1s;
	}

	public static Connection AddOnSlowUpdate(IUpdatableComponent updatable, float updateRate = 0.1f, bool unscaled = false)
	{
		if (instance == null)
		{
			return Connection.Empty;
		}
		return AddOnSlowUpdate(updateRate, unscaled)?.Connect(updatable) ?? Connection.Empty;
	}

	private void Awake()
	{
		if (instance != null)
		{
			throw new Exception("Two GameClock singletons are being registered, check your stuff");
		}
		instance = this;
	}

	private void Start()
	{
		base.enabled = false;
		GameSignals.Once(GameSignals.LevelReady, EnableTicking);
		GameSignals.Connect(GameSignals.GameModeTimersStarted, StartGameModeTimer);
	}

	private static void EnableTicking()
	{
		instance.gameModeTimestamp = instance.gameModeTimestamp ?? new SerializableTimestamp();
		instance.slowTimestamp_50 = new SerializableTimestamp();
		instance.slowTimestamp_100 = new SerializableTimestamp();
		instance.slowTimestamp_200 = new SerializableTimestamp();
		instance.slowTimestamp_500 = new SerializableTimestamp();
		instance.slowTimestamp_1s = new SerializableTimestamp();
		instance.slowUnscaledTimestamp_50 = new SerializableTimestamp();
		instance.slowUnscaledTimestamp_100 = new SerializableTimestamp();
		instance.slowUnscaledTimestamp_200 = new SerializableTimestamp();
		instance.slowUnscaledTimestamp_500 = new SerializableTimestamp();
		instance.slowUnscaledTimestamp_1s = new SerializableTimestamp();
		instance.enabled = true;
	}

	private void Update()
	{
		_updateIndex++;
		UpdateSig.SafeInvoke();
		_SlowUpdate();
	}

	private void FixedUpdate()
	{
		_updateIndex++;
		FixedUpdateSig.UnorderedSafeInvoke();
	}

	private void LateUpdate()
	{
		_updateIndex++;
		LateUpdateSig.SafeInvoke();
	}

	private void _SlowUpdate()
	{
		SlowUpdate_50();
		SlowUpdate_100();
		SlowUpdate_200();
		SlowUpdate_500();
		SlowUpdate_1s();
		SlowUnscaledUpdate_50();
		SlowUnscaledUpdate_100();
		SlowUnscaledUpdate_200();
		SlowUnscaledUpdate_500();
		SlowUnscaledUpdate_1s();
	}

	private void SlowUpdate_50()
	{
		if (!(slowTimestamp_50.Timestamp > Time.time))
		{
			slowTimestamp_50.Timestamp = Time.time + 0.047f;
			SlowUpdateSig_50.SafeInvoke();
		}
	}

	private void SlowUpdate_100()
	{
		if (!(slowTimestamp_100.Timestamp > Time.time))
		{
			slowTimestamp_100.Timestamp = Time.time + 0.101f;
			SlowUpdateSig_100.SafeInvoke();
		}
	}

	private void SlowUpdate_200()
	{
		if (!(slowTimestamp_200.Timestamp > Time.time))
		{
			slowTimestamp_200.Timestamp = Time.time + 0.227f;
			SlowUpdateSig_200.SafeInvoke();
		}
	}

	private void SlowUpdate_500()
	{
		if (!(slowTimestamp_500.Timestamp > Time.time))
		{
			slowTimestamp_500.Timestamp = Time.time + 0.467f;
			SlowUpdateSig_500.SafeInvoke();
		}
	}

	private void SlowUpdate_1s()
	{
		if (!(slowTimestamp_1s.Timestamp > Time.time))
		{
			slowTimestamp_1s.Timestamp = Time.time + 0.983f;
			SlowUpdateSig_1s.SafeInvoke();
		}
	}

	private void SlowUnscaledUpdate_50()
	{
		if (!(slowUnscaledTimestamp_50.Timestamp > Time.unscaledTime))
		{
			slowUnscaledTimestamp_50.Timestamp = Time.unscaledTime + 0.47f;
			SlowUnscaledSig_50.SafeInvoke();
		}
	}

	private void SlowUnscaledUpdate_100()
	{
		if (!(slowUnscaledTimestamp_100.Timestamp > Time.unscaledTime))
		{
			slowUnscaledTimestamp_100.Timestamp = Time.unscaledTime + 0.101f;
			SlowUnscaledSig_100.SafeInvoke();
		}
	}

	private void SlowUnscaledUpdate_200()
	{
		if (!(slowUnscaledTimestamp_200.Timestamp > Time.unscaledTime))
		{
			slowUnscaledTimestamp_200.Timestamp = Time.unscaledTime + 0.227f;
			SlowUnscaledSig_200.SafeInvoke();
		}
	}

	private void SlowUnscaledUpdate_500()
	{
		if (!(slowUnscaledTimestamp_500.Timestamp > Time.unscaledTime))
		{
			slowUnscaledTimestamp_500.Timestamp = Time.unscaledTime + 0.467f;
			SlowUnscaledSig_500.SafeInvoke();
		}
	}

	private void SlowUnscaledUpdate_1s()
	{
		if (!(slowUnscaledTimestamp_1s.Timestamp > Time.unscaledTime))
		{
			slowUnscaledTimestamp_1s.Timestamp = Time.unscaledTime + 0.983f;
			SlowUnscaledSig_1s.SafeInvoke();
		}
	}

	private void StartGameModeTimer()
	{
		instance.gameModeTimestamp = instance.gameModeTimestamp ?? new SerializableTimestamp();
		if (!gameModeStarted)
		{
			gameModeTimestamp.Reset();
		}
		gameModeStarted = true;
	}

	private void OnDestroy()
	{
		instance = null;
	}
}
