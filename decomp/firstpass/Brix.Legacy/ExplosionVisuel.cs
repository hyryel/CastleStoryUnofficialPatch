using System.Collections.Generic;
using UnityEngine;

namespace Brix.Legacy;

public class ExplosionVisuel : MonoBehaviour
{
	private Dictionary<string, ParticleSystem> liste;

	private void Awake()
	{
		liste = GetSystems();
	}

	public Dictionary<string, ParticleSystem> GetSystems()
	{
		Dictionary<string, ParticleSystem> dictionary = new Dictionary<string, ParticleSystem>();
		ParticleSystem[] componentsInChildren = GetComponentsInChildren<ParticleSystem>();
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			dictionary.Add(componentsInChildren[i].gameObject.name, componentsInChildren[i]);
		}
		return dictionary;
	}

	public ParticleSystem Get(string s)
	{
		return liste[s];
	}

	public ParticleSystem[] GetAll()
	{
		ParticleSystem[] array = new ParticleSystem[liste.Count];
		liste.Values.CopyTo(array, 0);
		return array;
	}
}
