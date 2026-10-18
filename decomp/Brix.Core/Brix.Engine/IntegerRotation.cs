using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Brix.Engine;

[Serializable]
public struct IntegerRotation : IComparable<IntegerRotation>, IEquatable<IntegerRotation>
{
	public int x;

	public int y;

	public int z;

	public int w;

	public static readonly IntegerRotation Identity = FromQuaternion(Quaternion.identity);

	public static readonly IntegerRotation Forward = FromQuaternion(Quaternion.Euler(0f, 0f, 0f));

	public static readonly IntegerRotation Right = FromQuaternion(Quaternion.Euler(0f, 90f, 0f));

	public static readonly IntegerRotation Back = FromQuaternion(Quaternion.Euler(0f, 180f, 0f));

	public static readonly IntegerRotation Left = FromQuaternion(Quaternion.Euler(0f, 270f, 0f));

	public static readonly IntegerRotation Up = FromQuaternion(Quaternion.Euler(270f, 0f, 0f));

	public static readonly IntegerRotation Down = FromQuaternion(Quaternion.Euler(90f, 0f, 0f));

	public static readonly IntegerRotation[] Distinct = GetAllDistinctRotations();

	private static readonly int[] HashCodeIndices = GetCanonicalIndicesByHashCode();

	public static readonly int[,] IMulJ = new int[24, 24]
	{
		{
			0, 1, 2, 3, 4, 5, 6, 7, 8, 9,
			10, 11, 12, 13, 14, 15, 16, 17, 18, 19,
			20, 21, 22, 23
		},
		{
			1, 2, 3, 0, 21, 22, 23, 20, 11, 8,
			9, 10, 17, 18, 19, 16, 5, 6, 7, 4,
			13, 14, 15, 12
		},
		{
			2, 3, 0, 1, 14, 15, 12, 13, 10, 11,
			8, 9, 6, 7, 4, 5, 22, 23, 20, 21,
			18, 19, 16, 17
		},
		{
			3, 0, 1, 2, 19, 16, 17, 18, 9, 10,
			11, 8, 23, 20, 21, 22, 15, 12, 13, 14,
			7, 4, 5, 6
		},
		{
			4, 5, 6, 7, 8, 9, 10, 11, 12, 13,
			14, 15, 0, 1, 2, 3, 19, 16, 17, 18,
			21, 22, 23, 20
		},
		{
			5, 6, 7, 4, 22, 23, 20, 21, 15, 12,
			13, 14, 16, 17, 18, 19, 9, 10, 11, 8,
			1, 2, 3, 0
		},
		{
			6, 7, 4, 5, 2, 3, 0, 1, 14, 15,
			12, 13, 10, 11, 8, 9, 23, 20, 21, 22,
			17, 18, 19, 16
		},
		{
			7, 4, 5, 6, 18, 19, 16, 17, 13, 14,
			15, 12, 20, 21, 22, 23, 3, 0, 1, 2,
			11, 8, 9, 10
		},
		{
			8, 9, 10, 11, 12, 13, 14, 15, 0, 1,
			2, 3, 4, 5, 6, 7, 18, 19, 16, 17,
			22, 23, 20, 21
		},
		{
			9, 10, 11, 8, 23, 20, 21, 22, 3, 0,
			1, 2, 19, 16, 17, 18, 13, 14, 15, 12,
			5, 6, 7, 4
		},
		{
			10, 11, 8, 9, 6, 7, 4, 5, 2, 3,
			0, 1, 14, 15, 12, 13, 20, 21, 22, 23,
			16, 17, 18, 19
		},
		{
			11, 8, 9, 10, 17, 18, 19, 16, 1, 2,
			3, 0, 21, 22, 23, 20, 7, 4, 5, 6,
			15, 12, 13, 14
		},
		{
			12, 13, 14, 15, 0, 1, 2, 3, 4, 5,
			6, 7, 8, 9, 10, 11, 17, 18, 19, 16,
			23, 20, 21, 22
		},
		{
			13, 14, 15, 12, 20, 21, 22, 23, 7, 4,
			5, 6, 18, 19, 16, 17, 1, 2, 3, 0,
			9, 10, 11, 8
		},
		{
			14, 15, 12, 13, 10, 11, 8, 9, 6, 7,
			4, 5, 2, 3, 0, 1, 21, 22, 23, 20,
			19, 16, 17, 18
		},
		{
			15, 12, 13, 14, 16, 17, 18, 19, 5, 6,
			7, 4, 22, 23, 20, 21, 11, 8, 9, 10,
			3, 0, 1, 2
		},
		{
			16, 17, 18, 19, 5, 6, 7, 4, 22, 23,
			20, 21, 15, 12, 13, 14, 10, 11, 8, 9,
			0, 1, 2, 3
		},
		{
			17, 18, 19, 16, 1, 2, 3, 0, 21, 22,
			23, 20, 11, 8, 9, 10, 6, 7, 4, 5,
			12, 13, 14, 15
		},
		{
			18, 19, 16, 17, 13, 14, 15, 12, 20, 21,
			22, 23, 7, 4, 5, 6, 2, 3, 0, 1,
			8, 9, 10, 11
		},
		{
			19, 16, 17, 18, 9, 10, 11, 8, 23, 20,
			21, 22, 3, 0, 1, 2, 14, 15, 12, 13,
			4, 5, 6, 7
		},
		{
			20, 21, 22, 23, 7, 4, 5, 6, 18, 19,
			16, 17, 13, 14, 15, 12, 0, 1, 2, 3,
			10, 11, 8, 9
		},
		{
			21, 22, 23, 20, 11, 8, 9, 10, 17, 18,
			19, 16, 1, 2, 3, 0, 4, 5, 6, 7,
			14, 15, 12, 13
		},
		{
			22, 23, 20, 21, 15, 12, 13, 14, 16, 17,
			18, 19, 5, 6, 7, 4, 8, 9, 10, 11,
			2, 3, 0, 1
		},
		{
			23, 20, 21, 22, 3, 0, 1, 2, 19, 16,
			17, 18, 9, 10, 11, 8, 12, 13, 14, 15,
			6, 7, 4, 5
		}
	};

