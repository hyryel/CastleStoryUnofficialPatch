using System;
using System.Runtime.InteropServices;

namespace RakNet;

public class SWIGTYPE_p_RakNet__ReadyEvent__RemoteSystem
{
	private HandleRef swigCPtr;

	internal SWIGTYPE_p_RakNet__ReadyEvent__RemoteSystem(IntPtr cPtr, bool futureUse)
	{
		swigCPtr = new HandleRef(this, cPtr);
	}

	protected SWIGTYPE_p_RakNet__ReadyEvent__RemoteSystem()
	{
		swigCPtr = new HandleRef(null, IntPtr.Zero);
	}

	internal static HandleRef getCPtr(SWIGTYPE_p_RakNet__ReadyEvent__RemoteSystem obj)
	{
		return obj?.swigCPtr ?? new HandleRef(null, IntPtr.Zero);
	}
}
