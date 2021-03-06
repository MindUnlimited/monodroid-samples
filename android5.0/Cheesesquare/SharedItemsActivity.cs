using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.Support.V7.Widget;

using V7Toolbar = Android.Support.V7.Widget.Toolbar;
using Todo;
using Android.Support.V4.View;
using Android.Support.V7.App;
using Android.Support.V4.Widget;
using Android.Support.Design.Widget;
using Android.Util;
using Android.Net;
using Android.Text.Format;

namespace MindSet
{
    [Activity(Label = "SharedItemsActivity")]
    public class SharedItemsActivity : AppCompatActivity, NavigationView.IOnNavigationItemSelectedListener
    {
        private RecyclerView recyclerView;
        private DrawerLayout drawerLayout;
        private NavigationView navigationView;
        private TextView userName;
        private SharedItemRecyclerViewAdapter sharedItemRecyclerViewAdapter;

        protected async override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // Create your application here
            SetContentView(Resource.Layout.activity_shared_items);

            var toolbar = FindViewById<V7Toolbar>(Resource.Id.toolbar);
            SetSupportActionBar(toolbar);

            SupportActionBar.SetHomeAsUpIndicator(Resource.Drawable.ic_menu);
            SupportActionBar.SetDisplayHomeAsUpEnabled(true);
            SupportActionBar.Title = "Shared with me";

            recyclerView = FindViewById<RecyclerView>(Resource.Id.recyclerview_share_items);
            drawerLayout = FindViewById<DrawerLayout>(Resource.Id.drawer_layout_shared);

            //update db
            PublicFields.UpdateDatabase();
            PublicFields.MakeTree();

            var itemLinks = await PublicFields.Database.GetItemLinks();
            itemLinks = itemLinks.Where(x => x.OwnedBy == PublicFields.Database.defGroup.id).ToList(); // filter so that we only see the ones we own not the ones we are part of the group

            List<Item> sharedItems = new List<Item>();
            List<TreeNode<Item>> sharedItemsTreeNode = new List<TreeNode<Item>>();

            foreach (var itl in itemLinks)
            {
                Item it = await PublicFields.Database.GetItem(itl.ItemID);
                if (it != null)
                {
                    it.SharedLink = itl;

                    if (itl.Parent == null) // to be assigned to a parent so shows up in list
                    {
                        it.Parent = itl.Parent;
                        sharedItems.Add(it);
                        sharedItemsTreeNode.Add(new TreeNode<Item>(it));
                    }
                }
            }

            Todo.TreeNode<Todo.Item> root = new TreeNode<Item>(null);
            root.Children.AddRange(sharedItemsTreeNode);

            navigationView = FindViewById<NavigationView>(Resource.Id.nav_view_shared);
            navigationView.SetNavigationItemSelectedListener(this);

            navigationView.SetCheckedItem(Resource.Id.shared_items);

            userName = navigationView.GetHeaderView(0).FindViewById<TextView>(Resource.Id.username_nav);
            userName.Text = PublicFields.Database.userName;

            setupRecyclerView(recyclerView, root);
        }

        protected void setupRecyclerView(RecyclerView recyclerView, TreeNode<Item> root)
        {
            recyclerView.SetLayoutManager(new LinearLayoutManager(recyclerView.Context));

            // provide 16dp of padding at the top of the list, 100 dp at the bottom
            DisplayMetrics metrics = new DisplayMetrics();
            IWindowManager windowManager = this.GetSystemService(Context.WindowService).JavaCast<IWindowManager>();
            windowManager.DefaultDisplay.GetMetrics(metrics);

            int sixteenDPTop = (int)Math.Ceiling(16 * metrics.Density);
            int hundredDPBottom = (int)Math.Ceiling(100 * metrics.Density);
            recyclerView.SetClipToPadding(false);
            recyclerView.SetPadding(0, sixteenDPTop, 0, hundredDPBottom);

            sharedItemRecyclerViewAdapter = new SharedItemRecyclerViewAdapter(this, root.Children);
            recyclerView.SetAdapter(sharedItemRecyclerViewAdapter);
        }

