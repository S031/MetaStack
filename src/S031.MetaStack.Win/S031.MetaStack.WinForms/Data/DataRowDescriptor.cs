using System.ComponentModel;

namespace System.Data
{
    public class DataRowPropertyDescriptor : PropertyDescriptor
	{
		DataColumn _col;
		public DataRowPropertyDescriptor(DataColumn column) : base(column.ColumnName, null)
		{
			_col = column;
		}
		public override object GetValue(object component)
		{
			return ((DataRow)component)[Name];
		}
		public override void SetValue(object component, object value)
		{
			((DataRow)component)[Name] = value;
		}
		public override void ResetValue(object component)
		{
			((DataRow)component)[Name] = null;
		}
		public override bool CanResetValue(object component)
		{
			return true;
		}
		public override bool ShouldSerializeValue(object component)
		{
			return ((DataRow)component)[Name] != null;
		}
		public override Type PropertyType
		{
			get { return _col.DataType; }
		}
		public override bool IsReadOnly
		{
			get { return false; }
		}
		public override Type ComponentType
		{
			get { return typeof(DataRow); }
		}
	}
}
