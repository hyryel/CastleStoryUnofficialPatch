using System.Collections.Generic;
using Brix.Util;
using UnityEngine;

namespace Brix.Engine.Abstract;

public class Voxel
{
	public const int pl = 4;

	public const int pl2 = 8;

	public const int gl = 6;

	public const int gl2 = 12;

	public const int p = 15;

	public const int g = 63;

	public const int P = 16;

	public const int G = 64;

	public const int pg = 945;

	public const int plgl = 24;

	public const int pgl = 10;

	public const int pgl2 = 20;

	public const int PG = 1023;

	public const int up = 1048576;

	public const int right = 1;

	public const int forward = 1024;

	public const int gUp = 4096;

	public const int gRight = 1;

	public const int gForward = 64;

	public const int pUp = 256;

	public const int pRight = 1;

	public const int pForward = 16;

	public const int xMasque = 1023;

	public const int yMasque = 1047552;

	public const int zMasque = 1072693248;

	public const int xzMasque = 1072694271;

	public const int maxNiveau = 3;

	public const int masqueType = 32767;

	public static readonly int[] faceIndexToScalaire = new int[6] { 1048576, 1024, -1024, 1, -1, -1048576 };

	public static readonly Vector3[] faceIndexToVecteur = new Vector3[6]
	{
		Vector3.up,
		Vector3.forward,
		-Vector3.forward,
		Vector3.right,
		-Vector3.right,
		-Vector3.up
	};

	public static readonly int[] coteVersFaceIndex = new int[6] { 0, 51, 13056, 816, 12291, 0 };

	public static int[] rubikIndexToScalaire = new List<int>(Calcule.CubeConcentrique(0, 1)).ToArray();

	public static string[] exceptions = null;

	private static int[] faceMask = new int[6] { 13107, 51, 13056, 816, 12291, 13107 };

	protected static string _GetSurfaceFromTexture(int texture)
	{
		return texture switch
		{
			1 => "Brick", 
			7 => "Stone", 
			8 => "Grass", 
			9 => "Sand", 
			10 => "Earth", 
			11 => "Wood", 
			_ => "Stone", 
		};
	}

	public static int CoteVers(int scalaire)
	{
		int result = 0;
		switch (scalaire)
		{
		case 1024:
			result = 51;
			break;
		case -1024:
			result = 13056;
			break;
		case 1:
			result = 816;
			break;
		case -1:
			result = 12291;
			break;
		}
		return result;
	}

	public static int GetH(int y, int n)
	{
		return y * 3 + n;
	}

	public static bool Placable(int a, int b)
	{
		int num = GetY(a) - GetY(b);
		return GetDistXZ(a, b) <= 1 && num >= -2 && num <= 1;
	}

	public static bool Placable(int a, int b, int c)
	{
		if (a == b || a == c || (GetX(a) == GetX(b) && GetZ(a) == GetZ(b)) || (GetX(a) == GetX(c) && GetZ(a) == GetZ(c)))
		{
			return false;
		}
		return (GetDistXZ(a, b) <= 1 && GetDistY(a, b) <= 1) || (GetDistXZ(a, c) <= 1 && GetDistY(a, c) <= 1);
	}

	public static int Placable(int a, int[] liste)
	{
		foreach (int num in liste)
		{
			if (Placable(a, num))
			{
				return num;
			}
		}
		return 0;
	}

	public static int Placable(int a, int[] liste, int dist)
	{
		foreach (int num in liste)
		{
			if (PlacableDist(a, num, dist))
			{
				return num;
			}
		}
		return 0;
	}

	public static bool PlacableDist(int a, int b, int dist)
	{
		int num = GetY(a) - GetY(b);
		return GetDistXZ(a, b) <= dist && num >= -2 && num <= 1;
	}

	public static int GetScalaire(int x, int y, int z)
	{
		return x + (z << 10) + (y << 20);
	}

	public static int GetScalaire(Vector3 vecteur)
	{
		return Mathf.FloorToInt(vecteur.x) + (Mathf.FloorToInt(vecteur.z) << 10) + (Mathf.FloorToInt(vecteur.y) << 20);
	}

