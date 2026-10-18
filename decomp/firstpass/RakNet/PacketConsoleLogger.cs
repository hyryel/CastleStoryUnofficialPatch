using System;
using System.Runtime.InteropServices;

namespace RakNet;

public class PacketConsoleLogger : PacketLogger
{
	private HandleRef swigCPtr;

	internal PacketConsoleLogger(IntPtr cPtr, bool cMemoryOwn)
		: base(RakNetPINVOKE.PacketConsoleLogger_SWIGUpcast(cPtr), cMemoryOwn)
	{
		swigCPtr = new HandleRef(this, cPtr);
	}

	public PacketConsoleLogger()
		: this(RakNetPINVOKE.new_PacketConsoleLogger(), cMemoryOwn: true)
	{
	}

	internal static HandleRef getCPtr(PacketConsoleLogger obj)
	{
		return obj?.swigCPtr ?? new HandleRef(null, IntPtr.Zero);
	}

	~PacketConsoleLogger()
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
					RakNetPINVOKE.delete_PacketConsoleLogger(swigCPtr);
				}
				swigCPtr = new HandleRef(null, IntPtr.Zero);
			}
			GC.SuppressFinalize(this);
			base.Dispose();
		}
	}

	public virtual void SetLogCommandParser(LogCommandParser lcp)
	{
		RakNetPINVOKE.PacketConsoleLogger_SetLogCommandParser(swigCPtr, LogCommandParser.getCPtr(lcp));
	}

	public override void WriteLog(string str)
	{
		RakNetPINVOKE.PacketConsoleLogger_WriteLog(swigCPtr, str);
	}
}
