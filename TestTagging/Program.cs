using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FISCA;
using Customization.Tagging;

namespace TestTagging
{
    public static class Program
    {
        [MainMethod(StartupPriority.FirstAsynchronized)]
        public static void Main()
        {
            CustomizationService.Register<GetStudentStatusList>(() =>
            {
                List<StatusItem> status = new List<StatusItem>();
                status.Add(new StatusItem() { Status = K12.Data.StudentRecord.StudentStatus.一般, Text = "A狀態" });
                status.Add(new StatusItem() { Status = K12.Data.StudentRecord.StudentStatus.休學, Text = "B狀態" });
                status.Add(new StatusItem() { Status = K12.Data.StudentRecord.StudentStatus.刪除, Text = "C狀態" });

                return status;
            });
        }
    }
}
