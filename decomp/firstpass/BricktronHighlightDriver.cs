public class BricktronHighlightDriver : CharacterHighlightDriver
{
	protected override SelectionCircle.MainColors mainColor => SelectionCircle.MainColors.Local;

	protected override UIGameObserver.Bricktron observer => UIGameObserver.bricktrons;

	protected override bool testFog => false;
}
