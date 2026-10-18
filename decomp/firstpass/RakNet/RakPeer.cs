using System;
using System.Runtime.InteropServices;

namespace RakNet;

public class RakPeer : RakPeerInterface
{
	private HandleRef swigCPtr;

	internal RakPeer(IntPtr cPtr, bool cMemoryOwn)
		: base(RakNetPINVOKE.RakPeer_SWIGUpcast(cPtr), cMemoryOwn)
	{
		swigCPtr = new HandleRef(this, cPtr);
	}

	public RakPeer()
		: this(RakNetPINVOKE.new_RakPeer(), cMemoryOwn: true)
	{
	}

	internal static HandleRef getCPtr(RakPeer obj)
	{
		return obj?.swigCPtr ?? new HandleRef(null, IntPtr.Zero);
	}

	~RakPeer()
	{
		Dispose();
	}

	public override void Dispose()
	{
		lock (this)
		{
			if (swigCPtr.Handle != IntPtr.Zero)
			{
				if (swigCMemOwn)
				{
					swigCMemOwn = false;
					RakNetPINVOKE.delete_RakPeer(swigCPtr);
				}
				swigCPtr = new HandleRef(null, IntPtr.Zero);
			}
			GC.SuppressFinalize(this);
			base.Dispose();
		}
	}

	public override void GetIncomingPassword(ref string passwordData, ref int passwordDataLength)
	{
		passwordData = CSharpGetIncomingPasswordHelper(passwordData, ref passwordDataLength);
	}

	public override void GetOfflinePingResponse(byte[] inOutByteArray, out uint length)
	{
		CSharpGetOfflinePingResponseHelper(inOutByteArray, out length);
	}

	public override bool GetConnectionList(out SystemAddress[] remoteSystems, ref ushort numberOfSystems)
	{
		RakNetListSystemAddress rakNetListSystemAddress = new RakNetListSystemAddress();
		bool connectionList = GetConnectionList(rakNetListSystemAddress, ref numberOfSystems);
		SystemAddress[] array = new SystemAddress[numberOfSystems];
		for (int i = 0; i < numberOfSystems; i++)
		{
			array[i] = rakNetListSystemAddress[i];
		}
		remoteSystems = array;
		return connectionList;
	}

	public override StartupResult Startup(uint maxConnections, SocketDescriptor socketDescriptors, uint socketDescriptorCount, int threadPriority)
	{
		return (StartupResult)RakNetPINVOKE.RakPeer_Startup__SWIG_0(swigCPtr, maxConnections, SocketDescriptor.getCPtr(socketDescriptors), socketDescriptorCount, threadPriority);
	}

	public override StartupResult Startup(uint maxConnections, SocketDescriptor socketDescriptors, uint socketDescriptorCount)
	{
		return (StartupResult)RakNetPINVOKE.RakPeer_Startup__SWIG_1(swigCPtr, maxConnections, SocketDescriptor.getCPtr(socketDescriptors), socketDescriptorCount);
	}

	public override bool InitializeSecurity(string publicKey, string privateKey, bool bRequireClientKey)
	{
		return RakNetPINVOKE.RakPeer_InitializeSecurity__SWIG_0(swigCPtr, publicKey, privateKey, bRequireClientKey);
	}

	public override bool InitializeSecurity(string publicKey, string privateKey)
	{
		return RakNetPINVOKE.RakPeer_InitializeSecurity__SWIG_1(swigCPtr, publicKey, privateKey);
	}

	public override void DisableSecurity()
	{
		RakNetPINVOKE.RakPeer_DisableSecurity(swigCPtr);
	}

	public override void AddToSecurityExceptionList(string ip)
	{
		RakNetPINVOKE.RakPeer_AddToSecurityExceptionList(swigCPtr, ip);
	}

	public override void RemoveFromSecurityExceptionList(string ip)
	{
		RakNetPINVOKE.RakPeer_RemoveFromSecurityExceptionList(swigCPtr, ip);
	}

	public override bool IsInSecurityExceptionList(string ip)
	{
		return RakNetPINVOKE.RakPeer_IsInSecurityExceptionList(swigCPtr, ip);
	}

	public override void SetMaximumIncomingConnections(ushort numberAllowed)
	{
		RakNetPINVOKE.RakPeer_SetMaximumIncomingConnections(swigCPtr, numberAllowed);
	}

	public override uint GetMaximumIncomingConnections()
	{
		return RakNetPINVOKE.RakPeer_GetMaximumIncomingConnections(swigCPtr);
	}

