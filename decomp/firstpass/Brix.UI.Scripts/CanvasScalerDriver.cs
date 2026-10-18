using System.Collections.Generic;
using Brix.Engine;
using Brix.External.Signals;
using Brix.NewUI.UserSettings_;
using UnityEngine;
using UnityEngine.UI;

namespace Brix.UI.Scripts;

public class CanvasScalerDriver : MonoBehaviour
{
	public List<CanvasScaler> CanvasScalers;

	private readonly List<SignalConnection> _sgs = new List<SignalConnection>();

	private void OnSetSettingIntoGame()
	{
		foreach (CanvasScaler canvasScaler in CanvasScalers)
		{
			canvasScaler.scaleFactor = Neo.canvasScale;
		}
	}

	private void Connect()
	{
		_sgs.Add(GameSignals.Connect(UserSettingManager.OnSetSettingIntoGameSignal, OnSetSettingIntoGame));
	}

	private void Disconnect()
	{
		foreach (SignalConnection sg in _sgs)
		{
			sg.Disconnect();
		}
		_sgs.Clear();
	}

	private void OnEnable()
	{
		OnSetSettingIntoGame();
		Connect();
	}

	private void OnDisable()
	{
		Disconnect();
	}
}
