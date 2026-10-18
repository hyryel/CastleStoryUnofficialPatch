using UnityEngine;

namespace Brix.NewUI.LoadingScreen;

public class BarFollow : MonoBehaviour
{
	public RectTransform masterBar;

	public RectTransform slaveBar;

	public float lagRatio = 1f;

	public void SetMasterValue(float value)
	{
		masterBar.anchorMax = new Vector2(value, masterBar.anchorMax.y);
	}

	private void Update()
	{
		if (lagRatio != 0f)
		{
			float x = Mathf.Lerp(slaveBar.anchorMax.x, masterBar.anchorMax.x, Time.deltaTime / lagRatio);
			slaveBar.anchorMax = new Vector2(x, slaveBar.anchorMax.y);
		}
	}
}
