using System.Collections.Generic;
using Brix.Engine;
using Brix.Game.Rules;
using Brix.Input;
using Brix.Utils;
using UnityEngine;

public class ClockConfig : MonoBehaviour
{
	public enum ClockSpeed
	{
		Default,
		Paused,
		Double,
		Half,
		Tenth,
		Fast,
		VeryFast,
		Unlisted
	}

	private static float defaultTimeScale = 1f;

	private static float pausedTimeScale = 0f;

	private static float doubleSpeedTimeScale = 2f;

	private static float halfSpeedTimeScale = 0.5f;

	private static float tenthSpeedTimeScale = 0.1f;

	private static float fastSpeedTimeScale = 1.5f;

	private static float veryFastSpeedTimeScale = 5f;

	private static ClockSpeed _unpausedClockSpeed = ClockSpeed.Default;

	private static readonly Dictionary<ClockSpeed, float> ClockSpeedMap = new Dictionary<ClockSpeed, float>(FastEnumIntEqualityComparer<ClockSpeed>.Instance)
	{
		{
			ClockSpeed.Default,
			defaultTimeScale
		},
		{
			ClockSpeed.Paused,
			pausedTimeScale
		},
		{
			ClockSpeed.Double,
			doubleSpeedTimeScale
		},
		{
			ClockSpeed.Half,
			halfSpeedTimeScale
		},
		{
			ClockSpeed.Tenth,
			tenthSpeedTimeScale
		},
		{
			ClockSpeed.Fast,
			fastSpeedTimeScale
		},
		{
			ClockSpeed.VeryFast,
			veryFastSpeedTimeScale
		},
		{
			ClockSpeed.Unlisted,
			-1f
		}
	};

	public static bool Paused
	{
		get
		{
			return GetTimeScale().Equals(pausedTimeScale);
		}
		set
		{
			if (Neo.multiplayer && !GeneralRules.AllowPauseInMultiplayer)
			{
				return;
			}
			bool flag = value;
			bool paused = Paused;
			if (flag != paused)
			{
				if (flag)
				{
					_unpausedClockSpeed = GetClockSpeedFromTimeScale();
					SetTimeScale(ClockSpeed.Paused);
				}
				else
				{
					SetTimeScale(_unpausedClockSpeed);
				}
				Picking.actif = !flag;
				GameSignals.Invoke(Journee.OnSetIsPaused);
			}
		}
	}

	public static float GetClockSpeedTimeScale(ClockSpeed clockSpeed)
	{
		float value;
		return (!ClockSpeedMap.TryGetValue(clockSpeed, out value)) ? defaultTimeScale : value;
	}

	public static float GetTimeScale()
	{
		return Time.timeScale;
	}

	public static void SetTimeScale(ClockSpeed clockSpeed)
	{
		Time.timeScale = GetClockSpeedTimeScale(clockSpeed);
	}

	public static ClockSpeed GetClockSpeedFromTimeScale()
	{
		foreach (KeyValuePair<ClockSpeed, float> item in ClockSpeedMap)
		{
			if (item.Value.Equals(GetTimeScale()))
			{
				return item.Key;
			}
		}
		return ClockSpeed.Unlisted;
	}

	public static void SetClockSpeed(ClockSpeed dstClockSpeed)
	{
		if (Neo.multiplayer)
		{
			dstClockSpeed = ClockSpeed.Default;
		}
		if (dstClockSpeed != ClockSpeed.Paused)
		{
			ClockSpeed clockSpeedFromTimeScale = GetClockSpeedFromTimeScale();
			if (dstClockSpeed != clockSpeedFromTimeScale)
			{
				_unpausedClockSpeed = dstClockSpeed;
				SetTimeScale(_unpausedClockSpeed);
			}
		}
	}

	public static void ToggleClockSpeed(ClockSpeed dstClockSpeed)
	{
		if (Neo.multiplayer)
		{
			dstClockSpeed = ClockSpeed.Default;
		}
		if (dstClockSpeed != ClockSpeed.Paused && dstClockSpeed != ClockSpeed.Default)
		{
			ClockSpeed unpausedClockSpeed = _unpausedClockSpeed;
			if (dstClockSpeed == unpausedClockSpeed)
			{
				dstClockSpeed = ClockSpeed.Default;
			}
			_unpausedClockSpeed = dstClockSpeed;
			if (!Paused)
			{
				SetTimeScale(_unpausedClockSpeed);
			}
		}
	}
}
