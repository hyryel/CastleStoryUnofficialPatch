using System;
using System.Runtime.InteropServices;

namespace RakNet;

public class UDPForwarder : IDisposable
{
	private HandleRef swigCPtr;

	protected bool swigCMemOwn;

	internal UDPForwarder(IntPtr cPtr, bool cMemoryOwn)
	{
		swigCMemOwn = cMemoryOwn;
		swigCPtr = new HandleRef(this, cPtr);
	}

	public UDPForwarder()
		: this(RakNetPINVOKE.new_UDPForwarder(), cMemoryOwn: true)
	{
	}

	internal static HandleRef getCPtr(UDPForwarder obj)
	{
		return obj?.swigCPtr ?? new HandleRef(null, IntPtr.Zero);
	}

	~UDPForwarder()
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
					RakNetPINVOKE.delete_UDPForwarder(swigCPtr);
				}
				swigCPtr = new HandleRef(null, IntPtr.Zero);
			}
			GC.SuppressFinalize(this);
		}
	}

	public void Startup()
	{
		RakNetPINVOKE.UDPForwarder_Startup(swigCPtr);
	}

	public void Shutdown()
	{
		RakNetPINVOKE.UDPForwarder_Shutdown(swigCPtr);
	}

	public void SetMaxForwardEntries(ushort maxEntries)
	{
		RakNetPINVOKE.UDPForwarder_SetMaxForwardEntries(swigCPtr, maxEntries);
	}

	public int GetMaxForwardEntries()
	{
		return RakNetPINVOKE.UDPForwarder_GetMaxForwardEntries(swigCPtr);
	}

	public int GetUsedForwardEntries()
	{
		return RakNetPINVOKE.UDPForwarder_GetUsedForwardEntries(swigCPtr);
	}

	public UDPForwarderResult StartForwarding(SystemAddress source, SystemAddress destination, uint timeoutOnNoDataMS, string forceHostAddress, ushort socketFamily, out ushort forwardingPort, out int forwardingSocket)
	{
		UDPForwarderResult result = (UDPForwarderResult)RakNetPINVOKE.UDPForwarder_StartForwarding(swigCPtr, SystemAddress.getCPtr(source), SystemAddress.getCPtr(destination), timeoutOnNoDataMS, forceHostAddress, socketFamily, out forwardingPort, out forwardingSocket);
		if (RakNetPINVOKE.SWIGPendingException.Pending)
		{
			throw RakNetPINVOKE.SWIGPendingException.Retrieve();
		}
		return result;
	}

	public void StopForwarding(SystemAddress source, SystemAddress destination)
	{
		RakNetPINVOKE.UDPForwarder_StopForwarding(swigCPtr, SystemAddress.getCPtr(source), SystemAddress.getCPtr(destination));
		if (RakNetPINVOKE.SWIGPendingException.Pending)
		{
			throw RakNetPINVOKE.SWIGPendingException.Retrieve();
		}
	}
}
