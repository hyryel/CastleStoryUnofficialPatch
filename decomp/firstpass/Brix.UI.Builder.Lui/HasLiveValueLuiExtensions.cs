using MoonSharp.Interpreter;

namespace Brix.UI.Builder.Lui;

public static class HasLiveValueLuiExtensions
{
	public static void InitHasLiveValue(this IHasLiveValueLui _this)
	{
		_this.T.Set("LiveValue", DynValue.Nil);
		_this.T.Set("LiveValueIsOnce", DynValue.False);
	}

	public static DynValue ClearLiveValue(this IHasLiveValueLui _this)
	{
		_this.T.Set("LiveValue", DynValue.Nil);
		_this.T.Set("LiveValueIsOnce", DynValue.False);
		return _this.D;
	}
}
