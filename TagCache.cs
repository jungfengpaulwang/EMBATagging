using K12.Data;
using System.Collections.Generic;
using System.Linq;

namespace Tagging
{
    /// <summary>
    /// 快取所有 Tag 相關資料，內容自動與 K12.Data 同步。
    /// </summary>
    public static class TagCache
    {
        /// <summary>
        /// 提供 TagConfig 快取，不分類型(Category)。如果只需要特定類型，請使用 GetByCategory() 方法。
        /// </summary>
        public static TagConfigRecordCache TagConfig { get; internal set; }

        /// <summary>
        /// 提供 Student 的 Tag 快取。
        /// </summary>
        public static EntityTagRecordCache<StudentTagRecord> StudentTag { get; internal set; }

        /// <summary>
        /// 提供 Class 的 Tag 快取。
        /// </summary>
        public static EntityTagRecordCache<ClassTagRecord> ClassTag { get; internal set; }

        /// <summary>
        /// 提供 Teacher 的 Tag 快取。
        /// </summary>
        public static EntityTagRecordCache<TeacherTagRecord> TeacherTag { get; internal set; }

        /// <summary>
        /// 提供 Course 的 Tag 快取。
        /// </summary>
        public static EntityTagRecordCache<CourseTagRecord> CourseTag { get; internal set; }
    }
}
