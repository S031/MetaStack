using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using System.Linq;
using ICSharpCode.TextEditor.Document;
using ICSharpCode.TextEditor.Gui.CompletionWindow;
using System.Windows.Forms;
using System.Drawing;
using S031.MetaStack.Common;

namespace S031.MetaStack.WinForms
{
	public  static partial class WinFormConfig
	{
		public static WinFormItem SinglePageForm =>
			new WinFormItem("MainPanel",
				new WinFormItem("FormRowsPanel")
				{
					PresentationType = typeof(TableLayoutPanel),
					ControlTrigger = (i, c) =>
					{
						TableLayoutPanel tlpRows = (c as TableLayoutPanel);
						tlpRows.AutoScroll = true;
						tlpRows.Dock = DockStyle.Fill;
						tlpRows.CellBorderStyle = WinForm.CellBorderStyle;
						tlpRows.ColumnCount = 1;
						tlpRows.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
						tlpRows.ControlAdded += (ctrl, e) =>
						{
							(ctrl as TableLayoutPanel)?.RowStyles.Add(new RowStyle(SizeType.AutoSize, 100));
						};
					}
				})
			{
				PresentationType = typeof(Panel),
				ControlTrigger = (i, c) =>
				{
					Panel mainPanel = (c as Panel);
					//mainPanel.AutoScroll = true;
					mainPanel.Dock = DockStyle.Fill;
					mainPanel.BorderStyle = BorderStyle.FixedSingle;
				}
			};
		public static WinFormItem StdButtons(string OKCaption = "&Запись", string CancelCaption = "&Отмена") =>
			new WinFormItem("FormRowsBottomPanel",
				new WinFormItem("StdButtonsCells",
					new WinFormItem("OK")
					{
						PresentationType = typeof(Button),
						Value = OKCaption,
						CellAddress = new Pair<int>(1, 0),
						ControlTrigger = (i, c) =>
						{
							Button btn = (c as Button);
							btn.Height = WinForm.ButtonHeight;
							btn.Width = WinForm.ButtonWidth;
							btn.Click += (o, e) =>
							  {
								  Button b = (o as Button);
								  if (b.GetSTDAction("click"))
								  {
									  WinForm parent = b.Tag().WinForm;
									  if (!parent.ContinuousEditing)
									  {
										  parent.DialogResult = DialogResult.OK;
										  if (!parent.Modal) parent.Close();
									  }

								  }
							  };
							btn.Tag().WinForm.AcceptButton = btn;
						}
					},
					new WinFormItem("Cancel")
					{
						PresentationType = typeof(Button),
						Value = CancelCaption,
						CellAddress = new Pair<int>(2, 0),
						ControlTrigger = (i, c) =>
						{
							Button btn = (c as Button);
							btn.Height = WinForm.ButtonHeight;
							btn.Width = WinForm.ButtonWidth;
							btn.Click += (o, e) =>
							{
								Button b = (o as Button);
								if (b.GetSTDAction("click"))
								{
									WinForm parent = b.Tag().WinForm;
									parent.DialogResult = DialogResult.Cancel;
									if (!parent.Modal) parent.Close();

								}
							};
							btn.Tag().WinForm.CancelButton = btn;
						}
					})
				{
					PresentationType = typeof(TableLayoutPanel),
					CellsSize = new Pair<int>(3, 1),
					ControlTrigger = (i, c) =>
					{
						TableLayoutPanel cells = (c as TableLayoutPanel);
						cells.Dock = DockStyle.Bottom;
						cells.CellBorderStyle = WinForm.CellBorderStyle;
						cells.ColumnStyles[0] = new ColumnStyle(SizeType.Percent, 100);
						cells.ColumnStyles[1] = new ColumnStyle(SizeType.Absolute, WinForm.ButtonWidth);
						cells.ColumnStyles[2] = new ColumnStyle(SizeType.Absolute, WinForm.ButtonWidth);
						cells.RowStyles[0] = new RowStyle(SizeType.Absolute, WinForm.ButtonHeight);
						cells.ControlAdded += (ctrl, e) =>
						{
							(ctrl as TableLayoutPanel)?.RowStyles.Add(new RowStyle(SizeType.AutoSize, 100));
						};
					}
				})
			{
				PresentationType = typeof(TableLayoutPanel),
				CellsSize = new Pair<int>(1, 1),
				ControlTrigger = (i, c) =>
				{
					TableLayoutPanel tlpRows = (c as TableLayoutPanel);
					tlpRows.Dock = DockStyle.Bottom;
					tlpRows.CellBorderStyle = WinForm.CellBorderStyle;
					tlpRows.ColumnCount = 1;
					tlpRows.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
					tlpRows.ControlAdded += (ctrl, e) =>
					{
						(ctrl as TableLayoutPanel)?.RowStyles.Add(new RowStyle(SizeType.AutoSize, 100));
					};
					tlpRows.Height = WinForm.ButtonHeight + 20;
				}
			};
	}
}
