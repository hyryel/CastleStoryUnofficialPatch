using UnityEngine;

namespace Brix.Game.Components.Skybox;

public class SkyboxCameraDrawer : MonoBehaviour
{
	public Camera skyboxCamera;

	public Material copyMaterial;

	public Material clearMaterial;

	public bool drawSkybox = true;

	public bool drawScene = true;

	public bool reverseOrder;

	public bool renderManually;

	private RenderTexture _source;

	private RenderTexture _destination;

	public RenderTexture SkyboxTexture => skyboxCamera.targetTexture;

	private void Blit(Texture source, RenderTexture destination, Material material)
	{
		material.SetPass(0);
		Graphics.Blit(source, destination, material, 0);
	}

	private void ClearScreen()
	{
		Blit(_source, _destination, clearMaterial);
	}

	private void DrawSkybox()
	{
		if (skyboxCamera == null)
		{
			ClearScreen();
		}
		else if (renderManually)
		{
			RenderTexture targetTexture = skyboxCamera.targetTexture;
			skyboxCamera.enabled = false;
			skyboxCamera.targetTexture = _destination;
			skyboxCamera.Render();
			skyboxCamera.targetTexture = targetTexture;
		}
		else
		{
			skyboxCamera.enabled = true;
			skyboxCamera.targetTexture = _destination;
		}
	}

	private void DrawScene()
	{
		Blit(_source, _destination, copyMaterial);
	}

	private void OnRenderImage(RenderTexture source, RenderTexture destination)
	{
		_source = source;
		_destination = destination;
		bool flag = !drawSkybox;
		if (!reverseOrder)
		{
			if (flag)
			{
				ClearScreen();
			}
			if (drawSkybox)
			{
				DrawSkybox();
			}
			if (drawScene)
			{
				DrawScene();
			}
		}
		else
		{
			if (flag)
			{
				ClearScreen();
			}
			if (drawScene)
			{
				DrawScene();
			}
			if (drawSkybox)
			{
				DrawSkybox();
			}
		}
	}
}
