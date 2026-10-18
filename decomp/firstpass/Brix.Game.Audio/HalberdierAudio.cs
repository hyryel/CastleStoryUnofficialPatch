namespace Brix.Game.Audio;

internal class HalberdierAudio : ProfessionAudio
{
	public override BricktronSoundEffects GetSoundEffects()
	{
		BricktronSoundEffects bricktronSoundEffects = new BricktronSoundEffects();
		bricktronSoundEffects.AttackBlocked = "Knight_Attack_Blocked";
		bricktronSoundEffects.AttackHit = "Halberd_Attack_Hit";
		bricktronSoundEffects.AttackSwing = "Halberd_Attack_Prepare";
		bricktronSoundEffects.Flying = "Knight_Flying";
		bricktronSoundEffects.Hurt = "Halberd_Hurt";
		bricktronSoundEffects.ActionAttack = "Knight_Action_Attack";
		bricktronSoundEffects.Go = "Knight_Go";
		bricktronSoundEffects.Impossible = "Knight_Impossible";
		bricktronSoundEffects.SpotEnemy = "Knight_Spot_Enemy";
		bricktronSoundEffects.ClimbUp = "Knight_ClimbUp";
		bricktronSoundEffects.ClimbDown = "Knight_ClimbDown";
		bricktronSoundEffects.ClimbDown_Land = "Knight_ClimbDown_Land";
		bricktronSoundEffects.Falls = "Knight_Falls";
		bricktronSoundEffects.FootstepRun = "Footstep_Halberd_Run";
		bricktronSoundEffects.FootstepWalk = "Footstep_Halberd_Walk";
		bricktronSoundEffects.Select = "Knight_Single_Select";
		bricktronSoundEffects.MultiSelect = "Bricktron_Multi_Select";
		bricktronSoundEffects.Spawn = "Bricktron_Spawn";
		bricktronSoundEffects.Equip = "Bricktron_Halberdier_Equip";
		bricktronSoundEffects.Death = "Bricktron_Death";
		bricktronSoundEffects.Disintegrate = "Bricktron_Disintegrate";
		bricktronSoundEffects.FallsLand = "Bricktron_Falls_Land";
		return bricktronSoundEffects;
	}
}
