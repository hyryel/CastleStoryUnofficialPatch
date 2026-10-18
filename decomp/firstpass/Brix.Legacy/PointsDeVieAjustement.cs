using UnityEngine;

namespace Brix.Legacy;

public class PointsDeVieAjustement : MonoBehaviour
{
	private void LateUpdate()
	{
		base.transform.rotation = Quaternion.Euler(0f, Camera.main.transform.eulerAngles.y, 0f);
	}
}
