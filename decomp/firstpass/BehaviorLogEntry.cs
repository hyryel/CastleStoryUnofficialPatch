using UnityEngine;

public class BehaviorLogEntry : MonoBehaviour
{
	public enum State
	{
		Destroyed,
		Created,
		Started,
		Ticked,
		Looped,
		Completed,
		Failed,
		Cancelled
	}

	public string path = string.Empty;

	public string groupTypeLabel = string.Empty;

	public string typeLabel = string.Empty;

	public string label = string.Empty;

	public string signatureLabel = string.Empty;

	public float isRun;

	public float stateIdx;

	public State state
	{
		get
		{
			return (State)stateIdx;
		}
		set
		{
			stateIdx = (float)value;
		}
	}

	private void Start()
	{
	}

	private void Update()
	{
	}
}