	public override ushort NumberOfConnections()
	{
		return RakNetPINVOKE.RakPeer_NumberOfConnections(swigCPtr);
	}

	public override void SetIncomingPassword(string passwordData, int passwordDataLength)
	{
		RakNetPINVOKE.RakPeer_SetIncomingPassword__SWIG_0(swigCPtr, passwordData, passwordDataLength);
	}

	public override ConnectionAttemptResult Connect(string host, ushort remotePort, string passwordData, int passwordDataLength, PublicKey publicKey, uint connectionSocketIndex, uint sendConnectionAttemptCount, uint timeBetweenSendConnectionAttemptsMS, uint timeoutTime)
	{
		return (ConnectionAttemptResult)RakNetPINVOKE.RakPeer_Connect__SWIG_0(swigCPtr, host, remotePort, passwordData, passwordDataLength, PublicKey.getCPtr(publicKey), connectionSocketIndex, sendConnectionAttemptCount, timeBetweenSendConnectionAttemptsMS, timeoutTime);
	}

	public override ConnectionAttemptResult Connect(string host, ushort remotePort, string passwordData, int passwordDataLength, PublicKey publicKey, uint connectionSocketIndex, uint sendConnectionAttemptCount, uint timeBetweenSendConnectionAttemptsMS)
	{
		return (ConnectionAttemptResult)RakNetPINVOKE.RakPeer_Connect__SWIG_1(swigCPtr, host, remotePort, passwordData, passwordDataLength, PublicKey.getCPtr(publicKey), connectionSocketIndex, sendConnectionAttemptCount, timeBetweenSendConnectionAttemptsMS);
	}

	public override ConnectionAttemptResult Connect(string host, ushort remotePort, string passwordData, int passwordDataLength, PublicKey publicKey, uint connectionSocketIndex, uint sendConnectionAttemptCount)
	{
		return (ConnectionAttemptResult)RakNetPINVOKE.RakPeer_Connect__SWIG_2(swigCPtr, host, remotePort, passwordData, passwordDataLength, PublicKey.getCPtr(publicKey), connectionSocketIndex, sendConnectionAttemptCount);
	}

	public override ConnectionAttemptResult Connect(string host, ushort remotePort, string passwordData, int passwordDataLength, PublicKey publicKey, uint connectionSocketIndex)
	{
		return (ConnectionAttemptResult)RakNetPINVOKE.RakPeer_Connect__SWIG_3(swigCPtr, host, remotePort, passwordData, passwordDataLength, PublicKey.getCPtr(publicKey), connectionSocketIndex);
	}

	public override ConnectionAttemptResult Connect(string host, ushort remotePort, string passwordData, int passwordDataLength, PublicKey publicKey)
	{
		return (ConnectionAttemptResult)RakNetPINVOKE.RakPeer_Connect__SWIG_4(swigCPtr, host, remotePort, passwordData, passwordDataLength, PublicKey.getCPtr(publicKey));
	}

	public override ConnectionAttemptResult Connect(string host, ushort remotePort, string passwordData, int passwordDataLength)
	{
		return (ConnectionAttemptResult)RakNetPINVOKE.RakPeer_Connect__SWIG_5(swigCPtr, host, remotePort, passwordData, passwordDataLength);
	}

	public override void Shutdown(uint blockDuration, byte orderingChannel, PacketPriority disconnectionNotificationPriority)
	{
		RakNetPINVOKE.RakPeer_Shutdown__SWIG_0(swigCPtr, blockDuration, orderingChannel, (int)disconnectionNotificationPriority);
	}

	public override void Shutdown(uint blockDuration, byte orderingChannel)
	{
		RakNetPINVOKE.RakPeer_Shutdown__SWIG_1(swigCPtr, blockDuration, orderingChannel);
	}

	public override void Shutdown(uint blockDuration)
	{
		RakNetPINVOKE.RakPeer_Shutdown__SWIG_2(swigCPtr, blockDuration);
	}

	public override bool IsActive()
	{
		return RakNetPINVOKE.RakPeer_IsActive(swigCPtr);
	}

	public override uint GetNextSendReceipt()
	{
		return RakNetPINVOKE.RakPeer_GetNextSendReceipt(swigCPtr);
	}

	public override uint IncrementNextSendReceipt()
	{
		return RakNetPINVOKE.RakPeer_IncrementNextSendReceipt(swigCPtr);
	}

