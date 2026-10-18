using System;
using System.Runtime.InteropServices;

namespace RakNet;

public class RakNetListCell : IDisposable
{
	private HandleRef swigCPtr;

	protected bool swigCMemOwn;

	public Cell this[int index]
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

	internal RakNetListCell(IntPtr cPtr, bool cMemoryOwn)
	{
		swigCMemOwn = cMemoryOwn;
		swigCPtr = new HandleRef(this, cPtr);
	}

	public RakNetListCell()
		: this(RakNetPINVOKE.new_RakNetListCell__SWIG_0(), cMemoryOwn: true)
	{
	}

	public RakNetListCell(RakNetListCell original_copy)
		: this(RakNetPINVOKE.new_RakNetListCell__SWIG_1(getCPtr(original_copy)), cMemoryOwn: true)
	{
		if (RakNetPINVOKE.SWIGPendingException.Pending)
		{
			throw RakNetPINVOKE.SWIGPendingException.Retrieve();
		}
	}

	internal static HandleRef getCPtr(RakNetListCell obj)
	{
		return obj?.swigCPtr ?? new HandleRef(null, IntPtr.Zero);
	}

	~RakNetListCell()
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
					RakNetPINVOKE.delete_RakNetListCell(swigCPtr);
				}
				swigCPtr = new HandleRef(null, IntPtr.Zero);
			}
			GC.SuppressFinalize(this);
		}
	}

	public RakNetListCell CopyData(RakNetListCell original_copy)
	{
		RakNetListCell result = new RakNetListCell(RakNetPINVOKE.RakNetListCell_CopyData(swigCPtr, getCPtr(original_copy)), cMemoryOwn: false);
		if (RakNetPINVOKE.SWIGPendingException.Pending)
		{
			throw RakNetPINVOKE.SWIGPendingException.Retrieve();
		}
		return result;
	}

	public Cell Get(uint position)
	{
		return new Cell(RakNetPINVOKE.RakNetListCell_Get(swigCPtr, position), cMemoryOwn: false);
	}

	public void Push(Cell input, string file, uint line)
	{
		RakNetPINVOKE.RakNetListCell_Push(swigCPtr, Cell.getCPtr(input), file, line);
		if (RakNetPINVOKE.SWIGPendingException.Pending)
		{
			throw RakNetPINVOKE.SWIGPendingException.Retrieve();
		}
	}

	public Cell Pop()
	{
		return new Cell(RakNetPINVOKE.RakNetListCell_Pop(swigCPtr), cMemoryOwn: false);
	}

	public void Insert(Cell input, uint position, string file, uint line)
	{
		RakNetPINVOKE.RakNetListCell_Insert__SWIG_0(swigCPtr, Cell.getCPtr(input), position, file, line);
		if (RakNetPINVOKE.SWIGPendingException.Pending)
		{
			throw RakNetPINVOKE.SWIGPendingException.Retrieve();
		}
	}

	public void Insert(Cell input, string file, uint line)
	{
		RakNetPINVOKE.RakNetListCell_Insert__SWIG_1(swigCPtr, Cell.getCPtr(input), file, line);
		if (RakNetPINVOKE.SWIGPendingException.Pending)
		{
			throw RakNetPINVOKE.SWIGPendingException.Retrieve();
		}
	}

	public void Replace(Cell input, Cell filler, uint position, string file, uint line)
	{
		RakNetPINVOKE.RakNetListCell_Replace__SWIG_0(swigCPtr, Cell.getCPtr(input), Cell.getCPtr(filler), position, file, line);
		if (RakNetPINVOKE.SWIGPendingException.Pending)
		{
			throw RakNetPINVOKE.SWIGPendingException.Retrieve();
		}
	}

	public void Replace(Cell input)
	{
		RakNetPINVOKE.RakNetListCell_Replace__SWIG_1(swigCPtr, Cell.getCPtr(input));
		if (RakNetPINVOKE.SWIGPendingException.Pending)
		{
			throw RakNetPINVOKE.SWIGPendingException.Retrieve();
		}
	}

	public void RemoveAtIndex(uint position)
	{
		RakNetPINVOKE.RakNetListCell_RemoveAtIndex(swigCPtr, position);
	}

	public void RemoveAtIndexFast(uint position)
	{
		RakNetPINVOKE.RakNetListCell_RemoveAtIndexFast(swigCPtr, position);
	}

	public void RemoveFromEnd(uint num)
	{
		RakNetPINVOKE.RakNetListCell_RemoveFromEnd__SWIG_0(swigCPtr, num);
	}

	public void RemoveFromEnd()
	{
		RakNetPINVOKE.RakNetListCell_RemoveFromEnd__SWIG_1(swigCPtr);
	}

	public uint Size()
	{
		return RakNetPINVOKE.RakNetListCell_Size(swigCPtr);
	}

	public void Clear(bool doNotDeallocateSmallBlocks, string file, uint line)
	{
		RakNetPINVOKE.RakNetListCell_Clear(swigCPtr, doNotDeallocateSmallBlocks, file, line);
	}

	public void Preallocate(uint countNeeded, string file, uint line)
	{
		RakNetPINVOKE.RakNetListCell_Preallocate(swigCPtr, countNeeded, file, line);
	}

	public void Compress(string file, uint line)
	{
		RakNetPINVOKE.RakNetListCell_Compress(swigCPtr, file, line);
	}
}
