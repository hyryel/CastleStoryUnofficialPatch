using Brix.Game.Components;

public class Contrainte
{
	public virtual ReponseContrainte Match(GameComponent component)
	{
		return ReponseContrainte.Neutre;
	}
}
