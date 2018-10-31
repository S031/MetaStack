using S031.MetaStack.Common;
using S031.MetaStack.WinForms;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using Newtonsoft.Json.Linq;
using System.IO;
using System;
using S031.MetaStack.WinForms.ORM;
using System.Reflection;
using System.ComponentModel;
using System.Collections.Generic;

namespace MetApp
{
	internal sealed partial class MainForm : WinForm
	{
		CommandExecuter<DBBrowseCommandsEnum> _commands = new CommandExecuter<DBBrowseCommandsEnum>();
		private int _curRowIdx = 0;
		private DBGrid _grid;
		private string _objectName;

		ToolStripDateEdit _dateStart;
		ToolStripDateEdit _dateFinish;

		public MainForm(string startupForm) : base(WinFormStyle.Form)
		{
			_objectName = startupForm;
			InitializeComponent();
			SetCommands();
			loadActions();
			loadRelated();
		}
		private void InitializeComponent()
		{
			FontManager.SetBaseFontSize(12f);
			this.SuspendLayout();
			//main 
			this.Text = Program.AppName;
			this.Icon = ResourceManager.GetIcon("metib_logo", new Size(16, 16), Assembly.GetExecutingAssembly());
			this.Width = 1000;
			this.Height = (int)(this.Width / vbo.GoldenRatio2);

			this.Add<Panel>(WinFormConfig.SinglePageForm);
			TableLayoutPanel tlpRows = this.Items["FormRowsPanel"].LinkedControl as TableLayoutPanel;
			tlpRows.Add<TableLayoutPanel>(
				new WinFormItem("WorkCells",
					SetMenuBar(),
					SetToolbar(),
					SetGrid()
				)
			);
			this.ResumeLayout();
		}

