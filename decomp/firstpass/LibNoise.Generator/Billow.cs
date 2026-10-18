using System;
using UnityEngine;

namespace LibNoise.Generator;

public class Billow : ModuleBase
{
	private double _frequency = 1.0;

	private double _lacunarity = 2.0;

	private QualityMode _quality = QualityMode.Medium;

	private int _octaveCount = 6;

	private double _persistence = 0.5;

	private int _seed;

	public double Frequency
	{
		get
		{
			return _frequency;
		}
		set
		{
			_frequency = value;
		}
	}

	public double Lacunarity
	{
		get
		{
			return _lacunarity;
		}
		set
		{
			_lacunarity = value;
		}
	}

	public QualityMode Quality
	{
		get
		{
			return _quality;
		}
		set
		{
			_quality = value;
		}
	}

	public int OctaveCount
	{
		get
		{
			return _octaveCount;
		}
		set
		{
			_octaveCount = Mathf.Clamp(value, 1, 30);
		}
	}

	public double Persistence
	{
		get
		{
			return _persistence;
		}
		set
		{
			_persistence = value;
		}
	}

	public int Seed
	{
		get
		{
			return _seed;
		}
		set
		{
			_seed = value;
		}
	}

	public Billow()
		: base(0)
	{
	}

	public Billow(double frequency, double lacunarity, double persistence, int octaves, int seed, QualityMode quality)
		: base(0)
	{
		Frequency = frequency;
		Lacunarity = lacunarity;
		OctaveCount = octaves;
		Persistence = persistence;
		Seed = seed;
		Quality = quality;
	}

	public override double GetValue(double x, double y, double z)
	{
		double num = 0.0;
		double num2 = 1.0;
		x *= _frequency;
		y *= _frequency;
		z *= _frequency;
		for (int i = 0; i < _octaveCount; i++)
		{
			double x2 = Utils.MakeInt32Range(x);
			double y2 = Utils.MakeInt32Range(y);
			double z2 = Utils.MakeInt32Range(z);
			long seed = (_seed + i) & 0xFFFFFFFFu;
			double value = Utils.GradientCoherentNoise3D(x2, y2, z2, seed, _quality);
			value = 2.0 * Math.Abs(value) - 1.0;
			num += value * num2;
			x *= _lacunarity;
			y *= _lacunarity;
			z *= _lacunarity;
			num2 *= _persistence;
		}
		return num + 0.5;
	}
}