	public static Vector3 GetVecteur(int nombre)
	{
		return new Vector3(nombre & 0x3FF, (nombre >> 20) & 0x3FF, (nombre >> 10) & 0x3FF);
	}

	public static int GetPScalaire(int nombre)
	{
		return (nombre & 0xF) + (((nombre >> 10) & 0xF) << 4) + (((nombre >> 20) & 0xF) << 8);
	}

	public static int GetPScalaire(Vector3 vecteur)
	{
		return (Mathf.FloorToInt(vecteur.x) & 0xF) + ((Mathf.FloorToInt(vecteur.z) & 0xF) << 4) + ((Mathf.FloorToInt(vecteur.y) & 0xF) << 8);
	}

	public static int GetPScalaire(int x, int y, int z)
	{
		return (x & 0xF) + ((z & 0xF) << 4) + ((y & 0xF) << 8);
	}

	public static Vector3 GetPVecteur(int nombre)
	{
		return new Vector3(nombre & 0xF, (nombre >> 20) & 0xF, (nombre >> 10) & 0xF);
	}

	public static Vector3 GetPVecteur(Vector3 vecteur)
	{
		return new Vector3(Mathf.FloorToInt(vecteur.x) & 0xF, Mathf.FloorToInt(vecteur.y) & 0xF, Mathf.FloorToInt(vecteur.z) & 0xF);
	}

	public static Vector3 GetPVecteur(int x, int y, int z)
	{
		return new Vector3(x & 0xF, y & 0xF, z & 0xF);
	}

	public static int GetUScalaire(int nombre)
	{
		return ((nombre >> 4) & 0x3C) | (((nombre >> 10 >> 4) & 0x3C) << 6) | (((nombre >> 20 >> 4) & 0x3E) << 12);
	}

	public static int GetGScalaire(int nombre)
	{
		return ((nombre >> 4) & 0x3F) + (((nombre >> 10 >> 4) & 0x3F) << 6) + (((nombre >> 20 >> 4) & 0x3F) << 12);
	}

	public static int GetGScalaire(Vector3 vecteur)
	{
		return ((Mathf.FloorToInt(vecteur.x) >> 4) & 0x3F) + (((Mathf.FloorToInt(vecteur.z) >> 4) & 0x3F) << 6) + (((Mathf.FloorToInt(vecteur.y) >> 4) & 0x3F) << 12);
	}

	public static int GetGScalaire(int x, int y, int z)
	{
		return ((x >> 4) & 0x3F) + (((z >> 4) & 0x3F) << 6) + (((y >> 4) & 0x3F) << 12);
	}

	public static Vector3 GetGVecteur(int nombre)
	{
		return new Vector3((nombre >> 4) & 0x3F, (nombre >> 20 >> 4) & 0x3F, (nombre >> 10 >> 4) & 0x3F);
	}

	public static Vector3 GetGVecteur(Vector3 vecteur)
	{
		return new Vector3((Mathf.FloorToInt(vecteur.x) >> 4) & 0x3F, (Mathf.FloorToInt(vecteur.y) >> 4) & 0x3F, (Mathf.FloorToInt(vecteur.z) >> 4) & 0x3F);
	}

	public static Vector3 GetGVecteur(int x, int y, int z)
	{
		return new Vector3((x >> 4) & 0x3F, (y >> 4) & 0x3F, (z >> 4) & 0x3F);
	}

	public static int GetX(int nombre)
	{
		return nombre & 0x3FF;
	}

	public static int GetY(int nombre)
	{
		return (nombre >> 20) & 0x3FF;
	}

	public static int GetZ(int nombre)
	{
		return (nombre >> 10) & 0x3FF;
	}

	public static int GetPX(int nombre)
	{
		return nombre & 0xF;
	}

	public static int GetPY(int nombre)
	{
		return (nombre >> 20) & 0xF;
	}

	public static int GetPZ(int nombre)
	{
		return (nombre >> 10) & 0xF;
	}

