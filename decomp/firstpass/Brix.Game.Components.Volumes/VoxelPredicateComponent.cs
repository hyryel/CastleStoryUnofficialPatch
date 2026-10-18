using Brix.Engine;
using UnityEngine;

namespace Brix.Game.Components.Volumes;

public abstract class VoxelPredicateComponent : MonoBehaviour
{
	public abstract bool HasProperty(XYZ p);
}
