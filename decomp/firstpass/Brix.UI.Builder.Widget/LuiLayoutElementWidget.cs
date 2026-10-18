using Brix.Lua;
using Brix.UI.Builder.Fields;

namespace Brix.UI.Builder.Widget;

public class LuiLayoutElementWidget : LuiWidget
{
	[LuaDrivable("minWidth")]
	public LuiFloatField MinWidth;

	[LuaDrivable("minHeight")]
	public LuiFloatField MinHeight;

	private static LuaObjectDescriptor<LuiLayoutElementWidget> _desc = new LuaObjectDescriptor<LuiLayoutElementWidget>();

	public LuiLayoutElementWidget()
	{
		MinWidth = new LuiFloatField(this);
		MinHeight = new LuiFloatField(this);
	}
}
