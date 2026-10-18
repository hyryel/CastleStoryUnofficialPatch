using System;
using System.Runtime.InteropServices;

namespace RakNet;

public class NatPunchthroughServerDebugInterface_PacketLogger : NatPunchthroughServerDebugInterface
{
	private HandleRef swigCPtr;

	public PacketLogger pl
	{
		get
		{
			IntPtr intPtr = RakNetPINVOKE.NatPunchthroughServerDebugInterface_PacketLogger_pl_get(swigCPtr);
			return (!(intPtr == IntPtr.Zero)) ? new PacketLogger(intPtr, cMemoryOwn: false) : null;
		}
		set
		{
			RakNetPINVOKE.NatPunchthroughServerDebugInterface_PacketLogger_pl_set(swigCPtr, PacketLogger.getCPtr(value));
		}
	}

	internal NatPunchthroughServerDebugInterface_PacketLogger(IntPtr cPtr, bool cMemoryOwn)
		: base(RakNetPINVOKE.NatPunchthroughServerDebugInterface_PacketLogger_SWIGUpcast(cPtr), cMemoryOwn)
	{
		swigCPtr = new HandleRef(this, cPtr);
	}

	public NatPunchthroughServerDebugInterface_PacketLogger()
		: this(RakNetPINVOKE.new_NatPunchthroughServerDebugInterface_PacketLogger(), cMemoryOwn: true)
	{
	}

	internal static HandleRef getCPtr(NatPunchthroughServerDebugInterface_PacketLogger obj)
	{
		return obj?.swigCPtr ?? new HandleRef(null, IntPtr.Zero);
	}

	~NatPunchthroughServerDebugInterface_PacketLogger()
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
					RakNetPINVOKE.delete_NatPunchthroughServerDebugInterface_PacketLogger(swigCPtr);
				}
				swigCPtr = new HandleRef(null, IntPtr.Zero);
			}
			GC.SuppressFinalize(this);
			base.Dispose();
		}
	}

	public override void OnServerMessage(string msg)
	{
		RakNetPINVOKE.NatPunchthroughServerDebugInterface_PacketLogger_OnServerMessage(swigCPtr, msg);
	}
}
