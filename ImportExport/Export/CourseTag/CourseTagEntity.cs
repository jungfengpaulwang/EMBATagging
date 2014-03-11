using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Tagging
{
    class CourseTagEntity
    {
        /// <summary>
        /// 學生系統編號
        /// </summary>
        public string ClassID { get; set; }

        /// <summary>
        /// Tag 前置與名稱
        /// </summary>
        private Dictionary<string, List<string>> _PrefixNameDic = new Dictionary<string, List<string>>();

        
        /// <summary>
        /// 新增
        /// </summary>
        /// <param name="Prefix"></param>
        /// <param name="Name"></param>
        public void AddPrefixName(string Prefix, string Name)
        {
            // 當群組是空白時
            if (string.IsNullOrEmpty(Prefix))
                Prefix = " ";

            if (_PrefixNameDic.ContainsKey(Prefix))
            {
                if (!_PrefixNameDic[Prefix].Contains(Name))
                    _PrefixNameDic[Prefix].Add(Name);
            }
            else
            {
                List<string> Names = new List<string>();
                Names.Add(Name);
                _PrefixNameDic.Add(Prefix, Names);            
            }
        }

        /// <summary>
        /// 取得類別群組資料
        /// </summary>
        /// <returns></returns>
        public Dictionary<string, List<string>> GetPrefixNameDic()
        {
            return _PrefixNameDic;
        }
    }
}