	public static readonly Func<XYZ, XYZ>[] CanonicalFormulas = new Func<XYZ, XYZ>[24]
	{
		Rot000, Rot001, Rot002, Rot003, Rot010, Rot011, Rot012, Rot013, Rot020, Rot021,
		Rot022, Rot023, Rot030, Rot031, Rot032, Rot033, Rot100, Rot130, Rot120, Rot110,
		Rot300, Rot310, Rot320, Rot330
	};

	public static readonly Func<Vector3, Vector3>[] CanonicalVFormulas = new Func<Vector3, Vector3>[24]
	{
		VRot000, VRot001, VRot002, VRot003, VRot010, VRot011, VRot012, VRot013, VRot020, VRot021,
		VRot022, VRot023, VRot030, VRot031, VRot032, VRot033, VRot100, VRot130, VRot120, VRot110,
		VRot300, VRot310, VRot320, VRot330
	};

	public const float H = 0.5f;

	public const float S = 0.70710677f;

	public IntegerRotation Inverse
	{
		get
		{
			IntegerRotation integerRotation = new IntegerRotation
			{
				w = w,
				x = -x,
				y = -y,
				z = -z
			};
			return integerRotation.Canonicalized;
		}
	}

	public int SqrMagnitude => w * w + x * x + y * y + z * z;

	public IntegerRotation Canonicalized
	{
		get
		{
			int rotationFactor = GetRotationFactor(this);
			return new IntegerRotation
			{
				x = x / rotationFactor,
				y = y / rotationFactor,
				z = z / rotationFactor,
				w = w / rotationFactor
			};
		}
	}

	public int CanonicalIndex => HashCodeIndices[GetHashCode()];

	public bool Equals(IntegerRotation other)
	{
		return this == other;
	}

	public override bool Equals(object obj)
	{
		if (obj == null)
		{
			return false;
		}
		return obj is IntegerRotation && Equals((IntegerRotation)obj);
	}

	public override int GetHashCode()
	{
		return 40 + w + x * 3 + y * 9 + z * 27;
	}

	public static float GetQuaternionFactor(Quaternion q)
	{
		if (q.x * q.x >= 0.2f)
		{
			return q.x;
		}
		if (q.y * q.y >= 0.2f)
		{
			return q.y;
		}
		if (q.z * q.z >= 0.2f)
		{
			return q.z;
		}
		return q.w;
	}

	public static int GetRotationFactor(IntegerRotation a)
	{
		if (a.x != 0)
		{
			return a.x;
		}
		if (a.y != 0)
		{
			return a.y;
		}
		if (a.z != 0)
		{
			return a.z;
		}
		return a.w;
	}

