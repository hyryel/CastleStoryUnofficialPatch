namespace Brix.Game.Audio;

internal class ArtificerAudio : ProfessionAudio
{
	public override BricktronSoundEffects GetSoundEffects()
	{
		BricktronSoundEffects bricktronSoundEffects = new BricktronSoundEffects();
		bricktronSoundEffects.Shoot = "Artificier_Shoot";
		bricktronSoundEffects.ArrowHit = "Sentinel_Ward_Shoot_Hit";
		bricktronSoundEffects.Flying = "Archer_Flying";
		bricktronSoundEffects.Hurt = "Artificier_Hurt";
		bricktronSoundEffects.Go = "Archer_Go";
		bricktronSoundEffects.Impossible = "Archer_Impossible";
		bricktronSoundEffects.TargetHit = "Archer_Target_Hit";
		bricktronSoundEffects.TargetMiss = "Archer_Target_Miss";
		bricktronSoundEffects.SpotEnemy = "Archer_Spot_Enemy";
		bricktronSoundEffects.Falls = "Archer_Falls";
		bricktronSoundEffects.ClimbUp = "Archer_ClimbUp";
		bricktronSoundEffects.ClimbDown = "Archer_ClimbDown";
		bricktronSoundEffects.ClimbDown_Land = "Archer_ClimbDown_Land";
		bricktronSoundEffects.FootstepRun = "Footstep_Artificier_Walk";
		bricktronSoundEffects.FootstepWalk = "Footstep_Artificier_Run";
		bricktronSoundEffects.Fail = "Archer_Fail";
		bricktronSoundEffects.Select = "Archer_Single_Select";
		bricktronSoundEffects.MultiSelect = "Bricktron_Multi_Select";
		bricktronSoundEffects.Spawn = "Bricktron_Spawn";
		bricktronSoundEffects.Equip = "Bricktron_Artificer_Equip";
		bricktronSoundEffects.Death = "Bricktron_Death";
		bricktronSoundEffects.Disintegrate = "Bricktron_Disintegrate";
		bricktronSoundEffects.FallsLand = "Bricktron_Falls_Land";
		return bricktronSoundEffects;
	}
}
