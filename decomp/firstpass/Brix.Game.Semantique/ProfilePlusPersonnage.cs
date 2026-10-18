using Brix.Game.AI;
using Brix.Game.Components;
using Smooth.Slinq;

namespace Brix.Game.Semantique;

public class ProfilePlusPersonnage : Profile
{
	public ProfilePlusPersonnage(Description description)
	{
		base.description = description;
	}

	public override bool MatchApparence(Apparence apparence)
	{
		if (base.MatchApparence(apparence))
		{
			return true;
		}
		if (apparence.recepteur == null || apparence.recepteur.Fonction != FonctionDeContenant.personnage)
		{
			return false;
		}
		Labor component = apparence.GetComponent<Labor>();
		if (component != null && component.recepteur.StoredItems.Slinq().Any((IDescriptor d, ProfilePlusPersonnage self) => self.MatchComponent(d as Apparence), this))
		{
			return true;
		}
		return false;
	}
}
