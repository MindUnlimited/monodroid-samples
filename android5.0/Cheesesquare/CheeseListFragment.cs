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

namespace Cheesesquare
{
    public class CheeseListFragment : Android.Support.V4.App.Fragment
    {
        public Todo.Item domain;
        private List<Todo.Item> childItems;
        public ItemRecyclerViewAdapter itemRecyclerViewAdapter;
        private const int ITEMDETAIL = 103;

        public CheeseListFragment(Todo.Item dom)
        {
            domain = dom;
            childItems = dom.SubItems;
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            var v = inflater.Inflate(
                Resource.Layout.fragment_cheese_list, container, false);
            var rv = v.JavaCast<RecyclerView>();

            setupRecyclerView(rv);

            return rv;
        }



        void setupRecyclerView(RecyclerView recyclerView)
        {
            recyclerView.SetLayoutManager(new LinearLayoutManager(recyclerView.Context));

            itemRecyclerViewAdapter = new ItemRecyclerViewAdapter(Activity, childItems, this);
            recyclerView.SetAdapter(itemRecyclerViewAdapter);
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
            private List<Todo.Item> items;
            private Android.App.Activity parent;
            private CheeseListFragment fragment;

            public class ViewHolder : RecyclerView.ViewHolder
            {
                public View View { get; set; }

                public TextView TextView { get; set; }
                public ImageView ImageView { get; set; }

                public TextView Importance { get; set; }
                public TextView DueDate { get; set; }

                public TextView AmountOfSubTasks { get; set; }
                public LinearLayout SubTasksLinearLayout { get; set; }
                public TextView MoreThanFiveSubtasks { get; set; }
                public TextView NoSubTasks { get; set; }

                public ImageButton Delete { get; set; }


                public ViewHolder(View view) : base(view)
                {
                    View = view;
                    TextView = view.FindViewById<TextView>(Resource.Id.task_title);
                    ImageView = view.FindViewById<ImageView>(Resource.Id.imageView);
                    Importance = view.FindViewById<TextView>(Resource.Id.importance_task);
                    DueDate = view.FindViewById<TextView>(Resource.Id.due_date_task);
                    AmountOfSubTasks = view.FindViewById<TextView>(Resource.Id.amountOfSubTasks);
                    SubTasksLinearLayout = view.FindViewById<LinearLayout>(Resource.Id.subtasks_llayout);
                    MoreThanFiveSubtasks = view.FindViewById<TextView>(Resource.Id.more_than_five_subtasks_text);
                    NoSubTasks = view.FindViewById<TextView>(Resource.Id.no_subtasks_text);
                    Delete = view.FindViewById<ImageButton>(Resource.Id.deleteButton);
                }

                public override string ToString()
                {
                    return base.ToString() + " '" + TextView.Text;
                }
            }

            public void UpdateValue(Todo.Item item)
            {
                int index = items.FindIndex(it => it.id == item.id);
                if (index >= 0)
                    items[index] = item;
            }

            public Todo.Item GetValueAt(int position)
            {
                return items[position];
            }

            public void ChangeValueAt(int position, Todo.Item item)
            {
                items[position] = item;
            }

            public void ApplyChanges()
            {
                fragment.childItems = items;
                NotifyDataSetChanged();
            }

            public void addItem(Todo.Item item)
            {
                items.Add(item);
            }


            public ItemRecyclerViewAdapter(Android.App.Activity context, List<Todo.Item> items, CheeseListFragment fragm)
            {
                parent = context;
                fragment = fragm;

                this.items = items ?? new List<Todo.Item>();
            }

            public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
            {

                var view = LayoutInflater.From(parent.Context)
                    .Inflate(Resource.Layout.item_card_view, parent, false);

                var vh = new ViewHolder(view);
                return vh;
            }

            private void SubtaskCheckBox_CheckedChange(object sender, CompoundButton.CheckedChangeEventArgs e)
            {
                Log.Debug("CheeseListFragment", "checkbox clicked");
            }

            public async void RecursiveDelete(Todo.Item item)
            {
                if (item == null)
                    return;

                foreach (var it in item.SubItems)
                {
                    RecursiveDelete(it);
                }

                // remove memory
                PublicFields.allItems.Remove(item);
                // remove locally as well
                items.Remove(item);

                // remove from db
                await PublicFields.Database.DeleteItem(item);
            }

            public override void OnBindViewHolder(RecyclerView.ViewHolder holder, int position)
            {
                var h = holder as ViewHolder;

                var item = items[position];

                h.View.Click += (sender, e) =>
                {
                    var context = h.View.Context;
                    var intent = new Intent(context, typeof(CheeseDetailActivity));
                    intent.PutExtra(CheeseDetailActivity.EXTRA_NAME, item.Name);
                    intent.PutExtra(CheeseDetailActivity.ITEM_ID, item.id);
                    parent.StartActivityForResult(intent, ITEMDETAIL);
                    //context.StartActivity(intent);
                };

                h.Delete.Click += (sender, e) =>
                {
                    new Android.Support.V7.App.AlertDialog.Builder(parent)
                        .SetMessage("Delete this item?")
                        .SetCancelable(false)
                        .SetPositiveButton("Yes", delegate
                        {
                            RecursiveDelete(item);

                            ApplyChanges();

                            Log.Debug("CheeseListFragment", string.Format("removed item {0} and its subitems", h.TextView.Text));
                        })
                       .SetNegativeButton("No", delegate
                       {
                           Log.Debug("CheeseListFragment", string.Format("Did not remove item {0}", h.TextView.Text));
                       })
                       .Show();
                };


                //long current_time_ms = Java.Lang.JavaSystem.CurrentTimeMillis();
                //DateTime time = System.DateTime.UtcNow.AddDays(5);
                //var long_time = time.ToFileTimeUtc();


                if (item.EndDate != null && item.EndDate != "")
                {
                    //String givenDateString = "Tue Apr 23 16:08:28 GMT+05:30 2013";
                    //SimpleDateFormat sdf = new SimpleDateFormat("dd-MM-yyyy");//new SimpleDateFormat("EEE MMM dd HH:mm:ss z yyyy");
                    try
                    {
                        //Date mDate = sdf.Parse(item.EndDate);
                        long timeInMilliseconds;
                        long.TryParse(item.EndDate, out timeInMilliseconds);
                        if (timeInMilliseconds > 0)
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



                //DateUtils.GetRelativeTimeSpanString(Application.Context, current_time_ms + 1000 * 60 * 60 * 24 * 2);

                //var importance = vh.View.FindViewById<TextView>(Resource.Id.importance_task);
                //Log.Debug("tag", vh.AdapterPosition.ToString());
                //importance.Text = string.Format("{0} stars", GetValueAt(vh.AdapterPosition).Importance) ?? "0 stars";

                int index = 1;

                if (item.SubItems == null || item.SubItems.Count == 0)
                {
                    h.NoSubTasks.Visibility = ViewStates.Visible;
                    h.AmountOfSubTasks.Text = string.Format("{0} subtasks", 0);
                }
                else
                {
                    // more items than the item signaling that there are no tasks and the item signaling
                    // that there are more than 5 tasks
                    while (h.SubTasksLinearLayout.ChildCount > 2) 
                        h.SubTasksLinearLayout.RemoveViewAt(1);
                    foreach (Todo.Item subitem in item.SubItems)
                    {
                        LinearLayout subtaskView = (LinearLayout)LayoutInflater.From(h.SubTasksLinearLayout.Context).Inflate(Resource.Layout.subtask_line, h.SubTasksLinearLayout, false);
                        var subtaskName = subtaskView.FindViewById<TextView>(Resource.Id.subtask_name);
                        subtaskName.Text = subitem.Name;

                        subtaskName.Click += (sender, e) =>
                        {
                            var context = h.View.Context;
                            var intent = new Intent(context, typeof(CheeseDetailActivity));
                            intent.PutExtra(CheeseDetailActivity.EXTRA_NAME, subtaskName.Text);
                            intent.PutExtra(CheeseDetailActivity.ITEM_ID, subitem.id);

                            parent.StartActivityForResult(intent, ITEMDETAIL);
                            //context.StartActivityForResult(intent);
                            //context.StartActivity(intent);
                        };

                        var subtaskCheckBox = subtaskView.FindViewById<CheckBox>(Resource.Id.checkbox);
                        subtaskCheckBox.CheckedChange += SubtaskCheckBox_CheckedChange;

                        h.SubTasksLinearLayout.AddView(subtaskView, index);

                        index++;

                        if (index > 5) // 5 items displayed as maximum
                        {
                            h.MoreThanFiveSubtasks.Visibility = ViewStates.Visible;
                            break;
                        }
                    }
                    h.AmountOfSubTasks.Text = string.Format("{0} subtasks", item.SubItems.Count.ToString());
                }

                h.TextView.Text = item.Name;
                h.Importance.Text = string.Format("{0} stars", item.Importance) ?? "0 stars";
                h.ImageView.SetImageDrawable(Cheeses.GetRandomCheeseDrawable(parent));
            }

            public override int ItemCount
            {
                get { return items.Count; }
            }
        }
    }
}

