using Brix.Utils.Logging;
using MoonSharp.Interpreter;

namespace Brix.UI.Builder.Lui;

public static class ScanLuiExtension
{
	public static BrixLogger logger = new BrixLogger(typeof(ScanLuiExtension).FullName);

	public static void InitScan(this IIsMenuLui _this)
	{
		_this.T.Set("Scan", DynValue.NewCallback((ScriptExecutionContext con, CallbackArguments args) => _this.Scan(con, args)));
	}

	public static DynValue Scan(this IIsMenuLui _this, ScriptExecutionContext context, CallbackArguments args)
	{
		if (LuiBuilder.IsDebug)
		{
			logger.Info("Scan\n" + _this.Id);
		}
		return DynValue.Void;
	}
}
