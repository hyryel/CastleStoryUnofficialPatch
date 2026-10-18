using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Brix.Engine;
using Brix.Engine.Abstract;
using Brix.Util.Paire;
using Brix.Utils;
using UnityEngine;

namespace Brix.Util;

public class Calcule
{
	public struct VolumeEnumerator : IPoolable, IResetable, IDisposable, IEnumerator<XYZ>, IEnumerator
	{
		private XYZ _min;

		private XYZ _max;

		private XYZ _current;

		public XYZ Current => _current;

		object IEnumerator.Current => Current;

		public VolumeEnumerator(XYZ min, XYZ max)
		{
			_min = min;
			_max = max;
			_current = min;
			_current.x--;
		}

		public VolumeEnumerator Reset(XYZ min, XYZ max)
		{
			_min = min;
			_max = max;
			_current = min;
			_current.x--;
			return this;
		}

		public bool MoveNext()
		{
			_current.x++;
			if (_current.x <= _max.x)
			{
				return true;
			}
			_current.x = _min.x;
			_current.z++;
			if (_current.z <= _max.z)
			{
				return true;
			}
			_current.z = _min.z;
			_current.y++;
			if (_current.y <= _max.y)
			{
				return true;
			}
			_current.y = _min.y;
			return false;
		}

		public void Reset()
		{
			_current = _min;
			_current.x--;
		}

		public void Dispose()
		{
			ResetObject();
		}

		public void ResetObject()
		{
			Reset();
		}
	}

	public struct VolumeEnumerable : IPoolable, IResetable, IDisposable, IEnumerable<XYZ>, IEnumerable
	{
		private XYZ _min;

		private XYZ _max;

		public VolumeEnumerable(XYZ min, XYZ max)
		{
			_min = min;
			_max = max;
		}

		public VolumeEnumerable Reset(XYZ min, XYZ max)
		{
			_min = min;
			_max = max;
			return this;
		}

		public VolumeEnumerator GetEnumerator()
		{
			return new VolumeEnumerator(_min, _max);
		}

		IEnumerator<XYZ> IEnumerable<XYZ>.GetEnumerator()
		{
			return GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		public void ResetObject()
		{
		}

		public void Dispose()
		{
		}
	}

	private const int DefaultRotationCenter = 524800;

	private static int[] _rotatedConfigs = (from i in Enumerable.Range(0, 384)
		select _RotatedConfig(i % 64, FaceIndexToRotationDeBase((byte)(i / 64)))).ToArray();

	private static int[] _inverseRotatedConfigs = (from i in Enumerable.Range(0, 384)
		select _RotatedConfig(i % 64, Quaternion.Inverse(FaceIndexToRotationDeBase((byte)(i / 64))))).ToArray();

	public static int Abs(int nombre)
	{
		return (nombre ^ (nombre >> 31)) - (nombre >> 31);
	}

	public static int Min(int a, int b)
	{
		return (a < b) ? a : b;
	}

	public static int Min(int a, int b, int c)
	{
		return Min(a, Min(b, c));
	}

	public static int Max(int a, int b)
	{
		return (a >= b) ? a : b;
	}

	public static int Max(int a, int b, int c)
	{
		return Max(a, Max(b, c));
	}

	public static int Floor(int a, int b)
	{
		return a / b;
	}

	public static int Ceil(int a, int b)
	{
		return Floor(a + b - 1, b);
	}

	public static int Round(int a, int b)
	{
		return Floor(a + b / 2, b);
	}

	public static int Sqr(int a)
	{
		return a * a;
	}

	public static float Sqr(float a)
	{
		return a * a;
	}

	public static int FloorSafe(int a, int b)
	{
		int num = Floor(a, b);
		return (num * b > a) ? (num - 1) : num;
	}

	public static int CeilSafe(int a, int b)
	{
		return FloorSafe(a + b - 1, b);
	}

	public static int RoundSafe(int a, int b)
	{
		return FloorSafe(a + b / 2, b);
	}

	public static int RemapMinMax(int x, int min0, int max0, int min1, int max1)
	{
		x = Mathf.Clamp(x, min0, max0);
		return Round((x - min0) * (max1 - min1), max0 - min0) + min1;
	}

	public static int MinTowardsZero(int a, int b)
	{
		return (a <= 0) ? ((a > b) ? a : ((b < 0) ? b : 0)) : ((a < b) ? a : ((b > 0) ? b : 0));
	}

	public static Vector3 AbsVecteur(Vector3 vecteur)
	{
		vecteur.x = Mathf.Abs(vecteur.x);
		vecteur.y = Mathf.Abs(vecteur.y);
		vecteur.z = Mathf.Abs(vecteur.z);
		return vecteur;
	}

	public static Vector3 FloorVecteur(Vector3 vecteur)
	{
		vecteur.x = Mathf.Floor(vecteur.x);
		vecteur.y = Mathf.Floor(vecteur.y);
		vecteur.z = Mathf.Floor(vecteur.z);
		return vecteur;
	}

	public static Vector3 CeilVecteur(Vector3 vecteur)
	{
		vecteur.x = Mathf.Ceil(vecteur.x);
		vecteur.y = Mathf.Ceil(vecteur.y);
		vecteur.z = Mathf.Ceil(vecteur.z);
		return vecteur;
	}

	public static Vector3 RoundVecteur(Vector3 vecteur)
	{
		vecteur.x = Mathf.Round(vecteur.x);
		vecteur.y = Mathf.Round(vecteur.y);
		vecteur.z = Mathf.Round(vecteur.z);
		return vecteur;
	}

	public static Vector3 FlatManhattanDirection(Vector3 position, Vector3 relativeTo)
	{
		Vector3 vector = position - relativeTo;
		return new Vector3(Mathf.Sign(vector.x), 0f, Mathf.Sign(vector.z));
	}

	public static XYZ RoundVecteurToward(Vector3 toRound, Vector3 direction, bool exeptY = true)
	{
		Vector3 v = toRound;
		v.x = RoundToward(toRound.x, direction.x);
		if (exeptY)
		{
			v.y = RoundVecteur(toRound).y;
		}
		else
		{
			v.y = RoundToward(toRound.y, direction.y);
		}
		v.z = RoundToward(toRound.z, direction.z);
		return XYZ.FromVector3(v);
	}

	public static float RoundToward(float toRound, float direction)
	{
		return (direction > toRound) ? Mathf.Ceil(toRound) : Mathf.Floor(toRound);
	}

	public static Vector3 MinVecteur(Vector3 u, Vector3 v)
	{
		return new Vector3(Mathf.Min(u.x, v.x), Mathf.Min(u.y, v.y), Mathf.Min(u.z, v.z));
	}

	public static Vector3 MaxVecteur(Vector3 u, Vector3 v)
	{
		return new Vector3(Mathf.Max(u.x, v.x), Mathf.Max(u.y, v.y), Mathf.Max(u.z, v.z));
	}

