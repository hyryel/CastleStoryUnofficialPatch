using UnityEngine;
using UnityEngine.Rendering;

public class CameraGrabPass : MonoBehaviour
{
	public CameraEvent camEvent = CameraEvent.AfterForwardOpaque;

	public BuiltinRenderTextureType texType = BuiltinRenderTextureType.CameraTarget;

	public string outputPropertyName = "_GrabTextureOpaque";

	private Camera cam;

	private CommandBuffer buf;

	private CommandBuffer activeCmdBuf;

	private CameraEvent activeCamEvent;

	private RenderTexture outputTexture;

	private bool started;

	private CameraEvent _exCamEvent;

	private BuiltinRenderTextureType _exTexType;

	private string _exOutputPropertyName;

	private void Start()
	{
		cam = GetComponent<Camera>();
		CreateRenderTexture();
		CreateCommandBuffer();
		EnableCommandBuffer();
		started = true;
	}

	private void OnDestroy()
	{
		DestroyRenderTexture();
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
		if (started && (_exCamEvent != camEvent || _exTexType != texType || _exOutputPropertyName != outputPropertyName))
		{
			_exCamEvent = camEvent;
			_exTexType = texType;
			_exOutputPropertyName = outputPropertyName;
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
		if (outputTexture.width != Screen.width || outputTexture.height != Screen.height)
		{
			DestroyRenderTexture();
			CreateRenderTexture();
			DisableCommandBuffer();
			CreateCommandBuffer();
			EnableCommandBuffer();
		}
	}

	private void CreateRenderTexture()
	{
		outputTexture = new RenderTexture(Screen.width, Screen.height, 0, (!cam.allowHDR) ? RenderTextureFormat.Default : RenderTextureFormat.DefaultHDR);
		outputTexture.Create();
	}

	private void DestroyRenderTexture()
	{
		outputTexture.Release();
		Object.Destroy(outputTexture);
	}

	private void SetGlobalRenderTextureProperty()
	{
		Shader.SetGlobalTexture(outputPropertyName, outputTexture);
	}

	private void CreateCommandBuffer()
	{
		buf = new CommandBuffer();
		buf.name = outputPropertyName;
		buf.Blit(texType, outputTexture);
	}

	private void EnableCommandBuffer()
	{
		if (activeCmdBuf == null)
		{
			activeCmdBuf = buf;
			activeCamEvent = camEvent;
			cam.AddCommandBuffer(camEvent, buf);
		}
		SetGlobalRenderTextureProperty();
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
