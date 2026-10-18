using UnityEngine;
using UnityEngine.Rendering;

public class CameraCustomImageEffect : MonoBehaviour
{
	public CameraEvent camEvent = CameraEvent.BeforeImageEffectsOpaque;

	public BuiltinRenderTextureType texType = BuiltinRenderTextureType.CameraTarget;

	public Material imageEffectMat;

	public bool autoDetectCameraSettings;

	private Camera cam;

	private int inverseViewProperty;

	private int uvFlipProperty;

	private CommandBuffer buf;

	private CommandBuffer activeCmdBuf;

	private CameraEvent activeCamEvent;

	private bool started;

	private CameraEvent _exCamEvent;

	private BuiltinRenderTextureType _exTexType;

	private Material _exImageEffectMat;

	private void Start()
	{
		inverseViewProperty = Shader.PropertyToID("_InverseView");
		uvFlipProperty = Shader.PropertyToID("_UvFlip");
		cam = GetComponent<Camera>();
		if (cam.depthTextureMode != DepthTextureMode.Depth && cam.depthTextureMode != DepthTextureMode.DepthNormals)
		{
			cam.depthTextureMode = DepthTextureMode.Depth;
		}
		if (autoDetectCameraSettings)
		{
			camEvent = ((cam.actualRenderingPath != RenderingPath.Forward) ? CameraEvent.AfterLighting : CameraEvent.BeforeImageEffectsOpaque);
		}
		CreateCommandBuffer();
		EnableCommandBuffer();
		started = true;
	}

	private void OnEnable()
	{
		if (started)
		{
			EnableCommandBuffer();
		}
	}

	private void OnDisable()
	{
		DisableCommandBuffer();
	}

	private void OnValidate()
	{
		if (started && (_exCamEvent != camEvent || _exTexType != texType || _exImageEffectMat != imageEffectMat))
		{
			_exCamEvent = camEvent;
			_exTexType = texType;
			_exImageEffectMat = imageEffectMat;
			DisableCommandBuffer();
			CreateCommandBuffer();
			EnableCommandBuffer();
		}
	}

	private void OnPreRender()
	{
		if (started)
		{
			UpdateProperties();
		}
	}

	private void UpdateProperties()
	{
		imageEffectMat.SetMatrix(inverseViewProperty, cam.cameraToWorldMatrix);
		imageEffectMat.SetVector(uvFlipProperty, new Vector2((cam.actualRenderingPath == RenderingPath.Forward) ? 1 : 0, 0f));
	}

	private void CreateCommandBuffer()
	{
		buf = new CommandBuffer();
		buf.name = imageEffectMat.name;
		int num = Shader.PropertyToID("_ScreenCopyTexture");
		buf.GetTemporaryRT(num, -1, -1, 0, FilterMode.Bilinear, (!cam.allowHDR) ? RenderTextureFormat.Default : RenderTextureFormat.DefaultHDR);
		buf.Blit(texType, num);
		buf.Blit(num, texType, imageEffectMat);
		buf.ReleaseTemporaryRT(num);
	}

	private void EnableCommandBuffer()
	{
		if (activeCmdBuf == null)
		{
			activeCmdBuf = buf;
			activeCamEvent = camEvent;
			cam.AddCommandBuffer(camEvent, buf);
		}
	}

	private void DisableCommandBuffer()
	{
		if (activeCmdBuf != null)
		{
			cam.RemoveCommandBuffer(activeCamEvent, activeCmdBuf);
			activeCmdBuf = null;
		}
	}
}
