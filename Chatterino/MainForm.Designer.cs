namespace Chatterino
{
    partial class MainForm
    {
        /// <summary>
        /// Erforderliche Designervariable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Verwendete Ressourcen bereinigen.
        /// </summary>
        /// <param name="disposing">True, wenn verwaltete Ressourcen gelöscht werden sollen; andernfalls False.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Vom Windows Form-Designer generierter Code

        /// <summary>
        /// Erforderliche Methode für die Designerunterstützung.
        /// Der Inhalt der Methode darf nicht mit dem Code-Editor geändert werden.
        /// </summary>
        private void InitializeComponent()
        {
            this.columnLayoutControl1 = new Chatterino.Controls.ColumnLayoutControl();
            this.SuspendLayout();
            // 
            // columnLayoutControl1
            // 
            this.columnLayoutControl1.AllowDrop = true;
            this.columnLayoutControl1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.columnLayoutControl1.Location = new System.Drawing.Point(0, 0);
            this.columnLayoutControl1.MaxColumns = 4;
            this.columnLayoutControl1.MaxRows = 4;
            this.columnLayoutControl1.Name = "columnLayoutControl1";
            this.columnLayoutControl1.Size = new System.Drawing.Size(809, 501);
            this.columnLayoutControl1.TabIndex = 0;
            this.columnLayoutControl1.Text = "columnLayoutControl1";
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(809, 501);
            this.Controls.Add(this.columnLayoutControl1);
            this.Name = "MainForm";
            this.ShowIcon = false;
            this.Text = "Chatterino";
            this.ResumeLayout(false);

        }

        #endregion

        private Controls.ColumnLayoutControl columnLayoutControl1;
    }
}

