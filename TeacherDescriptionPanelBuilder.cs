using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FISCA.Presentation;
using K12.Data;

namespace Tagging
{
    internal class TeacherDescriptionPanelBuilder : IDescriptionPaneBulider
    {
        #region IDescriptionPaneBulider 成員

        public DescriptionPane GetContent()
        {
            TaggingBar bar = new TaggingBar();
            bar.StatusVisible = false;
            List<TeacherTagRecord> stus;
            bar.GetTagsDelegate = key =>
            {
                stus = K12.Data.TeacherTag.SelectByTeacherID(key);
                return stus.ConvertAll<GeneralTagRecord>(x => x);
            };

            bar.GetDescriptionDelegate = key =>
            {
                TeacherRecord record = Teacher.SelectByID(key);
                return string.Format("{0}", record.Name);
            };

            TeacherTag.AfterInsert += bar.TagRecordChangedEventHandler;
            TeacherTag.AfterUpdate += bar.TagRecordChangedEventHandler;
            TeacherTag.AfterDelete += bar.TagRecordChangedEventHandler;

            bar.Disposed += delegate
            {
                TeacherTag.AfterInsert -= bar.TagRecordChangedEventHandler;
                TeacherTag.AfterUpdate -= bar.TagRecordChangedEventHandler;
                TeacherTag.AfterDelete -= bar.TagRecordChangedEventHandler;
            };

            return bar;
        }

        #endregion
    }
}
