using System.Linq;
using System;
using System.Collections.Generic;
using System.Windows.Forms;
using FISCA.Presentation.Controls;
using K12.Data;
using FISCA.DSAClient;
using XHelper = FISCA.DSAClient.XmlHelper;
using FISCA.Authentication;

namespace Tagging
{
    /// <summary>
    /// 管理 Tag 的表單(學生、班級、教師、課程共用的 Parent)。
    /// </summary>
    internal partial class TagForm : BaseForm
    {
        private Dictionary<string, TagConfigRecord> Editors = new Dictionary<string, TagConfigRecord>();
        private const string AllTagText = "<顯示所有類別>";
        private const string NoTagText="<未分類>";

        public TagForm()
        {
            InitializeComponent();
        }

        private void TagForm_Load(object sender, EventArgs e)
        {
            if (Site != null && Site.DesignMode) return;

            if (DSAServices.IsLogined)
            {
                
                RefreshTagData();
                TagConfig.AfterDelete += new EventHandler<K12.Data.DataChangedEventArgs>(JHTagConfig_AfterDelete);
                TagConfig.AfterInsert += new EventHandler<K12.Data.DataChangedEventArgs>(JHTagConfig_AfterInsert);
                TagConfig.AfterUpdate += new EventHandler<K12.Data.DataChangedEventArgs>(JHTagConfig_AfterUpdate);
            }
        }

        void JHTagConfig_AfterUpdate(object sender, K12.Data.DataChangedEventArgs e)
        {
            foreach (string each in e.PrimaryKeys)
            {
                TagConfigRecord rec = TagConfig.SelectByID(each);

                if (rec != null)
                {
                    if (Editors.ContainsKey(each))
                        Editors[each] = rec;
                    else
                        Editors.Add(each, rec);
                }
                else
                {
                    if (Editors.ContainsKey(each))
                        Editors.Remove(each);
                    else
                        throw new ArgumentException(string.Format("Tag 錯誤，編號：{0}", each));
                }
            }

            RefreshTagData();
        }

        void JHTagConfig_AfterInsert(object sender, K12.Data.DataChangedEventArgs e)
        {
            foreach (string each in e.PrimaryKeys)
            {
                TagConfigRecord rec = TagConfig.SelectByID(each);

                if (rec != null)
                {
                    if (Editors.ContainsKey(each))
                        Editors[each] = rec;
                    else
                        Editors.Add(each, rec);
                }
                else
                {
                    if (Editors.ContainsKey(each))
                        Editors.Remove(each);
                    else
                        throw new ArgumentException(string.Format("Tag 錯誤，編號：{0}", each));
                }
            }

            RefreshTagData();
        }

        void JHTagConfig_AfterDelete(object sender, K12.Data.DataChangedEventArgs e)
        {
            foreach (string each in e.PrimaryKeys)
            {
                TagConfigRecord rec = TagConfig.SelectByID(each);

                if (rec != null)
                {
                    if (Editors.ContainsKey(each))
                        Editors[each] = rec;
                    else
                        Editors.Add(each, rec);
                }
                else
                {
                    if (Editors.ContainsKey(each))
                        Editors.Remove(each);
                    else
                        throw new ArgumentException(string.Format("Tag 錯誤，編號：{0}", each));
                }
            }

            RefreshTagData();
        }

        /// <summary>
        /// 重新讀取 Tag 資料，會自動呼叫 RefreshPrefixList、RefreshTagNameList。
        /// </summary>
        private void RefreshTagData()
        {
            List<TagConfigRecord> records = TagConfig.SelectByCategory(Category);
            Editors = new Dictionary<string, TagConfigRecord>();
            foreach (TagConfigRecord each in records)
                Editors.Add(each.ID, each);

            RefreshPrefixList();
            RefreshTagNameList();
        }

        private void TagForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            TagConfig.AfterDelete -= (JHTagConfig_AfterDelete);
            TagConfig.AfterInsert -= (JHTagConfig_AfterInsert);
            TagConfig.AfterUpdate -= (JHTagConfig_AfterUpdate);
        }

        private void btnRefresh_Click(object sender, EventArgs e)
        {
            RefreshTagData();
        }

        private void cboGroup_SelectedIndexChanged(object sender, EventArgs e)
        {
            RefreshTagNameList();
        }

