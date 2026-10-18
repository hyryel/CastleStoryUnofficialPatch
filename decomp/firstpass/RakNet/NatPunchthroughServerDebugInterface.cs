using System;
using System.Runtime.InteropServices;

namespace RakNet;

public class NatPunchthroughServerDebugInterface : IDisposable
{
	private HandleRef swigCPtr;

	protected bool swigCMemOwn;

	internal NatPunchthroughServerDebugInterface(IntPtr cPtr, bool cMemoryOwn)
	{
		swigCMemOwn = cMemoryOwn;
		swigCPtr = new HandleRef(this, cPtr);
	}

	internal static HandleRef getCPtr(NatPunchthroughServerDebugInterface obj)
	{
		return obj?.swigCPtr ?? new HandleRef(null, IntPtr.Zero);
	}

	~NatPunchthroughServerDebugInterface()
	{
		Dispose();
	}

	public virtual void Dispose()
	{
		lock (this)
		{
			if (swigCPtr.Handle != IntPtr.Zero)
			{
				if (swigCMemOwn)
				{
					swigCMemOwn = false;
					RakNetPINVOKE.delete_NatPunchthroughServerDebugInterface(swigCPtr);
				}
				swigCPtr = new HandleRef(null, IntPtr.Zero);
			}
			GC.SuppressFinalize(this);
		}
	}

	public virtual void OnServerMessage(string msg)
	{
		RakNetPINVOKE.NatPunchthroughServerDebugInterface_OnServerMessage(swigCPtr, msg);
	}
}