		void loadActions()
		{
			ToolStripMenuItem menuRun = (ToolStripMenuItem)_grid.ContextMenuStrip.Items["EditRun"];
			ToolStripDropDownButton tsbRun = (ToolStripDropDownButton)GetControl<ToolStrip>("Toolbar").Items["Run"];
			List<ToolStripMenuItem> items = new List<ToolStripMenuItem>();
			List<ToolStripMenuItem> buttons = new List<ToolStripMenuItem>();
			JMXSchema schema = _grid.Schema;
			foreach (var att in schema.Attributes)
			{
				if (att.DataType == S031.MetaStack.WinForms.Data.MdbType.@object)
				{
					JMXSchema rs = ClientGate.GetObjectSchema(att.ObjectName);
					if (rs.DbObjectType == DbObjectTypes.Action)
					{
						items.Add(new ToolStripMenuItem(att.Name, null, MenuRun_Click) { Name = att.AttribName, ToolTipText = att.Description });
						FontManager.Adop(items[items.Count - 1]);
						buttons.Add(new ToolStripMenuItem(att.Name, null, MenuRun_Click) { Name = att.AttribName, ToolTipText = att.Description });
						FontManager.Adop(buttons[buttons.Count - 1]);

					}
					else if (rs.DbObjectType == DbObjectTypes.View)
					{

					}
				}
			}
			items.Add(new ToolStripMenuItem("Расчет и выгрузки данных по сделкам и денежным потокам", null, MenuRun_Click) { Name = "mib_msfo_CF_Calc", ToolTipText = "Расчет и выгрузки данных по сделкам и денежным потокам" });
			buttons.Add(new ToolStripMenuItem("Расчет и выгрузки данных по сделкам и денежным потокам", null, MenuRun_Click) { Name = "mib_msfo_CF_Calc", ToolTipText = "Расчет и выгрузки данных по сделкам и денежным потокам" });

			//foreach (ActionConfig config in configs)
			//{
			//	if (config.IncludeForms != null && config.IncludeForms.Count() > 0 &&
			//		string.IsNullOrEmpty(config.IncludeForms.FirstOrDefault(fn => fn.Equals(this.Form["FormName"], StringComparison.CurrentCultureIgnoreCase))))
			//		continue;
			//	else if (config.ExcludeForms != null && config.ExcludeForms.Count() > 0 &&
			//		!string.IsNullOrEmpty(config.ExcludeForms.FirstOrDefault(fn => fn.Equals(this.Form["FormName"], StringComparison.CurrentCultureIgnoreCase))))
			//		continue;

			//	if (config.AddContextMenu)
			//	{
			//		items.Add(new ToolStripMenuItem(config.Name, config.Image, menuRun_Click) { Name = config.ActionID, ToolTipText = config.Description });
			//		FontManager.Adop(items[items.Count - 1]);
			//		buttons.Add(new ToolStripMenuItem(config.Name, config.Image, menuRun_Click) { Name = config.ActionID, ToolTipText = config.Description });
			//		FontManager.Adop(buttons[buttons.Count - 1]);
			//	}
			//}
			if (items.Count > 0)
			{
				if (menuRun.DropDownItems.ContainsKey("Blank"))
					menuRun.DropDownItems.Remove(menuRun.DropDownItems["Blank"]);
				menuRun.DropDownItems.AddRange(items.ToArray());
				menuRun.DropDownItems.Add(new ToolStripSeparator());
				tsbRun.DropDown.Items.AddRange(buttons.ToArray());
			}
			if ((menuRun.DropDownItems[menuRun.DropDownItems.Count - 1] is ToolStripSeparator))
				menuRun.DropDownItems.Remove(menuRun.DropDownItems[menuRun.DropDownItems.Count - 1]);
		}
		void loadRelated()
		{
			ToolStripMenuItem menuRun = (ToolStripMenuItem)GetControl<MenuStrip>("MenuBar").Items["OpenRelated"];
			ToolStripDropDownButton tsbRun = (ToolStripDropDownButton)GetControl<ToolStrip>("Toolbar").Items["OpenRelated"];
			List<ToolStripMenuItem> items = new List<ToolStripMenuItem>();
			List<ToolStripMenuItem> buttons = new List<ToolStripMenuItem>();
			JMXSchema schema = _grid.Schema;
			foreach (var att in schema.Attributes)
			{
				if (att.DataType == S031.MetaStack.WinForms.Data.MdbType.@object)
				{
					JMXSchema rs = ClientGate.GetObjectSchema(att.ObjectName);
					if (rs.DbObjectType == DbObjectTypes.View)
					{
						items.Add(new ToolStripMenuItem(att.Name, null, MenuRun_Click) { Name = att.AttribName, ToolTipText = att.Description });
						FontManager.Adop(items[items.Count - 1]);
						buttons.Add(new ToolStripMenuItem(att.Name, null, MenuRun_Click) { Name = att.AttribName, ToolTipText = att.Description });
						FontManager.Adop(buttons[buttons.Count - 1]);

					}
				}
			}
			items.Add(new ToolStripMenuItem("Данные по счетам", null, MenuRun_Click) { Name = "mib_msfo_DealAcc_Select", ToolTipText = "Процедура для вывода данных счетам из таблицы msfodb..DealAcc по одному договору" });
			buttons.Add(new ToolStripMenuItem("Данные по счетам", null, MenuRun_Click) { Name = "mib_msfo_DealAcc_Select", ToolTipText = "Процедура для вывода данных счетам из таблицы msfodb..DealAcc по одному договору" });
			items.Add(new ToolStripMenuItem("Данные по потокам", null, MenuRun_Click) { Name = "mib_msfo_CashFlow_Select", ToolTipText = "Процедура для вывода данных потоков из таблицы msfodb..CashFlow_NKN по одному договору" });
			buttons.Add(new ToolStripMenuItem("Данные по потокам", null, MenuRun_Click) { Name = "mib_msfo_CashFlow_Select", ToolTipText = "Процедура для вывода данных потоков из таблицы msfodb..CashFlow_NKN по одному договору" });

			//foreach (ActionConfig config in configs)
			//{
			//	if (config.IncludeForms != null && config.IncludeForms.Count() > 0 &&
			//		string.IsNullOrEmpty(config.IncludeForms.FirstOrDefault(fn => fn.Equals(this.Form["FormName"], StringComparison.CurrentCultureIgnoreCase))))
			//		continue;
			//	else if (config.ExcludeForms != null && config.ExcludeForms.Count() > 0 &&
			//		!string.IsNullOrEmpty(config.ExcludeForms.FirstOrDefault(fn => fn.Equals(this.Form["FormName"], StringComparison.CurrentCultureIgnoreCase))))
			//		continue;

			//	if (config.AddContextMenu)
			//	{
			//		items.Add(new ToolStripMenuItem(config.Name, config.Image, menuRun_Click) { Name = config.ActionID, ToolTipText = config.Description });
			//		FontManager.Adop(items[items.Count - 1]);
			//		buttons.Add(new ToolStripMenuItem(config.Name, config.Image, menuRun_Click) { Name = config.ActionID, ToolTipText = config.Description });
			//		FontManager.Adop(buttons[buttons.Count - 1]);
			//	}
			//}
			if (items.Count > 0)
			{
				//menuRun.DropDownItems.AddRange(items.ToArray());
				//menuRun.DropDownItems.Add(new ToolStripSeparator());
				tsbRun.DropDown.Items.AddRange(buttons.ToArray());
			}
			//if ((menuRun.DropDownItems[menuRun.DropDownItems.Count - 1] is ToolStripSeparator))
			//	menuRun.DropDownItems.Remove(menuRun.DropDownItems[menuRun.DropDownItems.Count - 1]);
		}

