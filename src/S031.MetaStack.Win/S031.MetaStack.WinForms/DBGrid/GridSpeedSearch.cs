﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Data;
using S031.MetaStack.Common;

namespace S031.MetaStack.WinForms
{
	public sealed class GridSpeedSearch
	{
		DBGridBase _grid;
		ToolStripTextBox txtFind;
		ToolTip errToolTip;

		string searchColName;
		Type searchColType;

		public GridSpeedSearch(DBGridBase gridControl, ToolStripTextBox findControl)
		{
			_grid = gridControl;
			txtFind = findControl;
			txtFind.ToolTipText = "Быстрый поиск";
			txtFind.TextChanged += new EventHandler(txtFind_TextChanged);
			txtFind.Leave += new EventHandler((sender, e) => { searchColName = string.Empty; });
			txtFind.KeyDown += new KeyEventHandler((sender, e) =>
			{
				if (e.KeyCode == Keys.Enter || e.KeyCode == Keys.Down)
					_grid.Focus();
			});

			_grid.KeyPress += new KeyPressEventHandler((sender, e) =>
			{
				if (char.IsLetterOrDigit(e.KeyChar))
				{
					txtFind.Text += e.KeyChar.ToString();
				}
			});
			_grid.KeyDown += (sender, e) =>
			{
				if (e.KeyCode == Keys.Back)
				{
					int len;
					string text = txtFind.Text;
					if ((len = text.Length) > 1)
						txtFind.Text = text.Left(len - 1);
					else
						this.Clear();
				}
			};
			errToolTip = new ToolTip
			{
				ToolTipIcon = ToolTipIcon.Error
			};
		}

		public void Clear()
		{
			txtFind.Text = string.Empty;
			searchColName = string.Empty;
		}

		public void Search(string s)
		{
			txtFind.Text = s;
		}

		public string SearchText
		{
			get { return txtFind.Text; }
		}

		void txtFind_TextChanged(object sender, EventArgs e)
		{
			if (_grid.BaseTable.Rows.Count == 0)
				return;
			BindingSource datasource = _grid.DataSource as BindingSource;
			if (datasource == null)
				return;

			string colName;
			if (!string.IsNullOrEmpty(searchColName))
				colName = searchColName;
			else
			{
				colName = _grid.Columns[_grid.CurrentCellAddress.X].Name;
				searchColName = colName;
				searchColType = _grid.CurrentCell.Value.GetType();
			}
			string template = txtFind.Text.ToLower();
			string filter;
			if (string.IsNullOrEmpty(template))
				filter = string.Empty;
			else if (searchColType == typeof(string))
				filter = $"{colName} Like '%{template}%'";
			else if (searchColType.IsNumeric())
				filter = $"(Convert([{colName}], 'System.String') LIKE '%{template}%')";
			else if (searchColType == typeof(DateTime)) 
				filter = $"(Convert([{colName}], 'System.String') LIKE '%{template}%')";
			else 
				filter = $"(Convert([{colName}], 'System.String') LIKE '%{template}%')";

			try
			{
				datasource.Filter = filter;
			}
			catch (EvaluateException err)
			{
				errToolTip.Show(err.Message, txtFind.TextBox, 2000);
			}
			if (_grid.Rows.Count > 0)
				_grid.CurrentCell = _grid.Rows[0].Cells[_grid.Columns[colName].Index];
		}
	}
}
