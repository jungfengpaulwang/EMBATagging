using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SmartSchool.API.PlugIn;
using K12.Data;

namespace Tagging
{
    class ImportCourseTag : SmartSchool.API.PlugIn.Import.Importer
    {
        // 可匯入項目
        List<string> ImportItemList = new List<string>();

        public ImportCourseTag()
        {
            this.Image = null;
            this.Text = "匯入課程類別";
            ImportItemList.Add("群組");
            ImportItemList.Add("類別名稱");
        }

        public override void InitializeImport(SmartSchool.API.PlugIn.Import.ImportWizard wizard)
        {
            Dictionary<string, CourseRecord> Class_s = new Dictionary<string, CourseRecord>();
            Dictionary<string, List<CourseTagRecord>> ClassTagRecDic = new Dictionary<string, List<CourseTagRecord>>();

            Dictionary<string, Dictionary<string, string>> ClaTagNameDic = DACourseTransfer.GetCourseTagNameDic();

            // 取得可加入班級 TagName            
            wizard.PackageLimit = 3000;
            wizard.ImportableFields.AddRange(ImportItemList);
            wizard.ValidateStart += delegate(object sender, SmartSchool.API.PlugIn.Import.ValidateStartEventArgs e)
            {
                // 取得班級資料
                foreach (CourseRecord ClassRec in Course.SelectByIDs(e.List))
                    if (!Class_s.ContainsKey(ClassRec.ID))
                    {
                        Class_s.Add(ClassRec.ID, ClassRec);
                        ClassTagRecDic.Add(ClassRec.ID, new List<CourseTagRecord>());
                    }

                // 取得班級類別
                foreach (CourseTagRecord claTag in CourseTag.SelectByCourseIDs(Class_s.Keys))
                {
                    //if (!StudTagRecDic.ContainsKey(studTag.RefStudentID))
                    //{
                    //    List<JHStudentTagRecord> rec = new List<JHStudentTagRecord> ();
                    //    rec.Add(studTag );
                    //    StudTagRecDic.Add(studTag.RefStudentID,rec);                       
                    //}
                    //else
                    if (ClassTagRecDic.ContainsKey(claTag.RefCourseID))
                        ClassTagRecDic[claTag.RefCourseID].Add(claTag);
                }
            };

            wizard.ValidateRow += delegate(object sender, SmartSchool.API.PlugIn.Import.ValidateRowEventArgs e)
            {
                int i = 0;

                // 檢查課程是否存在
                CourseRecord teachRec = null;
                if (Class_s.ContainsKey(e.Data.ID))
                    teachRec = Class_s[e.Data.ID];
                else
                {
                    e.ErrorMessage = "沒有這個課程" + e.Data.ID;
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

                List<CourseTagRecord> InsertList = new List<CourseTagRecord>();
                //                List<JHStudentTagRecord> UpdateList = new List<JHStudentTagRecord>();

                // 放需要新增的教師類別
                Dictionary<string, List<string>> NeedAddPrefixName = new Dictionary<string, List<string>>();

                // 檢查用 List
                List<string> CheckClaTagName = new List<string>();

                foreach (KeyValuePair<string, Dictionary<string, string>> data in ClaTagNameDic)
                {
                    foreach (KeyValuePair<string, string> data1 in data.Value)
                        CheckClaTagName.Add(data.Key + data1.Key);
                }

                // 檢查類別是否已經存在
                foreach (string id in id_Rows.Keys)
                {
                    if (!ClassTagRecDic.ContainsKey(id))
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
                        if (!CheckClaTagName.Contains(FullName))
                        {
                            CheckClaTagName.Add(FullName);
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
                        rec.Category = "Course";
                        rec.Prefix = data.Key;
                        rec.Name = data1;
                        rec.Color = System.Drawing.Color.White;
                        Recs.Add(rec);
                    }
                }
                if (Recs.Count != 0)
                {
                    TagConfig.Insert(Recs);
                }

                ClaTagNameDic.Clear();

                // 重新取得
                ClaTagNameDic = DACourseTransfer.GetCourseTagNameDic();

                foreach (string id in id_Rows.Keys)
                {
                    if (!ClassTagRecDic.ContainsKey(id))
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

                        foreach (CourseTagRecord rec in ClassTagRecDic[id])
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
                            if (ClaTagNameDic.ContainsKey(strPrefix))
                            {
                                if (ClaTagNameDic[strPrefix].ContainsKey(strName))
                                {
                                    CourseTagRecord ClaTag = new CourseTagRecord();
                                    ClaTag.RefEntityID = id;
                                    ClaTag.RefTagID = ClaTagNameDic[strPrefix][strName];
                                    InsertList.Add(ClaTag);
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
                    prlp.SaveLog("課程.匯入類別", "匯入課程類別", "共新增" + InsertList.Count + "筆資料");
                    K12.Data.Course.RemoveAll();
                    K12.Data.Course.SelectAll();

                }
                catch (Exception ex) { }

            };
        }


        // 更新
        private void Update(object item)
        {
            try
            {
                List<CourseTagRecord> UpdatePackage = (List<CourseTagRecord>)item;
                CourseTag.Update(UpdatePackage);
            }
            catch (Exception ex) { }
        }

        // 新增
        private void Insert(object item)
        {
            try
            {
                List<CourseTagRecord> InsertPackage = (List<CourseTagRecord>)item;
                CourseTag.Insert(InsertPackage);
            }
            catch (Exception ex) 
            {
                SmartSchool.ErrorReporting.ReportingService.ReportException(ex);
            }
        }
    }
}
