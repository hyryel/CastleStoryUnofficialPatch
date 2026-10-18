using System;
using System.Collections.Generic;
using Brix.Utils;
using Brix.Utils.Collections;
using Smooth.Collections;
using Smooth.Slinq;
using UnityEngine;

namespace Brix.NewUI.LoadingScreen;

public static class LoadingScreenTipDispenser
{
	private static List<Tip> tips;

	private static float tipRatio = 0.5f;

	public static Tip GetARandomTipFor(string gamemode)
	{
		if (UnityEngine.Random.value < tipRatio)
		{
			tips = TipReader.ReadTips();
		}
		else
		{
			tips = TipReader.ReadTrivia();
		}
		return RandomTip(gamemode);
	}

	private static Tip RandomTip(string gamemode)
	{
		Tip tip = null;
		List<Tip> list = (from t in tips.Slinq()
			where t.gameMode == gamemode || t.gameMode == "Any"
			select t).ToPooledList();
		tip = list.Random().ValueOr(() => (Tip)null);
		list.Dispose();
		if (tip == null)
		{
			throw new ArgumentException($"No tip for gamemode '{gamemode}'", "gamemode");
		}
		return tip;
	}
}
