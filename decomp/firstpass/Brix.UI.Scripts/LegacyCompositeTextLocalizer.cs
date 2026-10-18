using Brix.Utils;
using Smooth.Slinq;

namespace Brix.UI.Scripts;

public sealed class LegacyCompositeTextLocalizer : LegacyBaseTextLocalizer
{
	public string[] TextValues;

	protected override string Value => string.Join(string.Empty, TextValues.Slinq().Select(I2Helper.TryGet).ToArray());
}
