using System;
using System.Collections.Generic;
using Brix.Utils.Unity;
using Smooth.Slinq;
using UnityEngine;

namespace Brix.UI.Icons;

public class SpriteCompiler : MonoBehaviour
{
	[Serializable]
	public class IconPair
	{
		public string key;

		public Sprite icon;

		private const string db = "SpriteCompiler";

		public string Db => "SpriteCompiler";

		public IconPair(string key, Sprite icon)
		{
			this.key = key.Replace(" ", "_").Insert(0, "_");
			this.icon = icon;
		}

		public MetaIcon ToMetaIcon()
		{
			MetaIcon metaIcon = new MetaIcon();
			metaIcon.Name = key;
			metaIcon.Image32 = icon;
			metaIcon.Image64 = icon;
			return metaIcon;
		}

		public string ToCode()
		{
			return string.Format("public static IconKey {0} = new IconKey(\"{1}\", \"{0}\");", key, "SpriteCompiler");
		}
	}

	private const string GeneratedPath = "Assets/Standard Assets/Brix/UI/Icons/IconKeys2.cs";

	[SerializeField]
	private List<Texture2D> textureFiles;

	[DisplayRules(DisplayFlags.ReadOnly)]
	[SerializeField]
	private List<IconPair> compiledIcons;

	private void Start()
	{
		RegisterIcons();
	}

	private void RegisterIcons()
	{
		compiledIcons.Slinq().ForEach(delegate(IconPair p)
		{
			IconDB.AddIcon(p.Db, p.key, p.ToMetaIcon());
		});
	}
}
