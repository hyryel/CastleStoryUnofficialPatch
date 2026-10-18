using UnityEngine;

namespace Brix.Network;

public class ReseauDisconnectionInfo : ReseauBasicInfo
{
	public string disconnectReason;

	public NetworkDisconnection reason;
}
