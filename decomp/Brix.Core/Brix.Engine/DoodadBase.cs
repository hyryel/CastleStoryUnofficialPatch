using Brix.Core;

namespace Brix.Engine;

public class DoodadBase : IDoodad
{
	public string nom;

	public int scalaire;

	public string GetNom()
	{
		return nom;
	}

	public int GetScalaire()
	{
		return scalaire;
	}

	public DoodadBase()
	{
	}

	public DoodadBase(IDoodad iDoodad)
	{
		nom = iDoodad.GetNom();
		scalaire = iDoodad.GetScalaire();
	}
}
