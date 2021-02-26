using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using S031.MetaStack.Common;

namespace S031.MetaStack.WinForms
{
	public static class ControlExtensions
	{
		public static T Add<T>(this Control parent) where T : Control
		{
			WinForm wf = parent.GetOwner();
			if (wf == null)
				throw new InvalidOperationException($"Control extensoin method Add<T> may be called only for WinForm child control");
			WinFormItem wfi = new WinFormItem(wf.NewName(typeof(T)))
			{
				PresentationType = typeof(T)
			};
			return (T)AddInternal(parent, wfi);
		}
		public static T Add<T>(this Control parent, string name) where T : Control
		{
			name.NullTest(nameof(name));
			WinFormItem wfi = new WinFormItem(name)
			{
				PresentationType = typeof(T)
			};
			return (T)AddInternal(parent, wfi);
		}

		public static T Add<T>(this Control parent, WinFormItem winFormItem) where T : Control
		{
			Type t = typeof(T);
			winFormItem.NullTest(nameof(winFormItem));
			winFormItem.PresentationType = t;
			return (T)AddInternal(parent, winFormItem);
		}

		public static object Add(this Control parent, WinFormItem winFormItem)
		{
			if (!winFormItem.Caption.IsEmpty())
			{
				Label l;
				if (winFormItem.CellAddress == default)
				{
					l = parent.Add<Label>(new WinFormItem("Label_" + winFormItem.Name)
					{
						CellAddress = new Pair<int>(winFormItem.CellAddress.X, winFormItem.CellAddress.Y)
					});
					winFormItem.CellAddress = new Pair<int>(winFormItem.CellAddress.X + 1, winFormItem.CellAddress.Y);
				}
				else
					l = parent.Add<Label>(new WinFormItem("Label_" + winFormItem.Name));
				l.Text = winFormItem.Caption;
			}
			if (winFormItem.PresentationType == null)
			{
				if (winFormItem.DataType == typeof(bool))
				{
					winFormItem.PresentationType = typeof(CheckBox);
					winFormItem.Clear();
					winFormItem.Add(new WinFormItem("Да") { Value = true });
					winFormItem.Add(new WinFormItem("Нет") { Value = false });
					winFormItem.ControlTrigger = (i, c) =>
					  {
						  CheckBox chkBox = c as CheckBox;
						  chkBox.Text = (bool)i.Value ? "Да" : "Нет";
						  chkBox.Checked = (bool)i.Value;
						  chkBox.CheckedChanged += (ctrl, e) =>
						  {
							  CheckBox cb = (CheckBox)ctrl;
							  chkBox.Text = cb.Checked ? "Да" : "Нет";
						  };
					  };
				}
				else if (winFormItem.DataType == typeof(DateTime))
				{
					winFormItem.PresentationType = typeof(DateEdit);
				}
				else if (!winFormItem.SuperForm.IsEmpty())
				{
					winFormItem.PresentationType = typeof(TextBuX);
				}
			}
			if ((winFormItem.PresentationType == null || winFormItem.PresentationType.IsSubclassOf(typeof(ListBox))) &&
				winFormItem.Count() > 0 && winFormItem.SuperForm.IsEmpty() && IsSimpleType(winFormItem.DataType))
			{

				winFormItem.PresentationType = typeof(ComboBox);
				winFormItem.ControlTrigger = (item, c) =>
				  {
					  ComboBox cmb = c as ComboBox;
					  if (item.Mask.ToLower() == "lock") cmb.DropDownStyle = ComboBoxStyle.DropDownList;
					  if (!string.IsNullOrEmpty(item.Format)) cmb.FormatString = item.Format;
					  cmb.Items.AddRange(item.ToArray());
					  cmb.DataSource = item.Select(wfi => wfi.Name).ToList();
					  cmb.DataBindings.Add("Text", item, "Value");

					  Type t = winFormItem.DataType;
					  if (t.IsNumeric())
					  {
						  var itemWithData = winFormItem.FirstOrDefault(d => d.Value.Equals(winFormItem.Value));
						  if (itemWithData != null)
							  cmb.Text = itemWithData.Caption;
					  }
				  };
			}
			else if ((winFormItem.PresentationType == typeof(RadioGroup) && winFormItem.Count() > 0 &&
				winFormItem.SuperForm.IsEmpty() && IsSimpleType(winFormItem.DataType)))
			{
				winFormItem.ControlTrigger = (i, c) =>
				  {
					  RadioGroup rg = c as RadioGroup;
					  rg.Items.AddRange(i.Select(item => item.Name).ToArray());
					  if (rg.Items.Count > 2)
						  rg.Height = Math.Max(rg.Height, rg.Items.Count * rg.Font.Height * 2);
					  else
						  rg.Height = rg.Font.Height * 5;

					  if (winFormItem.Value != null)
						  rg.Text = winFormItem.Value.ToString();
				  };
			}
			else if (winFormItem.PresentationType == null)
				winFormItem.PresentationType = typeof(TextBox);
			return AddInternal(parent, winFormItem);
		}
		public static WinFormRef Tag(this Control c)
		{
			return c.Tag as WinFormRef;
		}
		static object AddInternal(Control parent, WinFormItem winFormItem)
		{
			parent.SuspendLayout();
			WinForm wf = parent.GetOwner();
			wf.OnItemAdd(winFormItem);
			if (wf == null)
				throw new InvalidOperationException($"Control extensoin method Add<T> may be called only for WinForm child control");
			Type t = winFormItem.PresentationType;
			var instance = (Control)Activator.CreateInstance(t);
			instance.Tag = new WinFormRef() { WinForm = wf, WinFormItem = winFormItem };
			instance.Name = $"{t.Name}_{winFormItem.Name}";
			if (instance is Splitter)
				instance.Dock = DockStyle.Right;
			else
				instance.Dock = DockStyle.Fill;


