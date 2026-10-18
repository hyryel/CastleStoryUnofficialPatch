using System.Linq;
using UnityEngine;

internal class FractalNoise3DGenerator : Noise3DGenerator
{
	public float scaleFactor = 2f;

	public float amplitudeFalloff = 0.5f;

	public int octaves = 4;

	public Vector4[] permArray;

	public int permWidth;

	private const float ONE = 0.00390625f;

	private const float ONEHALF = 0.001953125f;

	public FractalNoise3DGenerator(Texture2D permTex)
		: this(permTex.GetPixels(), permTex.width)
	{
	}

	public FractalNoise3DGenerator(Color32[] permColors, int width)
	{
		permArray = permColors.Select((Color32 c) => new Vector4((int)c.r, (int)c.g, (int)c.b, (int)c.a) * 0.003921569f).ToArray();
		permWidth = width;
	}

	public FractalNoise3DGenerator(Color[] permColors, int width)
	{
		permArray = permColors.Select((Color c) => new Vector4(c.r, c.g, c.b, c.a)).ToArray();
		permWidth = width;
	}

	public override float Noise(Vector3 v)
	{
		return turbulence(v);
	}

	private float fade(float t)
	{
		return t * t * t * (t * (t * 6f - 15f) + 10f);
	}

	private float noise(Vector3 P)
	{
		Vector3 vector = 0.00390625f * floor(P) + Vector3.one * 0.001953125f;
		Vector3 vector2 = P - floor(P);
		float x = tex2D_a(new Vector2(vector.x, vector.y));
		Vector3 a = tex2D_rgb(new Vector2(x, vector.z)) * 4f - Vector3.one;
		float x2 = dot(a, vector2);
		Vector3 a2 = tex2D_rgb(new Vector2(x, vector.z + 0.00390625f)) * 4f - Vector3.one;
		float y = dot(a2, vector2 - new Vector3(0f, 0f, 1f));
		float x3 = tex2D_a(new Vector2(vector.x, vector.y) + new Vector2(0f, 0.00390625f));
		Vector3 a3 = tex2D_rgb(new Vector2(x3, vector.z)) * 4f - Vector3.one;
		float z = dot(a3, vector2 - new Vector3(0f, 1f, 0f));
		Vector3 a4 = tex2D_rgb(new Vector2(x3, vector.z + 0.00390625f)) * 4f - Vector3.one;
		float w = dot(a4, vector2 - new Vector3(0f, 1f, 1f));
		float x4 = tex2D_a(new Vector2(vector.x, vector.y) + new Vector2(0.00390625f, 0f));
		Vector3 a5 = tex2D_rgb(new Vector2(x4, vector.z)) * 4f - Vector3.one;
		float x5 = dot(a5, vector2 - new Vector3(1f, 0f, 0f));
		Vector3 a6 = tex2D_rgb(new Vector2(x4, vector.z + 0.00390625f)) * 4f - Vector3.one;
		float y2 = dot(a6, vector2 - new Vector3(1f, 0f, 1f));
		float x6 = tex2D_a(new Vector2(vector.x, vector.y) + new Vector2(0.00390625f, 0.00390625f));
		Vector3 a7 = tex2D_rgb(new Vector2(x6, vector.z)) * 4f - Vector3.one;
		float z2 = dot(a7, vector2 - new Vector3(1f, 1f, 0f));
		Vector3 a8 = tex2D_rgb(new Vector2(x6, vector.z + 0.00390625f)) * 4f - Vector3.one;
		float w2 = dot(a8, vector2 - new Vector3(1f, 1f, 1f));
		Vector4 vector3 = lerp(new Vector4(x2, y, z, w), new Vector4(x5, y2, z2, w2), fade(vector2.x));
		Vector2 vector4 = lerp(new Vector2(vector3.x, vector3.y), new Vector2(vector3.z, vector3.w), fade(vector2.y));
		return lerp(vector4.x, vector4.y, fade(vector2.z));
	}

	private float turbulence(Vector3 P)
	{
		float num = 0f;
		float num2 = 1f;
		float num3 = 1f;
		for (int i = 0; i < octaves; i++)
		{
			num3 *= step(i, (float)octaves - 0.5f);
			num += num3 * noise(P * num2);
			num2 *= scaleFactor;
			num3 *= amplitudeFalloff;
		}
		return abs(num);
	}

	private float step(float a, float x)
	{
		return (x >= a) ? 1 : 0;
	}

	private float abs(float x)
	{
		return Mathf.Abs(x);
	}

	private float dot(Vector3 a, Vector3 b)
	{
		return Vector3.Dot(a, b);
	}

	private Vector3 floor(Vector3 a)
	{
		return new Vector3(Mathf.Floor(a.x), Mathf.Floor(a.y), Mathf.Floor(a.z));
	}

	private float lerp(float x, float y, float p)
	{
		return x + (y - x) * p;
	}

	private Vector2 lerp(Vector2 x, Vector2 y, float p)
	{
		return x + (y - x) * p;
	}

	private Vector4 lerp(Vector4 x, Vector4 y, float p)
	{
		return x + (y - x) * p;
	}

	private Vector4 tex2D(Vector2 v)
	{
		int num = Mathf.FloorToInt((v.x - Mathf.Floor(v.x)) * (float)permWidth);
		int num2 = Mathf.FloorToInt((v.y - Mathf.Floor(v.y)) * (float)permWidth);
		return permArray[num + num2 * permWidth];
	}

	private float tex2D_a(Vector2 v)
	{
		return tex2D(v).w;
	}

	private Vector3 tex2D_rgb(Vector2 v)
	{
		return tex2D(v);
	}
}
