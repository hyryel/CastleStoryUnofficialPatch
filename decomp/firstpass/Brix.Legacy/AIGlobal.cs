using System;
using Brix.External.Factories;
using Brix.Game;
using Brix.Game.AI;
using Brix.Game.Network;
using Brix.Input;
using Brix.Transactions;
using Smooth.Slinq;
using UnityEngine;

namespace Brix.Legacy;

public class AIGlobal : MonoBehaviour
{
	private static void SpawnKit(params Factory.AssetKey[] keys)
	{
		keys.Slinq().ForEach(delegate(Factory.AssetKey k)
		{
			Transactor.SpawnUNet(k, Faction.DefaultAllied, CameraScript.Target, Quaternion.identity);
		});
	}

	public static void Spawn(Factory.AssetKey assetKey, Vector3 position, Quaternion rotation, Faction faction, Occupation.Type occupation, bool equipGear)
	{
		position = new Vector3(Mathf.Round(position.x), position.y, Mathf.Round(position.z));
		User.LocalUser.GetComponent<UNetProjectCmd>().CallCmdSpawn(assetKey.Factory, assetKey.Name, position, rotation, faction.gameObject, occupation, equipGear);
	}

	public static void SpawnWith(Func<UNetProjectCmd, Action<string, string, Vector3, Quaternion, GameObject>> func)
	{
		func(User.LocalUser.GetComponent<UNetProjectCmd>())(Test.Bricktron.Factory, Test.Bricktron.Name, CameraScript.Target, Quaternion.identity, Faction.DefaultAllied.gameObject);
	}
}
