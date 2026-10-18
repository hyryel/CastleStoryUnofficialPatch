using Brix.Engine;
using Brix.Game.AI;
using UnityEngine;

public class SignalTester : MonoBehaviour
{
	private void Start()
	{
		GameSignals.LateConnect(UIGameObserver.IsReady, delegate
		{
			ConnectToSignals();
		});
	}

	private void ConnectToSignals()
	{
		UIGameObserver.bricktrons.EnterCombat.Connect(delegate(Labor l)
		{
			Debug.Log(string.Concat("Allied", l, " Started Fighting"));
		});
		UIGameObserver.bricktrons.ExitCombat.Connect(delegate(Labor l)
		{
			Debug.Log(string.Concat("Allied", l, " Stopped Fighting"));
		});
		GameSignals.Connect(Labor.DangerStarted, delegate(Labor l)
		{
			Debug.Log(string.Concat("Allied", l, " Started Fighting (DIRECT)"));
		});
		GameSignals.Connect(Labor.DangerEnded, delegate(Labor l)
		{
			Debug.Log(string.Concat("Allied", l, " Stopped Fighting (DIRECT)"));
		});
		UIGameObserver.capturables.CaptureStarted.Connect(delegate(IsCapturable c)
		{
			Debug.Log("Capture Started At " + c);
		});
		UIGameObserver.capturables.CaptureChanged.Connect(delegate(IsCapturable c)
		{
			Debug.Log("Capture Changed At " + c);
		});
		UIGameObserver.capturables.CaptureEnded.Connect(delegate(IsCapturable c)
		{
			Debug.Log("Capture Ended At " + c);
		});
	}
}
