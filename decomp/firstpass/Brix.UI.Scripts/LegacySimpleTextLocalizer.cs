using Brix.Utils;

namespace Brix.UI.Scripts;

public sealed class LegacySimpleTextLocalizer : LegacyBaseTextLocalizer
{
	public string TextValue;

	protected override string Value => I2Helper.TryGet(TextValue);
}
