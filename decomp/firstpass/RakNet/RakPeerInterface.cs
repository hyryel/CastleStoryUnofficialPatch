using System;
using System.Runtime.InteropServices;

namespace RakNet;

public class RakPeerInterface : IDisposable
{
	private HandleRef swigCPtr;

	protected bool swigCMemOwn;

	internal RakPeerInterface(IntPtr cPtr, bool cMemoryOwn)
	{
		swigCMemOwn = cMemoryOwn;
		swigCPtr = new HandleRef(this, cPtr);
	}

	internal static HandleRef getCPtr(RakPeerInterface obj)
	{
		return obj?.swigCPtr ?? new HandleRef(null, IntPtr.Zero);
	}

	~RakPeerInterface()
	{
		Dispose();
	}

	public virtual void Dispose()
	{
		lock (this)
		{
			if (swigCPtr.Handle != IntPtr.Zero)
			{
				if (swigCMemOwn)
				{
					swigCMemOwn = false;
					RakNetPINVOKE.delete_RakPeerInterface(swigCPtr);
				}
				swigCPtr = new HandleRef(null, IntPtr.Zero);
			}
			GC.SuppressFinalize(this);
		}
	}

	public virtual void GetIncomingPassword(ref string passwordData, ref int passwordDataLength)
	{
		passwordData = CSharpGetIncomingPasswordHelper(passwordData, ref passwordDataLength);
	}

	public virtual void GetOfflinePingResponse(byte[] inOutByteArray, out uint length)
	{
		CSharpGetOfflinePingResponseHelper(inOutByteArray, out length);
	}

	public virtual bool GetConnectionList(out SystemAddress[] remoteSystems, ref ushort numberOfSystems)
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

	public static RakPeerInterface GetInstance()
	{
		IntPtr intPtr = RakNetPINVOKE.RakPeerInterface_GetInstance();
		return (!(intPtr == IntPtr.Zero)) ? new RakPeerInterface(intPtr, cMemoryOwn: false) : null;
	}

	public static void DestroyInstance(RakPeerInterface i)
	{
		RakNetPINVOKE.RakPeerInterface_DestroyInstance(getCPtr(i));
	}

	public virtual StartupResult Startup(uint maxConnections, SocketDescriptor socketDescriptors, uint socketDescriptorCount, int threadPriority)
	{
		return (StartupResult)RakNetPINVOKE.RakPeerInterface_Startup__SWIG_0(swigCPtr, maxConnections, SocketDescriptor.getCPtr(socketDescriptors), socketDescriptorCount, threadPriority);
	}

	public virtual StartupResult Startup(uint maxConnections, SocketDescriptor socketDescriptors, uint socketDescriptorCount)
	{
		return (StartupResult)RakNetPINVOKE.RakPeerInterface_Startup__SWIG_1(swigCPtr, maxConnections, SocketDescriptor.getCPtr(socketDescriptors), socketDescriptorCount);
	}

	public virtual bool InitializeSecurity(string publicKey, string privateKey, bool bRequireClientKey)
	{
		return RakNetPINVOKE.RakPeerInterface_InitializeSecurity__SWIG_0(swigCPtr, publicKey, privateKey, bRequireClientKey);
	}

	public virtual bool InitializeSecurity(string publicKey, string privateKey)
	{
		return RakNetPINVOKE.RakPeerInterface_InitializeSecurity__SWIG_1(swigCPtr, publicKey, privateKey);
	}

	public virtual void DisableSecurity()
	{
		RakNetPINVOKE.RakPeerInterface_DisableSecurity(swigCPtr);
	}

	public virtual void AddToSecurityExceptionList(string ip)
	{
		RakNetPINVOKE.RakPeerInterface_AddToSecurityExceptionList(swigCPtr, ip);
	}

	public virtual void RemoveFromSecurityExceptionList(string ip)
	{
		RakNetPINVOKE.RakPeerInterface_RemoveFromSecurityExceptionList(swigCPtr, ip);
	}

	public virtual bool IsInSecurityExceptionList(string ip)
	{
		return RakNetPINVOKE.RakPeerInterface_IsInSecurityExceptionList(swigCPtr, ip);
	}

	public virtual void SetMaximumIncomingConnections(ushort numberAllowed)
	{
		RakNetPINVOKE.RakPeerInterface_SetMaximumIncomingConnections(swigCPtr, numberAllowed);
	}

	public virtual uint GetMaximumIncomingConnections()
	{
		return RakNetPINVOKE.RakPeerInterface_GetMaximumIncomingConnections(swigCPtr);
	}

	public virtual ushort NumberOfConnections()
	{
		return RakNetPINVOKE.RakPeerInterface_NumberOfConnections(swigCPtr);
	}

