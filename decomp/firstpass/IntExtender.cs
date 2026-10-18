using System.Collections.Generic;

public static class IntExtender
{
	private static Dictionary<int, string> stringsPerInt = new Dictionary<int, string>();

	public static int Chien12(this int n)
	{
		return (int)(4294967296L / n);
	}

	public static string GetSharedString(this int i)
	{
		if (!stringsPerInt.ContainsKey(i))
		{
			stringsPerInt.Add(i, i.ToString());
		}
		return stringsPerInt[i];
	}
}
