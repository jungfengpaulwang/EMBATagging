using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FISCA.Presentation;
using K12.Data;

namespace Tagging
{
    internal class CourseDescriptionPanelBuilder : IDescriptionPaneBulider
    {
        #region IDescriptionPaneBulider 成員

        public DescriptionPane GetContent()
        {
            TaggingBar bar = new TaggingBar();
            bar.StatusVisible = false;
            List<CourseTagRecord> stus;
            bar.GetTagsDelegate = key =>
            {
                stus = K12.Data.CourseTag.SelectByCourseID(key);
                return stus.ConvertAll<GeneralTagRecord>(x => x);
            };

            bar.GetDescriptionDelegate = key =>
            {
                CourseRecord record = Course.SelectByID(key);
                return string.Format("{0}", record.Name);
            };

            CourseTag.AfterInsert += bar.TagRecordChangedEventHandler;
            CourseTag.AfterUpdate += bar.TagRecordChangedEventHandler;
            CourseTag.AfterDelete += bar.TagRecordChangedEventHandler;

            bar.Disposed += delegate
            {
                CourseTag.AfterInsert -= bar.TagRecordChangedEventHandler;
                CourseTag.AfterUpdate -= bar.TagRecordChangedEventHandler;
                CourseTag.AfterDelete -= bar.TagRecordChangedEventHandler;
            };

            return bar;
        }

        #endregion
    }
}
