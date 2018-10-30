using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using System.Threading;
using S031.MetaStack.WinForms.Data;

namespace S031.MetaStack.WinForms
{
	public delegate void BackWorkProc();

	public partial class TimeWaitDialog : Form
	{
		private BackWorkProc proc;
		int seconds;

		public Connectors.TCPConnectorException Error { get; private set; }

		public TimeWaitDialog(string text, int milliseconds)
		{
			InitializeComponent();
			if (!string.IsNullOrEmpty(text)) this.Text = text;
			progressBar1.Maximum = milliseconds;
		}
		
		public void Show(Control owner, BackWorkProc tsDelegate)
		{
			Point screenPoint = owner.PointToScreen(owner.Location);
			this.Location = new Point(screenPoint.X - owner.Location.X + owner.Size.Width / 2 - this.Size.Width / 2,
				screenPoint.Y - owner.Location.Y + owner.Size.Height / 2 - this.Size.Height / 2);
			this.Size = new Size(298, 184);
			this.Show(owner);
			timer1.Start();
			proc = tsDelegate;
			backWorker.RunWorkerAsync();
			for (; backWorker.IsBusy; )
			{
				Application.DoEvents();
			}
			this.Close();
		}

		private void timer1_Tick(object sender, EventArgs e)
		{
			if (progressBar1.Value+timer1.Interval > progressBar1.Maximum)
				progressBar1.Value = 0;
			progressBar1.Value += timer1.Interval;
			seconds++;
			if (seconds % 10 == 0)
			{
				lblTime.Text = "Время " + (seconds / 10).ToString();
			}

		}
		
		protected override void OnClosed(EventArgs e)
		{
			base.OnClosed(e);
			if (backWorker.IsBusy)
				backWorker.CancelAsync();
		}

		private void TimeWaitDialog_FormClosing(object sender, FormClosingEventArgs e)
		{
			progressBar1.Value = progressBar1.Maximum;
		}

		private void BackWorker_DoWork(object sender, System.ComponentModel.DoWorkEventArgs e)
		{
			proc();
		}

		private void BackWorker_RunWorkerCompleted(object sender, System.ComponentModel.RunWorkerCompletedEventArgs e)
		{
			if (e.Error != null)
			{
				Error = new Connectors.TCPConnectorException(new DataPackage(DataPackage.CreateErrorPackage(e.Error)));
			}
		}

	}
}
