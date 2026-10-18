namespace Brix.Game.Audio;

public class MagitronAudio : ProfessionAudio
{
	public override BricktronSoundEffects GetSoundEffects()
	{
		BricktronSoundEffects bricktronSoundEffects = new BricktronSoundEffects();
		bricktronSoundEffects.LoadArrow = "Warlock_Shoot_Prepare";
		bricktronSoundEffects.Shoot = "Warlock_Shoot_Attack";
		bricktronSoundEffects.Hurt = "Warlock_Hurt";
		bricktronSoundEffects.Flying = "Warlock_Flying";
		bricktronSoundEffects.Death = "Warlock_Death";
		bricktronSoundEffects.Disintegrate = "Warlock_Desintegrate";
		bricktronSoundEffects.Falls = "Warlock_Falls";
		bricktronSoundEffects.FallsLand = "Warlock_Falls_Land";
		bricktronSoundEffects.GetUp = "Warlock_GetUp";
		return bricktronSoundEffects;
	}
}
