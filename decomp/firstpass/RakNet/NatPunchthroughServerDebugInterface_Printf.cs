using System;
using System.Runtime.InteropServices;

namespace RakNet;

public class NatPunchthroughServerDebugInterface_Printf : NatPunchthroughServerDebugInterface
{
	private HandleRef swigCPtr;

	internal NatPunchthroughServerDebugInterface_Printf(IntPtr cPtr, bool cMemoryOwn)
		: base(RakNetPINVOKE.NatPunchthroughServerDebugInterface_Printf_SWIGUpcast(cPtr), cMemoryOwn)
	{
		swigCPtr = new HandleRef(this, cPtr);
	}

	public NatPunchthroughServerDebugInterface_Printf()
		: this(RakNetPINVOKE.new_NatPunchthroughServerDebugInterface_Printf(), cMemoryOwn: true)
	{
	}

	internal static HandleRef getCPtr(NatPunchthroughServerDebugInterface_Printf obj)
	{
		return obj?.swigCPtr ?? new HandleRef(null, IntPtr.Zero);
	}

	~NatPunchthroughServerDebugInterface_Printf()
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
					RakNetPINVOKE.delete_NatPunchthroughServerDebugInterface_Printf(swigCPtr);
				}
				swigCPtr = new HandleRef(null, IntPtr.Zero);
			}
			GC.SuppressFinalize(this);
			base.Dispose();
		}
	}

	public override void OnServerMessage(string msg)
	{
		RakNetPINVOKE.NatPunchthroughServerDebugInterface_Printf_OnServerMessage(swigCPtr, msg);
	}
}
