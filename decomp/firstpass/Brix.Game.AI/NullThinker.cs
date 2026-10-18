namespace Brix.Game.AI;

public class NullThinker : Thinker
{
	public new Thought PreviousThought => Thought.None;

	public override Thought CurrentThought
	{
		get
		{
			return Thought.None;
		}
		set
		{
		}
	}

	public new void StopThinking()
	{
	}

	public override void SetThoughtFor(float seconds, Thought newThought)
	{
	}

	public override void SetThought(Thought newThought, bool isTemporary = false)
	{
	}

	public override void ShowResolutionThought()
	{
	}

	public override bool HasThought()
	{
		return false;
	}

	protected override void BrixUpdate()
	{
		base.enabled = false;
	}
}
