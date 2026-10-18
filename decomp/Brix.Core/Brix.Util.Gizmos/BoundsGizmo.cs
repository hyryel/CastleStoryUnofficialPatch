using UnityEngine;

namespace Brix.Util.Gizmos;

public class BoundsGizmo : MonoBehaviour
{
	public Bounds bounds;

	public Color color = new Color(0f, 1f, 0f, 0.5f);

	public bool show = true;

	private void OnDrawGizmos()
	{
		if (show)
		{
			Color color = UnityEngine.Gizmos.color;
			UnityEngine.Gizmos.color = this.color;
			UnityEngine.Gizmos.DrawWireCube(bounds.center, bounds.size);
			UnityEngine.Gizmos.color = color;
		}
	}
}
