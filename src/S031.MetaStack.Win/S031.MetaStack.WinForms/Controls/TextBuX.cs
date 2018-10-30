using System;
using System.Drawing;
using System.Windows.Forms;

namespace S031.MetaStack.WinForms
{
	public class TextBuX : Control
	{
		private const int buttonWidth = 30;
		private TextBox _tb;
		private Button _btn;

		public override string Text
		{
			get
			{
				return _tb.Text;
			}
			set
			{
				_tb.Text = value;
			}
		}
		public override string ToString()
		{
			return _tb.Text;
		}


		public TextBuX()
		{
			_tb = new TextBox
			{
				Name = "TextBox",
				Margin = new Padding(0, 0, 0, 0),
				Dock = DockStyle.Left
			};
			_tb.TextChanged += new EventHandler((o, e) => this.OnTextChanged(e));
			_tb.KeyDown += new KeyEventHandler((o, e) =>
				{
					if (e.KeyCode == Keys.Enter)
						SendKeys.Send("{TAB}");
					this.OnKeyDown(e);
				});

			_btn = new Button
			{
				Name = "Button",
				Width = buttonWidth,
				Margin = new Padding(0, 0, 0, 0),
				Text = "...",
				Dock = DockStyle.Right
			};
			_btn.Click += new EventHandler((o, e) => { this.OnClick(e); });
			_btn.TextChanged += new EventHandler(TextBuX_Resize);

			this.Controls.Add(_tb);
			this.Controls.Add(_btn);
			this.Resize += new EventHandler(TextBuX_Resize);
			this.ResumeLayout(false);
			this.PerformLayout();
		}
		public TextBox TextBox
		{
			get { return _tb; }
		}

		public Button Button
		{
			get { return _btn; }
		}

		public HorizontalAlignment TextAlign
		{
			get { return _tb.TextAlign; }
			set { _tb.TextAlign = value; }
		}

		void TextBuX_Resize(object sender, EventArgs e)
		{
			using (Graphics g = this.CreateGraphics())
			{
				_btn.Width = (int)(g.MeasureString
					(_btn.Text, _btn.Font)).Width + 15;
			}
			this.Height = _tb.Height;
			_tb.Size = new Size(this.Width - _btn.Width - 2, this.Height);
		}

		protected override void Dispose(bool disposing)
		{
			_tb.Dispose();
			_btn.Dispose();
			base.Dispose(disposing);
		}

		protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
		{
			if (keyData == (Keys.Tab | Keys.Shift))
			{
				if (_tb.Focused)
				{
					this.Parent.SelectNextControl(this, false, true, true, true);
					return true;
				}
				else
				{
					_tb.Focus();
					return true;
				}

			}
			return base.ProcessCmdKey(ref msg, keyData);
		}

		protected override void OnGotFocus(EventArgs e)
		{
			//base.OnGotFocus(e);
			_tb.Focus();
		}
	}
}