		private WinFormItem SetMenuBar()
		{
			return new WinFormItem("MenuBar")
			{
				CellAddress = new Pair<int>(0, 0),
				PresentationType = typeof(MenuStrip),
				ControlTrigger = (i, c) =>
				{
					var mnuBar = (c as MenuStrip);
					mnuBar.TabIndex = 1;

					mnuBar.Items.AddRange(new ToolStripItem[] {
						new ToolStripMenuItem("Файл") { Name = "File" },
						new ToolStripMenuItem("Редактор") { Name = "Edit" },
						new ToolStripMenuItem("Сервис") { Name = "Tools" },
						new ToolStripMenuItem("Избранные") { Name = "Favorites" },
						new ToolStripMenuItem("Помощь") { Name = "Help" }
					});
					mnuBar.Dock = DockStyle.Top;
					((ToolStripMenuItem)mnuBar.Items["File"]).DropDownItems.AddRange(new ToolStripItem[] {
						new ToolStripMenuItem("Открыть...", ResourceManager.GetImage("Open"), MenuClick, Keys.Control | Keys.O){ Name = "FileOpen", Tag = DBBrowseCommandsEnum.FileOpen },
						new ToolStripMenuItem("Открыть связанную форму...", ResourceManager.GetImage("OpenRelated"), MenuClick, Keys.F7){ Name = "FileOpenRelated", Tag = DBBrowseCommandsEnum.FileOpenRelated },
						new ToolStripMenuItem("Закрыть", null, MenuClick, Keys.Control | Keys.F4){ Name = "FileOpen", Tag = DBBrowseCommandsEnum.FileClose },
						new ToolStripSeparator() { Name = "FileSep1" },
						new ToolStripMenuItem("Расчетный период...", null, MenuClick, Keys.F2){ Name = "FilePeriod", Tag = DBBrowseCommandsEnum.FilePeriod },
						new ToolStripSeparator() { Name = "FileSep2" },
						new ToolStripMenuItem("Экспорт") { Name = "FileExport" },
						new ToolStripMenuItem("Импорт") { Name = "FileImport" },
						new ToolStripSeparator() { Name = "FileSep3" },
						new ToolStripMenuItem("Печать...", ResourceManager.GetImage("Print"), MenuClick, Keys.Control | Keys.P){ Name = "FilePrint", Tag = DBBrowseCommandsEnum.FilePrint },
						new ToolStripMenuItem("Печать формы:", ResourceManager.GetImage("PrintPreview"), MenuClick, Keys.Control | Keys.Shift | Keys.P){ Name = "FilePrintCurrentForm", Tag = DBBrowseCommandsEnum.FilePrintCurrentForm },
						new ToolStripMenuItem("Принтер...", null, MenuClick){ Name = "FilePrinter", Tag = DBBrowseCommandsEnum.FilePrinter },
						new ToolStripSeparator() { Name = "FileSep4" },
						new ToolStripMenuItem("Выход из программы", null, MenuClick, Keys.Alt | Keys.F4){ Name = "FileExit", Tag = DBBrowseCommandsEnum.FileExit }
					});

					(((ToolStripMenuItem)mnuBar.Items["File"]).DropDownItems["FileExport"] as ToolStripMenuItem).DropDownItems.AddRange(new ToolStripItem[] {
						new ToolStripMenuItem("Результаты запроса...", null, MenuClick){ Name = "FileExportResults", Tag = DBBrowseCommandsEnum.FileExportResults },
						new ToolStripMenuItem("В текстовый файл с разделителем...", null, MenuClick){ Name = "FileExportText", Tag = DBBrowseCommandsEnum.FileExportText },
						new ToolStripMenuItem("Объектов в файл...", null, MenuClick){ Name = "FileExportObjects", Tag = DBBrowseCommandsEnum.FileExportObjects }
					});

					(((ToolStripMenuItem)mnuBar.Items["File"]).DropDownItems["FileImport"] as ToolStripMenuItem).DropDownItems.AddRange(new ToolStripItem[] {
						new ToolStripMenuItem("Объектов из файла...", null, MenuClick){ Name = "FileImportObjects", Tag = DBBrowseCommandsEnum.FileImportObjects }
					});

					((ToolStripMenuItem)mnuBar.Items["Edit"]).DropDownItems.AddRange(new ToolStripItem[] {
						new ToolStripMenuItem("Добавить...", ResourceManager.GetImage("New"), MenuClick, Keys.Insert) { Name = "EditNew", Tag = DBBrowseCommandsEnum.EditNew },
						new ToolStripMenuItem("Изменить...", ResourceManager.GetImage("Edit"), MenuClick, Keys.F4) { Name = "EditEdit", Tag = DBBrowseCommandsEnum.EditEdit },
						new ToolStripMenuItem("Удалить", ResourceManager.GetImage("Delete"), MenuClick, Keys.Delete) { Name = "EditDelete", Tag = DBBrowseCommandsEnum.EditDelete },
						new ToolStripSeparator() { Name = "EditSep1" },
						new ToolStripMenuItem("Вырезать", ResourceManager.GetImage("Cut"), MenuClick, Keys.Shift | Keys.Delete) { Name = "EditCut", Tag = DBBrowseCommandsEnum.EditCut },
						new ToolStripMenuItem("Копировать", ResourceManager.GetImage("Copy"), MenuClick, Keys.Control | Keys.Insert) { Name = "EditCopy", Tag = DBBrowseCommandsEnum.EditCopy },
						new ToolStripMenuItem("Вставить", ResourceManager.GetImage("Paste"), MenuClick, Keys.Shift | Keys.Insert) { Name = "EditPaste", Tag = DBBrowseCommandsEnum.EditPaste },
						new ToolStripMenuItem("Сохранить как шаблон...", null, MenuClick, Keys.Control | Keys.T) { Name = "EditCopy2Template", Tag = DBBrowseCommandsEnum.EditCopy2Template },
						new ToolStripMenuItem("Добавить из шаблона...", null, MenuClick, Keys.Control | Keys.V) { Name = "EditPasteFromTemplate", Tag = DBBrowseCommandsEnum.EditPasteFromTemplate },
						new ToolStripMenuItem("Копировать ячейку", ResourceManager.GetImage("Textbox"), MenuClick, Keys.Control | Keys.C) { Name = "EditCopyCell", Tag = DBBrowseCommandsEnum.EditCopyCell },
						new ToolStripSeparator() { Name = "EditSep2" },
						new ToolStripMenuItem("Поиск...", ResourceManager.GetImage("Find"), MenuClick, Keys.Control | Keys.F) { Name = "EditFind", Tag = DBBrowseCommandsEnum.EditFind },
						new ToolStripMenuItem("Поиск далее", null, MenuClick, Keys.F3) { Name = "EditFindNext", Tag = DBBrowseCommandsEnum.EditFindNext },
						new ToolStripMenuItem("Поиск и замена...", null, MenuClick, Keys.Control | Keys.H) { Name = "EditFindReplace", Tag = DBBrowseCommandsEnum.EditFindReplace },
						new ToolStripMenuItem("Фильтр...", ResourceManager.GetImage("Filter"), MenuClick) { Name = "EditFilter", Tag = DBBrowseCommandsEnum.EditFilter },
						new ToolStripMenuItem("Запрос...", null, MenuClick, Keys.Control | Keys.F2) { Name = "EditQuery", Tag = DBBrowseCommandsEnum.EditQuery },
						new ToolStripMenuItem("Обновить форму", ResourceManager.GetImage("Refresh"), MenuClick, Keys.F5) { Name = "EditRefresh", Tag = DBBrowseCommandsEnum.EditRefresh },
						new ToolStripSeparator() { Name = "EditSep3" },
						new ToolStripMenuItem("Выделить все", null, MenuClick, Keys.Control | Keys.A) { Name = "EditSelectAll", Tag = DBBrowseCommandsEnum.EditSelectAll },
						new ToolStripMenuItem("Выбрать помеченные", null, MenuClick, Keys.Control | Keys.Z) { Name = "EditDoSelected", Tag = DBBrowseCommandsEnum.EditDoSelected },
						new ToolStripSeparator() { Name = "EditSep4" },
						new ToolStripMenuItem("Статистика...", null, MenuClick, Keys.Control | Keys.S) { Name = "EditStat", Tag = DBBrowseCommandsEnum.EditStat }
					});

					((ToolStripMenuItem)mnuBar.Items["Tools"]).DropDownItems.AddRange(new ToolStripItem[] {
						new ToolStripMenuItem("Курсы валют...", ResourceManager.GetImage("Rate"), MenuClick) { Name = "ToolsRates", Tag = DBBrowseCommandsEnum.ToolsRates },
						new ToolStripMenuItem("Загрузка курсов валют из интернет...", null, MenuClick) { Name = "ToolsLoadRates", Tag = DBBrowseCommandsEnum.ToolsLoadRates },
						new ToolStripSeparator() { Name = "ToolsSep1" },
						//new ToolStripMenuItem("Выполнить процедуру...", null, mnuClick, Keys.F9) { Name = "ToolsRunFromAll", Tag = DBBrowseCommandsEnum.ToolsRunFromAll,Visible = false },
						new ToolStripMenuItem("Выполнить процедуру из списка...", ResourceManager.GetImage("Eval"), MenuClick, Keys.Shift | Keys.F9) { Name = "ToolsRunFromAll", Tag = DBBrowseCommandsEnum.ToolsRunFromAll },
						new ToolStripSeparator() { Name = "ToolsSep2" },
						new ToolStripMenuItem("Загрузка обновлений программы...", null, MenuClick) { Name = "ToolsLoadRates", Tag = DBBrowseCommandsEnum.ToolsUpdate },
						//new ToolStripMenuItem("Системный каталог") { Name = "ToolsSysCat" },
						new ToolStripMenuItem("Установки") { Name = "ToolsSettings" },
						new ToolStripSeparator() { Name = "ToolsSep3" },
						new ToolStripMenuItem("Файлы...", ResourceManager.GetImage("Attach"), MenuClick, Keys.F11) { Name = "ToolsFile", Tag = DBBrowseCommandsEnum.ToolsFile },
						new ToolStripSeparator() { Name = "ToolsSep4" },
						new ToolStripMenuItem("Отчеты") { Name = "ToolsReport" }
					}); ;

					(((ToolStripMenuItem)mnuBar.Items["Tools"]).DropDownItems["ToolsReport"] as ToolStripMenuItem).DropDownItems.AddRange(new ToolStripItem[] {
						new ToolStripMenuItem("Редактор отчетов...", null, MenuClick, Keys.Control | Keys.R) { Name = "ToolsReportDesigner", Tag = DBBrowseCommandsEnum.ToolsReportDesigner },
						new ToolStripMenuItem("Создать отчет из текущей формы...", null, MenuClick) { Name = "ToolsReportCurrentForm", Tag = DBBrowseCommandsEnum.ToolsReportCurrentForm }
					});
					(((ToolStripMenuItem)mnuBar.Items["Tools"]).DropDownItems["ToolsSettings"] as ToolStripMenuItem).DropDownItems.AddRange(new ToolStripItem[] {
						new ToolStripMenuItem("Типы установок", null, MenuClick, Keys.Control | Keys.U){ Name = "ToolsSettingsGlobal", Tag = DBBrowseCommandsEnum.ToolsSettingsGlobal },
						new ToolStripMenuItem("Установки пользователя", null, MenuClick){ Name = "ToolsSettingsLocal", Tag = DBBrowseCommandsEnum.ToolsSettingsLocal }
					});

					((ToolStripMenuItem)mnuBar.Items["Help"]).DropDownItems.AddRange(new ToolStripItem[] {
						new ToolStripMenuItem("О программе...", ResourceManager.GetImage("Help"), MenuClick) { Name = "HelpAbout", Tag = DBBrowseCommandsEnum.HelpAbout },
					});
					showFavorites();
				}
			};
		}

