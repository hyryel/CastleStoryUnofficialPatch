using Brix.Core;

namespace Brix.Engine;

public class DoodadDecorator<Doodad> : IDoodad where Doodad : IDoodad
{
	public IDoodad iDoodad;

	public virtual int GetScalaire()
	{
		return iDoodad.GetScalaire();
	}

	public virtual string GetNom()
	{
		return iDoodad.GetNom();
	}
}
