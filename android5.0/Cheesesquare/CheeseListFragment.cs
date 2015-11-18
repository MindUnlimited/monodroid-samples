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

namespace Cheesesquare
{
    public class CheeseListFragment : Android.Support.V4.App.Fragment
    {
        //private Todo.Item domain;
        private List<Todo.Item> childItems;

        //public CheeseListFragment(Todo.Item domain)
        //{
        //    this.domain = domain;
        //}

        public CheeseListFragment(List<Todo.Item> items)
        {
            childItems = items;
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
            //if (this.childItems == null || this.childItems.Count == 0)
            //{
                recyclerView.SetLayoutManager(new LinearLayoutManager(recyclerView.Context));

            //    childItems = (List<Todo.Item>)await PublicFields.Database.GetChildItems(domain);

            //    foreach (Todo.Item it in childItems)
            //    {
            //        it.SubItems = (List<Todo.Item>)await PublicFields.Database.GetChildItems(it);
            //        AddSubTasks(it.SubItems);
            //    }
            //}
            recyclerView.SetAdapter(new ItemRecyclerViewAdapter(Activity, childItems));
        }


        List<String> getRandomSublist(string[] array, int amount)
        {
            var list = new List<string>(amount);
            var random = new Random();
            while (list.Count < amount)
                list.Add(array[random.Next(array.Length)]);
            return list;
        }

        public class ItemRecyclerViewAdapter : RecyclerView.Adapter
        {
            private List<Todo.Item> items;
            private Android.App.Activity parent;


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


                //public List<Todo.Item> SubItems { get; set; }


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
                }

                public override string ToString()
                {
                    return base.ToString() + " '" + TextView.Text;
                }
            }

            public Todo.Item GetValueAt(int position)
            {
                return items[position];
            }

            public ItemRecyclerViewAdapter(Android.App.Activity context, List<Todo.Item> items)
            {
                parent = context;

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

            public override void OnBindViewHolder(RecyclerView.ViewHolder holder, int position)
            {
                var h = holder as ViewHolder;

                h.View.Click += (sender, e) =>
                {
                    var context = h.View.Context;
                    var intent = new Intent(context, typeof(CheeseDetailActivity));
                    intent.PutExtra(CheeseDetailActivity.EXTRA_NAME, items[position].Name);
                    intent.PutExtra(CheeseDetailActivity.ITEM_ID, items[position].ID);
                    context.StartActivity(intent);
                };


                long current_time_ms = Java.Lang.JavaSystem.CurrentTimeMillis();
                DateTime time = System.DateTime.UtcNow.AddDays(5);
                var long_time = time.ToFileTimeUtc();
                h.DueDate.Text =
                    DateUtils.GetRelativeTimeSpanString(Application.Context, current_time_ms + 1000 * 60 * 60 * 24 * 2);

                //var importance = vh.View.FindViewById<TextView>(Resource.Id.importance_task);
                //Log.Debug("tag", vh.AdapterPosition.ToString());
                //importance.Text = string.Format("{0} stars", GetValueAt(vh.AdapterPosition).Importance) ?? "0 stars";

                int index = 1;

                if (items[position].SubItems == null || items[position].SubItems.Count == 0)
                {
                    h.NoSubTasks.Visibility = ViewStates.Visible;
                    h.AmountOfSubTasks.Text = string.Format("{0} subtasks", 0);
                }
                else
                {
                    foreach (Todo.Item subitem in items[position].SubItems)
                    {
                        LinearLayout subtaskView = (LinearLayout)LayoutInflater.From(h.SubTasksLinearLayout.Context).Inflate(Resource.Layout.subtask_line, h.SubTasksLinearLayout, false);
                        var subtaskName = subtaskView.FindViewById<TextView>(Resource.Id.subtask_name);
                        subtaskName.Text = subitem.Name;

                        subtaskName.Click += (sender, e) =>
                        {
                            var context = h.View.Context;
                            var intent = new Intent(context, typeof(CheeseDetailActivity));
                            intent.PutExtra(CheeseDetailActivity.EXTRA_NAME, subtaskName.Text);
                            intent.PutExtra(CheeseDetailActivity.ITEM_ID, items[position].ID);

                            context.StartActivity(intent);
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
                    h.AmountOfSubTasks.Text = string.Format("{0} subtasks", items[position].SubItems.Count.ToString());
                }

                h.TextView.Text = items[position].Name;
                h.Importance.Text = string.Format("{0} stars", items[position].Importance) ?? "0 stars";
                h.ImageView.SetImageDrawable(Cheeses.GetRandomCheeseDrawable(parent));
            }

            public override int ItemCount
            {
                get { return items.Count; }
            }
        }
    }
}