	public override uint Send(string data, int length, PacketPriority priority, PacketReliability reliability, char orderingChannel, AddressOrGUID systemIdentifier, bool broadcast, uint forceReceiptNumber)
	{
		uint result = RakNetPINVOKE.RakPeer_Send__SWIG_0(swigCPtr, data, length, (int)priority, (int)reliability, orderingChannel, AddressOrGUID.getCPtr(systemIdentifier), broadcast, forceReceiptNumber);
		if (RakNetPINVOKE.SWIGPendingException.Pending)
		{
			throw RakNetPINVOKE.SWIGPendingException.Retrieve();
		}
		return result;
	}

	public override uint Send(string data, int length, PacketPriority priority, PacketReliability reliability, char orderingChannel, AddressOrGUID systemIdentifier, bool broadcast)
	{
		uint result = RakNetPINVOKE.RakPeer_Send__SWIG_1(swigCPtr, data, length, (int)priority, (int)reliability, orderingChannel, AddressOrGUID.getCPtr(systemIdentifier), broadcast);
		if (RakNetPINVOKE.SWIGPendingException.Pending)
		{
			throw RakNetPINVOKE.SWIGPendingException.Retrieve();
		}
		return result;
	}

	public override void SendLoopback(string data, int length)
	{
		RakNetPINVOKE.RakPeer_SendLoopback__SWIG_0(swigCPtr, data, length);
	}

	public override uint Send(BitStream bitStream, PacketPriority priority, PacketReliability reliability, char orderingChannel, AddressOrGUID systemIdentifier, bool broadcast, uint forceReceiptNumber)
	{
		uint result = RakNetPINVOKE.RakPeer_Send__SWIG_2(swigCPtr, BitStream.getCPtr(bitStream), (int)priority, (int)reliability, orderingChannel, AddressOrGUID.getCPtr(systemIdentifier), broadcast, forceReceiptNumber);
		if (RakNetPINVOKE.SWIGPendingException.Pending)
		{
			throw RakNetPINVOKE.SWIGPendingException.Retrieve();
		}
		return result;
	}

	public override uint Send(BitStream bitStream, PacketPriority priority, PacketReliability reliability, char orderingChannel, AddressOrGUID systemIdentifier, bool broadcast)
	{
		uint result = RakNetPINVOKE.RakPeer_Send__SWIG_3(swigCPtr, BitStream.getCPtr(bitStream), (int)priority, (int)reliability, orderingChannel, AddressOrGUID.getCPtr(systemIdentifier), broadcast);
		if (RakNetPINVOKE.SWIGPendingException.Pending)
		{
			throw RakNetPINVOKE.SWIGPendingException.Retrieve();
		}
		return result;
	}

	public override Packet Receive()
	{
		IntPtr intPtr = RakNetPINVOKE.RakPeer_Receive(swigCPtr);
		return (!(intPtr == IntPtr.Zero)) ? new Packet(intPtr, cMemoryOwn: false) : null;
	}

	public override void DeallocatePacket(Packet packet)
	{
		RakNetPINVOKE.RakPeer_DeallocatePacket(swigCPtr, Packet.getCPtr(packet));
	}

	public override uint GetMaximumNumberOfPeers()
	{
		return RakNetPINVOKE.RakPeer_GetMaximumNumberOfPeers(swigCPtr);
	}

	public override void CloseConnection(AddressOrGUID target, bool sendDisconnectionNotification, byte orderingChannel, PacketPriority disconnectionNotificationPriority)
	{
		RakNetPINVOKE.RakPeer_CloseConnection__SWIG_0(swigCPtr, AddressOrGUID.getCPtr(target), sendDisconnectionNotification, orderingChannel, (int)disconnectionNotificationPriority);
		if (RakNetPINVOKE.SWIGPendingException.Pending)
		{
			throw RakNetPINVOKE.SWIGPendingException.Retrieve();
		}
	}

	public override void CloseConnection(AddressOrGUID target, bool sendDisconnectionNotification, byte orderingChannel)
	{
		RakNetPINVOKE.RakPeer_CloseConnection__SWIG_1(swigCPtr, AddressOrGUID.getCPtr(target), sendDisconnectionNotification, orderingChannel);
		if (RakNetPINVOKE.SWIGPendingException.Pending)
		{
			throw RakNetPINVOKE.SWIGPendingException.Retrieve();
		}
	}

	public override void CloseConnection(AddressOrGUID target, bool sendDisconnectionNotification)
	{
		RakNetPINVOKE.RakPeer_CloseConnection__SWIG_2(swigCPtr, AddressOrGUID.getCPtr(target), sendDisconnectionNotification);
		if (RakNetPINVOKE.SWIGPendingException.Pending)
		{
			throw RakNetPINVOKE.SWIGPendingException.Retrieve();
		}
	}

