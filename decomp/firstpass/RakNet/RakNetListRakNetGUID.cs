using System;
using System.Runtime.InteropServices;

namespace RakNet;

public class RakNetListRakNetGUID : IDisposable
{
	private HandleRef swigCPtr;

	protected bool swigCMemOwn;

	public RakNetGUID this[int index]
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

	internal RakNetListRakNetGUID(IntPtr cPtr, bool cMemoryOwn)
	{
		swigCMemOwn = cMemoryOwn;
		swigCPtr = new HandleRef(this, cPtr);
	}

	public RakNetListRakNetGUID()
		: this(RakNetPINVOKE.new_RakNetListRakNetGUID__SWIG_0(), cMemoryOwn: true)
	{
	}

	public RakNetListRakNetGUID(RakNetListRakNetGUID original_copy)
		: this(RakNetPINVOKE.new_RakNetListRakNetGUID__SWIG_1(getCPtr(original_copy)), cMemoryOwn: true)
	{
		if (RakNetPINVOKE.SWIGPendingException.Pending)
		{
			throw RakNetPINVOKE.SWIGPendingException.Retrieve();
		}
	}

	internal static HandleRef getCPtr(RakNetListRakNetGUID obj)
	{
		return obj?.swigCPtr ?? new HandleRef(null, IntPtr.Zero);
	}

	~RakNetListRakNetGUID()
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
					RakNetPINVOKE.delete_RakNetListRakNetGUID(swigCPtr);
				}
				swigCPtr = new HandleRef(null, IntPtr.Zero);
			}
			GC.SuppressFinalize(this);
		}
	}

	public RakNetListRakNetGUID CopyData(RakNetListRakNetGUID original_copy)
	{
		RakNetListRakNetGUID result = new RakNetListRakNetGUID(RakNetPINVOKE.RakNetListRakNetGUID_CopyData(swigCPtr, getCPtr(original_copy)), cMemoryOwn: false);
		if (RakNetPINVOKE.SWIGPendingException.Pending)
		{
			throw RakNetPINVOKE.SWIGPendingException.Retrieve();
		}
		return result;
	}

	public RakNetGUID Get(uint position)
	{
		return new RakNetGUID(RakNetPINVOKE.RakNetListRakNetGUID_Get(swigCPtr, position), cMemoryOwn: false);
	}

	public void Push(RakNetGUID input, string file, uint line)
	{
		RakNetPINVOKE.RakNetListRakNetGUID_Push(swigCPtr, RakNetGUID.getCPtr(input), file, line);
		if (RakNetPINVOKE.SWIGPendingException.Pending)
		{
			throw RakNetPINVOKE.SWIGPendingException.Retrieve();
		}
	}

	public RakNetGUID Pop()
	{
		return new RakNetGUID(RakNetPINVOKE.RakNetListRakNetGUID_Pop(swigCPtr), cMemoryOwn: false);
	}

	public void Insert(RakNetGUID input, uint position, string file, uint line)
	{
		RakNetPINVOKE.RakNetListRakNetGUID_Insert__SWIG_0(swigCPtr, RakNetGUID.getCPtr(input), position, file, line);
		if (RakNetPINVOKE.SWIGPendingException.Pending)
		{
			throw RakNetPINVOKE.SWIGPendingException.Retrieve();
		}
	}

	public void Insert(RakNetGUID input, string file, uint line)
	{
		RakNetPINVOKE.RakNetListRakNetGUID_Insert__SWIG_1(swigCPtr, RakNetGUID.getCPtr(input), file, line);
		if (RakNetPINVOKE.SWIGPendingException.Pending)
		{
			throw RakNetPINVOKE.SWIGPendingException.Retrieve();
		}
	}

	public void Replace(RakNetGUID input, RakNetGUID filler, uint position, string file, uint line)
	{
		RakNetPINVOKE.RakNetListRakNetGUID_Replace__SWIG_0(swigCPtr, RakNetGUID.getCPtr(input), RakNetGUID.getCPtr(filler), position, file, line);
		if (RakNetPINVOKE.SWIGPendingException.Pending)
		{
			throw RakNetPINVOKE.SWIGPendingException.Retrieve();
		}
	}

	public void Replace(RakNetGUID input)
	{
		RakNetPINVOKE.RakNetListRakNetGUID_Replace__SWIG_1(swigCPtr, RakNetGUID.getCPtr(input));
		if (RakNetPINVOKE.SWIGPendingException.Pending)
		{
			throw RakNetPINVOKE.SWIGPendingException.Retrieve();
		}
	}

	public void RemoveAtIndex(uint position)
	{
		RakNetPINVOKE.RakNetListRakNetGUID_RemoveAtIndex(swigCPtr, position);
	}

	public void RemoveAtIndexFast(uint position)
	{
		RakNetPINVOKE.RakNetListRakNetGUID_RemoveAtIndexFast(swigCPtr, position);
	}

	public void RemoveFromEnd(uint num)
	{
		RakNetPINVOKE.RakNetListRakNetGUID_RemoveFromEnd__SWIG_0(swigCPtr, num);
	}

	public void RemoveFromEnd()
	{
		RakNetPINVOKE.RakNetListRakNetGUID_RemoveFromEnd__SWIG_1(swigCPtr);
	}

	public uint GetIndexOf(RakNetGUID input)
	{
		uint result = RakNetPINVOKE.RakNetListRakNetGUID_GetIndexOf(swigCPtr, RakNetGUID.getCPtr(input));
		if (RakNetPINVOKE.SWIGPendingException.Pending)
		{
			throw RakNetPINVOKE.SWIGPendingException.Retrieve();
		}
		return result;
	}

	public uint Size()
	{
		return RakNetPINVOKE.RakNetListRakNetGUID_Size(swigCPtr);
	}

	public void Clear(bool doNotDeallocateSmallBlocks, string file, uint line)
	{
		RakNetPINVOKE.RakNetListRakNetGUID_Clear(swigCPtr, doNotDeallocateSmallBlocks, file, line);
	}

	public void Preallocate(uint countNeeded, string file, uint line)
	{
		RakNetPINVOKE.RakNetListRakNetGUID_Preallocate(swigCPtr, countNeeded, file, line);
	}

	public void Compress(string file, uint line)
	{
		RakNetPINVOKE.RakNetListRakNetGUID_Compress(swigCPtr, file, line);
	}
}
