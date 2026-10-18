using Brix.Core;

namespace Brix.Engine;

public class ArbreDecorator<Arbre> : IArbre where Arbre : IArbre
{
	public IArbre iArbre;

	public virtual int GetScalaire()
	{
		return iArbre.GetScalaire();
	}
}
