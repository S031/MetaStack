using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using S031.MetaStack.WinForms.ORM;
using S031.MetaStack.WinForms.Data;
using S031.MetaStack.Common;
using S031.MetaStack.WinForms.Json;

namespace S031.MetaStack.WinForms
{
	public class MultiEdit : WinForm, IObjectEditor
	{
		JMXSchema _schema;
		JMXObject _objectSource;

		public event EventHandler<ActionEventArgs> LoadDataComplete;
		public event EventHandler<ActionEventArgs> DataSaved;

		public MultiEdit(string objectName)
		{
			this.Width = 500;
			this.Height = (int)(this.Width / vbo.GoldenRatio);
			_schema = JMXFactory
				.Create()
				.CreateJMXRepo()
				.GetSchema(objectName);
			this.Add<Panel>(WinFormConfig.SinglePageForm);
			this.LoadDataComplete += (c, e) => AddSTDButtons();
		}


		protected virtual void OnLoadDataComplete(ActionEventArgs e)
		{
			LoadDataComplete?.Invoke(this, e);
		}
		protected virtual void OnDataSaved(ActionEventArgs e)
		{
			DataSaved?.Invoke(this, e);
		}


		#region Protected Members
		protected virtual void OnLoadData()
		{
			TableLayoutPanel tlpRows = Items["FormRowsPanel"].LinkedControl as TableLayoutPanel;
			TableLayoutPanel p = tlpRows.Add<TableLayoutPanel>(new WinFormItem("WorkCells") { CellsSize = new Pair<int>(2, 1) });
			p.ColumnStyles[0].SizeType = SizeType.Percent;
			p.ColumnStyles[0].Width = 38;
			p.ColumnStyles[1].SizeType = SizeType.Percent;
			p.ColumnStyles[1].Width = 62;
			foreach (var item in _schema.Attributes)
				if (item.Visible)
					p.Add(Attrib2WinFormItem(item, null));
		}
		protected virtual void OnConfigureGrid(DBGridBase dg)
		{
		}
		protected virtual void OnPopulateGrid(DBGridBase dg)
		{
		}
		protected virtual void OnDataSave()
		{
			OnSave2Object();
			if (TrueSaved = this.OnObjectValidate())
			{
				this.GetItem("OK").As<Button>().Enabled = false;
				OnAfterDataSave();
			}
		}
		protected virtual void OnSave2Object()
		{
			this.Save();

			foreach (var item in _schema.Attributes)
				if (item.Visible)
				{
					if (item.DataType == MdbType.@object)
						OnGrid2Object(GetItem(item.AttribName).As<DBGridBase>());
					else if (this.Items.TryGetValue(item.AttribName, out WinFormItem cdi))
						_objectSource[item.AttribName] = new MetaStack.Json.JsonValue(cdi.Value);
				}
		}


		protected virtual bool OnObjectValidate()
		{
			return true;
		}
		protected virtual void OnGrid2Object(DBGridBase dg)
		{
		}

		protected virtual void OnAfterDataSave()
		{
		}
		#endregion Protected Members

		#region IObjectEditor Members
		public JMXObject EditObject
		{
			get => _objectSource;
			set
			{
				_objectSource = value;
				if (value != null)
				{
					OnLoadData();
					OnLoadDataComplete(new ActionEventArgs(_objectSource));
				}
			}
		}
		public new Form Owner
		{
			get => base.Owner;
			set
			{
				base.Owner = value;
				if (value != null)
				{
					//if (value.Modal)
						this.DialogStyle = WinFormStyle.Dialog;
					//else
					//	this.DialogStyle = WinFormStyle.Form;
				}
			}
		}
		public IObjectHost ObjectHost { get; set; }
		public bool TrueSaved { get; set; }
		public string ObjectName { get; set; }
		#endregion IObjectEditor Members

		private WinFormItem Attrib2WinFormItem(JMXAttribute ai, object value)
		{
			WinFormItem item = new WinFormItem(ai.AttribName)
			{
				Caption = ai.Caption,
				DataType = MdbTypeMap.GetType(ai.DataType),
				Width = ai.Width,
				Format = ai.Format,
				Mask = ai.Mask,
				SuperForm = ai.SuperForm
			};
			if (string.IsNullOrEmpty(item.SuperForm))
				item.SuperForm = ai.SuperObject;
			item.SuperMethod = ai.SuperMethod;
			item.SuperFilter = ai.SuperFilter;
			if (value != null)
				item.Value = value;
			else if (_objectSource != null)
			{
				try
				{
					item.Value = _objectSource[ai.AttribName];
				}
				catch { }
			}
			return item;
		}
		private void AddSTDButtons()
		{
			Items["MainPanel"].LinkedControl.Add<TableLayoutPanel>(WinFormConfig.StdButtons("&Сохранить","&Закрыть" ));
			Button okButton = GetItem("OK").As<Button>();
			Button cancelButton = GetItem("Cancel").As<Button>();
			okButton.Enabled = (IsNew && _objectSource != null && _objectSource.ID != 0);

			GetItem("OK").DisabledSTDActions.Add("Click");
			okButton.Click += new EventHandler((sender, e) =>
			{
				OnDataSave();
				if (TrueSaved)
					OnDataSaved(new ActionEventArgs(_objectSource));
				if (!ContinuousEditing)
				{
					DialogResult = TrueSaved ? DialogResult.OK : DialogResult.Cancel;
					if (!this.Modal) this.Close();
				}
			});
			GetItem("Cancel").DisabledSTDActions.Add("Click");
			cancelButton.Click += new EventHandler((sender, e) =>
			{
				DialogResult = TrueSaved ? DialogResult.OK : DialogResult.Cancel;
				if (!this.Modal) this.Close();
			});
			this.DataChanged += new EventHandler<DataChangedEventArgs>((sender, e) => { okButton.Enabled = true; });
			this.AcceptButton = null;
		}
	}
}