using System;

namespace Brix.Engine.Environment.Configs;

[Serializable]
public class LodTransition
{
	public int Start;

	public int Padding;

	public LodTransition(int start, int padding)
	{
		Start = start;
		Padding = padding;
	}
}