			wf.Items.Add(winFormItem.Name, winFormItem);
			if (winFormItem.Value != null)
				instance.Text = winFormItem.Value.ToString();
			winFormItem.LinkedControl = instance;
			var cellAddress = winFormItem.CellAddress;

			TextBuX tbux = instance as TextBuX;
			void dataChanged(object c, EventArgs e) { (c as Control)?.GetOwner().OnDataChanged(new DataChangedEventArgs(c as Control)); }

			if (instance is TextBoxBase || tbux != null)
			{
				instance.TextChanged += new EventHandler(dataChanged);
				instance.GotFocus += TextBox_GotFocus;
				instance.KeyDown += Enter_KeyDown;
				if (tbux != null)
				{
					tbux.Click += CmdTB_Click;
					tbux.TextBox.GotFocus += TextBox_GotFocus;
					if (winFormItem.DataType.IsNumeric(NumericTypesScope.All))
					{
						tbux.TextAlign = HorizontalAlignment.Right;
						tbux.KeyPress += new KeyPressEventHandler(Num_KeyPress);
					}
					else if (winFormItem.DataType == typeof(string) && winFormItem.SuperForm == WinForm.StrViewFormList)
					{
						//tbux.TextBox.Enabled = false;
						tbux.TextBox.Multiline = true;
						tbux.TextBox.WordWrap = false;
						tbux.TextBox.MaxLength = winFormItem.DataSize;
					}
				}
				else if (instance is TextBox && winFormItem.DataType.IsNumeric(NumericTypesScope.All))
				{
					TextBox tb = (instance as TextBox);
					tb.TextAlign = HorizontalAlignment.Right;
					tb.KeyPress += new KeyPressEventHandler(Num_KeyPress);
					if (!winFormItem.Format.IsEmpty())
					{
						tb.Text = string.Format("{0:" + winFormItem.Format + "}", winFormItem.Value);
						tb.Validating += (sender, e) =>
						{
							TextBox tbo = (sender as TextBox);
							if (tbo == null || !tbo.GetSTDAction("Validating"))
								return;
							else
							{
								WinFormItem wfi = tbo.GetItem();
								tbo.Text = string.Format("{0:" + wfi.Format + "}", tbo.Text.ToObjectOf(wfi.DataType));
							}
						};
					}
				}

			}
			else if (typeof(ListControl).IsInstanceOfType(instance) || instance is CheckBox || instance is TabControl || instance is DateTimePicker)
			{
				instance.TextChanged += new EventHandler(dataChanged);
				instance.KeyDown += Enter_KeyDown;
				ListControl lc = (instance as ListControl);
				if (lc != null)
				{
					lc.SelectedValueChanged += new EventHandler(dataChanged);
				}
			}
			else if (instance is ICSharpCode.TextEditor.TextEditorControlBase)
			{
				instance.TextChanged += new EventHandler(dataChanged);
			}
			else if (instance is TableLayoutPanel && parent is TableLayoutPanel)
			{
				TableLayoutPanel tp = instance as TableLayoutPanel;
				TableLayoutPanel tparent = parent as TableLayoutPanel;

				tparent.RowCount++;
				tp.CellBorderStyle = WinForm.CellBorderStyle;
				tp.Dock = DockStyle.Fill;
				tp.AutoSize = true;
				if (winFormItem.CellsSize != null)
				{
					for (int i = 0; i < winFormItem.CellsSize.X; i++)
					{
						tp.ColumnCount++;
						tp.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));

					}
					for (int i = 0; i < winFormItem.CellsSize.Y; i++)
					{
						tp.RowCount++;
						tp.RowStyles.Add(new RowStyle(SizeType.AutoSize, WinForm.CellsRowHieght));
					}
				}
			}

