using System;
using System.Runtime.InteropServices;

namespace RakNet;

public class FilterQuery : IDisposable
{
	private HandleRef swigCPtr;

	protected bool swigCMemOwn;

	public string columnName
	{
		get
		{
			return RakNetPINVOKE.FilterQuery_columnName_get(swigCPtr);
		}
		set
		{
			RakNetPINVOKE.FilterQuery_columnName_set(swigCPtr, value);
		}
	}

	public uint columnIndex
	{
		get
		{
			return RakNetPINVOKE.FilterQuery_columnIndex_get(swigCPtr);
		}
		set
		{
			RakNetPINVOKE.FilterQuery_columnIndex_set(swigCPtr, value);
		}
	}

	public Cell cellValue
	{
		get
		{
			IntPtr intPtr = RakNetPINVOKE.FilterQuery_cellValue_get(swigCPtr);
			return (!(intPtr == IntPtr.Zero)) ? new Cell(intPtr, cMemoryOwn: false) : null;
		}
		set
		{
			RakNetPINVOKE.FilterQuery_cellValue_set(swigCPtr, Cell.getCPtr(value));
		}
	}

	public Table.FilterQueryType operation
	{
		get
		{
			return (Table.FilterQueryType)RakNetPINVOKE.FilterQuery_operation_get(swigCPtr);
		}
		set
		{
			RakNetPINVOKE.FilterQuery_operation_set(swigCPtr, (int)value);
		}
	}

	internal FilterQuery(IntPtr cPtr, bool cMemoryOwn)
	{
		swigCMemOwn = cMemoryOwn;
		swigCPtr = new HandleRef(this, cPtr);
	}

	public FilterQuery()
		: this(RakNetPINVOKE.new_FilterQuery__SWIG_0(), cMemoryOwn: true)
	{
	}

	public FilterQuery(uint column, Cell cell, Table.FilterQueryType op)
		: this(RakNetPINVOKE.new_FilterQuery__SWIG_1(column, Cell.getCPtr(cell), (int)op), cMemoryOwn: true)
	{
	}

	internal static HandleRef getCPtr(FilterQuery obj)
	{
		return obj?.swigCPtr ?? new HandleRef(null, IntPtr.Zero);
	}

	~FilterQuery()
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
					RakNetPINVOKE.delete_FilterQuery(swigCPtr);
				}
				swigCPtr = new HandleRef(null, IntPtr.Zero);
			}
			GC.SuppressFinalize(this);
		}
	}
}
