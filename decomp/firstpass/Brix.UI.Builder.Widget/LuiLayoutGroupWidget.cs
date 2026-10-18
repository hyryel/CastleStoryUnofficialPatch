using Brix.Lua;
using Brix.UI.Builder.Fields;

namespace Brix.UI.Builder.Widget;

public class LuiLayoutGroupWidget : LuiWidget
{
	[LuaDrivable("padding")]
	public LuiVector4Field Padding;

	[LuaDrivable("spacing")]
	public LuiFloatField Spacing;

	private static LuaObjectDescriptor<LuiLayoutGroupWidget> _desc = new LuaObjectDescriptor<LuiLayoutGroupWidget>();

	public LuiLayoutGroupWidget()
	{
		Padding = new LuiVector4Field(this);
		Spacing = new LuiFloatField(this);
	}
}
