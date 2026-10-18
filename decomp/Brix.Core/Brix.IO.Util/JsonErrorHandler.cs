using Brix.Utils.Logging;
using Newtonsoft.Json.Serialization;

namespace Brix.IO.Util;

public class JsonErrorHandler
{
	private static BrixLogger logger = new BrixLogger();

	public static void LogErrorHandler(object sender, ErrorEventArgs e)
	{
		logger.Error(string.Concat("error: ", e.ErrorContext.Error, " object: ", e.ErrorContext.OriginalObject, " member: ", e.ErrorContext.Member, " path: ", e.ErrorContext.Path));
	}
}