	public static Vector3 ClampVecteur(Vector3 v, Vector3 min, Vector3 max)
	{
		return new Vector3(Mathf.Clamp(v.x, min.x, max.x), Mathf.Clamp(v.y, min.y, max.y), Mathf.Clamp(v.z, min.z, max.z));
	}

	public static Vector3 BoxVecteur(Vector3 v, Bounds bounds, bool checkSiDedans)
	{
		if (checkSiDedans && bounds.Contains(v))
		{
			return v;
		}
		Vector3 normalized = (bounds.center - v).normalized;
		Ray ray = new Ray(v, normalized);
		bounds.IntersectRay(ray, out var distance);
		return v + distance * normalized;
	}

	public static Vector3 NonColinearVector(Vector3 normal)
	{
		if (Mathf.Abs(Vector3.Dot(normal, Vector3.right)) < 0.8f)
		{
			return Vector3.right;
		}
		if (Mathf.Abs(Vector3.Dot(normal, Vector3.up)) < 0.8f)
		{
			return Vector3.up;
		}
		if (Mathf.Abs(Vector3.Dot(normal, Vector3.forward)) < 0.8f)
		{
			return Vector3.forward;
		}
		throw new ArgumentException("normal");
	}

	public static Vector3 NonColinearVector(Vector3 normal, Vector3 preferredResult)
	{
		if (Mathf.Abs(Vector3.Dot(normal, preferredResult)) < 0.8f)
		{
			return preferredResult;
		}
		Vector3 vector = PerpendicularVector(preferredResult);
		if (Mathf.Abs(Vector3.Dot(normal, vector)) < 0.8f)
		{
			return vector;
		}
		Vector3 vector2 = Vector3.Cross(preferredResult, vector);
		if (Mathf.Abs(Vector3.Dot(normal, vector2)) < 0.8f)
		{
			return vector2;
		}
		throw new ArgumentException("normal");
	}

	public static Vector3 PerpendicularVector(Vector3 normal)
	{
		return Vector3.Cross(normal, NonColinearVector(normal)).normalized;
	}

	public static Vector3 PerpendicularVector(Vector3 normal, Vector3 preferredResult)
	{
		return Vector3.Cross(normal, NonColinearVector(normal, preferredResult)).normalized;
	}

	public static Vector3 AverageVector(params Vector3[] vs)
	{
		Vector3 zero = Vector3.zero;
		for (int i = 0; i < vs.Length; i++)
		{
			zero += vs[i];
		}
		return zero / vs.Length;
	}

	public static Vector3 AverageVector(IList<Vector3> vs)
	{
		Vector3 zero = Vector3.zero;
		for (int i = 0; i < vs.Count; i++)
		{
			zero += vs[i];
		}
		return zero / vs.Count;
	}

	public static Vector3 AverageVector(IEnumerable<Vector3> vs)
	{
		IEnumerator<Vector3> enumerator = vs.GetEnumerator();
		if (!enumerator.MoveNext())
		{
			return Vector3.zero;
		}
		Vector3 current = enumerator.Current;
		int num = 1;
		while (enumerator.MoveNext())
		{
			current += enumerator.Current;
			num++;
		}
		return current / num;
	}

	public static float AngleVecteur(Vector3 a, Vector3 b, Vector3 directionRegardee)
	{
		a -= Vector3.Project(a, directionRegardee);
		b -= Vector3.Project(b, directionRegardee);
		return Vector3.Angle(a, b) * Mathf.Sign(Vector3.Dot(Vector3.Cross(a, b), directionRegardee));
	}

	public static float AngleHorizontal(Vector3 a, Vector3 b)
	{
		return AngleVecteur(a, b, -Vector3.up);
	}

	public static int Manhattan(Vector3 v1, Vector3 v2)
	{
		return Mathf.FloorToInt(Mathf.Abs(v1.x - v2.x) + Mathf.Abs(v1.y - v2.y) + Mathf.Abs(v1.z - v2.z));
	}

	public static int Manhattan(int int1, int int2)
	{
		return Abs(Voxel.GetX(int1) - Voxel.GetX(int2)) + Abs(Voxel.GetY(int1) - Voxel.GetY(int2)) + Abs(Voxel.GetZ(int1) - Voxel.GetZ(int2));
	}

	public static int Manhattan2(Vector3 v1, Vector3 v2)
	{
		return 15 * Mathf.FloorToInt(Mathf.Abs(v1.x - v2.x) + Mathf.Abs(v1.y - v2.y) + Mathf.Abs(v1.z - v2.z));
	}

	public static int Manhattan2(int int1, int int2)
	{
		return 15 * (Abs(Voxel.GetX(int1) - Voxel.GetX(int2)) + Abs(Voxel.GetY(int1) - Voxel.GetY(int2)) + Abs(Voxel.GetZ(int1) - Voxel.GetZ(int2)));
	}

	public static int Manhattan3(Vector3 v1, Vector3 v2)
	{
		Vector3 vector = new Vector3(Mathf.Abs(v1.x - v2.x), Mathf.Abs(v1.y - v2.y), Mathf.Abs(v1.z - v2.z));
		return Mathf.FloorToInt(30f * Mathf.Max(vector.x, vector.z));
	}

	public static bool IsPlusProche(int a, int b, int c)
	{
		return ((Voxel.GetVecteur(b) - Voxel.GetVecteur(a)).sqrMagnitude < (Voxel.GetVecteur(c) - Voxel.GetVecteur(a)).sqrMagnitude) ? true : false;
	}

	public static int GetMaxDistance(int a, int b)
	{
		Vector3 vector = new Vector3(Mathf.Abs(Voxel.GetX(a) - Voxel.GetX(b)), Mathf.Abs(Voxel.GetY(a) - Voxel.GetY(b)), Mathf.Abs(Voxel.GetZ(a) - Voxel.GetZ(b)));
		return Mathf.FloorToInt(Mathf.Max(vector.x, Mathf.Max(vector.y, vector.z)));
	}

	public static int GetSqrDistance(int a, int b)
	{
		int num = Voxel.GetX(a) - Voxel.GetX(b);
		int num2 = Voxel.GetY(a) - Voxel.GetY(b);
		int num3 = Voxel.GetZ(a) - Voxel.GetZ(b);
		return num * num + num2 * num2 + num3 * num3;
	}

	public static float GetDistance(int a, int b)
	{
		return Mathf.Sqrt(GetSqrDistance(a, b));
	}

	public static int GetSqrDistance(XYZ a, XYZ b)
	{
		int num = a.x - b.x;
		int num2 = a.y - b.y;
		int num3 = a.z - b.z;
		return num * num + num2 * num2 + num3 * num3;
	}

	public static float GetDistance(XYZ a, XYZ b)
	{
		return Mathf.Sqrt(GetSqrDistance(a, b));
	}

