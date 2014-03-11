using System;
using FISCA.Presentation;
using K12.Data;
using DevComponents.DotNetBar;
using System.Drawing;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Forms;
using FISCA.Presentation.Controls;
using Customization.Tagging;
using System.Collections;
using System.Linq;

namespace Tagging
{
    internal partial class TaggingBar : DescriptionPane
    {
        private BackgroundWorker TaggingWorker = new BackgroundWorker();

        private bool HasPadding = false;

        private List<GeneralTagRecord> CurrentTags;

        private LabelX DescriptionLabel;
        private PanelEx StatusPanel;
        private ContextMenuBar StatusMenuBar;

        /// <summary>
        /// 存放 Status MenuItem 的容器。
        /// </summary>
        private ButtonItem StatusContainer;
        /// <summary>
        /// 所有 Status 的項目。
        /// </summary>
        private List<ButtonItem> StatusList;

        public TaggingBar()
        {
            InitializeComponent();

            #region InitializeComponent Manual
            this.StatusMenuBar = new DevComponents.DotNetBar.ContextMenuBar();

            StatusList = new List<ButtonItem>();

            //依據設定動態建立功能表。
            foreach (StatusItem each in Program.StatusList)
            {
                ButtonItem item = new ButtonItem();

                item.AutoCheckOnClick = true;
                item.ImageListSizeSelection = DevComponents.DotNetBar.eButtonImageListSelection.NotSet;
                item.ImagePaddingHorizontal = 8;
                item.OptionGroup = "status";
                item.Text = each.Text;
                item.Tag = each;
                item.CheckedChanged += new System.EventHandler(this.StatusMenu_CheckedChanged);

                StatusList.Add(item);
            }

            this.StatusContainer = new DevComponents.DotNetBar.ButtonItem();
            this.StatusPanel = new DevComponents.DotNetBar.PanelEx();
            ((System.ComponentModel.ISupportInitialize)(this.StatusMenuBar)).BeginInit();
            this.SuspendLayout();
            // 
            // contextMenuBar1
            // 
            this.StatusMenuBar.Items.AddRange(new DevComponents.DotNetBar.BaseItem[] {
            this.StatusContainer});
            this.StatusMenuBar.Location = new System.Drawing.Point(36, 12);
            this.StatusMenuBar.Size = new System.Drawing.Size(123, 25);
            this.StatusMenuBar.Stretch = true;
            this.StatusMenuBar.TabIndex = 184;
            this.StatusMenuBar.TabStop = false;
            this.StatusMenuBar.Text = "StatusMenuBar";
            // 
            // buttonItem1
            // 
            this.StatusContainer.AutoExpandOnClick = true;
            this.StatusContainer.ImageListSizeSelection = DevComponents.DotNetBar.eButtonImageListSelection.NotSet;
            this.StatusContainer.ImagePaddingHorizontal = 8;
            this.StatusContainer.SubItems.AddRange(StatusList.ToArray());
            this.StatusContainer.Text = "statusMenu";
            // 
            // panelEx1
            // 
            this.StatusPanel.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.StatusPanel.CanvasColor = System.Drawing.SystemColors.Control;
            this.StatusPanel.Location = new System.Drawing.Point(66, 3);
            this.StatusPanel.Margin = new System.Windows.Forms.Padding(0);
            this.StatusPanel.Size = new System.Drawing.Size(95, 20);
            this.StatusPanel.Style.Alignment = System.Drawing.StringAlignment.Center;
            this.StatusPanel.Style.BackColor1.Color = System.Drawing.Color.LightBlue;
            this.StatusPanel.Style.BackColor2.ColorSchemePart = DevComponents.DotNetBar.eColorSchemePart.BarBackground2;
            this.StatusPanel.Style.BorderColor.ColorSchemePart = DevComponents.DotNetBar.eColorSchemePart.BarDockedBorder;
            this.StatusPanel.Style.BorderSide = DevComponents.DotNetBar.eBorderSide.None;
            this.StatusPanel.Style.BorderWidth = 0;
            this.StatusPanel.Style.CornerType = DevComponents.DotNetBar.eCornerType.Rounded;
            this.StatusPanel.Style.ForeColor.ColorSchemePart = DevComponents.DotNetBar.eColorSchemePart.ItemText;
            this.StatusPanel.Style.GradientAngle = 90;
            this.StatusPanel.Style.TextTrimming = System.Drawing.StringTrimming.Word;
            this.StatusPanel.TabIndex = 184;
            this.StatusPanel.Text = "一般";
            this.StatusPanel.Click += new System.EventHandler(this.StatusPanel_Click);
            //
            // DescriptionLabel
            //
            DescriptionLabel = new LabelX();
            DescriptionLabel.Text = string.Empty;
            DescriptionLabel.Dock = DockStyle.Left;
            DescriptionLabel.AutoSize = true;
            DescriptionLabel.Font = new Font(Font.FontFamily, 13);
            // 
            // StudentDescription
            // 
            //this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            //this.AutoSize = true;
            this.Controls.Add(this.StatusMenuBar);
            this.Name = "StudentDescription";

            ((System.ComponentModel.ISupportInitialize)(this.StatusMenuBar)).EndInit();
            this.ResumeLayout(false);

            DescriptionPanel.Controls.Add(DescriptionLabel);
            DescriptionPanel.Controls.Add(StatusPanel);
            #endregion

            TaggingWorker.DoWork += (TaggingWorker_DoWork);
            TaggingWorker.RunWorkerCompleted += (TaggingWorker_RunWorkerCompleted);

            TagConfig.AfterInsert += TagRecordChangedEventHandler;
            TagConfig.AfterUpdate += TagRecordChangedEventHandler;
            TagConfig.AfterDelete += TagRecordChangedEventHandler;
        }

