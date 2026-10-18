using System;
using System.Runtime.InteropServices;

namespace RakNet;

public class SWIGTYPE_p_unsigned_int
{
	private HandleRef swigCPtr;

	internal SWIGTYPE_p_unsigned_int(IntPtr cPtr, bool futureUse)
	{
		swigCPtr = new HandleRef(this, cPtr);
	}

	protected SWIGTYPE_p_unsigned_int()
	{
		swigCPtr = new HandleRef(null, IntPtr.Zero);
	}

	internal static HandleRef getCPtr(SWIGTYPE_p_unsigned_int obj)
	{
		return obj?.swigCPtr ?? new HandleRef(null, IntPtr.Zero);
	}
}
