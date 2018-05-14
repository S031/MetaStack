using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;

namespace System.Data
{
    public class DataTable : List<DataRow>, ITypedList
	{
		private ObservableCollection<DataColumn> _columns;

		private readonly Dictionary<string, int> _indexes;

		public IList<DataColumn> Columns => _columns;

		public DataTable()
		{
			_columns = new ObservableCollection<DataColumn>();
			_columns.CollectionChanged += columnsCollectionChanged;
			_indexes = new Dictionary<string, int>();
		}

		protected internal bool IsInited { get; set; }

		private void columnsCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			if (IsInited)
				throw new InvalidOperationException("Невозможно изменить схему объекта DataTable после добавления данных");
			if (e.Action == NotifyCollectionChangedAction.Add && e.NewStartingIndex == _columns.Count - 1)
			{
				DataColumn c = (e.NewItems[0] as DataColumn);
				_indexes.Add(c.ColumnName, e.NewStartingIndex);
			}
			else
			{
				_indexes.Clear();
				int i = 0;
				foreach (var c in _columns)
					_indexes.Add(c.ColumnName, i++);
			}
		}

		public int ColumnIndex(string columnName) => _indexes[columnName];

		public DataRow NewRow() => new DataRow(this);

		PropertyDescriptorCollection ITypedList.GetItemProperties(PropertyDescriptor[] listAccessors)
		{
			if (listAccessors == null || listAccessors.Length == 0)
			{
				PropertyDescriptor[] props = new PropertyDescriptor[Columns.Count];
				for (int i = 0; i < props.Length; i++)
				{
					props[i] = new DataRowPropertyDescriptor(Columns[i]);
				}
				return new PropertyDescriptorCollection(props, true);
			}
			throw new NotImplementedException("Relations not implemented");
		}

		string ITypedList.GetListName(PropertyDescriptor[] listAccessors)
		{
			return "DataRow";
		}
	}
}
