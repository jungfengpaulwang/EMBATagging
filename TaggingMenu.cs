using System;
using System.Collections.Generic;
using FISCA.Presentation;
using K12.Data;
using FISCA.Permission;
using FISCA.LogAgent;
using FISCA.Authentication;
using System.Linq;

namespace Tagging
{
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    internal class TaggingMenu<T>
        where T : GeneralTagRecord
    {
        /// <summary>
        /// 指定類別的權限代碼。
        /// </summary>
        private string[] AssignCodes { get; set; }
        /// <summary>
        /// 管理類別的權限代碼。
        /// </summary>
        private string[] ManageCodes { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="assignCodes">指定類別的權限代碼。</param>
        /// <param name="manageCodes">管理類別的權限代碼。</param>
        internal TaggingMenu(string[] assignCodes, string[] manageCodes)
        {
            AssignCodes = assignCodes;
            ManageCodes = manageCodes;
        }

        public string EntityTitle { get; set; }

        public TagCategory Category { get; set; }

        public EntityTagRecordCache<T> EntityTagCache { get; set; }

        public Func<List<string>> GetSelectedEntityList { get; set; }

        public Action<List<T>> InsertTagRecords { get; set; }

        public Action<List<T>> RemoveTagRecords { get; set; }

        /// <summary>
        /// 建立新的 TagRecord。(EntityID,TagConfigID)。
        /// </summary>
        public Func<string, string, T> NewTagRecord { get; set; }

        /// <summary>
        /// 開啟 Tag 管理畫面。
        /// </summary>
        public Action OpenManagerDelegate { get; set; }

        internal void MenuOpen(object sender, PopupOpenEventArgs e)
        {
            TagCache.TagConfig.WaitInitializeComplete();
            TagCache.TagConfig.WaitLazySyncComplete();

            List<string> selectedEntity = GetSelectedEntityList();
            if (selectedEntity.Count > 0)
            {
                //建立指定 Category 的所有 Tag 項目。
                List<TagConfigRecord> noprefix = new List<TagConfigRecord>();
                List<string> prefixList = new List<string>();
                foreach (TagConfigRecord record in GetSortedTags())
                {
                    if (string.IsNullOrEmpty(record.Prefix))
                        noprefix.Add(record);
                    else
                    {
                        if (!prefixList.Contains(record.Prefix))
                        {
                            SetPrefixEvent(selectedEntity, e.VirtualButtons[record.Prefix]);
                            prefixList.Add(record.Prefix);
                        }

                        CreateTagMenuItem(e, selectedEntity, record);
                    }
                }

                //不具有 Prefix 的要建立在根功能表。
                MenuButton topPrefixItem = e.VirtualButtons;
                foreach (TagConfigRecord record in noprefix)
                {
                    MenuButton tagItem = e.VirtualButtons[record.Name];
                    SetEvents(selectedEntity, record, tagItem, topPrefixItem);
                }
                PrefixMenuOpen(selectedEntity, topPrefixItem);
            }

            e.VirtualButtons["類別管理..."].BeginGroup = true;
            e.VirtualButtons["類別管理..."].Enable = Accept(ManageCodes);
            e.VirtualButtons["類別管理..."].Click += delegate
            {
                OpenManagerDelegate();
            };
        }

        private List<TagConfigRecord> GetSortedTags()
        {
            List<TagConfigRecord> items = TagCache.TagConfig.GetByCategory(Category);
            items.Sort((x, y) =>
            {
                return x.FullName.CompareTo(y.FullName);
            });
            return items;
        }

        private void CreateTagMenuItem(PopupOpenEventArgs e, List<string> selected, TagConfigRecord tcRecord)
        {
            string prefix = tcRecord.Prefix;

            MenuButton tagItem = e.VirtualButtons[prefix][tcRecord.Name];
            MenuButton prefixItem = e.VirtualButtons[prefix];

            SetEvents(selected, tcRecord, tagItem, prefixItem);
        }

        private void SetPrefixEvent(List<string> selected, MenuButton prefixItem)
        {
            prefixItem.PopupOpen += delegate
            {
                PrefixMenuOpen(selected, prefixItem);
            };
        }

        private void SetEvents(List<string> selected, TagConfigRecord tcRecord, MenuButton tagItem, MenuButton prefixItem)
        {
            tagItem.Tag = tcRecord; //TagConfigRecord。
            tagItem.AutoCheckOnClick = true;
            tagItem.AutoCollapseOnClick = false;
            tagItem.Enable = Accept(AssignCodes);
            tagItem.Click += delegate(object sender, EventArgs e1)
            {
                TagMenuCheckChanged(selected, sender as MenuButton);
                CalcCheckedState(selected, prefixItem);
            };
        }

        private void PrefixMenuOpen(List<string> selected, MenuButton prefixItem)
        {
            //當沒算過時，計算 Checked State。
            if (string.IsNullOrEmpty("" + prefixItem.Tag))
                CalcCheckedState(selected, prefixItem);

            Dictionary<string, int> tags = prefixItem.Tag as Dictionary<string, int>;
            int selectedCount = selected.Count;
            foreach (var item in prefixItem.Items)
            {
                if (item.Tag is TagConfigRecord)
                {
                    string tagID = (item.Tag as TagConfigRecord).ID;
                    if (tags.ContainsKey(tagID) && tags[tagID] == selectedCount)
                        item.Checked = true;
                    else
                        item.Checked = false;
                }
            }
        }

        private void CalcCheckedState(List<string> selected, MenuButton prefixMenuButton)
        {
            Dictionary<string, int> tagRefCountMap = new Dictionary<string, int>();

            EntityTagCache.SyncData(selected);
            foreach (T entityTag in EntityTagCache.GetByEntityIDs(selected))
            {
                if (!tagRefCountMap.ContainsKey(entityTag.RefTagID))
                    tagRefCountMap.Add(entityTag.RefTagID, 0);
                tagRefCountMap[entityTag.RefTagID]++;
            }
            prefixMenuButton.Tag = tagRefCountMap;
        }

        private void TagMenuCheckChanged(List<string> selectedEntityIDs, MenuButton mb)
        {
            List<T> addList = new List<T>();
            List<T> removeList = new List<T>();

            TagConfigRecord tag = mb.Tag as TagConfigRecord;

            List<T> tagRecords = EntityTagCache.GetByEntityIDs(selectedEntityIDs);
            Dictionary<string, Dictionary<string, T>> TagRecordMap = new Dictionary<string, Dictionary<string, T>>();
            foreach (T each in tagRecords)
            {
                if (!TagRecordMap.ContainsKey(each.RefEntityID))
                    TagRecordMap.Add(each.RefEntityID, new Dictionary<string, T>());

                TagRecordMap[each.RefEntityID].Add(each.RefTagID, each);
            }

            //LogSaver log = ApplicationLog.CreateLogSaverInstance();

            //Student.SelectByIDs(selectedEntityIDs);//先快取資料。
            foreach (string entityId in selectedEntityIDs)
            {
                if (!TagRecordMap.ContainsKey(entityId)) //防止 Key 不存在爆掉。
                    TagRecordMap.Add(entityId, new Dictionary<string, T>());

                if (mb.Checked == true)
                {
                    if (!TagRecordMap[entityId].ContainsKey(tag.ID)) //不在清單中就新增。
                    {
                        if (string.IsNullOrEmpty(entityId))
                            Console.WriteLine("Empty");
                        if (string.IsNullOrEmpty(tag.ID))
                            Console.WriteLine("Emtpy");

                        addList.Add(NewTagRecord(entityId, tag.ID));

                        //string actionBy = string.Format("類別.{0}類別", EntityTitle);
                        //string action = string.Format("指定{0}類別", EntityTitle);
                        //StudentRecord student = Student.SelectByID(entityId);
                        //log.AddBatch(actionBy, action, "description:" + student.Name);
                    }
                }
                else
                {
                    if (TagRecordMap[entityId].ContainsKey(tag.ID)) //如果在清單中就移除。
                    {
                        if (string.IsNullOrEmpty(entityId))
                            Console.WriteLine("Empty");
                        if (string.IsNullOrEmpty(tag.ID))
                            Console.WriteLine("Emtpy");

                        removeList.Add(TagRecordMap[entityId][tag.ID]);
                        //string actionBy = string.Format("類別.{0}類別", EntityTitle);
                        //string action = string.Format("移除{0}類別", EntityTitle);
                        //StudentRecord student = Student.SelectByID(entityId);
                        //log.AddBatch(actionBy, action, "description:" + student.Name);
                    }
                }
            }

            if (addList.Count > 0)
            {
                InsertTagRecords(addList);
                EntityTagCache.SyncData(from item in addList select item.RefEntityID);
            }

            if (removeList.Count > 0)
            {
                List<T> aa = (from item in removeList where item.ID == null select item).ToList();
                RemoveTagRecords(removeList);
            }

            EntityTagCache.WaitLazySyncComplete();
            //log.LogBatch();
        }

        private bool Accept(string[] codes)
        {
            bool accept = false;

            foreach (string code in codes)
                accept |= UserAcl.Current[code].Executable;

            return accept;
        }
    }
}
