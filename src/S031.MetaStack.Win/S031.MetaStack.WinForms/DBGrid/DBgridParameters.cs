using S031.MetaStack.Common;
using S031.MetaStack.WinForms.Data;
using S031.MetaStack.WinForms.ORM;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace S031.MetaStack.WinForms
{
	public enum DBGridParamShowType
	{
		None = 0,
		ShowSmart = 1,
		ShowAll = 2
	}
	public sealed class DBGridParam
	{
		private const string setPath = "Newbank;Setup;";
		private static Dictionary<string, object> _paramCash = new Dictionary<string, object>(16, System.StringComparer.CurrentCultureIgnoreCase);
		private WinFormItem[] _items;
		
		//public DBGridParam(XMLForm form): this(form, null){}

		public DBGridParam(JMXSchema schema, DataRow parentRow)
		{
			int count = schema.Parameters.Count;
			List<WinFormItem> listItems = new List<WinFormItem>(count);
			foreach (var p in schema.Parameters)
			{
				WinFormItem item = new WinFormItem(p.ParamName)
				{
					Caption = p.Name,
					DataType = MdbTypeMap.GetType(p.DataType),
					Width = p.DisplayWidth,
					Format = p.Format,
					SuperForm = p.SuperForm,
					SuperMethod = p.SuperMethod,
					ConstName = p.ConstName,
					PresentationType = Type.GetType(p.PresentationType),
					Value = p.DefaultValue
				};
				if (p.ListData.Count>0)
				{
					for (int i = 0; i < p.ListData.Count; i++)
						item.Add(new WinFormItem(p.ListItems[i])
						{
							Caption = p.ListItems[i],
							DataType = item.DataType,
							Value = p.ListData[i]
						});
					item.Mask = "lock";
				}
				//!!! Добавить добавление констант в расширениях (class UserConstants.GetConst(), AddConst()
				switch (item.ConstName.ToLower())
				{
					case "bs_datecurrent":
						item.Value = vbo.Date();
						break;
					case "bs_datestart":
						item.Value = rth.DateStart;
						break;
					case "bs_datefinish":
						item.Value = rth.DateFinish;
						break;
					case "bs_username":
						item.Value = PathHelper.UserName;
						break;
					default:
						if (item.ConstName.Left(1) == "=")
						{
							//!!! item.Value = Evaluator.Eval(item.ConstName.Substring(1), schema);
						}
						else
						{
							//!!!
							//string localSetting = schema[path + "ConstName"];
							//if (localSetting.IndexOf(vbo.chrSep) > -1)
							//	localSetting = dbs.GetSetting(localSetting);
							//else
							//	localSetting = dbs.GetSetting(setPath + localSetting);
							//if (string.IsNullOrEmpty(localSetting))
							//	localSetting = dbs.GetSetting(schema["ObjectName"] + vbo.chrSep + "Setup" + vbo.chrSep +
							//		schema[path + "ConstName"]);
							//if (!string.IsNullOrEmpty(localSetting))
							//	item.Value = localSetting;
						}
						break;
				}
				if (item.OriginalValue == null && parentRow != null && !string.IsNullOrEmpty(p.FieldName))
				{
					try
					{
						item.Value = parentRow[p.FieldName];
					}
					finally
					{
						if (vbo.IsEmpty(item.Value)) item.Value = null;
					}
				}
				if ((item.Visible = (vbo.IsEmpty(item.Value))) && _paramCash.ContainsKey(item.Name))
					item.Value = _paramCash[item.Name];
				listItems.Add(item);
			}
			_items = listItems.ToArray();
		}

		public DialogResult ShowDialog()
		{
			return this.ShowDialog(DBGridParamShowType.ShowSmart);
		}

		public DialogResult ShowDialog(DBGridParamShowType showType)
		{
			int processed = _items.Count(item => showType == DBGridParamShowType.ShowAll || item.Visible);
			if (processed == 0)
				return DialogResult.OK;

			using (WinForm cd = new WinForm(WinFormStyle.Dialog))
			{
				cd.Add<Panel>(WinFormConfig.SinglePageForm);
				TableLayoutPanel tlpRows = cd.Items["FormRowsPanel"].LinkedControl as TableLayoutPanel;
				TableLayoutPanel p = tlpRows.Add<TableLayoutPanel>(new WinFormItem("WorkCells") { CellsSize = new Pair<int>(2, 1) });
				p.ColumnStyles[0].SizeType = SizeType.Percent;
				p.ColumnStyles[0].Width = 38;
				p.ColumnStyles[1].SizeType = SizeType.Percent;
				p.ColumnStyles[1].Width = 62;
				foreach (var item in _items)
					if (showType == DBGridParamShowType.ShowAll || item.Visible)
						p.Add(item);

				cd.Items["MainPanel"].LinkedControl.Add<TableLayoutPanel>(WinFormConfig.StdButtons());
				cd.Size = new Size(450, (int)(450 / vbo.GoldenRatio));
				cd.MinimumSize = cd.Size;
				cd.AcceptButton = null;
				if (cd.Parent != null)
					cd.StartPosition = FormStartPosition.CenterParent;
				else
					cd.StartPosition = FormStartPosition.CenterScreen;

				if (cd.ShowDialog() == DialogResult.OK)
				{
					cd.Save();
					Save2Cash();
					return DialogResult.OK;
				}
				else
					return DialogResult.Cancel;
			}
		}

		private void Save2Cash()
		{
			var d = _items.FirstOrDefault(item => item.ConstName.ToLower() == "bs_datestart");
			if (d != null)
				rth.DateStart = (DateTime)d.Value;

			d = _items.FirstOrDefault(item => item.ConstName.ToLower() == "bs_datefinish");
			if (d != null)
				rth.DateFinish = (DateTime)d.Value;

			if (rth.DateStart > rth.DateFinish)
				rth.DateStart = rth.DateFinish;

				
			for (int i = 0; i < _items.Length; i++)
			{
				if (_items[i].Value != null)
					_paramCash[_items[i].Name] = _items[i].Value;
			}
		}

		public object[] Values()
		{
			if (_items.Length == 0)
				return null;
			
			object[] listItems = new object[_items.Length];
			for (int i = 0; i < _items.Length; i++)
			{
				listItems[i] = _items[i].Value;
			}
			return listItems;
		}

		public object[] GetQueryParameters()
		{
			List<object> result = new List<object>();
			foreach (var p in _items)
			{
				result.Add(p.Name);
				result.Add(p.Value);
			}
			return result.ToArray();
		}

		public IEnumerable<WinFormItem> Items()
		{
			return _items;
		}
	}
}
