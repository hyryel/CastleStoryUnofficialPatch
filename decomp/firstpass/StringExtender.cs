public static class StringExtender
{
	public static bool ContainsLetters(this string texte, string liste)
	{
		foreach (char value in texte)
		{
			if (liste.IndexOf(value) >= 0)
			{
				return true;
			}
		}
		return false;
	}
}
