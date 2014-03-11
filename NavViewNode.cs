using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DevComponents.AdvTree;

namespace Tagging
{
    internal class NavViewNode
    {
        public string Name { get; set; }

        private DevComponents.AdvTree.Node mViewNode = new DevComponents.AdvTree.Node();

        //Children。
        private Dictionary<string, NavViewNode> NodeChildren = new Dictionary<string, NavViewNode>();
        private List<string> mKeyChildren;
        private bool mIsRoot = false;

        public NavViewNode()
        {
            mKeyChildren = new List<string>();
        }

        public bool IsRoot
        {
            get { return mIsRoot; }
            set { mIsRoot = value; }
        }

        public Dictionary<string, NavViewNode> Nodes
        {
            get
            {
                return NodeChildren;
            }
        }

        /// <summary>
        /// 同一組(同一個 NavView 下面的所有 Node) Node 之間共用的對照表。
        /// </summary>
        public Dictionary<Node, List<string>> SharedKeysMap { get; set; }

        public void UpdateView(bool isNoRoot)
        {
            //有子節點的情況
            if (NodeChildren.Count > 0)
            {
                KeyChildren.Clear();
                ViewNode.Nodes.Clear();

                foreach (NavViewNode childNVNode in NodeChildren.Values)
                {
                    childNVNode.UpdateView(false);

                    if (!isNoRoot)
                    {
                        //新增子節點的PrimaryKey
                        foreach (string Key in childNVNode.KeyChildren)
                        {
                            if (!KeyChildren.Contains(Key))
                                KeyChildren.Add(Key);
                        }

                        //新增子節點的Node
                        ViewNode.Nodes.Add(childNVNode.ViewNode);
                    }
                }

                if (!isNoRoot)
                {
                    //設定節點的名稱
                    ViewNode.Text = Name + "(" + KeyChildren.Count + ")";

                    //將PrimaryKes加入到變數內
                    SharedKeysMap.Add(mViewNode, KeyChildren);
                }
            }
            else //無子節點的情況
            {
                ViewNode.Text = Name + "(" + KeyChildren.Count + ")";
                SharedKeysMap.Add(ViewNode, KeyChildren);
            }
        }

        public List<string> KeyChildren
        {
            get { return mKeyChildren; }
            set { mKeyChildren = value; }
        }

        private NavViewNode SmartAdd(string key)
        {
            if (!NodeChildren.ContainsKey(key))
            {
                NavViewNode Node = new NavViewNode() { SharedKeysMap = this.SharedKeysMap };
                Node.Name = key;
                NodeChildren.Add(key, Node);
                return Node;
            }
            else
                return NodeChildren[key];
        }

        public NavViewNode this[string key]
        {
            get { return SmartAdd(key); }
        }

        /// <summary>
        /// 名稱清單。
        /// </summary>
        public IEnumerable<string> SortedNames
        {
            get
            {
                List<string> names = new List<string>(NodeChildren.Keys);
                names.Sort((x, y) =>
                {
                    return x.CompareTo(y);
                });

                return names;
            }
        }

        /// <summary>
        /// 實際的 Node。
        /// </summary>
        public Node ViewNode
        {
            get { return mViewNode; }
        }
    }
}