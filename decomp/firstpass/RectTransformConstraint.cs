using UnityEngine;

public class RectTransformConstraint : MonoBehaviour
{
	public bool constrainOnRectTrsChange;

	public bool constrainOnStart;

	public bool constrainOnUpdate;

	public bool constrainWidth;

	public bool constrainHeight;

	public RectTransform constrainMaster;

	public RectTransform constrainSlave;

	public float widthOffset;

	public float heightOffset;

	private void Constrain()
	{
		if (!(constrainMaster == null) && !(constrainSlave == null))
		{
			Vector2 sizeDelta = constrainSlave.sizeDelta;
			bool flag = false;
			if (constrainWidth)
			{
				flag = true;
				sizeDelta.x = constrainMaster.sizeDelta.x + widthOffset;
			}
			if (constrainHeight)
			{
				flag = true;
				sizeDelta.y = constrainMaster.sizeDelta.y + heightOffset;
			}
			if (flag)
			{
				constrainSlave.sizeDelta = sizeDelta;
			}
		}
	}

	private void OnRectTransformDimensionsChange()
	{
		if (constrainOnRectTrsChange)
		{
			Constrain();
		}
	}

	private void Start()
	{
		if (constrainOnStart)
		{
			Constrain();
		}
	}

	private void Update()
	{
		if (constrainOnUpdate)
		{
			Constrain();
		}
	}
}