	public static int DownscaleScalaire(int scalaire, int factor)
	{
		int x = Voxel.GetX(scalaire);
		int y = Voxel.GetY(scalaire);
		int z = Voxel.GetZ(scalaire);
		return Voxel.GetScalaire(x / factor, y / factor, z / factor);
	}

	public static int UpscaleScalaire(int scalaire, int factor)
	{
		return scalaire * factor;
	}

	public static int RotateScalaire(int scalaire, int quadrant)
	{
		return RotateScalaire(scalaire, quadrant, 524800);
	}

	public static int RotateScalaire(int scalaire, int quadrant, int center)
	{
		Vector3 vecteur = GetVecteur(center);
		Quaternion quaternion = Quaternion.Euler(0f, 90 * quadrant, 0f);
		Vector3 vecteur2 = GetVecteur(scalaire);
		vecteur2 = quaternion * (vecteur2 - vecteur) + vecteur;
		return GetScalaire(vecteur2);
	}

	public static IEnumerable<int> RotateScalaires(IEnumerable<int> scalaires, int quadrant)
	{
		return RotateScalaires(scalaires, quadrant, 524800);
	}

	public static IEnumerable<int> RotateScalaires(IEnumerable<int> scalaires, int quadrant, int center = 524800)
	{
		Vector3 c = GetVecteur(center);
		Quaternion r = Quaternion.Euler(0f, 90 * quadrant, 0f);
		foreach (int scalaire in scalaires)
		{
			Vector3 s = GetVecteur(scalaire);
			s = r * (s - c) + c;
			yield return GetScalaire(s);
		}
	}

	public static int ScalaireToConfig(int scalaire)
	{
		return scalaire switch
		{
			1048576 => 1, 
			1024 => 2, 
			-1024 => 4, 
			1 => 8, 
			-1 => 16, 
			-1048576 => 32, 
			_ => 0, 
		};
	}

	public static int ScalaireToConfig(int scalaire1, int scalaire2)
	{
		return ScalaireToConfig(scalaire1 - scalaire2);
	}

	public static int ScalaireToCoteIndex(int index)
	{
		return index switch
		{
			1024 => 0, 
			-1024 => 2, 
			1 => 1, 
			-1 => 3, 
			_ => 0, 
		};
	}

	public static Vector3 ScalaireSimpleToVecteur(int index)
	{
		return index switch
		{
			1048576 => Vector3.up, 
			1024 => Vector3.forward, 
			-1024 => -Vector3.forward, 
			1 => Vector3.right, 
			-1 => -Vector3.right, 
			-1048576 => -Vector3.up, 
			_ => Vector3.zero, 
		};
	}

	public static int FaceIndexToScalaire(int index)
	{
		return Voxel.faceIndexToScalaire[index];
	}

	public static int VecteurToFaceIndex(Vector3 vecteur)
	{
		if (vecteur == Vector3.up)
		{
			return 0;
		}
		if (vecteur == Vector3.forward)
		{
			return 1;
		}
		if (vecteur == -Vector3.forward)
		{
			return 2;
		}
		if (vecteur == Vector3.right)
		{
			return 3;
		}
		if (vecteur == -Vector3.right)
		{
			return 4;
		}
		if (vecteur == -Vector3.up)
		{
			return 5;
		}
		return 0;
	}

	public static Vector3 FaceIndexToVecteur(int index)
	{
		return Voxel.faceIndexToVecteur[index];
	}

	public static int ScalaireToFaceIndex(int index)
	{
		return index switch
		{
			1048576 => 0, 
			1024 => 1, 
			-1024 => 2, 
			1 => 3, 
			-1 => 4, 
			-1048576 => 5, 
			_ => 0, 
		};
	}

	public static int InvertFaceIndex(int index)
	{
		return index switch
		{
			0 => 5, 
			1 => 2, 
			2 => 1, 
			3 => 4, 
			4 => 3, 
			5 => 0, 
			_ => 0, 
		};
	}

	public static int FaceIndexToConfig(int faceIndex)
	{
		return 1 << faceIndex;
	}

	public static int RotationToFaceIndex(Quaternion rotation)
	{
		return VecteurToFaceIndex(rotation * Vector3.forward);
	}

	public static int InvertConfig(int config)
	{
		return config switch
		{
			32 => 1, 
			16 => 8, 
			8 => 16, 
			4 => 2, 
			2 => 4, 
			1 => 32, 
			_ => 0, 
		};
	}

	public static int VecteurToConfig(Vector3 vecteur)
	{
		return FaceIndexToConfig(VecteurToFaceIndex(vecteur));
	}

	public static Vector3 ConfigToVecteur(int config)
	{
		return config switch
		{
			1 => Vector3.up, 
			2 => Vector3.forward, 
			4 => -Vector3.forward, 
			8 => Vector3.right, 
			16 => -Vector3.right, 
			32 => -Vector3.up, 
			_ => Vector3.zero, 
		};
	}

	public static int ConfigToScalaire(int config)
	{
		return config switch
		{
			1 => 1048576, 
			2 => 1024, 
			4 => -1024, 
			8 => 1, 
			16 => -1, 
			32 => -1048576, 
			_ => 0, 
		};
	}

	private static int _RotatedConfig(int config, Quaternion rot)
	{
		int num = 0;
		for (int i = 0; i < 6; i++)
		{
			int num2 = VecteurToFaceIndex(GetDiscreteVector(rot * FaceIndexToVecteur((byte)i)));
			if ((config & (1 << i)) != 0)
			{
				num |= 1 << num2;
			}
		}
		return num;
	}

	public static int RotatedConfig(int config, int faceIndex)
	{
		return _rotatedConfigs[config + faceIndex * 64];
	}

	public static int InverseRotatedConfig(int config, int faceIndex)
	{
		return _inverseRotatedConfigs[config + faceIndex * 64];
	}

	public static int RotatedConfig(int config, Quaternion rotation)
	{
		return _RotatedConfig(config, rotation);
	}

	public static int InverseRotatedConfig(int config, Quaternion rotation)
	{
		return _RotatedConfig(config, Quaternion.Inverse(rotation));
	}

	public static IEnumerable<T> PelureGenerique<T>(Func<int, int, int, T> makeValue, int rayon)
	{
		if (rayon == 0)
		{
			yield return makeValue(0, 0, 0);
			yield break;
		}
		for (int z = -rayon; z <= rayon; z += 2 * rayon)
		{
			for (int x = -rayon; x <= rayon; x++)
			{
				for (int y = -rayon; y <= rayon; y++)
				{
					yield return makeValue(x, y, z);
				}
			}
		}
		for (int y = -rayon; y <= rayon; y += 2 * rayon)
		{
			for (int x = -rayon; x <= rayon; x++)
			{
				for (int z = -rayon + 1; z <= rayon - 1; z++)
				{
					yield return makeValue(x, y, z);
				}
			}
		}
		for (int x = -rayon; x <= rayon; x += 2 * rayon)
		{
			for (int y = -rayon + 1; y <= rayon - 1; y++)
			{
				for (int z = -rayon + 1; z <= rayon - 1; z++)
				{
					yield return makeValue(x, y, z);
				}
			}
		}
	}

