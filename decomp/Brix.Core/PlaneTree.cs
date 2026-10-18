using System;
using UnityEngine;

public class PlaneTree
{
	private struct Box
	{
		public Vector3 min;

		public Vector3 max;

		public Box(Vector3 min, Vector3 max)
		{
			this.min = min;
			this.max = max;
		}

		public bool Contains(Vector3 p)
		{
			return p.x >= min.x && p.x <= max.x && p.y >= min.y && p.y <= max.y && p.z >= min.z && p.z <= max.z;
		}
	}

	private abstract class Plane
	{
		public abstract bool FirstHalf(Vector3 p);

		public abstract bool SecondHalf(Vector3 p);

		public abstract bool IntersectsFirst(Box box);

		public abstract bool IntersectsSecond(Box box);
	}

	private class XPlane : Plane
	{
		private float x;

		public XPlane(float x)
		{
			this.x = x;
		}

		public override bool FirstHalf(Vector3 p)
		{
			return p.x <= x;
		}

		public override bool SecondHalf(Vector3 p)
		{
			return p.x >= x;
		}

		public override bool IntersectsFirst(Box box)
		{
			return box.min.x <= x;
		}

		public override bool IntersectsSecond(Box box)
		{
			return box.max.x >= x;
		}
	}

	private class YPlane : Plane
	{
		private float y;

		public YPlane(float y)
		{
			this.y = y;
		}

		public override bool FirstHalf(Vector3 p)
		{
			return p.y <= y;
		}

		public override bool SecondHalf(Vector3 p)
		{
			return p.y >= y;
		}

		public override bool IntersectsFirst(Box box)
		{
			return box.min.y <= y;
		}

		public override bool IntersectsSecond(Box box)
		{
			return box.max.y >= y;
		}
	}

	private class ZPlane : Plane
	{
		private float z;

		public ZPlane(float z)
		{
			this.z = z;
		}

		public override bool FirstHalf(Vector3 p)
		{
			return p.z <= z;
		}

		public override bool SecondHalf(Vector3 p)
		{
			return p.z >= z;
		}

		public override bool IntersectsFirst(Box box)
		{
			return box.min.z <= z;
		}

		public override bool IntersectsSecond(Box box)
		{
			return box.max.z >= z;
		}
	}

	private abstract class Node
	{
		public IntMask layers;

		public abstract int GetPointsInsideBox(Box box, Classeur<Vector3> points, Classeur<object> objects, IntMask layers);

		public abstract int GetObjectsAtPoint(Vector3 point, Classeur<object> objects, IntMask layers);

		public abstract object GetObjectAtPoint(Vector3 point, IntMask layers);
	}

	private class Branch : Node
	{
		public Plane plane;

		public Node a;

		public Node b;

		public Branch(Leaf leaf, Vector3 p, object o, IntMask layers)
		{
			Vector3 vector = (leaf.p + p) / 2f;
			Vector3 vector2 = leaf.p - p;
			vector2 = new Vector3(Mathf.Abs(vector2.x), Mathf.Abs(vector2.y), Mathf.Abs(vector2.z));
			plane = ((vector2.x >= vector2.y && vector2.x >= vector2.z) ? new XPlane(vector.x) : ((vector2.y >= vector2.z) ? ((Plane)new YPlane(vector.y)) : ((Plane)new ZPlane(vector.z))));
			if (plane.FirstHalf(p))
			{
				a = new Leaf(p, o, layers);
				b = leaf;
			}
			else
			{
				a = leaf;
				b = new Leaf(p, o, layers);
			}
			layers.Add(leaf.layers);
			base.layers = layers;
		}

		public override int GetPointsInsideBox(Box box, Classeur<Vector3> points, Classeur<object> objects, IntMask layers)
		{
			int num = 0;
			if (plane.IntersectsFirst(box) && layers.Intersects(a.layers))
			{
				num += a.GetPointsInsideBox(box, points, objects, layers);
			}
			if (plane.IntersectsSecond(box) && layers.Intersects(b.layers))
			{
				num += b.GetPointsInsideBox(box, points, objects, layers);
			}
			return num;
		}

		public override int GetObjectsAtPoint(Vector3 point, Classeur<object> objects, IntMask layers)
		{
			int num;
			if (plane.FirstHalf(point) && layers.Intersects(a.layers))
			{
				num = a.GetObjectsAtPoint(point, objects, layers);
				if (num == 0 && plane.SecondHalf(point))
				{
					num += b.GetObjectsAtPoint(point, objects, layers);
				}
				return num;
			}
			num = b.GetObjectsAtPoint(point, objects, layers);
			if (num == 0 && plane.SecondHalf(point))
			{
				num += a.GetObjectsAtPoint(point, objects, layers);
			}
			return num;
		}

