using Brix.IO.Saves.Modifiers;
using Newtonsoft.Json.Linq;

namespace Brix.IO.Saves;

public class JComponentModifier : AbstractModifier
{
	public class JComponentModifierBuilder : BaseModifierBuilder<JComponentModifierBuilder, JComponentModifier>
	{
		public JComponentModifierBuilder Type(string newComponentType)
		{
			localModifiers.Add(new TypeModifier(newComponentType));
			return this;
		}
	}

	public static JComponentModifierBuilder New()
	{
		return new JComponentModifierBuilder();
	}

	public JContainer Apply(JComponentFetcher compFetcher, JContainer jContainer)
	{
		JContainer jContainer2 = jContainer.DeepClone() as JContainer;
		foreach (IJObjectLocalModifier localModifier in localModifiers)
		{
			jContainer2 = localModifier.ApplyChange(compFetcher, jContainer2);
		}
		return jContainer2;
	}
}
