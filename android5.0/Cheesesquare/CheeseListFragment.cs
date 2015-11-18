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
        Todo.Item domain;
        public CheeseListFragment (Todo.Item domain)
        {
            this.domain = domain;
        }

        public override View OnCreateView (LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            var v = inflater.Inflate (
                Resource.Layout.fragment_cheese_list, container, false);
            var rv = v.JavaCast<RecyclerView> ();

            setupRecyclerView(rv);

            return rv;
        }

        async void setupRecyclerView(RecyclerView recyclerView)
        {
            recyclerView.SetLayoutManager(new LinearLayoutManager(recyclerView.Context));
            var childItems = (List<Todo.Item>) await PublicFields.Database.GetChildItems(domain);
            recyclerView.SetAdapter(new ItemRecyclerViewAdapter(Activity, childItems));
        }

        List<String> getRandomSublist(string[] array, int amount) 
        {
            var list = new List<string> (amount);
            var random = new Random();
            while (list.Count < amount)
                list.Add (array[random.Next (array.Length)]);
            //list.Add ("Old Amsterdam");
            return list;
        }

        public class ItemRecyclerViewAdapter : RecyclerView.Adapter 
        {
            List<Todo.Item> items;
            Android.App.Activity parent;


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

                public List<Todo.Item> SubItems { get; set; }


                public ViewHolder (View view) : base (view) 
                {
                    View = view;
                    TextView = view.FindViewById<TextView> (Resource.Id.task_title);
                    ImageView = view.FindViewById<ImageView>(Resource.Id.imageView);
                    Importance = view.FindViewById<TextView>(Resource.Id.importance_task);
                    DueDate = view.FindViewById<TextView>(Resource.Id.due_date_task);
                    AmountOfSubTasks = view.FindViewById<TextView>(Resource.Id.amountOfSubTasks);
                    SubTasksLinearLayout = view.FindViewById<LinearLayout>(Resource.Id.subtasks_llayout);
                    MoreThanFiveSubtasks = view.FindViewById<TextView>(Resource.Id.more_than_five_subtasks_text);


                    SubItems = new List<Todo.Item>();
                    SubItems.Add(new Todo.Item { Name = "test" });
                    SubItems.Add(new Todo.Item { Name = "test2" });
                }

                public override string ToString () 
                {
                    return base.ToString () + " '" + TextView.Text;
                }
            }



            public Todo.Item GetValueAt (int position) 
            {
                return items[position];
            }

            public ItemRecyclerViewAdapter (Android.App.Activity context, List<Todo.Item> items) 
            {
                parent = context;

                this.items = items;
            }
            
            public override RecyclerView.ViewHolder OnCreateViewHolder (ViewGroup parent, int viewType) 
            {
                
                var view = LayoutInflater.From (parent.Context)
                    .Inflate(Resource.Layout.item_card_view, parent, false);

                var vh = new ViewHolder(view);
                
                return vh;
            }

            private void SubtaskCheckBox_CheckedChange(object sender, CompoundButton.CheckedChangeEventArgs e)
            {
                Log.Debug("CheeseListFragment", "checkbox clicked");
            }

            public override void OnBindViewHolder (RecyclerView.ViewHolder holder, int position) 
            {
                var h = holder as ViewHolder;

                h.View.Click += (sender, e) => {
                    var context = h.View.Context;
                    var intent = new Intent(context, typeof(CheeseDetailActivity));
                    intent.PutExtra(CheeseDetailActivity.EXTRA_NAME, items[position].Name);
                    intent.PutExtra("item", JsonConvert.SerializeObject(items[position]));
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

                if (h.SubItems == null || h.SubItems.Count == 0)
                {
                    h.SubTasksLinearLayout.Visibility = ViewStates.Visible;
                    h.AmountOfSubTasks.Text = string.Format("{0} subtasks", 0);
                }
                else
                {
                    foreach (Todo.Item subitem in h.SubItems)
                    {
                        LinearLayout subtaskView = (LinearLayout)LayoutInflater.From(h.SubTasksLinearLayout.Context).Inflate(Resource.Layout.subtask_line, h.SubTasksLinearLayout, false);
                        var subtaskName = subtaskView.FindViewById<TextView>(Resource.Id.subtask_name);
                        subtaskName.Text = subitem.Name;

                        subtaskName.Click += (sender, e) =>
                        {
                            var context = h.View.Context;
                            var intent = new Intent(context, typeof(CheeseDetailActivity));
                            intent.PutExtra(CheeseDetailActivity.EXTRA_NAME, subtaskName.Text);

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
                    h.AmountOfSubTasks.Text = string.Format("{0} subtasks", h.SubItems.Count.ToString());
                }

                h.TextView.Text = items[position].Name;
                h.Importance.Text = string.Format("{0} stars", items[position].Importance) ?? "0 stars";
                h.ImageView.SetImageDrawable(Cheeses.GetRandomCheeseDrawable(parent));
            }
                          
            public override int ItemCount {
                get { return items.Count; }
            }
        }
    }
}

