using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Forms;
using FISCA.Presentation;
using K12.Data;
using DevComponents.AdvTree;
using System.Text.RegularExpressions;
using System.Diagnostics;
using FISCA;

namespace Tagging
{
    /// <summary>
    /// 
    /// </summary>
    internal partial class TaggingView<T> : NavView
        where T : GeneralTagRecord
    {
        /// <summary>
        /// 是否有工作排程中。
        /// </summary>
        private bool TaskPadding = false;

        private bool _reload_padding = false;
        /// <summary>
        /// 是否有快取要重新整理。
        /// </summary>
        private bool ReloadPadding
        {
            get { return _reload_padding; }
            set { _reload_padding = value; }
        }

        //拒絕變更 ListPanelSource
        private bool DenyChangeListPaneSource = false;

        #region Cache and Sync
        private TagConfigRecordCache _tag_config_cache = null;
        internal TagConfigRecordCache TagConfigCache
        {
            get { return _tag_config_cache; }
            set
            {
                _tag_config_cache = value;
                _tag_config_cache.K12ItemUpdated += TagConfigChangedEventHandler;
            }
        }

        private EntityTagRecordCache<T> _tag_general_cache = null;
        internal EntityTagRecordCache<T> EntityTagCache
        {
            get { return _tag_general_cache; }
            set
            {
                _tag_general_cache = value;
                _tag_general_cache.K12ItemUpdated += TagGeneralChangedEventHandler;
            }
        }

        private void TagConfigChangedEventHandler(object sender, DataChangedEventArgs args)
        {
            LayoutAsync();
        }

        private void TagGeneralChangedEventHandler(object sender, DataChangedEventArgs args)
        {
            LayoutAsync();
        }
        #endregion

        private void Program_ReloadCache(object sender, Program.ReloadEventArgs e)
        {
            ReloadPadding = true;
        }

        protected override void OnSourceChanged()
        {
            base.OnSourceChanged();

            LayoutAsync();
        }

        /// <summary>
        /// 目前要被處理的資料。
        /// </summary>
        private List<T> Records
        {
            get
            {
                List<T> recordset = new List<T>();

                EntityTagCache.WaitInitializeComplete();
                EntityTagCache.WaitSyncComplete();
                foreach (string key in new List<string>(EntityTagCache.Items.Keys))
                {
                    if (Source.Contains(key))
                        recordset.AddRange(EntityTagCache[key]);
                }
                return recordset;
            }
        }

        private List<string> SelectedPath = new List<string>();

        private BackgroundWorker TaskThread = new BackgroundWorker();

        //private Node LoadingNode = new Node();

        public TaggingView()
        {
            InitializeComponent();
            this.NavText = "類別檢視";

            //LoadingNode.Text = "處理資料中...";
            SharedKeysMap = new Dictionary<Node, List<string>>();

            TaskThread.DoWork += (TaskThread_DoWork);
            TaskThread.RunWorkerCompleted += (TaskThread_RunWorkerCompleted);

            Program.ReloadCache += Program_ReloadCache;
        }

        private void LayoutAsync()
        {
            if (!Active) return;

            if (InvokeRequired) //防止背景執行緒衝進來。
                BeginInvoke(new Action(() => LayoutAsync()));
            else
            {
                #region 記錄選取的結點的完整路徑
                SelectedPath = new List<string>();
                var selectNode = TagTree.SelectedNode;
                if (selectNode != null)
                {
                    while (selectNode != null)
                    {
                        SelectedPath.Insert(0, selectNode.Text);
                        selectNode = selectNode.Parent;
                    }
                }
                #endregion

                DenyChangeListPaneSource = true;
                //TagTree.Nodes.Clear();
                //TagTree.Nodes.Add(LoadingNode);
                TagTree.Enabled = false;
                Application.DoEvents();
                DenyChangeListPaneSource = false;

                if (TaskThread.IsBusy)
                    TaskPadding = true;
                else
                    TaskThread.RunWorkerAsync();
            }
        }

        private void TaskThread_DoWork(object sender, DoWorkEventArgs e)
        {
            if (ReloadPadding)
            {
                TagConfigCache.SyncAll();
                EntityTagCache.SyncAll();
                ReloadPadding = false;
            }
            else
            {
                TagConfigCache.WaitInitializeComplete();
                EntityTagCache.WaitInitializeComplete();
            }

            TagConfigCache.WaitLazySyncComplete();
            EntityTagCache.WaitLazySyncComplete();

            PrefixCategoryNode = OrganizeNodes();
        }

        private void TaskThread_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Error != null)
            {
                MessageBox.Show(e.Error.Message);
                RTOut.WriteError(e.Error);
                return;
            }