	public virtual void SetIncomingPassword(string passwordData, int passwordDataLength)
	{
		RakNetPINVOKE.RakPeerInterface_SetIncomingPassword__SWIG_0(swigCPtr, passwordData, passwordDataLength);
	}

	public virtual ConnectionAttemptResult Connect(string host, ushort remotePort, string passwordData, int passwordDataLength, PublicKey publicKey, uint connectionSocketIndex, uint sendConnectionAttemptCount, uint timeBetweenSendConnectionAttemptsMS, uint timeoutTime)
	{
		return (ConnectionAttemptResult)RakNetPINVOKE.RakPeerInterface_Connect__SWIG_0(swigCPtr, host, remotePort, passwordData, passwordDataLength, PublicKey.getCPtr(publicKey), connectionSocketIndex, sendConnectionAttemptCount, timeBetweenSendConnectionAttemptsMS, timeoutTime);
	}

	public virtual ConnectionAttemptResult Connect(string host, ushort remotePort, string passwordData, int passwordDataLength, PublicKey publicKey, uint connectionSocketIndex, uint sendConnectionAttemptCount, uint timeBetweenSendConnectionAttemptsMS)
	{
		return (ConnectionAttemptResult)RakNetPINVOKE.RakPeerInterface_Connect__SWIG_1(swigCPtr, host, remotePort, passwordData, passwordDataLength, PublicKey.getCPtr(publicKey), connectionSocketIndex, sendConnectionAttemptCount, timeBetweenSendConnectionAttemptsMS);
	}

	public virtual ConnectionAttemptResult Connect(string host, ushort remotePort, string passwordData, int passwordDataLength, PublicKey publicKey, uint connectionSocketIndex, uint sendConnectionAttemptCount)
	{
		return (ConnectionAttemptResult)RakNetPINVOKE.RakPeerInterface_Connect__SWIG_2(swigCPtr, host, remotePort, passwordData, passwordDataLength, PublicKey.getCPtr(publicKey), connectionSocketIndex, sendConnectionAttemptCount);
	}

	public virtual ConnectionAttemptResult Connect(string host, ushort remotePort, string passwordData, int passwordDataLength, PublicKey publicKey, uint connectionSocketIndex)
	{
		return (ConnectionAttemptResult)RakNetPINVOKE.RakPeerInterface_Connect__SWIG_3(swigCPtr, host, remotePort, passwordData, passwordDataLength, PublicKey.getCPtr(publicKey), connectionSocketIndex);
	}

	public virtual ConnectionAttemptResult Connect(string host, ushort remotePort, string passwordData, int passwordDataLength, PublicKey publicKey)
	{
		return (ConnectionAttemptResult)RakNetPINVOKE.RakPeerInterface_Connect__SWIG_4(swigCPtr, host, remotePort, passwordData, passwordDataLength, PublicKey.getCPtr(publicKey));
	}

	public virtual ConnectionAttemptResult Connect(string host, ushort remotePort, string passwordData, int passwordDataLength)
	{
		return (ConnectionAttemptResult)RakNetPINVOKE.RakPeerInterface_Connect__SWIG_5(swigCPtr, host, remotePort, passwordData, passwordDataLength);
	}

	public virtual void Shutdown(uint blockDuration, byte orderingChannel, PacketPriority disconnectionNotificationPriority)
	{
		RakNetPINVOKE.RakPeerInterface_Shutdown__SWIG_0(swigCPtr, blockDuration, orderingChannel, (int)disconnectionNotificationPriority);
	}

	public virtual void Shutdown(uint blockDuration, byte orderingChannel)
	{
		RakNetPINVOKE.RakPeerInterface_Shutdown__SWIG_1(swigCPtr, blockDuration, orderingChannel);
	}

	public virtual void Shutdown(uint blockDuration)
	{
		RakNetPINVOKE.RakPeerInterface_Shutdown__SWIG_2(swigCPtr, blockDuration);
	}

	public virtual bool IsActive()
	{
		return RakNetPINVOKE.RakPeerInterface_IsActive(swigCPtr);
	}

	public virtual uint GetNextSendReceipt()
	{
		return RakNetPINVOKE.RakPeerInterface_GetNextSendReceipt(swigCPtr);
	}

	public virtual uint IncrementNextSendReceipt()
	{
		return RakNetPINVOKE.RakPeerInterface_IncrementNextSendReceipt(swigCPtr);
	}

