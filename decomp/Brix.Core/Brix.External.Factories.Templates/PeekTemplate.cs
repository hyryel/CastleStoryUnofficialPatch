using UnityEngine;

namespace Brix.External.Factories.Templates;

public class PeekTemplate : FactoryTemplate
{
	public override void Preload()
	{
		base.gameObject.SetActive(value: false);
	}

	public override GameObject Instantiate(Vector3 position, Quaternion rotation)
	{
		throw new FactoryException($"AssetKey '{base.name}' is marked as PeekTemplate and cannot be used to spawn GameObjects");
	}
}
