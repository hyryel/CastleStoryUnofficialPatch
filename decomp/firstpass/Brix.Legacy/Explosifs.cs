using System.Collections;
using Brix.Audio;
using Brix.Engine;
using Brix.Game.Components;
using Brix.Lifecycle.Pooling;
using Brix.Network;
using UnityEngine;
using UnityEngine.Networking;

namespace Brix.Legacy;

public class Explosifs : NetworkGameComponent
{
	private bool armed;

	[SerializeField]
	private float intensity = 5f;

	[SerializeField]
	private string effectTemplate = "Explosion";

	private static int kRpcRpcStartFuse;

	public void Explode()
	{
		Explode(intensity);
	}

	public void Explode(float intensity)
	{
		Architecte.ExplosionSimple(base.gameObject, base.transform.position, intensity, effectTemplate);
		if (!base.gameObject.IsNullOrReleased())
		{
			ObjectPoolSingleton.Release(base.gameObject);
		}
	}

	public override void OnRequested()
	{
		base.OnRequested();
		armed = false;
	}

	private void OnTransformParentChanged()
	{
		if (armed)
		{
			Explode(intensity);
		}
	}

	private IEnumerator FuseCoroutine()
	{
		SoundEngine.Play(FuseSound(effectTemplate), base.gameObject);
		armed = true;
		Emit(b: true);
		yield return new WaitForSeconds(4f);
		Emit(b: false);
		yield return new WaitForSeconds(0.7f);
		if (NetworkServer.active)
		{
			Explode(intensity);
		}
	}

	private string FuseSound(string template)
	{
		return template switch
		{
			"Explosion" => "Barrel_Fuse", 
			"ExplosionAlchemistPotion" => "Bomb_Start", 
			"Petite" => "Barrel_Fuse", 
			_ => "Barrel_Fuse", 
		};
	}

	[ClientRpc]
	public void RpcStartFuse()
	{
		StartCoroutine(FuseCoroutine());
	}

	public void Emit(bool b)
	{
		ParticleSystem[] componentsInChildren = base.transform.GetComponentsInChildren<ParticleSystem>();
		ParticleSystem[] array = componentsInChildren;
		foreach (ParticleSystem particleSystem in array)
		{
			if (b)
			{
				particleSystem.Play();
			}
			else
			{
				particleSystem.Stop();
			}
		}
	}

	private void UNetVersion()
	{
	}

	protected static void InvokeRpcRpcStartFuse(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcStartFuse called on server.");
		}
		else
		{
			((Explosifs)obj).RpcStartFuse();
		}
	}

	public void CallRpcStartFuse()
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("RPC Function RpcStartFuse called on client.");
			return;
		}
		NetworkWriter networkWriter = new NetworkWriter();
		networkWriter.Write((short)0);
		networkWriter.Write((short)2);
		networkWriter.WritePackedUInt32((uint)kRpcRpcStartFuse);
		networkWriter.Write(GetComponent<NetworkIdentity>().netId);
		SendRPCInternal(networkWriter, 0, "RpcStartFuse");
	}

	static Explosifs()
	{
		kRpcRpcStartFuse = -2032729366;
		NetworkBehaviour.RegisterRpcDelegate(typeof(Explosifs), kRpcRpcStartFuse, InvokeRpcRpcStartFuse);
		NetworkCRC.RegisterBehaviour("Explosifs", 0);
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