        /// <summary>
        /// 取得某個 PrimaryKey 的 Tag 清單。
        /// </summary>
        public Func<string, List<GeneralTagRecord>> GetTagsDelegate;

        public void TagRecordChangedEventHandler(object sender, DataChangedEventArgs args)
        {
            if (this.InvokeRequired)
            {
                BeginInvoke(new Action(() => OnPrimaryKeyChanged(EventArgs.Empty)));
            }
            else
                OnPrimaryKeyChanged(EventArgs.Empty);
        }

        public Func<string, string> GetDescriptionDelegate;

        public bool StatusVisible
        {
            get { return StatusPanel.Visible; }
            set { StatusPanel.Visible = value; }
        }

        public StatusItem? GetStatusItem(StudentRecord.StudentStatus status)
        {
            foreach (StatusItem item in Program.StatusList)
                if (item.Status == status)
                    return item;

            return new StatusItem() { Text = "未知", Status = StudentRecord.StudentStatus.一般 };
        }

        protected override void OnPrimaryKeyChanged(EventArgs e)
        {
            if (string.IsNullOrEmpty(PrimaryKey)) return;

            EmptyItems();

            if (TaggingWorker.IsBusy)
                HasPadding = true;
            else
                TaggingWorker.RunWorkerAsync();
        }

        private void TaggingWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            if (GetTagsDelegate != null)
                CurrentTags = GetTagsDelegate(PrimaryKey);
            else
                CurrentTags = new List<GeneralTagRecord>();

            //以下還要改寫。
            string desc = GetDescriptionDelegate(PrimaryKey);

            Invoke(new Action(() =>
            {
                DescriptionLabel.Text = desc;
            }));

            if (!StatusVisible)
                return; //不顯示就不要讀資料了。

            StudentRecord stu = Student.SelectByID(PrimaryKey);

            Invoke(new Action(() =>
            {
                DisplayStatus(stu);
            }));
        }

        private void DisplayStatus(StudentRecord stu)
        {
            if (stu != null)
            {
                Color s;
                switch (stu.Status)
                {
                    case StudentRecord.StudentStatus.一般:
                        s = Color.FromArgb(255, 255, 255);
                        break;
                    case StudentRecord.StudentStatus.畢業或離校:
                        s = Color.FromArgb(156, 220, 128);
                        break;
                    case StudentRecord.StudentStatus.休學:
                        s = Color.FromArgb(254, 244, 128);
                        break;
                    case StudentRecord.StudentStatus.延修:
                        s = Color.FromArgb(224, 254, 210);
                        break;
                    case StudentRecord.StudentStatus.輟學:
                        s = Color.FromArgb(254, 244, 128);
                        break;
                    case StudentRecord.StudentStatus.轉出:
                        s = Color.FromArgb(156, 220, 55);
                        break;
                    case StudentRecord.StudentStatus.退學:
                        s = Color.FromArgb(156, 55, 128);
                        break;
                    case StudentRecord.StudentStatus.刪除:
                        s = Color.FromArgb(254, 128, 155);
                        break;
                    default:
                        s = Color.Transparent;
                        break;
                }

                StatusItem sitem = GetStatusItem(stu.Status).Value;

                StatusPanel.Text = sitem.Text;
                StatusPanel.Style.BackColor1.Color = s;
                StatusPanel.Style.BackColor2.Color = s;

                foreach (var item in StatusList)
                {
                    if ((StatusItem)item.Tag == sitem)
                        item.Checked = true;
                    else
                        item.Checked = false;
                }
            }
        }

