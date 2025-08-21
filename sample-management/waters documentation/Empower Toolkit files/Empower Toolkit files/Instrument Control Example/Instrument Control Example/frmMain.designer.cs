
namespace Waters.Empower.InstrumentControl.Example
{
	public partial class FrmMain : System.Windows.Forms.Form
	{	
		//Form overrides dispose to clean up the component list.
		[System.Diagnostics.DebuggerNonUserCode()]protected override void Dispose(bool disposing)
		{
			try
			{
				if (disposing && components != null)
				{
					components.Dispose();
				}
			}
			finally
			{
				base.Dispose(disposing);
			}
		}
		
		//Required by the Windows Form Designer
		private System.ComponentModel.Container components = null;
		
		//NOTE: The following procedure is required by the Windows Form Designer
		//It can be modified using the Windows Form Designer.
		//Do not modify it using the code editor.
		[System.Diagnostics.DebuggerStepThrough()]private void InitializeComponent()
		{
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FrmMain));
            this.txtShowInstrumentConfig = new System.Windows.Forms.TextBox();
            this.btnShowInstConfig = new System.Windows.Forms.Button();
            this.txtNewSSMName = new System.Windows.Forms.TextBox();
            this.btnModifyNextLine = new System.Windows.Forms.Button();
            this.btnPauseRun = new System.Windows.Forms.Button();
            this.btnRefreshInfo = new System.Windows.Forms.Button();
            this.lbStatusInformation = new System.Windows.Forms.ListBox();
            this.btnAddALine = new System.Windows.Forms.Button();
            this.lblSSMNameTitle = new System.Windows.Forms.Label();
            this.gbRunStatus = new System.Windows.Forms.GroupBox();
            this.btnStartRun = new System.Windows.Forms.Button();
            this.lblSampleSetMethod = new System.Windows.Forms.Label();
            this.cbSampleSetMethod = new System.Windows.Forms.ComboBox();
            this.lblNodeTitle = new System.Windows.Forms.Label();
            this.lblSystemTitle = new System.Windows.Forms.Label();
            this.cbSystem = new System.Windows.Forms.ComboBox();
            this.cbNode = new System.Windows.Forms.ComboBox();
            this.btnConnect = new System.Windows.Forms.Button();
            this.gbRunStatus.SuspendLayout();
            this.SuspendLayout();
            // 
            // txtShowInstrumentConfig
            // 
            this.txtShowInstrumentConfig.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtShowInstrumentConfig.Location = new System.Drawing.Point(40, 583);
            this.txtShowInstrumentConfig.Multiline = true;
            this.txtShowInstrumentConfig.Name = "txtShowInstrumentConfig";
            this.txtShowInstrumentConfig.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.txtShowInstrumentConfig.Size = new System.Drawing.Size(346, 225);
            this.txtShowInstrumentConfig.TabIndex = 33;
            // 
            // btnShowInstConfig
            // 
            this.btnShowInstConfig.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnShowInstConfig.Location = new System.Drawing.Point(417, 588);
            this.btnShowInstConfig.Name = "btnShowInstConfig";
            this.btnShowInstConfig.Size = new System.Drawing.Size(127, 34);
            this.btnShowInstConfig.TabIndex = 32;
            this.btnShowInstConfig.Text = "Show Instrument Configuration";
            this.btnShowInstConfig.UseVisualStyleBackColor = true;
            this.btnShowInstConfig.Click += new System.EventHandler(this.BtnShowInstConfig_Click);
            // 
            // txtNewSSMName
            // 
            this.txtNewSSMName.Location = new System.Drawing.Point(166, 186);
            this.txtNewSSMName.Name = "txtNewSSMName";
            this.txtNewSSMName.Size = new System.Drawing.Size(235, 20);
            this.txtNewSSMName.TabIndex = 28;
            // 
            // btnModifyNextLine
            // 
            this.btnModifyNextLine.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnModifyNextLine.Location = new System.Drawing.Point(417, 372);
            this.btnModifyNextLine.Name = "btnModifyNextLine";
            this.btnModifyNextLine.Size = new System.Drawing.Size(127, 34);
            this.btnModifyNextLine.TabIndex = 27;
            this.btnModifyNextLine.Text = "Modify Next line";
            this.btnModifyNextLine.UseVisualStyleBackColor = true;
            this.btnModifyNextLine.Click += new System.EventHandler(this.BtnModifyNextLine_Click);
            // 
            // btnPauseRun
            // 
            this.btnPauseRun.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnPauseRun.Location = new System.Drawing.Point(417, 332);
            this.btnPauseRun.Name = "btnPauseRun";
            this.btnPauseRun.Size = new System.Drawing.Size(127, 34);
            this.btnPauseRun.TabIndex = 26;
            this.btnPauseRun.Text = "Pause Run";
            this.btnPauseRun.UseVisualStyleBackColor = true;
            this.btnPauseRun.Click += new System.EventHandler(this.BtnPauseRun_Click);
            // 
            // btnRefreshInfo
            // 
            this.btnRefreshInfo.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnRefreshInfo.Location = new System.Drawing.Point(417, 292);
            this.btnRefreshInfo.Name = "btnRefreshInfo";
            this.btnRefreshInfo.Size = new System.Drawing.Size(127, 34);
            this.btnRefreshInfo.TabIndex = 30;
            this.btnRefreshInfo.Text = "Refresh Status";
            this.btnRefreshInfo.UseVisualStyleBackColor = true;
            this.btnRefreshInfo.Click += new System.EventHandler(this.BtnRefreshInfo_Click);
            // 
            // lbStatusInformation
            // 
            this.lbStatusInformation.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lbStatusInformation.FormattingEnabled = true;
            this.lbStatusInformation.Location = new System.Drawing.Point(13, 23);
            this.lbStatusInformation.Name = "lbStatusInformation";
            this.lbStatusInformation.Size = new System.Drawing.Size(346, 251);
            this.lbStatusInformation.TabIndex = 0;
            // 
            // btnAddALine
            // 
            this.btnAddALine.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnAddALine.Location = new System.Drawing.Point(417, 412);
            this.btnAddALine.Name = "btnAddALine";
            this.btnAddALine.Size = new System.Drawing.Size(127, 34);
            this.btnAddALine.TabIndex = 31;
            this.btnAddALine.Text = "Add A Line";
            this.btnAddALine.UseVisualStyleBackColor = true;
            this.btnAddALine.Click += new System.EventHandler(this.BtnAddALine_Click);
            // 
            // lblSSMNameTitle
            // 
            this.lblSSMNameTitle.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblSSMNameTitle.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.lblSSMNameTitle.Location = new System.Drawing.Point(24, 184);
            this.lblSSMNameTitle.Name = "lblSSMNameTitle";
            this.lblSSMNameTitle.Size = new System.Drawing.Size(136, 23);
            this.lblSSMNameTitle.TabIndex = 29;
            this.lblSSMNameTitle.Text = "New SSM Name";
            this.lblSSMNameTitle.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // gbRunStatus
            // 
            this.gbRunStatus.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.gbRunStatus.Controls.Add(this.lbStatusInformation);
            this.gbRunStatus.Location = new System.Drawing.Point(27, 273);
            this.gbRunStatus.Name = "gbRunStatus";
            this.gbRunStatus.Size = new System.Drawing.Size(370, 292);
            this.gbRunStatus.TabIndex = 25;
            this.gbRunStatus.TabStop = false;
            this.gbRunStatus.Text = "Run Status";
            // 
            // btnStartRun
            // 
            this.btnStartRun.Location = new System.Drawing.Point(24, 223);
            this.btnStartRun.Name = "btnStartRun";
            this.btnStartRun.Size = new System.Drawing.Size(378, 34);
            this.btnStartRun.TabIndex = 24;
            this.btnStartRun.Text = "Start Run";
            this.btnStartRun.UseVisualStyleBackColor = true;
            this.btnStartRun.Click += new System.EventHandler(this.BtnStartRun_Click);
            // 
            // lblSampleSetMethod
            // 
            this.lblSampleSetMethod.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblSampleSetMethod.Location = new System.Drawing.Point(24, 131);
            this.lblSampleSetMethod.Name = "lblSampleSetMethod";
            this.lblSampleSetMethod.Size = new System.Drawing.Size(378, 23);
            this.lblSampleSetMethod.TabIndex = 23;
            this.lblSampleSetMethod.Text = "Sample Set Method";
            this.lblSampleSetMethod.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // cbSampleSetMethod
            // 
            this.cbSampleSetMethod.FormattingEnabled = true;
            this.cbSampleSetMethod.Location = new System.Drawing.Point(24, 156);
            this.cbSampleSetMethod.Name = "cbSampleSetMethod";
            this.cbSampleSetMethod.Size = new System.Drawing.Size(375, 21);
            this.cbSampleSetMethod.TabIndex = 22;
            // 
            // lblNodeTitle
            // 
            this.lblNodeTitle.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblNodeTitle.Location = new System.Drawing.Point(24, 14);
            this.lblNodeTitle.Name = "lblNodeTitle";
            this.lblNodeTitle.Size = new System.Drawing.Size(180, 19);
            this.lblNodeTitle.TabIndex = 21;
            this.lblNodeTitle.Text = "Node";
            this.lblNodeTitle.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // lblSystemTitle
            // 
            this.lblSystemTitle.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblSystemTitle.Location = new System.Drawing.Point(222, 14);
            this.lblSystemTitle.Name = "lblSystemTitle";
            this.lblSystemTitle.Size = new System.Drawing.Size(173, 19);
            this.lblSystemTitle.TabIndex = 20;
            this.lblSystemTitle.Text = "System";
            this.lblSystemTitle.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // cbSystem
            // 
            this.cbSystem.FormattingEnabled = true;
            this.cbSystem.Location = new System.Drawing.Point(222, 36);
            this.cbSystem.Name = "cbSystem";
            this.cbSystem.Size = new System.Drawing.Size(180, 21);
            this.cbSystem.TabIndex = 19;
            // 
            // cbNode
            // 
            this.cbNode.FormattingEnabled = true;
            this.cbNode.Location = new System.Drawing.Point(24, 36);
            this.cbNode.Name = "cbNode";
            this.cbNode.Size = new System.Drawing.Size(180, 21);
            this.cbNode.TabIndex = 18;
            // 
            // btnConnect
            // 
            this.btnConnect.Location = new System.Drawing.Point(24, 72);
            this.btnConnect.Name = "btnConnect";
            this.btnConnect.Size = new System.Drawing.Size(378, 34);
            this.btnConnect.TabIndex = 17;
            this.btnConnect.Text = "Connect";
            this.btnConnect.UseVisualStyleBackColor = true;
            this.btnConnect.Click += new System.EventHandler(this.BtnConnect_Click);
            // 
            // FrmMain
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(584, 821);
            this.Controls.Add(this.txtShowInstrumentConfig);
            this.Controls.Add(this.btnShowInstConfig);
            this.Controls.Add(this.txtNewSSMName);
            this.Controls.Add(this.btnModifyNextLine);
            this.Controls.Add(this.btnPauseRun);
            this.Controls.Add(this.btnRefreshInfo);
            this.Controls.Add(this.btnAddALine);
            this.Controls.Add(this.lblSSMNameTitle);
            this.Controls.Add(this.gbRunStatus);
            this.Controls.Add(this.btnStartRun);
            this.Controls.Add(this.lblSampleSetMethod);
            this.Controls.Add(this.cbSampleSetMethod);
            this.Controls.Add(this.lblNodeTitle);
            this.Controls.Add(this.lblSystemTitle);
            this.Controls.Add(this.cbSystem);
            this.Controls.Add(this.cbNode);
            this.Controls.Add(this.btnConnect);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MinimumSize = new System.Drawing.Size(600, 860);
            this.Name = "FrmMain";
            this.Padding = new System.Windows.Forms.Padding(10);
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Empower Instrument Control Example";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.FrmMain_FormClosed);
            this.Load += new System.EventHandler(this.FrmMain_Load);
            this.gbRunStatus.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

		}
		private System.Windows.Forms.TextBox txtShowInstrumentConfig;
		private System.Windows.Forms.Button btnShowInstConfig;
		private System.Windows.Forms.TextBox txtNewSSMName;
		private System.Windows.Forms.Button btnModifyNextLine;
		private System.Windows.Forms.Button btnPauseRun;
		private System.Windows.Forms.Button btnRefreshInfo;
		private System.Windows.Forms.ListBox lbStatusInformation;
		private System.Windows.Forms.Button btnAddALine;
		private System.Windows.Forms.Label lblSSMNameTitle;
		private System.Windows.Forms.GroupBox gbRunStatus;
		private System.Windows.Forms.Button btnStartRun;
		private System.Windows.Forms.Label lblSampleSetMethod;
		private System.Windows.Forms.ComboBox cbSampleSetMethod;
		private System.Windows.Forms.Label lblNodeTitle;
		private System.Windows.Forms.Label lblSystemTitle;
		private System.Windows.Forms.ComboBox cbSystem;
		private System.Windows.Forms.ComboBox cbNode;
		private System.Windows.Forms.Button btnConnect;
		
	}
	
}
