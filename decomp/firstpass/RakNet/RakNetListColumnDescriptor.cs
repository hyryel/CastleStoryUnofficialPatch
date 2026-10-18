using System;
using System.Runtime.InteropServices;

namespace RakNet;

public class RakNetListColumnDescriptor : IDisposable
{
	private HandleRef swigCPtr;

	protected bool swigCMemOwn;

	public ColumnDescriptor this[int index]
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

	internal RakNetListColumnDescriptor(IntPtr cPtr, bool cMemoryOwn)
	{
		swigCMemOwn = cMemoryOwn;
		swigCPtr = new HandleRef(this, cPtr);
	}

	public RakNetListColumnDescriptor()
		: this(RakNetPINVOKE.new_RakNetListColumnDescriptor__SWIG_0(), cMemoryOwn: true)
	{
	}

	public RakNetListColumnDescriptor(RakNetListColumnDescriptor original_copy)
		: this(RakNetPINVOKE.new_RakNetListColumnDescriptor__SWIG_1(getCPtr(original_copy)), cMemoryOwn: true)
	{
		if (RakNetPINVOKE.SWIGPendingException.Pending)
		{
			throw RakNetPINVOKE.SWIGPendingException.Retrieve();
		}
	}

	internal static HandleRef getCPtr(RakNetListColumnDescriptor obj)
	{
		return obj?.swigCPtr ?? new HandleRef(null, IntPtr.Zero);
	}

	~RakNetListColumnDescriptor()
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
					RakNetPINVOKE.delete_RakNetListColumnDescriptor(swigCPtr);
				}
				swigCPtr = new HandleRef(null, IntPtr.Zero);
			}
			GC.SuppressFinalize(this);
		}
	}

	public RakNetListColumnDescriptor CopyData(RakNetListColumnDescriptor original_copy)
	{
		RakNetListColumnDescriptor result = new RakNetListColumnDescriptor(RakNetPINVOKE.RakNetListColumnDescriptor_CopyData(swigCPtr, getCPtr(original_copy)), cMemoryOwn: false);
		if (RakNetPINVOKE.SWIGPendingException.Pending)
		{
			throw RakNetPINVOKE.SWIGPendingException.Retrieve();
		}
		return result;
	}

	public ColumnDescriptor Get(uint position)
	{
		return new ColumnDescriptor(RakNetPINVOKE.RakNetListColumnDescriptor_Get(swigCPtr, position), cMemoryOwn: false);
	}

	public void Push(ColumnDescriptor input, string file, uint line)
	{
		RakNetPINVOKE.RakNetListColumnDescriptor_Push(swigCPtr, ColumnDescriptor.getCPtr(input), file, line);
		if (RakNetPINVOKE.SWIGPendingException.Pending)
		{
			throw RakNetPINVOKE.SWIGPendingException.Retrieve();
		}
	}

	public ColumnDescriptor Pop()
	{
		return new ColumnDescriptor(RakNetPINVOKE.RakNetListColumnDescriptor_Pop(swigCPtr), cMemoryOwn: false);
	}

	public void Insert(ColumnDescriptor input, uint position, string file, uint line)
	{
		RakNetPINVOKE.RakNetListColumnDescriptor_Insert__SWIG_0(swigCPtr, ColumnDescriptor.getCPtr(input), position, file, line);
		if (RakNetPINVOKE.SWIGPendingException.Pending)
		{
			throw RakNetPINVOKE.SWIGPendingException.Retrieve();
		}
	}

	public void Insert(ColumnDescriptor input, string file, uint line)
	{
		RakNetPINVOKE.RakNetListColumnDescriptor_Insert__SWIG_1(swigCPtr, ColumnDescriptor.getCPtr(input), file, line);
		if (RakNetPINVOKE.SWIGPendingException.Pending)
		{
			throw RakNetPINVOKE.SWIGPendingException.Retrieve();
		}
	}

	public void Replace(ColumnDescriptor input, ColumnDescriptor filler, uint position, string file, uint line)
	{
		RakNetPINVOKE.RakNetListColumnDescriptor_Replace__SWIG_0(swigCPtr, ColumnDescriptor.getCPtr(input), ColumnDescriptor.getCPtr(filler), position, file, line);
		if (RakNetPINVOKE.SWIGPendingException.Pending)
		{
			throw RakNetPINVOKE.SWIGPendingException.Retrieve();
		}
	}

	public void Replace(ColumnDescriptor input)
	{
		RakNetPINVOKE.RakNetListColumnDescriptor_Replace__SWIG_1(swigCPtr, ColumnDescriptor.getCPtr(input));
		if (RakNetPINVOKE.SWIGPendingException.Pending)
		{
			throw RakNetPINVOKE.SWIGPendingException.Retrieve();
		}
	}

	public void RemoveAtIndex(uint position)
	{
		RakNetPINVOKE.RakNetListColumnDescriptor_RemoveAtIndex(swigCPtr, position);
	}

	public void RemoveAtIndexFast(uint position)
	{
		RakNetPINVOKE.RakNetListColumnDescriptor_RemoveAtIndexFast(swigCPtr, position);
	}

	public void RemoveFromEnd(uint num)
	{
		RakNetPINVOKE.RakNetListColumnDescriptor_RemoveFromEnd__SWIG_0(swigCPtr, num);
	}

	public void RemoveFromEnd()
	{
		RakNetPINVOKE.RakNetListColumnDescriptor_RemoveFromEnd__SWIG_1(swigCPtr);
	}

	public uint Size()
	{
		return RakNetPINVOKE.RakNetListColumnDescriptor_Size(swigCPtr);
	}

	public void Clear(bool doNotDeallocateSmallBlocks, string file, uint line)
	{
		RakNetPINVOKE.RakNetListColumnDescriptor_Clear(swigCPtr, doNotDeallocateSmallBlocks, file, line);
	}

	public void Preallocate(uint countNeeded, string file, uint line)
	{
		RakNetPINVOKE.RakNetListColumnDescriptor_Preallocate(swigCPtr, countNeeded, file, line);
	}

	public void Compress(string file, uint line)
	{
		RakNetPINVOKE.RakNetListColumnDescriptor_Compress(swigCPtr, file, line);
	}
}
