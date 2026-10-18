using System;
using UnityEngine;

namespace UnityTest;

public class IsRenderedByCamera : ComparerBaseGeneric<Renderer, Camera>
{
	public enum CompareType
	{
		IsVisible,
		IsNotVisible
	}

	public CompareType compareType;

	protected override bool Compare(Renderer renderer, Camera camera)
	{
		Plane[] planes = GeometryUtility.CalculateFrustumPlanes(camera);
		bool flag = GeometryUtility.TestPlanesAABB(planes, renderer.bounds);
		return compareType switch
		{
			CompareType.IsVisible => flag, 
			CompareType.IsNotVisible => !flag, 
			_ => throw new Exception(), 
		};
	}
}
