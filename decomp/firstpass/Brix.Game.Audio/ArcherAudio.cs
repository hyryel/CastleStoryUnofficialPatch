namespace Brix.Game.Audio;

public class ArcherAudio : ProfessionAudio
{
	public override BricktronSoundEffects GetSoundEffects()
	{
		BricktronSoundEffects bricktronSoundEffects = new BricktronSoundEffects();
		bricktronSoundEffects.Shoot = "Archer_Attack_Shoot";
		bricktronSoundEffects.ArrowHit = "Arrow_Hit";
		bricktronSoundEffects.Flying = "Archer_Flying";
		bricktronSoundEffects.Hurt = "Archer_Hurt";
		bricktronSoundEffects.Go = "Archer_Go";
		bricktronSoundEffects.Impossible = "Archer_Impossible";
		bricktronSoundEffects.TargetHit = "Archer_Target_Hit";
		bricktronSoundEffects.TargetMiss = "Archer_Target_Miss";
		bricktronSoundEffects.SpotEnemy = "Archer_Spot_Enemy";
		bricktronSoundEffects.ClimbUp = "Archer_ClimbUp";
		bricktronSoundEffects.ClimbDown = "Archer_ClimbDown";
		bricktronSoundEffects.ClimbDown_Land = "Archer_ClimbDown_Land";
		bricktronSoundEffects.LoadArrow = "Archer_Load_Arrow";
		bricktronSoundEffects.Falls = "Archer_Falls";
		bricktronSoundEffects.FootstepRun = "Footstep_Archer_Run";
		bricktronSoundEffects.FootstepWalk = "Footstep_Archer_Walk";
		bricktronSoundEffects.Fail = "Archer_Fail";
		bricktronSoundEffects.Select = "Archer_Single_Select";
		bricktronSoundEffects.MultiSelect = "Bricktron_Multi_Select";
		bricktronSoundEffects.Spawn = "Bricktron_Spawn";
		bricktronSoundEffects.Equip = "Bricktron_Archer_Equip";
		bricktronSoundEffects.Death = "Bricktron_Death";
		bricktronSoundEffects.Disintegrate = "Bricktron_Disintegrate";
		bricktronSoundEffects.FallsLand = "Bricktron_Falls_Land";
		bricktronSoundEffects.PullBow = "Archer_Attack_Pull_Bow";
		bricktronSoundEffects.TaskBar = "Taskbar_Archer_Select";
		bricktronSoundEffects.Unequip = "Bricktron_Archer_Unequip";
		return bricktronSoundEffects;
	}
}
