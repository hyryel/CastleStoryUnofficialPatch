using UnityEngine;

namespace Brix.Game.Storage;

public class StorageLocator : MonoBehaviour
{
	[SerializeField]
	public GameObject ReferencePoint;

	public CompoundRecepteur Storage;
}
