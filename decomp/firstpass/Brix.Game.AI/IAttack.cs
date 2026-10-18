using Brix.Game.AI.Damage;
using UnityEngine;

namespace Brix.Game.AI;

public interface IAttack
{
	float DamageValue { get; }

	GameObject Attacker { get; }

	void Apply(IDamageReceiver receiver);
}
