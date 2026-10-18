using System;
using System.Runtime.InteropServices;

namespace RakNet;

public class RakNetListRakString : IDisposable
{
	private HandleRef swigCPtr;

	protected bool swigCMemOwn;

	public RakString this[int index]
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

	internal RakNetListRakString(IntPtr cPtr, bool cMemoryOwn)
	{
		swigCMemOwn = cMemoryOwn;
		swigCPtr = new HandleRef(this, cPtr);
	}

	public RakNetListRakString()
		: this(RakNetPINVOKE.new_RakNetListRakString__SWIG_0(), cMemoryOwn: true)
	{
	}

	public RakNetListRakString(RakNetListRakString original_copy)
		: this(RakNetPINVOKE.new_RakNetListRakString__SWIG_1(getCPtr(original_copy)), cMemoryOwn: true)
	{
		if (RakNetPINVOKE.SWIGPendingException.Pending)
		{
			throw RakNetPINVOKE.SWIGPendingException.Retrieve();
		}
	}

	internal static HandleRef getCPtr(RakNetListRakString obj)
	{
		return obj?.swigCPtr ?? new HandleRef(null, IntPtr.Zero);
	}

	~RakNetListRakString()
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
					RakNetPINVOKE.delete_RakNetListRakString(swigCPtr);
				}
				swigCPtr = new HandleRef(null, IntPtr.Zero);
			}
			GC.SuppressFinalize(this);
		}
	}

	public RakNetListRakString CopyData(RakNetListRakString original_copy)
	{
		RakNetListRakString result = new RakNetListRakString(RakNetPINVOKE.RakNetListRakString_CopyData(swigCPtr, getCPtr(original_copy)), cMemoryOwn: false);
		if (RakNetPINVOKE.SWIGPendingException.Pending)
		{
			throw RakNetPINVOKE.SWIGPendingException.Retrieve();
		}
		return result;
	}

	public RakString Get(uint position)
	{
		return new RakString(RakNetPINVOKE.RakNetListRakString_Get(swigCPtr, position), cMemoryOwn: false);
	}

	public void Push(RakString input, string file, uint line)
	{
		RakNetPINVOKE.RakNetListRakString_Push(swigCPtr, RakString.getCPtr(input), file, line);
		if (RakNetPINVOKE.SWIGPendingException.Pending)
		{
			throw RakNetPINVOKE.SWIGPendingException.Retrieve();
		}
	}

	public RakString Pop()
	{
		return new RakString(RakNetPINVOKE.RakNetListRakString_Pop(swigCPtr), cMemoryOwn: false);
	}

	public void Insert(RakString input, uint position, string file, uint line)
	{
		RakNetPINVOKE.RakNetListRakString_Insert__SWIG_0(swigCPtr, RakString.getCPtr(input), position, file, line);
		if (RakNetPINVOKE.SWIGPendingException.Pending)
		{
			throw RakNetPINVOKE.SWIGPendingException.Retrieve();
		}
	}

	public void Insert(RakString input, string file, uint line)
	{
		RakNetPINVOKE.RakNetListRakString_Insert__SWIG_1(swigCPtr, RakString.getCPtr(input), file, line);
		if (RakNetPINVOKE.SWIGPendingException.Pending)
		{
			throw RakNetPINVOKE.SWIGPendingException.Retrieve();
		}
	}

	public void Replace(RakString input, RakString filler, uint position, string file, uint line)
	{
		RakNetPINVOKE.RakNetListRakString_Replace__SWIG_0(swigCPtr, RakString.getCPtr(input), RakString.getCPtr(filler), position, file, line);
		if (RakNetPINVOKE.SWIGPendingException.Pending)
		{
			throw RakNetPINVOKE.SWIGPendingException.Retrieve();
		}
	}

	public void Replace(RakString input)
	{
		RakNetPINVOKE.RakNetListRakString_Replace__SWIG_1(swigCPtr, RakString.getCPtr(input));
		if (RakNetPINVOKE.SWIGPendingException.Pending)
		{
			throw RakNetPINVOKE.SWIGPendingException.Retrieve();
		}
	}

	public void RemoveAtIndex(uint position)
	{
		RakNetPINVOKE.RakNetListRakString_RemoveAtIndex(swigCPtr, position);
	}

	public void RemoveAtIndexFast(uint position)
	{
		RakNetPINVOKE.RakNetListRakString_RemoveAtIndexFast(swigCPtr, position);
	}

	public void RemoveFromEnd(uint num)
	{
		RakNetPINVOKE.RakNetListRakString_RemoveFromEnd__SWIG_0(swigCPtr, num);
	}

	public void RemoveFromEnd()
	{
		RakNetPINVOKE.RakNetListRakString_RemoveFromEnd__SWIG_1(swigCPtr);
	}

	public uint GetIndexOf(RakString input)
	{
		uint result = RakNetPINVOKE.RakNetListRakString_GetIndexOf(swigCPtr, RakString.getCPtr(input));
		if (RakNetPINVOKE.SWIGPendingException.Pending)
		{
			throw RakNetPINVOKE.SWIGPendingException.Retrieve();
		}
		return result;
	}

	public uint Size()
	{
		return RakNetPINVOKE.RakNetListRakString_Size(swigCPtr);
	}

	public void Clear(bool doNotDeallocateSmallBlocks, string file, uint line)
	{
		RakNetPINVOKE.RakNetListRakString_Clear(swigCPtr, doNotDeallocateSmallBlocks, file, line);
	}

	public void Preallocate(uint countNeeded, string file, uint line)
	{
		RakNetPINVOKE.RakNetListRakString_Preallocate(swigCPtr, countNeeded, file, line);
	}

	public void Compress(string file, uint line)
	{
		RakNetPINVOKE.RakNetListRakString_Compress(swigCPtr, file, line);
	}
}
