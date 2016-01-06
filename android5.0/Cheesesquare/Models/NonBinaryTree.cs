using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;
using Microsoft.WindowsAzure.MobileServices;
using Android.Util;
using Cheesesquare;
using System.Threading.Tasks;

namespace Todo
{
    public class Tree<T> : TreeNode<T> where T : Item
    {
        //public Tree<T>() { }
        public Tree(T RootValue) : base(RootValue)
        {
            Value = RootValue;
        }
    }

    //public class ItemTree<T> : TreeNode<T> where T : Item
    //{
    //    //public Tree<T>() { }
    //    public ItemTree(T RootValue) : base(RootValue)
    //    {
    //        Value = RootValue;
    //    }


    //}

    //public class ItemTreeNode<T> : TreeNode<T> where T : Item
    //{
    //    public ItemTreeNode(T Value) : base(Value)
    //    {
    //        this.Value = Value;
    //        Parent = null;
    //        Children = new TreeNodeList<T>(this);
    //    }

    //    public ItemTreeNode(T Value, TreeNode<T> Parent): base(Value)
    //    {
    //    }


    //}


    public class TreeNodeList<T> : List<TreeNode<T>> where T : Item
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

            Parent.AmountOfChildrenChanged(1);

            if (Node.Value.Status == 7) // Node is already completed, count its children and that value plus one is the amount of children completed
            {
                Parent.AmountOfChildrenCompletedChanged(Node.AmountOfChildrenCompleted + 1);
            }

            return Node;
        }
        public TreeNode<T> Add(T Value)
        {
            return Add(new TreeNode<T>(Value));
        }

        public async void RemoveAllChildren()
        {
            for(int i = 0; i < Parent.AmountOfChildren; i++)
            {
                var child = Parent.Children[i];
                if (child.AmountOfChildren > 0)
                    child.Children.RemoveAllChildren();

                await PublicFields.Database.DeleteItem(child.Value);
                await Remove(child);
                base.Remove(child);                

                Parent.AmountOfChildrenChanged(-1);
            }
        }

        public async new Task<TreeNode<T>> Remove(TreeNode<T> Node)
        {
            if (this.Contains(Node))
            {
                foreach (var child in Node.Children)
                {
                    child.Children.RemoveAllChildren();
                }

                await PublicFields.Database.DeleteItem(Node.Value);
                base.Remove(Node);

                Parent.AmountOfChildrenChanged(-1);
                return Parent;
            }
            else
            {
                Log.Debug("Tree", string.Format("tried to remove node that was not there {0}", Node));
                return null;
            }
        }

        public override string ToString()
        {
            return "Count =" +Count.ToString();
        }
    }

    public interface ITreeNodeAware<T> where T : Item
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

    public class TreeNode<T> : IDisposable where T : Item //:IDisposable// where T is Item
    {
        public TreeNode(T Value)
        {
            this.Value = Value;
            Parent = null;
            Children = new TreeNodeList<T>(this);
            AmountOfChildren = Children.Count;
            AmountOfChildrenCompleted = 0;
        }

        public TreeNode(T Value, TreeNode<T> Parent)
        {
            this.Value = Value;
            this.Parent = Parent;
            Children = new TreeNodeList<T>(this);
            AmountOfChildren = Children.Count;
            AmountOfChildrenCompleted = 0;
        }

        private int _AmountOfChildren;
        public int AmountOfChildren
        {
            get { return _AmountOfChildren; }
            set
            {
                if (value == _AmountOfChildren)
                {
                    return;
                }
                if (value >= 0)
                {
                    _AmountOfChildren = value;
                }
            }
        }

        private int _AmountOfChildrenCompleted;
        public int AmountOfChildrenCompleted
        {
            get { return _AmountOfChildrenCompleted; }
            set
            {
                if (value == _AmountOfChildrenCompleted)
                {
                    return;
                }
                if (value >= 0)
                {
                    _AmountOfChildrenCompleted = value;
                }
            }
        }

        public IEnumerable<TreeNode<T>> Descendants()
        {
            var nodes = new Stack<TreeNode<T>>( (new[] { Root }));
            while (nodes.Count > 0)
            {
                TreeNode<T> node = nodes.Pop();
                yield return node;
                foreach (var n in node.Children) nodes.Push(n);
            }
        }

        public bool FindAndReplace(string ID, TreeNode<T> newNode)
        {
            var nodes = new Stack<TreeNode<T>>((new[] { Root }));
            while (nodes.Count > 0)
            {
                TreeNode<T> node = nodes.Pop();

                if (node.Value.id == ID)
                {
                    //var foundNodeIndex = node.Parent.Children.FindIndex(nod => nod.Value.id == ID);
                    node = newNode;
                    Log.Info("Tree", string.Format("found item with ID: {0}, replaced it with item {1}", ID, newNode.Value.Name));
                    return true; // node found and has been replaced
                }
                    
                //yield return node;
                foreach (var n in node.Children) nodes.Push(n);
            }
            return false; // node could not be found
        }



        public void AmountOfChildrenCompletedChanged(int Amount) // amount equals how many children haven been added or removed, can be -1 or 1
        {
            TreeNode<T> node = this;
            while (node.Parent != null)
            {
                node.AmountOfChildrenCompleted = node.AmountOfChildrenCompleted + Amount;
                node = node.Parent;
            }
            // for root node
            node.AmountOfChildrenCompleted = node.AmountOfChildrenCompleted + Amount;
        }

        public void AmountOfChildrenChanged(int Amount) // amount equals how many children haven been added or removed, can be -1 or 1
        {
            TreeNode<T> node = this;
            while (node.Parent != null)
            {
                node.AmountOfChildren = node.AmountOfChildren + Amount;
                node = node.Parent;
            }
            // for root node
            node.AmountOfChildren = node.AmountOfChildren + Amount;
        }

        public void Complete()
        {
            foreach (var child in Children)
            {
                child.Complete();
                AmountOfChildrenCompletedChanged(1);
            }

            Value.Status = 7; // complete own value
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
