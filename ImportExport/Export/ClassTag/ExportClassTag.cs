using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SmartSchool.API.PlugIn;
using K12.Data;

namespace Tagging
{
    class ExportClassTag: SmartSchool.API.PlugIn.Export.Exporter
    {
        List<string> ExportItemList = new List<string>();

        public ExportClassTag()
        {
            this.Image = null;
            this.Text = "匯出班級類別";

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
                Dictionary<string, ClassTagEntity> ClaTagDict = new Dictionary<string, ClassTagEntity>();
                // 取得班級類別
                foreach (ClassTagRecord claTagRec in ClassTag.SelectByClassIDs(e.List))
                {
                    if (ClaTagDict.ContainsKey(claTagRec.RefClassID))
                        ClaTagDict[claTagRec.RefClassID].AddPrefixName(claTagRec.Prefix, claTagRec.Name);
                    else
                    {
                        ClassTagEntity stn = new ClassTagEntity();
                        stn.ClassID = claTagRec.RefClassID;
                        stn.AddPrefixName(claTagRec.Prefix, claTagRec.Name);
                        ClaTagDict.Add(claTagRec.RefClassID, stn);                    
                    }
                }

                // 讀取組合後的學生類別
                foreach (ClassTagEntity cla in ClaTagDict.Values)
                {
                    foreach (KeyValuePair<string, List<string>> data in cla.GetPrefixNameDic())
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
                                row.ID = cla.ClassID;

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
                prlp.SaveLog("班級.匯出類別", "匯出", "共匯出" + ClaTagDict.Values.Count + "筆班級類別資料.");
            };  
        }
    }
}
