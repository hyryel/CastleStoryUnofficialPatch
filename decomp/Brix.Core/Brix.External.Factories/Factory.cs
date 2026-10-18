using System;
using System.Collections.Generic;
using System.Linq;
using Brix.External.Factories.Templates;
using Brix.External.Signals;
using Brix.Utils;
using Smooth.Slinq;
using Smooth.Slinq.Context;
using UnityEngine;
using UnityEngine.Networking;

namespace Brix.External.Factories;

[ExecuteInEditMode]
public class Factory : MonoBehaviour
{
	[Serializable]
	public class AssetKey : IEquatable<AssetKey>
	{
		private static Dictionary<AssetKey, NetworkHash128> assetKeyToAssetId = new Dictionary<AssetKey, NetworkHash128>();

		private static Dictionary<NetworkHash128, AssetKey> assetIdToAssetKey = new Dictionary<NetworkHash128, AssetKey>();

		public string Factory;

		public string Name;

		public NetworkHash128 AssetId
		{
			get
			{
				if (!assetKeyToAssetId.TryGetValue(this, out var value))
				{
					value = NetworkHash128.Parse(StringUtils.CalculateMD5Hash(Factory + Name));
					assetKeyToAssetId.Add(this, value);
					assetIdToAssetKey.Add(value, this);
				}
				assetKeyToAssetId.TryGetValue(this, out value);
				return value;
			}
		}

		public IEnumerable<AssetKey> Siblings => GetFactory(Factory).AssetKeys;

		public IEnumerable<AssetKey> SiblingsExcludingSelf
		{
			get
			{
				foreach (AssetKey key in GetFactory(Factory).AssetKeys)
				{
					if (key.Name != Name)
					{
						yield return key;
					}
				}
			}
		}

		public bool Equals(AssetKey other)
		{
			if ((object)other == null)
			{
				return false;
			}
			if ((object)this == other)
			{
				return true;
			}
			return string.Equals(Factory, other.Factory) && string.Equals(Name, other.Name);
		}

		public override int GetHashCode()
		{
			return (((Factory != null) ? Factory.GetHashCode() : 0) * 397) ^ ((Name != null) ? Name.GetHashCode() : 0);
		}

		public AssetKey(string factory, string name)
		{
			Factory = factory;
			Name = name;
		}

		public AssetKey(string assetkeyString)
			: this(assetkeyString.Split('.').FirstOrDefault(), assetkeyString.Split('.').LastOrDefault())
		{
		}

		public override string ToString()
		{
			return Factory + "." + Name;
		}

		public static bool IsNullOrEmpty(AssetKey key)
		{
			return key == null || (key.Factory == "" && key.Name == "");
		}

		public override bool Equals(object obj)
		{
			return this == obj as AssetKey;
		}

		public static bool operator ==(AssetKey key1, AssetKey key2)
		{
			return key1?.Equals(key2) ?? ((object)key2 == null);
		}

		public static bool operator !=(AssetKey key1, AssetKey key2)
		{
			return !(key1 == key2);
		}

		public static AssetKey AssetKeyFromId(NetworkHash128 assetId)
		{
			assetIdToAssetKey.TryGetValue(assetId, out var value);
			return value;
		}

		public bool IsEmpty()
		{
			return string.IsNullOrEmpty(Name) && string.IsNullOrEmpty(Factory);
		}
	}

	private static Dictionary<string, List<Factory>> MultiFactories = new Dictionary<string, List<Factory>>();

	private static Dictionary<string, Factory> Factories = new Dictionary<string, Factory>();

	public static string BaseAssetKeyPath = "Assets/Standard Assets/Brix/External/Factories/";

	public static Signal<Factory> FactoryCreatedSignal = new Signal<Factory>();

	public static Signal<Factory> FactoryModifiedSignal = new Signal<Factory>();

	public static Signal<Factory> FactoryOverrideCompleted = new Signal<Factory>();

	public string InstantiatedObjectDefaultPath = "/Jeu/UnclassedObjects";

	private Dictionary<string, FactoryTemplate> cachedTemplates = new Dictionary<string, FactoryTemplate>();

	private List<FactoryTemplate> _templates = new List<FactoryTemplate>();

	public static Signal<AssetKey, GameObject> GameObjectCreatedSignal = new Signal<AssetKey, GameObject>();

	private Transform _defaultParent;

	public List<FactoryTemplate> Templates
	{
		get
		{
			CacheTemplates();
			return _templates;
		}
	}

