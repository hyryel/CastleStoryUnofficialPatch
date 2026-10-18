using System.Collections.Generic;
using Brix.Util;
using UnityEngine;

namespace Brix.Engine.FlatWorld;

public class Common
{
	public Tex noiseTex;

	public Tex permTex;

	public Tex warpTex;

	public Tex typeAtlasTex;

	public Tex typeSampleTex;

	public INoise3DGenerator noiseGenerator;

	public MondeAtlas atlas;

	public TexColors PermColors = new TexColors();

	public TexColors WarpColors = new TexColors();

	public TexColors TypeAtlasColors = new TexColors();

	public TexColors TypeSampleColors = new TexColors();

	private Tex A = new Tex();

	private Tex B = new Tex();

	private readonly Dictionary<int, Tex> _noiseTexDico = new Dictionary<int, Tex>();

	private readonly Dictionary<int, TexColors> _noiseTexColorsDico = new Dictionary<int, TexColors>();

	private readonly TexColors _noiseTexColors = new TexColors();

	private int _noiseTexWidth;

	public Common(Texture2D noiseTex, Texture2D permTex, Texture2D warpTex, MondeAtlas atlas, bool useNoise)
	{
		this.atlas = atlas;
		this.noiseTex = noiseTex;
		this.permTex = permTex;
		this.warpTex = warpTex;
		if (atlas.InitTypeSampleTex(B, A))
		{
			B.FreeVRAM();
			typeSampleTex = B;
			TypeSampleColors.CopyFrom(typeSampleTex);
		}
		if (atlas.InitTypeAtlasTexture(A))
		{
			A.FreeVRAM();
			typeAtlasTex = A;
			TypeAtlasColors.CopyFrom(typeAtlasTex);
		}
		if (useNoise)
		{
			noiseGenerator = new FractalNoise3DGenerator(permTex);
		}
		else
		{
			noiseGenerator = new WhiteNoise3DGenerator();
		}
		PermColors.CopyFrom(this.permTex);
		WarpColors.CopyFrom(this.warpTex);
		InitNoiseTex();
	}

	public Tex GetProperlySizedNoiseTex(int width)
	{
		if (width <= _noiseTexWidth)
		{
			width = _noiseTexWidth;
		}
		Tex tex = _noiseTexDico.GetValueOrDefault(width);
		if (tex == null)
		{
			tex = new Tex();
			ComputeNoiseTex(tex, _noiseTexColors, width, width);
			_noiseTexDico[width] = tex;
		}
		return tex;
	}

	public TexColors GetProperlySizedNoiseTexColors(int width)
	{
		if (width <= _noiseTexWidth)
		{
			width = _noiseTexWidth;
		}
		TexColors texColors = _noiseTexColorsDico.GetValueOrDefault(width);
		if (texColors == null)
		{
			texColors = new TexColors();
			ComputeNoiseTex(texColors, _noiseTexColors, width, width);
			_noiseTexColorsDico[width] = texColors;
		}
		return texColors;
	}

	private void InitNoiseTex()
	{
		if (_noiseTexColors.Empty)
		{
			Texture2D readableTexture = TextureUtil.GetReadableTexture(noiseTex);
			_noiseTexColors.SetPixels32(readableTexture.GetPixels32(), noiseTex.width, noiseTex.height);
			if (readableTexture != (Texture)noiseTex)
			{
				Object.Destroy(readableTexture);
			}
			_noiseTexWidth = noiseTex.width;
		}
	}

	private void ComputeNoiseTex(ITexColors outTex, ITexColors noiseTex, int width, int height)
	{
		Color32[] pixels = noiseTex.GetPixels32();
		Color32[] orAllocPixels = Soft.GetOrAllocPixels32(outTex, width, height);
		if (width < noiseTex.Width)
		{
			Soft.Blit32(pixels, noiseTex.Width, orAllocPixels, width, 0, 0, 0, 0, width, height);
		}
		else
		{
			for (int i = 0; i * noiseTex.Width < width; i++)
			{
				for (int j = 0; j * noiseTex.Height < height; j++)
				{
					Soft.Blit32(pixels, noiseTex.Width, orAllocPixels, width, 0, 0, i * noiseTex.Width, j * noiseTex.Height, noiseTex.Width, noiseTex.Height);
				}
			}
		}
		outTex.SetPixels32(orAllocPixels, width, height);
	}

	public void Destroy()
	{
		noiseTex = null;
		permTex = null;
		warpTex = null;
		typeAtlasTex = null;
		typeSampleTex = null;
		noiseGenerator = null;
		PermColors.Destroy();
		WarpColors.Destroy();
		TypeAtlasColors.Destroy();
		TypeSampleColors.Destroy();
		Tex.Destroy(A, B);
		foreach (Tex value in _noiseTexDico.Values)
		{
			Tex.Destroy(value);
		}
		_noiseTexDico.Clear();
		foreach (TexColors value2 in _noiseTexColorsDico.Values)
		{
			value2.Destroy();
		}
		_noiseTexColorsDico.Clear();
		_noiseTexColors.Destroy();
		_noiseTexWidth = 0;
	}
}
