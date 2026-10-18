using Brix.External.Signals;
using Brix.Game.Components;
using UnityEngine;

public class BehindCameraDetector : UpdatableGameComponent<BehindCameraDetector>
{
	[SerializeField]
	public Transform pivot;

	private Signal wentBehindCamera = new Signal();

	private Signal cameBackIntoCamera = new Signal();

	private bool wasBehind;

	private bool isBehind;

	public IReadOnlySignal WentBehindCamera => wentBehindCamera;

	public IReadOnlySignal CameBackIntoCamera => cameBackIntoCamera;

	public bool IsBehindCamera => isBehind;

	protected override void BrixUpdate()
	{
		CallCameraEvents();
	}

	private void CallCameraEvents()
	{
		wasBehind = isBehind;
		isBehind = ComputeBehindCamera();
		if (!wasBehind && isBehind)
		{
			wentBehindCamera.Invoke();
		}
		if (wasBehind && !isBehind)
		{
			cameBackIntoCamera.Invoke();
		}
	}

	private bool ComputeBehindCamera()
	{
		if (pivot == null)
		{
			return false;
		}
		Camera main = Camera.main;
		if (main == null)
		{
			return false;
		}
		Transform transform = main.transform;
		Vector3 vector = pivot.position - transform.position;
		if (Vector3.Angle(vector, transform.forward) > 90f)
		{
			return true;
		}
		return false;
	}
}
