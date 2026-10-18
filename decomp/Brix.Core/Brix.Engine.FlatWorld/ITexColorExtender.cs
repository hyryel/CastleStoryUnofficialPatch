using UnityEngine;

namespace Brix.Engine.FlatWorld;

public static class ITexColorExtender
{
	public static void CopyFrom(this ITexColors self, ITexColors other)
	{
		if (!other.Empty)
		{
			self.SetPixels32(other.GetPixels32(), other.Width, other.Height);
		}
		else
		{
			self.Destroy();
		}
	}

	public static void CopyTo(this ITexColors self, ITexColors other)
	{
		other.CopyFrom(self);
	}

	public static void Swap(this ITexColors self, ITexColors other)
	{
		if (self.Empty)
		{
			self.CopyFrom(other);
			other.Destroy();
			return;
		}
		if (other.Empty)
		{
			other.CopyFrom(self);
			self.Destroy();
			return;
		}
		Color32[] pixels = self.GetPixels32();
		int width = self.Width;
		self.SetPixels32(other.GetPixels32(), other.Width, other.Height);
		other.SetPixels32(pixels, width, pixels.Length / width);
	}
}
