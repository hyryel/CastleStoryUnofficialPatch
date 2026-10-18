using System;
using System.Collections.Generic;
using System.Text;
using Brix.Common.Collections;
using Brix.External.Factories;
using Brix.Game.AI;
using Brix.Game.Components;
using Brix.Game.Semantique;
using Brix.Utils;
using Brix.Utils.Collections;
using Brix.Utils.Logging;
using Smooth.Slinq;
using Smooth.Slinq.Context;
using UnityEngine;
using UnityEngine.Serialization;

[Serializable]
public class Description : IEquatable<Description>
{
	private static BrixLogger logger = new BrixLogger(typeof(Description).FullName);

	[SerializeField]
	[FormerlySerializedAs("dicoAdjectifs")]
	public DicoAdjectif _dicoAdjectifs;

	private static Description empty = new Description();

	private static Dictionary<AdjectiveKey, Factory.AssetKey> reps;

	public DicoAdjectif DicoAdjectif => _dicoAdjectifs;

	public static Description Empty => empty;

	public Slinq<Adjectif, SelectContext<Adjectif, KeyValuePair<Type, Adjectif>, DictionaryContext<Type, Adjectif>>> Adjectifs => from kv in _dicoAdjectifs.Slinq()
		select kv.Value;

	public Description()
	{
		_dicoAdjectifs = new DicoAdjectif();
	}

	public Description(DicoAdjectif dico)
	{
		_dicoAdjectifs = dico;
	}

	public Description(IEnumerable<Type> liste)
	{
		_dicoAdjectifs = new DicoAdjectif();
		IEnumerator<Type> enumerator = liste.GetEnumerator();
		while (enumerator.MoveNext())
		{
			_dicoAdjectifs.Add(enumerator.Current);
		}
	}

	public Description(IEnumerable<Adjectif> liste)
	{
		_dicoAdjectifs = new DicoAdjectif();
		IEnumerator<Adjectif> enumerator = liste.GetEnumerator();
		while (enumerator.MoveNext())
		{
			_dicoAdjectifs.Add(enumerator.Current);
		}
	}

	public Description(Adjectif adjectif)
	{
		_dicoAdjectifs = ((adjectif == null) ? new DicoAdjectif() : new DicoAdjectif { adjectif });
	}

	public Description Clone()
	{
		Description description = new Description();
		description.CopyFrom(this);
		return description;
	}

	public Description RemoveZeros()
	{
		List<Ressource> list = GetRessources().ToPooledList();
		foreach (Ressource item in list)
		{
			if (Value(item) <= 0)
			{
				Remove(item);
			}
		}
		list.Dispose();
		return this;
	}

	public Description RessourceOnly()
	{
		List<Adjectif> list = Adjectifs.Where((Adjectif a) => !(a is Ressource)).ToPooledList();
		foreach (Adjectif item in list)
		{
			Remove(item);
		}
		list.Dispose();
		return this;
	}

	public void RemoveAllExcept(Adjectif a)
	{
		List<Adjectif> list = (from kvp in DicoAdjectif.Slinq()
			select kvp.Value into adj
			where a.GetType().IsInstanceOfType(adj)
			select adj).ToPooledList();
		list.Slinq().ForEach(delegate(Adjectif adj, Description self)
		{
			self.Remove(adj);
		}, this);
		list.Dispose();
	}

	public Description CapAt(int max)
	{
		Description description = Clone();
		foreach (Adjectif item in description.Adjectifs.NoLeak())
		{
			if (item is Quantifiable)
			{
				item.quantifiable.valeur = Mathf.Min(item.quantifiable.valeur, max);
			}
		}
		return description;
	}

	public static Description operator &(Description a, Description b)
	{
		if (b == null)
		{
			return a;
		}
		if (a == null)
		{
			return b;
		}
		DicoAdjectif dicoAdjectif = new DicoAdjectif(b._dicoAdjectifs.Count);
		Dictionary<Type, Adjectif>.Enumerator enumerator = b._dicoAdjectifs.GetEnumerator();
		while (enumerator.MoveNext())
		{
			KeyValuePair<Type, Adjectif> current = enumerator.Current;
			Adjectif value = null;
			if (a._dicoAdjectifs.TryGetValue(current.Value.GetType(), out value))
			{
				dicoAdjectif.Add(value & current.Value);
			}
		}
		return new Description(dicoAdjectif);
	}

