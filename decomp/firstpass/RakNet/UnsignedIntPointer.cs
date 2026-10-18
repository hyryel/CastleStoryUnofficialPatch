using System;
using System.Runtime.InteropServices;

namespace RakNet;

public class UnsignedIntPointer : IDisposable
{
	private HandleRef swigCPtr;

	protected bool swigCMemOwn;

	internal UnsignedIntPointer(IntPtr cPtr, bool cMemoryOwn)
	{
		swigCMemOwn = cMemoryOwn;
		swigCPtr = new HandleRef(this, cPtr);
	}

	public UnsignedIntPointer()
		: this(RakNetPINVOKE.new_UnsignedIntPointer(), cMemoryOwn: true)
	{
	}

	internal static HandleRef getCPtr(UnsignedIntPointer obj)
	{
		return obj?.swigCPtr ?? new HandleRef(null, IntPtr.Zero);
	}

	~UnsignedIntPointer()
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
					RakNetPINVOKE.delete_UnsignedIntPointer(swigCPtr);
				}
				swigCPtr = new HandleRef(null, IntPtr.Zero);
			}
			GC.SuppressFinalize(this);
		}
	}

	public void assign(uint value)
	{
		RakNetPINVOKE.UnsignedIntPointer_assign(swigCPtr, value);
	}

	public uint value()
	{
		return RakNetPINVOKE.UnsignedIntPointer_value(swigCPtr);
	}

	public SWIGTYPE_p_unsigned_int cast()
	{
		IntPtr intPtr = RakNetPINVOKE.UnsignedIntPointer_cast(swigCPtr);
		return (!(intPtr == IntPtr.Zero)) ? new SWIGTYPE_p_unsigned_int(intPtr, futureUse: false) : null;
	}

	public static UnsignedIntPointer frompointer(SWIGTYPE_p_unsigned_int t)
	{
		IntPtr intPtr = RakNetPINVOKE.UnsignedIntPointer_frompointer(SWIGTYPE_p_unsigned_int.getCPtr(t));
		return (!(intPtr == IntPtr.Zero)) ? new UnsignedIntPointer(intPtr, cMemoryOwn: false) : null;
	}
}
