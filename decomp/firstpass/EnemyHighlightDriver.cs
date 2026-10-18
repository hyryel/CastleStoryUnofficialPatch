public class EnemyHighlightDriver : CharacterHighlightDriver
{
	protected override SelectionCircle.MainColors mainColor => SelectionCircle.MainColors.Enemies;

	protected override UIGameObserver.Bricktron observer => UIGameObserver.enemyBricktrons;

	protected override bool testFog => true;
}