	public static Description operator +(Description a, Description b)
	{
		if (b == null)
		{
			return a;
		}
		if (a == null)
		{
			return b;
		}
		DicoAdjectif dicoAdjectif = new DicoAdjectif(a._dicoAdjectifs.Count);
		Dictionary<Type, Adjectif>.Enumerator enumerator = a._dicoAdjectifs.GetEnumerator();
		while (enumerator.MoveNext())
		{
			KeyValuePair<Type, Adjectif> current = enumerator.Current;
			Adjectif value = null;
			if (b._dicoAdjectifs.TryGetValue(current.Value.GetType(), out value))
			{
				dicoAdjectif.Add(current.Value + value);
			}
			else
			{
				dicoAdjectif.Add(current.Value);
			}
		}
		foreach (Adjectif item in b._dicoAdjectifs.Values.Slinq().Where((Adjectif adjB, Description _a) => !_a._dicoAdjectifs.ContainsKey(adjB.GetType()), a).NoLeak())
		{
			dicoAdjectif.Add(item);
		}
		return new Description(dicoAdjectif);
	}

	public Description ExceptWith(Description d)
	{
		foreach (Adjectif item in d.Adjectifs.NoLeak())
		{
			if (Has(item))
			{
				Remove(item);
			}
		}
		return this;
	}

	public static Description operator -(Description a, Description b)
	{
		if (b == null)
		{
			return a;
		}
		if (a == null)
		{
			logger.Info("Soustraction sur une description nulle. Il n'existe pas vraiment de description negative...");
			return null;
		}
		DicoAdjectif dicoAdjectif = new DicoAdjectif(a._dicoAdjectifs.Count);
		Dictionary<Type, Adjectif>.Enumerator enumerator = a._dicoAdjectifs.GetEnumerator();
		while (enumerator.MoveNext())
		{
			KeyValuePair<Type, Adjectif> current = enumerator.Current;
			Adjectif value = null;
			if (b._dicoAdjectifs.TryGetValue(current.Value.GetType(), out value))
			{
				dicoAdjectif.Add(current.Value - value);
			}
			else
			{
				dicoAdjectif.Add(current.Value);
			}
		}
		return new Description(dicoAdjectif);
	}

	public void UniqueRepresentatives(ref HashSet<Factory.AssetKey> keys)
	{
		(from rep in GetRessources().Select(RepresentativeOf)
			where rep != null
			select rep).AddTo(keys);
	}

