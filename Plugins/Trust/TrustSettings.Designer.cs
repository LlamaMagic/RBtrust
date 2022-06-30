namespace Trust
{
    partial class TrustSettings
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(TrustSettings));
            this.foodDropBox = new System.Windows.Forms.ComboBox();
            this.foodLabel = new System.Windows.Forms.Label();
            this.enableFood = new System.Windows.Forms.CheckBox();
            this.SuspendLayout();
            // 
            // foodDropBox
            // 
            this.foodDropBox.FormattingEnabled = true;
            resources.ApplyResources(this.foodDropBox, "foodDropBox");
            this.foodDropBox.Name = "foodDropBox";
            this.foodDropBox.SelectedIndexChanged += new System.EventHandler(this.FoodDropBox_SelectedIndexChanged);
            this.foodDropBox.Click += new System.EventHandler(this.FoodDropBox_Click);
            // 
            // foodLabel
            // 
            resources.ApplyResources(this.foodLabel, "foodLabel");
            this.foodLabel.Name = "foodLabel";
            // 
            // enableFood
            // 
            resources.ApplyResources(this.enableFood, "enableFood");
            this.enableFood.Name = "enableFood";
            this.enableFood.UseVisualStyleBackColor = true;
            this.enableFood.CheckedChanged += new System.EventHandler(this.enableFood_CheckedChanged);
            // 
            // TrustSettings
            // 
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.enableFood);
            this.Controls.Add(this.foodLabel);
            this.Controls.Add(this.foodDropBox);
            this.Name = "TrustSettings";
            this.Load += new System.EventHandler(this.TrustSettings_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ComboBox foodDropBox;
        private System.Windows.Forms.Label foodLabel;
        private System.Windows.Forms.CheckBox enableFood;
    }
}