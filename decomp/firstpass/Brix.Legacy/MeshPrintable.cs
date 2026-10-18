using Brix.Game.Components;
using UnityEngine;

namespace Brix.Legacy;

public class MeshPrintable
{
	public MeshInfoClasse meshInfo;

	public int printCode;

	public int materialId;

	public Color couleur;

	public MeshPrintable(int type, Blueprint fantome)
	{
		meshInfo = new MeshInfoClasse();
		materialId = GetMaterialId(type);
		printCode = GetPrintCode();
	}

	public int GetMaterialId(int type)
	{
		return 0;
	}

	public Color GetCouleur(Blueprint fantome)
	{
		if (fantome.selection)
		{
			return new Color(1f, 0.5f, 0f);
		}
		if (fantome.IsAnti)
		{
			return new Color(1f, 1f, 0f);
		}
		return new Color(0f, 0.95f, 1f);
	}

	public int GetPrintCode()
	{
		return (string.Empty + materialId + couleur).GetHashCode();
	}
}
