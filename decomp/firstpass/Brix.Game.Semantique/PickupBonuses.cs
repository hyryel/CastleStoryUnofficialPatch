using System.Collections.Generic;
using Brix.Game.AI;
using Smooth.Slinq;
using UnityEngine;

namespace Brix.Game.Semantique;

public class PickupBonuses : MonoBehaviour
{
	public struct Weights
	{
		public float Presence;

		public float Ratio;

		public static Weights Default = new Weights(5f, 0f);

		public Weights(float presence, float ratio)
		{
			Presence = presence;
			Ratio = ratio;
		}

		public float Apply(bool presence, float ratio)
		{
			return ((!presence) ? 0f : Presence) + ratio * Ratio;
		}
	}

	private struct Builder
	{
		private Ressource[] closed;

		public Builder(params Ressource[] r)
		{
			closed = r;
		}

		public Builder Pickup(float presence, float ratio)
		{
			Weights w = new Weights(presence, ratio);
			closed.Slinq().ForEach(delegate(Ressource r)
			{
				pickupWeights[r] = w;
			});
			return this;
		}

		public Builder Store(float presence, float ratio)
		{
			Weights w = new Weights(presence, ratio);
			closed.Slinq().ForEach(delegate(Ressource r)
			{
				storeWeights[r] = w;
			});
			return this;
		}
	}

	private static Dictionary<Ressource, Weights> pickupWeights = new Dictionary<Ressource, Weights>();

	private static Dictionary<Ressource, Weights> storeWeights = new Dictionary<Ressource, Weights>();

	public static float ForStoring(Ressource adj, bool present, float ratio)
	{
		return storeWeights.GetValueOrDefault(adj, Weights.Default).Apply(present, ratio);
	}

	public static float ForPickingUp(Ressource adj, bool present, float ratio)
	{
		return pickupWeights.GetValueOrDefault(adj, Weights.Default).Apply(present, ratio);
	}

	private void Init()
	{
		For(Adjectif.stones, Adjectif.woodBlock).Pickup(0f, 30f).Store(10f, 30f);
		For(Adjectif.rawIron, Adjectif.plant, Adjectif.orangeCrystal, Adjectif.iron, Adjectif.stoneBlock, Adjectif.plankBlock, Adjectif.glass, Adjectif.cog, Adjectif.fabric, Adjectif.rope).Pickup(0f, 30f).Store(10f, 0f);
		For(ToolRack.AllItems.ToArray()).Pickup(15f, 20f).Store(15f, 20f);
	}

	private void Awake()
	{
		Init();
	}

	private void OnDestroy()
	{
		pickupWeights.Clear();
		storeWeights.Clear();
	}

	private static Builder For(params Ressource[] r)
	{
		return new Builder(r);
	}
}