	public static IEnumerable<Vector3> Pelure(Vector3 centre, int rayon)
	{
		return from p in PelureGenerique((int x, int y, int z) => new Vector3(x, y, z), rayon)
			select centre + p;
	}

	public static IEnumerable<XYZ> Pelure(XYZ centre, int rayon)
	{
		return from p in PelureGenerique((int x, int y, int z) => new XYZ(x, y, z), rayon)
			select centre + p;
	}

	public static IEnumerable<int> Pelure(int centre, int rayon)
	{
		return from p in PelureGenerique(Voxel.GetScalaire, rayon)
			select centre + p;
	}

	public static IEnumerable<int> Pelure(int centre, int rayon, int cote)
	{
		return from p in PelureGenerique((int x, int y, int z) => x + y * cote * cote + z * cote, rayon)
			select centre + p;
	}

	public static IEnumerable<int> CubeConcentrique(int centre, int rayon)
	{
		for (int r = 0; r <= rayon; r++)
		{
			foreach (int item in Pelure(centre, r))
			{
				yield return item;
			}
		}
	}

	public static IEnumerable<XYZ> CubeConcentrique(XYZ centre, int rayon)
	{
		for (int r = 0; r <= rayon; r++)
		{
			foreach (XYZ item in Pelure(centre, r))
			{
				yield return item;
			}
		}
	}

	public static IEnumerable<int> CubeIncremental(int coin, int taille)
	{
		return CubeIncremental(coin, taille, 1024);
	}

	public static IEnumerable<int> CubeIncremental(int coin, int taille, int cote)
	{
		int step1 = cote - taille;
		int step2 = cote * (cote - taille);
		int scalaire = coin;
		for (int i = 0; i < taille; i++)
		{
			for (int j = 0; j < taille; j++)
			{
				for (int k = 0; k < taille; k++)
				{
					yield return scalaire++;
				}
				scalaire += step1;
			}
			scalaire += step2;
		}
	}

	public static VolumeEnumerable VolumeBetween(XYZ min, XYZ max)
	{
		return new VolumeEnumerable(min, max);
	}

	public static IEnumerable<T> VolumeIncrementalGeneriqueYZX<T>(Func<int, int, int, T> makeValue, int nx, int ny, int nz)
	{
		for (int y = 0; y < ny; y++)
		{
			for (int z = 0; z < nz; z++)
			{
				for (int x = 0; x < nx; x++)
				{
					yield return makeValue(x, y, z);
				}
			}
		}
	}

	public static IEnumerable<int> CountingRange(int start, int count, int weight)
	{
		int r = start * weight;
		int i = 0;
		while (i < count)
		{
			yield return r;
			i++;
			r += weight;
		}
	}

	public static IEnumerable<int> CombineRanges(params IEnumerable<int>[] ranges)
	{
		return CombineRanges(ranges, ranges.Length);
	}

	private static IEnumerable<int> CombineRanges(IEnumerable<int>[] ranges, int n)
	{
		if (n == 0)
		{
			return ranges[0];
		}
		return from b in ranges[n - 1]
			from a in CombineRanges(ranges, n - 1)
			select a + b;
	}

	public static float Integral(Func<float, float> f, float a, float b, int nSegments = 10)
	{
		float num = 0f;
		float num2 = (b - a) / (float)nSegments;
		for (a += num2 * 0.5f; a < b; a += num2)
		{
			num += f(a) * num2;
		}
		return num;
	}

	public static float Integral(AnimationCurve curve, float a, float b, int nSegments = 10)
	{
		return Integral(curve.Evaluate, a, b, nSegments);
	}

	public static AnimationCurve IntegralCurve(AnimationCurve curve, int nKeys = 10, int nSegmentsPerKey = 10)
	{
		AnimationCurve animationCurve = new AnimationCurve();
		float num = 0f;
		float num2 = 1f / (float)(nKeys - 1);
		float num3 = 0f;
		animationCurve.AddKey(num, num3);
		for (int i = 1; i < nKeys; i++)
		{
			num3 += Integral(curve, num, num + num2, nSegmentsPerKey);
			num += num2;
			animationCurve.AddKey(num, num3);
		}
		return animationCurve;
	}

	public static Rect CurveFrame(AnimationCurve curve)
	{
		float num = float.PositiveInfinity;
		float num2 = float.NegativeInfinity;
		float num3 = float.PositiveInfinity;
		float num4 = float.NegativeInfinity;
		Keyframe[] keys = curve.keys;
		for (int i = 0; i < keys.Length; i++)
		{
			Keyframe keyframe = keys[i];
			float time = keyframe.time;
			float value = keyframe.value;
			num = Mathf.Min(num, time);
			num2 = Mathf.Max(num2, time);
			num3 = Mathf.Min(num3, value);
			num4 = Mathf.Max(num4, value);
		}
		return new Rect(num, num3, num2 - num, num4 - num3);
	}

	public static AnimationCurve ReframedCurve(AnimationCurve curve, Rect oldFrame, Rect newFrame)
	{
		AnimationCurve animationCurve = new AnimationCurve();
		Keyframe[] keys = curve.keys;
		for (int i = 0; i < keys.Length; i++)
		{
			Keyframe keyframe = keys[i];
			float time = keyframe.time;
			float value = keyframe.value;
			float time2 = newFrame.x + newFrame.width * (time - oldFrame.x) / oldFrame.width;
			float value2 = newFrame.y + newFrame.height * (value - oldFrame.y) / oldFrame.height;
			animationCurve.AddKey(time2, value2);
		}
		return animationCurve;
	}

	public static AnimationCurve ReciprocalCurve(AnimationCurve curve)
	{
		AnimationCurve animationCurve = new AnimationCurve();
		Keyframe[] keys = curve.keys;
		for (int i = 0; i < keys.Length; i++)
		{
			animationCurve.AddKey(keys[i].value, keys[i].time);
		}
		return animationCurve;
	}

	public static float ParseFloat(string e)
	{
		return float.Parse(e, CultureInfo.InvariantCulture.NumberFormat);
	}

	public static int GetBestPower(int n)
	{
		return Mathf.CeilToInt(Mathf.Log(n, 2f));
	}

	public static int GetBestSquarePower(int n)
	{
		return Mathf.CeilToInt(Mathf.Log(n, 2f) / 2f);
	}

	public static IEnumerable<Plane> GetPlanesFromBounds(Bounds bounds)
	{
		Vector3 c = bounds.center;
		Vector3 d = bounds.extents;
		yield return new Plane(Vector3.right, c - Vector3.right * d.x);
		yield return new Plane(-Vector3.right, c + Vector3.right * d.x);
		yield return new Plane(Vector3.up, c - Vector3.up * d.y);
		yield return new Plane(-Vector3.up, c + Vector3.up * d.y);
		yield return new Plane(Vector3.forward, c - Vector3.forward * d.z);
		yield return new Plane(-Vector3.forward, c + Vector3.forward * d.z);
	}