		private WinFormItem SetToolbar()
		{
			return new WinFormItem("Toolbar")
			{
				CellAddress = new Pair<int>(0, 1),
				PresentationType = typeof(ToolStrip),
				ControlTrigger = (i, c) =>
				{
					var tbTools = (c as ToolStrip);
					tbTools.Dock = DockStyle.Top;
					tbTools.Items.AddRange(new ToolStripItem[]{
						new ToolStripButton("Открыть",ResourceManager.GetImage("Open"), MenuClick, "Open")
						{ DisplayStyle = ToolStripItemDisplayStyle.Image, ImageTransparentColor = System.Drawing.Color.Magenta, Tag = DBBrowseCommandsEnum.FileOpen },
						new ToolStripDropDownButton("Открыть связанную форму",ResourceManager.GetImage("OpenRelated"), MenuClick, "OpenRelated")
						{ DisplayStyle = ToolStripItemDisplayStyle.Image, ImageTransparentColor = System.Drawing.Color.Magenta, Tag = DBBrowseCommandsEnum.FileOpenRelated },
						new ToolStripSeparator() { Name = "Sep1" },
						new ToolStripButton("Печать отчетов",ResourceManager.GetImage("Print"), MenuClick, "Print")
						{ DisplayStyle = ToolStripItemDisplayStyle.Image, ImageTransparentColor = System.Drawing.Color.Magenta, Tag = DBBrowseCommandsEnum.FilePrint },
						new ToolStripSeparator() { Name = "Sep2" },
						new ToolStripButton("Создать запись",ResourceManager.GetImage("New"), MenuClick, "New")
						{ DisplayStyle = ToolStripItemDisplayStyle.Image, ImageTransparentColor = System.Drawing.Color.Magenta, Tag = DBBrowseCommandsEnum.EditNew },
						new ToolStripButton("Изменить запись",ResourceManager.GetImage("Edit"), MenuClick, "Edit")
						{ DisplayStyle = ToolStripItemDisplayStyle.Image, ImageTransparentColor = System.Drawing.Color.Magenta, Tag = DBBrowseCommandsEnum.EditEdit },
						new ToolStripButton("Удалить запись",ResourceManager.GetImage("Delete"), MenuClick, "Delete")
						{ DisplayStyle = ToolStripItemDisplayStyle.Image, ImageTransparentColor = System.Drawing.Color.Magenta, Tag = DBBrowseCommandsEnum.EditDelete },
						new ToolStripSeparator() { Name = "Sep3" },
						new ToolStripButton("Вырезать",ResourceManager.GetImage("Cut"), MenuClick, "Cut")
						{ DisplayStyle = ToolStripItemDisplayStyle.Image, ImageTransparentColor = System.Drawing.Color.Magenta, Tag = DBBrowseCommandsEnum.EditCut },
						new ToolStripButton("Копировать",ResourceManager.GetImage("Copy"), MenuClick, "Copy")
						{ DisplayStyle = ToolStripItemDisplayStyle.Image, ImageTransparentColor = System.Drawing.Color.Magenta, Tag = DBBrowseCommandsEnum.EditCopy },
						new ToolStripButton("Вставить",ResourceManager.GetImage("Paste"), MenuClick, "Paste")
						{ DisplayStyle = ToolStripItemDisplayStyle.Image, ImageTransparentColor = System.Drawing.Color.Magenta, Tag = DBBrowseCommandsEnum.EditPaste },
						new ToolStripSeparator() { Name = "Sep4" },
						new ToolStripButton("Найти",ResourceManager.GetImage("Find"), MenuClick, "Find")
						{ DisplayStyle = ToolStripItemDisplayStyle.Image, ImageTransparentColor = System.Drawing.Color.Magenta, Tag = DBBrowseCommandsEnum.EditFind },
						new ToolStripButton("Фильтр",ResourceManager.GetImage("Filter"), MenuClick, "Filter")
						{ DisplayStyle = ToolStripItemDisplayStyle.Image, ImageTransparentColor = System.Drawing.Color.Magenta, Tag = DBBrowseCommandsEnum.EditFilter},
						new ToolStripButton("Обновить форму",ResourceManager.GetImage("Refresh"), MenuClick, "Refresh")
						{ DisplayStyle = ToolStripItemDisplayStyle.Image, ImageTransparentColor = System.Drawing.Color.Magenta, Tag = DBBrowseCommandsEnum.EditRefresh},
						new ToolStripSeparator() { Name = "Sep5" },
						new ToolStripDropDownButton("Выполнить команду",ResourceManager.GetImage("Run"), MenuClick, "Run")
						{ DisplayStyle = ToolStripItemDisplayStyle.Image, ImageTransparentColor = System.Drawing.Color.Magenta, Tag = DBBrowseCommandsEnum.ToolsRun},
						new ToolStripButton("Прикрепить файл",ResourceManager.GetImage("Attach"), MenuClick, "Attach")
						{ DisplayStyle = ToolStripItemDisplayStyle.Image, ImageTransparentColor = System.Drawing.Color.Magenta, Tag = DBBrowseCommandsEnum.ToolsFile},
						new ToolStripSeparator() { Name = "Sep6" },
					});

					tbTools.Items.Add(new ToolStripLabel("Период с"));
					_dateStart = new ToolStripDateEdit();
					_dateStart.Size = new Size((int)(tbTools.Font.SizeInPoints * 12), _dateStart.Height);
					_dateStart.ToolTipText = "Дата начала периода";
					tbTools.Items.Add(_dateStart);
					tbTools.Items.Add(new ToolStripLabel("по"));
					_dateFinish = new ToolStripDateEdit();
					_dateFinish.Size = new Size((int)(tbTools.Font.SizeInPoints * 12), _dateFinish.Height);
					_dateFinish.ToolTipText = "Дата окончания периода";
					tbTools.Items.Add(_dateFinish);
					_dateStart.Value = rth.DateStart;
					_dateFinish.Value = rth.DateFinish;
					_dateFinish.Validating += new CancelEventHandler(dateFinish_Validating);
					_dateStart.KeyDown += new KeyEventHandler((sender, e) =>
					{
						if (e.KeyCode == Keys.Enter)
							SendKeys.Send("{TAB}");
					});
					_dateFinish.KeyDown += new KeyEventHandler((sender, e) =>
					{
						if (e.KeyCode == Keys.Enter)
							_grid.Focus();
					});

					ToolStripLabel lbl = new ToolStripLabel(ResourceManager.GetImage("FastFind"))
					{
						ToolTipText = "Быстрый поиск"
					};
					tbTools.Items.Add(lbl);
					ToolStripTextBox txtFind = new ToolStripTextBox("txtFind");
					txtFind.Size = new Size((int)(tbTools.Font.SizeInPoints* 20), txtFind.Height);
					tbTools.Items.Add(txtFind);
				}
			};
		}

