using System.Drawing;
using System.Windows.Forms;
using Newtonsoft.Json.Linq;
using System.IO;
using System;
using S031.MetaStack.WinForms.ORM;
using System.Reflection;
using S031.MetaStack.WinForms;

namespace MetApp
{
	internal sealed partial class MainForm : WinForm
	{
		void SetCommands()
		{
			//_commands[DBBrowseCommandsEnum.FileOpen] = cmdFileOpen;
			//_commands[DBBrowseCommandsEnum.FileOpenRelated] = cmdFileOpenRelated;
			_commands[DBBrowseCommandsEnum.FileClose] = this.Close;
			_commands[DBBrowseCommandsEnum.FileExit] = Application.Exit;
			_commands[DBBrowseCommandsEnum.FilePeriod] = _dateStart.Focus;
			//_commands[DBBrowseCommandsEnum.FilePrintCurrentForm] = () => ReportManager.PrintCurrentForm(dbGrid);
			//_commands[DBBrowseCommandsEnum.FilePrint] = cmdFilePrint;
			//_commands[DBBrowseCommandsEnum.FileExportObjects] = cmdFileExportObjects;
			//_commands[DBBrowseCommandsEnum.FileExportResults] = cmdFileExportResults;
			//_commands[DBBrowseCommandsEnum.FileImportObjects] = cmdFileImportObjects;

			//_commands[DBBrowseCommandsEnum.EditFind] = () => dbGrid.OnFindFirst(new NewbankEventArgs());
			//_commands[DBBrowseCommandsEnum.EditFindNext] = () => dbGrid.OnFindNext(new NewbankEventArgs());
			//_commands[DBBrowseCommandsEnum.EditRefresh] = () => dbGrid.Reload();
			//_commands[DBBrowseCommandsEnum.EditQuery] = () => dbGrid.Query();
			//_commands[DBBrowseCommandsEnum.EditFilter] = () => dbGrid.OnFilter(new NewbankEventArgs());
			//_commands[DBBrowseCommandsEnum.EditEdit] = () => dbGrid.OnObjectEdit(new NewbankEventArgs(dbGrid.ReadObject(false)) { ActionID = dbGrid.GetAction(2) });
			//_commands[DBBrowseCommandsEnum.EditNew] = () => dbGrid.OnObjectAdd(new NewbankEventArgs(dbGrid.ReadObject(true)) { ActionID = dbGrid.GetAction(1) });
			//_commands[DBBrowseCommandsEnum.EditCopy] = () => dbGrid.OnObjectCopy(new NewbankEventArgs(dbGrid.ReadObject(false)));
			//_commands[DBBrowseCommandsEnum.EditCut] = () => dbGrid.OnObjectCut(new NewbankEventArgs(dbGrid.ReadObject(false)));
			//_commands[DBBrowseCommandsEnum.EditPaste] = () => dbGrid.OnObjectPaste(new NewbankEventArgs());
			//_commands[DBBrowseCommandsEnum.EditDelete] = () => dbGrid.OnObjectDel(new NewbankEventArgs());
			//_commands[DBBrowseCommandsEnum.EditDoSelected] = () => dbGrid.DoSelected();
			//_commands[DBBrowseCommandsEnum.EditSelectAll] = () => dbGrid.SelectAll();
			//_commands[DBBrowseCommandsEnum.EditStat] = () => DBGridCmd.Statistics(dbGrid);
			//_commands[DBBrowseCommandsEnum.EditCopyCell] = () => dbGrid.CopyCellValue();

			//_commands[DBBrowseCommandsEnum.ToolsUpdate] = cmdLoadAssembly;
			//_commands[DBBrowseCommandsEnum.ToolsServerRestart] = cmdServerRestart;
			//_commands[DBBrowseCommandsEnum.ToolsRates] = cmdToolsRates;
			//_commands[DBBrowseCommandsEnum.ToolsLoadRates] = cmdLoadRates;
			//_commands[DBBrowseCommandsEnum.ToolsFile] = cmdFiles;
			//_commands[DBBrowseCommandsEnum.ToolsShowXML] = cmdShowXML;
			//_commands[DBBrowseCommandsEnum.ToolsReportCurrentForm] = () => ReportManager.DesignCurrentForm(dbGrid);
			//_commands[DBBrowseCommandsEnum.ToolsReportDesigner] = () => ReportManager.Design(dbGrid);
			//_commands[DBBrowseCommandsEnum.ToolsRunFromAll] = cmdRunFromAll;

			_commands[DBBrowseCommandsEnum.FavoritesAdd] = cmdFavoritesAdd;
			//_commands[DBBrowseCommandsEnum.FavoritesEdit] = cmdFavoritesEdit;

			_commands[DBBrowseCommandsEnum.HelpAbout] = () => MessageBox.Show(
				$"{Program.AppName} версия: {System.Reflection.Assembly.GetAssembly(typeof(MainForm)).GetName().Version}","", 
				MessageBoxButtons.OK, 
				MessageBoxIcon.Information);
		}
		void cmdFavoritesAdd()
		{
			//Favorites.Add(dbGrid.Form["FormName"], this.Caption);
			showFavorites();
		}

		void showFavorites()
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
	}
}
