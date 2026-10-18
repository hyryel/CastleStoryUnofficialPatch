using System;
using Brix.Game;
using Brix.Game.AI;
using Brix.Game.Network;
using Brix.Lua;
using MoonSharp.Interpreter;
using Rewired;
using Smooth.Algebraics;
using UnityEngine;

namespace Brix.Input;

[RequireComponent(typeof(Labor))]
public class CombatHotkeys : MonoBehaviour
{
	private class KeyPress
	{
		private readonly string key;

		private readonly Action<Labor> action;

		public KeyPress(string key, Action<Labor> action)
		{
			this.key = key;
			this.action = action;
		}

		public void Handle(Player _player, Labor labor)
		{
			if (_player.GetButtonDown(key))
			{
				action(labor);
			}
		}
	}

	private Player _player;

	private Labor _labor;

	private static bool hotkeysActivated = false;

	private static KeyPress stance = new KeyPress("bricktron_stance", OnStanceKeyPressed);

	private static KeyPress stop = new KeyPress("bricktron_stop", OnStopKeyPressed);

	private static Tuple<AutonomyStatus, int> statusAtFrame;

	private bool IsSelected => UIGameObserver.bricktrons.GetSelectionFlag(_labor);

	private void Start()
	{
		_player = ReInput.players.GetPlayer("local");
		_labor = GetComponent<Labor>();
	}

	private static void OnStanceKeyPressed(Labor labor)
	{
		if (statusAtFrame._2 < Time.frameCount)
		{
			AutonomyStatus onPlayerToggle = labor.Autonomy.OnPlayerToggle;
			statusAtFrame = Tuple.Create(onPlayerToggle, Time.frameCount);
		}
		User.LocalUser.GetComponent<UNetProjectCmd>().CallCmdSetAutonomy(labor.gameObject, (int)statusAtFrame._1.Id);
	}

	private static void OnStopKeyPressed(Labor labor)
	{
		if (labor.Autonomy.Id == AutonomyStatus.Ids.freeAgent)
		{
			Order.Move.HaltAndWait.Send(labor);
		}
		else
		{
			Order.Move.HaltWhatever.Send(labor);
		}
	}

	private void Update()
	{
		if (IsSelected && hotkeysActivated)
		{
			stance.Handle(_player, _labor);
			stop.Handle(_player, _labor);
		}
	}

	[LuaDrivable("ToggleHotkeys")]
	private static DynValue lua_ToggleHotkeys(ScriptExecutionContext context, CallbackArguments args)
	{
		bool flag = args.AsBool(0, "ToggleHotkeys");
		hotkeysActivated = flag;
		return DynValue.Void;
	}
}
