using System.IO;

namespace Brix.Util;

public static class FileExtender
{
	public static byte[] ReadAllBytes(this FileInfo fileInfo)
	{
		byte[] array;
		using (FileStream fileStream = fileInfo.OpenRead())
		{
			array = new byte[fileStream.Length];
			fileStream.Read(array, 0, array.Length);
		}
		return array;
	}

	public static string ReadAllText(this FileInfo fileInfo)
	{
		using StreamReader streamReader = fileInfo.OpenText();
		return streamReader.ReadToEnd();
	}

	public static void WriteAllBytes(this FileInfo fileInfo, byte[] bytes)
	{
		using FileStream fileStream = fileInfo.OpenWrite();
		fileStream.Write(bytes, 0, bytes.Length);
	}

	public static void WriteAllText(this FileInfo fileInfo, string text)
	{
		using StreamWriter streamWriter = fileInfo.CreateText();
		streamWriter.Write(text);
	}
}