		public override object GetObjectAtPoint(Vector3 point, IntMask layers)
		{
			object obj = null;
			if (plane.FirstHalf(point) && layers.Intersects(a.layers))
			{
				obj = a.GetObjectAtPoint(point, layers);
				if (obj == null && plane.SecondHalf(point))
				{
					obj = b.GetObjectAtPoint(point, layers);
				}
				return obj;
			}
			obj = b.GetObjectAtPoint(point, layers);
			if (obj == null && plane.SecondHalf(point))
			{
				obj = a.GetObjectAtPoint(point, layers);
			}
			return obj;
		}
	}

	private class Leaf : Node
	{
		public Vector3 p;

		public object o;

		public Leaf(Vector3 p, object o, IntMask layers)
		{
			this.p = p;
			this.o = o;
			base.layers = layers;
		}

		public override int GetPointsInsideBox(Box box, Classeur<Vector3> points, Classeur<object> objects, IntMask layers)
		{
			if (!box.Contains(p) || !layers.Intersects(base.layers))
			{
				return 0;
			}
			points.Add(p);
			objects.Add(o);
			return 1;
		}

		public override int GetObjectsAtPoint(Vector3 point, Classeur<object> objects, IntMask layers)
		{
			if (p != point || !layers.Intersects(base.layers))
			{
				return 0;
			}
			objects.Add(o);
			return 1;
		}

		public override object GetObjectAtPoint(Vector3 point, IntMask layers)
		{
			return (p != point || !layers.Intersects(base.layers)) ? null : o;
		}
	}

	private Node root;

	private static Node PlusPoint(Node n, Vector3 p, object o, IntMask layers)
	{
		if (n is Leaf)
		{
			return new Branch(n as Leaf, p, o, layers);
		}
		Branch branch = n as Branch;
		if (branch.plane.FirstHalf(p))
		{
			branch.a = PlusPoint(branch.a, p, o, layers);
		}
		else
		{
			branch.b = PlusPoint(branch.b, p, o, layers);
		}
		return n;
	}

	private static Node MinusPoint(Node n, Vector3 p, object o, IntMask layers)
	{
		if (n is Leaf)
		{
			Leaf leaf = n as Leaf;
			if (leaf.o != o || !leaf.layers.Matches(layers))
			{
				throw null;
			}
			return null;
		}
		Branch branch = n as Branch;
		if (branch.plane.FirstHalf(p) && branch.layers.Contains(layers))
		{
			try
			{
				branch.a = MinusPoint(branch.a, p, o, layers);
				if (branch.a == null)
				{
					return branch.b;
				}
			}
			catch (Exception)
			{
				branch.b = MinusPoint(branch.b, p, o, layers);
				if (branch.b == null)
				{
					return branch.a;
				}
			}
		}
		else
		{
			try
			{
				branch.b = MinusPoint(branch.b, p, o, layers);
				if (branch.b == null)
				{
					return branch.a;
				}
			}
			catch (Exception)
			{
				branch.a = MinusPoint(branch.a, p, o, layers);
				if (branch.a == null)
				{
					return branch.b;
				}
			}
		}
		branch.layers = branch.a.layers;
		branch.layers.Add(branch.b.layers);
		return branch;
	}

	public void AddPoint(Vector3 p, object o)
	{
		AddPoint(p, o, 1);
	}

	public void AddPoint(Vector3 p, object o, int layers)
	{
		if (root == null)
		{
			root = new Leaf(p, o, layers);
		}
		else
		{
			root = PlusPoint(root, p, o, layers);
		}
	}

	public void RemovePoint(Vector3 p, object o)
	{
		RemovePoint(p, o, 1);
	}

	public void RemovePoint(Vector3 p, object o, int layers)
	{
		try
		{
			root = MinusPoint(root, p, o, layers);
		}
		catch (Exception)
		{
			Donnees.Log("Mauvais objet!");
		}
	}

	public int GetPointsInsideBox(Bounds bounds, Classeur<Vector3> points, Classeur<object> objects)
	{
		return GetPointsInsideBox(bounds, points, objects, -1);
	}

	public int GetPointsInsideBox(Bounds bounds, Classeur<Vector3> points, Classeur<object> objects, int layers)
	{
		if (root == null)
		{
			return 0;
		}
		return root.GetPointsInsideBox(new Box(bounds.min, bounds.max), points, objects, layers);
	}

	public int GetObjectsAtPoint(Vector3 point, Classeur<object> objects)
	{
		return GetObjectsAtPoint(point, objects, -1);
	}

	public int GetObjectsAtPoint(Vector3 point, Classeur<object> objects, int layers)
	{
		if (root == null)
		{
			return 0;
		}
		return root.GetObjectsAtPoint(point, objects, layers);
	}

	public object GetObjectAtPoint(Vector3 point)
	{
		return GetObjectAtPoint(point, -1);
	}

	public object GetObjectAtPoint(Vector3 point, int layers)
	{
		if (root == null)
		{
			return null;
		}
		return root.GetObjectAtPoint(point, layers);
	}
}