	public static Plane GetClosestPlaneOutsideBounds(Vector3 point, Bounds bounds)
	{
		float num = float.PositiveInfinity;
		Plane? plane = null;
		foreach (Plane planesFromBound in GetPlanesFromBounds(bounds))
		{
			float distanceToPoint = planesFromBound.GetDistanceToPoint(point);
			if (distanceToPoint < num)
			{
				num = distanceToPoint;
				plane = planesFromBound;
			}
		}
		return plane ?? new Plane(Vector3.forward, Vector3.zero);
	}

	public static Vector3 ClampPointTowardBounds(Vector3 point, Bounds bounds)
	{
		foreach (Plane planesFromBound in GetPlanesFromBounds(bounds))
		{
			float distanceToPoint = planesFromBound.GetDistanceToPoint(point);
			if (distanceToPoint < 0f)
			{
				Vector3 vector = (point - bounds.center).normalized * Mathf.Abs(distanceToPoint);
				Vector3 vector2 = Vector3.Project(vector, -planesFromBound.normal);
				point -= vector2;
			}
		}
		return point;
	}

	public static float InfinityToOne(float x)
	{
		return Mathf.Atan(x) * 2f / (float)Math.PI;
	}

	public static float OneToInfinity(float x)
	{
		return Mathf.Tan(x * (float)Math.PI / 2f);
	}

	public static float LogInfinityToOne(float xAbs)
	{
		float num = InfinityToOne(Mathf.Log(xAbs));
		return (num + 1f) / 2f;
	}

	public static float SemiLogInfinityToOne(float xAbs, float cross)
	{
		float num = InfinityToOne(xAbs);
		if (num <= cross)
		{
			return num;
		}
		cross = OneToInfinity(cross);
		float x = Mathf.Log(xAbs / cross) + cross;
		return InfinityToOne(x);
	}

	public static float SemiLogInfinityToOne(float xAbs)
	{
		float x = ((xAbs < 1f) ? xAbs : (1f + Mathf.Log(xAbs)));
		return InfinityToOne(x);
	}

	public static int GetScalaire(Vector3 vecteur, int cote, bool b)
	{
		int num = Mathf.FloorToInt(vecteur.x);
		int num2 = Mathf.FloorToInt(vecteur.y);
		int num3 = Mathf.FloorToInt(vecteur.z);
		return num + num2 * cote * cote + num3 * cote;
	}

	public static int GetScalaire(Vector3 vecteur)
	{
		return Voxel.GetScalaire(vecteur);
	}

	public static Vector3 GetVecteur(int nombre, int cote, bool b)
	{
		int num = nombre % cote;
		int num2 = (nombre - nombre % (cote * cote)) / (cote * cote);
		int num3 = (nombre - nombre % cote) % (cote * cote) / cote;
		return new Vector3(num, num2, num3);
	}

	public static Vector3 GetVecteur(int nombre)
	{
		return Voxel.GetVecteur(nombre);
	}

	public static void VecteurToScalaire(ref Vector3 vecteur, out int scalaire)
	{
		scalaire = (int)vecteur.x + ((int)vecteur.z << 10) + ((int)vecteur.y << 20);
	}

	public static void ScalaireToVecteur(ref Vector3 vecteur, int scalaire)
	{
		vecteur.x = scalaire & 0x3FF;
		vecteur.z = (scalaire >> 10) & 0x3FF;
		vecteur.y = (scalaire >> 20) & 0x3FF;
	}

	public static int ScalaireRebase(int nombre, int ancienCote, int nouveauCote)
	{
		int num = nombre % ancienCote;
		int num2 = (nombre - nombre % (ancienCote * ancienCote)) / (ancienCote * ancienCote);
		int num3 = (nombre - nombre % ancienCote) % (ancienCote * ancienCote) / ancienCote;
		return num + num2 * nouveauCote * nouveauCote + num3 * nouveauCote;
	}

	public static int ScalaireProjectXZ(int scalaire)
	{
		return scalaire & 0xFFFFF;
	}

	public static Vector3 GetSignedVector(int signedScalaire)
	{
		Vector3 vecteur = Voxel.GetVecteur(signedScalaire);
		if (vecteur.x >= 512f)
		{
			vecteur.x -= 1024f;
			vecteur.z += 1f;
		}
		if (vecteur.z >= 512f)
		{
			vecteur.z -= 1024f;
			vecteur.y += 1f;
		}
		if (vecteur.y >= 512f)
		{
			vecteur.y -= 1024f;
		}
		return vecteur;
	}

	public static Vector3 GetDiscreteVector(Vector3 forward)
	{
		float num = Mathf.Abs(forward.x);
		float num2 = Mathf.Abs(forward.y);
		float num3 = Mathf.Abs(forward.z);
		if (num >= num2 && num >= num3)
		{
			return Vector3.right * Mathf.Sign(forward.x);
		}
		if (num3 >= num2)
		{
			return Vector3.forward * Mathf.Sign(forward.z);
		}
		return Vector3.up * Mathf.Sign(forward.y);
	}

	public static int Signe(int nombre)
	{
		return Math.Sign(nombre);
	}

	public static int ValeurAbsolue(int nombre)
	{
		return Math.Abs(nombre);
	}

	public static int BinaryToDecimal(int[] array)
	{
		int num = 0;
		for (int i = 0; i < array.Length; i++)
		{
			num += ((array[i] == 1) ? (1 << array.Length - 1 - i) : 0);
		}
		return num;
	}

	public static int[] DecimalToBinary(int nombre)
	{
		int num = Mathf.ClosestPowerOfTwo(nombre);
		int num2 = (int)Mathf.Log(num, 2f);
		if (num > nombre)
		{
			num = (int)Mathf.Pow(2f, num2 - 1);
		}
		else
		{
			num2++;
		}
		int[] array = new int[num2];
		for (int num3 = num2; num3 > 0; num3--)
		{
			if (Mathf.Pow(2f, num3 - 1) <= (float)nombre)
			{
				nombre -= (int)Mathf.Pow(2f, num3 - 1);
				array[num2 - num3] = 1;
			}
			else
			{
				array[num2 - num3] = 0;
			}
		}
		return array;
	}

	public static bool GetBit(int nombre, int index)
	{
		if (((nombre >> index) & 1) != 0)
		{
			return true;
		}
		return false;
	}

	public static int SetBit(int nombre, int index, bool b)
	{
		int num = 1 << index;
		if (((nombre >> index) & 1) != 0)
		{
			nombre -= num;
		}
		if (b)
		{
			nombre += num;
		}
		return nombre;
	}

	public static int SetBit(byte nombre, byte index, bool b)
	{
		byte b2 = (byte)(1 << (int)index);
		if (((nombre >> (int)index) & 1) != 0)
		{
			nombre -= b2;
		}
		if (b)
		{
			nombre += b2;
		}
		return nombre;
	}

