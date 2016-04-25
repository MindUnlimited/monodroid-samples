using System;
using Android.OS;
using Android.Support.V7.App;
using Android.Support.V7.Widget;
using Android.Views;
using Android.Widget;
using V7Toolbar = Android.Support.V7.Widget.Toolbar;
using V4Fragment = Android.Support.V4.App.Fragment;
using V4FragmentManager = Android.Support.V4.App.FragmentManager;
using Android.Support.Design.Widget;
using Android.App;
using Android.Util;
using Android.Content;
using Android.Support.V4.Widget;
using Android.Support.V4.App;
using Newtonsoft.Json;
using Android.Support.V4.View;
using System.Collections.Generic;
using System.Linq;
using Android.Text.Format;
using Android.Net;
using Java.Lang.Reflect;
using Todo;
using MindSet.Models;
using System.Threading.Tasks;
using Android.Content.PM;
using Android.Graphics;
using Android.Runtime;

namespace MindSet
{
    public class DataObserver : RecyclerView.AdapterDataObserver
    {
        private DetailActivity detailActivity;

        public DataObserver(DetailActivity activity) : base()
        {
            detailActivity = activity;
        }

        public override void OnChanged()
        {
            base.OnChanged();
            detailActivity.itemChanged = true;
            //CheckAdapterIsEmpty();
        }
    }

    public class ItemRecyclerViewAdapter : RecyclerView.Adapter
    {
        private const int SHARE_CONTACT = 101;
        public const int ITEMDETAIL = 103;
        public const int PICKIMAGE = 105;

        protected Todo.TreeNodeList<Todo.Item> items;
        protected Android.App.Activity parent;

        //Create an Event so that our our clients can act when a user clicks
        //on each individual item.
        public event EventHandler<int> ItemClick;

        ////Create an Event so that our our clients can act when a user clicks
        ////on each individual item.
        //public event EventHandler<int> SubTaskClick;

        //Create an Event so that our our clients can act when a user clicks
        //on each individual item.
        public event EventHandler<int> DeleteClick;

        //Create an Event so that our our clients can act when a user clicks
        //on each individual item.
        public event EventHandler<int> CompleteClick;

        //Create an Event so that our our clients can act when a user clicks
        //on each individual item.
        public event EventHandler<int> ShareClick;

        //Create an Event so that our our clients can act when a user clicks
        //on each individual item.
        public event EventHandler<int> ImageClick;

        public class ViewHolder : RecyclerView.ViewHolder
        {
            public View View { get; set; }

            public TextView TextView { get; set; }
            public ImageView ImageView { get; set; }

            public TextView Status { get; set; }
            public TextView Progress { get; set; }
            public TextView Importance { get; set; }
            public TextView DueDate { get; set; }

            public TextView AmountOfSubTasks { get; set; }
            public LinearLayout SubTasksLinearLayout { get; set; }
            public TextView MoreThanFiveSubtasks { get; set; }
            public TextView NoSubTasks { get; set; }

            public ImageButton Delete { get; set; }
            public ImageButton Complete { get; set; }
            public ImageButton Share { get; set; }


