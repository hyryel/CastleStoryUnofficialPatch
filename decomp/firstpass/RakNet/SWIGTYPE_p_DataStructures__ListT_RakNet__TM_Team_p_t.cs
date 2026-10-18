using System;
using System.Runtime.InteropServices;

namespace RakNet;

public class SWIGTYPE_p_DataStructures__ListT_RakNet__TM_Team_p_t
{
	private HandleRef swigCPtr;

	internal SWIGTYPE_p_DataStructures__ListT_RakNet__TM_Team_p_t(IntPtr cPtr, bool futureUse)
	{
		swigCPtr = new HandleRef(this, cPtr);
	}

	protected SWIGTYPE_p_DataStructures__ListT_RakNet__TM_Team_p_t()
	{
		swigCPtr = new HandleRef(null, IntPtr.Zero);
	}

	internal static HandleRef getCPtr(SWIGTYPE_p_DataStructures__ListT_RakNet__TM_Team_p_t obj)
	{
		return obj?.swigCPtr ?? new HandleRef(null, IntPtr.Zero);
	}
}
