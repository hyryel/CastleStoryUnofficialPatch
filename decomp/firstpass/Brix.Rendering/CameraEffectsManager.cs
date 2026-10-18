using Brix.Components;
using Brix.NewUI.UserSettings_;
using Brix.Utils.Unity;
using UnityEngine;
using UnityEngine.PostProcessing;

namespace Brix.Rendering;

public class CameraEffectsManager : BrixSingleton<CameraEffectsManager>
{
	private static UserSettings.SSAOQualityLevels _ssaoQuality;

	private static bool _bloomEnabled;

	private static bool _colorCorrectionEnabled;

	private static ShadowQuality _shadowQuality;

	private static ShadowResolution _shadowResolution;

	[DisplayRules(DisplayFlags.ReadOnlyInEditMode)]
	public SENaturalBloomAndDirtyLens Bloom;

	[DisplayRules(DisplayFlags.ReadOnlyInEditMode)]
	public AmplifyOcclusionEffect SSAO;

	[DisplayRules(DisplayFlags.ReadOnlyInEditMode)]
	public PostProcessingBehaviour PostProcessing;

	public static UserSettings.SSAOQualityLevels SSAOQuality
	{
		get
		{
			return _ssaoQuality;
		}
		set
		{
			if (_ssaoQuality != value)
			{
				_ssaoQuality = value;
				if ((bool)BrixSingleton<CameraEffectsManager>.Instance)
				{
					BrixSingleton<CameraEffectsManager>.Instance.SetQualitySSAO(_ssaoQuality);
				}
			}
		}
	}

	public static bool BloomEnabled
	{
		get
		{
			return _bloomEnabled;
		}
		set
		{
			if (_bloomEnabled != value)
			{
				_bloomEnabled = value;
				if ((bool)BrixSingleton<CameraEffectsManager>.Instance)
				{
					BrixSingleton<CameraEffectsManager>.Instance.SetBloomEnabled(_bloomEnabled);
				}
			}
		}
	}

	public static bool ColorCorrectionEnabled
	{
		get
		{
			return _colorCorrectionEnabled;
		}
		set
		{
			if (_colorCorrectionEnabled != value)
			{
				_colorCorrectionEnabled = value;
				if ((bool)BrixSingleton<CameraEffectsManager>.Instance)
				{
					BrixSingleton<CameraEffectsManager>.Instance.SetColorCorrectionEnabled(_colorCorrectionEnabled);
				}
			}
		}
	}

	public static ShadowQuality ShadowQuality
	{
		get
		{
			return _shadowQuality;
		}
		set
		{
			if (_shadowQuality != value)
			{
				_shadowQuality = value;
				if ((bool)BrixSingleton<CameraEffectsManager>.Instance)
				{
					BrixSingleton<CameraEffectsManager>.Instance.SetShadowQuality(_shadowQuality);
				}
			}
		}
	}

	public static ShadowResolution ShadowResolution
	{
		get
		{
			return _shadowResolution;
		}
		set
		{
			if (_shadowResolution != value)
			{
				_shadowResolution = value;
				if ((bool)BrixSingleton<CameraEffectsManager>.Instance)
				{
					BrixSingleton<CameraEffectsManager>.Instance.SetShadowResolution(_shadowResolution);
				}
			}
		}
	}

	protected override void Awake()
	{
		base.Awake();
		Bloom = Camera.main.GetComponent<SENaturalBloomAndDirtyLens>();
		SSAO = Camera.main.GetComponent<AmplifyOcclusionEffect>();
		PostProcessing = Camera.main.GetComponent<PostProcessingBehaviour>();
		SetQualitySSAO(_ssaoQuality);
		SetBloomEnabled(BloomEnabled);
		SetColorCorrectionEnabled(ColorCorrectionEnabled);
		SetShadowQuality(ShadowQuality);
		SetShadowResolution(ShadowResolution);
	}

	public void SetQualitySSAO(UserSettings.SSAOQualityLevels quality)
	{
		switch (quality)
		{
		case UserSettings.SSAOQualityLevels.high:
			SSAO.enabled = true;
			SSAO.SampleCount = AmplifyOcclusionBase.SampleCountLevel.Medium;
			SSAO.Downsample = true;
			SSAO.BlurEnabled = true;
			break;
		case UserSettings.SSAOQualityLevels.medium:
			SSAO.enabled = true;
			SSAO.SampleCount = AmplifyOcclusionBase.SampleCountLevel.Low;
			SSAO.Downsample = true;
			SSAO.BlurEnabled = true;
			break;
		case UserSettings.SSAOQualityLevels.low:
			SSAO.enabled = true;
			SSAO.SampleCount = AmplifyOcclusionBase.SampleCountLevel.Low;
			SSAO.Downsample = true;
			SSAO.BlurEnabled = false;
			break;
		case UserSettings.SSAOQualityLevels.off:
			SSAO.enabled = false;
			break;
		}
	}

	public void SetBloomEnabled(bool enable)
	{
		Bloom.enabled = enable;
	}

	public void SetColorCorrectionEnabled(bool enable)
	{
		PostProcessing.enabled = enable;
	}

	public void SetShadowQuality(ShadowQuality quality)
	{
		QualitySettings.shadows = quality;
	}

	public void SetShadowResolution(ShadowResolution resolution)
	{
		QualitySettings.shadowResolution = resolution;
	}
}
