using System.IO;

namespace Brix.Util;

internal class FileInfoHandle : Fichier
{
	public FileInfo fileInfo;

	public override bool Exists()
	{
		return fileInfo.Exists;
	}

	public override byte[] ReadAllBytes()
	{
		return fileInfo.ReadAllBytes();
	}

	public override string ReadAllText()
	{
		return fileInfo.ReadAllText();
	}

	public override void WriteAllBytes(byte[] bytes)
	{
		fileInfo.WriteAllBytes(bytes);
	}

	public override void WriteAllText(string text)
	{
		fileInfo.WriteAllText(text);
	}
}
