using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace S031.MetaStack.Common
{
	/// <summary>
	/// A simple class for working with data of type key = value, such as connection strings, etc.
	/// </summary>
	/// <example <see cref="Translater"/> />
	public class KeyValueList: Dictionary <string, string>
	{
		private const string _defaultListSep = ";";
		private const string _defaultValueSep = "=";

		private string _listSep;
		private string _valueSep;
		/// <summary>
		/// Create empty <see cref="KeyValueList"/> with default list and value sep
		/// </summary>
		public KeyValueList()
			: base(StringComparer.CurrentCultureIgnoreCase)
		{
			_listSep = _defaultListSep;
			_valueSep = _defaultValueSep;
		}

		/// <summary>
		/// Create empty <see cref="KeyValueList"/> with specific list and value sep
		/// </summary>
		public KeyValueList(string listSep, string valueSep)
			: base(StringComparer.CurrentCultureIgnoreCase)
		{
			listSep.NullTest(nameof(listSep));
			valueSep.NullTest(nameof(valueSep));
			_listSep = listSep;
			_valueSep = valueSep;
		}

		/// <summary>
		/// Create <see cref="KeyValueList"/> with default list and value sep from string source
		/// </summary>
		/// <param name="source">Source string for parse</param>
		public KeyValueList(string source)
			: this(source, _defaultListSep, _defaultValueSep)
		{
		}

		/// <summary>
		///  Create <see cref="KeyValueList"/> with specific list and value sep from string source
		/// </summary>
		/// <param name="source">Source string for parse</param>
		/// <param name="listSep">List separator</param>
		/// <param name="valueSep">Values separator</param>
		public KeyValueList(string source, string listSep, string valueSep)
			: base(StringComparer.CurrentCultureIgnoreCase)
		{
			source.NullTest(nameof(source));
			listSep.NullTest(nameof(listSep));
			valueSep.NullTest(nameof(valueSep));

			_listSep = listSep;
			_valueSep = valueSep;
			if (source.IndexOf(_valueSep) > -1)
			{
				var s = source.Split(_listSep.ToCharArray(), StringSplitOptions.RemoveEmptyEntries)
				.Select(t => t.Split(_valueSep.ToCharArray(), StringSplitOptions.RemoveEmptyEntries));
				foreach (var a in s)
					if (a.Length > 1 && !a[0].IsEmpty())
						Add(a[0], a[1]);
			}
		}

		/// <summary>
		///  Create <see cref="KeyValueList"/> from <see cref="IEnumerable{KeyValuePair{string, string}}"/> with specific list and value sep
		/// </summary>
		/// <param name="source">Source string for parse</param>
		/// <param name="listSep">List separator</param>
		/// <param name="valueSep">Values separator</param>
#if NETCOREAPP2_0
	public KeyValueList(IEnumerable<KeyValuePair<string, string>> source, string listSep, string valueSep)
#else
	public KeyValueList(IDictionary<string, string> source, string listSep, string valueSep)
#endif
			: base(source, StringComparer.CurrentCultureIgnoreCase)
		{
			_listSep = listSep;
			_valueSep = valueSep;
		}
		
		/// <summary>
		/// Get, Set value assotiated with specific key
		/// </summary>
		/// <param name="index">get value assotiated with specific key</param>
		/// <returns>string, if key not found return empty string</returns>
		public new string this[string index]
		{
			get
			{
				TryGetValue(index, out string result);
				return result;
			}
			set
			{
				base[index] = value;
			}
		}
		/// <summary>
		/// String representation for key-value collection
		/// </summary>
		/// <returns>string foratted as [key][valueSep][value][listsep]</returns>
		/// <example>key1=value1;key2=value2</example>
		public override string ToString()
		{
			StringBuilder sb = new StringBuilder(Count);
			foreach (var kvp in this)
				sb.AppendFormat("{0}{1}{2}{3}", kvp.Key, _valueSep, kvp.Value, _listSep);
			return sb.ToString();
		}
	}
}
