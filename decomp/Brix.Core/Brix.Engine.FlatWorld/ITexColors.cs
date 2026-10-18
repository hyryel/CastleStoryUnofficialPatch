using UnityEngine;

namespace Brix.Engine.FlatWorld;

public interface ITexColors
{
	int Width { get; }

	int Height { get; }

	bool Empty { get; }

	Color32 GetPixel(int x, int z);

	void SetPixel(int x, int z, Color32 c);

	Color32[] GetPixels32();

	void SetPixels32(Color32[] colors, int width, int height);

	void Destroy();
}
