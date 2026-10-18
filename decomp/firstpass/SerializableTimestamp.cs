using System.Runtime.Serialization;
using Newtonsoft.Json;
using UnityEngine;

[JsonObject(MemberSerialization.OptIn)]
public class SerializableTimestamp
{
	[JsonProperty]
	private float _age;

	private float _dateOfBirthAccordingToLocalTime;

	public float Timestamp
	{
		get
		{
			return _dateOfBirthAccordingToLocalTime;
		}
		set
		{
			_dateOfBirthAccordingToLocalTime = value;
		}
	}

	public float Delta => Time.time - Timestamp;

	public float Age
	{
		get
		{
			return _age;
		}
		set
		{
			_age = value;
			InitTimestamp();
		}
	}

	public SerializableTimestamp()
	{
		_dateOfBirthAccordingToLocalTime = Time.time;
	}

	[OnSerializing]
	internal void OnSerializing(StreamingContext context)
	{
		_age = Delta;
	}

	[OnDeserialized]
	internal void OnDeserialized(StreamingContext context)
	{
		InitTimestamp(Time.time);
	}

	public void InitTimestamp()
	{
		InitTimestamp(Time.time);
	}

	public void InitTimestamp(float newLocalTime)
	{
		_dateOfBirthAccordingToLocalTime = newLocalTime - _age;
	}

	public void Reset()
	{
		_age = 0f;
		_dateOfBirthAccordingToLocalTime = Time.time;
	}
}
