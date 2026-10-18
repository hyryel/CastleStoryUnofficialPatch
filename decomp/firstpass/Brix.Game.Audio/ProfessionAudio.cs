namespace Brix.Game.Audio;

public abstract class ProfessionAudio
{
	private BricktronSoundEffects _sounds;

	public BricktronSoundEffects Sounds => _sounds ?? (_sounds = GetSoundEffects());

	public abstract BricktronSoundEffects GetSoundEffects();
}