	public virtual uint Send(string data, int length, PacketPriority priority, PacketReliability reliability, char orderingChannel, AddressOrGUID systemIdentifier, bool broadcast, uint forceReceiptNumber)
	{
		uint result = RakNetPINVOKE.RakPeerInterface_Send__SWIG_0(swigCPtr, data, length, (int)priority, (int)reliability, orderingChannel, AddressOrGUID.getCPtr(systemIdentifier), broadcast, forceReceiptNumber);
		if (RakNetPINVOKE.SWIGPendingException.Pending)
		{
			throw RakNetPINVOKE.SWIGPendingException.Retrieve();
		}
		return result;
	}

	public virtual uint Send(string data, int length, PacketPriority priority, PacketReliability reliability, char orderingChannel, AddressOrGUID systemIdentifier, bool broadcast)
	{
		uint result = RakNetPINVOKE.RakPeerInterface_Send__SWIG_1(swigCPtr, data, length, (int)priority, (int)reliability, orderingChannel, AddressOrGUID.getCPtr(systemIdentifier), broadcast);
		if (RakNetPINVOKE.SWIGPendingException.Pending)
		{
			throw RakNetPINVOKE.SWIGPendingException.Retrieve();
		}
		return result;
	}

	public virtual void SendLoopback(string data, int length)
	{
		RakNetPINVOKE.RakPeerInterface_SendLoopback__SWIG_0(swigCPtr, data, length);
	}

	public virtual uint Send(BitStream bitStream, PacketPriority priority, PacketReliability reliability, char orderingChannel, AddressOrGUID systemIdentifier, bool broadcast, uint forceReceiptNumber)
	{
		uint result = RakNetPINVOKE.RakPeerInterface_Send__SWIG_2(swigCPtr, BitStream.getCPtr(bitStream), (int)priority, (int)reliability, orderingChannel, AddressOrGUID.getCPtr(systemIdentifier), broadcast, forceReceiptNumber);
		if (RakNetPINVOKE.SWIGPendingException.Pending)
		{
			throw RakNetPINVOKE.SWIGPendingException.Retrieve();
		}
		return result;
	}

	public virtual uint Send(BitStream bitStream, PacketPriority priority, PacketReliability reliability, char orderingChannel, AddressOrGUID systemIdentifier, bool broadcast)
	{
		uint result = RakNetPINVOKE.RakPeerInterface_Send__SWIG_3(swigCPtr, BitStream.getCPtr(bitStream), (int)priority, (int)reliability, orderingChannel, AddressOrGUID.getCPtr(systemIdentifier), broadcast);
		if (RakNetPINVOKE.SWIGPendingException.Pending)
		{
			throw RakNetPINVOKE.SWIGPendingException.Retrieve();
		}
		return result;
	}

	public virtual Packet Receive()
	{
		IntPtr intPtr = RakNetPINVOKE.RakPeerInterface_Receive(swigCPtr);
		return (!(intPtr == IntPtr.Zero)) ? new Packet(intPtr, cMemoryOwn: false) : null;
	}

	public virtual void DeallocatePacket(Packet packet)
	{
		RakNetPINVOKE.RakPeerInterface_DeallocatePacket(swigCPtr, Packet.getCPtr(packet));
	}

	public virtual uint GetMaximumNumberOfPeers()
	{
		return RakNetPINVOKE.RakPeerInterface_GetMaximumNumberOfPeers(swigCPtr);
	}

	public virtual void CloseConnection(AddressOrGUID target, bool sendDisconnectionNotification, byte orderingChannel, PacketPriority disconnectionNotificationPriority)
	{
		RakNetPINVOKE.RakPeerInterface_CloseConnection__SWIG_0(swigCPtr, AddressOrGUID.getCPtr(target), sendDisconnectionNotification, orderingChannel, (int)disconnectionNotificationPriority);
		if (RakNetPINVOKE.SWIGPendingException.Pending)
		{
			throw RakNetPINVOKE.SWIGPendingException.Retrieve();
		}
	}

	public virtual void CloseConnection(AddressOrGUID target, bool sendDisconnectionNotification, byte orderingChannel)
	{
		RakNetPINVOKE.RakPeerInterface_CloseConnection__SWIG_1(swigCPtr, AddressOrGUID.getCPtr(target), sendDisconnectionNotification, orderingChannel);
		if (RakNetPINVOKE.SWIGPendingException.Pending)
		{
			throw RakNetPINVOKE.SWIGPendingException.Retrieve();
		}
	}

	public virtual void CloseConnection(AddressOrGUID target, bool sendDisconnectionNotification)
	{
		RakNetPINVOKE.RakPeerInterface_CloseConnection__SWIG_2(swigCPtr, AddressOrGUID.getCPtr(target), sendDisconnectionNotification);
		if (RakNetPINVOKE.SWIGPendingException.Pending)
		{
			throw RakNetPINVOKE.SWIGPendingException.Retrieve();
		}
	}

