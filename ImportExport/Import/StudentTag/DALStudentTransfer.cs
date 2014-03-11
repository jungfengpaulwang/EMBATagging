using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using K12.Data;

namespace Tagging
{
    class DALStudentTransfer
    {
        /// <summary>
        /// 取得所選學生類別名稱集合(前置)
        /// </summary>
        /// <param name="StudentIDList"></param>
        /// <returns></returns>
        public static List<string> GetStudTagPrefixList(List<string> StudentIDList)        
        {
            List<string> PrefixList = new List<string>();

            List<StudentTagRecord> studTags = StudentTag.SelectByStudentIDs(StudentIDList);
            foreach (StudentTagRecord tr in studTags)
            {
                string tPrefix = "";
                if (tr.Prefix == "")
                    tPrefix = tr.Name;
                else
                    tPrefix = tr.Prefix;

                if (!PrefixList.Contains(tPrefix))
                    PrefixList.Add(tPrefix);
            }
            return PrefixList;
        }

        /// <summary>
        /// 取得學生 Tag
        /// </summary>
        /// <returns></returns>
        public static Dictionary<string, Dictionary<string,string>> GetStudentTagNameDic()
        {
            Dictionary<string, Dictionary<string, string>> retVal = new Dictionary<string, Dictionary<string, string>>();

            foreach (TagConfigRecord tr in TagConfig.SelectByCategory(K12.Data.TagCategory.Student))
            {
                if (retVal.ContainsKey(tr.Prefix))
                {
                    if (retVal[tr.Prefix].ContainsKey(tr.Name))
                        retVal[tr.Prefix][tr.Name] = tr.ID;
                    else
                        retVal[tr.Prefix].Add(tr.Name, tr.ID);
                }
                else
                {
                    Dictionary<string, string> str = new Dictionary<string, string>();
                    str.Add(tr.Name, tr.ID);
                    retVal.Add(tr.Prefix, str);
                }
            }
            return retVal;
        }

    }
}
