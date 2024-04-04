namespace Goofy_Server
{
    partial class ServerUI
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
            entityListView = new System.Windows.Forms.ListView();
            deleteAllDataButton = new System.Windows.Forms.Button();
            SuspendLayout();
            // 
            // entityListView
            // 
            entityListView.Location = new System.Drawing.Point(12, 12);
            entityListView.Name = "entityListView";
            entityListView.Size = new System.Drawing.Size(516, 397);
            entityListView.TabIndex = 0;
            entityListView.UseCompatibleStateImageBehavior = false;
            // 
            // deleteAllDataButton
            // 
            deleteAllDataButton.Location = new System.Drawing.Point(453, 415);
            deleteAllDataButton.Name = "deleteAllDataButton";
            deleteAllDataButton.Size = new System.Drawing.Size(75, 23);
            deleteAllDataButton.TabIndex = 1;
            deleteAllDataButton.Text = "Delete All";
            deleteAllDataButton.UseVisualStyleBackColor = true;
            deleteAllDataButton.Click += deleteAll;
            // 
            // ServerUI
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            ClientSize = new System.Drawing.Size(540, 450);
            Controls.Add(deleteAllDataButton);
            Controls.Add(entityListView);
            Name = "ServerUI";
            Text = "ServerUI";
            ResumeLayout(false);
        }

        #endregion

        private System.Windows.Forms.ListView entityListView;
        private System.Windows.Forms.Button deleteAllDataButton;
    }
}