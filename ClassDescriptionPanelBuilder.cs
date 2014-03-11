using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FISCA.Presentation;
using K12.Data;

namespace Tagging
{
    internal class ClassDescriptionPanelBuilder : IDescriptionPaneBulider
    {
        #region IDescriptionPaneBulider 成員

        public DescriptionPane GetContent()
        {
            TaggingBar bar = new TaggingBar();
            bar.StatusVisible = false;
            List<ClassTagRecord> stus;
            bar.GetTagsDelegate = key =>
            {
                stus = K12.Data.ClassTag.SelectByClassID(key);
                return stus.ConvertAll<GeneralTagRecord>(x => x);
            };

            bar.GetDescriptionDelegate = key =>
            {
                ClassRecord cls = Class.SelectByID(key);

                string desc = cls.Name;
                if (cls.Teacher != null)
                    desc = string.Format("{0} (導師：{1})", cls.Name, cls.Teacher.Name);

                return desc;
            };

            ClassTag.AfterInsert += bar.TagRecordChangedEventHandler;
            ClassTag.AfterUpdate += bar.TagRecordChangedEventHandler;
            ClassTag.AfterDelete += bar.TagRecordChangedEventHandler;

            bar.Disposed += delegate
            {
                ClassTag.AfterInsert -= bar.TagRecordChangedEventHandler;
                ClassTag.AfterUpdate -= bar.TagRecordChangedEventHandler;
                ClassTag.AfterDelete -= bar.TagRecordChangedEventHandler;
            };

            return bar;
        }

        #endregion
    }
}
