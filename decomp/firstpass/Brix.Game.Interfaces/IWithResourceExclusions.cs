using System.Collections.Generic;
using Brix.Game.AI;

namespace Brix.Game.Interfaces;

public interface IWithResourceExclusions
{
	bool IsExcluding(Ressource r);

	bool HasExclusions();

	void AddExclusion(Ressource r);

	void RemoveExclusion(Ressource r);

	void ToggleExclusion(Ressource r);

	void MakeExclusive(IEnumerable<Ressource> ressources);

	void ClearExclusions();
}
