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

namespace Cheesesquare
{
    public class CheeseListFragment : Android.Support.V4.App.Fragment
    {
        public CheeseListFragment ()
        {
        }

        public override View OnCreateView (LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            var v = inflater.Inflate (
                Resource.Layout.fragment_cheese_list, container, false);
            var rv = v.JavaCast<RecyclerView> ();

            setupRecyclerView(rv);

            return rv;
        }

        void setupRecyclerView (RecyclerView recyclerView) 
        {
            recyclerView.SetLayoutManager(new LinearLayoutManager(recyclerView.Context));
            recyclerView.SetAdapter (new SimpleStringRecyclerViewAdapter (Activity,
                getRandomSublist(Cheeses.CheeseList, 25)));
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

        public class SimpleStringRecyclerViewAdapter : RecyclerView.Adapter 
        {
            List<String> values;
            Android.App.Activity parent;

            public class ViewHolder : RecyclerView.ViewHolder 
            {
                public View View { get;set; }
                public TextView TextView { get; set; }
                public ImageView ImageView { get; set; }

                public ViewHolder (View view) : base (view) 
                {
                    View = view;
                    TextView = view.FindViewById<TextView> (Resource.Id.task_title);
                    ImageView = view.FindViewById<ImageView>(Resource.Id.imageView);
                }

                public override string ToString () 
                {
                    return base.ToString () + " '" + TextView.Text;
                }
            }



            public String GetValueAt (int position) 
            {
                return values[position];
            }

            public SimpleStringRecyclerViewAdapter (Android.App.Activity context, List<String> items) 
            {
                parent = context;

                values = items;
            }

            public override RecyclerView.ViewHolder OnCreateViewHolder (ViewGroup parent, int viewType) 
            {
                
                var view = LayoutInflater.From (parent.Context)
                    .Inflate(Resource.Layout.item_card_view, parent, false);

                return new ViewHolder(view);
            }

            public override void OnBindViewHolder (RecyclerView.ViewHolder holder, int position) 
            {
                var h = holder as ViewHolder;


                h.View.Click += (sender, e) => {
                    var context = h.View.Context;
                    var intent = new Intent(context, typeof(CheeseDetailActivity));
                    intent.PutExtra(CheeseDetailActivity.EXTRA_NAME, values[position]);

                    context.StartActivity(intent);
                };

                var titleSubTask1 = h.View.FindViewById<TextView>(Resource.Id.subTask1);
                titleSubTask1.Click += (sender, e) =>
                {
                    var context = h.View.Context;
                    var intent = new Intent(context, typeof(CheeseDetailActivity));
                    intent.PutExtra(CheeseDetailActivity.EXTRA_NAME, titleSubTask1.Text);

                    context.StartActivity(intent);
                };

                var titleSubTask2 = h.View.FindViewById<TextView>(Resource.Id.subTask2);
                titleSubTask2.Click += (sender, e) =>
                {
                    var context = h.View.Context;
                    var intent = new Intent(context, typeof(CheeseDetailActivity));
                    intent.PutExtra(CheeseDetailActivity.EXTRA_NAME, titleSubTask2.Text);

                    context.StartActivity(intent);
                };

                h.TextView.Text = values [position];
                h.ImageView.SetImageDrawable(Cheeses.GetRandomCheeseDrawable(parent));
            }
                          
            public override int ItemCount {
                get { return values.Count; }
            }
        }
    }
}

