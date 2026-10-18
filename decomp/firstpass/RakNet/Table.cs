using System;
using System.Runtime.InteropServices;

namespace RakNet;

public class Table : IDisposable
{
	public enum ColumnType
	{
		NUMERIC,
		STRING,
		BINARY,
		POINTER
	}

	public enum FilterQueryType
	{
		QF_EQUAL,
		QF_NOT_EQUAL,
		QF_GREATER_THAN,
		QF_GREATER_THAN_EQ,
		QF_LESS_THAN,
		QF_LESS_THAN_EQ,
		QF_IS_EMPTY,
		QF_NOT_EMPTY
	}

	public enum SortQueryType
	{
		QS_INCREASING_ORDER,
		QS_DECREASING_ORDER
	}

	private HandleRef swigCPtr;

	protected bool swigCMemOwn;

	internal Table(IntPtr cPtr, bool cMemoryOwn)
	{
		swigCMemOwn = cMemoryOwn;
		swigCPtr = new HandleRef(this, cPtr);
	}

	public Table()
		: this(RakNetPINVOKE.new_Table(), cMemoryOwn: true)
	{
	}

	internal static HandleRef getCPtr(Table obj)
	{
		return obj?.swigCPtr ?? new HandleRef(null, IntPtr.Zero);
	}