	public static Quaternion GetCanonicalized(Quaternion q)
	{
		return Quaternion.Euler(q.eulerAngles);
	}

	public static IntegerRotation FromQuaternion(Quaternion q)
	{
		float quaternionFactor = GetQuaternionFactor(q);
		IntegerRotation integerRotation = new IntegerRotation
		{
			x = Mathf.RoundToInt(q.x / quaternionFactor),
			y = Mathf.RoundToInt(q.y / quaternionFactor),
			z = Mathf.RoundToInt(q.z / quaternionFactor),
			w = Mathf.RoundToInt(q.w / quaternionFactor)
		};
		return integerRotation.Canonicalized;
	}

	public Quaternion ToQuaternion()
	{
		Vector4 normalized = new Vector4(x, y, z, w).normalized;
		return new Quaternion(normalized.x, normalized.y, normalized.z, normalized.w);
	}

	public int CompareTo(IntegerRotation other)
	{
		return w - other.w + (x - other.x) * 5 + (y - other.y) * 25 + (z - other.z) * 125;
	}

	public override string ToString()
	{
		return x + ", " + y + ", " + z + ", " + w;
	}

	public static IntegerRotation Mul4x4(IntegerRotation a, IntegerRotation b)
	{
		return new IntegerRotation
		{
			w = a.w * b.w - a.x * b.x - a.y * b.y - a.z * b.z,
			x = a.w * b.x + a.x * b.w + a.y * b.z - a.z * b.y,
			y = a.w * b.y - a.x * b.z + a.y * b.w + a.z * b.x,
			z = a.w * b.z + a.x * b.y - a.y * b.x + a.z * b.w
		};
	}

	public static XYZ Mul4x3(IntegerRotation a, XYZ p)
	{
		IntegerRotation integerRotation = new IntegerRotation
		{
			w = a.w - a.x * p.x - a.y * p.y - a.z * p.z,
			x = a.x + a.w * p.x + a.y * p.z - a.z * p.y,
			y = a.y + a.w * p.y - a.x * p.z + a.z * p.x,
			z = a.z + a.w * p.z + a.x * p.y - a.y * p.x
		};
		return new XYZ
		{
			x = a.w * integerRotation.x - a.x * integerRotation.w + a.y * integerRotation.z - a.z * integerRotation.y,
			y = a.w * integerRotation.y - a.x * integerRotation.z - a.y * integerRotation.w + a.z * integerRotation.x,
			z = a.w * integerRotation.z + a.x * integerRotation.y - a.y * integerRotation.x - a.z * integerRotation.w
		};
	}

	public static IntegerRotation operator *(IntegerRotation a, IntegerRotation b)
	{
		return Mul4x4(a, b).Canonicalized;
	}

	public static XYZ operator *(IntegerRotation a, XYZ b)
	{
		return Mul4x3(a, b) / a.SqrMagnitude;
	}

	public static Quaternion operator *(IntegerRotation a, Quaternion b)
	{
		return a.ToQuaternion() * b;
	}

	public static Vector3 operator *(IntegerRotation a, Vector3 b)
	{
		return a.ToQuaternion() * b;
	}

	public static bool operator ==(IntegerRotation a, IntegerRotation b)
	{
		return a.x == b.x && a.y == b.y && a.z == b.z && a.w == b.w;
	}

	public static bool operator !=(IntegerRotation a, IntegerRotation b)
	{
		return a.x != b.x || a.y != b.y || a.z != b.z || a.w != b.w;
	}

	private static int[] GetCanonicalIndicesByHashCode()
	{
		int[] array = new int[81];
		for (int i = 0; i < Distinct.Length; i++)
		{
			array[Distinct[i].GetHashCode()] = i;
		}
		return array;
	}

	private static IntegerRotation[] GetAllDistinctRotations()
	{
		return GetAllEulerQuaternions().Select(FromQuaternion).Distinct().ToArray();
	}

	private static Quaternion[] GetAllEulerQuaternions()
	{
		List<Quaternion> list = new List<Quaternion>();
		for (int i = 0; i < 360; i += 90)
		{
			for (int j = 0; j < 360; j += 90)
			{
				for (int k = 0; k < 360; k += 90)
				{
					list.Add(Quaternion.Euler(i, j, k));
				}
			}
		}
		return list.ToArray();
	}

