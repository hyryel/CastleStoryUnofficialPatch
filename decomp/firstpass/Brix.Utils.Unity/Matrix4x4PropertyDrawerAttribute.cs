using UnityEngine;

namespace Brix.Utils.Unity;

public class Matrix4x4PropertyDrawerAttribute : PropertyAttribute
{
	public bool showPosition = true;

	public bool showRotation = true;

	public bool showScale = true;

	public Matrix4x4PropertyDrawerAttribute()
	{
	}

	public Matrix4x4PropertyDrawerAttribute(bool showPosition, bool showRotation, bool showScale)
	{
		this.showPosition = showPosition;
		this.showRotation = showRotation;
		this.showScale = showScale;
	}
}
