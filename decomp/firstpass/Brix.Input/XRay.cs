using UnityEngine;

namespace Brix.Input;

public abstract class XRay : MonoBehaviour
{
	public abstract bool IsActive { get; }

	public abstract Bounds Bounds { get; }
}
