using System;
using System.Runtime.InteropServices;

namespace RakNet;

public class RakNetListFileListNode : IDisposable
{
	private HandleRef swigCPtr;

	protected bool swigCMemOwn;

	public FileListNode this[int index]
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

	internal RakNetListFileListNode(IntPtr cPtr, bool cMemoryOwn)
	{
		swigCMemOwn = cMemoryOwn;
		swigCPtr = new HandleRef(this, cPtr);
	}

	public RakNetListFileListNode()
		: this(RakNetPINVOKE.new_RakNetListFileListNode__SWIG_0(), cMemoryOwn: true)
	{
	}

	public RakNetListFileListNode(RakNetListFileListNode original_copy)
		: this(RakNetPINVOKE.new_RakNetListFileListNode__SWIG_1(getCPtr(original_copy)), cMemoryOwn: true)
	{
		if (RakNetPINVOKE.SWIGPendingException.Pending)
		{
			throw RakNetPINVOKE.SWIGPendingException.Retrieve();
		}
	}

	internal static HandleRef getCPtr(RakNetListFileListNode obj)
	{
		return obj?.swigCPtr ?? new HandleRef(null, IntPtr.Zero);
	}

	~RakNetListFileListNode()
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
					RakNetPINVOKE.delete_RakNetListFileListNode(swigCPtr);
				}
				swigCPtr = new HandleRef(null, IntPtr.Zero);
			}
			GC.SuppressFinalize(this);
		}
	}

	public RakNetListFileListNode CopyData(RakNetListFileListNode original_copy)
	{
		RakNetListFileListNode result = new RakNetListFileListNode(RakNetPINVOKE.RakNetListFileListNode_CopyData(swigCPtr, getCPtr(original_copy)), cMemoryOwn: false);
		if (RakNetPINVOKE.SWIGPendingException.Pending)
		{
			throw RakNetPINVOKE.SWIGPendingException.Retrieve();
		}
		return result;
	}

	public FileListNode Get(uint position)
	{
		return new FileListNode(RakNetPINVOKE.RakNetListFileListNode_Get(swigCPtr, position), cMemoryOwn: false);
	}

	public void Push(FileListNode input, string file, uint line)
	{
		RakNetPINVOKE.RakNetListFileListNode_Push(swigCPtr, FileListNode.getCPtr(input), file, line);
		if (RakNetPINVOKE.SWIGPendingException.Pending)
		{
			throw RakNetPINVOKE.SWIGPendingException.Retrieve();
		}
	}

	public FileListNode Pop()
	{
		return new FileListNode(RakNetPINVOKE.RakNetListFileListNode_Pop(swigCPtr), cMemoryOwn: false);
	}

	public void Insert(FileListNode input, uint position, string file, uint line)
	{
		RakNetPINVOKE.RakNetListFileListNode_Insert__SWIG_0(swigCPtr, FileListNode.getCPtr(input), position, file, line);
		if (RakNetPINVOKE.SWIGPendingException.Pending)
		{
			throw RakNetPINVOKE.SWIGPendingException.Retrieve();
		}
	}

	public void Insert(FileListNode input, string file, uint line)
	{
		RakNetPINVOKE.RakNetListFileListNode_Insert__SWIG_1(swigCPtr, FileListNode.getCPtr(input), file, line);
		if (RakNetPINVOKE.SWIGPendingException.Pending)
		{
			throw RakNetPINVOKE.SWIGPendingException.Retrieve();
		}
	}

	public void Replace(FileListNode input, FileListNode filler, uint position, string file, uint line)
	{
		RakNetPINVOKE.RakNetListFileListNode_Replace__SWIG_0(swigCPtr, FileListNode.getCPtr(input), FileListNode.getCPtr(filler), position, file, line);
		if (RakNetPINVOKE.SWIGPendingException.Pending)
		{
			throw RakNetPINVOKE.SWIGPendingException.Retrieve();
		}
	}

	public void Replace(FileListNode input)
	{
		RakNetPINVOKE.RakNetListFileListNode_Replace__SWIG_1(swigCPtr, FileListNode.getCPtr(input));
		if (RakNetPINVOKE.SWIGPendingException.Pending)
		{
			throw RakNetPINVOKE.SWIGPendingException.Retrieve();
		}
	}

	public void RemoveAtIndex(uint position)
	{
		RakNetPINVOKE.RakNetListFileListNode_RemoveAtIndex(swigCPtr, position);
	}

	public void RemoveAtIndexFast(uint position)
	{
		RakNetPINVOKE.RakNetListFileListNode_RemoveAtIndexFast(swigCPtr, position);
	}

	public void RemoveFromEnd(uint num)
	{
		RakNetPINVOKE.RakNetListFileListNode_RemoveFromEnd__SWIG_0(swigCPtr, num);
	}

	public void RemoveFromEnd()
	{
		RakNetPINVOKE.RakNetListFileListNode_RemoveFromEnd__SWIG_1(swigCPtr);
	}

	public uint Size()
	{
		return RakNetPINVOKE.RakNetListFileListNode_Size(swigCPtr);
	}

	public void Clear(bool doNotDeallocateSmallBlocks, string file, uint line)
	{
		RakNetPINVOKE.RakNetListFileListNode_Clear(swigCPtr, doNotDeallocateSmallBlocks, file, line);
	}

	public void Preallocate(uint countNeeded, string file, uint line)
	{
		RakNetPINVOKE.RakNetListFileListNode_Preallocate(swigCPtr, countNeeded, file, line);
	}

	public void Compress(string file, uint line)
	{
		RakNetPINVOKE.RakNetListFileListNode_Compress(swigCPtr, file, line);
	}
}
