using System;
using System.Runtime.InteropServices;

namespace RakNet;

public class NatPunchthroughDebugInterface_PacketLogger : NatPunchthroughDebugInterface
{
	private HandleRef swigCPtr;

	public PacketLogger pl
	{
		get
		{
			IntPtr intPtr = RakNetPINVOKE.NatPunchthroughDebugInterface_PacketLogger_pl_get(swigCPtr);
			return (!(intPtr == IntPtr.Zero)) ? new PacketLogger(intPtr, cMemoryOwn: false) : null;
		}
		set
		{
			RakNetPINVOKE.NatPunchthroughDebugInterface_PacketLogger_pl_set(swigCPtr, PacketLogger.getCPtr(value));
		}
	}

	internal NatPunchthroughDebugInterface_PacketLogger(IntPtr cPtr, bool cMemoryOwn)
		: base(RakNetPINVOKE.NatPunchthroughDebugInterface_PacketLogger_SWIGUpcast(cPtr), cMemoryOwn)
	{
		swigCPtr = new HandleRef(this, cPtr);
	}

	public NatPunchthroughDebugInterface_PacketLogger()
		: this(RakNetPINVOKE.new_NatPunchthroughDebugInterface_PacketLogger(), cMemoryOwn: true)
	{
	}

	internal static HandleRef getCPtr(NatPunchthroughDebugInterface_PacketLogger obj)
	{
		return obj?.swigCPtr ?? new HandleRef(null, IntPtr.Zero);
	}

	~NatPunchthroughDebugInterface_PacketLogger()
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
					RakNetPINVOKE.delete_NatPunchthroughDebugInterface_PacketLogger(swigCPtr);
				}
				swigCPtr = new HandleRef(null, IntPtr.Zero);
			}
			GC.SuppressFinalize(this);
			base.Dispose();
		}
	}

	public override void OnClientMessage(string msg)
	{
		RakNetPINVOKE.NatPunchthroughDebugInterface_PacketLogger_OnClientMessage(swigCPtr, msg);
	}
}
