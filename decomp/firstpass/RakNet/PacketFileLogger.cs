using System;
using System.Runtime.InteropServices;

namespace RakNet;

public class PacketFileLogger : PacketLogger
{
	private HandleRef swigCPtr;

	internal PacketFileLogger(IntPtr cPtr, bool cMemoryOwn)
		: base(RakNetPINVOKE.PacketFileLogger_SWIGUpcast(cPtr), cMemoryOwn)
	{
		swigCPtr = new HandleRef(this, cPtr);
	}

	public PacketFileLogger()
		: this(RakNetPINVOKE.new_PacketFileLogger(), cMemoryOwn: true)
	{
	}

	internal static HandleRef getCPtr(PacketFileLogger obj)
	{
		return obj?.swigCPtr ?? new HandleRef(null, IntPtr.Zero);
	}

	~PacketFileLogger()
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
					RakNetPINVOKE.delete_PacketFileLogger(swigCPtr);
				}
				swigCPtr = new HandleRef(null, IntPtr.Zero);
			}
			GC.SuppressFinalize(this);
			base.Dispose();
		}
	}

	public void StartLog(string filenamePrefix)
	{
		RakNetPINVOKE.PacketFileLogger_StartLog(swigCPtr, filenamePrefix);
	}

	public override void WriteLog(string str)
	{
		RakNetPINVOKE.PacketFileLogger_WriteLog(swigCPtr, str);
	}
}