            public ViewHolder(View view, Action<int> itemClickListener, Action<int> deleteClickListener, Action<int> completeClickListener, Action<int> shareClickListener, Action<int> imageClickListener) : base(view)
            {
                View = view;
                TextView = view.FindViewById<TextView>(Resource.Id.task_title);
                ImageView = view.FindViewById<ImageView>(Resource.Id.imageView);
                Status = view.FindViewById<TextView>(Resource.Id.status_task);
                Progress = view.FindViewById<TextView>(Resource.Id.progress_task);
                Importance = view.FindViewById<TextView>(Resource.Id.importance_task);
                DueDate = view.FindViewById<TextView>(Resource.Id.due_date_task);
                AmountOfSubTasks = view.FindViewById<TextView>(Resource.Id.amountOfSubTasks);
                SubTasksLinearLayout = view.FindViewById<LinearLayout>(Resource.Id.subtasks_llayout);
                MoreThanFiveSubtasks = view.FindViewById<TextView>(Resource.Id.more_than_five_subtasks_text);
                NoSubTasks = view.FindViewById<TextView>(Resource.Id.no_subtasks_text);
                Delete = view.FindViewById<ImageButton>(Resource.Id.deleteButton);
                Complete = view.FindViewById<ImageButton>(Resource.Id.finishButton);
                Share = view.FindViewById<ImageButton>(Resource.Id.shareButton);

                view.Click += (sender, e) => itemClickListener(base.LayoutPosition);

                ImageView.Click += (sender, e) => imageClickListener(base.LayoutPosition);

                Delete.Click += (sender, e) => deleteClickListener(base.LayoutPosition);
                Complete.Click += (sender, e) => completeClickListener(base.LayoutPosition);
                Share.Click += (sender, e) => shareClickListener(base.LayoutPosition);

                //for(int i = 1; i < SubTasksLinearLayout.ChildCount-1; i++) // skip the first and the last child
                //{
                //    SubTasksLinearLayout.GetChildAt(i).Click += (sender, e) => subTaskClickListener(base.LayoutPosition);
                //}
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
        protected void OnClick(int position)
        {
            if (ItemClick != null)
            {
                ItemClick(this, position);
            }
        }

        ////This will fire any event handlers that are registered with our DeleteItemClick
        ////event.
        //protected void OnSubTaskClick(int position)
        //{
        //    if (SubTaskClick != null)
        //    {
        //        SubTaskClick(this, position);
        //    }
        //}

        //This will fire any event handlers that are registered with our DeleteItemClick
        //event.
        protected void OnDeleteClick(int position)
        {
            if (DeleteClick != null)
            {
                DeleteClick(this, position);
            }
        }

        //This will fire any event handlers that are registered with our DeleteItemClick
        //event.
        protected void OnCompleteClick(int position)
        {
            if (CompleteClick != null)
            {
                CompleteClick(this, position);
            }
        }

        //This will fire any event handlers that are registered with our DeleteItemClick
        //event.
        protected void OnShareClick(int position)
        {
            if (ShareClick != null)
            {
                ShareClick(this, position);
            }
        }


        //This will fire any event handlers that are registered with our DeleteItemClick
        //event.
        protected void OnImageClick(int position)
        {
            if (ImageClick != null)
            {
                ImageClick(this, position);
            }
        }


        public ItemRecyclerViewAdapter(Android.App.Activity context, Todo.TreeNodeList<Todo.Item> items)
        {
            parent = context;

            this.items = items;

            this.ItemClick += OnItemClick;
            //this.SubTaskClick += OnSubTaskClick;
            this.DeleteClick += OnDeleteClick;
            this.CompleteClick += OnCompleteClick;
            this.ShareClick += OnShareClick;
            this.ImageClick += OnImageClick;
        }

        public void ChangeDateSet(Todo.TreeNodeList<Todo.Item> items)
        {
            this.items = items;
            NotifyDataSetChanged();
        }

        //protected void OnSubTaskClick(object sender, int e)
        //{
        //    var adapter = sender as ItemRecyclerViewAdapter;
        //    var item = adapter.GetValueAt(e);

        //    var context = fragment.Context;//h.View.Context;
        //    var intent = new Intent(context, typeof(CheeseDetailActivity));
        //    //intent.PutExtra(CheeseDetailActivity.EXTRA_NAME, item.na);
        //    //intent.PutExtra(CheeseDetailActivity.ITEM_ID, subitem.id);

        //    //parent.StartActivityForResult(intent, ITEMDETAIL);
        //}

        protected void OnDeleteClick(object sender, int e)
        {
            var adapter = sender as ItemRecyclerViewAdapter;
            var item = adapter.GetValueAt(e);


            new Android.Support.V7.App.AlertDialog.Builder(parent)
            .SetMessage("Delete this item?")
            .SetCancelable(false)
            .SetPositiveButton("Yes", async delegate
            {
                var dataSetID = item.Parent.Value.id;
                var parent = await adapter.items.Remove(item);

                    //RecursiveDelete(item);

                    //var itemInTree = PublicFields.ItemTree.Descendants().First(it => it.Value.id == item.Value.id);
                    //await itemInTree.Parent.Children.Remove(itemInTree);

                    var dataset = PublicFields.ItemTree.Descendants().First(it => it.Value.id == dataSetID).Children;
                ChangeDateSet(dataset);

                    //NotifyItemRemoved(e);
                    NotifyDataSetChanged();

                Log.Debug("CheeseListFragment", string.Format("Removed item {0} and its subitems", item.Value.Name));
            })
           .SetNegativeButton("No", delegate
           {
               Log.Debug("CheeseListFragment", string.Format("Did not remove item {0}", item.Value.Name));
           })
           .Show();
        }

        protected void OnCompleteClick(object sender, int e)
        {
            var adapter = sender as ItemRecyclerViewAdapter;
            var item = adapter.GetValueAt(e);


            new Android.Support.V7.App.AlertDialog.Builder(parent)
            .SetMessage("Complete this item?")
            .SetCancelable(false)
            .SetPositiveButton("Yes", async delegate
            {
                var dataSetID = item.Parent.Value.id;
                var parent = await adapter.items.Complete(item);

                var dataset = PublicFields.ItemTree.Descendants().First(it => it.Value.id == dataSetID).Children;
                ChangeDateSet(dataset);

                NotifyDataSetChanged();

                Log.Debug("CheeseListFragment", string.Format("Completed item {0} and its subitems", item.Value.Name));
            })
           .SetNegativeButton("No", delegate
           {
               Log.Debug("CheeseListFragment", string.Format("Did not complete item {0} and its subitems", item.Value.Name));
           })
           .Show();
        }

        protected void OnShareClick(object sender, int e)
        {
            var adapter = sender as ItemRecyclerViewAdapter;
            var item = adapter.GetValueAt(e);

            new Android.Support.V7.App.AlertDialog.Builder(parent)
            .SetMessage("Share this item?")
            .SetCancelable(false)
            .SetPositiveButton("Yes", delegate
            {
                Log.Debug("CheeseListFragment", string.Format("Shared item {0} and its subitems", item.Value.Name));

                var intent = new Intent(parent, typeof(SelectContactsActivity));
                intent.PutExtra("itemID", item.Value.id);
                parent.StartActivityForResult(intent, SHARE_CONTACT);
            })
           .SetNegativeButton("No", delegate
           {
               Log.Debug("CheeseListFragment", string.Format("Did not share item {0} and its subitems", item.Value.Name));
           })
           .Show();
        }


        protected void OnItemClick(object sender, int e)
        {
            var adapter = sender as ItemRecyclerViewAdapter;
            var item = adapter.GetValueAt(e);

            var intent = new Intent(parent, typeof(DetailActivity));
            intent.PutExtra(DetailActivity.EXTRA_NAME, item.Value.Name);
            intent.PutExtra(DetailActivity.ITEM_ID, item.Value.id);
            parent.StartActivityForResult(intent, ITEMDETAIL);
            //context.StartActivity(intent);
        }

        protected void OnImageClick(object sender, int e)
        {
            var adapter = sender as ItemRecyclerViewAdapter;
            var item = adapter.GetValueAt(e);

            //var intentAbstraction = new Intent(parent, typeof(PickImageActivity));
            //intentAbstraction.PutExtra(CheeseDetailActivity.ITEM_ID, item.Value.id);

            //parent.StartActivityForResult(intentAbstraction, PICKIMAGE);

            var intent = new Intent(parent, typeof(SelectImageActivity));
            intent.PutExtra(DetailActivity.ITEM_ID, item.Value.id);

            parent.StartActivityForResult(intent, PICKIMAGE);
        }

        public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
        {

            var view = LayoutInflater.From(parent.Context)
                .Inflate(Resource.Layout.item_card_view, parent, false);

            var vh = new ViewHolder(view, OnClick, OnDeleteClick, OnCompleteClick, OnShareClick, OnImageClick);
            return vh;
        }

        //protected void SubtaskCheckBox_CheckedChange(object sender, CompoundButton.CheckedChangeEventArgs e)
        //{
        //    var checkBox = (CheckBox)sender;
        //    Log.Debug("CheeseListFragment", "checkbox clicked");
        //    if(checkBox.Checked)
        //    {
        //        Log.Debug("CheeseListFragment", "checkbox clicked");

        //        Node.Value.Status = 7;
        //        await PublicFields.Database.SaveItem(Node.Value);
        //        Parent.AmountOfChildrenCompletedChanged(1);
        //    }

        //}

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

            if (item.AmountOfChildren != 0)
            {
                h.Progress.Text = string.Format("{0}%", (item.AmountOfChildrenCompleted / item.AmountOfChildren * 100));
            }
            else // no children
            {
                h.Progress.Text = item.Value.Status == 7 ? "100%" : "0%";
            }

            if (h.Progress.Text == "100%") // completed
                item.Value.Status = 7;
            else
            {
                if (item.Value.Status == 7) // status is completed but not all the subitems have been completed
                {
                    item.Value.Status = 2; // status is Started
                }
            }

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
                    subtaskCheckBox.Checked = subitem.Value.Status == 7; // set checked if completed
                    subtaskCheckBox.CheckedChange += async (sender, e) =>// SubtaskCheckBox_CheckedChange;
                    {
                        var checkBox = (CheckBox)sender;
                        if (checkBox.Checked)
                        {
                            Log.Debug("CheeseListFragment", string.Format("{0} checked as completed", subitem.Value.id));

                            subitem.Value.Status = 7;
                            await PublicFields.Database.SaveItem(subitem.Value);
                            subitem.Parent.AmountOfChildrenCompletedChanged(1);
                        }
                        else
                        {
                            Log.Debug("CheeseListFragment", string.Format("{0} unchecked as completed", subitem.Value.id));

                            subitem.Value.Status = 0;
                            await PublicFields.Database.SaveItem(subitem.Value);
                            subitem.Parent.AmountOfChildrenCompletedChanged(-1);
                        }
                    };

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
            if (item.Value.ImagePath != null)
            {
                //Bitmap bmImg = BitmapFactory.DecodeFile(item.Value.ImagePath);

                BitmapFactory.Options options = new BitmapFactory.Options();
                options.InJustDecodeBounds = true;
                BitmapFactory.DecodeFile(item.Value.ImagePath, options);
                int imageHeight = options.OutHeight;
                int imageWidth = options.OutWidth;
                String imageType = options.OutMimeType;

                var sampledBitmap = PublicFields.DecodeSampledBitmapFromFile(item.Value.ImagePath, 80, 80);

                h.ImageView.SetImageBitmap(sampledBitmap);
                //h.ImageView.SetImageURI(item.Value.ImageUri);
            }
            else if(!string.IsNullOrEmpty(item.Value.ResourceUrl))
            {
                switch (item.Value.ResourceUrl)
                {
                    case "Goal":
                        // goal
                        h.ImageView.SetImageResource(Resource.Drawable.Goal256);

                        item.Value.ImageResource = Resource.Drawable.Goal256;
                        item.Value.ImageResourceBackdrop = Resource.Drawable.Goal1024;
                        break;
                    case "Project":
                        // project
                        h.ImageView.SetImageResource(Resource.Drawable.Project256);

                        item.Value.ImageResource = Resource.Drawable.Project256;
                        item.Value.ImageResourceBackdrop = Resource.Drawable.Project1024;
                        break;
                    case "Task":
                        // task
                        h.ImageView.SetImageResource(Resource.Drawable.Task256);

                        item.Value.ImageResource = Resource.Drawable.Task256;
                        item.Value.ImageResourceBackdrop = Resource.Drawable.Task1024;
                        break;
                    default:
                        // handled same as task
                        h.ImageView.SetImageResource(Resource.Drawable.Task256);

                        item.Value.ImageResource = Resource.Drawable.Task256;
                        item.Value.ImageResourceBackdrop = Resource.Drawable.Task1024;
                        break;
                }
            }
            else if (item.Value.ImageResource != 0)
            {
                //h.ImageView.LayoutParameters = new GridView.LayoutParams(80, 80);
                //h.ImageView.SetScaleType(ImageView.ScaleType.CenterCrop);

                h.ImageView.SetImageResource(item.Value.ImageResource);
            }
            else
            {
                //h.ImageView.LayoutParameters = new GridView.LayoutParams(80, 80);
                //h.ImageView.SetScaleType(ImageView.ScaleType.CenterCrop);

                switch (item.Value.Type)
                {
                    case 1:
                        //domain
                        break;
                    case 2:
                        // goal
                        h.ImageView.SetImageResource(Resource.Drawable.Goal256);

                        item.Value.ImageResource = Resource.Drawable.Goal256;
                        item.Value.ImageResourceBackdrop = Resource.Drawable.Goal1024;
                        break;
                    case 3:
                        // project
                        h.ImageView.SetImageResource(Resource.Drawable.Project256);

                        item.Value.ImageResource = Resource.Drawable.Project256;
                        item.Value.ImageResourceBackdrop = Resource.Drawable.Project1024;
                        break;
                    case 4:
                        // task
                        h.ImageView.SetImageResource(Resource.Drawable.Task256);

                        item.Value.ImageResource = Resource.Drawable.Task256;
                        item.Value.ImageResourceBackdrop = Resource.Drawable.Task1024;
                        break;
                    default:
                        // handled same as task
                        h.ImageView.SetImageResource(Resource.Drawable.Task256);

                        item.Value.ImageResource = Resource.Drawable.Task256;
                        item.Value.ImageResourceBackdrop = Resource.Drawable.Task1024;
                        break;
                }

                //h.ImageView.SetImageDrawable(Cheeses.GetRandomCheeseDrawable(parent));
            }
        }

