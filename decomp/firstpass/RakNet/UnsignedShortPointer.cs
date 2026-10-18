using System;
using System.Runtime.InteropServices;

namespace RakNet;

public class UnsignedShortPointer : IDisposable
{
	private HandleRef swigCPtr;

	protected bool swigCMemOwn;

	internal UnsignedShortPointer(IntPtr cPtr, bool cMemoryOwn)
	{
		swigCMemOwn = cMemoryOwn;
		swigCPtr = new HandleRef(this, cPtr);
	}

	public UnsignedShortPointer()
		: this(RakNetPINVOKE.new_UnsignedShortPointer(), cMemoryOwn: true)
	{
	}

	internal static HandleRef getCPtr(UnsignedShortPointer obj)
	{
		return obj?.swigCPtr ?? new HandleRef(null, IntPtr.Zero);
	}

	~UnsignedShortPointer()
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
					RakNetPINVOKE.delete_UnsignedShortPointer(swigCPtr);
				}
				swigCPtr = new HandleRef(null, IntPtr.Zero);
			}
			GC.SuppressFinalize(this);
		}
	}

	public void assign(ushort value)
	{
		RakNetPINVOKE.UnsignedShortPointer_assign(swigCPtr, value);
	}

	public ushort value()
	{
		return RakNetPINVOKE.UnsignedShortPointer_value(swigCPtr);
	}

	public SWIGTYPE_p_unsigned_short cast()
	{
		IntPtr intPtr = RakNetPINVOKE.UnsignedShortPointer_cast(swigCPtr);
		return (!(intPtr == IntPtr.Zero)) ? new SWIGTYPE_p_unsigned_short(intPtr, futureUse: false) : null;
	}

	public static UnsignedShortPointer frompointer(SWIGTYPE_p_unsigned_short t)
	{
		IntPtr intPtr = RakNetPINVOKE.UnsignedShortPointer_frompointer(SWIGTYPE_p_unsigned_short.getCPtr(t));
		return (!(intPtr == IntPtr.Zero)) ? new UnsignedShortPointer(intPtr, cMemoryOwn: false) : null;
	}
}
