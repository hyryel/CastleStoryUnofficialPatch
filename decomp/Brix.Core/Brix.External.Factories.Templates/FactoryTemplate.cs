using UnityEngine;
using UnityEngine.Networking;

namespace Brix.External.Factories.Templates;

public abstract class FactoryTemplate : MonoBehaviour, AssetKeyHolder
{
	public bool SpawnInactive;

	public Factory.AssetKey AssetKey
	{
		get
		{
			Component[] components = base.gameObject.GetComponents(typeof(AssetKeyHolder));
			Component[] array = components;
			for (int i = 0; i < array.Length; i++)
			{
				AssetKeyHolder assetKeyHolder = (AssetKeyHolder)array[i];
				if (!(assetKeyHolder is FactoryTemplate))
				{
					return assetKeyHolder.AssetKey;
				}
			}
			Transform parent = base.transform.parent;
			while (parent != null && parent.gameObject.GetComponent<Factory>() == null)
			{
				parent = parent.parent;
			}
			if (parent == null)
			{
				throw new FactoryException("it is completely illegal to ask a factory template for its asset key when it is not inside a factory, check your stuff");
			}
			return new Factory.AssetKey(parent.name, base.gameObject.name);
		}
		set
		{
			throw new FactoryException("assetkey of factory template is read-only");
		}
	}

	public NetworkHash128 AssetId => AssetKey.AssetId;

	public abstract void Preload();

	public abstract GameObject Instantiate(Vector3 position, Quaternion rotation);
}