	public static int GetGX(int nombre)
	{
		return (nombre >> 4) & 0x3F;
	}

	public static int GetGY(int nombre)
	{
		return (nombre >> 20 >> 4) & 0x3F;
	}

	public static int GetGZ(int nombre)
	{
		return (nombre >> 10 >> 4) & 0x3F;
	}

	public static int GetXZ(int nombre)
	{
		return nombre & 0xFFFFF;
	}

	public static int GetDistXZ(int a, int b)
	{
		int x = GetX(a);
		int x2 = GetX(b);
		int z = GetZ(a);
		int z2 = GetZ(b);
		return ((x < x2) ? (x2 - x) : (x - x2)) + ((z < z2) ? (z2 - z) : (z - z2));
	}

	public static int GetDistY(int a, int b)
	{
		return Calcule.Abs(GetY(a) - GetY(b));
	}

	public static int TrimScalaire(int nombre, int masque)
	{
		masque = ~masque & 0x3FF;
		return nombre & (masque + (masque << 10) + (masque << 20));
	}

	public static int Conversion(int scalaire, int c1, int c2)
	{
		int num = (1 << c1) - 1;
		return (scalaire & num) + (((scalaire >> c1) & num) << c2) + (((scalaire >> (c1 << 1)) & num) << (c2 << 1));
	}

	public static int GVecteurToGScalaire(Vector3 vecteur)
	{
		return Mathf.FloorToInt(vecteur.x) + (Mathf.FloorToInt(vecteur.z) << 6) + (Mathf.FloorToInt(vecteur.y) << 12);
	}

	public static Vector3 GScalaireToGVecteur(int nombre)
	{
		return new Vector3(nombre & 0x3F, (nombre >> 12) & 0x3F, (nombre >> 6) & 0x3F);
	}

	public static int GetScalaire(int nombre1, int nombre2)
	{
		return ((nombre1 & 0x3F) << 4) + (nombre2 & 0xF) + (((nombre1 >> 6) & 0x3F) << 10 << 4) + (((nombre2 >> 4) & 0xF) << 10) + (((nombre1 >> 12) & 0x3F) << 20 << 4) + (((nombre2 >> 8) & 0xF) << 20);
	}

	public static IEnumerable<int> GetGScalairesVoisins(int index)
	{
		if (GetPX(index) == 0 && GetGX(index) > 0)
		{
			yield return GetGScalaire(index) - 1;
		}
		else if (GetPX(index) == 15 && GetGX(index) > 64)
		{
			yield return GetGScalaire(index) + 1;
		}
		if (GetPY(index) == 0 && GetGY(index) > 0)
		{
			yield return GetGScalaire(index) - 4096;
		}
		else if (GetPY(index) == 15 && GetGY(index) > 64)
		{
			yield return GetGScalaire(index) + 4096;
		}
		if (GetPZ(index) == 0 && GetGZ(index) > 0)
		{
			yield return GetGScalaire(index) - 64;
		}
		else if (GetPZ(index) == 15 && GetGZ(index) > 64)
		{
			yield return GetGScalaire(index) + 64;
		}
	}

	public static bool FacePleine(int niveaux, int iFace)
	{
		if (iFace == 0 || iFace == 5)
		{
			return true;
		}
		int num = faceMask[iFace];
		return (niveaux & num) == num;
	}

	public static bool FaceVide(int niveaux, int iFace)
	{
		if (iFace == 0 || iFace == 5)
		{
			return false;
		}
		int num = faceMask[iFace];
		return (niveaux & num) == 0;
	}

	public static int _GetTypeBloc(int info)
	{
		return info & 0x7F;
	}

	public static int _GetVariante(int info)
	{
		return (info >> 7) & 0xF;
	}

	public static int _GetNiveau(int info)
	{
		return ((info & 0x7F) < 6) ? ((info >> 13) & 3) : 3;
	}

	public static bool BoxedScalaire(int nombre)
	{
		return (nombre & 0x3FF) != 1023 && ((nombre >> 10) & 0x3FF) != 1023 && ((nombre >> 20) & 0x3FF) != 1023;
	}
}
