using UnityEngine;

namespace Brix.NewUI.Tooltips;

public class TestPositioner3D : MonoBehaviour
{
	public Tooltipositioner3D positioner;

	public GameObject target;

	public bool setTarget;

	private void Start()
	{
	}

	private void Update()
	{
		if (setTarget)
		{
			positioner.SetTarget(target);
			setTarget = false;
		}
	}
}
