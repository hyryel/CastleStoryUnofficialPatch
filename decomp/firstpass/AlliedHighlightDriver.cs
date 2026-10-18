public class AlliedHighlightDriver : CharacterHighlightDriver
{
	protected override SelectionCircle.MainColors mainColor => SelectionCircle.MainColors.Allies;

	protected override UIGameObserver.Bricktron observer => UIGameObserver.alliedBricktrons;

	protected override bool testFog => true;
}