	public static Quaternion ConfigToRotation(byte config)
	{
		Quaternion result = Quaternion.identity;
		switch (config)
		{
		case 1:
			result = Quaternion.Euler(-90f, 0f, -90f);
			break;
		case 2:
			result = Quaternion.Euler(-90f, 90f, 0f);
			break;
		case 4:
			result = Quaternion.Euler(-90f, -90f, 0f);
			break;
		case 8:
			result = Quaternion.Euler(-90f, 180f, 0f);
			break;
		case 16:
			result = Quaternion.Euler(-90f, 0f, 0f);
			break;
		case 32:
			result = Quaternion.Euler(-90f, 0f, 90f);
			break;
		}
		return result;
	}

	public static Quaternion FaceIndexToRotation(byte faceIndex)
	{
		Quaternion result = Quaternion.identity;
		switch (faceIndex)
		{
		case 0:
			result = Quaternion.Euler(-90f, 0f, -90f);
			break;
		case 1:
			result = Quaternion.Euler(-90f, 90f, 0f);
			break;
		case 2:
			result = Quaternion.Euler(-90f, -90f, 0f);
			break;
		case 3:
			result = Quaternion.Euler(-90f, 180f, 0f);
			break;
		case 4:
			result = Quaternion.Euler(-90f, 0f, 0f);
			break;
		case 5:
			result = Quaternion.Euler(-90f, 0f, 90f);
			break;
		}
		return result;
	}

	public static Quaternion FaceIndexToRotationDeBase(byte faceIndex)
	{
		return faceIndex switch
		{
			0 => Quaternion.Euler(270f, 0f, 0f), 
			1 => Quaternion.Euler(0f, 0f, 0f), 
			2 => Quaternion.Euler(0f, 180f, 0f), 
			3 => Quaternion.Euler(0f, 90f, 0f), 
			4 => Quaternion.Euler(0f, 270f, 0f), 
			5 => Quaternion.Euler(90f, 0f, 0f), 
			_ => Quaternion.identity, 
		};
	}

	public static int CrossScalaire(int a, int b)
	{
		return Voxel.GetScalaire(Vector3.Cross(Voxel.GetVecteur(a), Voxel.GetVecteur(b)));
	}

	public static int CrossFaceIndex(int a, int b)
	{
		return VecteurToFaceIndex(Vector3.Cross(FaceIndexToVecteur(a), FaceIndexToVecteur(b)));
	}

	public static byte[] GetFacesVoisines(byte config)
	{
		byte[] array = new byte[4];
		switch (config)
		{
		case 1:
			array[0] = 2;
			array[1] = 8;
			array[2] = 4;
			array[3] = 16;
			break;
		case 2:
			array[0] = 1;
			array[1] = 16;
			array[2] = 32;
			array[3] = 8;
			break;
		case 4:
			array[0] = 1;
			array[1] = 8;
			array[2] = 32;
			array[3] = 16;
			break;
		case 8:
			array[0] = 1;
			array[1] = 2;
			array[2] = 32;
			array[3] = 4;
			break;
		case 16:
			array[0] = 1;
			array[1] = 4;
			array[2] = 32;
			array[3] = 2;
			break;
		case 32:
			array[0] = 4;
			array[1] = 8;
			array[2] = 1;
			array[3] = 16;
			break;
		}
		return array;
	}

	public static int BinarySearch(int[] a, int i)
	{
		int num = 0;
		int num2 = a.Length;
		int num3 = 0;
		int num4 = 0;
		while (num2 - num > 0)
		{
			num3 = Mathf.FloorToInt((num2 + num) / 2);
			num4 = a[num3];
			if (num == num3 && i > num4)
			{
				num++;
				continue;
			}
			if (i > num4)
			{
				num = num3;
				continue;
			}
			if (i < num4)
			{
				num2 = num3;
				continue;
			}
			break;
		}
		return (i == num4) ? num3 : (~num2);
	}

	public static int BinarySearch(Brix.Util.Paire.Paire[] a, int i, int Max)
	{
		int num = 0;
		int num2 = 0;
		int num3 = 0;
		while (Max - num > 0)
		{
			num2 = Mathf.FloorToInt((Max + num) / 2);
			num3 = a[num2].a;
			if (num == num2 && i > num3)
			{
				num++;
				continue;
			}
			if (i > num3)
			{
				num = num2;
				continue;
			}
			if (i < num3)
			{
				Max = num2;
				continue;
			}
			break;
		}
		return (i == num3) ? num2 : (~Max);
	}

	public static int BinarySearch(Brix.Util.Paire.Paire[] a, int i, int Max, bool test)
	{
		int num = 0;
		int num2 = 0;
		int num3 = 0;
		i = Voxel.GetXZ(i);
		while (Max - num > 0)
		{
			num2 = Mathf.FloorToInt((Max + num) / 2);
			num3 = Voxel.GetXZ(a[num2].a);
			if (num == num2 && i > num3)
			{
				num++;
				continue;
			}
			if (i > num3)
			{
				num = num2;
				continue;
			}
			if (i < num3)
			{
				Max = num2;
				continue;
			}
			break;
		}
		return (i == num3) ? num2 : (~Max);
	}

	public static byte[] GetLocalDirection(byte config)
	{
		byte[] array = new byte[6];
		switch (config)
		{
		case 1:
			array[0] = 1;
			array[1] = 2;
			array[2] = 4;
			array[3] = 8;
			array[4] = 16;
			array[5] = 32;
			break;
		case 2:
			array[0] = 2;
			array[1] = 1;
			array[2] = 32;
			array[3] = 16;
			array[4] = 8;
			array[5] = 4;
			break;
		case 4:
			array[0] = 4;
			array[1] = 1;
			array[2] = 32;
			array[3] = 8;
			array[4] = 16;
			array[5] = 2;
			break;
		case 8:
			array[0] = 8;
			array[1] = 1;
			array[2] = 32;
			array[3] = 2;
			array[4] = 4;
			array[5] = 16;
			break;
		case 16:
			array[0] = 16;
			array[1] = 1;
			array[2] = 32;
			array[3] = 4;
			array[4] = 2;
			array[5] = 8;
			break;
		case 32:
			array[0] = 32;
			array[1] = 4;
			array[2] = 2;
			array[3] = 8;
			array[4] = 16;
			array[5] = 1;
			break;
		}
		return array;
	}

	public static float Extrapoler(float x, float y, float t)
	{
		return x + (y - x) * t;
	}

	public static float EaseIn(float x, float y, float t)
	{
		return Mathf.Lerp(x, y, t * t);
	}

	public static float EaseOut(float x, float y, float t)
	{
		return Mathf.Lerp(x, y, t * (2f - t));
	}

	public static float EaseInOut(float x, float y, float t)
	{
		return Mathf.Lerp(x, y, t * t * (3f - 2f * t));
	}

