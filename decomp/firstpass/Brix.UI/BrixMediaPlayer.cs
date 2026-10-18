using System;
using Brix.Components;
using Brix.Lua;
using Brix.Utils;
using MoonSharp.Interpreter;
using UnityEngine.Events;

namespace Brix.UI;

public class BrixMediaPlayer : BrixSingleton<BrixMediaPlayer>
{
	public MediaPlayer Player;

	private bool _playing;

	private bool _waitingToPlay;

	private static readonly Action<BrixMediaPlayer> InvokeAction = delegate(BrixMediaPlayer self)
	{
		if (!(self == null))
		{
			LuaHookBase.InvokeFromCLR(self.lua_OnReadyToPlay);
		}
	};

	private static readonly UnityAction<BrixMediaPlayer, MediaPlayer, MediaPlayerEvent.EventType, RenderHeads.Media.AVProVideo.ErrorCode> OnMediaReadyUnityAction = delegate(BrixMediaPlayer self, MediaPlayer mediaPlayer, MediaPlayerEvent.EventType eventType, RenderHeads.Media.AVProVideo.ErrorCode errorCode)
	{
		if (!(self == null) && mediaPlayer == self.Player && eventType == MediaPlayerEvent.EventType.ReadyToPlay && errorCode == RenderHeads.Media.AVProVideo.ErrorCode.None)
		{
			self.OnReadyToPlay();
		}
	};

	[LuaDrivable("hk_OnReadyToPlay")]
	private readonly LuaHook lua_OnReadyToPlay = new LuaHook();

	[LuaDrivable("path")]
	private string lua_path
	{
		get
		{
			return Player.m_VideoPath;
		}
		set
		{
			Player.m_VideoPath = value;
		}
	}

	[LuaDrivable("loop")]
	private bool lua_loop
	{
		get
		{
			return Player.m_Loop;
		}
		set
		{
			Player.m_Loop = value;
		}
	}

	[LuaDrivable("muted")]
	private bool lua_muted
	{
		get
		{
			return Player.m_Muted;
		}
		set
		{
			Player.m_Muted = value;
		}
	}

	[LuaDrivable("autoOpen")]
	private bool lua_autoOpen
	{
		get
		{
			return Player.m_AutoOpen;
		}
		set
		{
			Player.m_AutoOpen = value;
		}
	}

	[LuaDrivable("autoStart")]
	private bool lua_autoStart
	{
		get
		{
			return Player.m_AutoStart;
		}
		set
		{
			Player.m_AutoStart = value;
		}
	}

	[LuaDrivable("volume")]
	private float lua_volume
	{
		get
		{
			return Player.m_Volume;
		}
		set
		{
			Player.m_Volume = value;
		}
	}

	private void Load()
	{
		Player.Events.RemoveAllListeners();
		Player.Events.AddListener(OnMediaReadyUnityAction);
		Player.OpenVideoFromFile(MediaPlayer.FileLocation.RelativeToStreamingAssetsFolder, Player.m_VideoPath, Player.m_AutoStart);
	}

	private void Play()
	{
		if (!_playing && !_waitingToPlay)
		{
			_waitingToPlay = true;
		}
	}

	private void Stop()
	{
		if (_playing)
		{
			Player.Stop();
		}
		_waitingToPlay = false;
		_playing = false;
	}

	private void Rewind()
	{
	}

	private void OnReadyToPlay()
	{
		if (_waitingToPlay && !_playing)
		{
			_playing = true;
			Player.Play();
			PersistentCoroutineRunner.Relais(InvokeAction, this);
		}
	}

	[LuaDrivable("PlayFIX")]
	private static DynValue lua_Play(ScriptExecutionContext executionContext, CallbackArguments args)
	{
		BrixSingleton<BrixMediaPlayer>.Instance.Play();
		return DynValue.Void;
	}

	[LuaDrivable("StopFIX")]
	private static DynValue lua_Stop(ScriptExecutionContext executionContext, CallbackArguments args)
	{
		BrixSingleton<BrixMediaPlayer>.Instance.Stop();
		return DynValue.Void;
	}

	[LuaDrivable("LoadFIX")]
	private static DynValue lua_Load(ScriptExecutionContext executionContext, CallbackArguments args)
	{
		BrixSingleton<BrixMediaPlayer>.Instance.Load();
		return DynValue.Void;
	}

	[LuaDrivable("RewindFIX")]
	private static DynValue lua_Rewind(ScriptExecutionContext executionContext, CallbackArguments args)
	{
		BrixSingleton<BrixMediaPlayer>.Instance.Rewind();
		return DynValue.Void;
	}
}
