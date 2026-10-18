using System;
using System.Runtime.InteropServices;

namespace RakNet;

public class RakNetListFilterQuery : IDisposable
{
	private HandleRef swigCPtr;

	protected bool swigCMemOwn;

	public FilterQuery this[int index]
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

	internal RakNetListFilterQuery(IntPtr cPtr, bool cMemoryOwn)
	{
		swigCMemOwn = cMemoryOwn;
		swigCPtr = new HandleRef(this, cPtr);
	}

	public RakNetListFilterQuery()
		: this(RakNetPINVOKE.new_RakNetListFilterQuery__SWIG_0(), cMemoryOwn: true)
	{
	}

	public RakNetListFilterQuery(RakNetListFilterQuery original_copy)
		: this(RakNetPINVOKE.new_RakNetListFilterQuery__SWIG_1(getCPtr(original_copy)), cMemoryOwn: true)
	{
		if (RakNetPINVOKE.SWIGPendingException.Pending)
		{
			throw RakNetPINVOKE.SWIGPendingException.Retrieve();
		}
	}

	internal static HandleRef getCPtr(RakNetListFilterQuery obj)
	{
		return obj?.swigCPtr ?? new HandleRef(null, IntPtr.Zero);
	}

	~RakNetListFilterQuery()
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
					RakNetPINVOKE.delete_RakNetListFilterQuery(swigCPtr);
				}
				swigCPtr = new HandleRef(null, IntPtr.Zero);
			}
			GC.SuppressFinalize(this);
		}
	}

	public RakNetListFilterQuery CopyData(RakNetListFilterQuery original_copy)
	{
		RakNetListFilterQuery result = new RakNetListFilterQuery(RakNetPINVOKE.RakNetListFilterQuery_CopyData(swigCPtr, getCPtr(original_copy)), cMemoryOwn: false);
		if (RakNetPINVOKE.SWIGPendingException.Pending)
		{
			throw RakNetPINVOKE.SWIGPendingException.Retrieve();
		}
		return result;
	}

	public FilterQuery Get(uint position)
	{
		return new FilterQuery(RakNetPINVOKE.RakNetListFilterQuery_Get(swigCPtr, position), cMemoryOwn: false);
	}

	public void Push(FilterQuery input, string file, uint line)
	{
		RakNetPINVOKE.RakNetListFilterQuery_Push(swigCPtr, FilterQuery.getCPtr(input), file, line);
		if (RakNetPINVOKE.SWIGPendingException.Pending)
		{
			throw RakNetPINVOKE.SWIGPendingException.Retrieve();
		}
	}

	public FilterQuery Pop()
	{
		return new FilterQuery(RakNetPINVOKE.RakNetListFilterQuery_Pop(swigCPtr), cMemoryOwn: false);
	}

	public void Insert(FilterQuery input, uint position, string file, uint line)
	{
		RakNetPINVOKE.RakNetListFilterQuery_Insert__SWIG_0(swigCPtr, FilterQuery.getCPtr(input), position, file, line);
		if (RakNetPINVOKE.SWIGPendingException.Pending)
		{
			throw RakNetPINVOKE.SWIGPendingException.Retrieve();
		}
	}

	public void Insert(FilterQuery input, string file, uint line)
	{
		RakNetPINVOKE.RakNetListFilterQuery_Insert__SWIG_1(swigCPtr, FilterQuery.getCPtr(input), file, line);
		if (RakNetPINVOKE.SWIGPendingException.Pending)
		{
			throw RakNetPINVOKE.SWIGPendingException.Retrieve();
		}
	}

	public void Replace(FilterQuery input, FilterQuery filler, uint position, string file, uint line)
	{
		RakNetPINVOKE.RakNetListFilterQuery_Replace__SWIG_0(swigCPtr, FilterQuery.getCPtr(input), FilterQuery.getCPtr(filler), position, file, line);
		if (RakNetPINVOKE.SWIGPendingException.Pending)
		{
			throw RakNetPINVOKE.SWIGPendingException.Retrieve();
		}
	}

	public void Replace(FilterQuery input)
	{
		RakNetPINVOKE.RakNetListFilterQuery_Replace__SWIG_1(swigCPtr, FilterQuery.getCPtr(input));
		if (RakNetPINVOKE.SWIGPendingException.Pending)
		{
			throw RakNetPINVOKE.SWIGPendingException.Retrieve();
		}
	}

	public void RemoveAtIndex(uint position)
	{
		RakNetPINVOKE.RakNetListFilterQuery_RemoveAtIndex(swigCPtr, position);
	}

	public void RemoveAtIndexFast(uint position)
	{
		RakNetPINVOKE.RakNetListFilterQuery_RemoveAtIndexFast(swigCPtr, position);
	}

	public void RemoveFromEnd(uint num)
	{
		RakNetPINVOKE.RakNetListFilterQuery_RemoveFromEnd__SWIG_0(swigCPtr, num);
	}

	public void RemoveFromEnd()
	{
		RakNetPINVOKE.RakNetListFilterQuery_RemoveFromEnd__SWIG_1(swigCPtr);
	}

	public uint Size()
	{
		return RakNetPINVOKE.RakNetListFilterQuery_Size(swigCPtr);
	}

	public void Clear(bool doNotDeallocateSmallBlocks, string file, uint line)
	{
		RakNetPINVOKE.RakNetListFilterQuery_Clear(swigCPtr, doNotDeallocateSmallBlocks, file, line);
	}

	public void Preallocate(uint countNeeded, string file, uint line)
	{
		RakNetPINVOKE.RakNetListFilterQuery_Preallocate(swigCPtr, countNeeded, file, line);
	}

	public void Compress(string file, uint line)
	{
		RakNetPINVOKE.RakNetListFilterQuery_Compress(swigCPtr, file, line);
	}
}