	public override void CancelConnectionAttempt(SystemAddress target)
	{
		RakNetPINVOKE.RakPeer_CancelConnectionAttempt(swigCPtr, SystemAddress.getCPtr(target));
		if (RakNetPINVOKE.SWIGPendingException.Pending)
		{
			throw RakNetPINVOKE.SWIGPendingException.Retrieve();
		}
	}

	public override ConnectionState GetConnectionState(AddressOrGUID systemIdentifier)
	{
		ConnectionState result = (ConnectionState)RakNetPINVOKE.RakPeer_GetConnectionState(swigCPtr, AddressOrGUID.getCPtr(systemIdentifier));
		if (RakNetPINVOKE.SWIGPendingException.Pending)
		{
			throw RakNetPINVOKE.SWIGPendingException.Retrieve();
		}
		return result;
	}

	public override int GetIndexFromSystemAddress(SystemAddress systemAddress)
	{
		int result = RakNetPINVOKE.RakPeer_GetIndexFromSystemAddress(swigCPtr, SystemAddress.getCPtr(systemAddress));
		if (RakNetPINVOKE.SWIGPendingException.Pending)
		{
			throw RakNetPINVOKE.SWIGPendingException.Retrieve();
		}
		return result;
	}

	public override SystemAddress GetSystemAddressFromIndex(uint index)
	{
		return new SystemAddress(RakNetPINVOKE.RakPeer_GetSystemAddressFromIndex(swigCPtr, index), cMemoryOwn: true);
	}

	public override RakNetGUID GetGUIDFromIndex(uint index)
	{
		return new RakNetGUID(RakNetPINVOKE.RakPeer_GetGUIDFromIndex(swigCPtr, index), cMemoryOwn: true);
	}

	public override void GetSystemList(RakNetListSystemAddress addresses, RakNetListRakNetGUID guids)
	{
		RakNetPINVOKE.RakPeer_GetSystemList(swigCPtr, RakNetListSystemAddress.getCPtr(addresses), RakNetListRakNetGUID.getCPtr(guids));
		if (RakNetPINVOKE.SWIGPendingException.Pending)
		{
			throw RakNetPINVOKE.SWIGPendingException.Retrieve();
		}
	}

	public override void AddToBanList(string IP, uint milliseconds)
	{
		RakNetPINVOKE.RakPeer_AddToBanList__SWIG_0(swigCPtr, IP, milliseconds);
	}

	public override void AddToBanList(string IP)
	{
		RakNetPINVOKE.RakPeer_AddToBanList__SWIG_1(swigCPtr, IP);
	}

	public override void RemoveFromBanList(string IP)
	{
		RakNetPINVOKE.RakPeer_RemoveFromBanList(swigCPtr, IP);
	}

	public override void ClearBanList()
	{
		RakNetPINVOKE.RakPeer_ClearBanList(swigCPtr);
	}

	public override bool IsBanned(string IP)
	{
		return RakNetPINVOKE.RakPeer_IsBanned(swigCPtr, IP);
	}

	public override void SetLimitIPConnectionFrequency(bool b)
	{
		RakNetPINVOKE.RakPeer_SetLimitIPConnectionFrequency(swigCPtr, b);
	}

	public override void Ping(SystemAddress target)
	{
		RakNetPINVOKE.RakPeer_Ping__SWIG_0(swigCPtr, SystemAddress.getCPtr(target));
		if (RakNetPINVOKE.SWIGPendingException.Pending)
		{
			throw RakNetPINVOKE.SWIGPendingException.Retrieve();
		}
	}

	public override bool Ping(string host, ushort remotePort, bool onlyReplyOnAcceptingConnections, uint connectionSocketIndex)
	{
		return RakNetPINVOKE.RakPeer_Ping__SWIG_1(swigCPtr, host, remotePort, onlyReplyOnAcceptingConnections, connectionSocketIndex);
	}

	public override bool Ping(string host, ushort remotePort, bool onlyReplyOnAcceptingConnections)
	{
		return RakNetPINVOKE.RakPeer_Ping__SWIG_2(swigCPtr, host, remotePort, onlyReplyOnAcceptingConnections);
	}

	public override int GetAveragePing(AddressOrGUID systemIdentifier)
	{
		int result = RakNetPINVOKE.RakPeer_GetAveragePing(swigCPtr, AddressOrGUID.getCPtr(systemIdentifier));
		if (RakNetPINVOKE.SWIGPendingException.Pending)
		{
			throw RakNetPINVOKE.SWIGPendingException.Retrieve();
		}
		return result;
	}