			if (cellAddress == null)
				parent.Controls.Add(instance);
			else
				(parent as TableLayoutPanel)?.Controls.Add(instance, cellAddress.X, cellAddress.Y);
			winFormItem.ControlTrigger?.Invoke(winFormItem, instance);
			foreach (WinFormItem item in winFormItem)
			{
				if (item.PresentationType != null && typeof(Control).IsAssignableFrom(item.PresentationType))
					AddInternal(instance, item);
			}
			if (winFormItem.ReadOnly)
				instance.Enabled = false;
			parent.ResumeLayout();
			wf.OnItemAdded(winFormItem);
			return instance;
		}
		static void Enter_KeyDown(object sender, KeyEventArgs e)
		{
			if (!(sender as Control).GetSTDAction("KeyDown"))
				return;
			if (e.KeyCode == Keys.Enter)
			{
				SendKeys.Send("{TAB}");
				e.SuppressKeyPress = true;
			}
		}
		static void CmdTB_Click(object sender, EventArgs e)
		{
			TextBuX cmd = (sender as TextBuX);
			cmd.GetOwner().OnItemButtonClick(cmd.GetItem());
		}
		static void TextBox_GotFocus(object sender, EventArgs e)
		{
			TextBoxBase tb = (sender as TextBoxBase);
			if (tb != null && tb.GetSTDAction("GotFocus"))
			{
				tb.SelectionStart = 0;
				tb.SelectionLength = tb.Text.Length;
			}
		}
		static void Num_KeyPress(object sender, KeyPressEventArgs e)
		{
			TextBoxBase tb = (sender as TextBoxBase);
			if (tb == null) { }
			else if ("-0123456789".IndexOf(e.KeyChar) > -1) { }
			else if ((e.KeyChar == '.' || e.KeyChar == ',') && tb.Text.IndexOf(',') == -1) e.KeyChar = ',';
			else if (e.KeyChar != 8) e.KeyChar = char.MinValue;
		}
		static bool IsSimpleType(Type type)
		{
			if (type == typeof(string))
				return true;
			else if (type.IsNumeric(NumericTypesScope.All))
				return true;
			else if (type == typeof(DateTime))
				return true;
			return false;
		}
		public static WinFormItem GetItem(this Control c)
		{
			return (c.Tag as WinFormRef)?.WinFormItem;
		}
		public static WinForm GetOwner(this Control c)
		{
			if (c is WinForm)
				return (WinForm)c;

			WinFormRef r = (c.Tag as WinFormRef);
			if (r == null)
				return c.FindForm() as WinForm;
			return r.WinForm;
		}
		public static bool GetSTDAction(this Control c, string eventName)
		{
			WinFormRef r = (c.Tag as WinFormRef);
			if (r == null) return true;
			return !r.WinFormItem.DisabledSTDActions.Contains(eventName);

		}

		public static void SetCell(this Control c, int column, int row)
		{
			(c.Parent as TableLayoutPanel)?.SetCellPosition(c, new TableLayoutPanelCellPosition(column, row));
		}
	}
}
