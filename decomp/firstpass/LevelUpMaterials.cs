using System.Collections.Generic;
using UnityEngine;

public class LevelUpMaterials : MonoBehaviour
{
	public SkinnedMeshRenderer MaterialToReplace;

	public List<Material> MaterialsList;

	public void ChangeMaterial(int level)
	{
		Material material = GetMaterial(level);
		if (!(material == null))
		{
			SkinnedMeshRenderer skinnedMeshRenderer = ((!(MaterialToReplace != null)) ? GetComponentInChildren<SkinnedMeshRenderer>() : MaterialToReplace);
			if ((bool)skinnedMeshRenderer)
			{
				skinnedMeshRenderer.material = material;
			}
		}
	}

	public Material GetMaterial(int level)
	{
		if (MaterialsList == null || MaterialsList.Count == 0)
		{
			return null;
		}
		return MaterialsList[Mathf.Min(level, MaterialsList.Count - 1)];
	}
}
