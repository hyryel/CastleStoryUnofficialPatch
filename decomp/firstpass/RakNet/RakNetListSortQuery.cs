using System;
using System.Runtime.InteropServices;

namespace RakNet;

public class RakNetListSortQuery : IDisposable
{
	private HandleRef swigCPtr;

	protected bool swigCMemOwn;

	public SortQuery this[int index]
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

	internal RakNetListSortQuery(IntPtr cPtr, bool cMemoryOwn)
	{
		swigCMemOwn = cMemoryOwn;
		swigCPtr = new HandleRef(this, cPtr);
	}

	public RakNetListSortQuery()
		: this(RakNetPINVOKE.new_RakNetListSortQuery__SWIG_0(), cMemoryOwn: true)
	{
	}

	public RakNetListSortQuery(RakNetListSortQuery original_copy)
		: this(RakNetPINVOKE.new_RakNetListSortQuery__SWIG_1(getCPtr(original_copy)), cMemoryOwn: true)
	{
		if (RakNetPINVOKE.SWIGPendingException.Pending)
		{
			throw RakNetPINVOKE.SWIGPendingException.Retrieve();
		}
	}

	internal static HandleRef getCPtr(RakNetListSortQuery obj)
	{
		return obj?.swigCPtr ?? new HandleRef(null, IntPtr.Zero);
	}

	~RakNetListSortQuery()
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
					RakNetPINVOKE.delete_RakNetListSortQuery(swigCPtr);
				}
				swigCPtr = new HandleRef(null, IntPtr.Zero);
			}
			GC.SuppressFinalize(this);
		}
	}

	public RakNetListSortQuery CopyData(RakNetListSortQuery original_copy)
	{
		RakNetListSortQuery result = new RakNetListSortQuery(RakNetPINVOKE.RakNetListSortQuery_CopyData(swigCPtr, getCPtr(original_copy)), cMemoryOwn: false);
		if (RakNetPINVOKE.SWIGPendingException.Pending)
		{
			throw RakNetPINVOKE.SWIGPendingException.Retrieve();
		}
		return result;
	}

	public SortQuery Get(uint position)
	{
		return new SortQuery(RakNetPINVOKE.RakNetListSortQuery_Get(swigCPtr, position), cMemoryOwn: false);
	}

	public void Push(SortQuery input, string file, uint line)
	{
		RakNetPINVOKE.RakNetListSortQuery_Push(swigCPtr, SortQuery.getCPtr(input), file, line);
		if (RakNetPINVOKE.SWIGPendingException.Pending)
		{
			throw RakNetPINVOKE.SWIGPendingException.Retrieve();
		}
	}

	public SortQuery Pop()
	{
		return new SortQuery(RakNetPINVOKE.RakNetListSortQuery_Pop(swigCPtr), cMemoryOwn: false);
	}

	public void Insert(SortQuery input, uint position, string file, uint line)
	{
		RakNetPINVOKE.RakNetListSortQuery_Insert__SWIG_0(swigCPtr, SortQuery.getCPtr(input), position, file, line);
		if (RakNetPINVOKE.SWIGPendingException.Pending)
		{
			throw RakNetPINVOKE.SWIGPendingException.Retrieve();
		}
	}

	public void Insert(SortQuery input, string file, uint line)
	{
		RakNetPINVOKE.RakNetListSortQuery_Insert__SWIG_1(swigCPtr, SortQuery.getCPtr(input), file, line);
		if (RakNetPINVOKE.SWIGPendingException.Pending)
		{
			throw RakNetPINVOKE.SWIGPendingException.Retrieve();
		}
	}

	public void Replace(SortQuery input, SortQuery filler, uint position, string file, uint line)
	{
		RakNetPINVOKE.RakNetListSortQuery_Replace__SWIG_0(swigCPtr, SortQuery.getCPtr(input), SortQuery.getCPtr(filler), position, file, line);
		if (RakNetPINVOKE.SWIGPendingException.Pending)
		{
			throw RakNetPINVOKE.SWIGPendingException.Retrieve();
		}
	}

	public void Replace(SortQuery input)
	{
		RakNetPINVOKE.RakNetListSortQuery_Replace__SWIG_1(swigCPtr, SortQuery.getCPtr(input));
		if (RakNetPINVOKE.SWIGPendingException.Pending)
		{
			throw RakNetPINVOKE.SWIGPendingException.Retrieve();
		}
	}

	public void RemoveAtIndex(uint position)
	{
		RakNetPINVOKE.RakNetListSortQuery_RemoveAtIndex(swigCPtr, position);
	}

	public void RemoveAtIndexFast(uint position)
	{
		RakNetPINVOKE.RakNetListSortQuery_RemoveAtIndexFast(swigCPtr, position);
	}

	public void RemoveFromEnd(uint num)
	{
		RakNetPINVOKE.RakNetListSortQuery_RemoveFromEnd__SWIG_0(swigCPtr, num);
	}

	public void RemoveFromEnd()
	{
		RakNetPINVOKE.RakNetListSortQuery_RemoveFromEnd__SWIG_1(swigCPtr);
	}

	public uint Size()
	{
		return RakNetPINVOKE.RakNetListSortQuery_Size(swigCPtr);
	}

	public void Clear(bool doNotDeallocateSmallBlocks, string file, uint line)
	{
		RakNetPINVOKE.RakNetListSortQuery_Clear(swigCPtr, doNotDeallocateSmallBlocks, file, line);
	}

	public void Preallocate(uint countNeeded, string file, uint line)
	{
		RakNetPINVOKE.RakNetListSortQuery_Preallocate(swigCPtr, countNeeded, file, line);
	}

	public void Compress(string file, uint line)
	{
		RakNetPINVOKE.RakNetListSortQuery_Compress(swigCPtr, file, line);
	}
}
