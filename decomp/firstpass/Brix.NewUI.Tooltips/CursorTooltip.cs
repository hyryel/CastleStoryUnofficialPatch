using System.Collections.Generic;
using Brix.External.Factories;
using Brix.Game.Components;
using Brix.Game.Storage;
using Brix.Utils;
using UnityEngine;
using UnityEngine.UI;

namespace Brix.NewUI.Tooltips;

public class CursorTooltip : MonoBehaviour
{
	private static Dictionary<Factory.AssetKey, string> titles = new Dictionary<Factory.AssetKey, string>
	{
		{
			Blueprints.BearTrap,
			"##cursortip_beartrap"
		},
		{
			Blueprints.Barrel,
			"##cursortip_explosivebarrel"
		},
		{
			Blueprints.Catapult,
			"##cursortip_catapult"
		},
		{
			Blueprints.Door,
			"##cursortip_door"
		},
		{
			Blueprints.Pallet,
			"##cursortip_stockpile"
		},
		{
			Blueprints.Toolrack,
			"##cursortip_weaponstand"
		},
		{
			Blueprints.CaptureWard,
			"##cursortip_captureward"
		},
		{
			Blueprints.HealingWard,
			"##cursortip_healingward"
		},
		{
			Blueprints.LanternWard,
			"##cursortip_lanternward"
		},
		{
			Blueprints.LanternWall,
			"##cursortip_lanternward"
		},
		{
			Blueprints.SentinelWard,
			"##cursortip_sentinelward"
		},
		{
			Blueprints.PylonGround,
			"##cursortip_pylonward"
		},
		{
			Blueprints.PylonWall,
			"##cursortip_pylonward"
		},
		{
			Blueprints.Altar,
			"##cursortip_altar"
		},
		{
			Blueprints.Loom,
			"##cursortip_loom"
		},
		{
			Blueprints.Furnace,
			"##cursortip_furnace"
		},
		{
			Blueprints.Forge,
			"##cursortip_forge"
		},
		{
			Blueprints.Lab,
			"##cursortip_lab"
		},
		{
			Blueprints.MachineShop,
			"##cursortip_machineshop"
		},
		{
			Blueprints.ChoppingBlock,
			"##cursortip_choppingblock"
		},
		{
			Blueprints.Workbench,
			"##cursortip_workbench"
		},
		{
			Blueprints.RopeBridgeFrame,
			"##cursortip_ropebridge"
		},
		{
			Blueprints.RopeBridgePlank,
			"##cursortip_ropebridge"
		}
	};

	[SerializeField]
	private Text Title;

	[SerializeField]
	private GameObject Content;

	[SerializeField]
	private StorageUI StorageUi;

	private float duration = 0.1f;

	private float until;

	public static CursorTooltip instance { get; set; }

	private void Awake()
	{
		instance = this;
		base.gameObject.SetActive(value: false);
		StorageUi.NoButtonBackground = true;
		StorageUi.mode = StorageUI.Mode.blueprint;
	}

	private void Update()
	{
		if (Time.time > until)
		{
			base.gameObject.SetActive(value: false);
			Content.SetActive(value: false);
		}
		else
		{
			Content.SetActive(value: true);
			base.transform.position = UnityEngine.Input.mousePosition;
		}
	}

	private void Show()
	{
		base.gameObject.SetActive(value: true);
		until = float.MaxValue;
	}

	public void Hide()
	{
		until = Time.time + duration;
	}

	public void ShowBlueprintRequirement(Blueprint blueprint)
	{
		string valueOrDefault = titles.GetValueOrDefault(blueprint.AssetKey, "##gamemenu_taskbar_required");
		string text = I2Helper.TryGet(valueOrDefault);
		Title.text = text;
		StorageUi.SetBlueprintRecepteur(blueprint.recepteur);
		Show();
	}
}
