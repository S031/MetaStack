using System.Collections;
using System.Collections.Generic;

namespace System.Data
{
    public class DataRow : IEnumerable<object>
	{
		private readonly DataTable _parent;
		private object[] _data;
		protected internal DataRow(DataTable parent)
		{
			_parent = parent;
			parent.IsInited = true;
			_data = new object[parent.Columns.Count];
		}
		public object this[string columnName]
		{
			get
			{
				return _data[_parent.ColumnIndex(columnName)];
			}
			set
			{
				_data[_parent.ColumnIndex(columnName)] = value;
			}
		}
		public object this[int columnIndex]
		{
			get
			{
				return _data[columnIndex];
			}
			set
			{
				_data[columnIndex] = value;
			}
		}
		public IEnumerator<object> GetEnumerator()
		{
			return ((IEnumerable<object>)_data).GetEnumerator();
		}
		IEnumerator IEnumerable.GetEnumerator()
		{
			return ((IEnumerable<object>)_data).GetEnumerator();
		}
	}
}
