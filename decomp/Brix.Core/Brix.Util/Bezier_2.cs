using UnityEngine;

namespace Brix.Util;

public class Bezier_2
{
	public class BezierSection
	{
		public Vector3 p1;

		public Vector3 p2;

		public Vector3 t1;

		public Vector3 t2;

		public BezierSection(Vector3 startPosition, Vector3 endPosition)
			: this(startPosition, endPosition, endPosition - startPosition, endPosition - startPosition)
		{
		}

		public BezierSection(Vector3 startPosition, Vector3 endPosition, Vector3 startTangent, Vector3 endTangent)
		{
			p1 = startPosition;
			p2 = endPosition;
			t1 = startTangent;
			t2 = endTangent;
		}

		public Vector3 InterpolerPosition(float temps)
		{
			return Vector3.Lerp(p1, p2, temps);
		}
	}

	private Classeur<Vector3> points = new Classeur<Vector3>();

	private Classeur<float> tempsListe = new Classeur<float>();

	private Classeur<BezierSection> sections = new Classeur<BezierSection>();

	public void AddPoint(Vector3 p)
	{
		AddPoint(p, (tempsListe.Count == 0) ? 0f : (tempsListe[tempsListe.Count - 1] + 1f));
	}

	public void AddPoint(Vector3 p, float temps)
	{
		points.Add(p);
		tempsListe.Add(temps);
		if (points.Count >= 2)
		{
			sections.Add(new BezierSection(points[points.Count - 2], p));
		}
	}
}
