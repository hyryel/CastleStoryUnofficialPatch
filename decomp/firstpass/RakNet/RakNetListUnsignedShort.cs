using System;
using System.Runtime.InteropServices;

namespace RakNet;

public class RakNetListUnsignedShort : IDisposable
{
	private HandleRef swigCPtr;

	protected bool swigCMemOwn;

	public ushort this[int index]
	{
		get
		{
			return Get((uint)index);
		}
		set
		{
			Replace(value, value, (uint)index, "Not used", 0u);
		}
	}

	internal RakNetListUnsignedShort(IntPtr cPtr, bool cMemoryOwn)
	{
		swigCMemOwn = cMemoryOwn;
		swigCPtr = new HandleRef(this, cPtr);
	}

	public RakNetListUnsignedShort()
		: this(RakNetPINVOKE.new_RakNetListUnsignedShort__SWIG_0(), cMemoryOwn: true)
	{
	}

	public RakNetListUnsignedShort(RakNetListUnsignedShort original_copy)
		: this(RakNetPINVOKE.new_RakNetListUnsignedShort__SWIG_1(getCPtr(original_copy)), cMemoryOwn: true)
	{
		if (RakNetPINVOKE.SWIGPendingException.Pending)
		{
			throw RakNetPINVOKE.SWIGPendingException.Retrieve();
		}
	}

	internal static HandleRef getCPtr(RakNetListUnsignedShort obj)
	{
		return obj?.swigCPtr ?? new HandleRef(null, IntPtr.Zero);
	}

	~RakNetListUnsignedShort()
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
					RakNetPINVOKE.delete_RakNetListUnsignedShort(swigCPtr);
				}
				swigCPtr = new HandleRef(null, IntPtr.Zero);
			}
			GC.SuppressFinalize(this);
		}
	}

	public ushort Get(uint position)
	{
		SWIGTYPE_p_unsigned_short helper = GetHelper(position);
		return UnsignedShortPointer.frompointer(helper).value();
	}

	public ushort Pop()
	{
		SWIGTYPE_p_unsigned_short t = PopHelper();
		return UnsignedShortPointer.frompointer(t).value();
	}

	public RakNetListUnsignedShort CopyData(RakNetListUnsignedShort original_copy)
	{
		RakNetListUnsignedShort result = new RakNetListUnsignedShort(RakNetPINVOKE.RakNetListUnsignedShort_CopyData(swigCPtr, getCPtr(original_copy)), cMemoryOwn: false);
		if (RakNetPINVOKE.SWIGPendingException.Pending)
		{
			throw RakNetPINVOKE.SWIGPendingException.Retrieve();
		}
		return result;
	}

	private SWIGTYPE_p_unsigned_short GetHelper(uint position)
	{
		return new SWIGTYPE_p_unsigned_short(RakNetPINVOKE.RakNetListUnsignedShort_GetHelper(swigCPtr, position), futureUse: false);
	}

	public void Push(ushort input, string file, uint line)
	{
		RakNetPINVOKE.RakNetListUnsignedShort_Push(swigCPtr, input, file, line);
	}

	private SWIGTYPE_p_unsigned_short PopHelper()
	{
		return new SWIGTYPE_p_unsigned_short(RakNetPINVOKE.RakNetListUnsignedShort_PopHelper(swigCPtr), futureUse: false);
	}

	public void Insert(ushort input, uint position, string file, uint line)
	{
		RakNetPINVOKE.RakNetListUnsignedShort_Insert__SWIG_0(swigCPtr, input, position, file, line);
	}

	public void Insert(ushort input, string file, uint line)
	{
		RakNetPINVOKE.RakNetListUnsignedShort_Insert__SWIG_1(swigCPtr, input, file, line);
	}

	public void Replace(ushort input, ushort filler, uint position, string file, uint line)
	{
		RakNetPINVOKE.RakNetListUnsignedShort_Replace__SWIG_0(swigCPtr, input, filler, position, file, line);
	}

	public void Replace(ushort input)
	{
		RakNetPINVOKE.RakNetListUnsignedShort_Replace__SWIG_1(swigCPtr, input);
	}

	public void RemoveAtIndex(uint position)
	{
		RakNetPINVOKE.RakNetListUnsignedShort_RemoveAtIndex(swigCPtr, position);
	}

	public void RemoveAtIndexFast(uint position)
	{
		RakNetPINVOKE.RakNetListUnsignedShort_RemoveAtIndexFast(swigCPtr, position);
	}

	public void RemoveFromEnd(uint num)
	{
		RakNetPINVOKE.RakNetListUnsignedShort_RemoveFromEnd__SWIG_0(swigCPtr, num);
	}

	public void RemoveFromEnd()
	{
		RakNetPINVOKE.RakNetListUnsignedShort_RemoveFromEnd__SWIG_1(swigCPtr);
	}

	public uint GetIndexOf(ushort input)
	{
		return RakNetPINVOKE.RakNetListUnsignedShort_GetIndexOf(swigCPtr, input);
	}

	public uint Size()
	{
		return RakNetPINVOKE.RakNetListUnsignedShort_Size(swigCPtr);
	}

	public void Clear(bool doNotDeallocateSmallBlocks, string file, uint line)
	{
		RakNetPINVOKE.RakNetListUnsignedShort_Clear(swigCPtr, doNotDeallocateSmallBlocks, file, line);
	}

	public void Preallocate(uint countNeeded, string file, uint line)
	{
		RakNetPINVOKE.RakNetListUnsignedShort_Preallocate(swigCPtr, countNeeded, file, line);
	}

	public void Compress(string file, uint line)
	{
		RakNetPINVOKE.RakNetListUnsignedShort_Compress(swigCPtr, file, line);
	}
}
