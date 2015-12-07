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
using Android.Text.Format;
using Android.Net;

namespace Cheesesquare
{
    public class DataObserver : RecyclerView.AdapterDataObserver
    {
        private CheeseDetailActivity detailActivity;

        public DataObserver(CheeseDetailActivity activity) : base()
        {
            detailActivity = activity;
        }

        public override void OnChanged()
        {
            base.OnChanged();
            detailActivity.itemChanged = true;
            //CheckAdapterIsEmpty();
        }
        //public override void OnItemRangeChanged(int positionStart, int itemCount)
        //{

        //}

        //public override void OnItemRangeChanged(int positionStart, int itemCount, Java.Lang.Object payload)
        //{

        //}
        //public override void OnItemRangeInserted(int positionStart, int itemCount)
        //{

        //}
        //public override void OnItemRangeMoved(int fromPosition, int toPosition, int itemCount)
        //{

        //}
        //public override void OnItemRangeRemoved(int positionStart, int itemCount)
        //{

        //}
    }

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

        private TextView comment;

        private FloatingActionButton editFAB;
        private FloatingActionButton addItemFAB;
        private CollapsingToolbarLayout collapsingToolbar;
        private DrawerLayout drawerLayout;
        private ViewPager viewPager;

        private Todo.Item item;
        public bool itemChanged;

        private RecyclerView.AdapterDataObserver dataObserver;// = new DataObserver();

