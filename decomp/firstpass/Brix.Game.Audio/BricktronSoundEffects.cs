using System.Collections.Generic;
using System.Reflection;

namespace Brix.Game.Audio;

public class BricktronSoundEffects
{
	public string Abandon;

	public string ActionAttack;

	public string AttackBlocked;

	public string ActionBuild;

	public string ActionClean;

	public string ActionDefend;

	public string ActionLumber;

	public string ActionRepair;

	public string ArrowHit;

	public string AttackHit;

	public string AttackKill;

	public string ActionMine;

	public string AttackMiss;

	public string AttackRun;

	public string AttackSwing;

	public string AttackSwordHitFront;

	public string AttackSwordHitLeft;

	public string AttackSwordHitRight;

	public string Block;

	public string ClimbUp;

	public string ClimbDown;

	public string ClimbDown_Land;

	public string Death;

	public string DestroysSurface;

	public string Disintegrate;

	public string DropObject;

	public string Equip;

	public string Fail;

	public string Falls;

	public string FallsLand;

	public string Flying;

	public string FootstepRun;

	public string FootstepWalk;

	public string GetUp;

	public string Go;

	public string GrabsObject;

	public string Grunt;

	public string HitsSurface;

	public string Hurt;

	public string Idle;

	public string Impossible;

	public string Interested;

	public string LoadArrow;

	public string MultiSelect;

	public string Panic;

	public string PassObject;

	public string PlacesBlock;

	public string PullBow;

	public string Pursuit;

	public string Recall;

	public string Scared;

	public string Select;

	public string Shoot;

	public string Spawn;

	public string SpotEnemy;

	public string Surprise;

	public string SwitchTask;

	public string TargetHit;

	public string TargetMiss;

	public string TaskBar;

	public string TaskCompleted;

	public string Unequip;

	public string Wander;

	private Dictionary<string, string> _soundsByName;

	public Dictionary<string, string> SoundsByName
	{
		get
		{
			if (_soundsByName == null)
			{
				_soundsByName = new Dictionary<string, string>();
				FieldInfo[] fields = GetType().GetFields();
				foreach (FieldInfo fieldInfo in fields)
				{
					_soundsByName[fieldInfo.Name] = fieldInfo.GetValue(this) as string;
				}
			}
			return _soundsByName;
		}
	}
}
