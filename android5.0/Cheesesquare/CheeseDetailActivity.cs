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

namespace Cheesesquare
{
    [Activity (Label="Details")]
    public class CheeseDetailActivity : AppCompatActivity
    {
        public const string EXTRA_NAME = "cheese_name";
        private const string TAG = "CheeseActivity";

        bool editmode;

        LinearLayout titleDescriptionLayout;
        SeekBar progressSlider;
        TextView progressPercentText;
        FloatingActionButton editFAB;
        CollapsingToolbarLayout collapsingToolbar;

        protected override void OnCreate (Bundle savedInstanceState) 
        {
            base.OnCreate (savedInstanceState);

            SetContentView(Resource.Layout.activity_detail);

            var cheeseName = Intent.GetStringExtra (EXTRA_NAME);

            var toolbar = FindViewById<V7Toolbar>(Resource.Id.toolbar);
            SetSupportActionBar (toolbar);
            SupportActionBar.SetDisplayHomeAsUpEnabled (true);

            collapsingToolbar = FindViewById<CollapsingToolbarLayout> (Resource.Id.collapsing_toolbar);
            collapsingToolbar.SetTitle (cheeseName);

            var title = FindViewById<EditText>(Resource.Id.txt_title);
            title.SetText(cheeseName,TextView.BufferType.Editable);

            titleDescriptionLayout = FindViewById<LinearLayout>(Resource.Id.title_description_layout);

            progressSlider = FindViewById<SeekBar>(Resource.Id.progressSlider);
            progressPercentText = FindViewById<TextView>(Resource.Id.progressPercentText);
            progressSlider.ProgressChanged += ProgressSlider_ProgressChanged;

            editFAB = FindViewById<FloatingActionButton>(Resource.Id.edit_fab);
            editFAB.Click += EditFAB_Click;

            editmode = false;

            loadBackdrop();
        }

        private void EditFAB_Click(object sender, EventArgs e)
        {
            Log.Info(TAG, "edit fab clicked!");

            if (editmode) // saving edit
            {
                editmode = false;
                titleDescriptionLayout.Visibility = ViewStates.Gone;
                editFAB.SetImageResource(Resource.Drawable.ic_mode_edit_white_24dp);

                var title = FindViewById<EditText>(Resource.Id.txt_title);
                collapsingToolbar.SetTitle(title.Text);
            }
            else // going into edit mode
            {
                editmode = true;
                titleDescriptionLayout.Visibility = ViewStates.Visible;
                editFAB.SetImageResource(Resource.Drawable.ic_done);
                collapsingToolbar.SetTitle("");
            }


        }

        private void ProgressSlider_ProgressChanged(object sender, SeekBar.ProgressChangedEventArgs e)
        {
            progressPercentText.Text = string.Format("{0}%", progressSlider.Progress * 25);
        }

        protected override void OnStart()
        {
            base.OnStart();
            EditText txtDate = (EditText)FindViewById(Resource.Id.txtdate);
            txtDate.FocusChange += TxtDate_FocusChange;
        }

        private void TxtDate_FocusChange(object sender, View.FocusChangeEventArgs e)
        {
            var editText = (EditText)sender;
            if(e.HasFocus)
            {
                DateDialog dialog = new DateDialog((View)sender);
                FragmentTransaction ft = FragmentManager.BeginTransaction();
                dialog.Show(ft, "DatePicker");
            }
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