	private static Dictionary<AdjectiveKey, Factory.AssetKey> InitReps()
	{
		Dictionary<AdjectiveKey, Factory.AssetKey> dictionary = new Dictionary<AdjectiveKey, Factory.AssetKey>();
		dictionary.Add(Adjectif.orangeCrystal.GetKey(), RawResources.RawOrangeCrystal);
		dictionary.Add(Adjectif.blueCrystal.GetKey(), RawResources.RawBlueCrystal);
		dictionary.Add(Adjectif.stones.GetKey(), RawResources.RawStone);
		dictionary.Add(Adjectif.plant.GetKey(), RawResources.RawFiber);
		dictionary.Add(Adjectif.rawIron.GetKey(), RawResources.RawIron);
		dictionary.Add(Adjectif.iron.GetKey(), BuildResources.IronIngot);
		dictionary.Add(Adjectif.clay.GetKey(), RawResources.RawClay);
		dictionary.Add(Adjectif.glass.GetKey(), BuildResources.GlassIngot);
		dictionary.Add(Adjectif.terracotta.GetKey(), BuildResources.Terracotta);
		dictionary.Add(Adjectif.purifiedBlueCrystal.GetKey(), BuildResources.PurifiedBrimstone);
		dictionary.Add(Adjectif.rope.GetKey(), BuildResources.Rope);
		dictionary.Add(Adjectif.fabric.GetKey(), BuildResources.Fabric);
		dictionary.Add(Adjectif.cog.GetKey(), BuildResources.Cog);
		dictionary.Add(Adjectif.bomb.GetKey(), BuildResources.Bomb);
		dictionary.Add(Adjectif.pumpkin.GetKey(), BuildResources.Pumpkin);
		dictionary.Add(Adjectif.crystalObjective.GetKey(), BuildResources.Crystal);
		dictionary.Add(Adjectif.bow.GetKey(), Accessories.Bow);
		dictionary.Add(Adjectif.quiver.GetKey(), Accessories.Quiver);
		dictionary.Add(Adjectif.cap.GetKey(), Accessories.Cap);
		dictionary.Add(Adjectif.sword.GetKey(), Accessories.Sword);
		dictionary.Add(Adjectif.helmet.GetKey(), Accessories.Helmet);
		dictionary.Add(Adjectif.shield.GetKey(), Accessories.Shield);
		dictionary.Add(Adjectif.crossbowWeapon.GetKey(), Accessories.Crossbow);
		dictionary.Add(Adjectif.crossbowProp.GetKey(), Accessories.Pouch);
		dictionary.Add(Adjectif.crossbowHat.GetKey(), Accessories.Kepi);
		dictionary.Add(Adjectif.alchemistPouch.GetKey(), Accessories.Backpack);
		dictionary.Add(Adjectif.alchemistHelmet.GetKey(), Accessories.Bonnet);
		dictionary.Add(Adjectif.geomancerWeapon.GetKey(), Accessories.Staff);
		dictionary.Add(Adjectif.geomancerProp.GetKey(), Accessories.Sachel);
		dictionary.Add(Adjectif.geomancerHat.GetKey(), Accessories.ArtificerHat);
		dictionary.Add(Adjectif.halbard.GetKey(), Accessories.Poleaxe);
		dictionary.Add(Adjectif.kettleHat.GetKey(), Accessories.KettleHat);
		dictionary.Add(Adjectif.plankBlock.GetKey(), Blocks.Wood_DoublePlank_AA);
		dictionary.Add(Adjectif.woodBlock.GetKey(), Blocks.Wood_DoubleLog_AA);
		dictionary.Add(Adjectif.stoneBlock.GetKey(), Blocks.Stone_DoubleBrick);
		dictionary.Add(Adjectif.sentinelWardAdj.GetKey(), ObjetsDynamiques.SentinelWard);
		dictionary.Add(Adjectif.healingWardAdj.GetKey(), ObjetsDynamiques.HealingWard);
		dictionary.Add(Adjectif.captureWardAdj.GetKey(), ObjetsDynamiques.CaptureWard);
		dictionary.Add(Adjectif.lanternWardAdj.GetKey(), ObjetsDynamiques.LanternWard);
		dictionary.Add(Adjectif.pylonWardAdj.GetKey(), ObjetsDynamiques.PylonGround);
		dictionary.Add(Adjectif.explosiveBarrel.GetKey(), ObjetsDynamiques.Baril);
		dictionary.Add(Adjectif.blueCrystalChunck.GetKey(), RawResources.BlueCrystalChunck);
		dictionary.Add(Adjectif.bearTrap.GetKey(), ObjetsDynamiques.BearTrap);
		return dictionary;
	}

	public static Factory.AssetKey RepresentativeOf(Ressource r)
	{
		if (reps == null)
		{
			reps = InitReps();
		}
		return reps.GetValueOrDefault(r.GetKey(), null);
	}

	public static Factory.AssetKey RepresentativeOf(IGameComponent gc)
	{
		if (gc.apparence == null || gc.apparence.Principale == null)
		{
			return null;
		}
		return RepresentativeOf(gc.apparence.Principale);
	}

	public void FilterByAdjectives(ref HashSet<Factory.AssetKey> keys, bool keepIfPresent)
	{
		HashSet<Factory.AssetKey> hashSet = CollectionPool.RequestHashSet<Factory.AssetKey>();
		foreach (Factory.AssetKey key in keys)
		{
			Apparence component = Factory.Peek(key).GetComponent<Apparence>();
			if (keepIfPresent == component.Adjectifs.Any((Adjectif a, Description self) => self.Has(a), this))
			{
				hashSet.Add(key);
			}
		}
		keys.Dispose();
		keys = hashSet;
	}