        private void RefreshPrefixList()
        {
            string origin_selected = cboGroup.Text;

            List<string> prefixes = new List<string>();

            // 因為顯示所有類別要放在最前，所以不列入排序
            //prefixes.Add(AllTagText);
            bool hasNoTagText = false;
            foreach (TagConfigRecord each in TagConfig.SelectByCategory(Category))
            {
                if (!prefixes.Contains(each.Prefix))
                {
                    // 因需求將空白用未分類來處理
                    if (string.IsNullOrEmpty(each.Prefix))
                        hasNoTagText = true;
                    else
                        prefixes.Add(each.Prefix);
                }
            }
            // 有未分類
            if (hasNoTagText)
                if (!prefixes.Contains(NoTagText))
                    prefixes.Add(NoTagText);


            cboGroup.Items.Clear();
            prefixes.Sort();
            // 顯示所有類別放在最前
            cboGroup.Items.Add(AllTagText);
            cboGroup.Items.AddRange(prefixes.ToArray());

            int selIndex = cboGroup.FindString(origin_selected);
            if (selIndex == -1)
                cboGroup.SelectedIndex = (cboGroup.Items.Count > 0 ? 0 : -1);
            else
                cboGroup.SelectedIndex = selIndex;            
        }

        private void RefreshTagNameList()
        {
            string origin_selected_name = CurrentSelectedName;

            DGV.Rows.Clear();

            string prefix = cboGroup.Text;

            List<TagConfigRecord> EditorsList = new List<TagConfigRecord>();

            foreach (TagConfigRecord each in Editors.Values)
                EditorsList.Add(each);
            EditorsList.Sort(new Comparison<TagConfigRecord>(JHTagConfigRecordFullNameSorter));

            foreach (TagConfigRecord each in EditorsList)
            {
                // 因配合需求加入群組未分類處理,將空白用未分類表示
                if (prefix == NoTagText)
                    prefix = "";
                if (each.Prefix == prefix || prefix == AllTagText)
                {
                    DataGridViewRow row = new DataGridViewRow();
                    row.CreateCells(DGV, new TagColor(each.Color).Image, each.FullName);
                    row.Tag = each;

                    DGV.Rows.Add(row);

                    if (each.FullName == origin_selected_name)
                        row.Selected = true;
                }
            }
        }

        // 排序用
        private int JHTagConfigRecordFullNameSorter(TagConfigRecord x, TagConfigRecord y)
        {
            return x.FullName.CompareTo(y.FullName);
        }

        /// <summary>
        /// 取得畫面上使用者所選擇的 Tag 資料。
        /// </summary>
        private TagConfigRecord CurrentSelectedTag
        {
            get
            {
                if (DGV.SelectedRows.Count > 0)
                    return DGV.SelectedRows[0].Tag as TagConfigRecord;
                else
                    return null;
            }
        }

        /// <summary>
        /// 目前選擇到的 Tag 全名。
        /// </summary>
        private string CurrentSelectedName
        {
            get
            {
                if (CurrentSelectedTag != null)
                    return CurrentSelectedTag.FullName;
                else
                    return string.Empty;
            }
        }

        private void btnAdd_Click(object sender, EventArgs e)
        {
            TagEditor.InsertTag(Category);
        }

        private void btnDelete_Click(object sender, EventArgs e)
        {
            if (CurrentSelectedTag != null)
                DoDelete(CurrentSelectedTag);
        }

        private void btnModify_Click(object sender, EventArgs e)
        {
            if (CurrentSelectedTag != null)
                TagEditor.ModifyTag(CurrentSelectedTag);
        }

        public TagCategory Category { get; set; }

        /// <summary>
        /// 計算使用次數的服務名稱。
        /// </summary>
        public string CounterService { get; set; }

        public string EntityIdField { get; set; }

        public string EntityTitle { get; set; }

        private void DoDelete(TagConfigRecord record)
        {
            int use_count = TagUseCount(record.ID);

            string msg;
            if (use_count > 0)
                msg = string.Format("目前有「{0}」個{1}使用此類別，您確定要刪除類別嗎？", use_count, EntityTitle);
            else
                msg = "您確定要刪除此類別嗎？";

            if (FISCA.Presentation.Controls.MsgBox.Show(msg, MessageBoxButtons.YesNo) == DialogResult.Yes)
            {
                //PermRecLogProcess prlp = new PermRecLogProcess();
                //prlp.SaveLog("學籍.類別管理", "類別管理刪除類別", "刪除 " + record.Category + " 類別,名稱:" + record.FullName);
                TagConfig.Delete(record);
            }
        }

        private int TagUseCount(string tagId)
        {
            XHelper xreq = new XHelper("<Request/>");
            xreq.AddElement("Field");
            xreq.AddElement("Field", "ID");
            xreq.AddElement("Field", EntityIdField);
            xreq.AddElement("Condition");
            xreq.AddElement("Condition", "TagID", tagId);

            Envelope rsp = DSAServices.DefaultDataSource.SendRequest(CounterService, new Envelope(xreq));
            XHelper xrsp = new XHelper(rsp.Body);

            return xrsp.GetElements("Tag").Count();
        }
    }
}