	public static string TestMul()
	{
		if (Distinct.Length != 24)
		{
			return "Distinct.Length = " + Distinct.Length + " != " + 24;
		}
		IntegerRotation[] distinct = Distinct;
		for (int i = 0; i < distinct.Length; i++)
		{
			IntegerRotation integerRotation = distinct[i];
			if (!Distinct.Contains(integerRotation.Inverse))
			{
				return string.Concat("!Distinct.Contains((", integerRotation, ").Inverse = (", integerRotation.Inverse, "))");
			}
		}
		IntegerRotation[] distinct2 = Distinct;
		for (int j = 0; j < distinct2.Length; j++)
		{
			IntegerRotation integerRotation2 = distinct2[j];
			if (integerRotation2 != FromQuaternion(integerRotation2.ToQuaternion()))
			{
				return string.Concat("FromQuaternion(", integerRotation2, ").ToQuaternion()");
			}
		}
		IntegerRotation[] distinct3 = Distinct;
		for (int k = 0; k < distinct3.Length; k++)
		{
			IntegerRotation integerRotation3 = distinct3[k];
			if (GetCanonicalized(integerRotation3.Inverse.ToQuaternion()) != GetCanonicalized(Quaternion.Inverse(integerRotation3.ToQuaternion())))
			{
				return string.Concat("(", integerRotation3, ").Inverse = (", integerRotation3.Inverse, ") != ", integerRotation3.ToQuaternion(), ".Inverse = ", Quaternion.Inverse(integerRotation3.ToQuaternion()));
			}
		}
		IntegerRotation[] distinct4 = Distinct;
		for (int l = 0; l < distinct4.Length; l++)
		{
			IntegerRotation integerRotation4 = distinct4[l];
			IntegerRotation[] distinct5 = Distinct;
			for (int m = 0; m < distinct5.Length; m++)
			{
				IntegerRotation integerRotation5 = distinct5[m];
				if (GetCanonicalized((integerRotation4 * integerRotation5).ToQuaternion()) != GetCanonicalized(integerRotation4.ToQuaternion() * integerRotation5.ToQuaternion()))
				{
					return string.Concat("(", integerRotation4, ") * (", integerRotation5, ") = (", integerRotation4 * integerRotation5, ") != ", integerRotation4.ToQuaternion(), " * ", integerRotation5.ToQuaternion(), " = ", integerRotation4.ToQuaternion() * integerRotation5.ToQuaternion());
				}
			}
		}
		XYZ xYZ = new XYZ(2, 3, 5);
		IntegerRotation[] distinct6 = Distinct;
		for (int n = 0; n < distinct6.Length; n++)
		{
			IntegerRotation integerRotation6 = distinct6[n];
			if ((integerRotation6 * xYZ).ToVector3() != integerRotation6.ToQuaternion() * xYZ.ToVector3())
			{
				return string.Concat("(", integerRotation6, ") * (", xYZ, ") = (", integerRotation6 * xYZ, ") != ", integerRotation6.ToQuaternion(), " * ", xYZ.ToVector3(), " = ", integerRotation6.ToQuaternion() * xYZ.ToVector3());
			}
		}
		for (int num = 0; num < Distinct.Length; num++)
		{
			for (int num2 = 0; num2 < Distinct.Length; num2++)
			{
				if (num != num2)
				{
					IntegerRotation integerRotation7 = Distinct[num];
					IntegerRotation integerRotation8 = Distinct[num2];
					if (integerRotation7.GetHashCode() == integerRotation8.GetHashCode())
					{
						return string.Concat("(", integerRotation7, ").GetHashCode() = ", integerRotation7.GetHashCode(), " != (", integerRotation8, ").GetHashCode() = ", integerRotation8.GetHashCode());
					}
				}
			}
		}
		IntegerRotation[] distinct7 = Distinct;
		for (int num3 = 0; num3 < distinct7.Length; num3++)
		{
			IntegerRotation integerRotation9 = distinct7[num3];
			if (integerRotation9 != Distinct[integerRotation9.CanonicalIndex])
			{
				return string.Concat("Distinct[(", integerRotation9, ").CanonicalIndex = ", integerRotation9.CanonicalIndex, "] != (", Distinct[integerRotation9.CanonicalIndex], ")");
			}
		}
		return "All Good";
	}

	public static XYZ Rot000(XYZ a)
	{
		return new XYZ(a.x, a.y, a.z);
	}

