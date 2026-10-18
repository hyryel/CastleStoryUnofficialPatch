using System;
using System.Runtime.InteropServices;

namespace RakNet;

public class SWIGTYPE_p_RakNet__TCPInterface
{
	private HandleRef swigCPtr;

	internal SWIGTYPE_p_RakNet__TCPInterface(IntPtr cPtr, bool futureUse)
	{
		swigCPtr = new HandleRef(this, cPtr);
	}

	protected SWIGTYPE_p_RakNet__TCPInterface()
	{
		swigCPtr = new HandleRef(null, IntPtr.Zero);
	}

	internal static HandleRef getCPtr(SWIGTYPE_p_RakNet__TCPInterface obj)
	{
		return obj?.swigCPtr ?? new HandleRef(null, IntPtr.Zero);
	}
}
