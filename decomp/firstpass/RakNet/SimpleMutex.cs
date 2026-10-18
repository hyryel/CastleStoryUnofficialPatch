using System;
using System.Runtime.InteropServices;

namespace RakNet;

public class SimpleMutex : IDisposable
{
	private HandleRef swigCPtr;

	protected bool swigCMemOwn;

	internal SimpleMutex(IntPtr cPtr, bool cMemoryOwn)
	{
		swigCMemOwn = cMemoryOwn;
		swigCPtr = new HandleRef(this, cPtr);
	}

	public SimpleMutex()
		: this(RakNetPINVOKE.new_SimpleMutex(), cMemoryOwn: true)
	{
	}

	internal static HandleRef getCPtr(SimpleMutex obj)
	{
		return obj?.swigCPtr ?? new HandleRef(null, IntPtr.Zero);
	}

	~SimpleMutex()
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
					RakNetPINVOKE.delete_SimpleMutex(swigCPtr);
				}
				swigCPtr = new HandleRef(null, IntPtr.Zero);
			}
			GC.SuppressFinalize(this);
		}
	}

	public void Lock()
	{
		RakNetPINVOKE.SimpleMutex_Lock(swigCPtr);
	}

	public void Unlock()
	{
		RakNetPINVOKE.SimpleMutex_Unlock(swigCPtr);
	}
}