    protected override void OnCreate (Bundle savedInstanceState) 
        {
            base.OnCreate (savedInstanceState);
            SetContentView(Resource.Layout.activity_detail);

            dataObserver = new DataObserver(this);
            itemChanged = false;

            itemID = Intent.GetStringExtra(ITEM_ID);
            item = PublicFields.allItems.Find(it => it.id == itemID);

            drawerLayout = FindViewById<DrawerLayout>(Resource.Id.drawer_layout_detail);

            var toolbar = FindViewById<V7Toolbar>(Resource.Id.toolbar);
            SetSupportActionBar (toolbar);

            var navigationView = drawerLayout.FindViewById<NavigationView>(Resource.Id.nav_view_detail);
            if (navigationView != null)
                setupDrawerContent(navigationView);

            var username = navigationView.FindViewById<TextView>(Resource.Id.username_nav);
            username.Text = PublicFields.Database.userName;

            SupportActionBar.SetHomeAsUpIndicator(Resource.Drawable.ic_menu);
            SupportActionBar.SetDisplayHomeAsUpEnabled(true);

            collapsingToolbar = FindViewById<CollapsingToolbarLayout> (Resource.Id.collapsing_toolbar);
            collapsingToolbar.SetTitle (item.Name);

            txtDate = FindViewById<TextView>(Resource.Id.txtdate);
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
                        txtDate.Text = DateUtils.GetRelativeTimeSpanString(Application.Context, timeInMilliseconds);
                }
                catch (ParseException e)
                {
                    e.PrintStackTrace();
                }
            }
            else
            {
                txtDate.Text = "No due date";
            }

            progressSlider = FindViewById<SeekBar>(Resource.Id.progressSlider);
            progressPercentText = FindViewById<TextView>(Resource.Id.progressPercentText);
            progressSlider.ProgressChanged += ProgressSlider_ProgressChanged;

            importance = FindViewById<RatingBar>(Resource.Id.ratingbar);
            importance.RatingBarChange += Importance_RatingBarChange;
            importance.Rating = item.Importance;

            comment = FindViewById<TextView>(Resource.Id.comment_text);
            comment.Text = item.Notes ?? "no notes";

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
            PublicFields.allItems.Where(it => it.id == item.id).FirstOrDefault().Importance = rating;
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

            adapter.AddFragment(new CheeseListFragment(item, dataObserver), item.Name);
            viewPager.Adapter = adapter;
        }


        protected async override void OnActivityResult(int requestCode, Result resultCode,
Intent intent)
        {
            base.OnActivityResult(requestCode, resultCode, intent);

            if (resultCode == Result.Ok)
            {
                switch (requestCode)
                {
                    case
                        ITEMDETAIL: // back from other detail activity
                        if (intent.GetBooleanExtra("itemChanged", false))
                        {
                            string ItemID = intent.GetStringExtra("itemID");
                            Log.Debug("MainActivity", "Item changed");
                            int index = viewPager.CurrentItem;
                            var adapter = (MyAdapter)viewPager.Adapter;
                            var currentFragment = (CheeseListFragment)adapter.GetItem(index);
                            var fragmentAdapter = currentFragment.itemRecyclerViewAdapter;

                            var item = PublicFields.allItems.Find(it => it.id == ItemID);

                            fragmentAdapter.UpdateValue(item);
                            fragmentAdapter.ApplyChanges();
                        }

                        break;
                    case
                        EDIT_ITEM: // edited the detail item itself
                        var edited = intent.GetBooleanExtra("edited", false);
                        string itemID = intent.GetStringExtra("itemID");
                        bool groupChanged = intent.GetBooleanExtra("groupChanged", false);

                        if (groupChanged)
                        {
                            List<Todo.User> selectedContacts = JsonConvert.DeserializeObject<List<Todo.User>>(intent.GetStringExtra("selectedContacts"));
                            string groupName = intent.GetStringExtra("groupName");
                            //TODO: check if group already exists

                            if (selectedContacts != null && selectedContacts.Count > 0) // contacts selected so make a group for them
                            {
                                // make a new group
                                var ownedByGroupResult = await PublicFields.Database.SaveGroup(selectedContacts, groupName);
                                if (ownedByGroupResult != null)
                                {
                                    item.OwnedBy = ownedByGroupResult.ID;
                                    var itemAllItems = PublicFields.allItems.Find(it => it.id == itemID);
                                    var itemIndex = PublicFields.allItems.FindIndex(it => it.id == itemID);

                                    PublicFields.allItems[itemIndex].OwnedBy = ownedByGroupResult.ID;

                                    await PublicFields.Database.SaveItem(item);
                                }
                            }
                        }


                        if (edited) // the detail item
                        {
                            itemChanged = true;

                            var itemAllItems = PublicFields.allItems.Find(it => it.id == itemID);
                            var itemIndex = PublicFields.allItems.FindIndex(it => it.id == itemID);

                            await PublicFields.Database.SaveItem(itemAllItems);
                            for (int i = 0; i < itemAllItems.SubItems.Count; i++)// Todo.Item it in subItem.SubItems) // check if the subitems of the new card are new as well, if so save them
                            {
                                var it = itemAllItems.SubItems[i];
                                if (string.IsNullOrEmpty(it.id))
                                    await PublicFields.Database.SaveItem(it);
                                itemAllItems.SubItems[i] = it; // store with newly acquired id
                                if (PublicFields.allItems.Exists(item => item.id == it.id))
                                {
                                    var subItemIndex = PublicFields.allItems.FindIndex(item => item.id == it.id);
                                    PublicFields.allItems[subItemIndex] = it;
                                }
                                else
                                    PublicFields.allItems.Add(it);
                            }


                            PublicFields.allItems[itemIndex] = itemAllItems;
                            item = itemAllItems;

                            // refresh values
                            collapsingToolbar.SetTitle(item.Name);
                            importance.Rating = item.Importance;
                            comment.Text = item.Notes ?? "no notes";

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
                                        txtDate.Text = DateUtils.GetRelativeTimeSpanString(Application.Context, timeInMilliseconds);
                                }
                                catch (ParseException e)
                                {
                                    e.PrintStackTrace();
                                }
                            }

                            


                        }

                        else // added a new subitem
                        {
                            itemChanged = true;
                            var subItem = PublicFields.allItems.Find(it => it.id == itemID);
                            var indexSubItem = PublicFields.allItems.FindIndex(it => it.id == subItem.id);
                            subItem.id = null;

                            await PublicFields.Database.SaveItem(subItem);
                            for(int i = 0; i< subItem.SubItems.Count; i++)// Todo.Item it in subItem.SubItems) // check if the subitems of the new card are new as well, if so save them
                            {
                                var it = subItem.SubItems[i];
                                it.Parent = subItem.id; // change the parent id to the new one
                                if(string.IsNullOrEmpty(it.id))
                                    await PublicFields.Database.SaveItem(it);
                                subItem.SubItems[i] = it; // store with newly acquired id

                                if (PublicFields.allItems.Exists(item => item.id == it.id))
                                {
                                    var subItemIndex = PublicFields.allItems.FindIndex(item => item.id == it.id);
                                    PublicFields.allItems[subItemIndex] = it;
                                }
                                else
                                    PublicFields.allItems.Add(it);
                            }

                            PublicFields.allItems[indexSubItem] = subItem;

                            int index = viewPager.CurrentItem;
                            var adapter = (MyAdapter)viewPager.Adapter;
                            var currentFragment = (CheeseListFragment)adapter.GetItem(index);
                            var fragmentAdapter = currentFragment.itemRecyclerViewAdapter;

                            fragmentAdapter.addItem(subItem);//PublicFields.allItems.Find(it => it.id == itemID));
                            fragmentAdapter.ApplyChanges();
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
            intent.PutExtra("newItem", true);
            intent.PutExtra("parentItemID", item.id);
            StartActivityForResult(intent, EDIT_ITEM);
        }

        private void EditFAB_Click(object sender, EventArgs e)
        {
            Log.Info(TAG, "edit fab clicked!");
            var intent = new Intent(this, typeof(EditItemActivity));
            intent.PutExtra("itemID", item.id);
            intent.PutExtra("parentItemID", item.Parent);
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
                returnIntent.PutExtra("itemID", item.id);// ("passed_item", itemYouJustCreated);
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

