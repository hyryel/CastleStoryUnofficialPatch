using System;

namespace Brix.Legacy;

public class ScoreDescription
{
	public int index;

	public int height;

	public int score;

	public static Comparison<ScoreDescription> HeightComparison = _HeightComparison;

	public ScoreDescription(int index)
	{
		this.index = index;
	}

	public static int _HeightComparison(ScoreDescription sd1, ScoreDescription sd2)
	{
		return -1 * sd1.height.CompareTo(sd2.height);
	}
}
