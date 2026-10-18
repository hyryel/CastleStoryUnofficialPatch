using Brix.Core;

namespace Brix.Engine;

public class ArbreBase : IArbre
{
	public int scalaire;

	public int GetScalaire()
	{
		return scalaire;
	}

	public ArbreBase()
	{
	}

	public ArbreBase(IArbre iArbre)
	{
		scalaire = iArbre.GetScalaire();
	}
}
