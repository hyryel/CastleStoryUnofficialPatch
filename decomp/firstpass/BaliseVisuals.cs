using UnityEngine;

public class BaliseVisuals : MonoBehaviour
{
	[SerializeField]
	private float height = 4f;

	[SerializeField]
	private Transform topLine;

	[SerializeField]
	private Transform botLine;

	[SerializeField]
	private Transform overheadTarget;

	public float Height
	{
		get
		{
			return height;
		}
		set
		{
			height = value;
			topLine.localScale = new Vector3(1f, height / 4f, 1f);
			botLine.localScale = new Vector3(1f, height / 4f, 1f);
			overheadTarget.localPosition = new Vector3(0f, height, 0f);
		}
	}

	private void OnValidate()
	{
		Height = height;
	}

	private void Start()
	{
		Height = height;
	}
}