		private void SetMenuContext()
		{
			var mnuContext = new ContextMenuStrip();
			mnuContext.Items.AddRange(new ToolStripItem[]{
				new ToolStripMenuItem("Добавить...", ResourceManager.GetImage("New"), MenuClick, Keys.Insert) { Name = "EditNew", Tag = DBBrowseCommandsEnum.EditNew },
				new ToolStripMenuItem("Изменить...", ResourceManager.GetImage("Edit"), MenuClick, Keys.F4) { Name = "EditEdit", Tag = DBBrowseCommandsEnum.EditEdit },
				new ToolStripMenuItem("Удалить", ResourceManager.GetImage("Delete"), MenuClick, Keys.Delete) { Name = "EditDelete", Tag = DBBrowseCommandsEnum.EditDelete },
				new ToolStripMenuItem("Выполнить") { Name = "EditRun" },
				new ToolStripSeparator() { Name = "Sep1" },
				new ToolStripMenuItem("Вырезать", ResourceManager.GetImage("Cut"), MenuClick, Keys.Shift | Keys.Delete) { Name = "EditCut", Tag = DBBrowseCommandsEnum.EditCut },
				new ToolStripMenuItem("Копировать", ResourceManager.GetImage("Copy"), MenuClick, Keys.Control | Keys.Insert) { Name = "EditCopy", Tag = DBBrowseCommandsEnum.EditCopy },
				new ToolStripMenuItem("Вставить", ResourceManager.GetImage("Paste"), MenuClick, Keys.Shift | Keys.Insert) { Name = "EditPaste", Tag = DBBrowseCommandsEnum.EditPaste },
				new ToolStripMenuItem("Шаблон") { Name = "EditTemplate" },
				new ToolStripMenuItem("Копировать ячейку", ResourceManager.GetImage("Textbox"), MenuClick, Keys.Control | Keys.C) { Name = "EditCopyCell", Tag = DBBrowseCommandsEnum.EditCopyCell },
				new ToolStripSeparator() { Name = "Sep2" },
				new ToolStripMenuItem("Поиск...", ResourceManager.GetImage("Find"), MenuClick, Keys.Control | Keys.F) { Name = "EditFind", Tag = DBBrowseCommandsEnum.EditFind },
				new ToolStripMenuItem("Поиск и замена...", null, MenuClick, Keys.Control | Keys.H) { Name = "EditFindReplace", Tag = DBBrowseCommandsEnum.EditFindReplace },
				new ToolStripMenuItem("Фильтр...", ResourceManager.GetImage("Filter"), MenuClick) { Name = "EditFilter", Tag = DBBrowseCommandsEnum.EditFilter },
				new ToolStripMenuItem("Запрос...", null, MenuClick, Keys.Control | Keys.F2) { Name = "EditQuery", Tag = DBBrowseCommandsEnum.EditQuery },
				new ToolStripSeparator() { Name = "Sep3" },
				new ToolStripMenuItem("Выделить все", null, MenuClick, Keys.Control | Keys.A) { Name = "EditSelectAll", Tag = DBBrowseCommandsEnum.EditSelectAll },
				new ToolStripMenuItem("Выбрать помеченные", null, MenuClick, Keys.Control | Keys.Z) { Name = "EditDoSelected", Tag = DBBrowseCommandsEnum.EditDoSelected },
				new ToolStripSeparator() { Name = "Sep4" },
				new ToolStripMenuItem("Статистика...", null, MenuClick, Keys.Control | Keys.S) { Name = "EditStat", Tag = DBBrowseCommandsEnum.EditStat },
			});
			((ToolStripMenuItem)mnuContext.Items["EditTemplate"]).DropDownItems.AddRange(new ToolStripItem[] {
				new ToolStripMenuItem("Сохранить как шаблон...", null, MenuClick, Keys.Control | Keys.T) { Name = "EditCopy2Template", Tag = DBBrowseCommandsEnum.EditCopy2Template },
				new ToolStripMenuItem("Добавить из шаблона...", null, MenuClick, Keys.Control | Keys.V) { Name = "EditPasteFromTemplate", Tag = DBBrowseCommandsEnum.EditPasteFromTemplate },
			});

			((ToolStripMenuItem)mnuContext.Items["EditRun"]).DropDownItems.AddRange(new ToolStripItem[] {
				new ToolStripMenuItem("Нет данных"){Name = "Blank"}
			});
			_grid.ContextMenuStrip = mnuContext;
		}

