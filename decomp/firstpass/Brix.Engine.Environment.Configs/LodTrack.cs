using System;
using Newtonsoft.Json;

namespace Brix.Engine.Environment.Configs;

[Serializable]
[JsonObject(MemberSerialization.OptIn)]
public class LodTrack
{
	[NonSerialized]
	public LodTransition[] Transitions;

	[NonSerialized]
	public LodRange[] Ranges;

	public int LodCount => Transitions.Length + 1;

	public int LodMax => Transitions.Length;

	protected LodTrack()
	{
	}

	protected void Init(params LodTransition[] transitions)
	{
		Transitions = transitions;
	}

	public virtual void Refresh()
	{
		Ranges = new LodRange[Transitions.Length + 1];
		for (int i = 0; i < Ranges.Length; i++)
		{
			Ranges[i] = new LodRange((i > 0) ? (Transitions[i - 1].Start - Transitions[i - 1].Padding) : 0, (i >= Transitions.Length) ? 10000 : Transitions[i].Start);
			Ranges[i].LodTrack = this;
			if (i > 0)
			{
				Ranges[i].Finer = Ranges[i - 1];
			}
			if (i < LodMax)
			{
				Ranges[i].Coarser = Ranges[i + 1];
			}
		}
	}

	public LodRange GetLodRange(int lod)
	{
		return (lod >= 0 && lod <= LodMax && Ranges != null) ? Ranges[lod] : LodRange.Empty;
	}
}
