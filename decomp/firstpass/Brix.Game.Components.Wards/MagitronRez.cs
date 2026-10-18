using System.Collections.Generic;
using Brix.Engine;
using Brix.External.Factories;
using Brix.Game.AI;
using Brix.Game.Network;
using Brix.Network;
using UnityEngine;
using UnityEngine.Networking;

namespace Brix.Game.Components.Wards;

public class MagitronRez : NetworkGameComponent
{
	public float CoolDown = 40f;

	public float SpawnRadius = 10f;

	[SerializeField]
	private Labor labor;

	private int _allyCount;

	private int _enemyCount;

	private WorkNodeGenerator<MagitronRez> generator;

	private bool _isOnCooldown;

	private bool _readyToRez;

	private float lastRez;

	private int _spawnProgress = -1;

	private static Factory.AssetKey[] _spawnList = new Factory.AssetKey[3]
	{
		Test.Corruptron,
		Test.Minitron,
		Test.Minitron
	};

	private static Dictionary<Factory.AssetKey, Occupation.Type> KeyToOccupation = new Dictionary<Factory.AssetKey, Occupation.Type>
	{
		{
			Test.Minitron,
			Occupation.Type.Minitron
		},
		{
			Test.Corruptron,
			Occupation.Type.Corruptron
		},
		{
			Test.Biftron,
			Occupation.Type.Biftron
		}
	};

	public bool IsOnCooldown => _isOnCooldown;

	public bool ReadyToRez => _readyToRez;

	public void UnitsRefresh(HashSet<Trackable> units)
	{
		_allyCount = 0;
		_enemyCount = 0;
		foreach (Trackable unit in units)
		{
			GameObject gameObject = ((!(unit != null)) ? null : unit.gameObject);
			if (gameObject != base.gameObject && faction.IsAlliedTo(gameObject))
			{
				_allyCount++;
			}
			else if (faction.IsEnemyTo(gameObject))
			{
				_enemyCount++;
			}
		}
	}

	private void FixedUpdate()
	{
		if (Time.fixedTime > lastRez + CoolDown && _allyCount < _enemyCount)
		{
			lastRez = Time.fixedTime;
			_isOnCooldown = false;
			_readyToRez = true;
		}
		if (_spawnProgress >= 0)
		{
			if (_spawnProgress < _spawnList.Length)
			{
				Spawn(_spawnList[_spawnProgress]);
				_spawnProgress++;
			}
			else
			{
				_spawnProgress = -1;
			}
		}
	}

	public void SpawnCorruptrons()
	{
		_spawnProgress = 0;
		_isOnCooldown = true;
		_readyToRez = false;
	}

	private void Spawn(Factory.AssetKey assetKey)
	{
		User.LocalUser.GetComponent<UNetProjectCmd>().CallCmdSpawnWithLevel(assetKey.Factory, assetKey.Name, Architecte.PigerUneCase(base.transform.position, SpawnRadius).ToVector3(), base.transform.rotation, faction.gameObject, KeyToOccupation[assetKey], equipGear: true, GetComponent<Profession>().CharacterLevel);
	}

	private void UNetVersion()
	{
	}

	public override bool OnSerialize(NetworkWriter writer, bool forceAll)
	{
		bool flag = base.OnSerialize(writer, forceAll);
		bool flag2 = default(bool);
		return flag2 || flag;
	}

	public override void OnDeserialize(NetworkReader reader, bool initialState)
	{
		base.OnDeserialize(reader, initialState);
	}
}