	public static XYZ Rot001(XYZ a)
	{
		return new XYZ(-a.y, a.x, a.z);
	}

	public static XYZ Rot002(XYZ a)
	{
		return new XYZ(-a.x, -a.y, a.z);
	}

	public static XYZ Rot003(XYZ a)
	{
		return new XYZ(a.y, -a.x, a.z);
	}

	public static XYZ Rot010(XYZ a)
	{
		return new XYZ(-a.z, a.y, a.x);
	}

	public static XYZ Rot011(XYZ a)
	{
		return new XYZ(-a.z, a.x, -a.y);
	}

	public static XYZ Rot012(XYZ a)
	{
		return new XYZ(a.y, a.x, -a.z);
	}

	public static XYZ Rot013(XYZ a)
	{
		return new XYZ(a.z, a.x, a.y);
	}

	public static XYZ Rot020(XYZ a)
	{
		return new XYZ(-a.x, a.y, -a.z);
	}

	public static XYZ Rot021(XYZ a)
	{
		return new XYZ(-a.z, -a.y, -a.x);
	}

	public static XYZ Rot022(XYZ a)
	{
		return new XYZ(a.x, -a.y, -a.z);
	}

	public static XYZ Rot023(XYZ a)
	{
		return new XYZ(a.z, -a.y, a.x);
	}

	public static XYZ Rot030(XYZ a)
	{
		return new XYZ(a.z, a.y, -a.x);
	}

	public static XYZ Rot031(XYZ a)
	{
		return new XYZ(-a.z, -a.x, a.y);
	}

	public static XYZ Rot032(XYZ a)
	{
		return new XYZ(-a.y, -a.x, -a.z);
	}

	public static XYZ Rot033(XYZ a)
	{
		return new XYZ(a.z, -a.x, -a.y);
	}

	public static XYZ Rot100(XYZ a)
	{
		return new XYZ(a.x, -a.z, a.y);
	}

	public static XYZ Rot130(XYZ a)
	{
		return new XYZ(a.y, -a.z, -a.x);
	}

	public static XYZ Rot120(XYZ a)
	{
		return new XYZ(-a.x, -a.z, -a.y);
	}

	public static XYZ Rot110(XYZ a)
	{
		return new XYZ(-a.y, -a.z, a.x);
	}

	public static XYZ Rot300(XYZ a)
	{
		return new XYZ(a.x, a.z, -a.y);
	}

	public static XYZ Rot310(XYZ a)
	{
		return new XYZ(a.y, a.z, a.x);
	}

	public static XYZ Rot320(XYZ a)
	{
		return new XYZ(-a.x, a.z, a.y);
	}

	public static XYZ Rot330(XYZ a)
	{
		return new XYZ(-a.y, a.z, -a.x);
	}

	public static Vector3 VRot000(Vector3 a)
	{
		return new Vector3(a.x, a.y, a.z);
	}

	public static Vector3 VRot001(Vector3 a)
	{
		return new Vector3(0f - a.y, a.x, a.z);
	}

	public static Vector3 VRot002(Vector3 a)
	{
		return new Vector3(0f - a.x, 0f - a.y, a.z);
	}

	public static Vector3 VRot003(Vector3 a)
	{
		return new Vector3(a.y, 0f - a.x, a.z);
	}

	public static Vector3 VRot010(Vector3 a)
	{
		return new Vector3(0f - a.z, a.y, a.x);
	}

	public static Vector3 VRot011(Vector3 a)
	{
		return new Vector3(0f - a.z, a.x, 0f - a.y);
	}

	public static Vector3 VRot012(Vector3 a)
	{
		return new Vector3(a.y, a.x, 0f - a.z);
	}

	public static Vector3 VRot013(Vector3 a)
	{
		return new Vector3(a.z, a.x, a.y);
	}

	public static Vector3 VRot020(Vector3 a)
	{
		return new Vector3(0f - a.x, a.y, 0f - a.z);
	}

	public static Vector3 VRot021(Vector3 a)
	{
		return new Vector3(0f - a.z, 0f - a.y, 0f - a.x);
	}

	public static Vector3 VRot022(Vector3 a)
	{
		return new Vector3(a.x, 0f - a.y, 0f - a.z);
	}

	public static Vector3 VRot023(Vector3 a)
	{
		return new Vector3(a.z, 0f - a.y, a.x);
	}

