using System;
using S031.MetaStack.Common;

namespace S031.MetaStack.Core.Data
{
	/// <summary>
	/// A class of parameters for invariant database providers
	/// </summary>
	public class MdbParameter
    {
		private string _name;
		private object _value;
		private string _sqlTypeExact;
		private MdbTypeInfo _mdbTypeInfo;

		/// <summary>
		/// Create a new <see cref="MdbParameter"/>
		/// </summary>
		/// <param name="name">Parameter name</param>
		/// <param name="value">Parameter value</param>
		public MdbParameter(string name, object value)
		{
			Name = name;
			Value = value;
			this.NullIfEmpty = true;
		}
		/// <summary>
		/// Get, Set parameter name
		/// </summary>
		public string Name
		{
			get { return _name; }
			set { _name = value; }
		}
		/// <summary>
		/// If true return Null if parameter value is empty
		/// </summary>
		public bool NullIfEmpty { get; set; }
		/// <summary>
		/// Get, Set parameter value
		/// </summary>
		public object Value
		{
			set
			{
				_value = value;
				_mdbTypeInfo = MdbTypeMap.GetTypeInfo(value.GetType());
			}
			get
			{
				if (this.NullIfEmpty && vbo.IsEmpty(_value))
				{
					return DBNull.Value;
				}
				return _value;
			}
		}
		/// <summary>
		/// Return <see cref="MdbType" of parameter
		/// </summary>
		public MdbType MdbType => _mdbTypeInfo.MdbType;
		/// <summary>
		/// Return size of parameter
		/// </summary>
		public int Size => _mdbTypeInfo.Size;
		/// <summary>
		/// Get, Set SQL data type name for parameter (int, varchar(max)...)
		/// </summary>
		public string SqlTypeExact
		{
			set { _sqlTypeExact = value; }
			get { return _sqlTypeExact; }
		}
	}
}