        public bool OnNavigationItemSelected(IMenuItem menuItem)
        {
            Intent intent = null;
            switch (menuItem.ItemId)
            {
                case Resource.Id.nav_home:
                    drawerLayout.CloseDrawers();
                    Finish();
                    return true;
                case Resource.Id.shared_items:
                    drawerLayout.CloseDrawers();
                    break;
                case Resource.Id.groups:
                    intent = new Intent(this, typeof(GroupsActivity));
                    intent.AddFlags(ActivityFlags.ClearTop);
                    drawerLayout.CloseDrawers();
                    StartActivity(intent);
                    Finish();
                    return true;
            }

            return false;
        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            switch (item.ItemId)
            {
                case Android.Resource.Id.Home:
                    drawerLayout.OpenDrawer(Android.Support.V4.View.GravityCompat.Start);
                    return true;
            }
            return base.OnOptionsItemSelected(item);
        }


    }

    public class SharedItemRecyclerViewAdapter : RecyclerView.Adapter
    {
        private Todo.TreeNodeList<Todo.Item> items;
        protected Android.App.Activity parent;

        private const int SHARE_CONTACT = 101;
        public const int ITEMDETAIL = 103;
        public const int PICKIMAGE = 105;

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
            public ImageView ImageView { get; set; }

            public TextView Status { get; set; }
            public TextView Importance { get; set; }
            public TextView DueDate { get; set; }

            public TextView AmountOfSubTasks { get; set; }
            public LinearLayout SubTasksLinearLayout { get; set; }
            public TextView MoreThanFiveSubtasks { get; set; }
            public TextView NoSubTasks { get; set; }

            public ImageButton Delete { get; set; }


            public ViewHolder(View view, Action<int> itemClickListener, Action<int> deleteClickListener) : base(view)
            {
                View = view;
                TextView = view.FindViewById<TextView>(Resource.Id.task_title);
                ImageView = view.FindViewById<ImageView>(Resource.Id.imageView);
                Status = view.FindViewById<TextView>(Resource.Id.status_task);
                Importance = view.FindViewById<TextView>(Resource.Id.importance_task);
                DueDate = view.FindViewById<TextView>(Resource.Id.due_date_task);
                AmountOfSubTasks = view.FindViewById<TextView>(Resource.Id.amountOfSubTasks);
                SubTasksLinearLayout = view.FindViewById<LinearLayout>(Resource.Id.subtasks_llayout);
                MoreThanFiveSubtasks = view.FindViewById<TextView>(Resource.Id.more_than_five_subtasks_text);
                NoSubTasks = view.FindViewById<TextView>(Resource.Id.no_subtasks_text);
                Delete = view.FindViewById<ImageButton>(Resource.Id.deleteButton);

                view.Click += (sender, e) => itemClickListener(base.LayoutPosition);
                Delete.Click += (sender, e) => deleteClickListener(base.LayoutPosition);
            }

            public override string ToString()
            {
                return base.ToString() + " '" + TextView.Text;
            }
        }

        public void UpdateValue(Todo.TreeNode<Todo.Item> item)
        {
            int index = items.FindIndex(it => it.Value.id == item.Value.id);
            if (index >= 0)
                items[index] = item;
            NotifyItemChanged(index);
        }

        public Todo.TreeNode<Todo.Item> GetValueAt(int position)
        {
            return items[position];
        }

        public void ChangeValueAt(int position, Todo.Item item)
        {
            items[position].Value = item;
        }

        public void ApplyChanges()
        {
            //fragment.childItems = items;
            NotifyDataSetChanged();
        }

        public void AddItem(Todo.TreeNode<Todo.Item> item)
        {
            items.Add(item);
            NotifyItemInserted(items.Count);
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


        public SharedItemRecyclerViewAdapter(Android.App.Activity context, Todo.TreeNodeList<Todo.Item> items)
        {
            parent = context;

            this.items = items;

            this.ItemClick += OnItemClick;
            //this.SubTaskClick += OnSubTaskClick;
            this.DeleteClick += OnDeleteClick;
        }

        public void ChangeDateSet(Todo.TreeNodeList<Todo.Item> items)
        {
            this.items = items;
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
            var adapter = sender as SharedItemRecyclerViewAdapter;
            var item = adapter.GetValueAt(e);


            new Android.Support.V7.App.AlertDialog.Builder(parent)
            .SetMessage("Delete this item?")
            .SetCancelable(false)
            .SetPositiveButton("Yes", delegate
            {
                    //var dataSetID = item.Parent.Value.id;
                    //var parent = await adapter.items.Remove(item);

                    ////RecursiveDelete(item);

                    ////var itemInTree = PublicFields.ItemTree.Descendants().First(it => it.Value.id == item.Value.id);
                    ////await itemInTree.Parent.Children.Remove(itemInTree);

                    //var dataset = PublicFields.ItemTree.Descendants().First(it => it.Value.id == dataSetID).Children;
                    //ChangeDateSet(dataset);

                    ////NotifyItemRemoved(e);
                    //NotifyDataSetChanged();

                    Log.Debug("CheeseListFragment", string.Format("removed item {0} and its subitems", item.Value.Name));
            })
           .SetNegativeButton("No", delegate
           {
               Log.Debug("CheeseListFragment", string.Format("Did not remove item {0}", item.Value.Name));
           })
           .Show();
        }

        protected void OnItemClick(object sender, int e)
        {
            var adapter = sender as SharedItemRecyclerViewAdapter;
            var item = adapter.GetValueAt(e);

            // Build the dialog.
            var builder = new Android.App.AlertDialog.Builder(parent);
            builder.SetTitle("Place under");


            IEnumerable<Todo.Item> domains = from x in PublicFields.domains select x.Value;
            string[] itemNames = (from x in domains select x.Name).ToArray();
            builder.SetItems(itemNames, new EventHandler<DialogClickEventArgs>(
            (s, args) => {
                parent.RunOnUiThread(async () =>
                {
                    Item selectedDomain = domains.ElementAtOrDefault(args.Which);

                        // store domain as parent of this item in db
                        item.Value.SharedLink.Parent = selectedDomain.id;
                    await PublicFields.Database.SaveItemLink(item.Value.SharedLink);

                        // change the current item to correspond with its new parent
                        item.Value.Parent = selectedDomain.id;
                    var sharedItemNode = new TreeNode<Item>(item.Value);

                        // change locally
                        var parentOfItem = PublicFields.ItemTree.Descendants().FirstOrDefault(x => x.Value.id == selectedDomain.id);
                    parentOfItem.Children.Add(sharedItemNode);
                    PublicFields.ItemTree.FindAndReplace(parentOfItem.Value.id, parentOfItem);

                    await items.Remove(item);
                    NotifyItemRemoved(e);

                        //ChangeDateSet(items);
                        //NotifyDataSetChanged();
                    });
                    //clicked(true);
                }));

            var dialog = builder.Create();

            // Show the dialog. This is important to do before accessing the buttons.
            dialog.Show();
        }

        public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
        {

            var view = LayoutInflater.From(parent.Context)
                .Inflate(Resource.Layout.item_card_view, parent, false);

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

        public override void OnBindViewHolder(RecyclerView.ViewHolder holder, int position)
        {
            var h = holder as ViewHolder;

            var item = items[position];


            switch (item.Value.Status)
            {
                case -1:
                    h.Status.Text = "Cancelled";
                    break;
                case 0:
                    h.Status.Text = "Backlog";
                    break;
                case 6:
                    h.Status.Text = "On Hold";
                    break;
                case 7:
                    h.Status.Text = "Completed";
                    break;
                default:
                    h.Status.Text = "Started";
                    break;

            }

            if (item.Value.EndDate != null)
            {
                //String givenDateString = "Tue Apr 23 16:08:28 GMT+05:30 2013";
                //SimpleDateFormat sdf = new SimpleDateFormat("dd-MM-yyyy");//new SimpleDateFormat("EEE MMM dd HH:mm:ss z yyyy");
                try
                {
                    long timeInMilliseconds = (long)(TimeZoneInfo.ConvertTimeToUtc(item.Value.EndDate) -
                    new DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc)).TotalMilliseconds;

                    if (timeInMilliseconds >= 0)
                        h.DueDate.Text = DateUtils.GetRelativeTimeSpanString(Application.Context, timeInMilliseconds);
                }
                catch (ParseException e)
                {
                    e.PrintStackTrace();
                }
            }
            else
            {
                h.DueDate.Text = "No due date";
            }

            int index = 1;
            if (item.Children == null || item.Children.Count == 0)
            {
                h.NoSubTasks.Visibility = ViewStates.Visible;
                h.AmountOfSubTasks.Text = string.Format("{0} subtasks", 0);
                h.MoreThanFiveSubtasks.Visibility = ViewStates.Gone;

                while (h.SubTasksLinearLayout.ChildCount > 2)
                    h.SubTasksLinearLayout.RemoveViewAt(1);
            }
            else
            {
                h.NoSubTasks.Visibility = ViewStates.Gone;
                h.MoreThanFiveSubtasks.Visibility = ViewStates.Gone;

                // more items than the item signaling that there are no tasks and the item signaling
                // that there are more than 5 tasks
                while (h.SubTasksLinearLayout.ChildCount > 2)
                    h.SubTasksLinearLayout.RemoveViewAt(1);
                foreach (var subitem in item.Children)
                {
                    LinearLayout subtaskView = (LinearLayout)LayoutInflater.From(h.SubTasksLinearLayout.Context).Inflate(Resource.Layout.subtask_line, h.SubTasksLinearLayout, false);
                    var subtaskName = subtaskView.FindViewById<TextView>(Resource.Id.subtask_name);
                    subtaskName.Text = subitem.Value.Name;

                    subtaskName.Click += (sender, e) =>
                    {
                        Log.Debug("ListFragment", subtaskName.Text + " was clicked");
                        var context = h.View.Context;
                        var intent = new Intent(context, typeof(DetailActivity));
                        intent.PutExtra(DetailActivity.EXTRA_NAME, subtaskName.Text);
                        intent.PutExtra(DetailActivity.ITEM_ID, subitem.Value.id);

                        parent.StartActivityForResult(intent, ITEMDETAIL);
                        //context.StartActivityForResult(intent);
                        //context.StartActivity(intent);
                    };

                    var subtaskCheckBox = subtaskView.FindViewById<CheckBox>(Resource.Id.checkbox);
                    subtaskCheckBox.CheckedChange += SubtaskCheckBox_CheckedChange;

                    h.SubTasksLinearLayout.AddView(subtaskView, index);

                    index++;

                    if (index > 5) // 5 items max
                    {
                        break;
                    }

                }

                if (item.Children.Count > 5) // 5 items max
                {
                    h.MoreThanFiveSubtasks.Visibility = ViewStates.Visible;
                }
                else
                {
                    h.MoreThanFiveSubtasks.Visibility = ViewStates.Gone;
                }

                h.AmountOfSubTasks.Text = string.Format("{0} subtasks", item.Children.Count.ToString());
            }

            h.TextView.Text = item.Value.Name;
            h.Importance.Text = string.Format("{0} stars", item.Value.Importance) ?? "0 stars";
        }

        public override int ItemCount
        {
            get { return items.Count; }
        }
    }
}