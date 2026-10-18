using System;
using System.Runtime.InteropServices;

namespace RakNet;

public class SWIGTYPE_p_RakNet__TM_World__JoinRequestHelper
{
	private HandleRef swigCPtr;

	internal SWIGTYPE_p_RakNet__TM_World__JoinRequestHelper(IntPtr cPtr, bool futureUse)
	{
		swigCPtr = new HandleRef(this, cPtr);
	}

	protected SWIGTYPE_p_RakNet__TM_World__JoinRequestHelper()
	{
		swigCPtr = new HandleRef(null, IntPtr.Zero);
	}

	internal static HandleRef getCPtr(SWIGTYPE_p_RakNet__TM_World__JoinRequestHelper obj)
	{
		return obj?.swigCPtr ?? new HandleRef(null, IntPtr.Zero);
	}
}
