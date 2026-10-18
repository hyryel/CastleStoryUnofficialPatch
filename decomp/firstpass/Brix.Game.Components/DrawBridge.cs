using Brix.Engine;
using Brix.Game.Components.Volumes;
using UnityEngine;
using UnityEngine.Networking;

namespace Brix.Game.Components;

public class DrawBridge : Volume
{
	public enum State
	{
		Down,
		Up,
		GoingUp,
		GoingDown
	}

	public Transform bridge;

	public Transform modelUp;

	public Transform modelDown;

	public float rotationSpeed = 10f;

	public float clampTolerance = 5f;

	public State wantState;

	public State state;

	private State exState;

	public float ElevationAngle => 90f - Vector3.Angle(bridge.forward, Vector3.up);

	public override XYZ Position => XYZ.zero;

	private void Update()
	{
		UpdateRotation();
		UpdateVolume();
	}

	private void UpdateRotation()
	{
		if (bridge == null || modelUp == null || modelDown == null)
		{
			return;
		}
		if (wantState == State.Up && state != State.Up)
		{
			state = State.GoingUp;
			bridge.rotation = Quaternion.RotateTowards(bridge.rotation, modelUp.rotation, rotationSpeed * Time.deltaTime);
			if (Quaternion.Angle(bridge.rotation, modelUp.rotation) < clampTolerance)
			{
				bridge.rotation = modelUp.rotation;
				state = State.Up;
			}
		}
		else if (wantState == State.Down && state != State.Down)
		{
			state = State.GoingDown;
			bridge.rotation = Quaternion.RotateTowards(bridge.rotation, modelDown.rotation, rotationSpeed * Time.deltaTime);
			if (Quaternion.Angle(bridge.rotation, modelDown.rotation) < clampTolerance)
			{
				bridge.rotation = modelDown.rotation;
				state = State.Down;
			}
		}
	}

	private void UpdateVolume()
	{
		if (exState != this.state)
		{
			State state = this.state;
			this.state = exState;
			Unregister();
			this.state = state;
			Register();
			exState = this.state;
		}
	}

	private void UNetVersion()
	{
	}

	public override bool OnSerialize(NetworkWriter writer, bool forceAll)
	{
		bool flag = base.OnSerialize(writer, forceAll);
		bool flag2 = default(bool);
		return flag2 || flag;
	}

	public override void OnDeserialize(NetworkReader reader, bool initialState)
	{
		base.OnDeserialize(reader, initialState);
	}
}
