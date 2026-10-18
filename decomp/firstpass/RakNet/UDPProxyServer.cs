using System;
using System.Runtime.InteropServices;

namespace RakNet;

public class UDPProxyServer : PluginInterface2
{
	private HandleRef swigCPtr;

	public UDPForwarder udpForwarder
	{
		get
		{
			IntPtr intPtr = RakNetPINVOKE.UDPProxyServer_udpForwarder_get(swigCPtr);
			return (!(intPtr == IntPtr.Zero)) ? new UDPForwarder(intPtr, cMemoryOwn: false) : null;
		}
		set
		{
			RakNetPINVOKE.UDPProxyServer_udpForwarder_set(swigCPtr, UDPForwarder.getCPtr(value));
		}
	}

	internal UDPProxyServer(IntPtr cPtr, bool cMemoryOwn)
		: base(RakNetPINVOKE.UDPProxyServer_SWIGUpcast(cPtr), cMemoryOwn)
	{
		swigCPtr = new HandleRef(this, cPtr);
	}

	public UDPProxyServer()
		: this(RakNetPINVOKE.new_UDPProxyServer(), cMemoryOwn: true)
	{
	}

	internal static HandleRef getCPtr(UDPProxyServer obj)
	{
		return obj?.swigCPtr ?? new HandleRef(null, IntPtr.Zero);
	}

	~UDPProxyServer()
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
					RakNetPINVOKE.delete_UDPProxyServer(swigCPtr);
				}
				swigCPtr = new HandleRef(null, IntPtr.Zero);
			}
			GC.SuppressFinalize(this);
			base.Dispose();
		}
	}

	public static UDPProxyServer GetInstance()
	{
		IntPtr intPtr = RakNetPINVOKE.UDPProxyServer_GetInstance();
		return (!(intPtr == IntPtr.Zero)) ? new UDPProxyServer(intPtr, cMemoryOwn: false) : null;
	}

	public static void DestroyInstance(UDPProxyServer i)
	{
		RakNetPINVOKE.UDPProxyServer_DestroyInstance(getCPtr(i));
	}

	public void SetSocketFamily(ushort _socketFamily)
	{
		RakNetPINVOKE.UDPProxyServer_SetSocketFamily(swigCPtr, _socketFamily);
	}

	public void SetResultHandler(UDPProxyServerResultHandler rh)
	{
		RakNetPINVOKE.UDPProxyServer_SetResultHandler(swigCPtr, UDPProxyServerResultHandler.getCPtr(rh));
	}

	public bool LoginToCoordinator(RakString password, SystemAddress coordinatorAddress)
	{
		bool result = RakNetPINVOKE.UDPProxyServer_LoginToCoordinator(swigCPtr, RakString.getCPtr(password), SystemAddress.getCPtr(coordinatorAddress));
		if (RakNetPINVOKE.SWIGPendingException.Pending)
		{
			throw RakNetPINVOKE.SWIGPendingException.Retrieve();
		}
		return result;
	}

	public void SetServerPublicIP(RakString ip)
	{
		RakNetPINVOKE.UDPProxyServer_SetServerPublicIP(swigCPtr, RakString.getCPtr(ip));
		if (RakNetPINVOKE.SWIGPendingException.Pending)
		{
			throw RakNetPINVOKE.SWIGPendingException.Retrieve();
		}
	}
}
