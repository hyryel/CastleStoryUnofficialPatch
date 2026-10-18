using System;
using UnityEngine;

namespace Brix.Game.Components.Skybox;

public class SkyboxConfigurator : MonoBehaviour
{
	public Camera _skyboxCamera;

	public int _skyboxCameraScale = 1;

	public int _skyboxCameraMovementScale = 1;

	public float _skyboxHauteur;

	private Camera _mappedCamera;

	private RenderTexture _renderTexture;

	private void Start()
	{
		if (_skyboxCamera == null)
		{
			throw new NullReferenceException("The SkyboxConfigurator has no Skybox camera attached to it.");
		}
	}

	public void Inject(Camera camera)
	{
		if (camera == null)
		{
			throw new NullReferenceException("The injected camera to the SkyboxConfigurator is null.");
		}
		_mappedCamera = camera;
		_mappedCamera.GetComponent<SkyboxCameraDrawer>().skyboxCamera = _skyboxCamera;
	}

	private void Update()
	{
	}

	private void LateUpdate()
	{
		if (!(_mappedCamera == null))
		{
			_skyboxCamera.transform.rotation = _mappedCamera.transform.rotation;
			_skyboxCamera.transform.localPosition = (_mappedCamera.transform.position + Vector3.up * _skyboxHauteur) / _skyboxCameraMovementScale;
		}
	}

	[ContextMenu("Reconfigure RenderTexture")]
	private void ConfigureRenderTexture()
	{
		UnityEngine.Skybox component = _mappedCamera.GetComponent<UnityEngine.Skybox>();
		if (component == null)
		{
			throw new NullReferenceException("The injected camera '" + _mappedCamera.gameObject.name + "' does not have the required Skybox component.");
		}
		float num = _mappedCamera.pixelWidth;
		float num2 = _mappedCamera.pixelHeight;
		if (_renderTexture != null)
		{
			_renderTexture.Release();
			UnityEngine.Object.Destroy(_renderTexture);
		}
		RenderTextureFormat format = ((!_mappedCamera.allowHDR) ? RenderTextureFormat.Default : RenderTextureFormat.DefaultHDR);
		_renderTexture = new RenderTexture((int)num, (int)num2, 24, format, RenderTextureReadWrite.sRGB);
		_renderTexture.anisoLevel = 1;
		_renderTexture.filterMode = FilterMode.Point;
		_renderTexture.wrapMode = TextureWrapMode.Clamp;
		_renderTexture.Create();
		_skyboxCamera.ResetAspect();
		_skyboxCamera.aspect = num / num2;
		_skyboxCamera.targetTexture = _renderTexture;
		component.material.mainTexture = _renderTexture;
		_mappedCamera.allowHDR = _skyboxCamera.allowHDR;
	}

	public Camera GetMappedCamera()
	{
		return _mappedCamera;
	}
}
