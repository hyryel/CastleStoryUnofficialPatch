using System;
using System.Runtime.InteropServices;

namespace RakNet;

public class SWIGTYPE_p_unsigned_short
{
	private HandleRef swigCPtr;

	internal SWIGTYPE_p_unsigned_short(IntPtr cPtr, bool futureUse)
	{
		swigCPtr = new HandleRef(this, cPtr);
	}

	protected SWIGTYPE_p_unsigned_short()
	{
		swigCPtr = new HandleRef(null, IntPtr.Zero);
	}

	internal static HandleRef getCPtr(SWIGTYPE_p_unsigned_short obj)
	{
		return obj?.swigCPtr ?? new HandleRef(null, IntPtr.Zero);
	}
}
