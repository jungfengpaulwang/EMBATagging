using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FISCA.Presentation;
using K12.Data;
using System.Windows.Forms;
using DevComponents.DotNetBar;
using FISCA.Presentation.Controls;

namespace Tagging
{
    internal class StudentDescriptionPanelBuilder : IDescriptionPaneBulider
    {
        #region IDescriptionPaneBulider 成員

        public DescriptionPane GetContent()
        {
            TaggingBar bar = new TaggingBar();
            bar.StatusVisible = true;
            List<StudentTagRecord> stus;

            bar.GetTagsDelegate = key =>
            {
                stus = K12.Data.StudentTag.SelectByStudentID(key);
                return stus.ConvertAll<GeneralTagRecord>(x => x);
            };

            bar.GetDescriptionDelegate = key =>
            {
                StudentRecord stu = Student.SelectByID(key);

                if (stu.Class == null)
                    return string.Format("{0} {1}", stu.Name, stu.StudentNumber);
                else
                {
                    if (stu.SeatNo == null)
                        return string.Format("{0} {1} {2}", stu.Class.Name, stu.Name, stu.StudentNumber);
                    else
                        return string.Format("{0}({1}) {2} {3}", stu.Class.Name, stu.SeatNo, stu.Name, stu.StudentNumber);
                }
            };

            StudentTag.AfterInsert += bar.TagRecordChangedEventHandler;
            StudentTag.AfterUpdate += bar.TagRecordChangedEventHandler;
            StudentTag.AfterDelete += bar.TagRecordChangedEventHandler;

            bar.Disposed += delegate
            {
                StudentTag.AfterInsert -= bar.TagRecordChangedEventHandler;
                StudentTag.AfterUpdate -= bar.TagRecordChangedEventHandler;
                StudentTag.AfterDelete -= bar.TagRecordChangedEventHandler;
            };

            return bar;
        }

        #endregion
    }
}
