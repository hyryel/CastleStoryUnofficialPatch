namespace Brix.Game.Audio;

public class BuilderAudio : ProfessionAudio
{
	public override BricktronSoundEffects GetSoundEffects()
	{
		BricktronSoundEffects bricktronSoundEffects = new BricktronSoundEffects();
		bricktronSoundEffects.Abandon = "Bricktron_Abandon";
		bricktronSoundEffects.ActionBuild = "Bricktron_Action_Build";
		bricktronSoundEffects.ActionClean = "Bricktron_Action_Clean";
		bricktronSoundEffects.ActionLumber = "Bricktron_Action_Lumber";
		bricktronSoundEffects.ActionMine = "Bricktron_Action_Mine";
		bricktronSoundEffects.ActionRepair = "Bricktron_Action_Repair";
		bricktronSoundEffects.ClimbUp = "Bricktron_ClimbUp";
		bricktronSoundEffects.ClimbDown = "Bricktron_ClimbDown";
		bricktronSoundEffects.ClimbDown_Land = "Bricktron_ClimbDown_Land";
		bricktronSoundEffects.Death = "Bricktron_Death";
		bricktronSoundEffects.DestroysSurface = "Bricktron_Destroys_Surface";
		bricktronSoundEffects.Disintegrate = "Bricktron_Disintegrate";
		bricktronSoundEffects.DropObject = "Bricktron_Drop";
		bricktronSoundEffects.Falls = "Bricktron_Falls";
		bricktronSoundEffects.FallsLand = "Bricktron_Falls_Land";
		bricktronSoundEffects.Flying = "Bricktron_Flying";
		bricktronSoundEffects.FootstepRun = "Footstep_Bricktron_Run";
		bricktronSoundEffects.FootstepWalk = "Footstep_Bricktron_Walk";
		bricktronSoundEffects.GetUp = "Bricktron_GetUp_Back";
		bricktronSoundEffects.Go = "Bricktron_Go";
		bricktronSoundEffects.GrabsObject = "Bricktron_Grabs_Object";
		bricktronSoundEffects.HitsSurface = "Bricktron_Object_Hit";
		bricktronSoundEffects.Hurt = "Bricktron_Hurt";
		bricktronSoundEffects.Impossible = "Bricktron_Impossible";
		bricktronSoundEffects.MultiSelect = "Bricktron_Multi_Select";
		bricktronSoundEffects.Panic = "Bricktron_Panic";
		bricktronSoundEffects.PassObject = "Bricktron_Pass_Object";
		bricktronSoundEffects.PlacesBlock = "Bricktron_Places_Block";
		bricktronSoundEffects.Recall = "Bricktron_Recall";
		bricktronSoundEffects.Scared = "Bricktron_Scared";
		bricktronSoundEffects.Select = "Bricktron_Single_Select";
		bricktronSoundEffects.Spawn = "Bricktron_Spawn";
		bricktronSoundEffects.SpotEnemy = "Bricktron_Spot_Enemy";
		bricktronSoundEffects.SwitchTask = "Bricktron_Switch_Task";
		bricktronSoundEffects.TaskBar = "Taskbar_Worker_Select";
		bricktronSoundEffects.TaskCompleted = "Bricktron_Task_Completed";
		return bricktronSoundEffects;
	}
}
