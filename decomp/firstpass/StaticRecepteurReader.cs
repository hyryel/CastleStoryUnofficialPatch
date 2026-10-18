using System;
using System.Collections.Generic;
using Brix.Game.AI;
using Brix.Game.Semantique;
using Brix.Game.Storage;
using Brix.Utils;
using MoonSharp.Interpreter;
using UnityEngine;

public class StaticRecepteurReader : MonoBehaviour
{
	public struct Registration
	{
		public int count;

		public RecepteurReading reading;

		public RecepteurReadingLua readingLua;

		public Registration(IRecepteur r)
		{
			count = 1;
			reading = RecepteurReading.Request(r);
			readingLua = RecepteurReadingLua.Request(r);
		}

		public void Inc()
		{
			count++;
		}

		public List<InventoryItemCount> Read()
		{
			reading.Read();
			return reading.content;
		}

		public DynValue ReadLua()
		{
			readingLua.Read();
			return readingLua.content;
		}

		public void Dec()
		{
			count--;
			if (count <= 0)
			{
				registations.Remove(reading.recepteur);
				reading.Dispose();
				readingLua.Dispose();
			}
		}
	}

	public class RecepteurReading : IPoolable, IResetable, IDisposable
	{
		public List<InventoryItemCount> content;

		public DynValue lua;

		public IRecepteur recepteur;

		public static RecepteurReading Request(IRecepteur r)
		{
			RecepteurReading recepteurReading = ObjectPool<RecepteurReading>.Reserve();
			recepteurReading.content = CollectionPool.RequestList<InventoryItemCount>();
			recepteurReading.recepteur = r;
			recepteurReading.Read();
			return recepteurReading;
		}

		public void Read()
		{
			if (content.Count == 0)
			{
				InventoryItemCount.FromDescriptions(ref content, Resources);
			}
			for (int i = 0; i < content.Count; i++)
			{
				ContentCapacityOf(recepteur, content[i].adjectif, out content[i].content, out content[i].capacity);
			}
		}

		private static void ContentCapacityOf(IRecepteur recepteur, Adjectif adjectif, out int content, out int capacity)
		{
			content = recepteur.ContentDescription.Value(adjectif);
			capacity = recepteur.BaseCapacity.Value(adjectif) - recepteur.ContentDescription.Value(adjectif);
		}

		public void Dispose()
		{
			content.Dispose();
			ObjectPool<RecepteurReading>.Release(this);
		}

		public void ResetObject()
		{
			recepteur = null;
			content = null;
		}
	}

	public class RecepteurReadingLua : IPoolable, IResetable, IDisposable
	{
		public DynValue content;

		private Dictionary<DynValue, Adjectif> map;

		public IRecepteur recepteur;

		private static DynValue _str_content = DynValue.NewString("content");

		private static DynValue _str_capacity = DynValue.NewString("capacity");

		public static RecepteurReadingLua Request(IRecepteur r)
		{
			RecepteurReadingLua recepteurReadingLua = ObjectPool<RecepteurReadingLua>.Reserve();
			recepteurReadingLua.map = CollectionPool.RequestDictionary<DynValue, Adjectif>();
			recepteurReadingLua.recepteur = r;
			recepteurReadingLua.content = DynValue.NewPrimeTable();
			recepteurReadingLua.Read();
			return recepteurReadingLua;
		}

		public void Read()
		{
			if (content.Table.Length == 0)
			{
				InventoryItemCount.FromDescriptions(ref content, ref map, Resources);
			}
			foreach (DynValue value in content.Table.Values)
			{
				ContentCapacityOf(recepteur, map[value], out var num, out var capacity);
				value.Table.Set(_str_content, DynValue.NewNumber(num));
				value.Table.Set(_str_capacity, DynValue.NewNumber(capacity));
			}
		}

		private static void ContentCapacityOf(IRecepteur recepteur, Adjectif adjectif, out int content, out int capacity)
		{
			content = recepteur.ContentDescription.Value(adjectif);
			capacity = recepteur.BaseCapacity.Value(adjectif) - recepteur.ContentDescription.Value(adjectif);
		}

		public void Dispose()
		{
			map.Dispose();
			ObjectPool<RecepteurReadingLua>.Release(this);
		}

		public void ResetObject()
		{
			recepteur = null;
			content = DynValue.Invalid;
		}
	}

	private static List<InventoryItemDescription> Resources = new List<InventoryItemDescription>();

	private static Dictionary<IRecepteur, Registration> registations = new Dictionary<IRecepteur, Registration>();

	[SerializeField]
	public List<Recepteur> debugRecepteurs;

	[ContextMenu("StressTest")]
	private void StressTest()
	{
		for (int i = 0; i < 10; i++)
		{
			for (int j = 0; j < debugRecepteurs.Count; j++)
			{
				Register(debugRecepteurs[j]);
			}
			for (int k = 0; k < debugRecepteurs.Count; k++)
			{
				for (int l = 0; l < 100; l++)
				{
					Get(debugRecepteurs[k]);
				}
			}
			for (int m = 0; m < debugRecepteurs.Count; m++)
			{
				for (int n = 0; n < 100; n++)
				{
					GetLua(debugRecepteurs[m]);
				}
			}
			for (int num = 0; num < debugRecepteurs.Count; num++)
			{
				Unregister(debugRecepteurs[num]);
			}
		}
	}

	public void Start()
	{
	}

	public static void Register(IRecepteur r)
	{
		if (Resources.Count == 0)
		{
			InventoryItemDescription.ItemDescriptionsFromAdjectiveType(ref Resources, AdjectiveInfo.AdjectiveType.Ressource);
		}
		if (registations.ContainsKey(r))
		{
			registations[r].Inc();
		}
		else
		{
			registations[r] = new Registration(r);
		}
	}

	public static List<InventoryItemCount> Get(IRecepteur r)
	{
		if (!registations.ContainsKey(r))
		{
			return null;
		}
		return registations[r].Read();
	}

	public static DynValue GetLua(IRecepteur r)
	{
		if (!registations.ContainsKey(r))
		{
			return DynValue.Invalid;
		}
		return registations[r].ReadLua();
	}

	public static void Unregister(IRecepteur r)
	{
		if (registations.ContainsKey(r))
		{
			registations[r].Dec();
		}
	}
}
