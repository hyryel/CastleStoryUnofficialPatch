using System.Collections.Generic;
using UnityEngine;

namespace Brix.Game.AI;

public class TargetMotion
{
	public class DataPoint
	{
		public float time;

		public Vector3 position;

		public Vector3 offset;

		public DataPoint(float time, Vector3 position, Vector3 offset)
		{
			this.time = time;
			this.position = position;
			this.offset = offset;
		}
	}

	private bool dataFull;

	private int maxData;

	private DataPoint[] motionData;

	private Location target;

	private int currentIndex;

	private float lastTracking;

	private bool computed;

	private Vector3 avgMotionPerSecond;

	private Vector3 avgOffset;

	private float avgDistanceDeltaPerSecond;

	private GameObject prediction;

	public TargetMotion(int _maxData)
	{
		Init(_maxData);
	}

	public void Init(int _maxData)
	{
		dataFull = false;
		maxData = _maxData;
		motionData = new DataPoint[maxData];
		currentIndex = -1;
		lastTracking = 0f;
		computed = false;
		target = null;
	}

	private int CurrentIndex(int inc = 0)
	{
		return (currentIndex + maxData + inc) % maxData;
	}

	public void Follow(Location target)
	{
		Reset();
		this.target = target;
	}

	public void Follow(GameObject target)
	{
		Follow(Location.Of(target));
	}

	public void Reset()
	{
		Init(maxData);
	}

	public void Track()
	{
		Track(target.RangeHitPosition, Time.time);
	}

	public void Track(float time)
	{
		Track(target.RangeHitPosition, time);
	}

	public void Track(Vector3 positionWithOffset, float time)
	{
		Vector3 position = target.Position;
		Vector3 offset = positionWithOffset - position;
		DataPoint dataPoint = motionData[CurrentIndex()];
		if (dataPoint == null || time > dataPoint.time)
		{
			currentIndex++;
			if (currentIndex == maxData - 1)
			{
				dataFull = true;
			}
			if (currentIndex == maxData)
			{
				currentIndex = 0;
			}
		}
		computed = false;
		motionData[CurrentIndex()] = new DataPoint(time, position, offset);
	}

	public Vector3 Predict(float timeToHit, int rank = -2)
	{
		if (target is CharacterLocation)
		{
			return (target as CharacterLocation).Character.navigation.Locomotion.GetProjectedPosition(timeToHit);
		}
		if (!IsPredictable())
		{
			throw new InvalidStateException();
		}
		if (!computed)
		{
			ComputeMotion();
		}
		Vector3 position = motionData[CurrentIndex()].position;
		if ((bool)prediction)
		{
			Object.Destroy(prediction);
		}
		prediction = CreateTestMarker(position + avgMotionPerSecond * timeToHit, Color.white, rank);
		return position + avgMotionPerSecond * timeToHit + avgOffset;
	}

	private void CreateTestLine(Vector3 start, Vector3 end)
	{
		GameObject gameObject = new GameObject();
		LineRenderer lineRenderer = gameObject.AddComponent<LineRenderer>();
		lineRenderer.startColor = Color.black;
		lineRenderer.endColor = Color.black;
		lineRenderer.startWidth = 0.1f;
		lineRenderer.endWidth = 0.1f;
		lineRenderer.SetPosition(0, start);
		lineRenderer.SetPosition(1, end);
	}

	private GameObject CreateTestMarker(Vector3 position, Color color, int rank = -2)
	{
		GameObject gameObject;
		if (rank == -1)
		{
			gameObject = GameObject.CreatePrimitive(PrimitiveType.Sphere);
			gameObject.transform.localScale = Vector3.one * 0.5f;
			gameObject.name = "final prediction";
		}
		else
		{
			gameObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
			if (rank == -2)
			{
				gameObject.transform.localScale = Vector3.one * 0.2f;
				gameObject.GetComponent<Renderer>().material.color = color;
				gameObject.name = "observation";
			}
			else
			{
				gameObject.transform.localScale = Vector3.one * 0.1f;
				gameObject.GetComponent<Renderer>().material.color = new Color(0.2f * (float)rank, 0.2f * (float)rank, 0.2f * (float)rank);
				gameObject.name = "prediction no " + rank;
			}
		}
		gameObject.transform.position = position;
		return gameObject;
	}

	public bool IsPredictable()
	{
		return dataFull;
	}

	private void ComputeMotion()
	{
		Vector3 zero = Vector3.zero;
		Vector3 zero2 = Vector3.zero;
		float num = 0f;
		for (int i = 0; i < maxData - 1; i++)
		{
			float num2 = motionData[CurrentIndex(i + 2)].time - motionData[CurrentIndex(i + 1)].time;
			Vector3 vector = motionData[CurrentIndex(i + 2)].position - motionData[CurrentIndex(i + 1)].position;
			Vector3 vector2 = (motionData[CurrentIndex(i + 2)].offset + motionData[CurrentIndex(i + 1)].offset) / 2f;
			num += num2;
			zero += vector;
			zero2 += vector2 * num2;
		}
		avgMotionPerSecond = ((!(num > 0f)) ? Vector3.zero : (zero / num));
		avgOffset = ((!(num > 0f)) ? motionData[CurrentIndex()].offset : (zero2 / num));
		computed = true;
	}

	public Dictionary<string, string> CheckState()
	{
		Dictionary<string, string> dictionary = new Dictionary<string, string>();
		dictionary.Add("dataFull", dataFull.ToString());
		dictionary.Add("maxData", maxData.ToString());
		dictionary.Add("currentIndex", currentIndex.ToString());
		dictionary.Add("lastTracking", lastTracking.ToString());
		dictionary.Add("computed", computed.ToString());
		return dictionary;
	}

	public DataPoint CheckDataAt(int i)
	{
		return motionData[i];
	}
}