	public IEnumerable<AssetKey> AssetKeys => Templates.Select((FactoryTemplate t) => t.AssetKey);

	public FactoryTemplate GetTemplate(string assetName)
	{
		if (!cachedTemplates.TryGetValue(assetName, out var value) || value == null)
		{
			throw new ArgumentException($"Invalid assetKey '{base.gameObject.name}.{assetName}'. The referenced FactoryTemplate was not found");
		}
		return value;
	}

	public FactoryTemplate GetTemplate(AssetKey key)
	{
		if (!cachedTemplates.TryGetValue(key.Name, out var value) || value == null)
		{
			throw new ArgumentException($"Invalid Template name '{base.gameObject.name}'. The referenced FactoryTemplate was not found (AssetKey='{key.ToString()}')");
		}
		return value;
	}

	public GameObject GetOrCreateAsset(string assetName)
	{
		return GetOrCreateAsset<CloneTemplate>(assetName);
	}

	public GameObject GetOrCreateAsset<T>(string assetName) where T : FactoryTemplate
	{
		if (cachedTemplates.TryGetValue(assetName, out var value))
		{
			return value.gameObject;
		}
		Transform transform = base.transform.OfType<Transform>().FirstOrDefault((Transform child) => child.name == assetName);
		if (transform == null)
		{
			return CreateAsset<T>(assetName);
		}
		return transform.gameObject;
	}

	private GameObject CreateAsset<T>(string assetName) where T : FactoryTemplate
	{
		GameObject gameObject = new GameObject(assetName);
		gameObject.SetActive(value: false);
		T template = gameObject.AddComponent<T>();
		gameObject.transform.SetParent(base.transform);
		PreloadTemplate(template);
		ResetTemplateCache();
		return gameObject;
	}

	public AssetKey AddAsset<T>(GameObject asset) where T : FactoryTemplate
	{
		if (cachedTemplates.ContainsKey(asset.name))
		{
			throw new FactoryException($"An asset with name '{asset.name}' already exists in factory '{base.name}'");
		}
		asset.SetActive(value: false);
		T orAddComponent = asset.GetOrAddComponent<T>();
		asset.transform.SetParent(base.transform);
		PreloadTemplate(orAddComponent);
		ResetTemplateCache();
		return new AssetKey(base.name, asset.name);
	}

	[ContextMenu("Invalidate FactoryTemplate Cache")]
	public void ResetTemplateCache()
	{
		_templates.Clear();
		cachedTemplates.Clear();
		CacheTemplates();
	}

	public void AddMissingFactoryTemplates()
	{
		foreach (Transform item in base.transform)
		{
			if (item.GetComponent<FactoryTemplate>() == null)
			{
				item.gameObject.AddComponent<CloneTemplate>();
			}
		}
		ResetTemplateCache();
	}

	protected virtual void Awake()
	{
		CheckForInvalidNesting();
		Preload();
		if (!Factories.TryGetValue(base.name, out var value))
		{
			value = this;
			RegisterFactory();
		}
		else if (Application.isPlaying)
		{
			MoveTemplateToFactory(value);
			UnityEngine.Object.DestroyImmediate(base.gameObject);
		}
		FactoryModifiedSignal.Invoke(value);
	}

	private void CheckForInvalidNesting()
	{
		if (GetComponentsInParent<Factory>().Slinq().Any((Factory f, Factory self) => f != self, this))
		{
			throw new FactoryException("Error loading factory '{0}'. Factory nesting is not supported. Factory loading aborted");
		}
	}

	private void RegisterFactory()
	{
		Factories.Add(base.name, this);
		CacheTemplates();
		FactoryCreatedSignal.Invoke(this);
	}

	protected virtual void Preload()
	{
		if (Application.isPlaying)
		{
			Templates.Slinq().ForEach(delegate(FactoryTemplate t, Factory self)
			{
				self.PreloadTemplate(t);
			}, this);
			GetComponents<IFactoryPostProcessor>().Slinq().ForEach(delegate(IFactoryPostProcessor pp, Factory self)
			{
				pp.PostProcess(this);
			}, this);
		}
		MultiFactories.GetValueOrCreate(base.name).Add(this);
	}

	private void PreloadTemplate(FactoryTemplate template)
	{
		AssetKey assetKey = template.AssetKey;
		template.Preload();
		template.GetOrAddComponent<FactoryImprint>().AssetKey = assetKey;
	}

	private void MoveTemplateToFactory(Factory existingFactory)
	{
		foreach (FactoryTemplate item in Templates.Where((FactoryTemplate t) => t.transform.parent == base.transform))
		{
			existingFactory.AddOrUpdateTemplate(item);
		}
		existingFactory.ResetTemplateCache();
	}

