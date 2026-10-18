using Brix.Engine;
using LibNoise;
using LibNoise.Generator;

namespace Brix.Noise;

public class NoiseGenerator : NoiseBaseComponent
{
	public GeneratorType type;

	public int octaves = 1;

	public double frequency = 0.20000000298023224;

	public double lacunarity = 1.0;

	public double persistance = 1.0;

	public double displacement = 4.0;

	public bool applyDistanceFromNearestPoint;

	public int seed;

	public bool ignoreGlobalSeed;

	public override void SetSeed(int seed)
	{
		int num = ((!ignoreGlobalSeed) ? (seed + VoxelNoiseConfigurator.GetGlobalSeed()) : seed);
		switch (type)
		{
		case GeneratorType.Perlin:
		{
			Perlin perlin = base.NoiseModuleBase as Perlin;
			perlin.Seed = num;
			break;
		}
		case GeneratorType.Billow:
		{
			Billow billow = base.NoiseModuleBase as Billow;
			billow.Seed = num;
			break;
		}
		case GeneratorType.RidgedMultifractal:
		{
			RidgedMultifractal ridgedMultifractal = base.NoiseModuleBase as RidgedMultifractal;
			ridgedMultifractal.Seed = num;
			break;
		}
		case GeneratorType.Voronoi:
		{
			Voronoi voronoi = base.NoiseModuleBase as Voronoi;
			voronoi.Seed = num;
			break;
		}
		}
	}

	protected override ModuleBase InitNoiseModule()
	{
		int num = ((!ignoreGlobalSeed) ? (seed + VoxelNoiseConfigurator.GetGlobalSeed()) : seed);
		switch (type)
		{
		case GeneratorType.Perlin:
			base.NoiseModuleBase = new Perlin(frequency, lacunarity, persistance, octaves, num, QualityMode.Low);
			break;
		case GeneratorType.Billow:
			base.NoiseModuleBase = new Billow(frequency, lacunarity, persistance, octaves, num, QualityMode.Low);
			break;
		case GeneratorType.Checker:
			base.NoiseModuleBase = new Checker();
			break;
		case GeneratorType.RidgedMultifractal:
			base.NoiseModuleBase = new RidgedMultifractal(frequency, lacunarity, octaves, num, QualityMode.Low);
			break;
		case GeneratorType.Cylinder:
			base.NoiseModuleBase = new Cylinders(frequency);
			break;
		case GeneratorType.Voronoi:
			base.NoiseModuleBase = new Voronoi(frequency, displacement, num, applyDistanceFromNearestPoint);
			break;
		case GeneratorType.Sphere:
			base.NoiseModuleBase = new Spheres(frequency);
			break;
		}
		return base.NoiseModuleBase;
	}
}