	public override int GetLastPing(AddressOrGUID systemIdentifier)
	{
		int result = RakNetPINVOKE.RakPeer_GetLastPing(swigCPtr, AddressOrGUID.getCPtr(systemIdentifier));
		if (RakNetPINVOKE.SWIGPendingException.Pending)
		{
			throw RakNetPINVOKE.SWIGPendingException.Retrieve();
		}
		return result;
	}

	public override int GetLowestPing(AddressOrGUID systemIdentifier)
	{
		int result = RakNetPINVOKE.RakPeer_GetLowestPing(swigCPtr, AddressOrGUID.getCPtr(systemIdentifier));
		if (RakNetPINVOKE.SWIGPendingException.Pending)
		{
			throw RakNetPINVOKE.SWIGPendingException.Retrieve();
		}
		return result;
	}

	public override void SetOccasionalPing(bool doPing)
	{
		RakNetPINVOKE.RakPeer_SetOccasionalPing(swigCPtr, doPing);
	}

	public override ulong GetClockDifferential(AddressOrGUID systemIdentifier)
	{
		ulong result = RakNetPINVOKE.RakPeer_GetClockDifferential(swigCPtr, AddressOrGUID.getCPtr(systemIdentifier));
		if (RakNetPINVOKE.SWIGPendingException.Pending)
		{
			throw RakNetPINVOKE.SWIGPendingException.Retrieve();
		}
		return result;
	}

	public override void SetOfflinePingResponse(string data, uint length)
	{
		RakNetPINVOKE.RakPeer_SetOfflinePingResponse__SWIG_0(swigCPtr, data, length);
	}

	public override SystemAddress GetInternalID(SystemAddress systemAddress, int index)
	{
		SystemAddress result = new SystemAddress(RakNetPINVOKE.RakPeer_GetInternalID__SWIG_0(swigCPtr, SystemAddress.getCPtr(systemAddress), index), cMemoryOwn: true);
		if (RakNetPINVOKE.SWIGPendingException.Pending)
		{
			throw RakNetPINVOKE.SWIGPendingException.Retrieve();
		}
		return result;
	}

	public override SystemAddress GetInternalID(SystemAddress systemAddress)
	{
		SystemAddress result = new SystemAddress(RakNetPINVOKE.RakPeer_GetInternalID__SWIG_1(swigCPtr, SystemAddress.getCPtr(systemAddress)), cMemoryOwn: true);
		if (RakNetPINVOKE.SWIGPendingException.Pending)
		{
			throw RakNetPINVOKE.SWIGPendingException.Retrieve();
		}
		return result;
	}

	public override SystemAddress GetInternalID()
	{
		return new SystemAddress(RakNetPINVOKE.RakPeer_GetInternalID__SWIG_2(swigCPtr), cMemoryOwn: true);
	}

	public override void SetInternalID(SystemAddress systemAddress, int index)
	{
		RakNetPINVOKE.RakPeer_SetInternalID__SWIG_0(swigCPtr, SystemAddress.getCPtr(systemAddress), index);
		if (RakNetPINVOKE.SWIGPendingException.Pending)
		{
			throw RakNetPINVOKE.SWIGPendingException.Retrieve();
		}
	}

	public override void SetInternalID(SystemAddress systemAddress)
	{
		RakNetPINVOKE.RakPeer_SetInternalID__SWIG_1(swigCPtr, SystemAddress.getCPtr(systemAddress));
		if (RakNetPINVOKE.SWIGPendingException.Pending)
		{
			throw RakNetPINVOKE.SWIGPendingException.Retrieve();
		}
	}

	public override SystemAddress GetExternalID(SystemAddress target)
	{
		SystemAddress result = new SystemAddress(RakNetPINVOKE.RakPeer_GetExternalID(swigCPtr, SystemAddress.getCPtr(target)), cMemoryOwn: true);
		if (RakNetPINVOKE.SWIGPendingException.Pending)
		{
			throw RakNetPINVOKE.SWIGPendingException.Retrieve();
		}
		return result;
	}

	public override RakNetGUID GetMyGUID()
	{
		return new RakNetGUID(RakNetPINVOKE.RakPeer_GetMyGUID(swigCPtr), cMemoryOwn: true);
	}

	public override SystemAddress GetMyBoundAddress(int socketIndex)
	{
		return new SystemAddress(RakNetPINVOKE.RakPeer_GetMyBoundAddress__SWIG_0(swigCPtr, socketIndex), cMemoryOwn: true);
	}

