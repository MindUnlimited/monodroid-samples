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
using Todo;

namespace Cheesesquare
{
    public class ListFragmentGroups : Android.Support.V4.App.Fragment
    {
        List<Todo.Group> fragmentGroups;
        public GroupsRecyclerViewAdapter sharedItemRecyclerViewAdapter;
        RecyclerView.AdapterDataObserver dataObserver;

        public ListFragmentGroups(List<Group> groups, RecyclerView.AdapterDataObserver DataObserver)
        {
            dataObserver = DataObserver;
            fragmentGroups = groups;
        }

        public ListFragmentGroups(List<Group> groups)
        {
            fragmentGroups = groups;
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            var v = inflater.Inflate(
                Resource.Layout.fragment_cheese_list, container, false);
            var rv = (ScrollForwardingRecyclerView)v;//v.JavaCast<ScrollForwardingRecyclerView>();

            setupRecyclerView(rv);
            return rv;
        }

        protected new void setupRecyclerView(RecyclerView recyclerView)
        {
            recyclerView.SetLayoutManager(new LinearLayoutManager(recyclerView.Context));

            sharedItemRecyclerViewAdapter = new GroupsRecyclerViewAdapter(Activity, fragmentGroups, this);
            recyclerView.SetAdapter(sharedItemRecyclerViewAdapter);
            if (dataObserver != null)
                sharedItemRecyclerViewAdapter.RegisterAdapterDataObserver(dataObserver);
        }

        public class GroupsRecyclerViewAdapter : RecyclerView.Adapter
        {
            private List<Todo.Group> groups;
            protected Android.App.Activity parent;
            private ListFragmentGroups fragment;

            //Create an Event so that our our clients can act when a user clicks
            //on each individual item.
            public event EventHandler<int> ItemClick;

            ////Create an Event so that our our clients can act when a user clicks
            ////on each individual item.
            //public event EventHandler<int> SubTaskClick;

            //Create an Event so that our our clients can act when a user clicks
            //on each individual item.
            public event EventHandler<int> DeleteClick;

            public class ViewHolder : RecyclerView.ViewHolder
            {
                public View View { get; set; }

                public TextView TextView { get; set; }

                public TextView AmountOfItems { get; set; }
                public LinearLayout groupMembersLinearLayout { get; set; }

                public ImageButton Delete { get; set; }


                public ViewHolder(View view, Action<int> itemClickListener, Action<int> deleteClickListener) : base(view)
                {
                    View = view;
                    TextView = view.FindViewById<TextView>(Resource.Id.group_title);
                    AmountOfItems = view.FindViewById<TextView>(Resource.Id.amountOfItems);
                    groupMembersLinearLayout = view.FindViewById<LinearLayout>(Resource.Id.groups_llayout);

                    Delete = view.FindViewById<ImageButton>(Resource.Id.leaveButton);

                    view.Click += (sender, e) => itemClickListener(base.LayoutPosition);
                    Delete.Click += (sender, e) => deleteClickListener(base.LayoutPosition);
                }

                public override string ToString()
                {
                    return base.ToString() + " '" + TextView.Text;
                }
            }

            //public void UpdateValue(Todo.TreeNode<Todo.Item> item)
            //{
            //    int index = groups.FindIndex(it => it.Value.id == item.Value.id);
            //    if (index >= 0)
            //        groups[index] = item;
            //    NotifyItemChanged(index);
            //}

            public Todo.Group GetValueAt(int position)
            {
                return groups[position];
            }

            public void ChangeValueAt(int position, Todo.Group group)
            {
                groups[position] = group;
            }

            public void ApplyChanges()
            {
                //fragment.childItems = items;
                NotifyDataSetChanged();
            }

            public void AddItem(Todo.Group group)
            {
                groups.Add(group);
                NotifyItemInserted(groups.Count);
            }

            //This will fire any event handlers that are registered with our ItemClick
            //event.
            private void OnClick(int position)
            {
                if (ItemClick != null)
                {
                    ItemClick(this, position);
                }
            }

            ////This will fire any event handlers that are registered with our DeleteItemClick
            ////event.
            //private void OnSubTaskClick(int position)
            //{
            //    if (SubTaskClick != null)
            //    {
            //        SubTaskClick(this, position);
            //    }
            //}

            //This will fire any event handlers that are registered with our DeleteItemClick
            //event.
            private void OnDeleteClick(int position)
            {
                if (DeleteClick != null)
                {
                    DeleteClick(this, position);
                }
            }


            public GroupsRecyclerViewAdapter(Android.App.Activity context, List<Todo.Group> groups, ListFragmentGroups fragm)
            {
                parent = context;
                fragment = fragm;

                this.groups = groups ?? new List<Group>();

                this.ItemClick += OnItemClick;
                //this.SubTaskClick += OnSubTaskClick;
                this.DeleteClick += OnDeleteClick;
            }

            public void ChangeDateSet(List<Todo.Group> groups)
            {
                this.groups = groups ?? new List<Group>();
                NotifyDataSetChanged();
            }

