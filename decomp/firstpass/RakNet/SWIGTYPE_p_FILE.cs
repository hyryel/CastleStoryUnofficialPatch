using System;
using System.Runtime.InteropServices;

namespace RakNet;

public class SWIGTYPE_p_FILE
{
	private HandleRef swigCPtr;

	internal SWIGTYPE_p_FILE(IntPtr cPtr, bool futureUse)
	{
		swigCPtr = new HandleRef(this, cPtr);
	}

	protected SWIGTYPE_p_FILE()
	{
		swigCPtr = new HandleRef(null, IntPtr.Zero);
	}

	internal static HandleRef getCPtr(SWIGTYPE_p_FILE obj)
	{
		return obj?.swigCPtr ?? new HandleRef(null, IntPtr.Zero);
	}
}