	public static Vector3 VRot030(Vector3 a)
	{
		return new Vector3(a.z, a.y, 0f - a.x);
	}

	public static Vector3 VRot031(Vector3 a)
	{
		return new Vector3(0f - a.z, 0f - a.x, a.y);
	}

	public static Vector3 VRot032(Vector3 a)
	{
		return new Vector3(0f - a.y, 0f - a.x, 0f - a.z);
	}

	public static Vector3 VRot033(Vector3 a)
	{
		return new Vector3(a.z, 0f - a.x, 0f - a.y);
	}

	public static Vector3 VRot100(Vector3 a)
	{
		return new Vector3(a.x, 0f - a.z, a.y);
	}

	public static Vector3 VRot130(Vector3 a)
	{
		return new Vector3(a.y, 0f - a.z, 0f - a.x);
	}

	public static Vector3 VRot120(Vector3 a)
	{
		return new Vector3(0f - a.x, 0f - a.z, 0f - a.y);
	}

	public static Vector3 VRot110(Vector3 a)
	{
		return new Vector3(0f - a.y, 0f - a.z, a.x);
	}

	public static Vector3 VRot300(Vector3 a)
	{
		return new Vector3(a.x, a.z, 0f - a.y);
	}

	public static Vector3 VRot310(Vector3 a)
	{
		return new Vector3(a.y, a.z, a.x);
	}

	public static Vector3 VRot320(Vector3 a)
	{
		return new Vector3(0f - a.x, a.z, a.y);
	}

	public static Vector3 VRot330(Vector3 a)
	{
		return new Vector3(0f - a.y, a.z, 0f - a.x);
	}

	public static Quaternion QRot000(Quaternion q)
	{
		return new Quaternion(q.x, q.y, q.z, q.w);
	}

	public static Quaternion QRot001(Quaternion q)
	{
		return new Quaternion((q.x - q.y) * 0.70710677f, (q.x + q.y) * 0.70710677f, (q.z + q.w) * 0.70710677f, (q.z - q.w) * 0.70710677f);
	}

	public static Quaternion QRot002(Quaternion q)
	{
		return new Quaternion(0f - q.y, q.x, q.w, 0f - q.z);
	}

	public static Quaternion QRot003(Quaternion q)
	{
		return new Quaternion((0f - q.x - q.y) * 0.70710677f, (q.x - q.y) * 0.70710677f, (0f - q.z + q.w) * 0.70710677f, (0f - q.z - q.w) * 0.70710677f);
	}

	public static Quaternion QRot010(Quaternion q)
	{
		return default(Quaternion);
	}

	public static Quaternion QRot011(Quaternion q)
	{
		return default(Quaternion);
	}

	public static Quaternion QRot012(Quaternion q)
	{
		return default(Quaternion);
	}

	public static Quaternion QRot013(Quaternion q)
	{
		return default(Quaternion);
	}

	public static Quaternion QRot020(Quaternion q)
	{
		return default(Quaternion);
	}

	public static Quaternion QRot021(Quaternion q)
	{
		return default(Quaternion);
	}

	public static Quaternion QRot022(Quaternion q)
	{
		return default(Quaternion);
	}

	public static Quaternion QRot023(Quaternion q)
	{
		return default(Quaternion);
	}

	public static Quaternion QRot030(Quaternion q)
	{
		return default(Quaternion);
	}

	public static Quaternion QRot031(Quaternion q)
	{
		return default(Quaternion);
	}

	public static Quaternion QRot032(Quaternion q)
	{
		return default(Quaternion);
	}

	public static Quaternion QRot033(Quaternion q)
	{
		return default(Quaternion);
	}

	public static Quaternion QRot100(Quaternion q)
	{
		return default(Quaternion);
	}

	public static Quaternion QRot130(Quaternion q)
	{
		return default(Quaternion);
	}

	public static Quaternion QRot120(Quaternion q)
	{
		return default(Quaternion);
	}

	public static Quaternion QRot110(Quaternion q)
	{
		return default(Quaternion);
	}

	public static Quaternion QRot300(Quaternion q)
	{
		return default(Quaternion);
	}

	public static Quaternion QRot310(Quaternion q)
	{
		return default(Quaternion);
	}

	public static Quaternion QRot320(Quaternion q)
	{
		return default(Quaternion);
	}

	public static Quaternion QRot330(Quaternion q)
	{
		return default(Quaternion);
	}
}
