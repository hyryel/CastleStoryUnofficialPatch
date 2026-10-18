using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.Networking;

public class CaptureProgress : NetworkBehaviour
{
	[SerializeField]
	private int min;

	[SerializeField]
	public int max;

	[SerializeField]
	public float smooth;

	[SerializeField]
	[SyncVar(hook = "currentChanged_hook")]
	public float current;

	[SerializeField]
	public float previous;

	public float Networkcurrent
	{
		get
		{
			return current;
		}
		[param: In]
		set
		{
			ref float fieldValue = ref current;
			if (NetworkServer.localClientActive && !base.syncVarHookGuard)
			{
				base.syncVarHookGuard = true;
				currentChanged_hook(value);
				base.syncVarHookGuard = false;
			}
			SetSyncVar(value, ref fieldValue, 1u);
		}
	}

	public CaptureProgress(int min, int max, float smooth)
	{
		this.min = min;
		this.max = max;
		this.smooth = smooth;
		current = min;
		previous = min;
	}

	public void Reset()
	{
		Networkcurrent = 0f;
		previous = 0f;
	}

	public void Add(int inc)
	{
		Networkcurrent = Mathf.Clamp(current + (float)inc / smooth, min, max);
	}

	public float Ratio()
	{
		return current / (float)max;
	}

	public bool Changed()
	{
		return previous != current;
	}

	public bool Started()
	{
		return previous == (float)min && current != (float)min;
	}

	public bool Prevented()
	{
		return previous != (float)min && current == (float)min;
	}

	public bool Completed()
	{
		return previous != (float)max && current == (float)max;
	}

	public bool BeingCaptured()
	{
		return current > 0f && current != (float)max;
	}

	private void currentChanged_hook(float value)
	{
		previous = current;
		Networkcurrent = value;
	}

	private void UNetVersion()
	{
	}

	public override bool OnSerialize(NetworkWriter writer, bool forceAll)
	{
		if (forceAll)
		{
			writer.Write(current);
			return true;
		}
		bool flag = false;
		if ((base.syncVarDirtyBits & 1) != 0)
		{
			if (!flag)
			{
				writer.WritePackedUInt32(base.syncVarDirtyBits);
				flag = true;
			}
			writer.Write(current);
		}
		if (!flag)
		{
			writer.WritePackedUInt32(base.syncVarDirtyBits);
		}
		return flag;
	}

	public override void OnDeserialize(NetworkReader reader, bool initialState)
	{
		if (initialState)
		{
			current = reader.ReadSingle();
			return;
		}
		int num = (int)reader.ReadPackedUInt32();
		if ((num & 1) != 0)
		{
			currentChanged_hook(reader.ReadSingle());
		}
	}
}
