using System.Collections;

namespace Brix.Network;

public interface IOperationReseau
{
	void OnOperationStart(string operationID, double operationTime);

	void OnOperationEnd(string operationID, double operationEndTime, IList results);
}
