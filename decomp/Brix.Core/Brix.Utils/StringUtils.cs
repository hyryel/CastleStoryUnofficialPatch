using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using Brix.Util;

namespace Brix.Utils;

public static class StringUtils
{
	public const string CarriageReturnLineFeed = "\r\n";

	public const string Empty = "";

	public const char CarriageReturn = '\r';

	public const char LineFeed = '\n';

	public const char Tab = '\t';

	public static string FormatWith(this string format, IFormatProvider provider, object arg0)
	{
		return format.FormatWith(provider, new object[1] { arg0 });
	}

	public static string FormatWith(this string format, IFormatProvider provider, object arg0, object arg1)
	{
		return format.FormatWith(provider, new object[2] { arg0, arg1 });
	}

	public static string FormatWith(this string format, IFormatProvider provider, object arg0, object arg1, object arg2)
	{
		return format.FormatWith(provider, new object[3] { arg0, arg1, arg2 });
	}

	public static string FormatWith(this string format, IFormatProvider provider, params object[] args)
	{
		ValidationUtils.ArgumentNotNull(format, "format");
		return string.Format(provider, format, args);
	}

	public static bool IsWhiteSpace(string s)
	{
		if (s == null)
		{
			throw new ArgumentNullException("s");
		}
		if (s.Length == 0)
		{
			return false;
		}
		for (int i = 0; i < s.Length; i++)
		{
			if (!char.IsWhiteSpace(s[i]))
			{
				return false;
			}
		}
		return true;
	}

	public static string NullEmptyString(string s)
	{
		return string.IsNullOrEmpty(s) ? null : s;
	}

	public static StringWriter CreateStringWriter(int capacity)
	{
		StringBuilder sb = new StringBuilder(capacity);
		return new StringWriter(sb, CultureInfo.InvariantCulture);
	}

	public static int? GetLength(string value)
	{
		return value?.Length;
	}

	public static TSource ForgivingCaseSensitiveFind<TSource>(this IEnumerable<TSource> source, Func<TSource, string> valueSelector, string testValue)
	{
		if (source == null)
		{
			throw new ArgumentNullException("source");
		}
		if (valueSelector == null)
		{
			throw new ArgumentNullException("valueSelector");
		}
		IEnumerable<TSource> source2 = source.Where((TSource s) => string.Equals(valueSelector(s), testValue, StringComparison.OrdinalIgnoreCase));
		if (source2.Count() <= 1)
		{
			return source2.SingleOrDefault();
		}
		IEnumerable<TSource> source3 = source.Where((TSource s) => string.Equals(valueSelector(s), testValue, StringComparison.Ordinal));
		return source3.SingleOrDefault();
	}

	public static string ToCamelCase(string s)
	{
		if (string.IsNullOrEmpty(s))
		{
			return s;
		}
		if (!char.IsUpper(s[0]))
		{
			return s;
		}
		StringBuilder stringBuilder = new StringBuilder();
		for (int i = 0; i < s.Length; i++)
		{
			bool flag = i + 1 < s.Length;
			if (i == 0 || !flag || char.IsUpper(s[i + 1]))
			{
				char value = char.ToLower(s[i], CultureInfo.InvariantCulture);
				stringBuilder.Append(value);
				continue;
			}
			stringBuilder.Append(s.Substring(i));
			break;
		}
		return stringBuilder.ToString();
	}

	public static string CalculateMD5Hash(string input)
	{
		MD5 mD = MD5.Create();
		byte[] bytes = Encoding.ASCII.GetBytes(input);
		byte[] array = mD.ComputeHash(bytes);
		StringBuilder stringBuilder = new StringBuilder();
		for (int i = 0; i < array.Length; i++)
		{
			stringBuilder.Append(array[i].ToString("X2"));
		}
		return stringBuilder.ToString();
	}

	public static string ReplaceFirst(this string text, string search, string replace)
	{
		int num = text.IndexOf(search);
		if (num < 0)
		{
			return text;
		}
		return text.Substring(0, num) + replace + text.Substring(num + search.Length);
	}
}
