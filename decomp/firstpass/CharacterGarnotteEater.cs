using Brix.Game;
using Brix.Game.AI;
using UnityEngine;

public class CharacterGarnotteEater : GarnotteEater
{
	[SerializeField]
	private Labor labor;

	public override void Eat(float seconds = -1f)
	{
		if (labor.Profession.IsWorker())
		{
			base.Eat(seconds);
		}
	}
}
