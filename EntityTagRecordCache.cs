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
    public class EntityTagRecordCache<T> : CacheManager<List<T>>
        where T : GeneralTagRecord
    {
        /// <summary>
        /// 
        /// </summary>
        public EntityTagRecordCache()
        {
            SyncKeys = new List<string>();
        }

        private List<string> SyncKeys { get; set; }

        private bool Initialized = false;

        internal void InsertHandler(object sender, DataChangedEventArgs args)
        {
            if (K12ItemUpdated != null)
                K12ItemUpdated(this, args);
        }

        internal void GeneralChangedHandler(object sender, DataChangedEventArgs args)
        {
            if (SyncKeys == null)
                SyncKeys = new List<string>();

            Dictionary<string, string> newSyncKeys = new Dictionary<string, string>();
            lock (GeneralTagIDEntityIDMap)
            {
                foreach (string tagKey in args.PrimaryKeys)
                {
                    if (GeneralTagIDEntityIDMap.ContainsKey(tagKey))
                    {
                        if (!newSyncKeys.ContainsKey(GeneralTagIDEntityIDMap[tagKey]))
                            newSyncKeys.Add(GeneralTagIDEntityIDMap[tagKey], null);
                    }
                    else
                        Console.WriteLine("General Tag 同步異常 Key {0} 找不到。", tagKey);
                }
            }

            SyncKeys = SyncKeys.Union(newSyncKeys.Keys).ToList();

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
        public void WaitLazySyncComplete()
        {
            if (SyncKeys.Count > 0)
            {
                SyncData(SyncKeys);
                SyncKeys = new List<string>();
            }
        }

        internal Func<List<T>> GetAllRecord { get; set; }

        internal Func<IEnumerable<string>, List<T>> GetRecord { get; set; }

        /// <summary>
        /// GeneralTagID -> EntityID 對照表。
        /// </summary>
        private Dictionary<string, string> GeneralTagIDEntityIDMap = new Dictionary<string, string>();

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        protected override Dictionary<string, List<T>> GetAllData()
        {
            GeneralTagIDEntityIDMap = new Dictionary<string, string>();
            Dictionary<string, List<T>> result = new Dictionary<string, List<T>>();
            lock (GeneralTagIDEntityIDMap)
            {
                foreach (T record in GetAllRecord())
                {
                    GeneralTagIDEntityIDMap.Add(record.ID, record.RefEntityID);

                    if (!result.ContainsKey(record.RefEntityID))
                        result.Add(record.RefEntityID, new List<T>());

                    result[record.RefEntityID].Add(record);
                }
            }
            return result;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="primaryKeys"></param>
        /// <returns></returns>
        protected override Dictionary<string, List<T>> GetData(IEnumerable<string> primaryKeys)
        {
            Dictionary<string, List<T>> result = new Dictionary<string, List<T>>();

            lock (GeneralTagIDEntityIDMap)
            {
                foreach (T record in GetRecord(primaryKeys))
                {
                    if (GeneralTagIDEntityIDMap.ContainsKey(record.ID))
                        GeneralTagIDEntityIDMap[record.ID] = record.RefEntityID;
                    else
                        GeneralTagIDEntityIDMap.Add(record.ID, record.RefEntityID);

                    if (!result.ContainsKey(record.RefEntityID))
                        result.Add(record.RefEntityID, new List<T>());

                    result[record.RefEntityID].Add(record);
                }
            }

            foreach (string primaryKey in primaryKeys)
            {
                if (!result.ContainsKey(primaryKey))
                    result.Add(primaryKey, new List<T>());
            }

            return result;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="idList"></param>
        /// <returns></returns>
        public List<T> GetByEntityIDs(IEnumerable<string> idList)
        {
            List<T> records = new List<T>();
            foreach (string id in idList)
            {
                if (this[id] != null)
                    records.AddRange(this[id]);
            }
            return records;
        }
    }
}
