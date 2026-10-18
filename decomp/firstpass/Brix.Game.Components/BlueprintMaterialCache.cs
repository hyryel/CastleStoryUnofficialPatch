using System;
using System.Collections.Generic;
using Brix.External.Factories;
using Smooth.Slinq;
using UnityEngine;

namespace Brix.Game.Components;

public class BlueprintMaterialCache : MonoBehaviour
{
	[Serializable]
	public struct MaterialTrio
	{
		public enum Mode
		{
			OnePass,
			TwoPass,
			ThreePass
		}

		[SerializeField]
		private Material[] mats;

		private Material[] twopass;

		private Material[] onepass;

		public Material prepass => mats[0];

		public Material main => mats[1];

		public Material underground => mats[2];

		public MaterialTrio(Material prepass, Material main, Material underground)
		{
			mats = new Material[3];
			mats[0] = prepass;
			mats[1] = main;
			mats[2] = underground;
			twopass = new Material[2];
			twopass[0] = main;
			twopass[1] = underground;
			onepass = new Material[1];
			onepass[0] = main;
		}

		public Material[] WithPasses(Mode passes)
		{
			return passes switch
			{
				Mode.OnePass => onepass, 
				Mode.TwoPass => twopass, 
				Mode.ThreePass => mats, 
				_ => throw new ArgumentOutOfRangeException(), 
			};
		}
	}

	public class BlueprintDisplay
	{
		private string str;

		private MaterialTrio references;

		public Color color;

		private float scale;

		private Dictionary<Factory.AssetKey, MaterialTrio> byKey = new Dictionary<Factory.AssetKey, MaterialTrio>();

		public float Scale => scale;

		public BlueprintDisplay(string str, MaterialTrio references, Color color, float scale)
		{
			this.str = str;
			this.references = references;
			this.color = color;
			this.scale = scale;
		}

		public MaterialTrio ForKey(Factory.AssetKey key, bool UseCache)
		{
			if (!UseCache)
			{
				return CreateTrio(key);
			}
			if (!byKey.ContainsKey(key))
			{
				byKey.Add(key, CreateTrio(key));
			}
			return byKey[key];
		}

		private MaterialTrio CreateTrio(Factory.AssetKey key)
		{
			Texture tex = UVMapFor(key);
			Material material = new Material(references.prepass);
			SetNormalMap(material, tex);
			material.color = color;
			Material material2 = new Material(references.main);
			SetNormalMap(material2, tex);
			material2.color = color;
			Material material3 = new Material(references.underground);
			SetNormalMap(material3, tex);
			material3.color = color;
			return new MaterialTrio(material, material2, material3);
		}

		private Texture UVMapFor(Factory.AssetKey key)
		{
			StateRenderer component = Factory.Peek(key).GetComponent<StateRenderer>();
			return component.NormalMap;
		}

		private void SetNormalMap(Material mat, Texture tex)
		{
			if ((bool)tex)
			{
				if (mat.HasProperty("_Normal"))
				{
					mat.SetTexture("_Normal", tex);
				}
				else if (mat.HasProperty("_Normals"))
				{
					mat.SetTexture("_Normals", tex);
				}
			}
		}

		public override string ToString()
		{
			return str;
		}

		public void ClearCache()
		{
			byKey.Slinq().ForEach(delegate(KeyValuePair<Factory.AssetKey, MaterialTrio> materialTrioPerAssetKey)
			{
				UnityEngine.Object.Destroy(materialTrioPerAssetKey.Value.main);
				UnityEngine.Object.Destroy(materialTrioPerAssetKey.Value.prepass);
				UnityEngine.Object.Destroy(materialTrioPerAssetKey.Value.underground);
			});
		}
	}

	[SerializeField]
	private MaterialTrio Buildable;

	[SerializeField]
	private MaterialTrio NonBuildable;

	[SerializeField]
	private Color LightBlue;

	[SerializeField]
	private Color FunBlue;

	[SerializeField]
	private Color Purple;

	[SerializeField]
	private Color Red;

	[SerializeField]
	private Color Gray;

	[SerializeField]
	private Color Orange;

	[SerializeField]
	private Color Yellow;

	private BlueprintDisplay placed_invalid;

	private BlueprintDisplay placed_anti;

	private BlueprintDisplay placed_valid;

	private BlueprintDisplay placed_buildable;

