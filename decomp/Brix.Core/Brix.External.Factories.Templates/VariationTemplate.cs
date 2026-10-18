using System.Collections.Generic;
using UnityEngine;

namespace Brix.External.Factories.Templates;

public class VariationTemplate : FactoryTemplate
{
	private List<FactoryTemplate> Variations;

	public override void Preload()
	{
		Variations = new List<FactoryTemplate>();
		for (int i = 0; i < base.transform.childCount; i++)
		{
			FactoryTemplate component = base.transform.GetChild(i).GetComponent<FactoryTemplate>();
			if (component == null)
			{
				throw new FactoryException("VariationTemplate has an invalid child template (" + base.transform.GetChild(i).gameObject.name + "), please fix in factory");
			}
			Variations.Add(component);
		}
	}

	private FactoryTemplate GetRandomVariation()
	{
		if (Variations.Count == 0)
		{
			throw new FactoryException("VariationTemplate has no variations, must have at least one child template");
		}
		return Variations[Random.Range(0, Variations.Count)];
	}

	public override GameObject Instantiate(Vector3 position, Quaternion rotation)
	{
		return GetRandomVariation().Instantiate(position, rotation);
	}
}
