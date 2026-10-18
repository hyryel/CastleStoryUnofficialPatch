using System;
using System.Collections.Generic;
using System.Linq;

namespace Brix.Engine;

internal class CalculateurDeColonnes<Handle>
{
	public IEnumerable<Handle> handles = null;

	public Func<Handle, int> GetXY = null;

	public Func<Handle, int> GetBas = null;

	public Func<Handle, int> GetHaut = null;

	public Dictionary<int, IntRange> GetColonneDico()
	{
		return handles.ToDictionary(GetXY, (Handle h) => new IntRange
		{
			bas = GetBas(h),
			haut = GetHaut(h)
		});
	}
}