	public static float HardInOut(float x, float y, float t)
	{
		return Mathf.Lerp(x, y, t * (1f - 2f * t) * (1f - 2f * t));
	}

	public static float BounceIn(float x, float y, float t)
	{
		float num = 0.6f;
		float num2 = 0.85f;
		return x + (y - x) * ((t < num) ? EaseInOut(0f, 1.25f, (t - 0f) / (num - 0f)) : ((t < num2) ? EaseInOut(1.25f, 0.85f, (t - num) / (num2 - num)) : EaseInOut(0.85f, 1f, (t - num2) / (1f - num2))));
	}

	public static float StepIn(float x, float y, float t)
	{
		return (t <= 0f) ? x : y;
	}

	public static float StepOut(float x, float y, float t)
	{
		return (t >= 1f) ? y : x;
	}

	public static Func<float, float, float, float> CurveLerp(AnimationCurve curve)
	{
		return (float x, float y, float t) => x + (y - x) * curve.Evaluate(t);
	}

	public static int De(int i, int ajout)
	{
		return Mathf.FloorToInt(UnityEngine.Random.value * (float)ajout);
	}

	public static int De(int i, int ajout, float chance)
	{
		return (UnityEngine.Random.value < chance) ? (De(i, ajout, chance) + 4) : i;
	}

	public static int DicePool(int qte, float chance)
	{
		int num = 0;
		for (int i = 0; i < qte; i++)
		{
			num += ((UnityEngine.Random.value < chance) ? 1 : 0);
		}
		return num;
	}

	public static float SmoothStepSqrt(float from, float to, float t2)
	{
		return from + (to - from) * t2 * (2f - t2);
	}

	public static Vector3 RotateAround(Vector3 point, Vector3 axe, float degre)
	{
		Vector3 target = Vector3.Cross(axe - point, -Vector3.up);
		return point + Vector3.RotateTowards(axe - point, target, degre, 0f);
	}

	public static float Arrondir(float nombre, int decimales)
	{
		float num = Mathf.Pow(10f, decimales);
		return Mathf.Round(nombre * num) / num;
	}

	public static T Max<T>(IList<T> list) where T : IComparable<T>
	{
		return Max(list, (T a, T b) => a.CompareTo(b) > 0);
	}

	public static T Max<T>(IEnumerable<T> list) where T : IComparable<T>
	{
		return Max(list, (T a, T b) => a.CompareTo(b) > 0);
	}

	public static T Max<T>(IList<T> list, Func<T, T, bool> compare)
	{
		if (list.Count == 0)
		{
			return default(T);
		}
		T val = list[0];
		for (int i = 1; i < list.Count; i++)
		{
			T val2 = list[i];
			if (compare(val2, val))
			{
				val = val2;
			}
		}
		return val;
	}

	public static T Max<T>(IEnumerable<T> list, Func<T, T, bool> compare)
	{
		IEnumerator<T> enumerator = list.GetEnumerator();
		if (!enumerator.MoveNext())
		{
			return default(T);
		}
		T val = enumerator.Current;
		while (enumerator.MoveNext())
		{
			T current = enumerator.Current;
			if (compare(current, val))
			{
				val = current;
			}
		}
		return val;
	}

	public static T Max<T, TComp>(IList<T> list, Func<T, TComp> getComparable) where TComp : IComparable<TComp>
	{
		if (list.Count == 0)
		{
			return default(T);
		}
		T val = list[0];
		TComp other = getComparable(val);
		for (int i = 1; i < list.Count; i++)
		{
			T val2 = list[i];
			TComp val3 = getComparable(val2);
			if (val3.CompareTo(other) > 0)
			{
				val = val2;
				other = val3;
			}
		}
		return val;
	}

	public static T Max<T, TComp>(IEnumerable<T> list, Func<T, TComp> getComparable) where TComp : IComparable<TComp>
	{
		IEnumerator<T> enumerator = list.GetEnumerator();
		if (!enumerator.MoveNext())
		{
			return default(T);
		}
		T val = enumerator.Current;
		TComp other = getComparable(val);
		while (enumerator.MoveNext())
		{
			T current = enumerator.Current;
			TComp val2 = getComparable(current);
			if (val2.CompareTo(other) > 0)
			{
				val = current;
				other = val2;
			}
		}
		return val;
	}

	public static float Max(float[] liste)
	{
		int num = ArgMax(liste);
		return liste[num];
	}

	public static T Min<T>(IList<T> list) where T : IComparable<T>
	{
		return Max(list, (T a, T b) => a.CompareTo(b) < 0);
	}

	public static T Min<T>(IEnumerable<T> list) where T : IComparable<T>
	{
		return Max(list, (T a, T b) => a.CompareTo(b) < 0);
	}

	public static T Min<T>(IList<T> list, Func<T, T, bool> compare)
	{
		if (list.Count == 0)
		{
			return default(T);
		}
		T val = list[0];
		for (int i = 1; i < list.Count; i++)
		{
			T val2 = list[i];
			if (compare(val, val2))
			{
				val = val2;
			}
		}
		return val;
	}

	public static T Min<T>(IEnumerable<T> list, Func<T, T, bool> compare)
	{
		IEnumerator<T> enumerator = list.GetEnumerator();
		if (!enumerator.MoveNext())
		{
			return default(T);
		}
		T val = enumerator.Current;
		while (enumerator.MoveNext())
		{
			T current = enumerator.Current;
			if (compare(val, current))
			{
				val = current;
			}
		}
		return val;
	}

	public static T Min<T, TComp>(IList<T> list, Func<T, TComp> getComparable) where TComp : IComparable<TComp>
	{
		if (list.Count == 0)
		{
			return default(T);
		}
		T val = list[0];
		TComp other = getComparable(val);
		for (int i = 1; i < list.Count; i++)
		{
			T val2 = list[i];
			TComp val3 = getComparable(val2);
			if (val3.CompareTo(other) < 0)
			{
				val = val2;
				other = val3;
			}
		}
		return val;
	}

	public static T Min<T, TComp>(IEnumerable<T> list, Func<T, TComp> getComparable) where TComp : IComparable<TComp>
	{
		IEnumerator<T> enumerator = list.GetEnumerator();
		if (!enumerator.MoveNext())
		{
			return default(T);
		}
		T val = enumerator.Current;
		TComp other = getComparable(val);
		while (enumerator.MoveNext())
		{
			T current = enumerator.Current;
			TComp val2 = getComparable(current);
			if (val2.CompareTo(other) < 0)
			{
				val = current;
				other = val2;
			}
		}
		return val;
	}

	public static int ArgMax<T, TComp>(IList<T> list, Func<T, TComp> getComparable) where TComp : IComparable<TComp>
	{
		int result = 0;
		TComp other = getComparable(list[0]);
		for (int i = 1; i < list.Count; i++)
		{
			TComp val = getComparable(list[i]);
			if (val.CompareTo(other) > 0)
			{
				result = i;
				other = val;
			}
		}
		return result;
	}

