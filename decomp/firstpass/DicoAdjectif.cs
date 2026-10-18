using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Brix.Game.AI;
using Brix.IO.Serialization;
using Brix.Utils;
using Brix.Utils.Logging;
using Newtonsoft.Json;
using Smooth.Slinq;
using Smooth.Slinq.Context;
using UnityEngine;

[Serializable]
public class DicoAdjectif : IDictionary<Type, Adjectif>, ISerializationCallbackReceiver, ICollection<KeyValuePair<Type, Adjectif>>, IEnumerable<KeyValuePair<Type, Adjectif>>, IEnumerable
{
	private Dictionary<Type, Adjectif> _dictionary = new Dictionary<Type, Adjectif>();

	private static BrixLogger logger;

	public static Dictionary<string, DicoAdjectif> deserializeCache;

	private static JsonSerializer serializer;

	private bool _preloaded;

	[HideInInspector]
	[SerializeField]
	private string adjectives;

	[HideInInspector]
	[SerializeField]
	private string md5;

	[HideInInspector]
	private StringWriter stringWriter = new StringWriter();

	public bool Preloaded
	{
		get
		{
			return _preloaded;
		}
		set
		{
			_preloaded = value;
		}
	}

	public int Count => _dictionary.Count;

	public bool IsReadOnly => ((ICollection<KeyValuePair<Type, Adjectif>>)_dictionary).IsReadOnly;

	public Adjectif this[Type key]
	{
		get
		{
			return _dictionary[key];
		}
		set
		{
			_dictionary[key] = value;
		}
	}

	public ICollection<Type> Keys => _dictionary.Keys;

	public ICollection<Adjectif> Values => _dictionary.Values;

	static DicoAdjectif()
	{
		logger = new BrixLogger(typeof(DicoAdjectif).FullName);
		deserializeCache = new Dictionary<string, DicoAdjectif>();
		JsonSerializerSettings unitySerializerSettings = Io.GetUnitySerializerSettings("save");
		unitySerializerSettings.PreserveReferencesHandling = PreserveReferencesHandling.None;
		serializer = JsonSerializer.Create(unitySerializerSettings);
	}

	public DicoAdjectif()
	{
		_dictionary = new Dictionary<Type, Adjectif>();
	}

	public DicoAdjectif(int capacity)
	{
		_dictionary = new Dictionary<Type, Adjectif>(capacity);
	}

	public static implicit operator Dictionary<Type, Adjectif>(DicoAdjectif dico)
	{
		return dico._dictionary;
	}

	public bool Any()
	{
		return _dictionary.Count > 0;
	}

	public void OnBeforeSerialize()
	{
		lock (this)
		{
			if (!_preloaded)
			{
				stringWriter.GetStringBuilder().Length = 0;
				serializer.Serialize(stringWriter, this.Select((KeyValuePair<Type, Adjectif> kvp) => kvp.Value));
				adjectives = stringWriter.GetStringBuilder().ToString();
				md5 = Common.Md5Sum(adjectives);
			}
		}
	}

	public void OnAfterDeserialize()
	{
		lock (this)
		{
			if (md5 != null && deserializeCache.ContainsKey(md5))
			{
				DicoAdjectif dicoAdjectif = deserializeCache[md5];
				DuplicateFrom(dicoAdjectif);
				_preloaded = dicoAdjectif._preloaded;
				return;
			}
			DicoAdjectif x = new DicoAdjectif();
			List<Adjectif> list = serializer.Deserialize<List<Adjectif>>(new JsonTextReader(new StringReader(adjectives)));
			DuplicateFrom(list);
			list.Slinq().ForEach(delegate(Adjectif adj)
			{
				x.Add(adj.GetType(), adj);
			});
			if (string.IsNullOrEmpty(md5))
			{
				md5 = Common.Md5Sum(adjectives);
			}
			x.md5 = md5;
			x.adjectives = adjectives;
			CacheForDeserialization(x);
		}
	}

