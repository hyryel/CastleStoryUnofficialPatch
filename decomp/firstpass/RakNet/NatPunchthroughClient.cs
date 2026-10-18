using System;
using System.Runtime.InteropServices;

namespace RakNet;

public class NatPunchthroughClient : PluginInterface2
{
	private HandleRef swigCPtr;

	public SWIGTYPE_p_RakNet__NatPunchthroughClient__SendPing sp
	{
		get
		{
			IntPtr intPtr = RakNetPINVOKE.NatPunchthroughClient_sp_get(swigCPtr);
			return (!(intPtr == IntPtr.Zero)) ? new SWIGTYPE_p_RakNet__NatPunchthroughClient__SendPing(intPtr, futureUse: false) : null;
		}
		set
		{
			RakNetPINVOKE.NatPunchthroughClient_sp_set(swigCPtr, SWIGTYPE_p_RakNet__NatPunchthroughClient__SendPing.getCPtr(value));
		}
	}

	internal NatPunchthroughClient(IntPtr cPtr, bool cMemoryOwn)
		: base(RakNetPINVOKE.NatPunchthroughClient_SWIGUpcast(cPtr), cMemoryOwn)
	{
		swigCPtr = new HandleRef(this, cPtr);
	}

	public NatPunchthroughClient()
		: this(RakNetPINVOKE.new_NatPunchthroughClient(), cMemoryOwn: true)
	{
	}

	internal static HandleRef getCPtr(NatPunchthroughClient obj)
	{
		return obj?.swigCPtr ?? new HandleRef(null, IntPtr.Zero);
	}

	~NatPunchthroughClient()
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
					RakNetPINVOKE.delete_NatPunchthroughClient(swigCPtr);
				}
				swigCPtr = new HandleRef(null, IntPtr.Zero);
			}
			GC.SuppressFinalize(this);
			base.Dispose();
		}
	}

	public static NatPunchthroughClient GetInstance()
	{
		IntPtr intPtr = RakNetPINVOKE.NatPunchthroughClient_GetInstance();
		return (!(intPtr == IntPtr.Zero)) ? new NatPunchthroughClient(intPtr, cMemoryOwn: false) : null;
	}

	public static void DestroyInstance(NatPunchthroughClient i)
	{
		RakNetPINVOKE.NatPunchthroughClient_DestroyInstance(getCPtr(i));
	}

	public void FindRouterPortStride(SystemAddress facilitator)
	{
		RakNetPINVOKE.NatPunchthroughClient_FindRouterPortStride(swigCPtr, SystemAddress.getCPtr(facilitator));
		if (RakNetPINVOKE.SWIGPendingException.Pending)
		{
			throw RakNetPINVOKE.SWIGPendingException.Retrieve();
		}
	}

	public bool OpenNAT(RakNetGUID destination, SystemAddress facilitator)
	{
		bool result = RakNetPINVOKE.NatPunchthroughClient_OpenNAT(swigCPtr, RakNetGUID.getCPtr(destination), SystemAddress.getCPtr(facilitator));
		if (RakNetPINVOKE.SWIGPendingException.Pending)
		{
			throw RakNetPINVOKE.SWIGPendingException.Retrieve();
		}
		return result;
	}

	public PunchthroughConfiguration GetPunchthroughConfiguration()
	{
		IntPtr intPtr = RakNetPINVOKE.NatPunchthroughClient_GetPunchthroughConfiguration(swigCPtr);
		return (!(intPtr == IntPtr.Zero)) ? new PunchthroughConfiguration(intPtr, cMemoryOwn: false) : null;
	}

	public void SetDebugInterface(NatPunchthroughDebugInterface i)
	{
		RakNetPINVOKE.NatPunchthroughClient_SetDebugInterface(swigCPtr, NatPunchthroughDebugInterface.getCPtr(i));
	}

	public void GetUPNPPortMappings(string externalPort, string internalPort, SystemAddress natPunchthroughServerAddress)
	{
		RakNetPINVOKE.NatPunchthroughClient_GetUPNPPortMappings(swigCPtr, externalPort, internalPort, SystemAddress.getCPtr(natPunchthroughServerAddress));
		if (RakNetPINVOKE.SWIGPendingException.Pending)
		{
			throw RakNetPINVOKE.SWIGPendingException.Retrieve();
		}
	}

	public void Clear()
	{
		RakNetPINVOKE.NatPunchthroughClient_Clear(swigCPtr);
	}
}
