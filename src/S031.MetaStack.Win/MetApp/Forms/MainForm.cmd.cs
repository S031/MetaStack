﻿using System.Windows.Forms;
using System.Linq;
using S031.MetaStack.WinForms;
using System.Collections.Generic;

namespace MetApp
{
	internal sealed partial class MainForm : WinForm
	{
		void SetCommands()
		{
			//_commands[DBBrowseCommandsEnum.FileOpen] = cmdFileOpen;
			//_commands[DBBrowseCommandsEnum.FileOpenRelated] = cmdFileOpenRelated;
			_commands[DBBrowseCommandsEnum.FileClose] = this.Close;
			_commands[DBBrowseCommandsEnum.FileExit] = () =>
				{
					OutputWindow.Exit();
					Application.Exit();
				};
			_commands[DBBrowseCommandsEnum.FilePeriod] = _dateStart.Focus;
			_commands[DBBrowseCommandsEnum.FilePrintCurrentForm] = () => ReportManager.PrintCurrentForm(_grid);
			//_commands[DBBrowseCommandsEnum.FilePrint] = cmdFilePrint;
			//_commands[DBBrowseCommandsEnum.FileExportObjects] = cmdFileExportObjects;
			_commands[DBBrowseCommandsEnum.FileExportResults] = CmdFileExportResults;
			//_commands[DBBrowseCommandsEnum.FileImportObjects] = cmdFileImportObjects;

			_commands[DBBrowseCommandsEnum.EditFind] = () => _grid.OnFindFirst(new ActionEventArgs());
			_commands[DBBrowseCommandsEnum.EditFindNext] = () => _grid.OnFindNext(new ActionEventArgs());
			_commands[DBBrowseCommandsEnum.EditRefresh] = () => _grid.Reload();
			//_commands[DBBrowseCommandsEnum.EditQuery] = () => dbGrid.Query();
			//_commands[DBBrowseCommandsEnum.EditFilter] = () => dbGrid.OnFilter(new NewbankEventArgs());
			//_commands[DBBrowseCommandsEnum.EditEdit] = () => dbGrid.OnObjectEdit(new NewbankEventArgs(dbGrid.ReadObject(false)) { ActionID = dbGrid.GetAction(2) });
			//_commands[DBBrowseCommandsEnum.EditNew] = () => dbGrid.OnObjectAdd(new NewbankEventArgs(dbGrid.ReadObject(true)) { ActionID = dbGrid.GetAction(1) });
			//_commands[DBBrowseCommandsEnum.EditCopy] = () => dbGrid.OnObjectCopy(new NewbankEventArgs(dbGrid.ReadObject(false)));
			//_commands[DBBrowseCommandsEnum.EditCut] = () => dbGrid.OnObjectCut(new NewbankEventArgs(dbGrid.ReadObject(false)));
			//_commands[DBBrowseCommandsEnum.EditPaste] = () => dbGrid.OnObjectPaste(new NewbankEventArgs());
			//_commands[DBBrowseCommandsEnum.EditDelete] = () => dbGrid.OnObjectDel(new NewbankEventArgs());
			_commands[DBBrowseCommandsEnum.EditDoSelected] = () => _grid.FilterSelected(_grid.IdColName);
			_commands[DBBrowseCommandsEnum.EditSelectAll] = () => _grid.SelectAll();
			//_commands[DBBrowseCommandsEnum.EditStat] = () => DBGridCmd.Statistics(dbGrid);
			_commands[DBBrowseCommandsEnum.EditCopyCell] = () => _grid.CopyCellValue();

			//_commands[DBBrowseCommandsEnum.ToolsUpdate] = cmdLoadAssembly;
			//_commands[DBBrowseCommandsEnum.ToolsServerRestart] = cmdServerRestart;
			//_commands[DBBrowseCommandsEnum.ToolsRates] = cmdToolsRates;
			//_commands[DBBrowseCommandsEnum.ToolsLoadRates] = cmdLoadRates;
			//_commands[DBBrowseCommandsEnum.ToolsFile] = cmdFiles;
			//_commands[DBBrowseCommandsEnum.ToolsShowXML] = cmdShowXML;
			//_commands[DBBrowseCommandsEnum.ToolsReportCurrentForm] = () => ReportManager.DesignCurrentForm(dbGrid);
			//_commands[DBBrowseCommandsEnum.ToolsReportDesigner] = () => ReportManager.Design(dbGrid);
			//_commands[DBBrowseCommandsEnum.ToolsRunFromAll] = cmdRunFromAll;

			_commands[DBBrowseCommandsEnum.FavoritesAdd] = CmdFavoritesAdd;
			//_commands[DBBrowseCommandsEnum.FavoritesEdit] = cmdFavoritesEdit;

			_commands[DBBrowseCommandsEnum.HelpAbout] = () => MessageBox.Show(
				$"{Program.AppName} версия: {System.Reflection.Assembly.GetAssembly(typeof(MainForm)).GetName().Version}","", 
				MessageBoxButtons.OK, 
				MessageBoxIcon.Information);
		}

		void CmdFavoritesAdd()
		{
			//Favorites.Add(dbGrid.Form["FormName"], this.Caption);
			ShowFavorites();
		}

		void ShowFavorites()
		{
			ToolStripMenuItem mi = (this.GetControl<MenuStrip>("MenuBar").Items["Favorites"] as ToolStripMenuItem);
			mi.DropDownItems.Clear();
			mi.DropDownItems.AddRange(new ToolStripItem[] {
				new ToolStripMenuItem("Добавить в избранные...", ResourceManager.GetImage("FavoritesAdd"), MenuClick) { Name = "FavoritesAdd", Tag = DBBrowseCommandsEnum.FavoritesAdd },
				new ToolStripMenuItem("Управление избранным...", ResourceManager.GetImage("Favorites"), MenuClick) { Name = "FavoritesEdit", Tag = DBBrowseCommandsEnum.FavoritesEdit },
				new ToolStripSeparator() { Name = "FavoritesSep1" }
			});

			//using (DataTable dt = Favorites.Read())
			//{
			//	foreach (DataRow dr in dt.Rows)
			//	{
			//		mi.DropDownItems.Add(new ToolStripMenuItem((string)dr["Caption"], null, favorites_Click) { Name = "Favorites_" + (string)dr["FormName"], Tag = (string)dr["FormName"] });
			//	}
			//}
		}

		void CmdFileExportResults()
		{
			string fileName = _grid.Schema.Name.Replace(".", "-");
			ReportExportFormat format = ReportManager.GetExportReportFormat(fileName, ReportExportFormats.Formats.First().Value.ID);
			if (format != null)
			{
				format.CreateParam.Sign = false; //!!! (_grid.Form.GetProperty("Sign").ToIntOrDefault() != 0);
				ReportManager.ExportCurrentForm(_grid, format);
			}
		}
	}
}