	public virtual ConnectionState GetConnectionState(AddressOrGUID systemIdentifier)
	{
		ConnectionState result = (ConnectionState)RakNetPINVOKE.RakPeerInterface_GetConnectionState(swigCPtr, AddressOrGUID.getCPtr(systemIdentifier));
		if (RakNetPINVOKE.SWIGPendingException.Pending)
		{
			throw RakNetPINVOKE.SWIGPendingException.Retrieve();
		}
		return result;
	}

	public virtual void CancelConnectionAttempt(SystemAddress target)
	{
		RakNetPINVOKE.RakPeerInterface_CancelConnectionAttempt(swigCPtr, SystemAddress.getCPtr(target));
		if (RakNetPINVOKE.SWIGPendingException.Pending)
		{
			throw RakNetPINVOKE.SWIGPendingException.Retrieve();
		}
	}

	public virtual int GetIndexFromSystemAddress(SystemAddress systemAddress)
	{
		int result = RakNetPINVOKE.RakPeerInterface_GetIndexFromSystemAddress(swigCPtr, SystemAddress.getCPtr(systemAddress));
		if (RakNetPINVOKE.SWIGPendingException.Pending)
		{
			throw RakNetPINVOKE.SWIGPendingException.Retrieve();
		}
		return result;
	}

	public virtual SystemAddress GetSystemAddressFromIndex(uint index)
	{
		return new SystemAddress(RakNetPINVOKE.RakPeerInterface_GetSystemAddressFromIndex(swigCPtr, index), cMemoryOwn: true);
	}

	public virtual RakNetGUID GetGUIDFromIndex(uint index)
	{
		return new RakNetGUID(RakNetPINVOKE.RakPeerInterface_GetGUIDFromIndex(swigCPtr, index), cMemoryOwn: true);
	}

	public virtual void GetSystemList(RakNetListSystemAddress addresses, RakNetListRakNetGUID guids)
	{
		RakNetPINVOKE.RakPeerInterface_GetSystemList(swigCPtr, RakNetListSystemAddress.getCPtr(addresses), RakNetListRakNetGUID.getCPtr(guids));
		if (RakNetPINVOKE.SWIGPendingException.Pending)
		{
			throw RakNetPINVOKE.SWIGPendingException.Retrieve();
		}
	}

	public virtual void AddToBanList(string IP, uint milliseconds)
	{
		RakNetPINVOKE.RakPeerInterface_AddToBanList__SWIG_0(swigCPtr, IP, milliseconds);
	}

	public virtual void AddToBanList(string IP)
	{
		RakNetPINVOKE.RakPeerInterface_AddToBanList__SWIG_1(swigCPtr, IP);
	}

	public virtual void RemoveFromBanList(string IP)
	{
		RakNetPINVOKE.RakPeerInterface_RemoveFromBanList(swigCPtr, IP);
	}

	public virtual void ClearBanList()
	{
		RakNetPINVOKE.RakPeerInterface_ClearBanList(swigCPtr);
	}

	public virtual bool IsBanned(string IP)
	{
		return RakNetPINVOKE.RakPeerInterface_IsBanned(swigCPtr, IP);
	}

	public virtual void SetLimitIPConnectionFrequency(bool b)
	{
		RakNetPINVOKE.RakPeerInterface_SetLimitIPConnectionFrequency(swigCPtr, b);
	}

	public virtual void Ping(SystemAddress target)
	{
		RakNetPINVOKE.RakPeerInterface_Ping__SWIG_0(swigCPtr, SystemAddress.getCPtr(target));
		if (RakNetPINVOKE.SWIGPendingException.Pending)
		{
			throw RakNetPINVOKE.SWIGPendingException.Retrieve();
		}
	}

	public virtual bool Ping(string host, ushort remotePort, bool onlyReplyOnAcceptingConnections, uint connectionSocketIndex)
	{
		return RakNetPINVOKE.RakPeerInterface_Ping__SWIG_1(swigCPtr, host, remotePort, onlyReplyOnAcceptingConnections, connectionSocketIndex);
	}

	public virtual bool Ping(string host, ushort remotePort, bool onlyReplyOnAcceptingConnections)
	{
		return RakNetPINVOKE.RakPeerInterface_Ping__SWIG_2(swigCPtr, host, remotePort, onlyReplyOnAcceptingConnections);
	}

	public virtual int GetAveragePing(AddressOrGUID systemIdentifier)
	{
		int result = RakNetPINVOKE.RakPeerInterface_GetAveragePing(swigCPtr, AddressOrGUID.getCPtr(systemIdentifier));
		if (RakNetPINVOKE.SWIGPendingException.Pending)
		{
			throw RakNetPINVOKE.SWIGPendingException.Retrieve();
		}
		return result;
	}

