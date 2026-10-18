using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Brix.External.Factories;
using Brix.Game.Components;
using UnityEngine;

public class DicoAdjectifPostProcessor : MonoBehaviour, IFactoryPostProcessor
{
	private BindingFlags _bf = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

	private HashSet<object> objects = new HashSet<object>();

	private void Awake()
	{
		DicoAdjectif.deserializeCache.Clear();
	}

	private void OnDestroy()
	{
		DicoAdjectif.deserializeCache = new Dictionary<string, DicoAdjectif>();
	}

	public void PostProcess(Factory f)
	{
		List<GameComponent> source = GetComponentsInChildrenWithDisabled<GameComponent>(f.gameObject).ToList();
		if (source.Any((GameComponent g) => !g))
		{
			throw new FactoryException(string.Empty);
		}
		IEnumerable<DicoAdjectif> enumerable = source.SelectMany(AllDicoAdjectifIn);
		foreach (DicoAdjectif item in enumerable)
		{
			Preload(item);
		}
	}

	private List<DicoAdjectif> AllDicoAdjectifIn(object parentField)
	{
		List<DicoAdjectif> list = new List<DicoAdjectif>();
		if (!objects.Contains(parentField) && !parentField.GetType().IsValueType)
		{
			objects.Add(parentField);
			foreach (object item in from fieldInfo in parentField.GetType().GetFields(_bf)
				select fieldInfo.GetValue(parentField) into fieldValue
				where fieldValue != null && !fieldValue.GetType().IsValueType
				select fieldValue)
			{
				if (item.GetType() == typeof(DicoAdjectif))
				{
					list.Add((DicoAdjectif)item);
				}
				else
				{
					list.AddRange(AllDicoAdjectifIn(item));
				}
			}
			objects.Remove(parentField);
		}
		return list;
	}

	public static List<TType> GetComponentsInChildrenWithDisabled<TType>(GameObject objRoot) where TType : Component
	{
		List<TType> list = objRoot.GetComponents<TType>().ToList();
		foreach (Transform item in objRoot.transform)
		{
			list.AddRange(GetComponentsInChildrenWithDisabled<TType>(item.gameObject));
		}
		return list;
	}

	[ContextMenu("Preload Factories")]
	private void ForcePreload()
	{
		foreach (Factory factory in Factory.GetFactories())
		{
			PostProcess(factory);
		}
	}

	private static void Preload(DicoAdjectif dicoAdjectif)
	{
		dicoAdjectif.OnBeforeSerialize();
		DicoAdjectif.CacheForDeserialization(dicoAdjectif);
		dicoAdjectif.Preloaded = true;
	}
}
