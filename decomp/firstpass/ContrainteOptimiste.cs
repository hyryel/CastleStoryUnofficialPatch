using Brix.Game.Components;

public class ContrainteOptimiste : Contrainte
{
	public override ReponseContrainte Match(GameComponent component)
	{
		return ReponseContrainte.Positif;
	}
}