        public override int ItemCount
        {
            get { return items.Count; }
        }
    }

    [Activity (Label="Details", ScreenOrientation = ScreenOrientation.Portrait)]
    [MetaData("android.support.PARENT_ACTIVITY", Value = "com.sample.mindset.MainActivity")]
    public class DetailActivity : AppCompatActivity
    {
        public const string EXTRA_NAME = "item_name";
        public const string ITEM_ID = "item_id";

        private const int ITEMDETAIL = 103;
        private const int EDIT_ITEM = 104;
        private const int PICKIMAGE = 105;

        private const string TAG = "DetailActivity";

        //bool editmode;

        private TextView txtDate;
        private String itemID;
        //private LinearLayout titleDescriptionLayout;

        private RatingBar importance;

        private SeekBar progressSlider;
        private TextView progressPercentText;

        private TextView comment;
        private TextView status;
        private ImageView statusIcon;
        private RelativeLayout statusRelLayout;

        private Android.Support.V7.Widget.PopupMenu statusMenu;

        TextView userName;

        private FloatingActionButton editFAB;
        private FloatingActionButton addItemFAB;
        private CollapsingToolbarLayout collapsingToolbar;
        private DrawerLayout drawerLayout;
        //private WrapContentHeightViewPager viewPager;
        private ScrollForwardingRecyclerView recyclerview;

