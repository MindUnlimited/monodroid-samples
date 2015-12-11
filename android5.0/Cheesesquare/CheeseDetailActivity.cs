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
using Java.Lang.Reflect;

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
        private TextView status;
        private ImageView statusIcon;

        private Android.Support.V7.Widget.PopupMenu statusMenu;

        private FloatingActionButton editFAB;
        private FloatingActionButton addItemFAB;
        private CollapsingToolbarLayout collapsingToolbar;
        private DrawerLayout drawerLayout;
        private ViewPager viewPager;

        private Todo.TreeNode<Todo.Item> item;
        public bool itemChanged;

        private RecyclerView.AdapterDataObserver dataObserver;// = new DataObserver();

    protected override void OnCreate (Bundle savedInstanceState) 
        {
            base.OnCreate (savedInstanceState);
            SetContentView(Resource.Layout.activity_detail);

            dataObserver = new DataObserver(this);
            itemChanged = false;

            itemID = Intent.GetStringExtra(ITEM_ID);
            var test = PublicFields.ItemTree.Descendants();
            item = PublicFields.ItemTree.Descendants().FirstOrDefault(node => node.Value.id == itemID);

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
            collapsingToolbar.SetTitle (item.Value.Name);

            txtDate = FindViewById<TextView>(Resource.Id.txtdate);
            if (item.Value.EndDate != null && item.Value.EndDate != "")
            {
                //String givenDateString = "Tue Apr 23 16:08:28 GMT+05:30 2013";
                //SimpleDateFormat sdf = new SimpleDateFormat("dd-MM-yyyy");//new SimpleDateFormat("EEE MMM dd HH:mm:ss z yyyy");
                try
                {
                    //Date mDate = sdf.Parse(item.EndDate);
                    long timeInMilliseconds;
                    long.TryParse(item.Value.EndDate, out timeInMilliseconds);
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
            importance.Rating = item.Value.Importance;

            comment = FindViewById<TextView>(Resource.Id.comment_text);
            comment.Text = item.Value.Notes ?? "no notes";

            status = FindViewById<TextView>(Resource.Id.status_text);
            status.Click += statusClick;

            statusIcon = FindViewById<ImageView>(Resource.Id.status_icon);

            editFAB = FindViewById<FloatingActionButton>(Resource.Id.edit_fab);
            editFAB.Click += EditFAB_Click;

            addItemFAB = FindViewById<FloatingActionButton>(Resource.Id.add_task_fab);
            addItemFAB.Click += AddItemFAB_Click;

            viewPager = FindViewById<Android.Support.V4.View.ViewPager>(Resource.Id.viewpager_cards_detail);
            setupViewPager(viewPager);

            loadBackdrop();
        }

        private void statusClick(object sender, EventArgs e)
        {
            statusMenu = new Android.Support.V7.Widget.PopupMenu(this, comment);
            statusMenu.Inflate(Resource.Menu.status_popup_menu);

            Field field = statusMenu.Class.GetDeclaredField("mPopup");
            field.Accessible = true;
            Java.Lang.Object menuPopupHelper = field.Get(statusMenu);
            Method setForceIcons = menuPopupHelper.Class.GetDeclaredMethod("setForceShowIcon", Java.Lang.Boolean.Type);
            setForceIcons.Invoke(menuPopupHelper, true);

            statusMenu.MenuItemClick += (s1, arg1) => {
                Console.WriteLine("{0} selected", arg1.Item.TitleFormatted);
                status.Text = arg1.Item.TitleFormatted.ToString();
                statusIcon.SetImageDrawable(arg1.Item.Icon);

                switch (status.Text)
                {
                    case "Cancelled":
                        item.Value.Status = -1;
                        break;
                    case "Backlog":
                        item.Value.Status = 0;
                        break;
                    case "On Hold":
                        item.Value.Status = 6;
                        break;
                    case "Completed":
                        item.Value.Status = 7;
                        break;
                    default:
                        item.Value.Status = 2;
                        break;
                }

                itemChanged = true;
            };

            statusMenu.DismissEvent += (s2, arg2) => {
                Console.WriteLine("menu dismissed");
            };

            statusMenu.Show();
        }

        private void Importance_RatingBarChange(object sender, RatingBar.RatingBarChangeEventArgs e)
        {
            var ratingbar = (RatingBar) sender;
            int rating = (int)ratingbar.Rating;

            item.Value.Importance = rating;
            PublicFields.ItemTree.Root.Descendants().FirstOrDefault(node => node.Value.id == item.Value.id).Value.Importance = rating;
            //PublicFields.ItemDictionary[item.Value.id].Value.Importance = rating;
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

            adapter.AddFragment(new CheeseListFragment(item, dataObserver), item.Value.Name);
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

                            var item = PublicFields.ItemTree.Root.Descendants().FirstOrDefault(node => node.Value.id == ItemID);

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
                                    item.Value.OwnedBy = ownedByGroupResult.ID;
                                    await PublicFields.Database.SaveItem(item.Value);
                                }
                            }
                        }


                        if (edited) // the detail item
                        {
                            itemChanged = true;

                            var detailItem = PublicFields.ItemTree.Root.Descendants().FirstOrDefault(node => node.Value.id == itemID);

                            await PublicFields.Database.SaveItem(detailItem.Value);
                            for (int i = 0; i < detailItem.Children.Count; i++)// Todo.Item it in subItem.SubItems) // check if the subitems of the new card are new as well, if so save them
                            {
                                var it = detailItem.Children[i];
                                if (string.IsNullOrEmpty(it.Value.id))
                                    await PublicFields.Database.SaveItem(it.Value);

                                detailItem.Children[i] = it; // store with newly acquired id

                                if (PublicFields.ItemTree.Descendants().FirstOrDefault(node => node.Value.id == it.Value.id) != null)
                                {
                                    PublicFields.ItemTree.FindAndReplace(it.Value.id, it);
                                }
                                else
                                {
                                    //?
                                }
                                    //PublicFields.ItemTree.FindAndReplace(it.Value.id, it);
                                //PublicFields.ItemDictionary[it.Value.id] = it;
                            }


                            //PublicFields.ItemDictionary[detailItem.Value.id] = detailItem;
                            item = detailItem;

                            // refresh values
                            collapsingToolbar.SetTitle(item.Value.Name);
                            importance.Rating = item.Value.Importance;
                            comment.Text = item.Value.Notes ?? "no notes";

                            if (item.Value.EndDate != null && item.Value.EndDate != "")
                            {
                                //String givenDateString = "Tue Apr 23 16:08:28 GMT+05:30 2013";
                                //SimpleDateFormat sdf = new SimpleDateFormat("dd-MM-yyyy");//new SimpleDateFormat("EEE MMM dd HH:mm:ss z yyyy");
                                try
                                {
                                    //Date mDate = sdf.Parse(item.EndDate);
                                    long timeInMilliseconds;
                                    long.TryParse(item.Value.EndDate, out timeInMilliseconds);
                                    if (timeInMilliseconds > 0)
                                        txtDate.Text = DateUtils.GetRelativeTimeSpanString(Application.Context, timeInMilliseconds);
                                }
                                catch (ParseException e)
                                {
                                    e.PrintStackTrace();
                                }
                            }

                            


                        }

                        else // added a new cardview (subItem)
                        {
                            itemChanged = true;
                            var itemCard = PublicFields.ItemTree.Descendants().FirstOrDefault(node => node.Value.id == itemID);
                            itemCard.Value.id = null;

                            await PublicFields.Database.SaveItem(itemCard.Value);

                            for(int i = 0; i< itemCard.Children.Count; i++) // check if the subitems of the new card are new as well, if so save them
                            {
                                var it = itemCard.Children[i];
                                it.Value.Parent = itemCard.Value.id; // change the parent id to the new one

                                if(string.IsNullOrEmpty(it.Value.id))
                                    await PublicFields.Database.SaveItem(it.Value);

                                itemCard.Children[i] = it; // store with newly acquired id

                                //// exists already so update
                                //if (PublicFields.ItemDictionary.Exists(item => item.id == it.id))
                                //{
                                //    var subItemIndex = PublicFields.ItemDictionary.FindIndex(item => item.id == it.id);
                                //    PublicFields.ItemDictionary[subItemIndex] = it;
                                //}
                                //else // new so add
                                //    PublicFields.ItemDictionary.Add(it);

                                PublicFields.ItemTree.FindAndReplace(it.Value.id, it);
                            }

                            PublicFields.ItemTree.FindAndReplace(itemCard.Value.id, itemCard);
                            //PublicFields.ItemDictionary[indexSubItem] = itemCard;

                            int index = viewPager.CurrentItem;
                            var adapter = (MyAdapter)viewPager.Adapter;
                            var currentFragment = (CheeseListFragment)adapter.GetItem(index);
                            var fragmentAdapter = currentFragment.itemRecyclerViewAdapter;

                            fragmentAdapter.addItem(itemCard);//PublicFields.allItems.Find(it => it.id == itemID));
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
            intent.PutExtra("parentItemID", item.Value.id);
            StartActivityForResult(intent, EDIT_ITEM);
        }

        private void EditFAB_Click(object sender, EventArgs e)
        {
            Log.Info(TAG, "edit fab clicked!");
            var intent = new Intent(this, typeof(EditItemActivity));
            intent.PutExtra("itemID", item.Value.id);
            intent.PutExtra("parentItemID", item.Parent.Value.id);
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


            //-1: Abandoned(Cancelled)
            //0: Backlog(Conceived = voor inbox, cq mind sweep)
            //1: Planned
            //Skip        2: Initiated
            //Skip        3: < 25 % completed
            //4: In progress (< 50 %)
            //Skip        5: < 75 %
            //6: On hold / Blocked
            //7: Completed

            switch (item.Value.Status)
            {
                case -1:
                    status.Text = "Cancelled";
                    statusIcon.SetImageResource(Resource.Drawable.ic_clear_black_24dp);
                    break;
                case 0:
                    status.Text = "Backlog";
                    statusIcon.SetImageResource(Resource.Drawable.ic_inbox_black_24dp);
                    break;
                case 6:
                    status.Text = "On Hold";
                    statusIcon.SetImageResource(Resource.Drawable.ic_block_black_24dp);
                    break;
                case 7:
                    status.Text = "Completed";
                    statusIcon.SetImageResource(Resource.Drawable.ic_check_black_24dp);
                    break;
                default:
                    status.Text = "Started";
                    statusIcon.SetImageResource(Resource.Drawable.ic_play_arrow_black_24dp);
                    break;

            }
        }

        public override void Finish()
        {
            Intent returnIntent = new Intent();
            returnIntent.PutExtra("itemChanged", itemChanged);// ("passed_item", itemYouJustCreated);
            if (itemChanged)
                returnIntent.PutExtra("itemID", item.Value.id);// ("passed_item", itemYouJustCreated);
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

