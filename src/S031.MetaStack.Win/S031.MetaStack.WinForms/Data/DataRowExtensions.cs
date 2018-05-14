using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace S031.MetaStack.WinForms.Data
{
	public static class DataRowExtension
	{
		public static DataRow Clone(this DataRow sourceRow)
		{
			DataTable sourceTable = sourceRow.Table;
			DataRow newRow = sourceTable.NewRow();
			for (int i = 0; i < sourceTable.Columns.Count; i++)
			{
				newRow[i] = sourceRow[i];
			}
			return newRow;
		}

		public static void CopyTo(this DataRow sourceRow, DataRow destinationRow)
		{
			for (int i = 0; i < sourceRow.Table.Columns.Count; i++)
			{
				destinationRow[i] = sourceRow[i];
			}
		}
	}
}