	public virtual int GetLastPing(AddressOrGUID systemIdentifier)
	{
		int result = RakNetPINVOKE.RakPeerInterface_GetLastPing(swigCPtr, AddressOrGUID.getCPtr(systemIdentifier));
		if (RakNetPINVOKE.SWIGPendingException.Pending)
		{
			throw RakNetPINVOKE.SWIGPendingException.Retrieve();
		}
		return result;
	}

	public virtual int GetLowestPing(AddressOrGUID systemIdentifier)
	{
		int result = RakNetPINVOKE.RakPeerInterface_GetLowestPing(swigCPtr, AddressOrGUID.getCPtr(systemIdentifier));
		if (RakNetPINVOKE.SWIGPendingException.Pending)
		{
			throw RakNetPINVOKE.SWIGPendingException.Retrieve();
		}
		return result;
	}

	public virtual void SetOccasionalPing(bool doPing)
	{
		RakNetPINVOKE.RakPeerInterface_SetOccasionalPing(swigCPtr, doPing);
	}

	public virtual ulong GetClockDifferential(AddressOrGUID systemIdentifier)
	{
		ulong result = RakNetPINVOKE.RakPeerInterface_GetClockDifferential(swigCPtr, AddressOrGUID.getCPtr(systemIdentifier));
		if (RakNetPINVOKE.SWIGPendingException.Pending)
		{
			throw RakNetPINVOKE.SWIGPendingException.Retrieve();
		}
		return result;
	}

	public virtual void SetOfflinePingResponse(string data, uint length)
	{
		RakNetPINVOKE.RakPeerInterface_SetOfflinePingResponse__SWIG_0(swigCPtr, data, length);
	}

	public virtual SystemAddress GetInternalID(SystemAddress systemAddress, int index)
	{
		SystemAddress result = new SystemAddress(RakNetPINVOKE.RakPeerInterface_GetInternalID__SWIG_0(swigCPtr, SystemAddress.getCPtr(systemAddress), index), cMemoryOwn: true);
		if (RakNetPINVOKE.SWIGPendingException.Pending)
		{
			throw RakNetPINVOKE.SWIGPendingException.Retrieve();
		}
		return result;
	}

	public virtual SystemAddress GetInternalID(SystemAddress systemAddress)
	{
		SystemAddress result = new SystemAddress(RakNetPINVOKE.RakPeerInterface_GetInternalID__SWIG_1(swigCPtr, SystemAddress.getCPtr(systemAddress)), cMemoryOwn: true);
		if (RakNetPINVOKE.SWIGPendingException.Pending)
		{
			throw RakNetPINVOKE.SWIGPendingException.Retrieve();
		}
		return result;
	}

	public virtual SystemAddress GetInternalID()
	{
		return new SystemAddress(RakNetPINVOKE.RakPeerInterface_GetInternalID__SWIG_2(swigCPtr), cMemoryOwn: true);
	}

	public virtual void SetInternalID(SystemAddress systemAddress, int index)
	{
		RakNetPINVOKE.RakPeerInterface_SetInternalID__SWIG_0(swigCPtr, SystemAddress.getCPtr(systemAddress), index);
		if (RakNetPINVOKE.SWIGPendingException.Pending)
		{
			throw RakNetPINVOKE.SWIGPendingException.Retrieve();
		}
	}

	public virtual void SetInternalID(SystemAddress systemAddress)
	{
		RakNetPINVOKE.RakPeerInterface_SetInternalID__SWIG_1(swigCPtr, SystemAddress.getCPtr(systemAddress));
		if (RakNetPINVOKE.SWIGPendingException.Pending)
		{
			throw RakNetPINVOKE.SWIGPendingException.Retrieve();
		}
	}

	public virtual SystemAddress GetExternalID(SystemAddress target)
	{
		SystemAddress result = new SystemAddress(RakNetPINVOKE.RakPeerInterface_GetExternalID(swigCPtr, SystemAddress.getCPtr(target)), cMemoryOwn: true);
		if (RakNetPINVOKE.SWIGPendingException.Pending)
		{
			throw RakNetPINVOKE.SWIGPendingException.Retrieve();
		}
		return result;
	}

	public virtual RakNetGUID GetMyGUID()
	{
		return new RakNetGUID(RakNetPINVOKE.RakPeerInterface_GetMyGUID(swigCPtr), cMemoryOwn: true);
	}

	public virtual SystemAddress GetMyBoundAddress(int socketIndex)
	{
		return new SystemAddress(RakNetPINVOKE.RakPeerInterface_GetMyBoundAddress__SWIG_0(swigCPtr, socketIndex), cMemoryOwn: true);
	}