	private void AddOrUpdateTemplate(FactoryTemplate newTemplate)
	{
		List<FactoryTemplate> list = (from t in Templates.Slinq()
			where t.name == newTemplate.name
			select t).ToList();
		if (list.Count > 1)
		{
			throw new FactoryException("Two Factory Template with the name '" + newTemplate.name + "' were found in the Factory. Only one can have the name.");
		}
		FactoryTemplate factoryTemplate = list.Slinq().FirstOrDefault();
		if (factoryTemplate != null)
		{
			DestroyTemplate(factoryTemplate);
		}
		newTemplate.transform.SetParent(base.transform);
	}

	private void DestroyTemplate(FactoryTemplate oldTemplate)
	{
		_templates.Remove(oldTemplate);
		UnityEngine.Object.DestroyImmediate(oldTemplate.gameObject);
	}

	private void CacheTemplates()
	{
		if (_templates.Count == 0)
		{
			GetComponentsInChildren<FactoryTemplate>(includeInactive: true).Slinq().ForEach(delegate(FactoryTemplate t, Factory self)
			{
				AddTemplateToCache(t);
			}, this);
		}
	}

	private void AddTemplateToCache(FactoryTemplate template)
	{
		_templates.Add(template);
		try
		{
			cachedTemplates.Add(template.gameObject.name, template);
		}
		catch (ArgumentException ex)
		{
			throw new FactoryException(string.Format("Loading of Factory '{0}' failed because of a multiple assets sharing the name '{1}'\n" + ex.Message, base.name, template.name));
		}
	}

	protected void OnDestroy()
	{
		if (Factories.TryGetValue(base.name, out var value) && value == this)
		{
			Factories.Remove(base.name);
		}
		else
		{
			FactoryOverrideCompleted.Invoke(this);
		}
		MultiFactories.GetValueOrCreate(base.name).Remove(this);
	}

	protected virtual T Instantiate<T>(AssetKey asset, Vector3 position, Quaternion rotation, Func<FactoryTemplate, bool> IsSpawnedActive) where T : Component
	{
		FactoryTemplate template = GetTemplate(asset);
		GameObject gameObject = template.Instantiate(position, rotation);
		T component = gameObject.GetComponent<T>();
		if (component == null)
		{
			UnityEngine.Object.Destroy(gameObject);
			throw new FactoryException($"Instantiation request failed for GameObject with assetKey '{asset}'. Requested component '{typeof(T).Name}' could not be found.");
		}
		OnGameObjectInstantiated(asset, IsSpawnedActive(template), gameObject);
		return component;
	}

	private GameObject Instantiate(AssetKey asset, Vector3 position, Quaternion rotation, Func<FactoryTemplate, bool> IsSpawnedActive)
	{
		FactoryTemplate template = GetTemplate(asset);
		GameObject gameObject = template.Instantiate(position, rotation);
		OnGameObjectInstantiated(asset, IsSpawnedActive(template), gameObject);
		return gameObject;
	}

	private void OnGameObjectInstantiated(AssetKey asset, bool IsSpawnedActive, GameObject newGO)
	{
		ReparentToDefault(newGO.transform);
		InitializeGO(newGO);
		newGO.SetActive(IsSpawnedActive);
		GameObjectCreatedSignal.Invoke(asset, newGO);
	}

	private void InitializeGO(GameObject newGo)
	{
		Array.ForEach(newGo.GetComponents<IInitializable>(), delegate(IInitializable component)
		{
			component.Initialize();
		});
	}

	private void ReparentToDefault(Transform newObject)
	{
		if (_defaultParent == null || _defaultParent.gameObject.FullPath() != InstantiatedObjectDefaultPath)
		{
			_defaultParent = GameObjectResolver.FindOrCreateGameObject(InstantiatedObjectDefaultPath);
		}
		newObject.SetParent(_defaultParent);
	}

	private static Func<FactoryTemplate, bool> GetIsSpawnedActiveDelegate(bool spawnedActive)
	{
		return spawnedActive ? ((Func<FactoryTemplate, bool>)((FactoryTemplate t) => true)) : ((Func<FactoryTemplate, bool>)((FactoryTemplate t) => false));
	}

	public static IEnumerable<Factory> GetFactories()
	{
		Dictionary<string, Factory>.Enumerator e = Factories.GetEnumerator();
		while (e.MoveNext())
		{
			yield return e.Current.Value;
		}
	}

