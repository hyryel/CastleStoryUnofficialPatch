using System;
using UnityEngine;

namespace Brix.Audio;

public class SoundTest : MonoBehaviour
{
	public string son;

	private GameObject obj;

	private void Start()
	{
		obj = new GameObject(base.gameObject.name + " " + base.gameObject.GetInstanceID());
		obj.transform.position = base.transform.position;
		obj.transform.parent = base.transform;
		try
		{
			SoundEngine.listeSonSFX.Add(obj);
		}
		catch (Exception)
		{
		}
		SoundEngine.Play(son, obj.gameObject);
	}

	private void Update()
	{
	}

	private void OnApplicationQuit()
	{
		obj = null;
	}

	private void OnDestroy()
	{
		if (!(obj == null))
		{
			obj.transform.parent = GameObject.Find("SoundEngine").transform;
			SoundEngine.Play("Env_Sound_Stop", obj.gameObject);
			UnityEngine.Object.Destroy(obj, 0.1f);
		}
	}
}
