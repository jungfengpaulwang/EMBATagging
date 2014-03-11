using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SmartSchool.API.PlugIn;
using K12.Data;

namespace Tagging
{
    class ExportCourseTag : SmartSchool.API.PlugIn.Export.Exporter
    {
        List<string> ExportItemList = new List<string>();

        public ExportCourseTag()
        {
            this.Image = null;
            this.Text = "匯出課程類別";

            // 可匯出項目            
            ExportItemList.Add("群組");
            ExportItemList.Add("類別名稱");

        }

        public override void InitializeExport(SmartSchool.API.PlugIn.Export.ExportWizard wizard)
        {
            wizard.ExportableFields.AddRange(ExportItemList);

            wizard.ExportPackage += delegate(object sender, SmartSchool.API.PlugIn.Export.ExportPackageEventArgs e)
            {
                // 班級類別組合
                Dictionary<string, CourseTagEntity> courTagDict = new Dictionary<string, CourseTagEntity>();
                // 取得班級類別
                foreach (CourseTagRecord courTagRec in CourseTag.SelectByCourseIDs(e.List))
                {
                    if (courTagDict.ContainsKey(courTagRec.RefCourseID))
                        courTagDict[courTagRec.RefCourseID].AddPrefixName(courTagRec.Prefix, courTagRec.Name);
                    else
                    {
                        CourseTagEntity cou = new CourseTagEntity();
                        cou.ClassID = courTagRec.RefCourseID;
                        cou.AddPrefixName(courTagRec.Prefix, courTagRec.Name);
                        courTagDict.Add(courTagRec.RefCourseID, cou);                    
                    }
                }

                // 讀取組合後的學生類別
                foreach (CourseTagEntity cour in courTagDict.Values)
                {
                    foreach (KeyValuePair<string, List<string>> data in cour.GetPrefixNameDic())
                    {
                        // 當群組空白
                        string key = string.Empty;
                        if (data.Key != " ")
                            key = data.Key;
                        
                        // 類別名稱
                        foreach (string str in data.Value)
                        {
                            RowData row = new RowData();
                            foreach (string field in e.ExportFields)
                            {
                                row.ID = cour.ClassID;

                                if (field == "群組")
                                    row.Add(field, key);

                                if (field == "類別名稱")
                                    row.Add(field, str);                                
                            }
                            e.Items.Add(row);
                        }
                    }
                }
                
                PermRecLogProcess prlp = new PermRecLogProcess();
                prlp.SaveLog("課程.匯出類別", "匯出", "共匯出" + courTagDict.Values.Count + "筆課程類別資料.");
            };  
        }
    }
}
