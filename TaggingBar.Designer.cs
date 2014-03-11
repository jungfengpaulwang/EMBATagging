namespace Tagging
{
    partial class TaggingBar
    {
        /// <summary> 
        /// 設計工具所需的變數。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// 清除任何使用中的資源。
        /// </summary>
        /// <param name="disposing">如果應該處置 Managed 資源則為 true，否則為 false。</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region 元件設計工具產生的程式碼

        /// <summary> 
        /// 此為設計工具支援所需的方法 - 請勿使用程式碼編輯器
        /// 修改這個方法的內容。
        /// </summary>
        private void InitializeComponent()
        {
            this.TableLayout = new System.Windows.Forms.TableLayoutPanel();
            this.TaggingContainer = new System.Windows.Forms.FlowLayoutPanel();
            this.DescriptionPanel = new System.Windows.Forms.FlowLayoutPanel();
            this.Tip = new DevComponents.DotNetBar.SuperTooltip();
            this.TableLayout.SuspendLayout();
            this.SuspendLayout();
            // 
            // TableLayout
            // 
            this.TableLayout.ColumnCount = 1;
            this.TableLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.TableLayout.Controls.Add(this.TaggingContainer, 0, 1);
            this.TableLayout.Controls.Add(this.DescriptionPanel, 0, 0);
            this.TableLayout.Dock = System.Windows.Forms.DockStyle.Fill;
            this.TableLayout.Location = new System.Drawing.Point(0, 0);
            this.TableLayout.Name = "TableLayout";
            this.TableLayout.RowCount = 2;
            this.TableLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 55F));
            this.TableLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 45F));
            this.TableLayout.Size = new System.Drawing.Size(563, 60);
            this.TableLayout.TabIndex = 0;
            // 
            // TaggingContainer
            // 
            this.TaggingContainer.Dock = System.Windows.Forms.DockStyle.Fill;
            this.TaggingContainer.Location = new System.Drawing.Point(5, 33);
            this.TaggingContainer.Margin = new System.Windows.Forms.Padding(5, 0, 5, 0);
            this.TaggingContainer.Name = "TaggingContainer";
            this.TaggingContainer.Size = new System.Drawing.Size(553, 27);
            this.TaggingContainer.TabIndex = 0;
            // 
            // DescriptionPanel
            // 
            this.DescriptionPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.DescriptionPanel.Location = new System.Drawing.Point(0, 0);
            this.DescriptionPanel.Margin = new System.Windows.Forms.Padding(0);
            this.DescriptionPanel.Name = "DescriptionPanel";
            this.DescriptionPanel.Size = new System.Drawing.Size(563, 33);
            this.DescriptionPanel.TabIndex = 1;
            // 
            // Tip
            // 
            this.Tip.LicenseKey = "F962CEC7-CD8F-4911-A9E9-CAB39962FC1F";
            // 
            // TaggingBar
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 17F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.TableLayout);
            this.Margin = new System.Windows.Forms.Padding(0);
            this.Name = "TaggingBar";
            this.Size = new System.Drawing.Size(563, 60);
            this.TableLayout.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel TableLayout;
        private System.Windows.Forms.FlowLayoutPanel TaggingContainer;
        private DevComponents.DotNetBar.SuperTooltip Tip;
        private System.Windows.Forms.FlowLayoutPanel DescriptionPanel;

    }
}
