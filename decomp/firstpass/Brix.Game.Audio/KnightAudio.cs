namespace Brix.Game.Audio;

public class KnightAudio : ProfessionAudio
{
	public override BricktronSoundEffects GetSoundEffects()
	{
		BricktronSoundEffects bricktronSoundEffects = new BricktronSoundEffects();
		bricktronSoundEffects.AttackBlocked = "Knight_Attack_Blocked";
		bricktronSoundEffects.AttackHit = "Knight_Attack_Hit";
		bricktronSoundEffects.AttackSwing = "Knight_Attack_Swing";
		bricktronSoundEffects.Block = "Knight_Block";
		bricktronSoundEffects.Flying = "Knight_Flying";
		bricktronSoundEffects.Hurt = "Knight_Hurt";
		bricktronSoundEffects.ActionAttack = "Knight_Action_Attack";
		bricktronSoundEffects.Go = "Knight_Go";
		bricktronSoundEffects.Impossible = "Knight_Impossible";
		bricktronSoundEffects.SpotEnemy = "Knight_Spot_Enemy";
		bricktronSoundEffects.ClimbUp = "Knight_ClimbUp";
		bricktronSoundEffects.ClimbDown = "Knight_ClimbDown";
		bricktronSoundEffects.ClimbDown_Land = "Knight_ClimbDown_Land";
		bricktronSoundEffects.Falls = "Knight_Falls";
		bricktronSoundEffects.FootstepRun = "Footstep_Knight_Run";
		bricktronSoundEffects.FootstepWalk = "Footstep_Knight_Walk";
		bricktronSoundEffects.Select = "Knight_Single_Select";
		bricktronSoundEffects.MultiSelect = "Bricktron_Multi_Select";
		bricktronSoundEffects.Spawn = "Bricktron_Spawn";
		bricktronSoundEffects.Equip = "Bricktron_Knight_Equip";
		bricktronSoundEffects.Death = "Bricktron_Death";
		bricktronSoundEffects.Disintegrate = "Bricktron_Disintegrate";
		bricktronSoundEffects.FallsLand = "Bricktron_Falls_Land";
		bricktronSoundEffects.ActionDefend = "Knight_Action_Defend";
		bricktronSoundEffects.AttackKill = "Knight_Attack_Kill";
		bricktronSoundEffects.AttackMiss = "Knight_Attack_Miss";
		bricktronSoundEffects.AttackRun = "Knight_Attack_Run";
		bricktronSoundEffects.AttackSwordHitFront = "Knight_Attack_SwordHit_Front";
		bricktronSoundEffects.AttackSwordHitLeft = "Knight_Attack_SwordHit_Left";
		bricktronSoundEffects.AttackSwordHitRight = "Knight_Attack_SwordHit_Right";
		bricktronSoundEffects.TaskBar = "Taskbar_Knight_Select";
		bricktronSoundEffects.Unequip = "Bricktron_Knight_Unequip";
		return bricktronSoundEffects;
	}
}