	public bool PeutPrendre(GameObject go)
	{
		return PeutPrendre(go.GetComponent<IDescriptor>());
	}

	public bool PeutPrendre(IDescriptor d)
	{
		return d != null && PeutPrendre(d.Description);
	}

	public bool PeutPrendre(Description autre)
	{
		if (autre.Has(Adjectif.structure) || autre.Has(Adjectif.projectAdj))
		{
			return false;
		}
		bool result = false;
		using Dictionary<Type, Adjectif>.Enumerator enumerator = _dicoAdjectifs.GetEnumerator();
		while (enumerator.MoveNext())
		{
			switch (enumerator.Current.Value.PeutPrendre(autre))
			{
			case ReponseContrainte.Echec:
				return false;
			case ReponseContrainte.Positif:
				result = true;
				break;
			case ReponseContrainte.Succes:
				return true;
			}
		}
		return result;
	}

	public bool PeutPrendre(Apparence autre)
	{
		return PeutPrendre(autre.description);
	}

	public bool PeutPrendre(Factory.AssetKey key)
	{
		return PeutPrendre(Factory.Peek(key).GetComponent<Apparence>());
	}

	public bool PeutPrendre(GameComponent autre)
	{
		Apparence matchingDescription = autre.GetMatchingDescription(FonctionDeContenant.entrepot, (Apparence a, Description self) => self.PeutPrendre(a), this);
		return matchingDescription != null;
	}

	public bool FitDans(Description autre)
	{
		return autre.PeutPrendre(this);
	}

	public bool NoneOrZeros()
	{
		bool result = true;
		foreach (KeyValuePair<Type, Adjectif> dicoAdjectif in _dicoAdjectifs)
		{
			if (dicoAdjectif.Value is Quantifiable && dicoAdjectif.Value.quantifiable.valeur != 0)
			{
				result = false;
				break;
			}
		}
		return result;
	}

	public bool Equals(Description other)
	{
		if (_dicoAdjectifs == other._dicoAdjectifs)
		{
			return true;
		}
		if (_dicoAdjectifs == null || other._dicoAdjectifs == null)
		{
			return false;
		}
		foreach (Type key in _dicoAdjectifs.Keys)
		{
			if (other._dicoAdjectifs.TryGetValue(key, out var value) && value == _dicoAdjectifs[key])
			{
				continue;
			}
			return false;
		}
		return true;
	}

	public void Add(Adjectif adjectif)
	{
		if (adjectif != null)
		{
			if (!_dicoAdjectifs.ContainsKey(adjectif.GetType()))
			{
				_dicoAdjectifs.Add(adjectif);
			}
			else if (adjectif.quantifiable != null)
			{
				_dicoAdjectifs[adjectif.GetType()].quantifiable.valeur += adjectif.quantifiable.valeur;
			}
		}
	}

	public void AddWith(Adjectif newAdj)
	{
		if (_dicoAdjectifs.TryGetValue(newAdj.GetType(), out var value))
		{
			value.AddTo(newAdj);
		}
		else
		{
			_dicoAdjectifs.Add(newAdj.GetType(), newAdj.Clone());
		}
	}

	public void AddAllFrom(Description description)
	{
		(from kvp in description.DicoAdjectif.Slinq()
			where kvp.Value.GetKey().GetInfo().transferable
			select kvp).ForEach(delegate(KeyValuePair<Type, Adjectif> kvp, Description modifiedDesc)
		{
			modifiedDesc.AddWith(kvp.Value);
		}, this);
	}

	public void Add<T>(Description description) where T : Adjectif
	{
		foreach (KeyValuePair<Type, Adjectif> item in description.DicoAdjectif)
		{
			if (item.Value is T)
			{
				AddWith(item.Value);
			}
		}
	}

	public void Increment<T>(Description description) where T : Adjectif
	{
		foreach (KeyValuePair<Type, Adjectif> item in description.DicoAdjectif)
		{
			if (item.Value is T && Has(item.Value))
			{
				AddWith(item.Value);
			}
		}
	}

