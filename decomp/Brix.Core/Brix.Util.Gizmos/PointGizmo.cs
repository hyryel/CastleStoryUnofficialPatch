using UnityEngine;

namespace Brix.Util.Gizmos;

public class PointGizmo : MonoBehaviour
{
	public Vector3 point = Vector3.zero;

	public Color color = Color.green;

	public float radius = 0.05f;

	public bool show = true;

	private void OnDrawGizmos()
	{
		if (show)
		{
			Color color = UnityEngine.Gizmos.color;
			UnityEngine.Gizmos.color = this.color;
			UnityEngine.Gizmos.DrawSphere(base.transform.position + point, radius);
			UnityEngine.Gizmos.color = color;
		}
	}
}
