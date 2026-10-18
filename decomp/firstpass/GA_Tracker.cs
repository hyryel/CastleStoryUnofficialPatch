using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

[ExecuteInEditMode]
public class GA_Tracker : MonoBehaviour
{
	public enum GAEventType
	{
		BreadCrumb,
		Start,
		OnDestroy,
		OnLevelWasLoaded,
		OnTriggerEnter,
		OnCollisionEnter,
		OnControllerColliderHit
	}

	public static Dictionary<GAEventType, string> EventTooltips = new Dictionary<GAEventType, string>
	{
		{
			GAEventType.BreadCrumb,
			"Send an event every time interval. Good for generating heatmaps of player position/movement in your levels."
		},
		{
			GAEventType.Start,
			"Send an event when the Start method is run. Use this for tracking spawning of the object"
		},
		{
			GAEventType.OnDestroy,
			"Send an event when the OnDestroy method is run. Use this for tracking \"death\" of the object."
		},
		{
			GAEventType.OnLevelWasLoaded,
			"Send an event when the OnLevelWasLoaded method is run. Use this for tracking when a new level is loaded."
		},
		{
			GAEventType.OnTriggerEnter,
			"Send an event when the OnTriggerEnter method is run. Use this for tracking when something (f.x. the player) enters a trigger area."
		},
		{
			GAEventType.OnCollisionEnter,
			"Send an event when the OnCollisionEnter method is run. Use this for tracking when objects collide."
		},
		{
			GAEventType.OnControllerColliderHit,
			"Send an event when the OnControllerColliderHit method is run. Use this for tracking when a controller hits a collider while performing a Move."
		}
	};

	[SerializeField]
	public List<GAEventType> TrackedEvents = new List<GAEventType>();

	public bool TrackedEventsFoldOut;

	public bool ShowGizmo = true;

	public float BreadCrumbTrackInterval = 1f;

	private float _lastBreadCrumbTrackTime;

	private void Start()
	{
		if (Application.isPlaying)
		{
			if (TrackedEvents.Contains(GAEventType.Start))
			{
				GA.API.Design.NewEvent("Start:" + base.gameObject.name);
			}
			SceneManager.sceneLoaded += LevelLoaded;
		}
	}

	private void Update()
	{
		if (Application.isPlaying && TrackedEvents.Contains(GAEventType.BreadCrumb) && Time.time > _lastBreadCrumbTrackTime + BreadCrumbTrackInterval)
		{
			_lastBreadCrumbTrackTime = Time.time;
			GA.API.Design.NewEvent("BreadCrumb:" + base.gameObject.name);
		}
	}

	private void OnDestroy()
	{
		if (Application.isPlaying && TrackedEvents.Contains(GAEventType.OnDestroy))
		{
			GA.API.Design.NewEvent("OnDestroy:" + base.gameObject.name);
		}
	}

	public void LevelLoaded(Scene scene, LoadSceneMode mode)
	{
		if (Application.isPlaying && TrackedEvents.Contains(GAEventType.OnLevelWasLoaded))
		{
			GA.API.Design.NewEvent("OnLevelWasLoaded:" + base.gameObject.name);
		}
	}

	public void OnTriggerEnter()
	{
		if (Application.isPlaying && TrackedEvents.Contains(GAEventType.OnTriggerEnter))
		{
			GA.API.Design.NewEvent("OnTriggerEnter:" + base.gameObject.name);
		}
	}

	public void OnCollisionEnter()
	{
		if (Application.isPlaying && TrackedEvents.Contains(GAEventType.OnCollisionEnter))
		{
			GA.API.Design.NewEvent("OnCollisionEnter:" + base.gameObject.name);
		}
	}

	public void OnControllerColliderHit()
	{
		if (Application.isPlaying && TrackedEvents.Contains(GAEventType.OnControllerColliderHit))
		{
			GA.API.Design.NewEvent("OnControllerColliderHit:" + base.gameObject.name);
		}
	}

	public Array GetEventValues()
	{
		return Enum.GetValues(typeof(GAEventType));
	}

	private void OnDrawGizmos()
	{
		if (ShowGizmo)
		{
			Gizmos.DrawIcon(base.transform.position, "gaLogo", allowScaling: true);
		}
	}
}
