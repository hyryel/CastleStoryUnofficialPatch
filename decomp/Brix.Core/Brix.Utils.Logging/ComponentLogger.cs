using System.Diagnostics;
using log4net;

namespace Brix.Utils.Logging;

public class ComponentLogger : BrixLogger
{
	private DualLogger dualLogger;

	private ILog instanceLogger;

	private static ILog staticLogger;

	public ComponentLogger()
		: this(new StackFrame(1, fNeedFileInfo: true).GetMethod().DeclaringType.FullName)
	{
	}

	public ComponentLogger(string name)
		: base("instance." + name)
	{
		instanceLogger = ilog;
		ilog = (staticLogger = LogManager.GetLogger(name));
		dualLogger = new DualLogger(instanceLogger, staticLogger);
	}

	public void DisableInstanceLogging()
	{
		ilog = staticLogger;
	}

	public void EnableInstanceLogging()
	{
		ilog = dualLogger;
	}
}