        private Todo.TreeNode<Todo.Item> item;
        public bool itemChanged;
        public bool databaseUpdated;
        private ItemRecyclerViewAdapter itemRecyclerViewAdapter;
        private RecyclerView.AdapterDataObserver dataObserver;

        //private RecyclerView.AdapterDataObserver dataObserver;// = new DataObserver();

        protected override void OnCreate (Bundle savedInstanceState) 
        {
            base.OnCreate (savedInstanceState);
            SetContentView(Resource.Layout.activity_detail);

            //dataObserver = new DataObserver(this);
            itemChanged = false;
            databaseUpdated = false;

            drawerLayout = FindViewById<DrawerLayout>(Resource.Id.drawer_layout_detail);

            var toolbar = FindViewById<V7Toolbar>(Resource.Id.toolbar);
            SetSupportActionBar (toolbar);

            var navigationView = drawerLayout.FindViewById<NavigationView>(Resource.Id.nav_view_detail);
            if (navigationView != null)
                setupDrawerContent(navigationView);

            userName = navigationView.GetHeaderView(0).FindViewById<TextView>(Resource.Id.username_nav);

            SupportActionBar.SetHomeAsUpIndicator(Resource.Drawable.ic_menu);
            SupportActionBar.SetDisplayHomeAsUpEnabled(true);

            collapsingToolbar = FindViewById<CollapsingToolbarLayout> (Resource.Id.collapsing_toolbar);

            txtDate = FindViewById<TextView>(Resource.Id.txtdate);

            progressSlider = FindViewById<SeekBar>(Resource.Id.progressSlider);
            progressSlider.Touch += (o, e) =>
            {
                return;
            };
            progressPercentText = FindViewById<TextView>(Resource.Id.progressPercentText);
            progressSlider.ProgressChanged += ProgressSlider_ProgressChanged;

            importance = FindViewById<RatingBar>(Resource.Id.ratingbar);
            importance.RatingBarChange += Importance_RatingBarChange;

            comment = FindViewById<TextView>(Resource.Id.comment_text);

            statusIcon = FindViewById<ImageView>(Resource.Id.status_icon);
            status = FindViewById<TextView>(Resource.Id.status_text);
            statusRelLayout = FindViewById<RelativeLayout>(Resource.Id.status_rel_layout);
            statusRelLayout.Clickable = true;

            statusRelLayout.Click += statusClick;

            //statusIcon.Click += statusClick;
            //status.Click += statusClick;

            editFAB = FindViewById<FloatingActionButton>(Resource.Id.edit_fab);
            editFAB.Click += EditFAB_Click;

            addItemFAB = FindViewById<FloatingActionButton>(Resource.Id.add_task_fab);
            addItemFAB.Click += AddItemFAB_Click;

            //viewPager = FindViewById<WrapContentHeightViewPager>(Resource.Id.viewpager_cards_detail);
            recyclerview = FindViewById<ScrollForwardingRecyclerView>(Resource.Id.recyclerview_detail);
            recyclerview.NestedScrollingEnabled = false;
            recyclerview.HasFixedSize = false;

            loadBackdrop();
        }

