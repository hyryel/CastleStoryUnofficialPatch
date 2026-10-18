using Brix.Game.Components;
using Brix.Game.Rules;
using UnityEngine;

namespace Brix.Game;

public static class Affiliation
{
	public enum Relation
	{
		None = 0,
		Allied = 1,
		Neutral = 2,
		Enemy = 4,
		Undefined = 8,
		All = 15
	}

	public static Relation GetRelation(GameObject a, GameObject b)
	{
		return GetRelation(Faction.GetFaction(a), Faction.GetFaction(b));
	}

	public static Relation GetRelation(Faction a, GameObject b)
	{
		return GetRelation(a, Faction.GetFaction(b));
	}

	public static Relation GetRelation(Faction a, Faction b)
	{
		if (a == null || b == null)
		{
			return Relation.Undefined;
		}
		Relation relation = a.GetRelation(b);
		Relation relation2 = b.GetRelation(a);
		if (relation != Relation.Undefined || relation2 != Relation.Undefined)
		{
			if (relation == Relation.Enemy || relation2 == Relation.Enemy)
			{
				return Relation.Enemy;
			}
			if (relation == Relation.Allied && relation2 == Relation.Allied)
			{
				return Relation.Allied;
			}
			if (relation == Relation.Neutral && relation2 == Relation.Neutral)
			{
				return Relation.Neutral;
			}
		}
		if (a == b)
		{
			return Relation.Allied;
		}
		if (a == Faction.DefaultNeutral || b == Faction.DefaultNeutral)
		{
			return Relation.Neutral;
		}
		if (a == Faction.DefaultEnemy || b == Faction.DefaultEnemy)
		{
			return Relation.Enemy;
		}
		return GeneralRules.Main.PlayerRelations;
	}

	public static bool IsAllied(GameObject a, GameObject b)
	{
		return GetRelation(a, b) == Relation.Allied;
	}

	public static bool IsAllied(Faction f, GameObject go)
	{
		Relation relation = GetRelation(f, go);
		return relation == Relation.Allied;
	}

	public static bool IsAllied(Faction f, Faction other)
	{
		return GetRelation(f, other) == Relation.Allied;
	}

	public static bool IsAllied(Faction f)
	{
		return GetRelation(f, Faction.DefaultAllied) == Relation.Allied;
	}

	public static bool IsAlliedOrNeutral(GameObject a, GameObject b)
	{
		Relation relation = GetRelation(a, b);
		return relation == Relation.Allied || relation == Relation.Neutral;
	}

	public static bool IsAlliedOrNeutral(Faction faction, GameObject go)
	{
		Relation relation = GetRelation(faction, go);
		return relation == Relation.Allied || relation == Relation.Neutral;
	}

	public static bool IsAlliedOrNeutral(Faction faction, Faction other)
	{
		Relation relation = GetRelation(faction, other);
		return relation == Relation.Allied || relation == Relation.Neutral;
	}

	public static bool IsNeutral(GameObject a, GameObject b)
	{
		return GetRelation(a, b) == Relation.Neutral;
	}

	public static bool IsNeutral(Faction a, Faction b)
	{
		return GetRelation(a, b) == Relation.Neutral;
	}

	public static bool IsEnemy(GameObject a, GameObject b)
	{
		return GetRelation(a, b) == Relation.Enemy;
	}

	public static bool IsEnemy(Faction a, Faction b)
	{
		return GetRelation(a, b) == Relation.Enemy;
	}

	public static bool IsEnemyOrNeutral(GameObject a, GameObject b)
	{
		Relation relation = GetRelation(a, b);
		return relation == Relation.Enemy || relation == Relation.Neutral;
	}

	public static bool BelongsToMe(GameObject a)
	{
		AffiliationComponent component = a.GetComponent<AffiliationComponent>();
		if (component == null || component.faction == null || User.LocalUser == null)
		{
			return false;
		}
		return component.faction.OwnerSyncList.Contains(User.LocalUser.gameObject);
	}

	public static bool IsOwnedBy(GameObject obj, GameObject player)
	{
		AffiliationComponent component = obj.GetComponent<AffiliationComponent>();
		if ((bool)component)
		{
			return component.faction.OwnerSyncList.Contains(player);
		}
		return false;
	}

	public static bool IsOwnedBy(GameObject obj, User player)
	{
		return IsOwnedBy(obj, player.gameObject);
	}

	public static bool IsSameOrNeutral(GameObject obj, GameObject player)
	{
		AffiliationComponent component = player.GetComponent<AffiliationComponent>();
		if (component == null)
		{
			return false;
		}
		AffiliationComponent component2 = obj.GetComponent<AffiliationComponent>();
		if ((bool)component2)
		{
			return component2.faction == component.faction || component2.faction == Faction.DefaultNeutral;
		}
		return true;
	}

	public static bool IsSameOrNeutral(Faction obj, Faction player)
	{
		return obj == player || obj == Faction.DefaultNeutral;
	}

	public static bool BelongsToEnnemy(GameObject a)
	{
		AffiliationComponent component = a.GetComponent<AffiliationComponent>();
		if (component == null || component.faction == null)
		{
			return false;
		}
		return IsEnemy(User.LocalUser.DefaultAlliedFactionGO, a);
	}

	public static bool BelongsToAllied(GameObject a)
	{
		AffiliationComponent component = a.GetComponent<AffiliationComponent>();
		if (component == null || component.faction == null)
		{
			return false;
		}
		return IsAllied(User.LocalUser.DefaultAlliedFactionGO, a);
	}
}
