using System;
using System.Runtime.InteropServices;

namespace RakNet;

public class SWIGTYPE_p_RNS2RecvStruct
{
	private HandleRef swigCPtr;

	internal SWIGTYPE_p_RNS2RecvStruct(IntPtr cPtr, bool futureUse)
	{
		swigCPtr = new HandleRef(this, cPtr);
	}

	protected SWIGTYPE_p_RNS2RecvStruct()
	{
		swigCPtr = new HandleRef(null, IntPtr.Zero);
	}

	internal static HandleRef getCPtr(SWIGTYPE_p_RNS2RecvStruct obj)
	{
		return obj?.swigCPtr ?? new HandleRef(null, IntPtr.Zero);
	}
}
