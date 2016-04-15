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
using Android.Graphics;

namespace MindSet
{
    [Register("com.sample.mindset.ScrollForwardingRecyclerView")]
    public class ScrollForwardingRecyclerView : RecyclerView//, GestureDetector.IOnGestureListener
    {
        //protected GestureDetectorCompat mDetector;

        public ScrollForwardingRecyclerView(Context context): base(context)
        {
            NestedScrollingEnabled = true;
            //mDetector = new GestureDetectorCompat(context, this);
        }

        public ScrollForwardingRecyclerView(Context context, IAttributeSet attributes) : base(context, attributes)
        {
            NestedScrollingEnabled = true;
            //mDetector = new GestureDetectorCompat(context, this);
        }

        //public override bool OnTouchEvent(MotionEvent e)
        //{
        //    Boolean handled = mDetector.OnTouchEvent(e);
        //    if (!handled && e.Action == MotionEventActions.Up) {
        //        StopNestedScroll();
        //    }
        //    return true;
        //}

        //public bool OnFling(MotionEvent e1, MotionEvent e2, float velocityX, float velocityY)
        //{
        //    //throw new NotImplementedException();
        //    return true;
        //}

        //public void OnLongPress(MotionEvent e)
        //{
        //    throw new NotImplementedException();
        //}

        //public bool OnScroll(MotionEvent e1, MotionEvent e2, float distanceX, float distanceY)
        //{
        //    DispatchNestedPreScroll(0, (int)distanceY, null, null);
        //    DispatchNestedScroll(0, 0, 0, 0, null);
        //    return true;
        //}

        //public void OnShowPress(MotionEvent e)
        //{
        //    //throw new NotImplementedException();
        //}

        //public bool OnSingleTapUp(MotionEvent e)
        //{
        //    return true;
        //    //throw new NotImplementedException();
        //}
    }

    public class ListFragment : Android.Support.V4.App.Fragment
    {
        public Todo.TreeNode<Todo.Item> domain;

        public ItemRecyclerViewAdapter itemRecyclerViewAdapter;
        private const int SHARE_CONTACT = 101;
        public const int ITEMDETAIL = 103;
        public const int PICKIMAGE = 105;
        RecyclerView.AdapterDataObserver dataObserver;

        public ListFragment(Todo.TreeNode<Todo.Item> dom, RecyclerView.AdapterDataObserver DataObserver)
        {
            domain = dom;
            dataObserver = DataObserver;
        }

        public ListFragment(Todo.TreeNode<Todo.Item> dom)
        {
            domain = dom;
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            var v = inflater.Inflate(
                Resource.Layout.fragment_cheese_list, container, false);
            var rv = (ScrollForwardingRecyclerView)v;//v.JavaCast<ScrollForwardingRecyclerView>();


            setupRecyclerView(rv);
            return rv;
        }



        protected void setupRecyclerView(RecyclerView recyclerView)
        {
            recyclerView.SetLayoutManager(new LinearLayoutManager(recyclerView.Context));

            // provide 16dp of padding at the top of the list, 100 dp at the bottom
            DisplayMetrics metrics = new DisplayMetrics();
            IWindowManager windowManager = Context.GetSystemService(Context.WindowService).JavaCast<IWindowManager>();
            windowManager.DefaultDisplay.GetMetrics(metrics);

            int sixteenDPTop = (int)Math.Ceiling( 16 * metrics.Density);
            int hundredDPBottom = (int)Math.Ceiling(100 * metrics.Density);
            recyclerView.SetClipToPadding(false);
            recyclerView.SetPadding(0, sixteenDPTop, 0, hundredDPBottom);


            itemRecyclerViewAdapter = new ItemRecyclerViewAdapter(Activity, domain.Children, this);
            recyclerView.SetAdapter(itemRecyclerViewAdapter);
            if (dataObserver != null)
                itemRecyclerViewAdapter.RegisterAdapterDataObserver(dataObserver);
        }


        List<String> getRandomSublist(string[] array, int amount)
        {
            var list = new List<string>(amount);
            var random = new System.Random();
            while (list.Count < amount)
                list.Add(array[random.Next(array.Length)]);
            return list;
        }

        public class ItemRecyclerViewAdapter : RecyclerView.Adapter
        {
            protected Todo.TreeNodeList<Todo.Item> items;
            protected Android.App.Activity parent;
            protected ListFragment fragment;

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


            public ItemRecyclerViewAdapter(Android.App.Activity context, Todo.TreeNodeList<Todo.Item> items, ListFragment fragm)
            {
                parent = context;
                fragment = fragm;

                this.items = items ?? new Todo.TreeNodeList<Todo.Item>(fragment.domain);

                this.ItemClick += OnItemClick;
                //this.SubTaskClick += OnSubTaskClick;
                this.DeleteClick += OnDeleteClick;
                this.CompleteClick += OnCompleteClick;
                this.ShareClick += OnShareClick;
                this.ImageClick += OnImageClick;
            }

            public void ChangeDateSet(Todo.TreeNodeList<Todo.Item> items)
            { 
                this.items = items ?? new Todo.TreeNodeList<Todo.Item>(fragment.domain);
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
                var context = fragment.Context;

                new Android.Support.V7.App.AlertDialog.Builder(parent)
                .SetMessage("Share this item?")
                .SetCancelable(false)
                .SetPositiveButton("Yes", delegate
                {
                    Log.Debug("CheeseListFragment", string.Format("Shared item {0} and its subitems", item.Value.Name));

                    var intent = new Intent(context, typeof(SelectContactsActivity));
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

                var context = fragment.Context;//h.View.Context;
                var intent = new Intent(context, typeof(DetailActivity));
                intent.PutExtra(DetailActivity.EXTRA_NAME, item.Value.Name);
                intent.PutExtra(DetailActivity.ITEM_ID, item.Value.id);
                parent.StartActivityForResult(intent, ITEMDETAIL);
                //context.StartActivity(intent);
            }

            protected void OnImageClick(object sender, int e)
            {
                var adapter = sender as ItemRecyclerViewAdapter;
                var item = adapter.GetValueAt(e);

                var context = fragment.Context;

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
                    if(item.Value.Status == 7) // status is completed but not all the subitems have been completed
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
                        long timeInMilliseconds = (long) (TimeZoneInfo.ConvertTimeToUtc(item.Value.EndDate) -
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
                if(item.Value.ImagePath != null)
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
                else if(item.Value.ImageResource != 0)
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
    }
}

