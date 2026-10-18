using System;
using System.Runtime.InteropServices;

namespace RakNet;

public class RakNetListCellPointer : IDisposable
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

	internal RakNetListCellPointer(IntPtr cPtr, bool cMemoryOwn)
	{
		swigCMemOwn = cMemoryOwn;
		swigCPtr = new HandleRef(this, cPtr);
	}

	public RakNetListCellPointer()
		: this(RakNetPINVOKE.new_RakNetListCellPointer__SWIG_0(), cMemoryOwn: true)
	{
	}

	public RakNetListCellPointer(RakNetListCellPointer original_copy)
		: this(RakNetPINVOKE.new_RakNetListCellPointer__SWIG_1(getCPtr(original_copy)), cMemoryOwn: true)
	{
		if (RakNetPINVOKE.SWIGPendingException.Pending)
		{
			throw RakNetPINVOKE.SWIGPendingException.Retrieve();
		}
	}

	internal static HandleRef getCPtr(RakNetListCellPointer obj)
	{
		return obj?.swigCPtr ?? new HandleRef(null, IntPtr.Zero);
	}

	~RakNetListCellPointer()
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
					RakNetPINVOKE.delete_RakNetListCellPointer(swigCPtr);
				}
				swigCPtr = new HandleRef(null, IntPtr.Zero);
			}
			GC.SuppressFinalize(this);
		}
	}

	public Cell Get(uint position)
	{
		return GetHelper(position);
	}

	public Cell Pop()
	{
		return PopHelper();
	}

	public RakNetListCellPointer CopyData(RakNetListCellPointer original_copy)
	{
		RakNetListCellPointer result = new RakNetListCellPointer(RakNetPINVOKE.RakNetListCellPointer_CopyData(swigCPtr, getCPtr(original_copy)), cMemoryOwn: false);
		if (RakNetPINVOKE.SWIGPendingException.Pending)
		{
			throw RakNetPINVOKE.SWIGPendingException.Retrieve();
		}
		return result;
	}

	public void Push(Cell input, string file, uint line)
	{
		RakNetPINVOKE.RakNetListCellPointer_Push(swigCPtr, Cell.getCPtr(input), file, line);
	}

	public void Insert(Cell input, uint position, string file, uint line)
	{
		RakNetPINVOKE.RakNetListCellPointer_Insert__SWIG_0(swigCPtr, Cell.getCPtr(input), position, file, line);
	}

	public void Insert(Cell input, string file, uint line)
	{
		RakNetPINVOKE.RakNetListCellPointer_Insert__SWIG_1(swigCPtr, Cell.getCPtr(input), file, line);
	}

	public void Replace(Cell input, Cell filler, uint position, string file, uint line)
	{
		RakNetPINVOKE.RakNetListCellPointer_Replace__SWIG_0(swigCPtr, Cell.getCPtr(input), Cell.getCPtr(filler), position, file, line);
	}

	public void Replace(Cell input)
	{
		RakNetPINVOKE.RakNetListCellPointer_Replace__SWIG_1(swigCPtr, Cell.getCPtr(input));
	}

	public void RemoveAtIndex(uint position)
	{
		RakNetPINVOKE.RakNetListCellPointer_RemoveAtIndex(swigCPtr, position);
	}

	public void RemoveAtIndexFast(uint position)
	{
		RakNetPINVOKE.RakNetListCellPointer_RemoveAtIndexFast(swigCPtr, position);
	}

	public void RemoveFromEnd(uint num)
	{
		RakNetPINVOKE.RakNetListCellPointer_RemoveFromEnd__SWIG_0(swigCPtr, num);
	}

	public void RemoveFromEnd()
	{
		RakNetPINVOKE.RakNetListCellPointer_RemoveFromEnd__SWIG_1(swigCPtr);
	}

	public uint GetIndexOf(Cell input)
	{
		return RakNetPINVOKE.RakNetListCellPointer_GetIndexOf(swigCPtr, Cell.getCPtr(input));
	}

	public uint Size()
	{
		return RakNetPINVOKE.RakNetListCellPointer_Size(swigCPtr);
	}

	public void Clear(bool doNotDeallocateSmallBlocks, string file, uint line)
	{
		RakNetPINVOKE.RakNetListCellPointer_Clear(swigCPtr, doNotDeallocateSmallBlocks, file, line);
	}

	public void Preallocate(uint countNeeded, string file, uint line)
	{
		RakNetPINVOKE.RakNetListCellPointer_Preallocate(swigCPtr, countNeeded, file, line);
	}

	public void Compress(string file, uint line)
	{
		RakNetPINVOKE.RakNetListCellPointer_Compress(swigCPtr, file, line);
	}

	public Cell GetHelper(uint position)
	{
		IntPtr intPtr = RakNetPINVOKE.RakNetListCellPointer_GetHelper(swigCPtr, position);
		return (!(intPtr == IntPtr.Zero)) ? new Cell(intPtr, cMemoryOwn: false) : null;
	}

	public Cell PopHelper()
	{
		IntPtr intPtr = RakNetPINVOKE.RakNetListCellPointer_PopHelper(swigCPtr);
		return (!(intPtr == IntPtr.Zero)) ? new Cell(intPtr, cMemoryOwn: false) : null;
	}
}
