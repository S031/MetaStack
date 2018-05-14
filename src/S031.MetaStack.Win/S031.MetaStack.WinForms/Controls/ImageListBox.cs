using System;
using System.Drawing;
using System.Windows.Forms;

namespace S031.MetaStack.WinForms
{

	public class ImageListBox : ListBox
	{
		private const int imageWidthDefault = 16;
		private const int shift = 8;

		int imgWidth;

		//ToolTip toolTip = new ToolTip();

		private int imageWidth
		{
			get { return imgWidth == 0 ? imageWidthDefault : imgWidth; }
		}

		public ImageListBox()
		{
			this.DrawMode = DrawMode.OwnerDrawFixed;
			this.ItemHeight = imageWidth + 1;
		}

		//protected override void OnMouseMove(MouseEventArgs e)
		//{
		//    base.OnMouseMove(e);
		//    Point point = new Point(e.X, e.Y); 
		//    int hoverIndex = this.IndexFromPoint(point); 
		//    if (hoverIndex >= 0 && hoverIndex < this.Items.Count) 
		//    {
		//        toolTip.Show(this.Items[hoverIndex].ToString(), this.FindForm(), 1000);
		//    }
		//}

		protected override void OnDrawItem(DrawItemEventArgs e)
		{
			e.DrawBackground();
			e.DrawFocusRectangle();

			if (e.Index < 0)
				e.Graphics.DrawString(this.Text, e.Font, new SolidBrush(e.ForeColor), e.Bounds.Left + this.imageWidth + shift, e.Bounds.Top + (this.ItemHeight - this.FontHeight) / 2);
			else if (this.Items.Count > 0)
			{
				if (this.Items[e.Index].GetType() == typeof(ImageComboItem))
				{
					ImageComboItem item = (ImageComboItem)this.Items[e.Index];
					Color forecolor = (item.ForeColor != Color.FromKnownColor(KnownColor.Transparent)) ? item.ForeColor : e.ForeColor;
					Font font = item.Mark ? new Font(e.Font, FontStyle.Bold) : e.Font;

					if (!string.IsNullOrEmpty(item.ImageIndex))
					{
						Image img = ResourceManager.GetImage(item.ImageIndex);
						if (imgWidth == 0)
						{
							imgWidth = img.Size.Width;
							this.ItemHeight = Math.Max(this.FontHeight + 1, img.Size.Height + 1);
						}
						e.Graphics.DrawImage(img, e.Bounds.Left, e.Bounds.Top);
						e.Graphics.DrawString(item.Text, font, new SolidBrush(forecolor), e.Bounds.Left + this.imageWidth + shift, e.Bounds.Top + (this.ItemHeight - this.FontHeight) / 2);
					}
					else
						e.Graphics.DrawString(item.Text, font, new SolidBrush(forecolor), e.Bounds.Left + this.imageWidth + shift, e.Bounds.Top + (this.ItemHeight - this.FontHeight) / 2);
				}
				else
					e.Graphics.DrawString(this.Items[e.Index].ToString(), e.Font, new SolidBrush(e.ForeColor), e.Bounds.Left + this.imageWidth + shift, e.Bounds.Top + (this.ItemHeight - this.FontHeight) / 2);

			}
			base.OnDrawItem(e);
		}
	}

	public class ImageComboItem : object
	{

		// constructors
		public ImageComboItem()
			: this(null, null, string.Empty, false, Color.FromKnownColor(KnownColor.Transparent), null){}

		public ImageComboItem(string text)
			: this(text, null, string.Empty, false, Color.FromKnownColor(KnownColor.Transparent), null) { }

		public ImageComboItem(string text, string key)
			: this(text, key, string.Empty, false, Color.FromKnownColor(KnownColor.Transparent), null) { }

		public ImageComboItem(string text, string key, string imageIndex)
			: this(text, key, imageIndex, false, Color.FromKnownColor(KnownColor.Transparent), null) { }

		public ImageComboItem(string text, string key, string imageIndex, bool mark)
			: this(text, key, imageIndex, mark, Color.FromKnownColor(KnownColor.Transparent), null) { }

		public ImageComboItem(string text, string key, string imageIndex, bool mark, Color foreColor)
			: this(text, key, imageIndex, mark, foreColor, null) { }

		public ImageComboItem(string text, string key, string imageIndex, bool mark, Color foreColor, object tag)
		{
			this.Text = text;
			this.Key = key;
			this.ImageIndex = imageIndex;
			this.Mark = mark;
			this.ForeColor = foreColor;
			this.Tag = tag;
		}

		public Color ForeColor { get; set; }

		public string ImageIndex { get; set; }

		public bool Mark { get; set; }

		public object Tag { get; set; }
		
		public string Text { get; set; }

		public string Key { get; set; }
		
		public override string ToString()
		{
			return this.Text;
		}

	}
}