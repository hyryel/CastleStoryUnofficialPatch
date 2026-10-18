using UnityEngine;

namespace Brix.Game.AI;

public class AIError
{
	public int priority;

	public Thought thought;

	private float _peramption;

	public float Peramption
	{
		get
		{
			return _peramption - Time.time;
		}
		set
		{
			_peramption = Time.time + value;
		}
	}

	public AIError(int priority, Thought thought)
	{
		this.priority = priority;
		this.thought = thought;
	}

	public AIError Copy()
	{
		return new AIError(priority, thought);
	}

	public override string ToString()
	{
		return thought.GetLabel();
	}
}
