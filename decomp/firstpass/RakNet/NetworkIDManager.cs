using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace RakNet;

public class NetworkIDManager : IDisposable
{
	private HandleRef swigCPtr;

	protected bool swigCMemOwn;

	public Dictionary<IntPtr, NetworkIDObject> pointerDictionary = new Dictionary<IntPtr, NetworkIDObject>();

	internal NetworkIDManager(IntPtr cPtr, bool cMemoryOwn)
	{
		swigCMemOwn = cMemoryOwn;
		swigCPtr = new HandleRef(this, cPtr);
	}

	public NetworkIDManager()
		: this(RakNetPINVOKE.new_NetworkIDManager(), cMemoryOwn: true)
	{
	}

	internal static HandleRef getCPtr(NetworkIDManager obj)
	{
		return obj?.swigCPtr ?? new HandleRef(null, IntPtr.Zero);
	}

	~NetworkIDManager()
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
					RakNetPINVOKE.delete_NetworkIDManager(swigCPtr);
				}
				swigCPtr = new HandleRef(null, IntPtr.Zero);
			}
			GC.SuppressFinalize(this);
		}
	}

	public NetworkIDObject GET_BASE_OBJECT_FROM_ID(ulong x)
	{
		return pointerDictionary[GET_BASE_OBJECT_FROM_IDORIG(x).GetIntPtr()];
	}

	public static NetworkIDManager GetInstance()
	{
		IntPtr intPtr = RakNetPINVOKE.NetworkIDManager_GetInstance();
		return (!(intPtr == IntPtr.Zero)) ? new NetworkIDManager(intPtr, cMemoryOwn: false) : null;
	}

	public static void DestroyInstance(NetworkIDManager i)
	{
		RakNetPINVOKE.NetworkIDManager_DestroyInstance(getCPtr(i));
	}

	public void Clear()
	{
		RakNetPINVOKE.NetworkIDManager_Clear(swigCPtr);
	}

	protected NetworkIDObject GET_BASE_OBJECT_FROM_IDORIG(ulong x)
	{
		IntPtr intPtr = RakNetPINVOKE.NetworkIDManager_GET_BASE_OBJECT_FROM_IDORIG(swigCPtr, x);
		return (!(intPtr == IntPtr.Zero)) ? new NetworkIDObject(intPtr, cMemoryOwn: false) : null;
	}
}
