namespace Brix.Game.Audio;

public class CorruptronAudio : ProfessionAudio
{
	public override BricktronSoundEffects GetSoundEffects()
	{
		BricktronSoundEffects bricktronSoundEffects = new BricktronSoundEffects();
		bricktronSoundEffects.AttackHit = "Corruptron_Attack_Hit";
		bricktronSoundEffects.AttackKill = "Corruptron_Attack_Kill";
		bricktronSoundEffects.AttackMiss = "Corruptron_Attack_Miss";
		bricktronSoundEffects.Death = "Corruptron_Death";
		bricktronSoundEffects.DestroysSurface = "Corruptron_Destroys_Surface";
		bricktronSoundEffects.Disintegrate = "Corruptron_Disintegrate";
		bricktronSoundEffects.Falls = "Corruptron_Falls";
		bricktronSoundEffects.FallsLand = "Corruptron_Falls_Land";
		bricktronSoundEffects.Flying = "Corruptron_Flying";
		bricktronSoundEffects.GetUp = "Corruptron_GetUp";
		bricktronSoundEffects.HitsSurface = "Minitron_Hits_Surface";
		bricktronSoundEffects.Hurt = "Corruptron_Hurt";
		bricktronSoundEffects.Idle = "Corruptron_Idle";
		bricktronSoundEffects.Interested = "Corruptron_Interested";
		bricktronSoundEffects.Pursuit = "Corruptron_Pursuit";
		bricktronSoundEffects.Scared = "Corruptron_Scared";
		bricktronSoundEffects.SpotEnemy = "Corruptron_Spot";
		bricktronSoundEffects.Surprise = "Corruptron_Surprise";
		bricktronSoundEffects.Wander = "Corruptron_Wander";
		return bricktronSoundEffects;
	}
}