        private void statusClick(object sender, EventArgs e)
        {
            statusMenu = new Android.Support.V7.Widget.PopupMenu(this, statusIcon);
            statusMenu.Inflate(Resource.Menu.status_popup_menu);

            Field field = statusMenu.Class.GetDeclaredField("mPopup");
            field.Accessible = true;
            Java.Lang.Object menuPopupHelper = field.Get(statusMenu);
            Method setForceIcons = menuPopupHelper.Class.GetDeclaredMethod("setForceShowIcon", Java.Lang.Boolean.Type);
            setForceIcons.Invoke(menuPopupHelper, true);

            statusMenu.MenuItemClick += (s1, arg1) => {
                Console.WriteLine("{0} selected", arg1.Item.TitleFormatted);
                status.Text = arg1.Item.TitleFormatted.ToString();
                statusIcon.SetImageDrawable(arg1.Item.Icon);

                switch (status.Text)
                {
                    case "Cancelled":
                        item.Value.Status = -1;
                        break;
                    case "Backlog":
                        item.Value.Status = 0;
                        break;
                    case "On Hold":
                        item.Value.Status = 6;
                        break;
                    case "Completed":
                        item.Value.Status = 7;
                        break;
                    case "Started":
                        item.Value.Status = 2;
                        break;
                    default:
                        item.Value.Status = 2;
                        break;
                }

                itemChanged = true;
            };

            statusMenu.DismissEvent += (s2, arg2) => {
                Console.WriteLine("menu dismissed");
            };

            statusMenu.Show();
        }

        private void Importance_RatingBarChange(object sender, RatingBar.RatingBarChangeEventArgs e)
        {
            var ratingbar = (RatingBar) sender;
            int rating = (int)ratingbar.Rating;

            item.Value.Importance = rating;
            PublicFields.ItemTree.Root.Descendants().FirstOrDefault(node => node.Value.id == item.Value.id).Value.Importance = rating;
            //PublicFields.ItemDictionary[item.Value.id].Value.Importance = rating;
            itemChanged = true;
        }


