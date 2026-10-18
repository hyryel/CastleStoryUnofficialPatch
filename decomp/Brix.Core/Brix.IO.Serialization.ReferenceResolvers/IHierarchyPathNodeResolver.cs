using UnityEngine;

namespace Brix.IO.Serialization.ReferenceResolvers;

public interface IHierarchyPathNodeResolver
{
	bool IsAGameObjectReferenceNode(string node);

	GameObject GetReferencedGameObject(string node);
}
