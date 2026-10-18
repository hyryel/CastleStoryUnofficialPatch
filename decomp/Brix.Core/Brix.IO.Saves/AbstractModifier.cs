using System.Collections.Generic;
using Brix.IO.Saves.Modifiers;

namespace Brix.IO.Saves;

public class AbstractModifier
{
	internal List<IJObjectLocalModifier> localModifiers;

	internal AbstractModifier()
	{
	}
}