        protected async override void OnActivityResult(int requestCode, Result resultCode,
Intent intent)
        {
            base.OnActivityResult(requestCode, resultCode, intent);

            if (resultCode == Result.Ok)
            {
                List<TreeNode<Todo.Item>> allItemsList = PublicFields.ItemTree.Descendants().ToList();

                //int index = viewPager.CurrentItem;
                //var adapter = (MyAdapter)viewPager.Adapter;
                //var currentFragment = (ListFragment)adapter.GetItem(index);
                //var fragmentAdapter = currentFragment.itemRecyclerViewAdapter;

                switch (requestCode)
                {
                    case PICKIMAGE:
                        if (intent != null)
                        {
                            Android.Net.Uri uri = intent.Data;
                            string ItemID = intent.GetStringExtra(DetailActivity.ITEM_ID);
                            string path = intent.GetStringExtra("path");
                            int resourceID = intent.GetIntExtra("resourceID", 0);
                            int resourceBackdropID = intent.GetIntExtra("resourceBackdropID", 0);

                            if (resourceID != 0)
                            {
                                var item = PublicFields.ItemTree.Descendants().FirstOrDefault(node => node.Value.id == ItemID);
                                item.Value.ImageResource = resourceID;
                                item.Value.ImageResourceBackdrop = resourceBackdropID;

                                switch (resourceID)
                                {
                                    case 1:
                                        //domain
                                        break;
                                    case Resource.Drawable.Goal256:
                                        // goal
                                        item.Value.ResourceUrl = "Goal";
                                        break;
                                    case Resource.Drawable.Project256:
                                        // project
                                        item.Value.ResourceUrl = "Project";
                                        break;
                                    case Resource.Drawable.Task256:
                                        // task
                                        item.Value.ResourceUrl = "Task";
                                        break;
                                    default:
                                        // handled same as task
                                        item.Value.ResourceUrl = "";
                                        break;
                                }

                                await PublicFields.Database.SaveItem(item.Value);

                                //fragmentAdapter.UpdateValue(item);
                                //fragmentAdapter.ApplyChanges();
                            }
                            else
                            {
                                var item = PublicFields.ItemTree.Descendants().FirstOrDefault(node => node.Value.id == ItemID);
                                item.Value.ImagePath = path;

                                //fragmentAdapter.UpdateValue(item);
                                //fragmentAdapter.ApplyChanges();
                            }
                        }
                        break;
                    case
                        ITEMDETAIL: // back from other detail activity
                        if (intent.GetBooleanExtra("itemChanged", false))
                        {
                            string ItemID = intent.GetStringExtra("itemID");
                            Log.Debug("MainActivity", "Item changed");

                            var item = PublicFields.ItemTree.Root.Descendants().FirstOrDefault(node => node.Value.id == ItemID);

                            //fragmentAdapter.UpdateValue(item);
                            //fragmentAdapter.ApplyChanges();
                        }

                        break;
                    case
                        EDIT_ITEM: // edited the detail item itself

                        var edited = intent.GetBooleanExtra("edited", false); // pressed the edit button or added new one
                        string editItemId = intent.GetStringExtra("itemID");
                        bool groupChanged = intent.GetBooleanExtra("groupChanged", false);

                        itemChanged = true;

                        // update the item of this detail view
                        this.item = PublicFields.ItemTree.Descendants().FirstOrDefault(node => node.Value.id == item.Value.id); 

                        if (groupChanged) // sharing an item
                        {
                            List<Todo.User> selectedContacts = JsonConvert.DeserializeObject<List<Todo.User>>(intent.GetStringExtra("selectedContacts"));
                            string groupName = intent.GetStringExtra("groupName"); // only has a name if user generated group
                            //TODO: check if group already exists

                            if (selectedContacts != null && selectedContacts.Count > 0) // contacts selected so make a group for them
                            {
                                var ownedByGroupResult = await PublicFields.Database.SaveGroup(selectedContacts, groupName);
                                if (ownedByGroupResult != null)
                                {
                                    item.Value.OwnedBy = ownedByGroupResult.id;
                                    PublicFields.ItemTree.FindAndReplace(item.Value.id, item);
                                }

                                var groupMembers = await PublicFields.Database.MembersOfGroup(ownedByGroupResult);

                                foreach(var grp in  groupMembers)
                                {
                                    // this account does not need the id. only the ones the item gets shared with
                                    if (grp != null && grp.id != PublicFields.Database.defGroup.id)
                                    {
                                        var link = new ItemLink { ItemID = item.Value.id, Parent = null, OwnedBy = grp.id };
                                        await PublicFields.Database.SaveItemLink(link);
                                    }
                                }
                            }



                        }

                        

                        if (edited && this.item != null) // the detail item
                        {
                            await PublicFields.Database.SaveItem(this.item.Value);

                            for (int i = 0; i < item.Children.Count; i++) // check if the subitems of the new card are new as well, if so save them
                            {
                                var it = item.Children[i];
                                if (string.IsNullOrEmpty(it.Value.id))
                                    await PublicFields.Database.SaveItem(it.Value);

                                item.Children[i] = it; // store with newly acquired id
                            }

                            // refresh values
                            collapsingToolbar.SetTitle(item.Value.Name);
                            importance.Rating = item.Value.Importance;

                            if (item.Value.EndDate != null)
                            {
                                //String givenDateString = "Tue Apr 23 16:08:28 GMT+05:30 2013";
                                //SimpleDateFormat sdf = new SimpleDateFormat("dd-MM-yyyy");//new SimpleDateFormat("EEE MMM dd HH:mm:ss z yyyy");
                                try
                                {
                                    //Date mDate = sdf.Parse(item.EndDate);

                                    long timeInMilliseconds = (long)(TimeZoneInfo.ConvertTimeToUtc(item.Value.EndDate) -
                                    new DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc)).TotalMilliseconds;

                                    if (timeInMilliseconds >= 0)
                                        txtDate.Text = DateUtils.GetRelativeTimeSpanString(Application.Context, timeInMilliseconds);
                                }
                                catch (ParseException e)
                                {
                                    e.PrintStackTrace();
                                }
                            }

                            switch (item.Value.Status)
                            {
                                case -1:
                                    status.Text = "Cancelled";
                                    statusIcon.SetImageResource(Resource.Drawable.ic_clear_black_24dp);
                                    break;
                                case 0:
                                    status.Text = "Backlog";
                                    statusIcon.SetImageResource(Resource.Drawable.ic_inbox_black_24dp);
                                    break;
                                case 6:
                                    status.Text = "On Hold";
                                    statusIcon.SetImageResource(Resource.Drawable.ic_block_black_24dp);
                                    break;
                                case 7:
                                    status.Text = "Completed";
                                    statusIcon.SetImageResource(Resource.Drawable.ic_check_black_24dp);
                                    break;
                                default:
                                    status.Text = "Started";
                                    statusIcon.SetImageResource(Resource.Drawable.ic_play_arrow_black_24dp);
                                    break;

                            }

                            if (string.IsNullOrEmpty(item.Value.Notes))
                            {
                                comment.Text = "No notes";
                            }
                            else
                            {
                                comment.Text = item.Value.Notes;
                            }
                        }
                        else // added a new cardview (subItem)
                        {
                            var itemCard = PublicFields.ItemTree.Descendants().FirstOrDefault(node => node.Value.id == editItemId); // update the item of this detail view
                            var tempID = itemCard.Value.id;
                            itemCard.Value.id = null; //get rid of the temporaryID string

                            await PublicFields.Database.SaveItem(itemCard.Value);

                            for(int i = 0; i< itemCard.Children.Count; i++) // check if the subitems of the new card are new as well, if so save them
                            {
                                var it = itemCard.Children[i];
                                it.Value.Parent = itemCard.Value.id; // change the parent id to the new one

                                if(string.IsNullOrEmpty(it.Value.id))
                                    await PublicFields.Database.SaveItem(it.Value);

                                itemCard.Children[i] = it; // store with newly acquired id

                                //PublicFields.ItemTree.FindAndReplace(it.Value.id, it);
                            }
                        }
                        this.item = PublicFields.ItemTree.Descendants().FirstOrDefault(node => node.Value.id == item.Value.id);
                        break;
                    default:
                        break;
                }
            }
        }

