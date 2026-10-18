using System;
using System.Runtime.InteropServices;

namespace RakNet;

public class Router2 : PluginInterface2
{
	public enum Router2RequestStates
	{
		R2RS_REQUEST_STATE_QUERY_FORWARDING,
		REQUEST_STATE_REQUEST_FORWARDING
	}

	private HandleRef swigCPtr;

	internal Router2(IntPtr cPtr, bool cMemoryOwn)
		: base(RakNetPINVOKE.Router2_SWIGUpcast(cPtr), cMemoryOwn)
	{
		swigCPtr = new HandleRef(this, cPtr);
	}

	public Router2()
		: this(RakNetPINVOKE.new_Router2(), cMemoryOwn: true)
	{
	}

	internal static HandleRef getCPtr(Router2 obj)
	{
		return obj?.swigCPtr ?? new HandleRef(null, IntPtr.Zero);
	}

	~Router2()
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
					RakNetPINVOKE.delete_Router2(swigCPtr);
				}
				swigCPtr = new HandleRef(null, IntPtr.Zero);
			}
			GC.SuppressFinalize(this);
			base.Dispose();
		}
	}

	public static Router2 GetInstance()
	{
		IntPtr intPtr = RakNetPINVOKE.Router2_GetInstance();
		return (!(intPtr == IntPtr.Zero)) ? new Router2(intPtr, cMemoryOwn: false) : null;
	}

	public static void DestroyInstance(Router2 i)
	{
		RakNetPINVOKE.Router2_DestroyInstance(getCPtr(i));
	}

	public void SetSocketFamily(ushort _socketFamily)
	{
		RakNetPINVOKE.Router2_SetSocketFamily(swigCPtr, _socketFamily);
	}

	public void EstablishRouting(RakNetGUID endpointGuid)
	{
		RakNetPINVOKE.Router2_EstablishRouting(swigCPtr, RakNetGUID.getCPtr(endpointGuid));
		if (RakNetPINVOKE.SWIGPendingException.Pending)
		{
			throw RakNetPINVOKE.SWIGPendingException.Retrieve();
		}
	}

	public void SetMaximumForwardingRequests(int max)
	{
		RakNetPINVOKE.Router2_SetMaximumForwardingRequests(swigCPtr, max);
	}

	public void SetDebugInterface(Router2DebugInterface _debugInterface)
	{
		RakNetPINVOKE.Router2_SetDebugInterface(swigCPtr, Router2DebugInterface.getCPtr(_debugInterface));
	}

	public Router2DebugInterface GetDebugInterface()
	{
		IntPtr intPtr = RakNetPINVOKE.Router2_GetDebugInterface(swigCPtr);
		return (!(intPtr == IntPtr.Zero)) ? new Router2DebugInterface(intPtr, cMemoryOwn: false) : null;
	}

	public uint GetConnectionRequestIndex(RakNetGUID endpointGuid)
	{
		uint result = RakNetPINVOKE.Router2_GetConnectionRequestIndex(swigCPtr, RakNetGUID.getCPtr(endpointGuid));
		if (RakNetPINVOKE.SWIGPendingException.Pending)
		{
			throw RakNetPINVOKE.SWIGPendingException.Retrieve();
		}
		return result;
	}
}
