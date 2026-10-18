using System;
using System.Runtime.Serialization;
using Newtonsoft.Json;
using UnityEngine;

namespace Brix.Lua;

[Serializable]
[JsonObject(MemberSerialization.OptIn)]
public class BrixTimer
{
	private float _startTime;

	[JsonProperty]
	private float _elapsedTime;

	[JsonProperty]
	private bool _running;

	[JsonProperty]
	private bool _initialized;

	[JsonProperty]
	private float _delay;

	[JsonProperty]
	private bool _useUnscaledTime;

	public bool running
	{
		get
		{
			return _running;
		}
		private set
		{
			if (_running && !value)
			{
				_elapsedTime = ElapsedSeconds();
			}
			else if (!_running && value)
			{
				if (_useUnscaledTime)
				{
					_startTime = Time.unscaledTime - _elapsedTime;
				}
				else
				{
					_startTime = Time.time - _elapsedTime;
				}
			}
			_running = value;
		}
	}

	public float delay => _delay;

	public bool useUnscaledTime
	{
		get
		{
			return _useUnscaledTime;
		}
		set
		{
			_useUnscaledTime = value;
		}
	}

	private BrixTimer()
	{
	}

	public BrixTimer(float time, bool unscaledTime = false)
	{
		_useUnscaledTime = unscaledTime;
		Set(time);
	}

	[OnSerializing]
	internal void OnSerializing(StreamingContext context)
	{
		if (_running)
		{
			_elapsedTime = ElapsedSeconds();
		}
	}

	[OnDeserialized]
	internal void OnDeserialized(StreamingContext context)
	{
		if (_useUnscaledTime)
		{
			_startTime = Time.unscaledTime - _elapsedTime;
		}
		else
		{
			_startTime = Time.time - _elapsedTime;
		}
	}

	public void Set(float time)
	{
		_startTime = 0f;
		_delay = time;
	}

	public void Start()
	{
		if (_useUnscaledTime)
		{
			_startTime = Time.unscaledTime;
		}
		else
		{
			_startTime = Time.time;
		}
		_initialized = true;
		running = true;
	}

	public void Stop()
	{
		running = false;
	}

	public bool Check()
	{
		if (!_initialized)
		{
			return false;
		}
		if (_useUnscaledTime)
		{
			return Time.unscaledTime >= _startTime + _delay;
		}
		return Time.time >= _startTime + _delay;
	}

	public float ElapsedSeconds()
	{
		if (_running)
		{
			if (_useUnscaledTime)
			{
				return Time.unscaledTime - _startTime;
			}
			return Time.time - _startTime;
		}
		return _elapsedTime;
	}

	public float RemainingSeconds()
	{
		return Mathf.Max(0f, _delay - ElapsedSeconds());
	}

	public string FormatedElapsedSeconds(float offsetTime = 0f)
	{
		int num = Mathf.FloorToInt(ElapsedSeconds() + offsetTime);
		if (num < 3600)
		{
			return $"{num / 60}:{num % 60:D2}";
		}
		return $"{num / 3600}:{num / 60 % 60:D2}:{num % 60:D2}";
	}

	public string FormatedRemainingSeconds()
	{
		int num = Mathf.FloorToInt(RemainingSeconds());
		if (num < 3600)
		{
			return $"{num / 60}:{num % 60:D2}";
		}
		return $"{num / 3600}:{num / 60 % 60:D2}:{num % 60:D2}";
	}

	public float PercentDone()
	{
		float num = ElapsedSeconds() / _delay;
		return (!(num > 1f)) ? num : 1f;
	}

	public void Reset()
	{
		if (_useUnscaledTime)
		{
			_startTime = Time.unscaledTime;
		}
		else
		{
			_startTime = Time.time;
		}
	}
}
