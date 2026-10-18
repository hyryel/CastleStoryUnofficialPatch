using System;
using System.Collections.Generic;
using Brix.Utils.Logging;
using Smooth.Slinq;
using UnityEngine;

namespace Brix.Utils;

public class SphereCollection
{
	public class Sphere
	{
		public Vector3 center;

		public float radius;

		public float R2 => radius * radius;
	}

	private static BrixLogger logger = new BrixLogger(typeof(SphereCollection).FullName);

	public readonly List<Sphere> Spheres = new List<Sphere>();

	public void Clear()
	{
		Spheres.Clear();
	}

	public Sphere Add(float radius = 1f)
	{
		if (radius < 0.01f)
		{
			logger.WarnFormat("SphereCollection.Add() with negligeable radius {0}.", radius);
			radius = 0.01f;
		}
		Sphere sphere = new Sphere();
		sphere.center = Vector3.zero;
		sphere.radius = radius;
		Sphere sphere2 = sphere;
		Spheres.Add(sphere2);
		return sphere2;
	}

	public Sphere Add(Vector3 center, float radius = 1f)
	{
		if (radius < 0.01f)
		{
			logger.WarnFormat("SphereCollection.Add() with negligeable radius {0}.", radius);
			radius = 0.01f;
		}
		Sphere sphere = new Sphere();
		sphere.center = center;
		sphere.radius = radius;
		Sphere sphere2 = sphere;
		Spheres.Add(sphere2);
		return sphere2;
	}

	public void Shuffle(Vector3 center, bool avoidCollisions = true, Func<Vector3, bool> isValidPosition = null)
	{
		if (Spheres.Count == 1)
		{
			Spheres[0].center = center;
			return;
		}
		for (int i = 0; i < Spheres.Count; i++)
		{
			Sphere sphere = Spheres[i];
			float num = sphere.radius;
			for (int j = 0; j < 100; j++)
			{
				sphere.center = center + UnityEngine.Random.insideUnitSphere * num;
				if ((isValidPosition == null || isValidPosition(sphere.center)) && (!avoidCollisions || i <= 0 || !Collides(Spheres[i], Spheres, i - 1)))
				{
					break;
				}
				num += sphere.radius / 2f;
			}
		}
	}

	private bool Collides(Sphere s1, IList<Sphere> spheres, int n)
	{
		for (int i = 0; i < n; i++)
		{
			if (Collides(s1, spheres[i]))
			{
				return true;
			}
		}
		return false;
	}

	public bool Collides(Sphere s1, IEnumerable<Sphere> spheres)
	{
		return spheres.Slinq().Any((Sphere s2) => Collides(s1, s2));
	}

	public bool Collides(Sphere s1, Sphere s2)
	{
		Vector3 vector = s1.center - s2.center;
		float num = s1.radius + s2.radius;
		return vector.sqrMagnitude < num * num;
	}
}
