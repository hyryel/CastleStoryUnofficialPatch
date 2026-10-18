using System;
using System.Runtime.InteropServices;

namespace RakNet;

public class TransportInterface : IDisposable
{
	private HandleRef swigCPtr;

	protected bool swigCMemOwn;

	internal TransportInterface(IntPtr cPtr, bool cMemoryOwn)
	{
		swigCMemOwn = cMemoryOwn;
		swigCPtr = new HandleRef(this, cPtr);
	}

	internal static HandleRef getCPtr(TransportInterface obj)
	{
		return obj?.swigCPtr ?? new HandleRef(null, IntPtr.Zero);
	}

	~TransportInterface()
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
					RakNetPINVOKE.delete_TransportInterface(swigCPtr);
				}
				swigCPtr = new HandleRef(null, IntPtr.Zero);
			}
			GC.SuppressFinalize(this);
		}
	}

	public virtual bool Start(ushort port, bool serverMode)
	{
		return RakNetPINVOKE.TransportInterface_Start(swigCPtr, port, serverMode);
	}

	public virtual void Stop()
	{
		RakNetPINVOKE.TransportInterface_Stop(swigCPtr);
	}

	public virtual void CloseConnection(SystemAddress systemAddress)
	{
		RakNetPINVOKE.TransportInterface_CloseConnection(swigCPtr, SystemAddress.getCPtr(systemAddress));
		if (RakNetPINVOKE.SWIGPendingException.Pending)
		{
			throw RakNetPINVOKE.SWIGPendingException.Retrieve();
		}
	}

	public virtual Packet Receive()
	{
		IntPtr intPtr = RakNetPINVOKE.TransportInterface_Receive(swigCPtr);
		return (!(intPtr == IntPtr.Zero)) ? new Packet(intPtr, cMemoryOwn: false) : null;
	}

	public virtual void DeallocatePacket(Packet packet)
	{
		RakNetPINVOKE.TransportInterface_DeallocatePacket(swigCPtr, Packet.getCPtr(packet));
	}

	public virtual SystemAddress HasNewIncomingConnection()
	{
		return new SystemAddress(RakNetPINVOKE.TransportInterface_HasNewIncomingConnection(swigCPtr), cMemoryOwn: true);
	}

	public virtual SystemAddress HasLostConnection()
	{
		return new SystemAddress(RakNetPINVOKE.TransportInterface_HasLostConnection(swigCPtr), cMemoryOwn: true);
	}

	public virtual CommandParserInterface GetCommandParser()
	{
		IntPtr intPtr = RakNetPINVOKE.TransportInterface_GetCommandParser(swigCPtr);
		return (!(intPtr == IntPtr.Zero)) ? new CommandParserInterface(intPtr, cMemoryOwn: false) : null;
	}

	public void Send(SystemAddress systemAddress, byte[] inByteArray)
	{
		RakNetPINVOKE.TransportInterface_Send(swigCPtr, SystemAddress.getCPtr(systemAddress), inByteArray);
		if (RakNetPINVOKE.SWIGPendingException.Pending)
		{
			throw RakNetPINVOKE.SWIGPendingException.Retrieve();
		}
	}
}
