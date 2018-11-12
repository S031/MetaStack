using S031.MetaStack.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace S031.MetaStack.WinForms
{
	internal class FormOpenTimeStatistics: KeyValueList
	{
		public FormOpenTimeStatistics() : base(LocalSettings.Default.FormOpenTimeStatistics)
		{
		}

		public new string this[string index]
		{
			get
			{
				if (!TryGetValue(index, out string result))
				{
					result = "0";
					Add(index, result);
				}
				return result;
			}
			set
			{
				base[index] = value;
				LocalSettings.Default.FormOpenTimeStatistics = this.ToString();
			}
		}
		public int GetTime(string formName) => this[formName].ToIntOrDefault();

		public int GetTime(string formName, int defaultValue)
		{
			int result = this[formName].ToIntOrDefault();
			return result == 0 ? defaultValue : result;			
		}

		public void SetTime(string formName, int milliseconds)=>this[formName] = milliseconds.ToString();

	}
}