	private BlueprintDisplay placed_invalid_uncommited;

	private BlueprintDisplay placed_anti_uncommited;

	private BlueprintDisplay placed_valid_uncommited;

	private BlueprintDisplay placed_buildable_uncommited;

	private BlueprintDisplay picking_remove;

	private BlueprintDisplay picking_invalid;

	private BlueprintDisplay picking_anti;

	private BlueprintDisplay picking_valid;

	private BlueprintDisplay picking_buildable;

	private static BlueprintMaterialCache instance;

	private void Awake()
	{
		instance = this;
		placed_invalid = new BlueprintDisplay("placed_invalid", NonBuildable, Red, 1f);
		placed_anti = new BlueprintDisplay("placed_anti", Buildable, Orange, 1.04f);
		placed_valid = new BlueprintDisplay("placed_valid", NonBuildable, FunBlue, 1f);
		placed_buildable = new BlueprintDisplay("placed_buildable", Buildable, FunBlue, 1f);
		placed_invalid_uncommited = new BlueprintDisplay("placed_invalid_uncommited", NonBuildable, Red, 1f);
		placed_anti_uncommited = new BlueprintDisplay("placed_anti_uncommited", Buildable, Orange, 1.04f);
		placed_valid_uncommited = new BlueprintDisplay("placed_valid_uncommited", NonBuildable, LightBlue, 1f);
		placed_buildable_uncommited = new BlueprintDisplay("placed_buildable_uncommited", Buildable, LightBlue, 1f);
		picking_remove = new BlueprintDisplay("picking_remove", Buildable, Yellow, 1.04f);
		picking_invalid = new BlueprintDisplay("picking_invalid", Buildable, Red, 1f);
		picking_anti = new BlueprintDisplay("picking_anti", Buildable, Orange, 1.04f);
		picking_valid = new BlueprintDisplay("picking_valid", Buildable, LightBlue, 1f);
		picking_buildable = new BlueprintDisplay("picking_buildable", Buildable, LightBlue, 1f);
	}

	private void Destroy()
	{
		placed_invalid.ClearCache();
		placed_anti.ClearCache();
		placed_valid.ClearCache();
		placed_buildable.ClearCache();
		placed_invalid_uncommited.ClearCache();
		placed_anti_uncommited.ClearCache();
		placed_valid_uncommited.ClearCache();
		placed_buildable_uncommited.ClearCache();
		picking_remove.ClearCache();
		picking_invalid.ClearCache();
		picking_anti.ClearCache();
		picking_valid.ClearCache();
		picking_buildable.ClearCache();
	}

	public static BlueprintDisplay ForMode(PlacementFlags flag)
	{
		if (flag.HasFlag(PlacementFlags.mayRemove))
		{
			return instance.picking_remove;
		}
		if (flag.HasFlag(PlacementFlags.placed))
		{
			if (flag.HasFlag(PlacementFlags.commited))
			{
				if (flag.HasFlag(PlacementFlags.anti))
				{
					return instance.placed_anti;
				}
				if (!flag.HasFlag(PlacementFlags.valid))
				{
					return instance.placed_invalid;
				}
				if (flag.HasFlag(PlacementFlags.buildable))
				{
					return instance.placed_buildable;
				}
				if (flag.HasFlag(PlacementFlags.valid))
				{
					return instance.placed_valid;
				}
			}
			else
			{
				if (flag.HasFlag(PlacementFlags.anti))
				{
					return instance.placed_anti_uncommited;
				}
				if (!flag.HasFlag(PlacementFlags.valid))
				{
					return instance.placed_invalid_uncommited;
				}
				if (flag.HasFlag(PlacementFlags.buildable))
				{
					return instance.placed_buildable_uncommited;
				}
				if (flag.HasFlag(PlacementFlags.valid))
				{
					return instance.placed_valid_uncommited;
				}
			}
		}
		else
		{
			if (flag.HasFlag(PlacementFlags.anti))
			{
				return instance.picking_anti;
			}
			if (!flag.HasFlag(PlacementFlags.valid))
			{
				return instance.picking_invalid;
			}
			if (flag.HasFlag(PlacementFlags.buildable))
			{
				return instance.picking_buildable;
			}
			if (flag.HasFlag(PlacementFlags.valid))
			{
				return instance.picking_valid;
			}
		}
		throw new ArgumentOutOfRangeException();
	}
}