        void setupDrawerContent(NavigationView navigationView)
        {
            navigationView.NavigationItemSelected += (sender, e) => {
                Intent intent;
                switch (e.MenuItem.ItemId)
                {
                    case Resource.Id.nav_home:
                        intent = new Intent(this, typeof(MainActivity));
                        intent.AddFlags(ActivityFlags.ClearTop);
                        StartActivity(intent);
                        //NavigateUpTo(ParentActivityIntent);
                        drawerLayout.CloseDrawers();
                        Finish();
                        //NavUtils.NavigateUpFromSameTask(this);
                        break;
                    case Resource.Id.shared_items:
                        intent = new Intent(this, typeof(SharedItemsActivity));
                        intent.AddFlags(ActivityFlags.ClearTop);
                        drawerLayout.CloseDrawers();
                        StartActivity(intent);
                        break;
                    case Resource.Id.groups:
                        intent = new Intent(this, typeof(GroupsActivity));
                        intent.AddFlags(ActivityFlags.ClearTop);
                        drawerLayout.CloseDrawers();
                        StartActivity(intent);
                        break;
                }
                //e.P0.SetChecked (true);
                drawerLayout.CloseDrawers();
            };
        }


        public override bool OnCreateOptionsMenu(IMenu menu)
        {
            MenuInflater.Inflate(Resource.Menu.sample_actions, menu);
            return true;
        }

        public override bool OnOptionsItemSelected(IMenuItem menuItem)
        {
            switch (menuItem.ItemId)
            {
                case Android.Resource.Id.Home:
                    drawerLayout.OpenDrawer(Android.Support.V4.View.GravityCompat.Start);
                    return true;
                case Resource.Id.refresh:
                    Log.Debug("detail", "attempting refresh");

                    PublicFields.UpdateDatabase();

                    PublicFields.MakeTree();
                    item = PublicFields.ItemTree.Descendants().FirstOrDefault(node => node.Value.id == itemID);
                    //setupViewPager(viewPager);


                    // refresh values detail item
                    collapsingToolbar.SetTitle(item.Value.Name);
                    importance.Rating = item.Value.Importance;

                    if (item.Value.EndDate != null)
                    {
                        //String givenDateString = "Tue Apr 23 16:08:28 GMT+05:30 2013";
                        //SimpleDateFormat sdf = new SimpleDateFormat("dd-MM-yyyy");//new SimpleDateFormat("EEE MMM dd HH:mm:ss z yyyy");
                        try
                        {
                            //Date mDate = sdf.Parse(item.EndDate);

                            long timeInMilliseconds = (long)(TimeZoneInfo.ConvertTimeToUtc(item.Value.EndDate) -
                            new DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc)).TotalMilliseconds;

                            if (timeInMilliseconds >= 0)
                                txtDate.Text = DateUtils.GetRelativeTimeSpanString(Application.Context, timeInMilliseconds);
                        }
                        catch (ParseException e)
                        {
                            e.PrintStackTrace();
                        }
                    }

                    switch (item.Value.Status)
                    {
                        case -1:
                            status.Text = "Cancelled";
                            statusIcon.SetImageResource(Resource.Drawable.ic_clear_black_24dp);
                            break;
                        case 0:
                            status.Text = "Backlog";
                            statusIcon.SetImageResource(Resource.Drawable.ic_inbox_black_24dp);
                            break;
                        case 6:
                            status.Text = "On Hold";
                            statusIcon.SetImageResource(Resource.Drawable.ic_block_black_24dp);
                            break;
                        case 7:
                            status.Text = "Completed";
                            statusIcon.SetImageResource(Resource.Drawable.ic_check_black_24dp);
                            break;
                        default:
                            status.Text = "Started";
                            statusIcon.SetImageResource(Resource.Drawable.ic_play_arrow_black_24dp);
                            break;

                    }

                    if (string.IsNullOrEmpty(item.Value.Notes))
                    {
                        comment.Text = "No notes";
                    }
                    else
                    {
                        comment.Text = item.Value.Notes;
                    }

                    databaseUpdated = true;

                    return true;

            }
            return base.OnOptionsItemSelected(menuItem);
        }

        private void AddItemFAB_Click(object sender, EventArgs e)
        {
            var intent = new Intent(this, typeof(EditItemActivity));
            intent.PutExtra("newItem", true);
            intent.PutExtra("parentItemID", item.Value.id);
            StartActivityForResult(intent, EDIT_ITEM);
        }

        private void EditFAB_Click(object sender, EventArgs e)
        {
            Log.Info(TAG, "edit fab clicked!");
            var intent = new Intent(this, typeof(EditItemActivity));
            intent.PutExtra("itemID", item.Value.id);
            intent.PutExtra("parentItemID", item.Parent.Value.id);
            StartActivityForResult(intent, EDIT_ITEM);
        }

        private void ProgressSlider_ProgressChanged(object sender, SeekBar.ProgressChangedEventArgs e)
        {
            progressPercentText.Text = string.Format("{0}%", progressSlider.Progress);
        }