	public override SystemAddress GetMyBoundAddress()
	{
		return new SystemAddress(RakNetPINVOKE.RakPeer_GetMyBoundAddress__SWIG_1(swigCPtr), cMemoryOwn: true);
	}

	public override RakNetGUID GetGuidFromSystemAddress(SystemAddress input)
	{
		RakNetGUID result = new RakNetGUID(RakNetPINVOKE.RakPeer_GetGuidFromSystemAddress(swigCPtr, SystemAddress.getCPtr(input)), cMemoryOwn: false);
		if (RakNetPINVOKE.SWIGPendingException.Pending)
		{
			throw RakNetPINVOKE.SWIGPendingException.Retrieve();
		}
		return result;
	}

	public override SystemAddress GetSystemAddressFromGuid(RakNetGUID input)
	{
		SystemAddress result = new SystemAddress(RakNetPINVOKE.RakPeer_GetSystemAddressFromGuid(swigCPtr, RakNetGUID.getCPtr(input)), cMemoryOwn: true);
		if (RakNetPINVOKE.SWIGPendingException.Pending)
		{
			throw RakNetPINVOKE.SWIGPendingException.Retrieve();
		}
		return result;
	}

	public override bool GetClientPublicKeyFromSystemAddress(SystemAddress input, string client_public_key)
	{
		bool result = RakNetPINVOKE.RakPeer_GetClientPublicKeyFromSystemAddress(swigCPtr, SystemAddress.getCPtr(input), client_public_key);
		if (RakNetPINVOKE.SWIGPendingException.Pending)
		{
			throw RakNetPINVOKE.SWIGPendingException.Retrieve();
		}
		return result;
	}

	public override void SetTimeoutTime(uint timeMS, SystemAddress target)
	{
		RakNetPINVOKE.RakPeer_SetTimeoutTime(swigCPtr, timeMS, SystemAddress.getCPtr(target));
		if (RakNetPINVOKE.SWIGPendingException.Pending)
		{
			throw RakNetPINVOKE.SWIGPendingException.Retrieve();
		}
	}

	public override uint GetTimeoutTime(SystemAddress target)
	{
		uint result = RakNetPINVOKE.RakPeer_GetTimeoutTime(swigCPtr, SystemAddress.getCPtr(target));
		if (RakNetPINVOKE.SWIGPendingException.Pending)
		{
			throw RakNetPINVOKE.SWIGPendingException.Retrieve();
		}
		return result;
	}

	public override int GetMTUSize(SystemAddress target)
	{
		int result = RakNetPINVOKE.RakPeer_GetMTUSize(swigCPtr, SystemAddress.getCPtr(target));
		if (RakNetPINVOKE.SWIGPendingException.Pending)
		{
			throw RakNetPINVOKE.SWIGPendingException.Retrieve();
		}
		return result;
	}

	public override uint GetNumberOfAddresses()
	{
		return RakNetPINVOKE.RakPeer_GetNumberOfAddresses(swigCPtr);
	}

	public override string GetLocalIP(uint index)
	{
		return RakNetPINVOKE.RakPeer_GetLocalIP(swigCPtr, index);
	}

	public override bool IsLocalIP(string ip)
	{
		return RakNetPINVOKE.RakPeer_IsLocalIP(swigCPtr, ip);
	}

	public override void AllowConnectionResponseIPMigration(bool allow)
	{
		RakNetPINVOKE.RakPeer_AllowConnectionResponseIPMigration(swigCPtr, allow);
	}

	public override bool AdvertiseSystem(string host, ushort remotePort, string data, int dataLength, uint connectionSocketIndex)
	{
		return RakNetPINVOKE.RakPeer_AdvertiseSystem__SWIG_0(swigCPtr, host, remotePort, data, dataLength, connectionSocketIndex);
	}

	public override bool AdvertiseSystem(string host, ushort remotePort, string data, int dataLength)
	{
		return RakNetPINVOKE.RakPeer_AdvertiseSystem__SWIG_1(swigCPtr, host, remotePort, data, dataLength);
	}

	public override void SetSplitMessageProgressInterval(int interval)
	{
		RakNetPINVOKE.RakPeer_SetSplitMessageProgressInterval(swigCPtr, interval);
	}

	public override int GetSplitMessageProgressInterval()
	{
		return RakNetPINVOKE.RakPeer_GetSplitMessageProgressInterval(swigCPtr);
	}

