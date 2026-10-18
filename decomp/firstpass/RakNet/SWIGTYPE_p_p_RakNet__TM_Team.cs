using System;
using System.Runtime.InteropServices;

namespace RakNet;

public class SWIGTYPE_p_p_RakNet__TM_Team
{
	private HandleRef swigCPtr;

	internal SWIGTYPE_p_p_RakNet__TM_Team(IntPtr cPtr, bool futureUse)
	{
		swigCPtr = new HandleRef(this, cPtr);
	}

	protected SWIGTYPE_p_p_RakNet__TM_Team()
	{
		swigCPtr = new HandleRef(null, IntPtr.Zero);
	}

	internal static HandleRef getCPtr(SWIGTYPE_p_p_RakNet__TM_Team obj)
	{
		return obj?.swigCPtr ?? new HandleRef(null, IntPtr.Zero);
	}
}
