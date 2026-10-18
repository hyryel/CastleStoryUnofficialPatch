using System;

namespace UnityEngine.PostProcessing;

[Serializable]
public class BuiltinDebugViewsModel : PostProcessingModel
{
	[Serializable]
	public struct DepthSettings
	{
		[Tooltip("Scales the camera far plane before displaying the depth map.")]
		[Range(0f, 1f)]
		public float scale;

		public static DepthSettings defaultSettings => new DepthSettings
		{
			scale = 1f
		};
	}

	[Serializable]
	public struct MotionVectorsSettings
	{
		[Tooltip("Opacity of the source render.")]
		[Range(0f, 1f)]
		public float sourceOpacity;

		[Range(0f, 1f)]
		[Tooltip("Opacity of the per-pixel motion vector colors.")]
		public float motionImageOpacity;

		[Tooltip("Because motion vectors are mainly very small vectors, you can use this setting to make them more visible.")]
		[Min(0f)]
		public float motionImageAmplitude;

		[Tooltip("Opacity for the motion vector arrows.")]
		[Range(0f, 1f)]
		public float motionVectorsOpacity;

		[Tooltip("The arrow density on screen.")]
		[Range(8f, 64f)]
		public int motionVectorsResolution;

		[Tooltip("Tweaks the arrows length.")]
		[Min(0f)]
		public float motionVectorsAmplitude;

		public static MotionVectorsSettings defaultSettings => new MotionVectorsSettings
		{
			sourceOpacity = 1f,
			motionImageOpacity = 0f,
			motionImageAmplitude = 16f,
			motionVectorsOpacity = 1f,
			motionVectorsResolution = 24,
			motionVectorsAmplitude = 64f
		};
	}

	public enum Mode
	{
		None,
		Depth,
		Normals,
		MotionVectors,
		AmbientOcclusion,
		EyeAdaptation,
		FocusPlane,
		PreGradingLog,
		LogLut,
		UserLut
	}

	[Serializable]
	public struct Settings
	{
		public Mode mode;

		public DepthSettings depth;

		public MotionVectorsSettings motionVectors;

		public static Settings defaultSettings => new Settings
		{
			mode = Mode.None,
			depth = DepthSettings.defaultSettings,
			motionVectors = MotionVectorsSettings.defaultSettings
		};
	}

	[SerializeField]
	private Settings m_Settings = Settings.defaultSettings;

	public Settings settings
	{
		get
		{
			return m_Settings;
		}
		set
		{
			m_Settings = value;
		}
	}

	public bool willInterrupt => !IsModeActive(Mode.None) && !IsModeActive(Mode.EyeAdaptation) && !IsModeActive(Mode.PreGradingLog) && !IsModeActive(Mode.LogLut) && !IsModeActive(Mode.UserLut);

	public override void Reset()
	{
		settings = Settings.defaultSettings;
	}

	public bool IsModeActive(Mode mode)
	{
		return m_Settings.mode == mode;
	}
}
