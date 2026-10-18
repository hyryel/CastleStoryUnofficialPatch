namespace Brix.Game.Audio;

internal class AlchemistAudio : ProfessionAudio
{
	public override BricktronSoundEffects GetSoundEffects()
	{
		BricktronSoundEffects bricktronSoundEffects = new BricktronSoundEffects();
		bricktronSoundEffects.Shoot = "Alchemist_Attack_Shoot";
		bricktronSoundEffects.Flying = "Archer_Flying";
		bricktronSoundEffects.Hurt = "Alchemist_Hurt";
		bricktronSoundEffects.Go = "Archer_Go";
		bricktronSoundEffects.Impossible = "Archer_Impossible";
		bricktronSoundEffects.TargetHit = "Archer_Target_Hit";
		bricktronSoundEffects.TargetMiss = "Archer_Target_Miss";
		bricktronSoundEffects.SpotEnemy = "Archer_Spot_Enemy";
		bricktronSoundEffects.ClimbUp = "Archer_ClimbUp";
		bricktronSoundEffects.ClimbDown = "Archer_ClimbDown";
		bricktronSoundEffects.ClimbDown_Land = "Archer_ClimbDown_Land";
		bricktronSoundEffects.LoadArrow = "Alchemist_Attack_Prepare";
		bricktronSoundEffects.Falls = "Archer_Falls";
		bricktronSoundEffects.FootstepRun = "Footstep_Alchemist_Run";
		bricktronSoundEffects.FootstepWalk = "Footstep_Alchemist_Walk";
		bricktronSoundEffects.Fail = "Archer_Fail";
		bricktronSoundEffects.Select = "Archer_Single_Select";
		bricktronSoundEffects.MultiSelect = "Bricktron_Multi_Select";
		bricktronSoundEffects.Spawn = "Bricktron_Spawn";
		bricktronSoundEffects.Equip = "Bricktron_Alchemist_Equip";
		bricktronSoundEffects.Death = "Bricktron_Death";
		bricktronSoundEffects.Disintegrate = "Bricktron_Disintegrate";
		bricktronSoundEffects.FallsLand = "Bricktron_Falls_Land";
		return bricktronSoundEffects;
	}
}
