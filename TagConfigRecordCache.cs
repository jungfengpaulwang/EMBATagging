using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using K12.Data;

namespace Tagging
{
    /// <summary>
    /// 
    /// </summary>
    public class TagConfigRecordCache : CacheManager<TagConfigRecord>
    {
        /// <summary>
        /// 
        /// </summary>
        public TagConfigRecordCache()
        {
            TagConfig.AfterInsert += ConfigChangedHandler;
            TagConfig.AfterUpdate += ConfigChangedHandler;
            TagConfig.AfterDelete += ConfigChangedHandler;
            SyncKeys = new List<string>();
        }

        private List<string> SyncKeys { get; set; }

        private bool Initialized = false;

        internal void ConfigChangedHandler(object sender, DataChangedEventArgs args)
        {
            if (SyncKeys == null)
                SyncKeys = new List<string>();

            SyncKeys = SyncKeys.Union(args.PrimaryKeys).ToList();

            if (K12ItemUpdated != null)
                K12ItemUpdated(this, args);
        }

        /// <summary>
        /// 當 K12 的項目變更事件發生時(包含 Insert、Update、Delete)。
        /// </summary>
        public event EventHandler<DataChangedEventArgs> K12ItemUpdated;

        /// <summary>
        /// 
        /// </summary>
        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.Synchronized)]
        public void WaitInitializeComplete()
        {
            if (!Initialized)
            {
                SyncAll();
                Initialized = true;
            }
        }

        /// <summary>
        /// 等待 TagConfig 延遲工作執行完成。
        /// </summary>
        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.Synchronized)]
        public void WaitLazySyncComplete()
        {
            if (SyncKeys.Count > 0)
            {
                SyncData(SyncKeys);
                SyncKeys = new List<string>();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        protected override Dictionary<string, TagConfigRecord> GetAllData()
        {
            return TagConfig.SelectAll().ToDictionary(arg => arg.ID);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="primaryKeys"></param>
        /// <returns></returns>
        protected override Dictionary<string, TagConfigRecord> GetData(IEnumerable<string> primaryKeys)
        {
            return TagConfig.SelectByIDs(primaryKeys).ToDictionary(arg => arg.ID);
        }

        /// <summary>
        /// 依類別所屬類型取得類別資訊。
        /// </summary>
        /// <param name="category">TagCategory。</param>
        /// <returns></returns>
        public List<TagConfigRecord> GetByCategory(TagCategory category)
        {
            List<TagConfigRecord> records = new List<TagConfigRecord>();
            string condition = category.ToString();
            foreach (TagConfigRecord record in this)
            {
                if (record.Category == condition)
                    records.Add(record);
            }
            return records;
        }
    }
}
