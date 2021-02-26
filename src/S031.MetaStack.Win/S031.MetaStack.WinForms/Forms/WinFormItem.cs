using S031.MetaStack.Common;
using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Collections;

namespace S031.MetaStack.WinForms
{
	public class WinFormItem : INotifyPropertyChanged, IList<WinFormItem>
	{
		private List<WinFormItem> _childItems;

		private object _value;

		private readonly string _name;

		public event PropertyChangedEventHandler PropertyChanged;

		public WinFormItem(string name) : this(name, null) { }
		public WinFormItem(string name, params WinFormItem[] chiltItems)
		{
			_name = name;
			PresentationType = null;
			DataType = typeof(string);
			Mask = "";
			Format = "";
			Width = 10;
			ReadOnly = false;
			Visible = true;
			DisabledSTDActions = new List<string>();
			_childItems = new List<WinFormItem>();
			if (chiltItems != null)
				_childItems.AddRange(chiltItems);
		}
		[System.ComponentModel.Bindable(true)]
		public string Name => _name;
		[System.ComponentModel.Bindable(true)]
		public object Value
		{
			get => _value.CastAs(DataType);
			set
			{
				_value = value;
				OnPropertyChanged();
			}
		}
		public object OriginalValue => _value;
		public string Caption { get; set; }
		public Type PresentationType { get; set; }
		public Type DataType { get; set; }
		public string Mask { get; set; }
		public string Format { get; set; }
		public int Width { get; set; }
		public int DataSize { get; set; } = 256;
		public bool ReadOnly { get; set; }
		public bool Visible { get; set; }
		public string SuperForm { get; set; }
		public string SuperMethod { get; set; }
		public string SuperFilter { get; set; }
		public string ConstName { get; set; }
		public List<string> DisabledSTDActions { get; protected internal set; }
		public Pair<int> CellAddress { get; set; }
		public Pair<int> CellsSize { get; set; }
		public Action<WinFormItem, Control> ControlTrigger { get; set; }
		public WinForm Parent { get; protected internal set; }
		public Control LinkedControl { get; protected internal set; }
		public Control LinkedLabel { get; protected internal set; }

		public int Count
		{
			get
			{
				return ((IList<WinFormItem>)_childItems).Count;
			}
		}

		public bool IsReadOnly
		{
			get
			{
				return ((IList<WinFormItem>)_childItems).IsReadOnly;
			}
		}

		public WinFormItem this[int index]
		{
			get
			{
				return ((IList<WinFormItem>)_childItems)[index];
			}

			set
			{
				((IList<WinFormItem>)_childItems)[index] = value;
			}
		}

		protected virtual void OnPropertyChanged([CallerMemberName] String propertyName = "")
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

		}

		public int IndexOf(WinFormItem item)
		{
			return ((IList<WinFormItem>)_childItems).IndexOf(item);
		}

		public void Insert(int index, WinFormItem item)
		{
			((IList<WinFormItem>)_childItems).Insert(index, item);
		}

		public void RemoveAt(int index)
		{
			((IList<WinFormItem>)_childItems).RemoveAt(index);
		}

		public void Add(WinFormItem item)
		{
			((IList<WinFormItem>)_childItems).Add(item);
		}

		public void Clear()
		{
			((IList<WinFormItem>)_childItems).Clear();
		}

		public bool Contains(WinFormItem item)
		{
			return ((IList<WinFormItem>)_childItems).Contains(item);
		}

		public void CopyTo(WinFormItem[] array, int arrayIndex)
		{
			((IList<WinFormItem>)_childItems).CopyTo(array, arrayIndex);
		}

		public bool Remove(WinFormItem item)
		{
			return ((IList<WinFormItem>)_childItems).Remove(item);
		}

		public IEnumerator<WinFormItem> GetEnumerator()
		{
			return ((IList<WinFormItem>)_childItems).GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return ((IList<WinFormItem>)_childItems).GetEnumerator();
		}
		public T As<T>() where T : Control
		{
			return (T)this.LinkedControl;
		}
	}
}
