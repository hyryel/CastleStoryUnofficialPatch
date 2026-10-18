using System.Diagnostics;
using UnityEngine;

namespace Brix.Game.Components;

public class LaggerComponent : MonoBehaviour
{
	public int sleepDelay;

	private Stopwatch myWatch = new Stopwatch();

	private void FixedUpdate()
	{
		myWatch.Start();
		while (myWatch.ElapsedMilliseconds < sleepDelay)
		{
		}
		myWatch.Reset();
	}
}
