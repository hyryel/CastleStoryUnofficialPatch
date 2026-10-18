using Brix.External.Signals;
using UnityEngine;

public class AudioGameObjectTracker : MonoBehaviour
{
	public Transform tracked;

	public Signal onLostTrack = new Signal();

	private bool tracking;

	private int nPlaying;

	private float expiration;

	public void BeginSound()
	{
		nPlaying++;
	}

	public void EndSound()
	{
		nPlaying--;
	}

	private void Awake()
	{
		if (tracked != null)
		{
			base.name = tracked.name;
		}
	}

	private void LateUpdate()
	{
		if (tracked == null)
		{
			if (tracking)
			{
				if (expiration == 0f)
				{
					expiration = Time.time + 10f;
				}
				if (nPlaying == 0 || Time.time > expiration)
				{
					onLostTrack.Invoke();
					tracking = false;
					expiration = 0f;
					nPlaying = 0;
				}
			}
		}
		else
		{
			if (!tracking)
			{
				tracking = true;
				base.name = tracked.name;
			}
			base.transform.position = tracked.position;
			base.transform.rotation = tracked.rotation;
		}
	}
}