	public static IEnumerable<Factory> GetFactories_Editor()
	{
		Slinq<Factory, SelectSlinqContext<Factory, IListContext<Factory>, KeyValuePair<string, List<Factory>>, DictionaryContext<string, List<Factory>>>> slinq = MultiFactories.Slinq().SelectMany((KeyValuePair<string, List<Factory>> kv) => kv.Value.Slinq());
		while (slinq.current.isSome)
		{
			yield return slinq.current.value;
			slinq.Skip();
		}
	}

	public static Factory GetFactory(string name)
	{
		if (!Factories.TryGetValue(name, out var value) || value == null)
		{
			throw new FactoryException($"Invalid Factory name '{name}'. The referenced Factory was not found");
		}
		return value;
	}

	public static Factory GetFactory(AssetKey key)
	{
		if (!Factories.TryGetValue(key.Factory, out var value) || value == null)
		{
			throw new FactoryException($"Invalid Factory name '{key.Factory}'. The referenced Factory was not found (AssetKey='{key}')");
		}
		return value;
	}

	public static IEnumerable<Factory> GetFactory_Editor(AssetKey key)
	{
		IEnumerable<Factory> valueOrDefault = MultiFactories.GetValueOrDefault(key.Factory);
		return valueOrDefault ?? Enumerable.Empty<Factory>();
	}

	public static FactoryTemplate TryPeek_Editor(AssetKey ak)
	{
		if (MultiFactories.TryGetValue(ak.Factory, out var value))
		{
			foreach (Factory item in value)
			{
				if (item.cachedTemplates.TryGetValue(ak.Name, out var value2))
				{
					return value2;
				}
			}
		}
		return null;
	}

	public static Factory GetOrCreateFactory(string factoryName)
	{
		if (!Factories.TryGetValue(factoryName, out var value))
		{
			GameObject gameObject = new GameObject(factoryName);
			return gameObject.AddComponent<Factory>();
		}
		return value;
	}

	public static bool Exist(AssetKey key)
	{
		Factory value;
		return Factories.TryGetValue(key.Factory, out value) && value.cachedTemplates.ContainsKey(key.Name);
	}

	public static GameObject Peek(AssetKey key)
	{
		return GetFactory(key.Factory).GetTemplate(key.Name).gameObject;
	}

	public static GameObject Instantiate(AssetKey asset)
	{
		return Instantiate(asset, Vector3.zero, Quaternion.identity);
	}

	public static GameObject Instantiate(AssetKey asset, bool active)
	{
		return Instantiate(asset, Vector3.zero, Quaternion.identity, active);
	}

	public static GameObject Instantiate(AssetKey asset, Vector3 position, Quaternion rotation)
	{
		Factory factory = GetFactory(asset);
		return factory.Instantiate(asset, position, rotation, (FactoryTemplate t) => !t.SpawnInactive);
	}

	public static GameObject Instantiate(AssetKey asset, Vector3 position, Quaternion rotation, bool active)
	{
		Factory factory = GetFactory(asset);
		return factory.Instantiate(asset, position, rotation, GetIsSpawnedActiveDelegate(active));
	}

	public static T Instantiate<T>(AssetKey asset) where T : Component
	{
		return Instantiate<T>(asset, Vector3.zero, Quaternion.identity);
	}

	public static T Instantiate<T>(AssetKey asset, bool active) where T : Component
	{
		return Instantiate<T>(asset, Vector3.zero, Quaternion.identity, active);
	}

	public static T Instantiate<T>(AssetKey asset, Vector3 position, Quaternion rotation) where T : Component
	{
		Factory factory = GetFactory(asset);
		return factory.Instantiate<T>(asset, position, rotation, (FactoryTemplate t) => !t.SpawnInactive);
	}

	public static T Instantiate<T>(AssetKey asset, Vector3 position, Quaternion rotation, bool active) where T : Component
	{
		Factory factory = GetFactory(asset);
		return factory.Instantiate<T>(asset, position, rotation, GetIsSpawnedActiveDelegate(active));
	}

	public T _Instantiate<T>(AssetKey key, Vector3 position, Quaternion rotation) where T : Component
	{
		return Instantiate<T>(key, position, rotation, (FactoryTemplate t) => !t.SpawnInactive);
	}

	public GameObject _Instantiate(AssetKey key, Vector3 position, Quaternion rotation)
	{
		return Instantiate(key, position, rotation, (FactoryTemplate t) => !t.SpawnInactive);
	}
}
