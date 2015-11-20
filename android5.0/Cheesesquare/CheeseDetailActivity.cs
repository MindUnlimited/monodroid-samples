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
        private const int ITEMDETAIL = 103;
        private const int EDIT_ITEM = 104;
        private const string TAG = "CheeseActivity";

        //bool editmode;

        private TextView txtDate;
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
        private bool itemChanged;

        protected override void OnCreate (Bundle savedInstanceState) 
        {
            base.OnCreate (savedInstanceState);
            SetContentView(Resource.Layout.activity_detail);

            itemChanged = false;

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
            collapsingToolbar.SetTitle (item.Name);

            txtDate = FindViewById<TextView>(Resource.Id.txtdate);
            txtDate.Text = item.EndDate;

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
            itemChanged = true;
        }

        class MyAdapter : Android.Support.V4.App.FragmentPagerAdapter
        {
            List<V4Fragment> fragments = new List<V4Fragment>();
            List<string> fragmentTitles = new List<string>();

            public MyAdapter(V4FragmentManager fm) : base(fm)
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
            var adapter = new MyAdapter(SupportFragmentManager);

            adapter.AddFragment(new CheeseListFragment(item.SubItems), item.Name);
            viewPager.Adapter = adapter;
        }

        protected override void OnActivityResult(int requestCode, Result resultCode,
Intent intent)
        {
            base.OnActivityResult(requestCode, resultCode, intent);

            if (resultCode == Result.Ok)
            {
                switch (requestCode)
                {
                    case
                        ITEMDETAIL:
                        if (intent.GetBooleanExtra("itemChanged", false))
                        {
                            string ItemID = intent.GetStringExtra("itemID");
                            Log.Debug("MainActivity", "Item changed");
                            int index = viewPager.CurrentItem;
                            var adapter = (MyAdapter)viewPager.Adapter;
                            var currentFragment = (CheeseListFragment)adapter.GetItem(index);
                            var fragmentAdapter = currentFragment.itemRecyclerViewAdapter;

                            var item = PublicFields.allItems.Find(it => it.ID == ItemID);

                            fragmentAdapter.UpdateValue(item);
                            fragmentAdapter.ApplyChanges();
                        }

                        break;
                    case
                        EDIT_ITEM:
                        var itemJSON = intent.GetBooleanExtra("edited", false);
                        if (itemJSON)
                        {
                            itemChanged = true;
                            item = PublicFields.allItems.Find(it => it.ID == item.ID);
                            //Refresh?
                            //ViewGroup drawerLayout = FindViewById<ViewGroup>(Resource.Id.main_content);
                            //drawerLayout.Invalidate();

                            //Finish();
                            //var updateIntent = new Intent();
                            //updateIntent.PutExtra(itemID, item.ID);
                            //StartActivity(updateIntent);
                        }
                        break;
                    default:
                        break;
                }
            }
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
            var intent = new Intent(this, typeof(EditItemActivity));
            intent.PutExtra("itemID", item.ID);
            StartActivityForResult(intent, EDIT_ITEM);
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

        public override void Finish()
        {
            Intent returnIntent = new Intent();
            returnIntent.PutExtra("itemChanged", itemChanged);// ("passed_item", itemYouJustCreated);
            if (itemChanged)
                returnIntent.PutExtra("itemID", item.ID);// ("passed_item", itemYouJustCreated);
            SetResult(Result.Ok, returnIntent);
            base.Finish();
        }


        void loadBackdrop() 
        {            
            var imageView = FindViewById<ImageView> (Resource.Id.backdrop);

            var r = Cheeses.GetRandomCheeseBackground ();
            imageView.SetImageResource (r);
        }
    }
}

