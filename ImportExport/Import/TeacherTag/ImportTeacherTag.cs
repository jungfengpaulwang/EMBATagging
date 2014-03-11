using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SmartSchool.API.PlugIn;
using K12.Data;

namespace Tagging
{
    class ImportTeacherTag : SmartSchool.API.PlugIn.Import.Importer
    {
        // 可匯入項目
        List<string> ImportItemList = new List<string>();

        public ImportTeacherTag()
        {
            this.Image = null;
            this.Text = "匯入教師類別";
            ImportItemList.Add("群組");
            ImportItemList.Add("類別名稱");
        }

        public override void InitializeImport(SmartSchool.API.PlugIn.Import.ImportWizard wizard)
        {
            Dictionary<string, TeacherRecord> Teachers = new Dictionary<string, TeacherRecord>();
            Dictionary<string, List<TeacherTagRecord>> TeacherTagRecDic = new Dictionary<string, List<TeacherTagRecord>>();

            Dictionary<string, Dictionary<string, string>> TeachTagNameDic = DATeacherTransfer.GetTeacherTagNameDic();

            // 取得可加入學生 TagName            
            wizard.PackageLimit = 3000;
            wizard.ImportableFields.AddRange(ImportItemList);
            wizard.ValidateStart += delegate(object sender, SmartSchool.API.PlugIn.Import.ValidateStartEventArgs e)
            {
                // 取得學生資料
                foreach (TeacherRecord TeacherRec in Teacher.SelectByIDs(e.List))
                    if (!Teachers.ContainsKey(TeacherRec.ID))
                    {
                        Teachers.Add(TeacherRec.ID, TeacherRec);
                        TeacherTagRecDic.Add(TeacherRec.ID, new List<TeacherTagRecord>());
                    }



                // 取得學生類別
                foreach (TeacherTagRecord teachTag in TeacherTag.SelectByTeacherIDs(Teachers.Keys))
                {
                    //if (!StudTagRecDic.ContainsKey(studTag.RefStudentID))
                    //{
                    //    List<JHStudentTagRecord> rec = new List<JHStudentTagRecord> ();
                    //    rec.Add(studTag );
                    //    StudTagRecDic.Add(studTag.RefStudentID,rec);                       
                    //}
                    //else
                    if (TeacherTagRecDic.ContainsKey(teachTag.RefTeacherID))
                        TeacherTagRecDic[teachTag.RefTeacherID].Add(teachTag);
                }
            };

            wizard.ValidateRow += delegate(object sender, SmartSchool.API.PlugIn.Import.ValidateRowEventArgs e)
            {
                int i = 0;

                // 檢查學生是否存在
                TeacherRecord teachRec = null;
                if (Teachers.ContainsKey(e.Data.ID))
                    teachRec = Teachers[e.Data.ID];
                else
                {
                    e.ErrorMessage = "沒有這位老師" + e.Data.ID;
                    return;
                }

                // 驗證資料
                foreach (string field in e.SelectFields)
                {
                    string value = e.Data[field].Trim();

                    // 驗證$無法匯入
                    if (value.IndexOf('$') > -1)
                    {
                        e.ErrorFields.Add(field, "儲存格有$無法匯入.");
                        break;
                    }


                    if (field == "類別名稱")
                    {
                        if (string.IsNullOrEmpty(value))
                        {
                            e.ErrorFields.Add(field, "不允許空白");
                        }
                    }
                }

            };

            wizard.ImportPackage += delegate(object sender, SmartSchool.API.PlugIn.Import.ImportPackageEventArgs e)
            {
                // 目前學生類別管理，沒有新增標示類別，有的就不更動跳過。

                Dictionary<string, List<RowData>> id_Rows = new Dictionary<string, List<RowData>>();
                foreach (RowData data in e.Items)
                {
                    if (!id_Rows.ContainsKey(data.ID))
                        id_Rows.Add(data.ID, new List<RowData>());
                    id_Rows[data.ID].Add(data);
                }

                List<TeacherTagRecord> InsertList = new List<TeacherTagRecord>();
                //                List<JHStudentTagRecord> UpdateList = new List<JHStudentTagRecord>();

                // 放需要新增的教師類別
                Dictionary<string, List<string>> NeedAddPrefixName = new Dictionary<string, List<string>>();

                // 檢查用 List
                List<string> CheckStudTagName = new List<string>();

                foreach (KeyValuePair<string, Dictionary<string, string>> data in TeachTagNameDic)
                {
                    foreach (KeyValuePair<string, string> data1 in data.Value)
                        CheckStudTagName.Add(data.Key + data1.Key);
                }

                // 檢查類別是否已經存在
                foreach (string id in id_Rows.Keys)
                {
                    if (!TeacherTagRecDic.ContainsKey(id))
                        continue;
                    foreach (RowData data in id_Rows[id])
                    {
                        string strPrefix = string.Empty, strName = string.Empty;

                        if (data.ContainsKey("群組"))
                            strPrefix = data["群組"];

                        if (data.ContainsKey("類別名稱"))
                            strName = data["類別名稱"];

                        string FullName = strPrefix + strName;

                        // 需要新增的,
                        if (!CheckStudTagName.Contains(FullName))
                        {
                            CheckStudTagName.Add(FullName);
                            if ((NeedAddPrefixName.ContainsKey(strPrefix)))
                                NeedAddPrefixName[strPrefix].Add(strName);
                            else
                            {
                                List<string> Names = new List<string>();
                                Names.Add(strName);
                                NeedAddPrefixName.Add(strPrefix, Names);
                            }
                        }
                    }
                }

                // 新增至學生類別管理
                List<TagConfigRecord> Recs = new List<TagConfigRecord>();
                foreach (KeyValuePair<string, List<string>> data in NeedAddPrefixName)
                {
                    foreach (string data1 in data.Value)
                    {
                        TagConfigRecord rec = new TagConfigRecord();
                        rec.Category = "Teacher";
                        rec.Prefix = data.Key;
                        rec.Name = data1;
                        rec.Color = System.Drawing.Color.White;
                        Recs.Add(rec);
                    }
                }
                TagConfig.Insert(Recs);

                TeachTagNameDic.Clear();

                // 重新取得
                TeachTagNameDic = DATeacherTransfer.GetTeacherTagNameDic();

                foreach (string id in id_Rows.Keys)
                {
                    if (!TeacherTagRecDic.ContainsKey(id))
                        continue;
                    foreach (RowData data in id_Rows[id])
                    {
                        string strPrefix = string.Empty, strName = string.Empty;

                        if (data.ContainsKey("群組"))
                            strPrefix = data["群組"];

                        if (data.ContainsKey("類別名稱"))
                            strName = data["類別名稱"];

                        // 欄位有在 Tag Prefix 內
                        bool isInsert = true;

                        foreach (TeacherTagRecord rec in TeacherTagRecDic[id])
                        {
                            if (rec.Prefix == strPrefix && rec.Name == strName)
                            {
                                isInsert = false;
                                break;
                            }
                        }

                        if (isInsert)
                        {
                            // 學生類別管理名稱對照
                            if (TeachTagNameDic.ContainsKey(strPrefix))
                            {
                                if (TeachTagNameDic[strPrefix].ContainsKey(strName))
                                {
                                    TeacherTagRecord TeachTag = new TeacherTagRecord();
                                    TeachTag.RefEntityID = id;
                                    TeachTag.RefTagID = TeachTagNameDic[strPrefix][strName];
                                    InsertList.Add(TeachTag);
                                }
                            }
                        }
                    }
                }

                try
                {
                    if (InsertList.Count > 0)
                        Insert(InsertList);

                    //if (UpdateList.Count > 0)
                    //    Update(UpdateList);

                    Tagging.PermRecLogProcess prlp = new Tagging.PermRecLogProcess();
                    prlp.SaveLog("教師.匯入類別", "匯入教師類別", "共新增" + InsertList.Count + "筆資料");
                    K12.Data.Teacher.RemoveAll();
                    K12.Data.Teacher.SelectAll();

                }
                catch (Exception ex) { }

            };
        }


        // 更新
        private void Update(object item)
        {
            try
            {
                List<TeacherTagRecord> UpdatePackage = (List<TeacherTagRecord>)item;
                TeacherTag.Update(UpdatePackage);
            }
            catch (Exception ex) { }
        }

        // 新增
        private void Insert(object item)
        {
            try
            {
                List<TeacherTagRecord> InsertPackage = (List<TeacherTagRecord>)item;
                TeacherTag.Insert(InsertPackage);
            }
            catch (Exception ex) { }
        }
    }
}
