using System;
using Android.OS;
using Android.Support.V7.App;
using Android.Support.V7.Widget;
using Android.Views;
using Android.Widget;
using V7Toolbar = Android.Support.V7.Widget.Toolbar;
using Android.Support.Design.Widget;
using Android.App;
using Android.Util;
using Android.Content;

namespace Cheesesquare
{
    [Activity (Label="Details")]
    public class CheeseDetailActivity : AppCompatActivity
    {
        public const string EXTRA_NAME = "cheese_name";
        private const string TAG = "CheeseActivity";

        bool editmode;

        TextView txtDate;
        String cheeseName;
        LinearLayout titleDescriptionLayout;
        SeekBar progressSlider;
        TextView progressPercentText;
        FloatingActionButton editFAB;
        FloatingActionButton addItemFAB;
        CollapsingToolbarLayout collapsingToolbar;

        protected override void OnCreate (Bundle savedInstanceState) 
        {
            base.OnCreate (savedInstanceState);

            SetContentView(Resource.Layout.activity_detail);

            cheeseName = Intent.GetStringExtra (EXTRA_NAME);

            var toolbar = FindViewById<V7Toolbar>(Resource.Id.toolbar);
            SetSupportActionBar (toolbar);
            SupportActionBar.SetDisplayHomeAsUpEnabled (true);

            collapsingToolbar = FindViewById<CollapsingToolbarLayout> (Resource.Id.collapsing_toolbar);
            collapsingToolbar.SetTitle (cheeseName);
                
            //var title = FindViewById<EditText>(Resource.Id.txt_title);
            //title.SetText(cheeseName,TextView.BufferType.Editable);

            //titleDescriptionLayout = FindViewById<LinearLayout>(Resource.Id.title_description_layout);

            progressSlider = FindViewById<SeekBar>(Resource.Id.progressSlider);
            progressPercentText = FindViewById<TextView>(Resource.Id.progressPercentText);
            progressSlider.ProgressChanged += ProgressSlider_ProgressChanged;

            editFAB = FindViewById<FloatingActionButton>(Resource.Id.edit_fab);
            editFAB.Click += EditFAB_Click;

            addItemFAB = FindViewById<FloatingActionButton>(Resource.Id.add_task_fab);
            addItemFAB.Click += AddItemFAB_Click;

            editmode = false;

            CardView card1 = FindViewById<CardView>(Resource.Id.detail_card_1);
            card1.Click += (sender, e) => {
                var context = card1.Context;
                var intent = new Intent(context, typeof(CheeseDetailActivity));

                var taskTitle = card1.FindViewById<TextView>(Resource.Id.task_title);
                intent.PutExtra(CheeseDetailActivity.EXTRA_NAME, taskTitle.Text);

                context.StartActivity(intent);
            };

            var titleSubTask1 = card1.FindViewById<TextView>(Resource.Id.subTask1);
            titleSubTask1.Click += (sender, e) =>
            {
                var context = this;
                var intent = new Intent(context, typeof(CheeseDetailActivity));
                intent.PutExtra(CheeseDetailActivity.EXTRA_NAME, titleSubTask1.Text);

                context.StartActivity(intent);
            };
            var titleSubTask2 = card1.FindViewById<TextView>(Resource.Id.subTask2);
            titleSubTask2.Click += (sender, e) =>
            {
                var context = this;
                var intent = new Intent(context, typeof(CheeseDetailActivity));
                intent.PutExtra(CheeseDetailActivity.EXTRA_NAME, titleSubTask2.Text);

                context.StartActivity(intent);
            };

            CardView card2 = FindViewById<CardView>(Resource.Id.detail_card_2);

            card2.Click += (sender, e) => {
                var context = card2.Context;
                var intent = new Intent(context, typeof(CheeseDetailActivity));

                var taskTitle = card2.FindViewById<TextView>(Resource.Id.task_title);
                intent.PutExtra(CheeseDetailActivity.EXTRA_NAME, taskTitle.Text);

                context.StartActivity(intent);
            };

            var titleSubTask1c2 = card2.FindViewById<TextView>(Resource.Id.subTask1);
            titleSubTask1c2.Click += (sender, e) =>
            {
                var context = this;
                var intent = new Intent(context, typeof(CheeseDetailActivity));
                intent.PutExtra(CheeseDetailActivity.EXTRA_NAME, titleSubTask1.Text);

                context.StartActivity(intent);
            };
            var titleSubTask2c2 = card2.FindViewById<TextView>(Resource.Id.subTask2);
            titleSubTask2c2.Click += (sender, e) =>
            {
                var context = this;
                var intent = new Intent(context, typeof(CheeseDetailActivity));
                intent.PutExtra(CheeseDetailActivity.EXTRA_NAME, titleSubTask2.Text);

                context.StartActivity(intent);
            };

            loadBackdrop();
        }

        private void AddItemFAB_Click(object sender, EventArgs e)
        {
            var intent = new Intent(this, typeof(EditItemActivity));
            StartActivity(intent);
        }

        private void EditFAB_Click(object sender, EventArgs e)
        {
            Log.Info(TAG, "edit fab clicked!");

            //if (editmode) // saving edit
            //{
            //    editmode = false;
            //    titleDescriptionLayout.Visibility = ViewStates.Gone;
            //    editFAB.SetImageResource(Resource.Drawable.ic_mode_edit_white_24dp);

            //    var title = FindViewById<EditText>(Resource.Id.txt_title);
            //    collapsingToolbar.SetTitle(title.Text);
            //}
            //else // going into edit mode
            //{
            //    editmode = true;
            //    titleDescriptionLayout.Visibility = ViewStates.Visible;
            //    editFAB.SetImageResource(Resource.Drawable.ic_done);
            //    collapsingToolbar.SetTitle("");
            //}


            var intent = new Intent(this, typeof(EditItemActivity));
            intent.PutExtra(EditItemActivity.EXTRA_NAME, cheeseName);
            StartActivity(intent);
        }

        private void ProgressSlider_ProgressChanged(object sender, SeekBar.ProgressChangedEventArgs e)
        {
            progressPercentText.Text = string.Format("{0}%", progressSlider.Progress * 25);
        }

        protected override void OnStart()
        {
            base.OnStart();
            txtDate = (TextView)FindViewById(Resource.Id.txtdate);
        }


        public override bool OnOptionsItemSelected (IMenuItem item) 
        {
            switch (item.ItemId) {
            case Android.Resource.Id.Home:
                Finish ();
                return true;
            }
            return base.OnOptionsItemSelected (item);
        }

        void loadBackdrop() 
        {            
            var imageView = FindViewById<ImageView> (Resource.Id.backdrop);

            var r = Cheeses.GetRandomCheeseBackground ();
            imageView.SetImageResource (r);
        }
            
        public override bool OnCreateOptionsMenu(IMenu menu) 
        {
            MenuInflater.Inflate (Resource.Menu.sample_actions, menu);
            return true;
        }
    }
}

