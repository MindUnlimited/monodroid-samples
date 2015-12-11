using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;
using Microsoft.WindowsAzure.MobileServices;

namespace Todo
{
    public class Tree<T> : TreeNode<T>
    {
        //public Tree<T>() { }
        public Tree(T RootValue) : base(RootValue)
        {
            Value = RootValue;
        }
    }


    public class TreeNodeList<T> : List<TreeNode<T>>
    {
        public TreeNode<T> Parent;
        public TreeNodeList(TreeNode<T> Parent)
        {
            this.Parent = Parent;
        }
        public new TreeNode<T> Add(TreeNode<T> Node)
        {
            base.Add(Node);
            Node.Parent = Parent;
            return Node;
        }
        public TreeNode<T> Add(T Value)
        {
            return Add(new TreeNode<T>(Value));
        }
        public override string ToString()
        {
            return "Count =" +Count.ToString();
        }
    }

    public interface ITreeNodeAware<T>
    {
        TreeNode<T> Node { get; set; }
    }

    //public class Task : ITreeNodeAware<Task>
    //{
    //    public bool Complete = false;
    //    private TreeNode<Task> _Node;
    //    public TreeNode<Task> Node
    //    {
    //        get { return _Node; }
    //        set
    //        {
    //            _Node = value;
    //            // do something when the Node changes
    //            // if non-null, maybe we can do some setup
    //        }
    //    }
    //    // recursive
    //    public void MarkComplete()
    //    {
    //        // mark all children, and their children, etc., complete
    //        foreach (TreeNode<Task> ChildTreeNode in Node.Children)
    //        {
    //            ChildTreeNode.Value.MarkComplete();
    //        }
    //        // now that all decendents are complete, mark this task complete
    //        Complete = true;
    //    }
    //}

        public class TreeNode<T> : IDisposable

    {
        public TreeNode(T Value)
        {
            this.Value = Value;
            Parent = null;
            Children = new TreeNodeList<T>(this);
        }

        public TreeNode(T Value, TreeNode<T> Parent)
        {
            this.Value = Value;
            this.Parent = Parent;
            Children = new TreeNodeList<T>(this);
        }

        private bool _IsDisposed;
        public bool IsDisposed
        {
            get { return _IsDisposed; }
        }
        public void Dispose()
        {
            CheckDisposed();
            OnDisposing();
            // clean up contained objects (in Value property)
            if (Value is IDisposable)
            {
                //if (DisposeTraversal == TreeTraversalType.BottomUp)
                //{
                //    foreach (TreeNode node in Children)
                //    {
                //        node.Dispose();
                //    }
                //}
                //(Value as IDisposable).Dispose();
                //if (DisposeTraversal == TreeTraversalType.TopDown)
                //{
                    foreach (TreeNode<T> node in Children)
                    {
                        node.Dispose();
                    }
                //}
            }
            _IsDisposed = true;
        }
        public event EventHandler Disposing;
        protected void OnDisposing()
        {
            if (Disposing != null)
            {
                Disposing(this, EventArgs.Empty);
            }
        }
        public void CheckDisposed()
        {
            if (IsDisposed)
            {
                throw new ObjectDisposedException(GetType().Name);
            }
        }

        private TreeNode<T> _Parent;
        public TreeNode<T> Parent
        {
            get { return _Parent; }
            set
            {
                if (value == _Parent)
                {
                    return;
                }
                if (_Parent != null)
                {
                    _Parent.Children.Remove(this);
                }
                if (value != null && !value.Children.Contains(this))
                {
                    value.Children.Add(this);
                }
                _Parent = value;
            }
        }
        public TreeNode<T> Root
        {
            get
            {
                //return (Parent == null) ? this : Parent.Root;
                TreeNode<T> node = this;
                while (node.Parent != null)
                {
                    node = node.Parent;
                }
                return node;
            }
        }
        private TreeNodeList<T> _Children;
        public TreeNodeList<T> Children
        {
            get { return _Children; }
            private set { _Children = value; }
        }
        private T _Value;
        public T Value
        {
            get { return _Value; }
            set
            {
                _Value = value;
                if (_Value != null && _Value is ITreeNodeAware<T>)
                {
                    (_Value as ITreeNodeAware<T>).Node = this;
                }
            }
        }

        public int Depth
        {
            get
            {
                //return (Parent == null ? -1 : Parent.Depth) + 1;
                int depth = 0;
                TreeNode<T> node = this;
                while (node.Parent != null)
                {
                    node = node.Parent;
                    depth++;
                }
                return depth;
            }
        }

        public override string ToString()
        {
            string Description = string.Empty;
            if (Value != null)
            {
                Description = "[" + Value.ToString() + "] ";
            }
            return Description + "Depth=" + Depth.ToString() + ", Children= "
              + Children.Count.ToString();
        }


    }

}
