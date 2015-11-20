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

namespace Cheesesquare
{
    [Activity (Label="Details")]
    [MetaData("android.support.PARENT_ACTIVITY", Value = "com.sample.cheesesquare.MainActivity")]
    public class CheeseDetailActivity : AppCompatActivity
    {
        public const string EXTRA_NAME = "cheese_name";
        public const string ITEM_ID = "item_id";
        private const string TAG = "CheeseActivity";

        //bool editmode;

        private TextView txtDate;
        private String cheeseName;
        private String itemID;
        //private LinearLayout titleDescriptionLayout;

        private RatingBar importance;

        private SeekBar progressSlider;
        private TextView progressPercentText;

        private FloatingActionButton editFAB;
        private FloatingActionButton addItemFAB;
        private CollapsingToolbarLayout collapsingToolbar;
        private DrawerLayout drawerLayout;
        private ViewPager viewPager;
        private Todo.Item item;


        protected override void OnCreate (Bundle savedInstanceState) 
        {
            base.OnCreate (savedInstanceState);
            SetContentView(Resource.Layout.activity_detail);
            cheeseName = Intent.GetStringExtra (EXTRA_NAME);

            if (Parent == null)
                Log.Debug("CheeseDetailAcitivity", "Parent not found");

            itemID = Intent.GetStringExtra(ITEM_ID);
            item = PublicFields.allItems.Find(it => it.ID == itemID);

            drawerLayout = FindViewById<DrawerLayout>(Resource.Id.drawer_layout_detail);

            var toolbar = FindViewById<V7Toolbar>(Resource.Id.toolbar);
            SetSupportActionBar (toolbar);

            var navigationView = drawerLayout.FindViewById<NavigationView>(Resource.Id.nav_view_detail);
            if (navigationView != null)
                setupDrawerContent(navigationView);

            SupportActionBar.SetHomeAsUpIndicator(Resource.Drawable.ic_menu);
            SupportActionBar.SetDisplayHomeAsUpEnabled(true);

            collapsingToolbar = FindViewById<CollapsingToolbarLayout> (Resource.Id.collapsing_toolbar);
            collapsingToolbar.SetTitle (cheeseName);

            progressSlider = FindViewById<SeekBar>(Resource.Id.progressSlider);
            progressPercentText = FindViewById<TextView>(Resource.Id.progressPercentText);
            progressSlider.ProgressChanged += ProgressSlider_ProgressChanged;

            importance = FindViewById<RatingBar>(Resource.Id.ratingbar);
            importance.RatingBarChange += Importance_RatingBarChange;
            importance.Rating = item.Importance;

            editFAB = FindViewById<FloatingActionButton>(Resource.Id.edit_fab);
            editFAB.Click += EditFAB_Click;

            addItemFAB = FindViewById<FloatingActionButton>(Resource.Id.add_task_fab);
            addItemFAB.Click += AddItemFAB_Click;

            //CardView card1 = FindViewById<CardView>(Resource.Id.detail_card_1);
            //card1.Click += (sender, e) => {
            //    var context = card1.Context;
            //    var intent = new Intent(context, typeof(CheeseDetailActivity));

            //    var taskTitle = card1.FindViewById<TextView>(Resource.Id.task_title);
            //    intent.PutExtra(CheeseDetailActivity.EXTRA_NAME, taskTitle.Text);

            //    context.StartActivity(intent);
            //};

            //CardView card2 = FindViewById<CardView>(Resource.Id.detail_card_2);
            //card2.Click += (sender, e) => {
            //    var context = card2.Context;
            //    var intent = new Intent(context, typeof(CheeseDetailActivity));

            //    var taskTitle = card2.FindViewById<TextView>(Resource.Id.task_title);
            //    intent.PutExtra(CheeseDetailActivity.EXTRA_NAME, taskTitle.Text);

            //    context.StartActivity(intent);
            //};

            viewPager = FindViewById<Android.Support.V4.View.ViewPager>(Resource.Id.viewpager_cards_detail);
            setupViewPager(viewPager);

            loadBackdrop();
        }

        private void Importance_RatingBarChange(object sender, RatingBar.RatingBarChangeEventArgs e)
        {
            var ratingbar = (RatingBar) sender;
            int rating = (int)ratingbar.Rating;

            item.Importance = rating;
            PublicFields.allItems.Where(it => it.ID == item.ID).FirstOrDefault().Importance = rating;
        }

        class Adapter : Android.Support.V4.App.FragmentPagerAdapter
        {
            List<V4Fragment> fragments = new List<V4Fragment>();
            List<string> fragmentTitles = new List<string>();

            public Adapter(V4FragmentManager fm) : base(fm)
            {
            }

            public void AddFragment(V4Fragment fragment, String title)
            {
                fragments.Add(fragment);
                fragmentTitles.Add(title);
            }

            public override V4Fragment GetItem(int position)
            {
                return fragments[position];
            }

            public override int Count
            {
                get { return fragments.Count; }
            }

            public override Java.Lang.ICharSequence GetPageTitleFormatted(int position)
            {
                return new Java.Lang.String(fragmentTitles[position]);
            }

        }

        void setupViewPager(Android.Support.V4.View.ViewPager viewPager)
        {
            var adapter = new Adapter(SupportFragmentManager);

            adapter.AddFragment(new CheeseListFragment(item.SubItems), item.Name);
            viewPager.Adapter = adapter;
        }

        void setupDrawerContent(NavigationView navigationView)
        {
            navigationView.NavigationItemSelected += (sender, e) => {
                switch (e.MenuItem.ItemId)
                {
                    case Resource.Id.nav_home:
                        //Intent intent = new Intent(this, typeof(MainActivity));
                        //intent.AddFlags(ActivityFlags.ClearTop);
                        //StartActivity(intent);
                        //NavigateUpTo(ParentActivityIntent);
                        NavUtils.NavigateUpFromSameTask(this);
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
            //txtDate = (TextView)FindViewById(Resource.Id.txtdate);

        }


        void loadBackdrop() 
        {            
            var imageView = FindViewById<ImageView> (Resource.Id.backdrop);

            var r = Cheeses.GetRandomCheeseBackground ();
            imageView.SetImageResource (r);
        }
    }
}

