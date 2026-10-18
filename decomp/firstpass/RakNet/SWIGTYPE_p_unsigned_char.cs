using System;
using System.Runtime.InteropServices;

namespace RakNet;

public class SWIGTYPE_p_unsigned_char
{
	private HandleRef swigCPtr;

	internal SWIGTYPE_p_unsigned_char(IntPtr cPtr, bool futureUse)
	{
		swigCPtr = new HandleRef(this, cPtr);
	}

	protected SWIGTYPE_p_unsigned_char()
	{
		swigCPtr = new HandleRef(null, IntPtr.Zero);
	}

	internal static HandleRef getCPtr(SWIGTYPE_p_unsigned_char obj)
	{
		return obj?.swigCPtr ?? new HandleRef(null, IntPtr.Zero);
	}
}