	public void Add(Type type)
	{
		Add(type, Adjectif.New(type));
	}

	public void Add(Adjectif adjectif)
	{
		if (adjectif != null)
		{
			Add(adjectif.GetType(), adjectif);
		}
	}

	public bool Contains(Adjectif adjectif)
	{
		return adjectif != null && ContainsKey(adjectif.GetType());
	}

	public bool Remove(Adjectif adjectif)
	{
		return adjectif != null && Remove(adjectif.GetType());
	}

	private void DuplicateFrom(Dictionary<Type, Adjectif> preDeserializedDico)
	{
		Clear();
		preDeserializedDico.Slinq().ForEach(delegate(KeyValuePair<Type, Adjectif> pair, DicoAdjectif self)
		{
			self.Add(pair.Key, pair.Value.Clone());
		}, this);
	}

	private void DuplicateFrom(List<Adjectif> preDeserializedDico)
	{
		Clear();
		preDeserializedDico.Slinq().ForEach(delegate(Adjectif adj, DicoAdjectif self)
		{
			self.Add(adj.GetType(), adj.Clone());
		}, this);
	}

	public static void CacheForDeserialization(DicoAdjectif dico)
	{
		CacheForDeserialization(dico, dico.md5, dico.adjectives);
	}

	public static void CacheForDeserialization(DicoAdjectif dico, string md5, string adjective)
	{
		if (!deserializeCache.ContainsKey(md5))
		{
			deserializeCache.Add(md5, dico);
			return;
		}
		DicoAdjectif dicoAdjectif = deserializeCache[md5];
		if (dicoAdjectif != dico)
		{
			logger.Info("DicoAdjectif.CacheDeserialization(): A collision happened... identifying if it's an issue");
			if (dicoAdjectif.adjectives != adjective)
			{
				logger.Error("DicoAdjectif.CacheDeserialization(): CacheDeserialization has an md5 collision with the following json:\nCached:\n" + dicoAdjectif.adjectives + "\n\nNew:\n" + adjective);
				return;
			}
			logger.Info("DicoAdjectif.CacheDeserialization(): False positive, they have the same STRING");
			deserializeCache[md5] = dico;
		}
	}

	public Dictionary<Type, Adjectif>.Enumerator GetEnumerator()
	{
		return _dictionary.GetEnumerator();
	}

	IEnumerator<KeyValuePair<Type, Adjectif>> IEnumerable<KeyValuePair<Type, Adjectif>>.GetEnumerator()
	{
		return _dictionary.GetEnumerator();
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return ((IEnumerable)_dictionary).GetEnumerator();
	}

	public void Add(KeyValuePair<Type, Adjectif> item)
	{
		_dictionary.Add(item.Key, item.Value);
	}

	public void Clear()
	{
		_dictionary.Clear();
	}

	public bool Contains(KeyValuePair<Type, Adjectif> item)
	{
		return _dictionary.ContainsKey(item.Key);
	}

	public void CopyTo(KeyValuePair<Type, Adjectif>[] array, int arrayIndex)
	{
		((ICollection<KeyValuePair<Type, Adjectif>>)_dictionary).CopyTo(array, arrayIndex);
	}

	public bool Remove(KeyValuePair<Type, Adjectif> item)
	{
		return _dictionary.Remove(item.Key);
	}

	public void Add(Type key, Adjectif value)
	{
		_dictionary[key] = value;
	}

	public bool ContainsKey(Type key)
	{
		return _dictionary.ContainsKey(key);
	}

	public bool Remove(Type key)
	{
		return _dictionary.Remove(key);
	}

	public bool TryGetValue(Type key, out Adjectif value)
	{
		return _dictionary.TryGetValue(key, out value);
	}

	public Slinq<KeyValuePair<Type, Adjectif>, DictionaryContext<Type, Adjectif>> Slinq()
	{
		return _dictionary.Slinq();
	}
}
