using Brix.Lifecycle.Pooling;
using Brix.Utils;
using Smooth.Slinq;
using UnityEngine;

namespace Brix.Game.AI;

public class Transferable : MonoBehaviour
{
	public enum DropBehaviour
	{
		Delete,
		TransferToCorpseAndDrop,
		TransferToCorpseAndDelete,
		TransferToCorpseAndTimeDelete,
		Drop,
		DropAndTimeDelete
	}

	private static Transform _accessoriesObject;

	public DropBehaviour dropBehaviour = DropBehaviour.TransferToCorpseAndDelete;

	public float _timeBeforeeDestruction = 10f;

	private static Transform accessoriesObject
	{
		get
		{
			if (_accessoriesObject == null)
			{
				_accessoriesObject = GameObjectResolver.FindOrCreateGameObject("Jeu/Accessories");
			}
			return _accessoriesObject;
		}
	}

	public static void TransferOnTransform(Transform from, Transform to)
	{
		Transferable[] componentsInChildren = from.GetComponentsInChildren<Transferable>();
		for (int i = 0; i != componentsInChildren.Length; i++)
		{
			componentsInChildren[i].TransferToTransform(to);
		}
	}

	public static void TransferOutOfDyingTransform(Transform corpse)
	{
		Transferable[] componentsInChildren = corpse.GetComponentsInChildren<Transferable>();
		for (int i = 0; i != componentsInChildren.Length; i++)
		{
			componentsInChildren[i].TransferOutOfDyingTransform();
		}
	}

	private void TransferToTransform(Transform target)
	{
		Tool component = GetComponent<Tool>();
		if (component != null && component.Magical)
		{
			if (dropBehaviour == DropBehaviour.TransferToCorpseAndDrop)
			{
				dropBehaviour = DropBehaviour.TransferToCorpseAndTimeDelete;
			}
			else if (dropBehaviour == DropBehaviour.Drop)
			{
				dropBehaviour = DropBehaviour.DropAndTimeDelete;
			}
		}
		if (dropBehaviour == DropBehaviour.TransferToCorpseAndDelete || dropBehaviour == DropBehaviour.TransferToCorpseAndDrop || dropBehaviour == DropBehaviour.TransferToCorpseAndTimeDelete)
		{
			if (component != null)
			{
				component.OnDropBehaviour = Tool.OnDropBehaviours.Transfer;
			}
			Transform parent = target.GetComponentsInChildren<Transform>(includeInactive: true).Slinq().Where((Transform c, string t) => c.name == t, base.transform.parent.name)
				.FirstOrDefault();
			base.transform.SetParent(parent);
			if (dropBehaviour == DropBehaviour.TransferToCorpseAndTimeDelete)
			{
				base.gameObject.AddComponent<TimedDestruction>().TimeBeforeDestruction = _timeBeforeeDestruction;
			}
		}
		else if (dropBehaviour == DropBehaviour.Drop || dropBehaviour == DropBehaviour.DropAndTimeDelete)
		{
			Deparent();
			if (dropBehaviour == DropBehaviour.DropAndTimeDelete)
			{
				base.gameObject.AddComponent<TimedDestruction>().TimeBeforeDestruction = _timeBeforeeDestruction;
			}
		}
		else if (dropBehaviour == DropBehaviour.Delete)
		{
			ObjectPoolSingleton.Release(base.gameObject);
		}
	}

	private void TransferOutOfDyingTransform()
	{
		if (dropBehaviour == DropBehaviour.TransferToCorpseAndDelete)
		{
			ObjectPoolSingleton.Release(base.gameObject);
		}
		else if (dropBehaviour == DropBehaviour.TransferToCorpseAndDrop || dropBehaviour == DropBehaviour.TransferToCorpseAndTimeDelete)
		{
			Deparent();
		}
	}

	private void Deparent()
	{
		base.transform.SetParent(accessoriesObject);
		base.transform.position = base.transform.position + Vector3.up;
		base.transform.localScale = Vector3.one;
		Rigidbody component = GetComponent<Rigidbody>();
		if (component != null)
		{
			component.isKinematic = false;
			component.detectCollisions = true;
		}
	}
}
