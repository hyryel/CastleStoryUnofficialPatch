using UnityEngine;

public class Tooltipositioner3D : MonoBehaviour
{
	[SerializeField]
	private TransformConstraint constraint;

	[SerializeField]
	private GameObject target;

	[SerializeField]
	private GameObject tooltipTarget;

	public RectTransform SetTarget(GameObject _target)
	{
		target = _target;
		constraint.masterTransform = target.transform;
		return GetComponent<RectTransform>();
	}

	public void SetOffsetFromTarget(Vector3 offset)
	{
		GetComponent<TransformConstraint>().worldOffset = offset;
	}

	private void Start()
	{
	}

	private void Update()
	{
	}
}
