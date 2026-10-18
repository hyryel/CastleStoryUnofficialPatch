namespace Brix.Legacy;

public class Roche : BlocLibre
{
	public override bool CheckTag(string tag)
	{
		return "Roche Ramassable".Contains(tag);
	}
}