	public override void SetUnreliableTimeout(uint timeoutMS)
	{
		RakNetPINVOKE.RakPeer_SetUnreliableTimeout(swigCPtr, timeoutMS);
	}

	public override void SendTTL(string host, ushort remotePort, int ttl, uint connectionSocketIndex)
	{
		RakNetPINVOKE.RakPeer_SendTTL__SWIG_0(swigCPtr, host, remotePort, ttl, connectionSocketIndex);
	}

	public override void SendTTL(string host, ushort remotePort, int ttl)
	{
		RakNetPINVOKE.RakPeer_SendTTL__SWIG_1(swigCPtr, host, remotePort, ttl);
	}

	public override void AttachPlugin(PluginInterface2 plugin)
	{
		RakNetPINVOKE.RakPeer_AttachPlugin(swigCPtr, PluginInterface2.getCPtr(plugin));
	}

	public override void DetachPlugin(PluginInterface2 messageHandler)
	{
		RakNetPINVOKE.RakPeer_DetachPlugin(swigCPtr, PluginInterface2.getCPtr(messageHandler));
	}

	public override void PushBackPacket(Packet packet, bool pushAtHead)
	{
		RakNetPINVOKE.RakPeer_PushBackPacket(swigCPtr, Packet.getCPtr(packet), pushAtHead);
	}

	public override void ChangeSystemAddress(RakNetGUID guid, SystemAddress systemAddress)
	{
		RakNetPINVOKE.RakPeer_ChangeSystemAddress(swigCPtr, RakNetGUID.getCPtr(guid), SystemAddress.getCPtr(systemAddress));
		if (RakNetPINVOKE.SWIGPendingException.Pending)
		{
			throw RakNetPINVOKE.SWIGPendingException.Retrieve();
		}
	}

	public override Packet AllocatePacket(uint dataSize)
	{
		IntPtr intPtr = RakNetPINVOKE.RakPeer_AllocatePacket(swigCPtr, dataSize);
		return (!(intPtr == IntPtr.Zero)) ? new Packet(intPtr, cMemoryOwn: false) : null;
	}

	public override void GetSockets(SWIGTYPE_p_DataStructures__ListT_RakNetSocket2_p_t sockets)
	{
		RakNetPINVOKE.RakPeer_GetSockets(swigCPtr, SWIGTYPE_p_DataStructures__ListT_RakNetSocket2_p_t.getCPtr(sockets));
		if (RakNetPINVOKE.SWIGPendingException.Pending)
		{
			throw RakNetPINVOKE.SWIGPendingException.Retrieve();
		}
	}

	public override void WriteOutOfBandHeader(BitStream bitStream)
	{
		RakNetPINVOKE.RakPeer_WriteOutOfBandHeader(swigCPtr, BitStream.getCPtr(bitStream));
	}

	public override void ApplyNetworkSimulator(float packetloss, ushort minExtraPing, ushort extraPingVariance)
	{
		RakNetPINVOKE.RakPeer_ApplyNetworkSimulator(swigCPtr, packetloss, minExtraPing, extraPingVariance);
	}

	public override void SetPerConnectionOutgoingBandwidthLimit(uint maxBitsPerSecond)
	{
		RakNetPINVOKE.RakPeer_SetPerConnectionOutgoingBandwidthLimit(swigCPtr, maxBitsPerSecond);
	}

	public override bool IsNetworkSimulatorActive()
	{
		return RakNetPINVOKE.RakPeer_IsNetworkSimulatorActive(swigCPtr);
	}

	public override RakNetStatistics GetStatistics(SystemAddress systemAddress, RakNetStatistics rns)
	{
		IntPtr intPtr = RakNetPINVOKE.RakPeer_GetStatistics__SWIG_0(swigCPtr, SystemAddress.getCPtr(systemAddress), RakNetStatistics.getCPtr(rns));
		RakNetStatistics result = ((!(intPtr == IntPtr.Zero)) ? new RakNetStatistics(intPtr, cMemoryOwn: false) : null);
		if (RakNetPINVOKE.SWIGPendingException.Pending)
		{
			throw RakNetPINVOKE.SWIGPendingException.Retrieve();
		}
		return result;
	}

	public override RakNetStatistics GetStatistics(SystemAddress systemAddress)
	{
		IntPtr intPtr = RakNetPINVOKE.RakPeer_GetStatistics__SWIG_1(swigCPtr, SystemAddress.getCPtr(systemAddress));
		RakNetStatistics result = ((!(intPtr == IntPtr.Zero)) ? new RakNetStatistics(intPtr, cMemoryOwn: false) : null);
		if (RakNetPINVOKE.SWIGPendingException.Pending)
		{
			throw RakNetPINVOKE.SWIGPendingException.Retrieve();
		}
		return result;
	}

