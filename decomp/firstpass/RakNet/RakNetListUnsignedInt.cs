using System;
using System.Runtime.InteropServices;

namespace RakNet;

public class RakNetListUnsignedInt : IDisposable
{
	private HandleRef swigCPtr;

	protected bool swigCMemOwn;

	public uint this[int index]
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

	internal RakNetListUnsignedInt(IntPtr cPtr, bool cMemoryOwn)
	{
		swigCMemOwn = cMemoryOwn;
		swigCPtr = new HandleRef(this, cPtr);
	}

	public RakNetListUnsignedInt()
		: this(RakNetPINVOKE.new_RakNetListUnsignedInt__SWIG_0(), cMemoryOwn: true)
	{
	}

	public RakNetListUnsignedInt(RakNetListUnsignedInt original_copy)
		: this(RakNetPINVOKE.new_RakNetListUnsignedInt__SWIG_1(getCPtr(original_copy)), cMemoryOwn: true)
	{
		if (RakNetPINVOKE.SWIGPendingException.Pending)
		{
			throw RakNetPINVOKE.SWIGPendingException.Retrieve();
		}
	}

	internal static HandleRef getCPtr(RakNetListUnsignedInt obj)
	{
		return obj?.swigCPtr ?? new HandleRef(null, IntPtr.Zero);
	}

	~RakNetListUnsignedInt()
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
					RakNetPINVOKE.delete_RakNetListUnsignedInt(swigCPtr);
				}
				swigCPtr = new HandleRef(null, IntPtr.Zero);
			}
			GC.SuppressFinalize(this);
		}
	}

	public uint Get(uint position)
	{
		SWIGTYPE_p_unsigned_int helper = GetHelper(position);
		return UnsignedIntPointer.frompointer(helper).value();
	}

	public uint Pop()
	{
		SWIGTYPE_p_unsigned_int t = PopHelper();
		return UnsignedIntPointer.frompointer(t).value();
	}

	public RakNetListUnsignedInt CopyData(RakNetListUnsignedInt original_copy)
	{
		RakNetListUnsignedInt result = new RakNetListUnsignedInt(RakNetPINVOKE.RakNetListUnsignedInt_CopyData(swigCPtr, getCPtr(original_copy)), cMemoryOwn: false);
		if (RakNetPINVOKE.SWIGPendingException.Pending)
		{
			throw RakNetPINVOKE.SWIGPendingException.Retrieve();
		}
		return result;
	}

	private SWIGTYPE_p_unsigned_int GetHelper(uint position)
	{
		return new SWIGTYPE_p_unsigned_int(RakNetPINVOKE.RakNetListUnsignedInt_GetHelper(swigCPtr, position), futureUse: false);
	}

	public void Push(uint input, string file, uint line)
	{
		RakNetPINVOKE.RakNetListUnsignedInt_Push(swigCPtr, input, file, line);
	}

	private SWIGTYPE_p_unsigned_int PopHelper()
	{
		return new SWIGTYPE_p_unsigned_int(RakNetPINVOKE.RakNetListUnsignedInt_PopHelper(swigCPtr), futureUse: false);
	}

	public void Insert(uint input, uint position, string file, uint line)
	{
		RakNetPINVOKE.RakNetListUnsignedInt_Insert__SWIG_0(swigCPtr, input, position, file, line);
	}

	public void Insert(uint input, string file, uint line)
	{
		RakNetPINVOKE.RakNetListUnsignedInt_Insert__SWIG_1(swigCPtr, input, file, line);
	}

	public void Replace(uint input, uint filler, uint position, string file, uint line)
	{
		RakNetPINVOKE.RakNetListUnsignedInt_Replace__SWIG_0(swigCPtr, input, filler, position, file, line);
	}

	public void Replace(uint input)
	{
		RakNetPINVOKE.RakNetListUnsignedInt_Replace__SWIG_1(swigCPtr, input);
	}

	public void RemoveAtIndex(uint position)
	{
		RakNetPINVOKE.RakNetListUnsignedInt_RemoveAtIndex(swigCPtr, position);
	}

	public void RemoveAtIndexFast(uint position)
	{
		RakNetPINVOKE.RakNetListUnsignedInt_RemoveAtIndexFast(swigCPtr, position);
	}

	public void RemoveFromEnd(uint num)
	{
		RakNetPINVOKE.RakNetListUnsignedInt_RemoveFromEnd__SWIG_0(swigCPtr, num);
	}

	public void RemoveFromEnd()
	{
		RakNetPINVOKE.RakNetListUnsignedInt_RemoveFromEnd__SWIG_1(swigCPtr);
	}

	public uint GetIndexOf(uint input)
	{
		return RakNetPINVOKE.RakNetListUnsignedInt_GetIndexOf(swigCPtr, input);
	}

	public uint Size()
	{
		return RakNetPINVOKE.RakNetListUnsignedInt_Size(swigCPtr);
	}

	public void Clear(bool doNotDeallocateSmallBlocks, string file, uint line)
	{
		RakNetPINVOKE.RakNetListUnsignedInt_Clear(swigCPtr, doNotDeallocateSmallBlocks, file, line);
	}

	public void Preallocate(uint countNeeded, string file, uint line)
	{
		RakNetPINVOKE.RakNetListUnsignedInt_Preallocate(swigCPtr, countNeeded, file, line);
	}

	public void Compress(string file, uint line)
	{
		RakNetPINVOKE.RakNetListUnsignedInt_Compress(swigCPtr, file, line);
	}
}
