using System;
using Android.Support.V4.App;
using Android.Views;
using Android.OS;
using Android.Support.V7.Widget;
using System.Collections.Generic;
using Android.Widget;
using Android.Content;
using Android.Util;
using Android.Runtime;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Linq;
using Android.Text.Format;
using Android.App;
using Java.Text;
using Java.Util;
using Android.Support.V4.View;

namespace MindSet
{
    public class ListFragmentDetail : ListFragment
    {
        public ListFragmentDetail(Todo.TreeNode<Todo.Item> dom, RecyclerView.AdapterDataObserver DataObserver) : base(dom, DataObserver)
        {
        }

        public ListFragmentDetail(Todo.TreeNode<Todo.Item> dom) : base(dom)
        {
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            var v = inflater.Inflate(
                Resource.Layout.fragment_cheese_list, container, false);
            var rv = (ScrollForwardingRecyclerView)v;//v.JavaCast<ScrollForwardingRecyclerView>();

            setupRecyclerView(rv);

            //rv.SetLayoutManager(new RecyclerViewLinearLayoutManager(Context));
            rv.NestedScrollingEnabled = false;
            rv.HasFixedSize = false;

            return rv;
        }
    }
}