	public void Remove<T>()
	{
		_dicoAdjectifs.Remove(typeof(T));
	}

	public void Remove(Type type)
	{
		_dicoAdjectifs.Remove(type);
	}

	public void Remove(Adjectif adjectif)
	{
		_dicoAdjectifs.Remove(adjectif.GetType());
	}

	public Adjectif Get(Type adjectif)
	{
		Adjectif value = null;
		_dicoAdjectifs.TryGetValue(adjectif, out value);
		return value;
	}

	public Adjectif GetFirstWithBase(Type baseType)
	{
		foreach (Adjectif value in _dicoAdjectifs.Values)
		{
			for (Type type = value.GetType(); type != typeof(Adjectif); type = type.BaseType)
			{
				if (type == baseType)
				{
					return value;
				}
			}
		}
		return null;
	}

	public Adjectif Get(Adjectif adjectif)
	{
		return Get(adjectif.GetType());
	}

	public T Get<T>(T adjectif) where T : Adjectif
	{
		Adjectif value = null;
		_dicoAdjectifs.TryGetValue(adjectif.GetType(), out value);
		return value as T;
	}

	public T Get<T>() where T : Adjectif
	{
		Adjectif value = null;
		_dicoAdjectifs.TryGetValue(typeof(T), out value);
		return value as T;
	}

	public void Set(Quantifiable q, int qty)
	{
		if (_dicoAdjectifs.TryGetValue(q.GetType(), out var value))
		{
			value.quantifiable.valeur = qty;
		}
	}

	public int Value(Adjectif adjectif)
	{
		Adjectif adj = Get(adjectif.GetType());
		return _LocalValue(adj);
	}

	public int Value(Type adjectif)
	{
		Adjectif adj = Get(adjectif);
		return _LocalValue(adj);
	}

	public int Value<T>() where T : Quantite
	{
		T adj = Get<T>();
		return _LocalValue(adj);
	}

	private static int _LocalValue(Adjectif adj)
	{
		if (adj == null)
		{
			return 0;
		}
		Quantifiable quantifiable = adj.quantifiable;
		if (quantifiable == null)
		{
			throw new Exception("You are asking the value of an adjectif that is not Quantifiable. There might be a wrong use of the Adjectif mecanics");
		}
		return quantifiable.valeur;
	}

	public bool Has(Adjectif adjectif)
	{
		return Get(adjectif) != null;
	}

	public bool Has(Type adjectif)
	{
		return Get(adjectif) != null;
	}

	public bool Has<T>() where T : Adjectif
	{
		return Get<T>() != null;
	}

	public bool HasPresenceOrValue(Adjectif adjectif)
	{
		return HasPresenceOrValue(adjectif.GetType());
	}

	public bool HasPresenceOrValue(Type adjectif)
	{
		return Get(adjectif)?.HasPresenceOrValue() ?? false;
	}

	public bool HasPresenceOrValue<T>() where T : Quantite
	{
		T val = Get<T>();
		return val?.HasPresenceOrValue() ?? false;
	}

	public bool HasRoom()
	{
		Adjectif adjectif = Get(typeof(Encombrement));
		if (adjectif != null)
		{
			return adjectif.quantifiable.valeur != 0;
		}
		return GetRessources().Any((Ressource value) => value.quantifiable.valeur > 0);
	}

	public Slinq<Ressource, PredicateContext<Ressource, SelectContext<Ressource, KeyValuePair<Type, Adjectif>, DictionaryContext<Type, Adjectif>>>> GetRessources()
	{
		return from kvp in _dicoAdjectifs.Slinq()
			select kvp.Value as Ressource into r
			where r != null && r.GetKey() != Adjectif.outil.GetKey()
			select r;
	}

	public IEnumerable<Adjectif> GetUIRelevant()
	{
		Slinq<KeyValuePair<Type, Adjectif>, PredicateContext<KeyValuePair<Type, Adjectif>, DictionaryContext<Type, Adjectif>>> slinq = from kvp in _dicoAdjectifs.Slinq()
			where kvp.Value.GetKey().GetInfo().UIRelevant
			select kvp;
		while (slinq.current.isSome)
		{
			yield return slinq.current.value.Value;
			slinq.Skip();
		}
	}

