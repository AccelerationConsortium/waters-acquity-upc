namespace STFTest
{
    partial class frmSTFTest
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.btnTestService = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // btnTestService
            // 
            this.btnTestService.Font = new System.Drawing.Font("Microsoft Sans Serif", 16F);
            this.btnTestService.Location = new System.Drawing.Point(109, 56);
            this.btnTestService.Name = "btnTestService";
            this.btnTestService.Size = new System.Drawing.Size(151, 42);
            this.btnTestService.TabIndex = 0;
            this.btnTestService.Text = "Test STF Service";
            this.btnTestService.UseVisualStyleBackColor = true;
            this.btnTestService.Click += new System.EventHandler(this.btnTestService_Click);
            // 
            // frmSTFTest
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(379, 173);
            this.Controls.Add(this.btnTestService);
            this.Name = "frmSTFTest";
            this.Text = "STF Test";
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button btnTestService;
    }
}

