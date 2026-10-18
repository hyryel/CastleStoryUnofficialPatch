using LibNoise;
using UnityEngine;

namespace Brix.Noise;

public abstract class NoiseBaseComponent : MonoBehaviour
{
	private ModuleBase _noiseModuleBase;

	private Noise2D m_noiseMap;

	[SerializeField]
	private Texture2D noiseTexture;

	public ModuleBase NoiseModuleBase
	{
		get
		{
			if (_noiseModuleBase == null)
			{
				InitNoiseModule();
			}
			return _noiseModuleBase;
		}
		set
		{
			_noiseModuleBase = value;
		}
	}

	private void Awake()
	{
		GenerateNoiseTexture2D();
	}

	public Texture2D GenerateNoiseTexture2D()
	{
		Refresh();
		m_noiseMap = new Noise2D(64, 64, NoiseModuleBase);
		if (m_noiseMap.Generator == null)
		{
			return null;
		}
		m_noiseMap.GeneratePlanar(-10.0, 10.0, -10.0, 10.0);
		noiseTexture = m_noiseMap.GetTexture(GradientPresets.RGB);
		noiseTexture.Apply();
		return noiseTexture;
	}

	public abstract void SetSeed(int seed);

	protected abstract ModuleBase InitNoiseModule();

	public virtual void Refresh()
	{
		InitNoiseModule();
	}

	public double GetValue(Vector3 positon, int seed = 0)
	{
		return NoiseModuleBase.GetValue(positon);
	}
}
