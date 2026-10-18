using System;
using Brix.IO.Saves.Modifiers;
using Newtonsoft.Json.Linq;

namespace Brix.IO.Saves;

public class JObjectModifier : AbstractModifier
{
	public class JObjectModifierBuilder : BaseModifierBuilder<JObjectModifierBuilder, JObjectModifier>
	{
		public JObjectModifierBuilder Type(string newComponentType)
		{
			localModifiers.Add(new TypeModifier(newComponentType));
			return this;
		}

		public JObjectModifierBuilder ReplaceByValue(Func<JObject, JContainer, string> conversionFunction)
		{
			localModifiers.Add(new JObjectConverter(conversionFunction));
			return this;
		}
	}

	public static JObjectModifierBuilder New()
	{
		return new JObjectModifierBuilder();
	}

	public JContainer Apply(AbstractFetcher fetcher, JContainer jContainer)
	{
		JContainer jContainer2 = jContainer;
		foreach (IJObjectLocalModifier localModifier in localModifiers)
		{
			jContainer2 = localModifier.ApplyChange(fetcher, jContainer2);
		}
		return jContainer2;
	}
}
