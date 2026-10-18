using System.Collections.Generic;
using Brix.Assets;
using Brix.Components;
using Brix.Engine;
using Brix.External.Factories;
using Brix.External.Signals;
using Brix.Game;
using Brix.Lifecycle.Pooling;
using Smooth.Slinq;
using UnityEngine;
using UnityEngine.Networking;

namespace Brix.Network;

internal static class SimpleHostScript
{
	public static Signal AllFactionsFirstReady = new Signal();

	public static void Run()
	{
		Asset_Gamemode.FactionPrototype[] prototypes = GameParam.Gamemode.Disk_ReadFactionPrototypes();
		List<Faction> factions = new List<Faction>(prototypes.Length);
		for (int i = 0; i != prototypes.Length; i++)
		{
			factions.Add(prototypes[i].CreateFaction());
		}
		if (factions == null || factions.Count == 0)
		{
			return;
		}
		List<Faction> playableFactions = new List<Faction>();
		if (GameParam.Map.IsNewGame())
		{
			playableFactions = factions;
		}
		else
		{
			Faction.FactionMeta[] array = GameParam.Map.Disk_ReadFactionMeta();
			if (array == null)
			{
				playableFactions = factions;
			}
			else
			{
				int i2 = 0;
				array.Slinq().ForEach(delegate(Faction.FactionMeta meta)
				{
					if (meta.assigned)
					{
						playableFactions.Add(factions[i2]);
					}
					i2++;
				});
			}
		}
		for (int num = 0; num < factions.Count; num++)
		{
			NetworkServer.Spawn(factions[num].gameObject, factions[num].AssetKey.AssetId);
		}
		for (int i3 = 0; i3 != prototypes.Length; i3++)
		{
			Faction fac = factions.Slinq().First((Faction x) => x.ID == prototypes[i3].uid);
			prototypes[i3].PopulateFaction(fac);
		}
		if (UNetManager.instance.debugLoader)
		{
			factions[0].AddOwner(User.LocalUser);
		}
		else
		{
			if (!Neo.IsMultiplayer && !GameParam.Map.IsNewGame())
			{
				User.LocalUser.factionNumber = Faction.GetFirstAvalaibleFaction();
			}
			foreach (User player in UNetManager.instance.UserList)
			{
				playableFactions[player.factionNumber].AddOwner(UNetManager.instance.GetUserFromConnection(player.connectionToClient));
				UNetManager.instance.UserList.Slinq().ForEach(delegate(User otherPlayer)
				{
					if (otherPlayer.TeamNumber == player.TeamNumber)
					{
						playableFactions[player.factionNumber].SetFactionRelation(playableFactions[otherPlayer.factionNumber], Affiliation.Relation.Allied);
					}
					else
					{
						playableFactions[player.factionNumber].SetFactionRelation(playableFactions[otherPlayer.factionNumber], Affiliation.Relation.Enemy);
					}
				});
				playableFactions[player.factionNumber].SetFactionRelation(Faction.DefaultEnemy, Affiliation.Relation.Enemy);
				playableFactions[player.factionNumber].SetFactionRelation(Faction.DefaultNeutral, Affiliation.Relation.Neutral);
				Faction.DefaultEnemy.SetFactionRelation(playableFactions[player.factionNumber], Affiliation.Relation.Enemy);
				Faction.DefaultNeutral.SetFactionRelation(playableFactions[player.factionNumber], Affiliation.Relation.Neutral);
			}
			Faction.DefaultEnemy.SetFactionRelation(Faction.DefaultNeutral, Affiliation.Relation.Neutral);
			Faction.DefaultNeutral.SetFactionRelation(Faction.DefaultEnemy, Affiliation.Relation.Neutral);
		}
		BrixSingleton<AutoList>.Instance.GetInstances(Groupes.Idle).Slinq().ForEach(delegate(GameObject g)
		{
			g.GetComponent<NetworkIdentity>().RebuildObservers(initialize: false);
		});
		GameParam.instance.NetworkHostLoadedAllFactions = true;
	}

	public static void PopulateFactionsLocally()
	{
		Asset_Gamemode.FactionPrototype[] prototypes = GameParam.Gamemode.Disk_ReadFactionPrototypes();
		List<Faction> allFactions = Faction.GetAllFactions();
		for (int i = 0; i != prototypes.Length; i++)
		{
			Faction fac = allFactions.Slinq().First((Faction x) => x.ID == prototypes[i].uid);
			prototypes[i].PopulateFaction(fac);
		}
	}
}
You are not using the latest version of the tool, please update.
Latest version is '11.1.0.9782' (yours is '9.1.0.7988')
