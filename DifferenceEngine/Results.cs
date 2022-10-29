using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace DifferenceEngine
{
	/// <summary>
	/// Summary description for Results.
	/// </summary>
	public class Results : Form
	{
		private ListView _lvSource;
		private ColumnHeader _columnHeader1;
		private ColumnHeader _columnHeader2;
		private ListView _lvDestination;
		private ColumnHeader _columnHeader3;
		private ColumnHeader _columnHeader4;
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private Container _components = null;

		public Results(DiffListTextFile source, DiffListTextFile destination, ArrayList diffLines, double seconds)
		{
			InitializeComponent();
			Text = $"Results: {seconds:#0.00} secs.";

			ListViewItem lviS;
			ListViewItem lviD;
			int cnt = 1;
			int i;

			foreach (DiffResultSpan drs in diffLines)
			{				
				switch (drs.Status)
				{
					case DiffResultSpanStatus.DeleteSource:
						for (i = 0; i < drs.Length; i++)
						{
							lviS = new ListViewItem(cnt.ToString("00000"));
							lviD = new ListViewItem(cnt.ToString("00000"));
							lviS.BackColor = Color.Red;
							lviS.SubItems.Add(((TextLine)source.GetByIndex(drs.SourceIndex + i)).Line);
							lviD.BackColor = Color.LightGray;
							lviD.SubItems.Add("");

							_lvSource.Items.Add(lviS);
							_lvDestination.Items.Add(lviD);
							cnt++;
						}
						
						break;
					case DiffResultSpanStatus.NoChange:
						for (i = 0; i < drs.Length; i++)
						{
							lviS = new ListViewItem(cnt.ToString("00000"));
							lviD = new ListViewItem(cnt.ToString("00000"));
							lviS.BackColor = Color.White;
							lviS.SubItems.Add(((TextLine)source.GetByIndex(drs.SourceIndex+i)).Line);
							lviD.BackColor = Color.White;
							lviD.SubItems.Add(((TextLine)destination.GetByIndex(drs.DestIndex+i)).Line);

							_lvSource.Items.Add(lviS);
							_lvDestination.Items.Add(lviD);
							cnt++;
						}
						
						break;
					case DiffResultSpanStatus.AddDestination:
						for (i = 0; i < drs.Length; i++)
						{
							lviS = new ListViewItem(cnt.ToString("00000"));
							lviD = new ListViewItem(cnt.ToString("00000"));
							lviS.BackColor = Color.LightGray;
							lviS.SubItems.Add("");
							lviD.BackColor = Color.LightGreen;
							lviD.SubItems.Add(((TextLine)destination.GetByIndex(drs.DestIndex+i)).Line);

							_lvSource.Items.Add(lviS);
							_lvDestination.Items.Add(lviD);
							cnt++;
						}
						
						break;
					case DiffResultSpanStatus.Replace:
						for (i = 0; i < drs.Length; i++)
						{
							lviS = new ListViewItem(cnt.ToString("00000"));
							lviD = new ListViewItem(cnt.ToString("00000"));
							lviS.BackColor = Color.Red;
							lviS.SubItems.Add(((TextLine)source.GetByIndex(drs.SourceIndex+i)).Line);
							lviD.BackColor = Color.LightGreen;
							lviD.SubItems.Add(((TextLine)destination.GetByIndex(drs.DestIndex+i)).Line);

							_lvSource.Items.Add(lviS);
							_lvDestination.Items.Add(lviD);
							cnt++;
						}
						
						break;
				}
				
			}
		}

        public sealed override string Text
        {
            get { return base.Text; }
            set { base.Text = value; }
        }

        /// <summary>
		/// Clean up any resources being used.
		/// </summary>
		protected override void Dispose( bool disposing )
		{
			if( disposing )
			{
				if(_components != null)
				{
					_components.Dispose();
				}
			}
			base.Dispose( disposing );
		}

		#region Windows Form Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this._lvSource = new System.Windows.Forms.ListView();
			this._columnHeader1 = new System.Windows.Forms.ColumnHeader();
			this._columnHeader2 = new System.Windows.Forms.ColumnHeader();
			this._lvDestination = new System.Windows.Forms.ListView();
			this._columnHeader3 = new System.Windows.Forms.ColumnHeader();
			this._columnHeader4 = new System.Windows.Forms.ColumnHeader();
			this.SuspendLayout();
			// 
			// lvSource
			// 
			this._lvSource.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
																					   this._columnHeader1,
																					   this._columnHeader2});
			this._lvSource.FullRowSelect = true;
			this._lvSource.HideSelection = false;
			this._lvSource.Location = new System.Drawing.Point(28, 17);
			this._lvSource.MultiSelect = false;
			this._lvSource.Name = "_lvSource";
			this._lvSource.Size = new System.Drawing.Size(114, 102);
			this._lvSource.TabIndex = 0;
			this._lvSource.View = System.Windows.Forms.View.Details;
			this._lvSource.Resize += new System.EventHandler(this.lvSource_Resize);
			this._lvSource.SelectedIndexChanged += new System.EventHandler(this.lvSource_SelectedIndexChanged);
			// 
			// columnHeader1
			// 
			this._columnHeader1.Text = "Line";
			this._columnHeader1.Width = 50;
			// 
			// columnHeader2
			// 
			this._columnHeader2.Text = "Text (Source)";
			this._columnHeader2.Width = 147;
			// 
			// lvDestination
			// 
			this._lvDestination.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
																							this._columnHeader3,
																							this._columnHeader4});
			this._lvDestination.FullRowSelect = true;
			this._lvDestination.HideSelection = false;
			this._lvDestination.Location = new System.Drawing.Point(176, 15);
			this._lvDestination.MultiSelect = false;
			this._lvDestination.Name = "_lvDestination";
			this._lvDestination.Size = new System.Drawing.Size(123, 110);
			this._lvDestination.TabIndex = 2;
			this._lvDestination.View = System.Windows.Forms.View.Details;
			this._lvDestination.Resize += new System.EventHandler(this.lvDestination_Resize);
			this._lvDestination.SelectedIndexChanged += new System.EventHandler(this.lvDestination_SelectedIndexChanged);
			// 
			// columnHeader3
			// 
			this._columnHeader3.Text = "Line";
			this._columnHeader3.Width = 50;
			// 
			// columnHeader4
			// 
			this._columnHeader4.Text = "Text (Destination)";
			this._columnHeader4.Width = 198;
			// 
			// Results
			// 
			this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
			this.ClientSize = new System.Drawing.Size(533, 440);
			this.Controls.Add(this._lvDestination);
			this.Controls.Add(this._lvSource);
			this.Name = "Results";
			this.Text = "Results";
			this.Resize += new System.EventHandler(this.Results_Resize);
			this.Load += new System.EventHandler(this.Results_Load);
			this.ResumeLayout(false);

		}
		#endregion

		private void lvSource_Resize(object sender, EventArgs e)
		{
			if (_lvSource.Width > 100)
			{
				_lvSource.Columns[1].Width = -2;
			}
		}

		private void lvDestination_Resize(object sender, EventArgs e)
		{
			if (_lvDestination.Width > 100)
			{
				_lvDestination.Columns[1].Width = -2;
			}
		}

		private void Results_Resize(object sender, EventArgs e)
		{
			int w = ClientRectangle.Width/2;
			_lvSource.Location = new Point(0,0);
			_lvSource.Width = w;
			_lvSource.Height = ClientRectangle.Height;

			_lvDestination.Location = new Point(w+1,0);
			_lvDestination.Width = ClientRectangle.Width - (w+1);
			_lvDestination.Height = ClientRectangle.Height;
		}

		private void Results_Load(object sender, EventArgs e)
		{
			Results_Resize(sender,e);
		}

		private void lvSource_SelectedIndexChanged(object sender, EventArgs e)
		{
			if (_lvSource.SelectedItems.Count > 0)
			{
				ListViewItem lvi = _lvDestination.Items[_lvSource.SelectedItems[0].Index];
				lvi.Selected = true;
				lvi.EnsureVisible();
			}
		}

		private void lvDestination_SelectedIndexChanged(object sender, EventArgs e)
		{
			if (_lvDestination.SelectedItems.Count > 0)
			{
				ListViewItem lvi = _lvSource.Items[_lvDestination.SelectedItems[0].Index];
				lvi.Selected = true;
				lvi.EnsureVisible();
			}
		}
	}
}