		void MenuClick(object sender, EventArgs e)
		{
			ToolStripItem mi = (sender as ToolStripItem);
			if (mi != null && mi.Tag is DBBrowseCommandsEnum)
			{
				DBBrowseCommandsEnum cmdID;
				if (mi.Tag.GetType() == typeof(string))
					cmdID = (DBBrowseCommandsEnum)Enum.Parse(typeof(DBBrowseCommandsEnum), (string)mi.Tag);
				else
					cmdID = (DBBrowseCommandsEnum)mi.Tag;
				_commands[cmdID]?.Invoke();
			}
		}
		void MenuRun_Click(object sender, EventArgs e)
		{
		}

		void dateFinish_Validating(object sender, CancelEventArgs e)
		{
			try
			{
				DateTime dateTop = new DateTime(2017, 1, 1);
				if (_dateStart.Value < dateTop)
					throw new ArgumentException("Дата начала периода не может быть ранее " + dateTop.ToString(vbo.DateFormat));
				else if (_dateFinish.Value < _dateStart.Value)
					throw new ArgumentException("Неверно указан временной диапазон!");
				else
				{
					rth.DateStart = _dateStart.Value;
					rth.DateFinish = _dateFinish.Value;
					_grid.Reload();
					this.Text = _grid.Schema.Name.Replace("за_", "за " + rth.Za());
				}
			}
			catch (ArgumentException ex)
			{
				MessageBox.Show(ex.Message, "Установка даты", MessageBoxButtons.OK, MessageBoxIcon.Error);
				_dateStart.Value = rth.DateStart;
				_dateFinish.Value = rth.DateFinish;
				_dateStart.Focus();
			}
		}
	}
}
