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
                public List<Todo.Item> SubItems { get; set; }
                public Todo.Item item {get;set;}

                public ViewHolder (View view) : base (view) 
                {
                    View = view;
                    TextView = view.FindViewById<TextView> (Resource.Id.task_title);
                    ImageView = view.FindViewById<ImageView>(Resource.Id.imageView);

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
                var subtasksLinearLayout = vh.View.FindViewById<LinearLayout>(Resource.Id.subtasks_llayout);
                var amountOfSubTasks = vh.View.FindViewById<TextView>(Resource.Id.amountOfSubTasks);


                var dueDate = vh.View.FindViewById<TextView>(Resource.Id.due_date_task);

                long current_time_ms = Java.Lang.JavaSystem.CurrentTimeMillis();
                DateTime time = System.DateTime.UtcNow.AddDays(5);
                var long_time = time.ToFileTimeUtc();
                dueDate.Text =
                    DateUtils.GetRelativeTimeSpanString(parent.Context, current_time_ms + 1000 * 60 * 60* 24 * 2);

                var importance = vh.View.FindViewById<TextView>(Resource.Id.importance_task);
                Log.Debug("tag", vh.AdapterPosition.ToString());
                importance.Text = string.Format("{0} stars", GetValueAt(vh.AdapterPosition).Importance) ?? "0 stars";

                int index = 1;

                if (vh.SubItems == null || vh.SubItems.Count == 0)
                {
                    var noSubtasksText = subtasksLinearLayout.FindViewById<TextView>(Resource.Id.no_subtasks_text);
                    noSubtasksText.Visibility = ViewStates.Visible;
                    amountOfSubTasks.Text = string.Format("{0} subtasks", 0);
                }
                else
                {
                    foreach (Todo.Item subitem in vh.SubItems)
                    {
                        LinearLayout subtaskView = (LinearLayout)LayoutInflater.From(subtasksLinearLayout.Context).Inflate(Resource.Layout.subtask_line, subtasksLinearLayout, false);
                        var subtaskName = subtaskView.FindViewById<TextView>(Resource.Id.subtask_name);
                        subtaskName.Text = subitem.Name;

                        subtaskName.Click += (sender, e) =>
                        {
                            var context = vh.View.Context;
                            var intent = new Intent(context, typeof(CheeseDetailActivity));
                            intent.PutExtra(CheeseDetailActivity.EXTRA_NAME, subtaskName.Text);

                            context.StartActivity(intent);
                        };

                        var subtaskCheckBox = subtaskView.FindViewById<CheckBox>(Resource.Id.checkbox);
                        subtaskCheckBox.CheckedChange += SubtaskCheckBox_CheckedChange;

                        subtasksLinearLayout.AddView(subtaskView, index);

                        index++;

                        if (index > 5) // 5 items displayed as maximum
                        {
                            var moreThanFiveSubtasksText = view.FindViewById<TextView>(Resource.Id.more_than_five_subtasks_text);
                            moreThanFiveSubtasksText.Visibility = ViewStates.Visible;
                            break;
                        }
                    }
                    amountOfSubTasks.Text = string.Format("{0} subtasks", vh.SubItems.Count.ToString());
                }

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

                

                //var titleSubTask1 = h.View.FindViewById<TextView>(Resource.Id.subTask1);
                //titleSubTask1.Click += (sender, e) =>
                //{
                //    var context = h.View.Context;
                //    var intent = new Intent(context, typeof(CheeseDetailActivity));
                //    intent.PutExtra(CheeseDetailActivity.EXTRA_NAME, titleSubTask1.Text);

                //    context.StartActivity(intent);
                //};

                //var titleSubTask2 = h.View.FindViewById<TextView>(Resource.Id.subTask2);
                //titleSubTask2.Click += (sender, e) =>
                //{
                //    var context = h.View.Context;
                //    var intent = new Intent(context, typeof(CheeseDetailActivity));
                //    intent.PutExtra(CheeseDetailActivity.EXTRA_NAME, titleSubTask2.Text);

                //    context.StartActivity(intent);
                //};

                h.TextView.Text = items [position].Name;
                h.ImageView.SetImageDrawable(Cheeses.GetRandomCheeseDrawable(parent));
            }
                          
            public override int ItemCount {
                get { return items.Count; }
            }
        }
    }
}