	public static int ArgMax<T>(IList<T> list) where T : IComparable<T>
	{
		int num = 0;
		for (int i = 1; i < list.Count; i++)
		{
			if (list[i].CompareTo(list[num]) > 0)
			{
				num = i;
			}
		}
		return num;
	}

	public static int ArgMax(IList<int> list)
	{
		int num = 0;
		for (int i = 1; i < list.Count; i++)
		{
			if (list[i] > list[num])
			{
				num = i;
			}
		}
		return num;
	}

	public static int ArgMax(IList<float> list)
	{
		int num = 0;
		for (int i = 1; i < list.Count; i++)
		{
			if (list[i] > list[num])
			{
				num = i;
			}
		}
		return num;
	}

	public static int ArgMax(params int[] list)
	{
		return ArgMax((IList<int>)list);
	}

	public static int ArgMax(params float[] list)
	{
		return ArgMax((IList<float>)list);
	}

	public static int ArgMin<T, TComp>(IList<T> list, Func<T, TComp> getComparable) where TComp : IComparable<TComp>
	{
		int result = 0;
		TComp other = getComparable(list[0]);
		for (int i = 1; i < list.Count; i++)
		{
			TComp val = getComparable(list[i]);
			if (val.CompareTo(other) < 0)
			{
				result = i;
				other = val;
			}
		}
		return result;
	}

	public static int ArgMin<T>(IList<T> list) where T : IComparable<T>
	{
		int num = 0;
		for (int i = 1; i < list.Count; i++)
		{
			if (list[i].CompareTo(list[num]) < 0)
			{
				num = i;
			}
		}
		return num;
	}

	public static int ArgMin(IList<int> list)
	{
		int num = 0;
		for (int i = 1; i < list.Count; i++)
		{
			if (list[i] < list[num])
			{
				num = i;
			}
		}
		return num;
	}

	public static int ArgMin(IList<float> list)
	{
		int num = 0;
		for (int i = 1; i < list.Count; i++)
		{
			if (list[i] < list[num])
			{
				num = i;
			}
		}
		return num;
	}

	public static int ArgMin(params int[] list)
	{
		return ArgMin((IList<int>)list);
	}

	public static int ArgMin(params float[] list)
	{
		return ArgMin((IList<float>)list);
	}

	public static float LerpInverse(float a, float b, float interp)
	{
		if (interp == a)
		{
			return 0f;
		}
		float num = interp - a;
		float num2 = b - interp;
		return num / (num + num2);
	}

	public static float LerpInverse(Vector3 a, Vector3 b, Vector3 interp)
	{
		Vector3 vector = a - b;
		vector = new Vector3(Mathf.Abs(vector.x), Mathf.Abs(vector.y), Mathf.Abs(vector.z));
		if (vector.x >= vector.y && vector.x >= vector.z)
		{
			return LerpInverse(a.x, b.x, interp.x);
		}
		if (vector.y >= vector.z)
		{
			return LerpInverse(a.y, b.y, interp.y);
		}
		return LerpInverse(a.z, b.z, interp.z);
	}

	public static Vector3[] GetVecteurs3(IEnumerable floats)
	{
		Classeur<Vector3> classeur = new Classeur<Vector3>();
		IEnumerator enumerator = floats.GetEnumerator();
		while (enumerator.MoveNext())
		{
			float x = (float)enumerator.Current;
			enumerator.MoveNext();
			float y = (float)enumerator.Current;
			enumerator.MoveNext();
			float z = (float)enumerator.Current;
			classeur.Add(new Vector3(x, y, z));
		}
		return classeur;
	}

	public static Vector2[] GetVecteurs2(IEnumerable floats)
	{
		Classeur<Vector2> classeur = new Classeur<Vector2>();
		IEnumerator enumerator = floats.GetEnumerator();
		while (enumerator.MoveNext())
		{
			float x = (float)enumerator.Current;
			enumerator.MoveNext();
			float y = (float)enumerator.Current;
			classeur.Add(new Vector2(x, y));
		}
		return classeur;
	}

	public static float[] GetFloats(IEnumerable vecteurs)
	{
		Classeur<float> classeur = new Classeur<float>();
		IEnumerator enumerator = vecteurs.GetEnumerator();
		if (!enumerator.MoveNext())
		{
			return null;
		}
		if (enumerator.Current.GetType() == typeof(Vector3))
		{
			do
			{
				Vector3 vector = (Vector3)enumerator.Current;
				classeur.Add(vector.x);
				classeur.Add(vector.y);
				classeur.Add(vector.z);
			}
			while (enumerator.MoveNext());
		}
		else if (enumerator.Current.GetType() == typeof(Vector2))
		{
			do
			{
				Vector2 vector2 = (Vector2)enumerator.Current;
				classeur.Add(vector2.x);
				classeur.Add(vector2.y);
			}
			while (enumerator.MoveNext());
		}
		return classeur;
	}

	public static int[] GetInts(IEnumerable donnees)
	{
		IntClasse intClasse = new IntClasse();
		foreach (object donnee in donnees)
		{
			intClasse.Add((donnee is Vector3) ? GetScalaire((Vector3)donnee) : ((int)donnee));
		}
		return intClasse;
	}

	public static Vector3 ScreenToPerceivedPoint(Vector3 screenPos)
	{
		float num = Screen.width;
		float num2 = Screen.height;
		float num3 = Mathf.Min(num, num2);
		return new Vector3((2f * screenPos.x - num) / num3, (2f * screenPos.y - num2) / num3, 0f);
	}

	public static int Combinaisons(int j, int n)
	{
		int num = 0;
		int num2 = 0;
		int num3 = 1;
		num2 = Mathf.Max(j, n - j);
		for (num = num2 + 1; num <= n; num++)
		{
			num3 = num3 * num / (num - num2);
		}
		return num3;
	}

	public static int[] Combinaisons(int n)
	{
		int num = 0;
		int num2 = 1;
		int[] array = new int[n + 1];
		array[0] = num2;
		int num3 = n + 1;
		for (num = 1; num < num3; num++)
		{
			num2 = (array[num] = num2 * (num3 - num) / num);
		}
		return array;
	}

	public static float PseudoRandom(int i)
	{
		UnityEngine.Random.State state = UnityEngine.Random.state;
		UnityEngine.Random.InitState(i);
		float value = UnityEngine.Random.value;
		UnityEngine.Random.state = state;
		return value;
	}

	public static float PseudoRandom_Fast(int i)
	{
		UnityEngine.Random.InitState(i);
		return UnityEngine.Random.value;
	}

	public static int RandomInt(int max)
	{
		if (max <= 0)
		{
			return 0;
		}
		float f = UnityEngine.Random.value * (1f * (float)max);
		int num = Mathf.FloorToInt(f);
		if (num == max)
		{
			num = RandomInt(max);
		}
		return num;
	}
}
