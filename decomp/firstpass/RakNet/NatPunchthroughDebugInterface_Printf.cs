using System;
using System.Runtime.InteropServices;

namespace RakNet;

public class NatPunchthroughDebugInterface_Printf : NatPunchthroughDebugInterface
{
	private HandleRef swigCPtr;

	internal NatPunchthroughDebugInterface_Printf(IntPtr cPtr, bool cMemoryOwn)
		: base(RakNetPINVOKE.NatPunchthroughDebugInterface_Printf_SWIGUpcast(cPtr), cMemoryOwn)
	{
		swigCPtr = new HandleRef(this, cPtr);
	}

	public NatPunchthroughDebugInterface_Printf()
		: this(RakNetPINVOKE.new_NatPunchthroughDebugInterface_Printf(), cMemoryOwn: true)
	{
	}

	internal static HandleRef getCPtr(NatPunchthroughDebugInterface_Printf obj)
	{
		return obj?.swigCPtr ?? new HandleRef(null, IntPtr.Zero);
	}

	~NatPunchthroughDebugInterface_Printf()
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
					RakNetPINVOKE.delete_NatPunchthroughDebugInterface_Printf(swigCPtr);
				}
				swigCPtr = new HandleRef(null, IntPtr.Zero);
			}
			GC.SuppressFinalize(this);
			base.Dispose();
		}
	}

	public override void OnClientMessage(string msg)
	{
		RakNetPINVOKE.NatPunchthroughDebugInterface_Printf_OnClientMessage(swigCPtr, msg);
	}
}
