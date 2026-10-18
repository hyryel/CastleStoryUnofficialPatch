using System.IO;

namespace Brix.Util;

public abstract class Fichier
{
	public abstract bool Exists();

	public abstract byte[] ReadAllBytes();

	public abstract string ReadAllText();

	public abstract void WriteAllBytes(byte[] bytes);

	public abstract void WriteAllText(string text);

	public static Fichier FromFileInfo(FileInfo fileInfo)
	{
		return new FileInfoHandle
		{
			fileInfo = fileInfo
		};
	}
}