	~Table()
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
					RakNetPINVOKE.delete_Table(swigCPtr);
				}
				swigCPtr = new HandleRef(null, IntPtr.Zero);
			}
			GC.SuppressFinalize(this);
		}
	}

	public RakNetPageRow GetListHead()
	{
		return GetListHeadHelper();
	}

	public void SortTable(SortQuery[] sortQueries, uint numSortQueries, out Row[] arg2)
	{
		RakNetListSortQuery rakNetListSortQuery = null;
		if (sortQueries != null)
		{
			rakNetListSortQuery = new RakNetListSortQuery();
			int num = sortQueries.Length;
			for (int i = 0; i < num; i++)
			{
				rakNetListSortQuery.Insert(sortQueries[i], string.Empty, 1u);
			}
		}
		int rowCount = (int)GetRowCount();
		Row[] array = new Row[rowCount];
		RakNetListTableRow rakNetListTableRow = new RakNetListTableRow();
		SortTableHelper(rakNetListSortQuery, numSortQueries, rakNetListTableRow);
		for (int j = 0; j < rowCount; j++)
		{
			array[j] = rakNetListTableRow[j];
		}
		arg2 = array;
	}

	public void GetCellValueByIndex(uint rowIndex, uint columnIndex, out byte[] outByteArray, out int outputLength)
	{
		int key = 0;
		Row rowByIndex = GetRowByIndex(rowIndex, ref key);
		int num = 0;
		if (rowByIndex != null)
		{
			num = (int)rowByIndex.cells[(int)columnIndex].i;
		}
		byte[] array = new byte[num];
		GetCellValueByIndexHelper(rowIndex, columnIndex, array, out outputLength);
		outByteArray = array;
	}

	public void GetCellValueByIndex(uint rowIndex, uint columnIndex, out string output)
	{
		int key = 0;
		Row rowByIndex = GetRowByIndex(rowIndex, ref key);
		int count = 0;
		if (rowByIndex != null)
		{
			count = (int)rowByIndex.cells[(int)columnIndex].i;
		}
		string output2 = new string('c', count);
		output = GetCellValueByIndexHelper(rowIndex, columnIndex, output2);
	}

	public void QueryTable(uint[] columnIndicesSubset, uint numColumnSubset, FilterQuery[] inclusionFilters, uint numInclusionFilters, uint[] rowIds, uint numRowIDs, Table result)
	{
		RakNetListFilterQuery rakNetListFilterQuery = null;
		if (rakNetListFilterQuery != null)
		{
			rakNetListFilterQuery = new RakNetListFilterQuery();
			int num = inclusionFilters.Length;
			for (int i = 0; i < num; i++)
			{
				rakNetListFilterQuery.Insert(inclusionFilters[i], string.Empty, 1u);
			}
		}
		QueryTableHelper(columnIndicesSubset, numColumnSubset, rakNetListFilterQuery, numInclusionFilters, rowIds, numRowIDs, result);
	}

	public uint ColumnIndex(string columnName)
	{
		return ColumnIndexHelper(columnName);
	}

	public uint AddColumn(string columnName, ColumnType columnType)
	{
		return RakNetPINVOKE.Table_AddColumn(swigCPtr, columnName, (int)columnType);
	}

	public void RemoveColumn(uint columnIndex)
	{
		RakNetPINVOKE.Table_RemoveColumn(swigCPtr, columnIndex);
	}

	public string ColumnName(uint index)
	{
		return RakNetPINVOKE.Table_ColumnName(swigCPtr, index);
	}

	public ColumnType GetColumnType(uint index)
	{
		return (ColumnType)RakNetPINVOKE.Table_GetColumnType(swigCPtr, index);
	}

	public uint GetColumnCount()
	{
		return RakNetPINVOKE.Table_GetColumnCount(swigCPtr);
	}

	public uint GetRowCount()
	{
		return RakNetPINVOKE.Table_GetRowCount(swigCPtr);
	}

	public Row AddRow(uint rowId)
	{
		IntPtr intPtr = RakNetPINVOKE.Table_AddRow__SWIG_0(swigCPtr, rowId);
		return (!(intPtr == IntPtr.Zero)) ? new Row(intPtr, cMemoryOwn: false) : null;
	}

	public Row AddRow(uint rowId, RakNetListCell initialCellValues)
	{
		IntPtr intPtr = RakNetPINVOKE.Table_AddRow__SWIG_1(swigCPtr, rowId, RakNetListCell.getCPtr(initialCellValues));
		Row result = ((!(intPtr == IntPtr.Zero)) ? new Row(intPtr, cMemoryOwn: false) : null);
		if (RakNetPINVOKE.SWIGPendingException.Pending)
		{
			throw RakNetPINVOKE.SWIGPendingException.Retrieve();
		}
		return result;
	}

	public Row AddRow(uint rowId, RakNetListCellPointer initialCellValues, bool copyCells)
	{
		IntPtr intPtr = RakNetPINVOKE.Table_AddRow__SWIG_2(swigCPtr, rowId, RakNetListCellPointer.getCPtr(initialCellValues), copyCells);
		Row result = ((!(intPtr == IntPtr.Zero)) ? new Row(intPtr, cMemoryOwn: false) : null);
		if (RakNetPINVOKE.SWIGPendingException.Pending)
		{
			throw RakNetPINVOKE.SWIGPendingException.Retrieve();
		}
		return result;
	}

	public Row AddRow(uint rowId, RakNetListCellPointer initialCellValues)
	{
		IntPtr intPtr = RakNetPINVOKE.Table_AddRow__SWIG_3(swigCPtr, rowId, RakNetListCellPointer.getCPtr(initialCellValues));
		Row result = ((!(intPtr == IntPtr.Zero)) ? new Row(intPtr, cMemoryOwn: false) : null);
		if (RakNetPINVOKE.SWIGPendingException.Pending)
		{
			throw RakNetPINVOKE.SWIGPendingException.Retrieve();
		}
		return result;
	}

	public bool RemoveRow(uint rowId)
	{
		return RakNetPINVOKE.Table_RemoveRow(swigCPtr, rowId);
	}

	public void RemoveRows(Table tableContainingRowIDs)
	{
		RakNetPINVOKE.Table_RemoveRows(swigCPtr, getCPtr(tableContainingRowIDs));
	}

	public bool UpdateCell(uint rowId, uint columnIndex, int value)
	{
		return RakNetPINVOKE.Table_UpdateCell__SWIG_0(swigCPtr, rowId, columnIndex, value);
	}

	public bool UpdateCell(uint rowId, uint columnIndex, string str)
	{
		return RakNetPINVOKE.Table_UpdateCell__SWIG_1(swigCPtr, rowId, columnIndex, str);
	}

	public bool UpdateCellByIndex(uint rowIndex, uint columnIndex, int value)
	{
		return RakNetPINVOKE.Table_UpdateCellByIndex__SWIG_0(swigCPtr, rowIndex, columnIndex, value);
	}

	public bool UpdateCellByIndex(uint rowIndex, uint columnIndex, string str)
	{
		return RakNetPINVOKE.Table_UpdateCellByIndex__SWIG_1(swigCPtr, rowIndex, columnIndex, str);
	}

	public void GetCellValueByIndex(uint rowIndex, uint columnIndex, out int output)
	{
		RakNetPINVOKE.Table_GetCellValueByIndex(swigCPtr, rowIndex, columnIndex, out output);
	}

	public Row GetRowByID(uint rowId)
	{
		IntPtr intPtr = RakNetPINVOKE.Table_GetRowByID(swigCPtr, rowId);
		return (!(intPtr == IntPtr.Zero)) ? new Row(intPtr, cMemoryOwn: false) : null;
	}

	public Row GetRowByIndex(uint rowIndex, ref int key)
	{
		IntPtr intPtr = RakNetPINVOKE.Table_GetRowByIndex(swigCPtr, rowIndex, ref key);
		return (!(intPtr == IntPtr.Zero)) ? new Row(intPtr, cMemoryOwn: false) : null;
	}

	public void Clear()
	{
		RakNetPINVOKE.Table_Clear(swigCPtr);
	}

	public RakNetListColumnDescriptor GetColumns()
	{
		return new RakNetListColumnDescriptor(RakNetPINVOKE.Table_GetColumns(swigCPtr), cMemoryOwn: false);
	}

	public RakNetBPlusTreeRow GetRows()
	{
		return new RakNetBPlusTreeRow(RakNetPINVOKE.Table_GetRows(swigCPtr), cMemoryOwn: false);
	}

	public uint GetAvailableRowId()
	{
		return RakNetPINVOKE.Table_GetAvailableRowId(swigCPtr);
	}

	public Table CopyData(Table input)
	{
		Table result = new Table(RakNetPINVOKE.Table_CopyData(swigCPtr, getCPtr(input)), cMemoryOwn: false);
		if (RakNetPINVOKE.SWIGPendingException.Pending)
		{
			throw RakNetPINVOKE.SWIGPendingException.Retrieve();
		}
		return result;
	}

	private RakNetPageRow GetListHeadHelper()
	{
		IntPtr intPtr = RakNetPINVOKE.Table_GetListHeadHelper(swigCPtr);
		return (!(intPtr == IntPtr.Zero)) ? new RakNetPageRow(intPtr, cMemoryOwn: false) : null;
	}

	private void SortTableHelper(RakNetListSortQuery sortQueries, uint numSortQueries, RakNetListTableRow arg2)
	{
		RakNetPINVOKE.Table_SortTableHelper(swigCPtr, RakNetListSortQuery.getCPtr(sortQueries), numSortQueries, RakNetListTableRow.getCPtr(arg2));
	}

	private void GetCellValueByIndexHelper(uint rowIndex, uint columnIndex, byte[] inOutByteArray, out int outputLength)
	{
		RakNetPINVOKE.Table_GetCellValueByIndexHelper__SWIG_0(swigCPtr, rowIndex, columnIndex, inOutByteArray, out outputLength);
	}

	private string GetCellValueByIndexHelper(uint rowIndex, uint columnIndex, string output)
	{
		return RakNetPINVOKE.Table_GetCellValueByIndexHelper__SWIG_1(swigCPtr, rowIndex, columnIndex, output);
	}

	public void PrintColumnHeaders(byte[] inOutByteArray, int byteArrayLength, char columnDelineator)
	{
		RakNetPINVOKE.Table_PrintColumnHeaders(swigCPtr, inOutByteArray, byteArrayLength, columnDelineator);
	}

	public void PrintRow(byte[] inOutByteArray, int byteArrayLength, char columnDelineator, bool printDelineatorForBinary, Row inputRow)
	{
		RakNetPINVOKE.Table_PrintRow(swigCPtr, inOutByteArray, byteArrayLength, columnDelineator, printDelineatorForBinary, Row.getCPtr(inputRow));
	}

	private void QueryTableHelper(uint[] columnIndicesSubset, uint numColumnSubset, RakNetListFilterQuery inclusionFilters, uint numInclusionFilters, uint[] rowIds, uint numRowIDs, Table result)
	{
		RakNetPINVOKE.Table_QueryTableHelper(swigCPtr, columnIndicesSubset, numColumnSubset, RakNetListFilterQuery.getCPtr(inclusionFilters), numInclusionFilters, rowIds, numRowIDs, getCPtr(result));
	}

	public bool UpdateCell(uint rowId, uint columnIndex, int byteLength, byte[] inByteArray)
	{
		return RakNetPINVOKE.Table_UpdateCell__SWIG_2(swigCPtr, rowId, columnIndex, byteLength, inByteArray);
	}

	public bool UpdateCellByIndex(uint rowIndex, uint columnIndex, int byteLength, byte[] inByteArray)
	{
		return RakNetPINVOKE.Table_UpdateCellByIndex__SWIG_2(swigCPtr, rowIndex, columnIndex, byteLength, inByteArray);
	}

	private uint ColumnIndexHelper(string columnName)
	{
		return RakNetPINVOKE.Table_ColumnIndexHelper(swigCPtr, columnName);
	}
}
