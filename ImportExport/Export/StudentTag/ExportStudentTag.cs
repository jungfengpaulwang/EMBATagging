using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SmartSchool.API.PlugIn;
using K12.Data;

namespace Tagging
{
    class ExportStudentTag: SmartSchool.API.PlugIn.Export.Exporter
    {
        List<string> ExportItemList = new List<string>();

        public ExportStudentTag()
        {
            this.Image = null;
            this.Text = "匯出學生類別";

            // 可匯出項目            
            ExportItemList.Add("群組");
            ExportItemList.Add("類別名稱");

        }

        public override void InitializeExport(SmartSchool.API.PlugIn.Export.ExportWizard wizard)
        {
            wizard.ExportableFields.AddRange(ExportItemList);

            wizard.ExportPackage += delegate(object sender, SmartSchool.API.PlugIn.Export.ExportPackageEventArgs e)
            {
                // 學生類別組合
                Dictionary<string, StudentTagEntity> StudTagDict = new Dictionary<string, StudentTagEntity>();

                // 取得學生類別
                foreach (StudentTagRecord studTagRec in StudentTag.SelectByStudentIDs(e.List))
                {
                    if (StudTagDict.ContainsKey(studTagRec.RefStudentID))
                        StudTagDict[studTagRec.RefStudentID].AddPrefixName(studTagRec.Prefix, studTagRec.Name);
                    else
                    {
                        StudentTagEntity stn = new StudentTagEntity();
                        stn.StudentID = studTagRec.RefStudentID;
                        stn.AddPrefixName(studTagRec.Prefix, studTagRec.Name);
                        StudTagDict.Add(studTagRec.RefStudentID, stn);                    
                    }
                }

                // 讀取組合後的學生類別
                foreach (StudentTagEntity ste in StudTagDict.Values)
                { 
                    foreach (KeyValuePair<string, List<string>> data in ste.GetPrefixNameDic())
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
                                row.ID = ste.StudentID;

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
                prlp.SaveLog("學生.匯出類別", "匯出", "共匯出" + StudTagDict.Values.Count + "筆學生類別資料.");
            };  
        }
    }
}