        private void TaggingWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (HasPadding)
                TaggingWorker.RunWorkerAsync();
            else
            {
                ClearItems();

                foreach (GeneralTagRecord tag in CurrentTags)
                    AddItem(CreateItem(tag.FullName, tag.Color));
                AverageItemsSzie();
            }
            HasPadding = false;
        }

        private void EmptyItems()
        {
            //會閃....
            //foreach (PanelEx each in TaggingContainer.Controls)
            //    each.Text = string.Empty;
        }

        private void ClearItems()
        {
            foreach (Control ctl in TaggingContainer.Controls)
                Tip.SetSuperTooltip(ctl, null);

            TaggingContainer.Controls.Clear();
        }

        private void AddItem(PanelEx panel)
        {
            TaggingContainer.Controls.Add(panel);
        }

        protected override void OnParentChanged(EventArgs e)
        {
            base.OnParentChanged(e);

            if (Parent != null)
                Parent.SizeChanged += delegate //當 Parent 的大小改變時，要重新計算控制項大小。
                {
                    AverageItemsSzie();
                };
        }

        private PanelEx CreateItem(string title, Color color)
        {
            PanelEx objPanel = new PanelEx();

            objPanel.Margin = new System.Windows.Forms.Padding(2);
            objPanel.CanvasColor = System.Drawing.SystemColors.Control;
            objPanel.ColorSchemeStyle = eDotNetBarStyle.Office2010;
            objPanel.Size = new System.Drawing.Size(91, 20);
            objPanel.Style.Alignment = System.Drawing.StringAlignment.Center;
            objPanel.Style.BorderColor.ColorSchemePart = eColorSchemePart.Custom;
            objPanel.Style.BorderColor.Color = Color.LightBlue;
            objPanel.Style.BackColor2.ColorSchemePart = eColorSchemePart.Custom;
            objPanel.Style.BackColor2.Color = color;
            objPanel.Style.BackColor1.ColorSchemePart = eColorSchemePart.Custom;
            objPanel.Style.BackColor1.Color = Color.White;
            objPanel.Style.ForeColor.ColorSchemePart = eColorSchemePart.Custom;
            objPanel.Style.ForeColor.Color = Color.Black;
            objPanel.Style.Border = DevComponents.DotNetBar.eBorderType.SingleLine;
            objPanel.Style.CornerDiameter = 3;
            objPanel.Style.CornerType = DevComponents.DotNetBar.eCornerType.Diagonal;
            objPanel.Style.GradientAngle = 90;
            objPanel.Style.TextTrimming = System.Drawing.StringTrimming.None;
            objPanel.Text = title;
            Tip.DefaultFont = Font;
            Tip.SetSuperTooltip(objPanel, new SuperTooltipInfo("", "", title, null, null, eTooltipColor.Office2003));

            return objPanel;
        }

        /// <summary>
        /// 計算 Tag 的大小，使其剛好填滿 Tag Panel。
        /// </summary>
        private void AverageItemsSzie()
        {
            List<PanelEx> tags = new List<PanelEx>();
            foreach (PanelEx panel in TaggingContainer.Controls)
                tags.Add(panel);

            if (Parent == null || tags.Count <= 0) return;

            //TableLayout.Width = Parent.Width;
            //Width = Parent.Width;

            int partsize = TaggingContainer.Width / tags.Count;

            foreach (PanelEx each in tags)
                each.Size = new Size(partsize - 4, each.Size.Height);
        }

        private void StatusMenu_CheckedChanged(object sender, EventArgs e)
        {
            var button = (DevComponents.DotNetBar.ButtonItem)sender;
            StatusCheckedChanged(button, PrimaryKey);
        }

        private void StatusCheckedChanged(ButtonItem button, string key)
        {
            if (button.Checked)
            {
                //var studentRec = Student.Instance.Items[PrimaryKey];
                StudentRecord studentRec = Student.SelectByID(key);

                if (studentRec != null)
                {
                    if ((StatusItem)button.Tag != GetStatusItem(studentRec.Status).Value)
                    {
                        try
                        {
                            studentRec.Status = ((StatusItem)button.Tag).Status;

                            // 檢查同狀態要身分證或學號相同時，無法變更
                            List<string> checkIDNumber = new List<string>();
                            List<string> checkSnum = new List<string>();

                            foreach (StudentRecord studRec in K12.Data.Student.SelectAll())
                            {
                                if (studRec.Status == studentRec.Status)
                                {
                                    if (!string.IsNullOrEmpty(studRec.StudentNumber))
                                        checkSnum.Add(studRec.StudentNumber.Trim());
                                    if (!string.IsNullOrEmpty(studRec.IDNumber))
                                        checkIDNumber.Add(studRec.IDNumber.Trim());
                                }
                            }

                            if (checkSnum.Contains(studentRec.StudentNumber.Trim()))
                            {
                                MsgBox.Show("在" + studentRec.Status.ToString() + "狀態學號有重複無法變更.");
                                return;
                            }

                            if (checkIDNumber.Contains(studentRec.IDNumber.Trim()))
                            {
                                MsgBox.Show("在" + studentRec.Status.ToString() + "狀態身分證號有重複無法變更.");
                                return;
                            }

                            K12.Data.Student.Update(studentRec);
                        }
                        catch (ArgumentException)
                        {
                            MessageBox.Show("目前無法移到刪除");
                        }
                        catch
                        {
                            MotherForm.SetStatusBarMessage("變更狀態失敗，可能發生原因為學號或身分證號在" + button.Text + "學生中已經存在，請檢查學生資料。");
                            return;
                        }
                    }
                }
            }
        }

        private void StatusPanel_Click(object sender, EventArgs e)
        {
            StatusContainer.Popup(StatusPanel.PointToScreen(new Point(0, StatusPanel.Height)));
        }
    }
}
