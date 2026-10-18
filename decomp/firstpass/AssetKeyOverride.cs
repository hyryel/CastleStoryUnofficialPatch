using Brix.External.Factories;
using UnityEngine;

public class AssetKeyOverride : MonoBehaviour
{
	[SerializeField]
	private Factory.AssetKey keyOverride;

	public Factory.AssetKey KeyOverride => keyOverride;
}
