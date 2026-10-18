namespace Brix.Game.Audio;

public class MinitronAudio : ProfessionAudio
{
	public override BricktronSoundEffects GetSoundEffects()
	{
		BricktronSoundEffects bricktronSoundEffects = new BricktronSoundEffects();
		bricktronSoundEffects.AttackHit = "Minitron_Attack_Hit";
		bricktronSoundEffects.AttackKill = "Minitron_Attack_Kill";
		bricktronSoundEffects.AttackMiss = "Minitron_Attack_Miss";
		bricktronSoundEffects.Death = "Minitron_Death";
		bricktronSoundEffects.DestroysSurface = "Minitron_Destroys_Surface";
		bricktronSoundEffects.Disintegrate = "Minitron_Disintegrate";
		bricktronSoundEffects.Falls = "Minitron_Falls";
		bricktronSoundEffects.FallsLand = "Minitron_Falls_Land";
		bricktronSoundEffects.Flying = "Minitron_Flying";
		bricktronSoundEffects.GetUp = "Minitron_GetUp";
		bricktronSoundEffects.HitsSurface = "Minitron_Hits_Surface";
		bricktronSoundEffects.Hurt = "Minitron_Hurt";
		bricktronSoundEffects.Idle = "Minitron_Idle";
		bricktronSoundEffects.Interested = "Minitron_Interested";
		bricktronSoundEffects.Pursuit = "Minitron_Pursuit";
		bricktronSoundEffects.Scared = "Minitron_Scared";
		bricktronSoundEffects.SpotEnemy = "Minitron_Spot";
		bricktronSoundEffects.Surprise = "Minitron_Surprise";
		bricktronSoundEffects.Wander = "Minitron_Wander";
		return bricktronSoundEffects;
	}
}