	public virtual SystemAddress GetMyBoundAddress()
	{
		return new SystemAddress(RakNetPINVOKE.RakPeerInterface_GetMyBoundAddress__SWIG_1(swigCPtr), cMemoryOwn: true);
	}

	public static ulong Get64BitUniqueRandomNumber()
	{
		return RakNetPINVOKE.RakPeerInterface_Get64BitUniqueRandomNumber();
	}

	public virtual RakNetGUID GetGuidFromSystemAddress(SystemAddress input)
	{
		RakNetGUID result = new RakNetGUID(RakNetPINVOKE.RakPeerInterface_GetGuidFromSystemAddress(swigCPtr, SystemAddress.getCPtr(input)), cMemoryOwn: false);
		if (RakNetPINVOKE.SWIGPendingException.Pending)
		{
			throw RakNetPINVOKE.SWIGPendingException.Retrieve();
		}
		return result;
	}

	public virtual SystemAddress GetSystemAddressFromGuid(RakNetGUID input)
	{
		SystemAddress result = new SystemAddress(RakNetPINVOKE.RakPeerInterface_GetSystemAddressFromGuid(swigCPtr, RakNetGUID.getCPtr(input)), cMemoryOwn: true);
		if (RakNetPINVOKE.SWIGPendingException.Pending)
		{
			throw RakNetPINVOKE.SWIGPendingException.Retrieve();
		}
		return result;
	}

	public virtual bool GetClientPublicKeyFromSystemAddress(SystemAddress input, string client_public_key)
	{
		bool result = RakNetPINVOKE.RakPeerInterface_GetClientPublicKeyFromSystemAddress(swigCPtr, SystemAddress.getCPtr(input), client_public_key);
		if (RakNetPINVOKE.SWIGPendingException.Pending)
		{
			throw RakNetPINVOKE.SWIGPendingException.Retrieve();
		}
		return result;
	}

	public virtual void SetTimeoutTime(uint timeMS, SystemAddress target)
	{
		RakNetPINVOKE.RakPeerInterface_SetTimeoutTime(swigCPtr, timeMS, SystemAddress.getCPtr(target));
		if (RakNetPINVOKE.SWIGPendingException.Pending)
		{
			throw RakNetPINVOKE.SWIGPendingException.Retrieve();
		}
	}

	public virtual uint GetTimeoutTime(SystemAddress target)
	{
		uint result = RakNetPINVOKE.RakPeerInterface_GetTimeoutTime(swigCPtr, SystemAddress.getCPtr(target));
		if (RakNetPINVOKE.SWIGPendingException.Pending)
		{
			throw RakNetPINVOKE.SWIGPendingException.Retrieve();
		}
		return result;
	}

	public virtual int GetMTUSize(SystemAddress target)
	{
		int result = RakNetPINVOKE.RakPeerInterface_GetMTUSize(swigCPtr, SystemAddress.getCPtr(target));
		if (RakNetPINVOKE.SWIGPendingException.Pending)
		{
			throw RakNetPINVOKE.SWIGPendingException.Retrieve();
		}
		return result;
	}

	public virtual uint GetNumberOfAddresses()
	{
		return RakNetPINVOKE.RakPeerInterface_GetNumberOfAddresses(swigCPtr);
	}

	public virtual string GetLocalIP(uint index)
	{
		return RakNetPINVOKE.RakPeerInterface_GetLocalIP(swigCPtr, index);
	}

	public virtual bool IsLocalIP(string ip)
	{
		return RakNetPINVOKE.RakPeerInterface_IsLocalIP(swigCPtr, ip);
	}

	public virtual void AllowConnectionResponseIPMigration(bool allow)
	{
		RakNetPINVOKE.RakPeerInterface_AllowConnectionResponseIPMigration(swigCPtr, allow);
	}

	public virtual bool AdvertiseSystem(string host, ushort remotePort, string data, int dataLength, uint connectionSocketIndex)
	{
		return RakNetPINVOKE.RakPeerInterface_AdvertiseSystem__SWIG_0(swigCPtr, host, remotePort, data, dataLength, connectionSocketIndex);
	}

	public virtual bool AdvertiseSystem(string host, ushort remotePort, string data, int dataLength)
	{
		return RakNetPINVOKE.RakPeerInterface_AdvertiseSystem__SWIG_1(swigCPtr, host, remotePort, data, dataLength);
	}

	public virtual void SetSplitMessageProgressInterval(int interval)
	{
		RakNetPINVOKE.RakPeerInterface_SetSplitMessageProgressInterval(swigCPtr, interval);
	}

	public virtual int GetSplitMessageProgressInterval()
	{
		return RakNetPINVOKE.RakPeerInterface_GetSplitMessageProgressInterval(swigCPtr);
	}