	public void Clear()
	{
		_dicoAdjectifs.Clear();
	}

	public bool isPreloaded()
	{
		return _dicoAdjectifs.Preloaded;
	}

	public void IntersectWith(Description description)
	{
		List<Adjectif> list = (from kvp in _dicoAdjectifs.Slinq()
			select kvp.Value).Where((Adjectif adj, Description desc) => !desc._dicoAdjectifs.ContainsKey(adj.GetType()), description).ToPooledList();
		list.Slinq().ForEach(delegate(Adjectif adj, Description self)
		{
			SetToZero(adj);
		}, this);
		list.Dispose();
	}

	public void Subtract(Description description)
	{
		(from kvp in description._dicoAdjectifs.Slinq()
			select kvp.Value as Quantite into adj
			where adj != null
			select adj).ForEach(delegate(Quantite adj, Description self)
		{
			self.Subtract(adj);
		}, this);
	}

	private void Subtract(Quantite qToRemove)
	{
		if (_dicoAdjectifs.TryGetValue(qToRemove.GetType(), out var value))
		{
			(value as Quantite).valeur -= qToRemove.valeur;
		}
	}

	public void ConvertTo(Description description)
	{
		IntersectWith(description);
		(from kvp in description._dicoAdjectifs.Slinq()
			select kvp.Value).ForEach(delegate(Adjectif adj, Description self)
		{
			self.Set(adj);
		}, this);
	}

	private void Set(Adjectif adjReference)
	{
		if (!_dicoAdjectifs.TryGetValue(adjReference.GetType(), out var value))
		{
			value = adjReference.Clone();
			_dicoAdjectifs.Add(adjReference.GetType(), value);
		}
		value.Set(adjReference);
	}

	public void ExceptWith(DicoAdjectif dico)
	{
		(from kvp in dico.Slinq()
			select kvp.Value).Where((Adjectif adj, Description self) => self._dicoAdjectifs.ContainsKey(adj.GetType()), this).ForEach(delegate(Adjectif adj, Description self)
		{
			self.SetToZero(adj);
		}, this);
	}

	private void SetToZero(Adjectif q)
	{
		if (q is Limite)
		{
			Remove(q);
		}
		else if (q is Quantifiable)
		{
			(Get(q) as Quantifiable).valeur = 0;
		}
	}

	public bool Any()
	{
		return DicoAdjectif.Any();
	}

	public void CopyQuantifiablesFrom(Description originalDescription)
	{
		foreach (KeyValuePair<Type, Adjectif> item in originalDescription.DicoAdjectif)
		{
			if (item.Value is Quantifiable quantifiable)
			{
				if (DicoAdjectif.TryGetValue(item.Key, out var value))
				{
					(value as Quantifiable).valeur = quantifiable.valeur;
				}
				else
				{
					DicoAdjectif[item.Key] = item.Value.Clone();
				}
			}
		}
		foreach (KeyValuePair<Type, Adjectif> item2 in DicoAdjectif)
		{
			if (item2.Value is Quantifiable quantifiable2 && !originalDescription.DicoAdjectif.ContainsKey(item2.Key))
			{
				quantifiable2.valeur = 0;
			}
		}
	}

	public void CopyFrom(Description originalDescription)
	{
		DicoAdjectif.Clear();
		foreach (KeyValuePair<Type, Adjectif> item in originalDescription.DicoAdjectif)
		{
			DicoAdjectif.Add(item.Value.Clone());
		}
	}

	public string GetPrintableDescription()
	{
		StringBuilder stringBuilder = new StringBuilder();
		foreach (KeyValuePair<Type, Adjectif> dicoAdjectif in _dicoAdjectifs)
		{
			Adjectif value = dicoAdjectif.Value;
			stringBuilder.Append(value.GetType());
			if (value.quantifiable != null)
			{
				stringBuilder.Append(" -> ").Append(value.quantifiable.valeur);
			}
			stringBuilder.Append("\n");
		}
		return stringBuilder.ToString();
	}
}
