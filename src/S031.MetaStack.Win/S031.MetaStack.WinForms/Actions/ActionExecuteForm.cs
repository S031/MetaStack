using S031.MetaStack.Common;
using System;
using System.Linq;
using System.Windows.Forms;
using S031.MetaStack.Data;

namespace S031.MetaStack.WinForms.Actions
{
	public sealed class ActionExecuteForm: WinForm
	{
		private readonly ActionInfo _ai;
		public ActionExecuteForm(DBGrid grid, string actionID) 
			: base(WinFormStyle.Dialog)
		{
			this.Add<Panel>(WinFormConfig.SinglePageForm);
			TableLayoutPanel tlpRows = this.Items["FormRowsPanel"].LinkedControl as TableLayoutPanel;
			TableLayoutPanel tp = tlpRows.Add<TableLayoutPanel>(new WinFormItem("WorkCells") { CellsSize = new Pair<int>(2, 1) });
			tp.ColumnStyles[0].SizeType = SizeType.Percent;
			tp.ColumnStyles[0].Width = 38;
			tp.ColumnStyles[1].SizeType = SizeType.Percent;
			tp.ColumnStyles[1].Width = 62;

			int maxWidth = 480;
			_ai = ClientGate.GetActionInfo(actionID);
			this.Text = _ai.Name;
			foreach (var p in _ai.InterfaceParameters
				.Where(param=>param.Value.Dirrect == ParamDirrect.Input)
				.OrderBy(param=>param.Value.Position)
				.Select(param=>param.Value))
			{
				WinFormItem item = new WinFormItem(p.AttribName)
				{
					Caption = p.Name,
					DataType = MdbTypeMap.GetType(p.DataType),
					Width = p.DisplayWidth,
					DataSize = p.Width,
					Format = p.Format,
					SuperForm = p.SuperForm,
					SuperMethod = p.SuperMethod,
					ConstName = p.ConstName,
					PresentationType = Type.GetType(p.PresentationType),
					Value = p.DefaultValue
				};
				if (!p.ListData.IsEmpty())
				{
					string[] data = p.ListData.Split(',');
					string[] text = p.ListItems.Split(',');
					for (int i = 0; i < data.Length; i++)
						item.Add(new WinFormItem(text[i])
						{
							Caption = text[i],
							DataType = item.DataType,
							Value = data[i].CastOf(item.DataType)
						});
					item.Mask = "lock";
				}
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
					case "bs_selection":
						item.Value = string.Join(",", grid.CheckedRows.Select(r => r[grid.IdColName].ToString()));
						break;
					case "bs_objectname":
						item.Value = grid.SchemaName;
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
				if (item.OriginalValue == null && grid.ParentRow != null && !string.IsNullOrEmpty(p.FieldName))
				{
					try
					{
						item.Value = grid.ParentRow[p.FieldName];
					}
					finally
					{
						if (vbo.IsEmpty(item.Value)) item.Value = null;
					}
				}
				int width = Math.Max(p.Name.Length, p.DisplayWidth) * (int)this.Font.SizeInPoints;
				if (width > maxWidth) maxWidth = width;
				tp.Add(item);
			}
			var btp = GetItem("MainPanel")
				.LinkedControl
				.Add<TableLayoutPanel>(WinFormConfig.StdButtons(OKCaption: "&Выполнить"));
			this.AcceptButton = null;
			this.Width = maxWidth;
			this.Height = (int)(this.Width / vbo.GoldenRatio);
			if (this.Parent != null)
				this.StartPosition = FormStartPosition.CenterParent;
			else
				this.StartPosition = FormStartPosition.CenterScreen;
		}

		public DataPackage GetInputParamTable()
		{
			if (this.DialogResult == DialogResult.OK)
			{
				this.Save();
				var dr = _ai.GetInputParamTable();
				dr.AddNew();
				foreach (var pi in _ai.InterfaceParameters.Where(kvp => kvp.Value.Dirrect == ParamDirrect.Input).Select(kvp => kvp.Value))
				{
					dr[pi.AttribName] = this.GetItem(pi.AttribName).Value;
				}
				dr.Update();
				return dr;
			}
			else
				return null;
		}
	}
}
