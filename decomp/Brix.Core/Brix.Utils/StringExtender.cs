namespace Brix.Utils;

public static class StringExtender
{
	public static string SubstringBefore(this string s, string substring)
	{
		int num = s.IndexOf(substring);
		if (num < 0)
		{
			return "";
		}
		return s.Substring(0, num);
	}

	public static string SubstringAfter(this string s, string substring)
	{
		int num = s.IndexOf(substring);
		if (num < 0)
		{
			return "";
		}
		return s.Substring(num + substring.Length);
	}

	public static string SubstringBetween(this string s, string prefix, string suffix)
	{
		return s.SubstringAfter(prefix).SubstringBefore(suffix);
	}
}
