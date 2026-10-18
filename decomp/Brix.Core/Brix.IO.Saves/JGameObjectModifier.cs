using Brix.IO.Saves.Modifiers;
using Newtonsoft.Json.Linq;

namespace Brix.IO.Saves;

public class JGameObjectModifier : JObjectModifier
{
	public class JGameObjectModifierBuilder : BaseModifierBuilder<JGameObjectModifierBuilder, JGameObjectModifier>
	{
		public JGameObjectModifierBuilder AssetKey(string assetKey)
		{
			ChangeFieldValue("$assetKey", assetKey);
			return this;
		}

		public JGameObjectModifierBuilder Path(string path)
		{
			ChangeFieldValue("$path", path);
			return this;
		}

		public JGameObjectModifierBuilder ReplacePath(string tokenToFind, string tokenToReplace)
		{
			localModifiers.Add(new PathReplacer(tokenToFind, tokenToReplace));
			return this;
		}

		public JGameObjectModifierBuilder GOName(string name)
		{
			ChangeFieldValue("name", name);
			return this;
		}

		public JGameObjectModifierBuilder Active(bool isActive)
		{
			ChangeFieldValue("activeSelf", isActive);
			return this;
		}

		public JGameObjectModifierBuilder InjectRawComponent(JObject componentContainer)
		{
			InjectJObject("components", componentContainer);
			return this;
		}
	}

	public new static JGameObjectModifierBuilder New()
	{
		return new JGameObjectModifierBuilder();
	}

	public JContainer Apply(JGameObjectFetcher fetcher, JContainer jContainer)
	{
		JContainer jContainer2 = jContainer.DeepClone() as JContainer;
		foreach (IJObjectLocalModifier localModifier in localModifiers)
		{
			jContainer2 = localModifier.ApplyChange(fetcher, jContainer2);
		}
		return jContainer2;
	}
}
