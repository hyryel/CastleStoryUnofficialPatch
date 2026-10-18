using System.Collections.Generic;
using UnityEngine;

namespace Brix.Rendering;

public class ShareMaterialPropertyBlock : MonoBehaviour
{
	public Renderer sourceRenderer;

	public List<Renderer> targetRenderers;

	private MaterialPropertyBlock sourcePropertyBlock;

	private void Start()
	{
		Init();
	}

	private void Update()
	{
		UpdateValues();
	}

	[ContextMenu("Update All")]
	private void UpdateAll()
	{
		Init();
		UpdateValues();
	}

	private void Init()
	{
		sourcePropertyBlock = new MaterialPropertyBlock();
		foreach (Renderer targetRenderer in targetRenderers)
		{
			targetRenderer.sharedMaterials = sourceRenderer.sharedMaterials;
		}
	}

	private void UpdateValues()
	{
		sourceRenderer.GetPropertyBlock(sourcePropertyBlock);
		foreach (Renderer targetRenderer in targetRenderers)
		{
			targetRenderer.SetPropertyBlock(sourcePropertyBlock);
		}
	}
}
