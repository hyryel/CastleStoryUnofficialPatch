using System;
using System.Runtime.InteropServices;

namespace RakNet;

public class SWIGTYPE_p_RakNet__RakNetSocket2
{
	private HandleRef swigCPtr;

	internal SWIGTYPE_p_RakNet__RakNetSocket2(IntPtr cPtr, bool futureUse)
	{
		swigCPtr = new HandleRef(this, cPtr);
	}

	protected SWIGTYPE_p_RakNet__RakNetSocket2()
	{
		swigCPtr = new HandleRef(null, IntPtr.Zero);
	}

	internal static HandleRef getCPtr(SWIGTYPE_p_RakNet__RakNetSocket2 obj)
	{
		return obj?.swigCPtr ?? new HandleRef(null, IntPtr.Zero);
	}
}
