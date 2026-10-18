using Brix.External.Factories;
using Brix.Legacy.Rendering;
using UnityEngine;

namespace Brix.Legacy;

internal class ScreenZone : MonoBehaviour
{
	public enum Shape
	{
		Circular
	}

	public enum Position
	{
		Center,
		BottomLeft
	}

	public enum Coordinates
	{
		Pixels,
		Percentage,
		SmallSidePercentage
	}

	public static GameObject master;

	public Shape shape;

	public Position position = Position.BottomLeft;

	public Coordinates coordinates = Coordinates.Percentage;

	public Vector2 size = Vector2.zero;

	public Vector2 offset = Vector2.zero;

	public bool show;

	private GameObject objDisplay;

	private void Start()
	{
		base.gameObject.GetOrAddComponent<FactoryImprint>().Id = "ScreenZoneDefault";
	}

	public void SetRadius(float radius)
	{
		size = Vector2.one * radius;
	}

	public bool ContainsWorldPoint(Vector3 p)
	{
		Vector3 vector = Camera.main.WorldToScreenPoint(p);
		if (vector.z < Camera.main.nearClipPlane)
		{
			return false;
		}
		return ContainsScreenPoint(vector);
	}

	public bool ContainsScreenPoint(Vector2 p)
	{
		Vector2 vector = new Vector2(Screen.width, Screen.height) * 0.5f;
		Vector2 vector2 = p - vector;
		if (position == Position.BottomLeft)
		{
			vector2 += vector;
		}
		if (coordinates == Coordinates.Percentage)
		{
			vector2 = new Vector2(vector2.x / vector.x, vector2.y / vector.y);
		}
		if (coordinates == Coordinates.SmallSidePercentage)
		{
			vector2 *= 1f / Mathf.Min(vector.x, vector.y);
		}
		Vector2 vector3 = new Vector2(vector2.x, vector2.y);
		Vector2 vector4 = offset;
		Vector2 vector5 = new Vector2((vector4.x - vector3.x) / size.x, (vector4.y - vector3.y) / size.y);
		if (shape == Shape.Circular)
		{
			return vector5.sqrMagnitude <= 1f;
		}
		return false;
	}

	private void Update()
	{
		if (objDisplay != null)
		{
			objDisplay.SetActive(show);
		}
		if (!show)
		{
			return;
		}
		if (objDisplay == null)
		{
			if (master == null)
			{
				return;
			}
			objDisplay = Object.Instantiate(master);
			objDisplay.transform.parent = master.transform.parent;
		}
		Vector2 vector = new Vector2(Screen.width, Screen.height) * 0.5f;
		Vector2 vector2 = size;
		if (coordinates == Coordinates.Percentage)
		{
			vector2 = new Vector2(vector2.x * vector.x, vector2.y * vector.y);
		}
		if (coordinates == Coordinates.SmallSidePercentage)
		{
			vector2 *= Mathf.Min(vector.x, vector.y);
		}
		float num = Mathf.Min(vector2.x, vector2.y);
		objDisplay.transform.localScale = new Vector3(vector2.x / num, vector2.y / num, 1f);
		IRadialRenderer component = objDisplay.GetComponent<IRadialRenderer>();
		if (component != null)
		{
			component.Radius = num;
		}
	}

	private void OnDestroy()
	{
		if ((bool)objDisplay)
		{
			Object.Destroy(objDisplay);
		}
	}
}
