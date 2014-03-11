namespace Tagging
{
    partial class TaggingView<T>
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
        /// 此為設計工具支援所需的方法 - 請勿使用程式碼編輯器修改這個方法的內容。
        ///
        /// </summary>
        private void InitializeComponent()
        {
            this.TagTree = new DevComponents.AdvTree.AdvTree();
            this.columnHeader1 = new DevComponents.AdvTree.ColumnHeader();
            this.contextMenuBar1 = new DevComponents.DotNetBar.ContextMenuBar();
            this.CoreMenu = new DevComponents.DotNetBar.ButtonItem();
            this.btnRefreshAll = new DevComponents.DotNetBar.ButtonItem();
            this.nodeConnector1 = new DevComponents.AdvTree.NodeConnector();
            this.elementStyle1 = new DevComponents.DotNetBar.ElementStyle();
            ((System.ComponentModel.ISupportInitialize)(this.TagTree)).BeginInit();
            this.TagTree.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.contextMenuBar1)).BeginInit();
            this.SuspendLayout();
            // 
            // TagTree
            // 
            this.TagTree.AccessibleRole = System.Windows.Forms.AccessibleRole.Outline;
            this.TagTree.AllowDrop = true;
            this.TagTree.BackColor = System.Drawing.SystemColors.Window;
            // 
            // 
            // 
            this.TagTree.BackgroundStyle.Class = "TreeBorderKey";
            this.TagTree.BackgroundStyle.CornerType = DevComponents.DotNetBar.eCornerType.Square;
            this.TagTree.Columns.Add(this.columnHeader1);
            this.TagTree.ColumnsVisible = false;
            this.contextMenuBar1.SetContextMenuEx(this.TagTree, this.CoreMenu);
            this.TagTree.Controls.Add(this.contextMenuBar1);
            this.TagTree.Dock = System.Windows.Forms.DockStyle.Fill;
            this.TagTree.DragDropEnabled = false;
            this.TagTree.ExpandWidth = 16;
            this.TagTree.LicenseKey = "F962CEC7-CD8F-4911-A9E9-CAB39962FC1F";
            this.TagTree.Location = new System.Drawing.Point(0, 0);
            this.TagTree.Name = "TagTree";
            this.TagTree.NodesConnector = this.nodeConnector1;
            this.TagTree.NodeStyle = this.elementStyle1;
            this.TagTree.PathSeparator = ";";
            this.TagTree.Size = new System.Drawing.Size(338, 514);
            this.TagTree.Styles.Add(this.elementStyle1);
            this.TagTree.TabIndex = 1;
            this.TagTree.Text = "advTree1";
            this.TagTree.AfterNodeSelect += new DevComponents.AdvTree.AdvTreeNodeEventHandler(this.advTree1_AfterNodeSelect);
            this.TagTree.NodeClick += new DevComponents.AdvTree.TreeNodeMouseEventHandler(this.advTree1_NodeClick);
            this.TagTree.NodeDoubleClick += new DevComponents.AdvTree.TreeNodeMouseEventHandler(this.advTree1_NodeDoubleClick);
            // 
            // columnHeader1
            // 
            this.columnHeader1.Name = "columnHeader1";
            this.columnHeader1.Width.Relative = 100;
            // 
            // contextMenuBar1
            // 
            this.contextMenuBar1.AntiAlias = true;
            this.contextMenuBar1.DockSide = DevComponents.DotNetBar.eDockSide.Document;
            this.contextMenuBar1.Items.AddRange(new DevComponents.DotNetBar.BaseItem[] {
            this.CoreMenu});
            this.contextMenuBar1.Location = new System.Drawing.Point(132, 240);
            this.contextMenuBar1.Name = "contextMenuBar1";
            this.contextMenuBar1.Size = new System.Drawing.Size(75, 27);
            this.contextMenuBar1.Stretch = true;
            this.contextMenuBar1.Style = DevComponents.DotNetBar.eDotNetBarStyle.StyleManagerControlled;
            this.contextMenuBar1.TabIndex = 3;
            this.contextMenuBar1.TabStop = false;
            this.contextMenuBar1.Text = "contextMenuBar1";
            // 
            // CoreMenu
            // 
            this.CoreMenu.AutoExpandOnClick = true;
            this.CoreMenu.Name = "CoreMenu";
            this.CoreMenu.SubItems.AddRange(new DevComponents.DotNetBar.BaseItem[] {
            this.btnRefreshAll});
            this.CoreMenu.Text = "Menu";
            // 
            // btnRefreshAll
            // 
            this.btnRefreshAll.Name = "btnRefreshAll";
            this.btnRefreshAll.Text = "重新整理";
            this.btnRefreshAll.Tooltip = "強制重新讀取所有資料";
            this.btnRefreshAll.Click += new System.EventHandler(this.btnRefreshAll_Click);
            // 
            // nodeConnector1
            // 
            this.nodeConnector1.LineColor = System.Drawing.SystemColors.ControlText;
            // 
            // elementStyle1
            // 
            this.elementStyle1.Class = "";
            this.elementStyle1.CornerType = DevComponents.DotNetBar.eCornerType.Square;
            this.elementStyle1.Name = "elementStyle1";
            this.elementStyle1.TextColor = System.Drawing.SystemColors.ControlText;
            // 
            // TaggingView
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 17F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.Window;
            this.Controls.Add(this.TagTree);
            this.Name = "TaggingView";
            this.Size = new System.Drawing.Size(338, 514);
            ((System.ComponentModel.ISupportInitialize)(this.TagTree)).EndInit();
            this.TagTree.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.contextMenuBar1)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private DevComponents.AdvTree.AdvTree TagTree;
        private DevComponents.AdvTree.ColumnHeader columnHeader1;
        private DevComponents.AdvTree.NodeConnector nodeConnector1;
        private DevComponents.DotNetBar.ElementStyle elementStyle1;
        private DevComponents.DotNetBar.ContextMenuBar contextMenuBar1;
        private DevComponents.DotNetBar.ButtonItem CoreMenu;
        private DevComponents.DotNetBar.ButtonItem btnRefreshAll;
    }
}
