using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SmartSchool.API.PlugIn;
using K12.Data;

namespace Tagging
{
    class ExportTeacherTag : SmartSchool.API.PlugIn.Export.Exporter
    {
        List<string> ExportItemList = new List<string>();

        public ExportTeacherTag()
        {
            this.Image = null;
            this.Text = "匯出教師類別";

            // 可匯出項目            
            ExportItemList.Add("群組");
            ExportItemList.Add("類別名稱");

        }

        public override void InitializeExport(SmartSchool.API.PlugIn.Export.ExportWizard wizard)
        {

            wizard.ExportableFields.AddRange(ExportItemList);

            wizard.ExportPackage += delegate(object sender, SmartSchool.API.PlugIn.Export.ExportPackageEventArgs e)
            {
                // 教師類別組合
                Dictionary<string, TeacherTagEntity> teachTagDict = new Dictionary<string, TeacherTagEntity>();
                // 取得教師類別
                foreach (TeacherTagRecord teachTagRec in TeacherTag.SelectByTeacherIDs(e.List))
                {
                    if (teachTagDict.ContainsKey(teachTagRec.RefTeacherID))
                        teachTagDict[teachTagRec.RefTeacherID].AddPrefixName(teachTagRec.Prefix, teachTagRec.Name);
                    else
                    {
                        TeacherTagEntity stn = new TeacherTagEntity();
                        stn.TeacherID = teachTagRec.RefTeacherID;
                        stn.AddPrefixName(teachTagRec.Prefix, teachTagRec.Name);
                        teachTagDict.Add(teachTagRec.RefTeacherID, stn);
                    }
                }

                // 讀取組合後的教師類別
                foreach (TeacherTagEntity teach in teachTagDict.Values)
                {
                    foreach (KeyValuePair<string, List<string>> data in teach.GetPrefixNameDic())
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
                                row.ID = teach.TeacherID;

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
                prlp.SaveLog("教師.匯出類別", "匯出", "共匯出" + teachTagDict.Values.Count + "筆教師類別資料.");
            };  
        }
    }
}
