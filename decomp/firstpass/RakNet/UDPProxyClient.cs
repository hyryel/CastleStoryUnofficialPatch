using System;
using System.Runtime.InteropServices;

namespace RakNet;

public class UDPProxyClient : PluginInterface2
{
	private HandleRef swigCPtr;

	internal UDPProxyClient(IntPtr cPtr, bool cMemoryOwn)
		: base(RakNetPINVOKE.UDPProxyClient_SWIGUpcast(cPtr), cMemoryOwn)
	{
		swigCPtr = new HandleRef(this, cPtr);
	}

	public UDPProxyClient()
		: this(RakNetPINVOKE.new_UDPProxyClient(), cMemoryOwn: true)
	{
	}

	internal static HandleRef getCPtr(UDPProxyClient obj)
	{
		return obj?.swigCPtr ?? new HandleRef(null, IntPtr.Zero);
	}

	~UDPProxyClient()
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
					RakNetPINVOKE.delete_UDPProxyClient(swigCPtr);
				}
				swigCPtr = new HandleRef(null, IntPtr.Zero);
			}
			GC.SuppressFinalize(this);
			base.Dispose();
		}
	}

	public static UDPProxyClient GetInstance()
	{
		IntPtr intPtr = RakNetPINVOKE.UDPProxyClient_GetInstance();
		return (!(intPtr == IntPtr.Zero)) ? new UDPProxyClient(intPtr, cMemoryOwn: false) : null;
	}

	public static void DestroyInstance(UDPProxyClient i)
	{
		RakNetPINVOKE.UDPProxyClient_DestroyInstance(getCPtr(i));
	}

	public void SetResultHandler(UDPProxyClientResultHandler rh)
	{
		RakNetPINVOKE.UDPProxyClient_SetResultHandler(swigCPtr, UDPProxyClientResultHandler.getCPtr(rh));
	}

	public bool RequestForwarding(SystemAddress proxyCoordinator, SystemAddress sourceAddress, SystemAddress targetAddressAsSeenFromCoordinator, uint timeoutOnNoDataMS, BitStream serverSelectionBitstream)
	{
		bool result = RakNetPINVOKE.UDPProxyClient_RequestForwarding__SWIG_0(swigCPtr, SystemAddress.getCPtr(proxyCoordinator), SystemAddress.getCPtr(sourceAddress), SystemAddress.getCPtr(targetAddressAsSeenFromCoordinator), timeoutOnNoDataMS, BitStream.getCPtr(serverSelectionBitstream));
		if (RakNetPINVOKE.SWIGPendingException.Pending)
		{
			throw RakNetPINVOKE.SWIGPendingException.Retrieve();
		}
		return result;
	}

	public bool RequestForwarding(SystemAddress proxyCoordinator, SystemAddress sourceAddress, SystemAddress targetAddressAsSeenFromCoordinator, uint timeoutOnNoDataMS)
	{
		bool result = RakNetPINVOKE.UDPProxyClient_RequestForwarding__SWIG_1(swigCPtr, SystemAddress.getCPtr(proxyCoordinator), SystemAddress.getCPtr(sourceAddress), SystemAddress.getCPtr(targetAddressAsSeenFromCoordinator), timeoutOnNoDataMS);
		if (RakNetPINVOKE.SWIGPendingException.Pending)
		{
			throw RakNetPINVOKE.SWIGPendingException.Retrieve();
		}
		return result;
	}

	public bool RequestForwarding(SystemAddress proxyCoordinator, SystemAddress sourceAddress, RakNetGUID targetGuid, uint timeoutOnNoDataMS, BitStream serverSelectionBitstream)
	{
		bool result = RakNetPINVOKE.UDPProxyClient_RequestForwarding__SWIG_2(swigCPtr, SystemAddress.getCPtr(proxyCoordinator), SystemAddress.getCPtr(sourceAddress), RakNetGUID.getCPtr(targetGuid), timeoutOnNoDataMS, BitStream.getCPtr(serverSelectionBitstream));
		if (RakNetPINVOKE.SWIGPendingException.Pending)
		{
			throw RakNetPINVOKE.SWIGPendingException.Retrieve();
		}
		return result;
	}

	public bool RequestForwarding(SystemAddress proxyCoordinator, SystemAddress sourceAddress, RakNetGUID targetGuid, uint timeoutOnNoDataMS)
	{
		bool result = RakNetPINVOKE.UDPProxyClient_RequestForwarding__SWIG_3(swigCPtr, SystemAddress.getCPtr(proxyCoordinator), SystemAddress.getCPtr(sourceAddress), RakNetGUID.getCPtr(targetGuid), timeoutOnNoDataMS);
		if (RakNetPINVOKE.SWIGPendingException.Pending)
		{
			throw RakNetPINVOKE.SWIGPendingException.Retrieve();
		}
		return result;
	}
}
