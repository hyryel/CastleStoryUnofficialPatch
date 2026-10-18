using UnityEngine;

namespace Brix.External.Factories.Templates;

public class CloneTemplate : FactoryTemplate
{
	public override void Preload()
	{
		base.gameObject.SetActive(value: false);
	}

	public override GameObject Instantiate(Vector3 position, Quaternion rotation)
	{
		return Object.Instantiate(base.gameObject, position, rotation);
	}
}