	public virtual void SetUnreliableTimeout(uint timeoutMS)
	{
		RakNetPINVOKE.RakPeerInterface_SetUnreliableTimeout(swigCPtr, timeoutMS);
	}

	public virtual void SendTTL(string host, ushort remotePort, int ttl, uint connectionSocketIndex)
	{
		RakNetPINVOKE.RakPeerInterface_SendTTL__SWIG_0(swigCPtr, host, remotePort, ttl, connectionSocketIndex);
	}

	public virtual void SendTTL(string host, ushort remotePort, int ttl)
	{
		RakNetPINVOKE.RakPeerInterface_SendTTL__SWIG_1(swigCPtr, host, remotePort, ttl);
	}

	public virtual void AttachPlugin(PluginInterface2 plugin)
	{
		RakNetPINVOKE.RakPeerInterface_AttachPlugin(swigCPtr, PluginInterface2.getCPtr(plugin));
	}

	public virtual void DetachPlugin(PluginInterface2 messageHandler)
	{
		RakNetPINVOKE.RakPeerInterface_DetachPlugin(swigCPtr, PluginInterface2.getCPtr(messageHandler));
	}

	public virtual void PushBackPacket(Packet packet, bool pushAtHead)
	{
		RakNetPINVOKE.RakPeerInterface_PushBackPacket(swigCPtr, Packet.getCPtr(packet), pushAtHead);
	}

	public virtual void ChangeSystemAddress(RakNetGUID guid, SystemAddress systemAddress)
	{
		RakNetPINVOKE.RakPeerInterface_ChangeSystemAddress(swigCPtr, RakNetGUID.getCPtr(guid), SystemAddress.getCPtr(systemAddress));
		if (RakNetPINVOKE.SWIGPendingException.Pending)
		{
			throw RakNetPINVOKE.SWIGPendingException.Retrieve();
		}
	}

	public virtual Packet AllocatePacket(uint dataSize)
	{
		IntPtr intPtr = RakNetPINVOKE.RakPeerInterface_AllocatePacket(swigCPtr, dataSize);
		return (!(intPtr == IntPtr.Zero)) ? new Packet(intPtr, cMemoryOwn: false) : null;
	}

	public virtual void GetSockets(SWIGTYPE_p_DataStructures__ListT_RakNetSocket2_p_t sockets)
	{
		RakNetPINVOKE.RakPeerInterface_GetSockets(swigCPtr, SWIGTYPE_p_DataStructures__ListT_RakNetSocket2_p_t.getCPtr(sockets));
		if (RakNetPINVOKE.SWIGPendingException.Pending)
		{
			throw RakNetPINVOKE.SWIGPendingException.Retrieve();
		}
	}

	public virtual void WriteOutOfBandHeader(BitStream bitStream)
	{
		RakNetPINVOKE.RakPeerInterface_WriteOutOfBandHeader(swigCPtr, BitStream.getCPtr(bitStream));
	}

	public virtual void ApplyNetworkSimulator(float packetloss, ushort minExtraPing, ushort extraPingVariance)
	{
		RakNetPINVOKE.RakPeerInterface_ApplyNetworkSimulator(swigCPtr, packetloss, minExtraPing, extraPingVariance);
	}

	public virtual void SetPerConnectionOutgoingBandwidthLimit(uint maxBitsPerSecond)
	{
		RakNetPINVOKE.RakPeerInterface_SetPerConnectionOutgoingBandwidthLimit(swigCPtr, maxBitsPerSecond);
	}

	public virtual bool IsNetworkSimulatorActive()
	{
		return RakNetPINVOKE.RakPeerInterface_IsNetworkSimulatorActive(swigCPtr);
	}

	public virtual RakNetStatistics GetStatistics(SystemAddress systemAddress, RakNetStatistics rns)
	{
		IntPtr intPtr = RakNetPINVOKE.RakPeerInterface_GetStatistics__SWIG_0(swigCPtr, SystemAddress.getCPtr(systemAddress), RakNetStatistics.getCPtr(rns));
		RakNetStatistics result = ((!(intPtr == IntPtr.Zero)) ? new RakNetStatistics(intPtr, cMemoryOwn: false) : null);
		if (RakNetPINVOKE.SWIGPendingException.Pending)
		{
			throw RakNetPINVOKE.SWIGPendingException.Retrieve();
		}
		return result;
	}

	public virtual RakNetStatistics GetStatistics(SystemAddress systemAddress)
	{
		IntPtr intPtr = RakNetPINVOKE.RakPeerInterface_GetStatistics__SWIG_1(swigCPtr, SystemAddress.getCPtr(systemAddress));
		RakNetStatistics result = ((!(intPtr == IntPtr.Zero)) ? new RakNetStatistics(intPtr, cMemoryOwn: false) : null);
		if (RakNetPINVOKE.SWIGPendingException.Pending)
		{
			throw RakNetPINVOKE.SWIGPendingException.Retrieve();
		}
		return result;
	}