	public override bool GetStatistics(uint index, RakNetStatistics rns)
	{
		return RakNetPINVOKE.RakPeer_GetStatistics__SWIG_2(swigCPtr, index, RakNetStatistics.getCPtr(rns));
	}

	public override uint GetReceiveBufferSize()
	{
		return RakNetPINVOKE.RakPeer_GetReceiveBufferSize(swigCPtr);
	}

	public override bool RunUpdateCycle(BitStream updateBitStream)
	{
		bool result = RakNetPINVOKE.RakPeer_RunUpdateCycle(swigCPtr, BitStream.getCPtr(updateBitStream));
		if (RakNetPINVOKE.SWIGPendingException.Pending)
		{
			throw RakNetPINVOKE.SWIGPendingException.Retrieve();
		}
		return result;
	}

	public override bool SendOutOfBand(string host, ushort remotePort, string data, uint dataLength, uint connectionSocketIndex)
	{
		return RakNetPINVOKE.RakPeer_SendOutOfBand__SWIG_0(swigCPtr, host, remotePort, data, dataLength, connectionSocketIndex);
	}

	public override bool SendOutOfBand(string host, ushort remotePort, string data, uint dataLength)
	{
		return RakNetPINVOKE.RakPeer_SendOutOfBand__SWIG_1(swigCPtr, host, remotePort, data, dataLength);
	}

	public new uint Send(byte[] inByteArray, int length, PacketPriority priority, PacketReliability reliability, char orderingChannel, AddressOrGUID systemIdentifier, bool broadcast)
	{
		uint result = RakNetPINVOKE.RakPeer_Send__SWIG_4(swigCPtr, inByteArray, length, (int)priority, (int)reliability, orderingChannel, AddressOrGUID.getCPtr(systemIdentifier), broadcast);
		if (RakNetPINVOKE.SWIGPendingException.Pending)
		{
			throw RakNetPINVOKE.SWIGPendingException.Retrieve();
		}
		return result;
	}

	public new void SendLoopback(byte[] inByteArray, int length)
	{
		RakNetPINVOKE.RakPeer_SendLoopback__SWIG_1(swigCPtr, inByteArray, length);
	}

	public new void SetOfflinePingResponse(byte[] inByteArray, uint length)
	{
		RakNetPINVOKE.RakPeer_SetOfflinePingResponse__SWIG_1(swigCPtr, inByteArray, length);
	}

	public new bool AdvertiseSystem(string host, ushort remotePort, byte[] inByteArray, int dataLength, uint connectionSocketIndex)
	{
		return RakNetPINVOKE.RakPeer_AdvertiseSystem__SWIG_2(swigCPtr, host, remotePort, inByteArray, dataLength, connectionSocketIndex);
	}

	public new bool AdvertiseSystem(string host, ushort remotePort, byte[] inByteArray, int dataLength)
	{
		return RakNetPINVOKE.RakPeer_AdvertiseSystem__SWIG_3(swigCPtr, host, remotePort, inByteArray, dataLength);
	}

	private string CSharpGetIncomingPasswordHelper(string passwordData, ref int passwordDataLength)
	{
		return RakNetPINVOKE.RakPeer_CSharpGetIncomingPasswordHelper(swigCPtr, passwordData, ref passwordDataLength);
	}

	public new void SetIncomingPassword(byte[] passwordDataByteArray, int passwordDataLength)
	{
		RakNetPINVOKE.RakPeer_SetIncomingPassword__SWIG_1(swigCPtr, passwordDataByteArray, passwordDataLength);
	}

	public new void GetIncomingPassword(byte[] passwordDataByteArray, ref int passwordDataLength)
	{
		RakNetPINVOKE.RakPeer_GetIncomingPassword(swigCPtr, passwordDataByteArray, ref passwordDataLength);
	}

	private void CSharpGetOfflinePingResponseHelper(byte[] inOutByteArray, out uint outLength)
	{
		RakNetPINVOKE.RakPeer_CSharpGetOfflinePingResponseHelper(swigCPtr, inOutByteArray, out outLength);
	}

	public new bool GetConnectionList(RakNetListSystemAddress remoteSystems, ref ushort numberOfSystems)
	{
		return RakNetPINVOKE.RakPeer_GetConnectionList(swigCPtr, RakNetListSystemAddress.getCPtr(remoteSystems), ref numberOfSystems);
	}
}
