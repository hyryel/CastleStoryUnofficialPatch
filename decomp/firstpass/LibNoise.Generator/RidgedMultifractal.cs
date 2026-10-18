using System;
using UnityEngine;

namespace LibNoise.Generator;

public class RidgedMultifractal : ModuleBase
{
	private double _frequency = 1.0;

	private double _lacunarity = 2.0;

	private QualityMode _quality = QualityMode.Medium;

	private int _octaveCount = 6;

	private int _seed;

	private readonly double[] _weights = new double[30];

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
			UpdateWeights();
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

	public RidgedMultifractal()
		: base(0)
	{
		UpdateWeights();
	}

	public RidgedMultifractal(double frequency, double lacunarity, int octaves, int seed, QualityMode quality)
		: base(0)
	{
		Frequency = frequency;
		Lacunarity = lacunarity;
		OctaveCount = octaves;
		Seed = seed;
		Quality = quality;
	}

	private void UpdateWeights()
	{
		double num = 1.0;
		for (int i = 0; i < 30; i++)
		{
			_weights[i] = Math.Pow(num, -1.0);
			num *= _lacunarity;
		}
	}

	public override double GetValue(double x, double y, double z)
	{
		x *= _frequency;
		y *= _frequency;
		z *= _frequency;
		double num = 0.0;
		double num2 = 1.0;
		double num3 = 1.0;
		double num4 = 2.0;
		for (int i = 0; i < _octaveCount; i++)
		{
			double x2 = Utils.MakeInt32Range(x);
			double y2 = Utils.MakeInt32Range(y);
			double z2 = Utils.MakeInt32Range(z);
			long seed = (_seed + i) & 0x7FFFFFFF;
			double value = Utils.GradientCoherentNoise3D(x2, y2, z2, seed, _quality);
			value = Math.Abs(value);
			value = num3 - value;
			value *= value;
			value *= num2;
			num2 = value * num4;
			num2 = Mathf.Clamp01((float)num2);
			num += value * _weights[i];
			x *= _lacunarity;
			y *= _lacunarity;
			z *= _lacunarity;
		}
		return num * 1.25 - 1.0;
	}
}