            if (TaskPadding)
                TaskThread.RunWorkerAsync();
            else
                Layout();

            TaskPadding = false;
        }

        #region NavView 成員

        //一個 Node 與裡面包含的 TagRecord 對照表。
        private Dictionary<Node, List<string>> NodeTagsMap { get { return PrefixCategoryNode.SharedKeysMap; } }

        public Dictionary<Node, List<string>> SharedKeysMap;

        private NavViewNode PrefixCategoryNode;

        public new void Layout()
        {
            UpdateUI(PrefixCategoryNode);
        }

        private NavViewNode OrganizeNodes()
        {
            this.SharedKeysMap.Clear();
            NavViewNode PrefixedTags = new NavViewNode() { SharedKeysMap = this.SharedKeysMap };
            NavViewNode NoPrefixTags = new NavViewNode() { SharedKeysMap = this.SharedKeysMap };
            NavViewNode NoTags = new NavViewNode() { SharedKeysMap = this.SharedKeysMap };

            Dictionary<string, List<GeneralTagRecord>> TagsMap = new Dictionary<string, List<GeneralTagRecord>>();

            //建立 Entity 與 EntityTags 的對應。
            foreach (GeneralTagRecord tag in Records)
            {
                if (!TagsMap.ContainsKey(tag.RefEntityID))
                    TagsMap.Add(tag.RefEntityID, new List<GeneralTagRecord>());

                TagsMap[tag.RefEntityID].Add(tag);
            }

            //bool checkHasTag = false;
            foreach (var studentID in new List<string>(Source)) //Scan Source。
            {
                if (!TagsMap.ContainsKey(studentID))
                    NoTags.KeyChildren.Add(studentID);
                else
                {
                    //一個學生的所有 Tag。
                    List<GeneralTagRecord> stuTags = TagsMap[studentID];

                    foreach (GeneralTagRecord stuTag in stuTags)
                    {
                        if (!TagConfigCache.Items.ContainsKey(stuTag.RefTagID))
                        {
                            Console.WriteLine("TagConfig {0} 不存在。", stuTag.RefTagID);
                            continue;
                        }

                        TagConfigRecord tc = TagConfigCache[stuTag.RefTagID];
                        string tagName = tc.Name;
                        string prefix = tc.Prefix;

                        if (!prefix.Equals(string.Empty) && !tagName.Equals(string.Empty))
                            PrefixedTags[prefix][tagName].KeyChildren.Add(studentID); //直接加入。
                        else if (prefix.Equals(string.Empty) && !tagName.Equals(string.Empty))
                            NoPrefixTags[tagName].KeyChildren.Add(studentID);
                    }
                }
            }

            NavViewNode Root = new NavViewNode() { SharedKeysMap = this.SharedKeysMap, Name = "所有項目" };

            //將所有具有 Prefix 的項目加入到 Root。
            foreach (string prefix in PrefixedTags.SortedNames)
                foreach (string tagName in PrefixedTags[prefix].SortedNames)
                    Root[prefix][tagName].KeyChildren.AddRange(PrefixedTags[prefix][tagName].KeyChildren);

            //將所有沒有 Prefix 的項目加入到 Root。
            foreach (string tagName in NoPrefixTags.SortedNames)
                Root[tagName].KeyChildren.AddRange(NoPrefixTags[tagName].KeyChildren);

            Root["未分類"].KeyChildren.AddRange(NoTags.KeyChildren);

            Root.UpdateView(false);

            Root.ViewNode.Expand();
            return Root;
        }

        private void UpdateUI(NavViewNode PrefixCategoryNode)
        {
            TagTree.Nodes.Clear();
            TagTree.Nodes.Add(PrefixCategoryNode.ViewNode);

            if (SelectedPath.Count != 0)
            {
                var MySelectNode = SelectNode(SelectedPath, 0, TagTree.Nodes);
                if (MySelectNode != null)
                    TagTree.SelectedNode = MySelectNode;
            }
            TagTree.Enabled = true;
        }

        private Node SelectNode(List<string> selectPath, int level, NodeCollection nodeCollection)
        {
            foreach (var item in nodeCollection)
            {
                if (item is Node)
                {
                    var node = (Node)item;
                    if (IsSameTitle(node.Text, selectPath[level]))
                    {
                        if (selectPath.Count - 1 == level)
                            return node;
                        else
                        {
                            var childNode = SelectNode(selectPath, level + 1, node.Nodes);
                            if (childNode == null)
                                return node;
                            else
                                return childNode;
                        }
                    }
                }
            }
            return null;
        }

        private static bool IsSameTitle(string title1, string title2)
        {
            Regex r = new Regex(@"\(\d+\)");

            string title1Trim = r.Replace(title1, string.Empty);
            string title2Trim = r.Replace(title2, string.Empty);

            return title1Trim == title2Trim;
        }

        #endregion

        private void advTree1_AfterNodeSelect(object sender, AdvTreeNodeEventArgs e)
        {
            SetListPaneSource(e.Node);
        }

        private void advTree1_NodeClick(object sender, DevComponents.AdvTree.TreeNodeMouseEventArgs e)
        {
            SetListPaneSource(e.Node);
        }

        private void advTree1_NodeDoubleClick(object sender, DevComponents.AdvTree.TreeNodeMouseEventArgs e)
        {
            SetListPaneSource(e.Node);
        }

        private void SetListPaneSource(Node node)
        {
            //if (node == LoadingNode) return;
            if (DenyChangeListPaneSource) return;

            if (node == null || !NodeTagsMap.ContainsKey(node))
            {
                SetListPaneSource(new List<string>(), false, false);
                return;
            }

            bool selAll = (Control.ModifierKeys & Keys.Control) == Keys.Control;
            bool addToTemp = (Control.ModifierKeys & Keys.Shift) == Keys.Shift;
            SetListPaneSource(NodeTagsMap[node], selAll, addToTemp);
        }

        private void btnRefreshAll_Click(object sender, EventArgs e)
        {
            //ReserveTreeSelection();
            RenderTreeView();
        }

        /// <summary>
        /// 保留目前在 TreeView 上的選擇項目。
        /// </summary>
        private void ReserveTreeSelection()
        {
            #region 記錄選取的結點的完整路徑
            SelectedPath.Clear();
            var selectNode = TagTree.SelectedNode;
            if (selectNode != null)
            {
                while (selectNode != null)
                {
                    SelectedPath.Insert(0, selectNode.Text);
                    selectNode = selectNode.Parent;
                }
            }
            #endregion
        }

        private void RenderTreeView()
        {
            DenyChangeListPaneSource = true;
            //TagTree.Nodes.Clear();
            //TagTree.Nodes.Add(LoadingNode);
            TagTree.Enabled = false;
            Application.DoEvents();
            DenyChangeListPaneSource = false;

            if (TaskThread.IsBusy)
                TaskPadding = true;
            else
                TaskThread.RunWorkerAsync();
        }
    }
}