            //private void OnSubTaskClick(object sender, int e)
            //{
            //    var adapter = sender as ItemRecyclerViewAdapter;
            //    var item = adapter.GetValueAt(e);

            //    var context = fragment.Context;//h.View.Context;
            //    var intent = new Intent(context, typeof(CheeseDetailActivity));
            //    //intent.PutExtra(CheeseDetailActivity.EXTRA_NAME, item.na);
            //    //intent.PutExtra(CheeseDetailActivity.ITEM_ID, subitem.id);

            //    //parent.StartActivityForResult(intent, ITEMDETAIL);
            //}

            private void OnDeleteClick(object sender, int e)
            {
                var adapter = sender as GroupsRecyclerViewAdapter;
                var group = adapter.GetValueAt(e);
                var context = fragment.Context;
                var activity = fragment.Activity;

                new Android.Support.V7.App.AlertDialog.Builder(parent)
                .SetMessage(string.Format("Unsubscribe from group {0}", group.Name))
                .SetCancelable(false)
                .SetPositiveButton("Yes", delegate
                {
                    Log.Debug("ListFragmentGroup", string.Format("unsubscribed from group {0}", group.Name));
                })
                .SetNegativeButton("No", delegate
                {
                    Log.Debug("ListFragmentGroup", string.Format("Stayed in group {0}", group.Name));
                })
                .Create()
                .Show();
            }

            protected void OnItemClick(object sender, int e)
            {
                var adapter = sender as GroupsRecyclerViewAdapter;
                var group = adapter.GetValueAt(e);
                var context = fragment.Context;
                var activity = fragment.Activity;

                //new Android.Support.V7.App.AlertDialog.Builder(parent)
                //.SetMessage(string.Format("Unsubscribe from group {0}", group.Name))
                //.SetCancelable(false)
                //.SetPositiveButton("Yes", delegate
                //{
                //    Log.Debug("ListFragmentGroup", string.Format("unsubscribed from group {0}", group.Name));
                //})
                //.SetNegativeButton("No", delegate
                //{
                //   Log.Debug("ListFragmentGroup", string.Format("Stayed in group {0}", group.Name));
                //})
                //.Create()
                //.Show();
            }

            public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
            {

                var view = LayoutInflater.From(parent.Context)
                    .Inflate(Resource.Layout.group_card_view, parent, false);

                var vh = new ViewHolder(view, OnClick, OnDeleteClick);
                return vh;
            }

            private void SubtaskCheckBox_CheckedChange(object sender, CompoundButton.CheckedChangeEventArgs e)
            {
                Log.Debug("CheeseListFragment", "checkbox clicked");
            }

            public async void RecursiveDelete(Todo.TreeNode<Todo.Item> item)
            {
                if (item == null)
                    return;

                // remove from tree and db
                var parent = await item.Parent.Children.Remove(item); // return the parent so we can update the value in the tree
                //PublicFields.ItemTree.FindAndReplace(parent.Value.id, parent);

                //// remove from db
                //await PublicFields.Database.DeleteItem(item.Value);
            }

            public async override void OnBindViewHolder(RecyclerView.ViewHolder holder, int position)
            {
                var h = holder as ViewHolder;

                Group boundGroup = groups[position];
                var groupMembers = await PublicFields.Database.MembersOfGroup(boundGroup);

                var items = await PublicFields.Database.GetItems();
                var itemsFromGroup = from x in items where x.OwnedBy == boundGroup.id select x;

                var sharedMembers = await PublicFields.Database.MembersOfGroup(boundGroup);
                if (boundGroup.Name.Contains("+") && sharedMembers.Count == 2)
                {
                    if (PublicFields.Database.defGroup.id == sharedMembers[0].id)
                    {
                        h.TextView.Text = sharedMembers[1].Name;
                    }
                    else
                    {
                        h.TextView.Text = sharedMembers[0].Name;
                    }
                }
                else
                {
                    h.TextView.Text = boundGroup.Name ?? null;
                }

                h.AmountOfItems.Text = itemsFromGroup.Count().ToString() + " items";

                if (h.groupMembersLinearLayout.ChildCount > 0)
                    h.groupMembersLinearLayout.RemoveAllViews();

                int index = 0;
                foreach (var grp in groupMembers)
                {
                    LinearLayout groupView = (LinearLayout)LayoutInflater.From(h.groupMembersLinearLayout.Context).Inflate(Resource.Layout.group_line, h.groupMembersLinearLayout, false);
                    var grpName = groupView.FindViewById<TextView>(Resource.Id.group_name);
                    grpName.Text = grp.Name;

                    grpName.Click += (sender, e) =>
                    {
                        Log.Debug("ListFragment", grpName.Text + " was clicked");
                        var context = h.View.Context;
                    };

                    h.groupMembersLinearLayout.AddView(groupView, index);

                    index++;
                }
            }

            public override int ItemCount
            {
                get { return groups.Count; }
            }
        }
    }
}