	public virtual bool GetStatistics(uint index, RakNetStatistics rns)
	{
		return RakNetPINVOKE.RakPeerInterface_GetStatistics__SWIG_2(swigCPtr, index, RakNetStatistics.getCPtr(rns));
	}

	public virtual uint GetReceiveBufferSize()
	{
		return RakNetPINVOKE.RakPeerInterface_GetReceiveBufferSize(swigCPtr);
	}

	public virtual bool RunUpdateCycle(BitStream updateBitStream)
	{
		bool result = RakNetPINVOKE.RakPeerInterface_RunUpdateCycle(swigCPtr, BitStream.getCPtr(updateBitStream));
		if (RakNetPINVOKE.SWIGPendingException.Pending)
		{
			throw RakNetPINVOKE.SWIGPendingException.Retrieve();
		}
		return result;
	}

	public virtual bool SendOutOfBand(string host, ushort remotePort, string data, uint dataLength, uint connectionSocketIndex)
	{
		return RakNetPINVOKE.RakPeerInterface_SendOutOfBand__SWIG_0(swigCPtr, host, remotePort, data, dataLength, connectionSocketIndex);
	}

	public virtual bool SendOutOfBand(string host, ushort remotePort, string data, uint dataLength)
	{
		return RakNetPINVOKE.RakPeerInterface_SendOutOfBand__SWIG_1(swigCPtr, host, remotePort, data, dataLength);
	}

	public uint Send(byte[] inByteArray, int length, PacketPriority priority, PacketReliability reliability, char orderingChannel, AddressOrGUID systemIdentifier, bool broadcast)
	{
		uint result = RakNetPINVOKE.RakPeerInterface_Send__SWIG_4(swigCPtr, inByteArray, length, (int)priority, (int)reliability, orderingChannel, AddressOrGUID.getCPtr(systemIdentifier), broadcast);
		if (RakNetPINVOKE.SWIGPendingException.Pending)
		{
			throw RakNetPINVOKE.SWIGPendingException.Retrieve();
		}
		return result;
	}

	public void SendLoopback(byte[] inByteArray, int length)
	{
		RakNetPINVOKE.RakPeerInterface_SendLoopback__SWIG_1(swigCPtr, inByteArray, length);
	}

	public void SetOfflinePingResponse(byte[] inByteArray, uint length)
	{
		RakNetPINVOKE.RakPeerInterface_SetOfflinePingResponse__SWIG_1(swigCPtr, inByteArray, length);
	}

	public bool AdvertiseSystem(string host, ushort remotePort, byte[] inByteArray, int dataLength, uint connectionSocketIndex)
	{
		return RakNetPINVOKE.RakPeerInterface_AdvertiseSystem__SWIG_2(swigCPtr, host, remotePort, inByteArray, dataLength, connectionSocketIndex);
	}

	public bool AdvertiseSystem(string host, ushort remotePort, byte[] inByteArray, int dataLength)
	{
		return RakNetPINVOKE.RakPeerInterface_AdvertiseSystem__SWIG_3(swigCPtr, host, remotePort, inByteArray, dataLength);
	}

	private string CSharpGetIncomingPasswordHelper(string passwordData, ref int passwordDataLength)
	{
		return RakNetPINVOKE.RakPeerInterface_CSharpGetIncomingPasswordHelper(swigCPtr, passwordData, ref passwordDataLength);
	}

	public void SetIncomingPassword(byte[] passwordDataByteArray, int passwordDataLength)
	{
		RakNetPINVOKE.RakPeerInterface_SetIncomingPassword__SWIG_1(swigCPtr, passwordDataByteArray, passwordDataLength);
	}

	public void GetIncomingPassword(byte[] passwordDataByteArray, ref int passwordDataLength)
	{
		RakNetPINVOKE.RakPeerInterface_GetIncomingPassword(swigCPtr, passwordDataByteArray, ref passwordDataLength);
	}

	private void CSharpGetOfflinePingResponseHelper(byte[] inOutByteArray, out uint outLength)
	{
		RakNetPINVOKE.RakPeerInterface_CSharpGetOfflinePingResponseHelper(swigCPtr, inOutByteArray, out outLength);
	}

	public bool GetConnectionList(RakNetListSystemAddress remoteSystems, ref ushort numberOfSystems)
	{
		return RakNetPINVOKE.RakPeerInterface_GetConnectionList(swigCPtr, RakNetListSystemAddress.getCPtr(remoteSystems), ref numberOfSystems);
	}
}