        protected void setupRecyclerView()
        {
            recyclerview.SetLayoutManager(new LinearLayoutManager(recyclerview.Context));

            // provide 16dp of padding at the top of the list, 100 dp at the bottom
            DisplayMetrics metrics = new DisplayMetrics();
            IWindowManager windowManager = this.GetSystemService(Context.WindowService).JavaCast<IWindowManager>();
            windowManager.DefaultDisplay.GetMetrics(metrics);

            int sixteenDPTop = (int)Math.Ceiling(16 * metrics.Density);
            int hundredDPBottom = (int)Math.Ceiling(100 * metrics.Density);
            recyclerview.SetClipToPadding(false);
            recyclerview.SetPadding(0, sixteenDPTop, 0, hundredDPBottom);


            itemRecyclerViewAdapter = new ItemRecyclerViewAdapter(this, item.Children);
            recyclerview.SetAdapter(itemRecyclerViewAdapter);
            if (dataObserver != null)
                itemRecyclerViewAdapter.RegisterAdapterDataObserver(dataObserver);
        }

        protected override void OnStart()
        {
            base.OnStart();

            itemID = Intent.GetStringExtra(ITEM_ID);
            item = PublicFields.ItemTree.Descendants().FirstOrDefault(node => node.Value.id == itemID);

            if (item == null || item.Value == null)
            {
                RunOnUiThread(
                    () =>
                    {
                        new Android.Support.V7.App.AlertDialog.Builder(this)
                        .SetMessage("The item could not be found")
                        .SetCancelable(false)
                        .SetPositiveButton("OK", delegate
                        {
                            Finish();
                        })
                        .Show();
                    }
                );
            }
            else
            {
                userName.Text = PublicFields.Database.userName;
                collapsingToolbar.SetTitle(item.Value.Name);

                importance.Rating = item.Value.Importance;

                if (item.AmountOfChildren != 0)
                {
                    progressSlider.Progress = (item.AmountOfChildrenCompleted / item.AmountOfChildren * 100);
                }
                else // no children
                {
                    progressSlider.Progress = item.Value.Status == 7 ? 100 : 0;
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
                            txtDate.Text = DateUtils.GetRelativeTimeSpanString(Application.Context, timeInMilliseconds);
                    }
                    catch (ParseException e)
                    {
                        e.PrintStackTrace();
                    }
                }
                else
                {
                    txtDate.Text = "No due date";
                }

                if (string.IsNullOrEmpty(item.Value.Notes))
                {
                    comment.Text = "No notes";
                }
                else
                {
                    comment.Text = item.Value.Notes;
                }

                switch (item.Value.Status)
                {
                    case -1:
                        status.Text = "Cancelled";
                        statusIcon.SetImageResource(Resource.Drawable.ic_clear_black_24dp);
                        break;
                    case 0:
                        status.Text = "Backlog";
                        statusIcon.SetImageResource(Resource.Drawable.ic_inbox_black_24dp);
                        break;
                    case 6:
                        status.Text = "On Hold";
                        statusIcon.SetImageResource(Resource.Drawable.ic_block_black_24dp);
                        break;
                    case 7:
                        status.Text = "Completed";
                        statusIcon.SetImageResource(Resource.Drawable.ic_check_black_24dp);
                        break;
                    default:
                        status.Text = "Started";
                        statusIcon.SetImageResource(Resource.Drawable.ic_play_arrow_black_24dp);
                        break;

                }

                setupRecyclerView();
                
            }
        }

        public override void Finish()
        {
            Intent returnIntent = new Intent();
            returnIntent.PutExtra("itemChanged", itemChanged);// ("passed_item", itemYouJustCreated);
            returnIntent.PutExtra("databaseUpdated", databaseUpdated);
            if (itemChanged)
                returnIntent.PutExtra("itemID", item.Value.id);// ("passed_item", itemYouJustCreated);
            SetResult(Result.Ok, returnIntent);
            base.Finish();
        }


        void loadBackdrop() 
        {            
            var imageView = FindViewById<ImageView> (Resource.Id.backdrop);

            itemID = Intent.GetStringExtra(ITEM_ID);
            item = PublicFields.ItemTree.Descendants().FirstOrDefault(node => node.Value.id == itemID);
            if (item.Value.ImagePath != null)
            {
                BitmapFactory.Options options = new BitmapFactory.Options();
                options.InJustDecodeBounds = true;
                BitmapFactory.DecodeFile(item.Value.ImagePath, options);
                int imageHeight = options.OutHeight;
                int imageWidth = options.OutWidth;
                String imageType = options.OutMimeType;

                var sampledBitmap = PublicFields.DecodeSampledBitmapFromFile(item.Value.ImagePath, 500, 500);

                imageView.SetImageBitmap(sampledBitmap);
            }
            else if (item.Value.ImageResourceBackdrop != 0)
            {
                imageView.SetImageResource(item.Value.ImageResourceBackdrop);
            }
        }

        /// <summary>
        /// Necessary OnDestroy because the garbage collection is too slow, otherwise moving quickly between items can lead to out of memory errors.
        /// </summary>
        protected override void OnDestroy()
        {
            base.OnDestroy();

            var imageView = FindViewById<ImageView>(Resource.Id.backdrop);
            imageView.SetImageDrawable(null);
        }
    }
}

