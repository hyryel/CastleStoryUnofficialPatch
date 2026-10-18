namespace Brix.Game.Semantique;

public class NoneProfile : Profile
{
	private static Profile _singleton = new NoneProfile();

	public static Profile Default => _singleton;

	public override bool MatchApparence(Apparence apparence)
	{
		return false;
	}
}
