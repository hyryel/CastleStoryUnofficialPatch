namespace Brix.Game.Audio;

public class BiftronAudio : ProfessionAudio
{
	public override BricktronSoundEffects GetSoundEffects()
	{
		BricktronSoundEffects bricktronSoundEffects = new BricktronSoundEffects();
		bricktronSoundEffects.AttackBlocked = "Biftron_Attack_Blocked";
		bricktronSoundEffects.AttackHit = "Biftron_Attack_Hit";
		bricktronSoundEffects.AttackKill = "Biftron_Attack_Kill";
		bricktronSoundEffects.AttackMiss = "Biftron_Attack_Miss";
		bricktronSoundEffects.Death = "Biftron_Death";
		bricktronSoundEffects.DestroysSurface = "Biftron_Destroys_Surface";
		bricktronSoundEffects.Disintegrate = "Biftron_Disintegrate";
		bricktronSoundEffects.Falls = "Biftron_Falls";
		bricktronSoundEffects.FallsLand = "Biftron_Falls_Land";
		bricktronSoundEffects.Flying = "Biftron_Flying";
		bricktronSoundEffects.GetUp = "Biftron_GetUp";
		bricktronSoundEffects.Grunt = "Biftron_Grunt";
		bricktronSoundEffects.HitsSurface = "Biftron_Hits_Surface";
		bricktronSoundEffects.Hurt = "Biftron_Hurt";
		bricktronSoundEffects.Idle = "Biftron_Idle";
		bricktronSoundEffects.SpotEnemy = "Biftron_Spot";
		return bricktronSoundEffects;
	}
}
