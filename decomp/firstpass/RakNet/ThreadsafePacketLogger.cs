using System;
using System.Runtime.InteropServices;

namespace RakNet;

public class ThreadsafePacketLogger : PacketLogger
{
	private HandleRef swigCPtr;

	internal ThreadsafePacketLogger(IntPtr cPtr, bool cMemoryOwn)
		: base(RakNetPINVOKE.ThreadsafePacketLogger_SWIGUpcast(cPtr), cMemoryOwn)
	{
		swigCPtr = new HandleRef(this, cPtr);
	}

	public ThreadsafePacketLogger()
		: this(RakNetPINVOKE.new_ThreadsafePacketLogger(), cMemoryOwn: true)
	{
	}

	internal static HandleRef getCPtr(ThreadsafePacketLogger obj)
	{
		return obj?.swigCPtr ?? new HandleRef(null, IntPtr.Zero);
	}

	~ThreadsafePacketLogger()
	{
		Dispose();
	}

	public override void Dispose()
	{
		lock (this)
		{
			if (swigCPtr.Handle != IntPtr.Zero)
			{
				if (swigCMemOwn)
				{
					swigCMemOwn = false;
					RakNetPINVOKE.delete_ThreadsafePacketLogger(swigCPtr);
				}
				swigCPtr = new HandleRef(null, IntPtr.Zero);
			}
			GC.SuppressFinalize(this);
			base.Dispose();
		}
	}
}
