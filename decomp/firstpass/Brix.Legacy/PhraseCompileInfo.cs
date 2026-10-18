namespace Brix.Legacy;

public class PhraseCompileInfo
{
	public Livre.Chapitre chapitre;

	public int cIndex;

	public Livre.Page page;

	public int pIndex;

	public Livre.Paragraphe paragraphe;

	public int ppIndex;

	public PhraseCompileInfo(Livre.Chapitre chapitre, int cIndex, Livre.Page page, int pIndex, Livre.Paragraphe paragraphe, int ppIndex)
	{
		this.chapitre = chapitre;
		this.cIndex = cIndex;
		this.page = page;
		this.pIndex = pIndex;
		this.paragraphe = paragraphe;
		this.ppIndex = ppIndex;
	}

	public PhraseCompileInfo(Livre.Chapitre chapitre, int cIndex)
	{
		this.chapitre = chapitre;
		this.cIndex = cIndex;
	}

	public PhraseCompileInfo Add(Livre.Page page, int pIndex)
	{
		this.page = page;
		this.pIndex = pIndex;
		return this;
	}

	public PhraseCompileInfo Add(Livre.Paragraphe paragraphe, int ppIndex)
	{
		this.paragraphe = paragraphe;
		this.ppIndex = ppIndex;
		return this;
	}
}
