using System;
using System.Runtime.InteropServices;

namespace RakNet;

public class Router2DebugInterface : IDisposable
{
	private HandleRef swigCPtr;

	protected bool swigCMemOwn;

	internal Router2DebugInterface(IntPtr cPtr, bool cMemoryOwn)
	{
		swigCMemOwn = cMemoryOwn;
		swigCPtr = new HandleRef(this, cPtr);
	}

	public Router2DebugInterface()
		: this(RakNetPINVOKE.new_Router2DebugInterface(), cMemoryOwn: true)
	{
	}

	internal static HandleRef getCPtr(Router2DebugInterface obj)
	{
		return obj?.swigCPtr ?? new HandleRef(null, IntPtr.Zero);
	}

	~Router2DebugInterface()
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
					RakNetPINVOKE.delete_Router2DebugInterface(swigCPtr);
				}
				swigCPtr = new HandleRef(null, IntPtr.Zero);
			}
			GC.SuppressFinalize(this);
		}
	}

	public virtual void ShowFailure(string message)
	{
		RakNetPINVOKE.Router2DebugInterface_ShowFailure(swigCPtr, message);
	}

	public virtual void ShowDiagnostic(string message)
	{
		RakNetPINVOKE.Router2DebugInterface_ShowDiagnostic(swigCPtr, message);
	}
}
