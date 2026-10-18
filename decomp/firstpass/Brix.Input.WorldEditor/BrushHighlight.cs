using Brix.Engine.Environment;
using UnityEngine;

namespace Brix.Input.WorldEditor;

public class BrushHighlight : MonoBehaviour
{
	public bool active;

	public Vector3 position;

	public Color DefaultBrushColor;

	public Color DesertBrushColor;

	public Color DefaultRemoverColor;

	public Color DesertRemoverColor;

	public GameObject CircleHighlight;

	public GameObject SquareHighlight;

	private float _size;

	private Renderer _circleRenderer;

	private Renderer _squareRenderer;

	public float Size
	{
		get
		{
			return _size;
		}
		set
		{
			_size = value;
			Vector3 localScale = new Vector3(value, value, value);
			CircleHighlight.transform.localScale = localScale;
			SquareHighlight.transform.localScale = localScale;
		}
	}

	public Color BrushColor
	{
		get
		{
			return _circleRenderer.material.GetColor("_Color");
		}
		set
		{
			_circleRenderer.material.SetColor("_Color", value);
			_squareRenderer.material.SetColor("_Color", value);
		}
	}

	private void Start()
	{
		_circleRenderer = CircleHighlight.GetComponent<Renderer>();
		_squareRenderer = SquareHighlight.GetComponent<Renderer>();
		if (GameParam.GetBiomeType() == BiomeType.Desert)
		{
			DefaultBrushColor = DesertBrushColor;
			DefaultRemoverColor = DesertRemoverColor;
		}
	}

	private void Update()
	{
		if (_size <= 3f)
		{
			_circleRenderer.enabled = false;
			_squareRenderer.enabled = active;
		}
		else
		{
			_squareRenderer.enabled = false;
			_circleRenderer.enabled = active;
		}
		if (active)
		{
			CircleHighlight.transform.position = position;
			SquareHighlight.transform.position = position;
		}
	}
}
