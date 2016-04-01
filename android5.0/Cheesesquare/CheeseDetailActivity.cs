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
using Todo;
using Cheesesquare.Models;
using System.Threading.Tasks;
using Android.Content.PM;
using Android.Graphics;

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

    [Activity (Label="Details", ScreenOrientation = ScreenOrientation.Portrait)]
    [MetaData("android.support.PARENT_ACTIVITY", Value = "com.sample.cheesesquare.MainActivity")]
    public class CheeseDetailActivity : AppCompatActivity
    {
        public const string EXTRA_NAME = "item_name";
        public const string ITEM_ID = "item_id";

        private const int ITEMDETAIL = 103;
        private const int EDIT_ITEM = 104;
        private const int PICKIMAGE = 105;

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
        private RelativeLayout statusRelLayout;

        private Android.Support.V7.Widget.PopupMenu statusMenu;

        TextView userName;

        private FloatingActionButton editFAB;
        private FloatingActionButton addItemFAB;
        private CollapsingToolbarLayout collapsingToolbar;
        private DrawerLayout drawerLayout;
        private WrapContentHeightViewPager viewPager;

        private Todo.TreeNode<Todo.Item> item;
        public bool itemChanged;
        public bool databaseUpdated;

        //private RecyclerView.AdapterDataObserver dataObserver;// = new DataObserver();

    protected override void OnCreate (Bundle savedInstanceState) 
        {
            base.OnCreate (savedInstanceState);
            SetContentView(Resource.Layout.activity_detail);

            //dataObserver = new DataObserver(this);
            itemChanged = false;
            databaseUpdated = false;

            drawerLayout = FindViewById<DrawerLayout>(Resource.Id.drawer_layout_detail);

            var toolbar = FindViewById<V7Toolbar>(Resource.Id.toolbar);
            SetSupportActionBar (toolbar);

            var navigationView = drawerLayout.FindViewById<NavigationView>(Resource.Id.nav_view_detail);
            if (navigationView != null)
                setupDrawerContent(navigationView);

            userName = navigationView.GetHeaderView(0).FindViewById<TextView>(Resource.Id.username_nav);

            SupportActionBar.SetHomeAsUpIndicator(Resource.Drawable.ic_menu);
            SupportActionBar.SetDisplayHomeAsUpEnabled(true);

            collapsingToolbar = FindViewById<CollapsingToolbarLayout> (Resource.Id.collapsing_toolbar);

            txtDate = FindViewById<TextView>(Resource.Id.txtdate);

            progressSlider = FindViewById<SeekBar>(Resource.Id.progressSlider);
            progressPercentText = FindViewById<TextView>(Resource.Id.progressPercentText);
            progressSlider.ProgressChanged += ProgressSlider_ProgressChanged;

            importance = FindViewById<RatingBar>(Resource.Id.ratingbar);
            importance.RatingBarChange += Importance_RatingBarChange;

            comment = FindViewById<TextView>(Resource.Id.comment_text);

            statusIcon = FindViewById<ImageView>(Resource.Id.status_icon);
            status = FindViewById<TextView>(Resource.Id.status_text);
            statusRelLayout = FindViewById<RelativeLayout>(Resource.Id.status_rel_layout);
            statusRelLayout.Clickable = true;

            statusRelLayout.Click += statusClick;

            //statusIcon.Click += statusClick;
            //status.Click += statusClick;

            editFAB = FindViewById<FloatingActionButton>(Resource.Id.edit_fab);
            editFAB.Click += EditFAB_Click;

            addItemFAB = FindViewById<FloatingActionButton>(Resource.Id.add_task_fab);
            addItemFAB.Click += AddItemFAB_Click;

            viewPager = FindViewById<WrapContentHeightViewPager>(Resource.Id.viewpager_cards_detail);

            loadBackdrop();
        }

        private void statusClick(object sender, EventArgs e)
        {
            statusMenu = new Android.Support.V7.Widget.PopupMenu(this, statusIcon);
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
                    case "Started":
                        item.Value.Status = 2;
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

        public class MyAdapter : Android.Support.V4.App.FragmentPagerAdapter
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

            //adapter.AddFragment(new CheeseListFragmentDetail(item, dataObserver), item.Value.Name);
            adapter.AddFragment(new CheeseListFragmentDetail(item), item.Value.Name);
            viewPager.Adapter = adapter;
        }


        protected async override void OnActivityResult(int requestCode, Result resultCode,
Intent intent)
        {
            base.OnActivityResult(requestCode, resultCode, intent);

            if (resultCode == Result.Ok)
            {
                List<TreeNode<Todo.Item>> allItemsList = PublicFields.ItemTree.Descendants().ToList();
                //item = PublicFields.ItemTree.Descendants().First(node => node.Value.id == itemID);

                switch (requestCode)
                {
                    case PICKIMAGE:
                        if (intent != null)
                        {
                            Android.Net.Uri uri = intent.Data;
                            string ItemID = intent.GetStringExtra(CheeseDetailActivity.ITEM_ID);
                            string path = intent.GetStringExtra("path");
                            int resourceID = intent.GetIntExtra("resourceID", 0);
                            int resourceBackdropID = intent.GetIntExtra("resourceBackdropID", 0);

                            if (resourceID != 0)
                            {
                                int index = viewPager.CurrentItem;
                                var adapter = (MyAdapter)viewPager.Adapter;
                                var currentFragment = (CheeseListFragment)adapter.GetItem(index);
                                var fragmentAdapter = currentFragment.itemRecyclerViewAdapter;

                                var item = PublicFields.ItemTree.Descendants().FirstOrDefault(node => node.Value.id == ItemID);
                                item.Value.ImageResource = resourceID;
                                item.Value.ImageResourceBackdrop = resourceBackdropID;

                                fragmentAdapter.UpdateValue(item);
                                fragmentAdapter.ApplyChanges();
                            }
                            else
                            {
                                int index = viewPager.CurrentItem;
                                var adapter = (MyAdapter)viewPager.Adapter;
                                var currentFragment = (CheeseListFragment)adapter.GetItem(index);
                                var fragmentAdapter = currentFragment.itemRecyclerViewAdapter;

                                var item = PublicFields.ItemTree.Descendants().FirstOrDefault(node => node.Value.id == ItemID);
                                item.Value.ImagePath = path;

                                fragmentAdapter.UpdateValue(item);
                                fragmentAdapter.ApplyChanges();
                            }
                        }
                        break;
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

                        var edited = intent.GetBooleanExtra("edited", false); // pressed the edit button or added new one
                        string editItemId = intent.GetStringExtra("itemID");
                        bool groupChanged = intent.GetBooleanExtra("groupChanged", false);

                        itemChanged = true;

                        // update the item of this detail view
                        this.item = PublicFields.ItemTree.Descendants().FirstOrDefault(node => node.Value.id == item.Value.id); 

                        if (groupChanged) // sharing an item
                        {
                            List<Todo.User> selectedContacts = JsonConvert.DeserializeObject<List<Todo.User>>(intent.GetStringExtra("selectedContacts"));
                            string groupName = intent.GetStringExtra("groupName"); // only has a name if user generated group
                            //TODO: check if group already exists

                            if (selectedContacts != null && selectedContacts.Count > 0) // contacts selected so make a group for them
                            {
                                var ownedByGroupResult = await PublicFields.Database.SaveGroup(selectedContacts, groupName);
                                if (ownedByGroupResult != null)
                                {
                                    item.Value.OwnedBy = ownedByGroupResult.id;
                                    PublicFields.ItemTree.FindAndReplace(item.Value.id, item);
                                }

                                var groupMembers = await PublicFields.Database.MembersOfGroup(ownedByGroupResult);

                                foreach(var grp in  groupMembers)
                                {
                                    // this account does not need the id. only the ones the item gets shared with
                                    if (grp != null && grp.id != PublicFields.Database.defGroup.id)
                                    {
                                        var link = new ItemLink { ItemID = item.Value.id, Parent = null, OwnedBy = grp.id };
                                        await PublicFields.Database.SaveItemLink(link);
                                    }
                                }
                            }



                        }

                        

                        if (edited && this.item != null) // the detail item
                        {
                            await PublicFields.Database.SaveItem(this.item.Value);

                            for (int i = 0; i < item.Children.Count; i++) // check if the subitems of the new card are new as well, if so save them
                            {
                                var it = item.Children[i];
                                if (string.IsNullOrEmpty(it.Value.id))
                                    await PublicFields.Database.SaveItem(it.Value);

                                item.Children[i] = it; // store with newly acquired id
                            }

                            // refresh values
                            collapsingToolbar.SetTitle(item.Value.Name);
                            importance.Rating = item.Value.Importance;

                            if (item.Value.EndDate != null)
                            {
                                //String givenDateString = "Tue Apr 23 16:08:28 GMT+05:30 2013";
                                //SimpleDateFormat sdf = new SimpleDateFormat("dd-MM-yyyy");//new SimpleDateFormat("EEE MMM dd HH:mm:ss z yyyy");
                                try
                                {
                                    //Date mDate = sdf.Parse(item.EndDate);

                                    long timeInMilliseconds = (long)(TimeZoneInfo.ConvertTimeToUtc(item.Value.EndDate) -
                                    new DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc)).TotalMilliseconds;

                                    if (timeInMilliseconds >= 0)
                                        txtDate.Text = DateUtils.GetRelativeTimeSpanString(Application.Context, timeInMilliseconds);
                                }
                                catch (ParseException e)
                                {
                                    e.PrintStackTrace();
                                }
                            }

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

                            if (string.IsNullOrEmpty(item.Value.Notes))
                            {
                                comment.Text = "No notes";
                            }
                            else
                            {
                                comment.Text = item.Value.Notes;
                            }
                        }
                        else // added a new cardview (subItem)
                        {
                            var itemCard = PublicFields.ItemTree.Descendants().FirstOrDefault(node => node.Value.id == editItemId); // update the item of this detail view
                            var tempID = itemCard.Value.id;
                            itemCard.Value.id = null; //get rid of the temporaryID string

                            await PublicFields.Database.SaveItem(itemCard.Value);

                            for(int i = 0; i< itemCard.Children.Count; i++) // check if the subitems of the new card are new as well, if so save them
                            {
                                var it = itemCard.Children[i];
                                it.Value.Parent = itemCard.Value.id; // change the parent id to the new one

                                if(string.IsNullOrEmpty(it.Value.id))
                                    await PublicFields.Database.SaveItem(it.Value);

                                itemCard.Children[i] = it; // store with newly acquired id

                                //PublicFields.ItemTree.FindAndReplace(it.Value.id, it);
                            }
                        }
                        this.item = PublicFields.ItemTree.Descendants().FirstOrDefault(node => node.Value.id == item.Value.id);
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
                        drawerLayout.CloseDrawers();
                        Finish();
                        //NavUtils.NavigateUpFromSameTask(this);
                        break;
                    case Resource.Id.shared_items:
                        Intent intent = new Intent(this, typeof(SharedItemsActivity));
                        intent.AddFlags(ActivityFlags.ClearTop);
                        drawerLayout.CloseDrawers();
                        StartActivity(intent);
                        break;
                    case Resource.Id.groups:
                        intent = new Intent(this, typeof(GroupsActivity));
                        intent.AddFlags(ActivityFlags.ClearTop);
                        drawerLayout.CloseDrawers();
                        StartActivity(intent);
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

        public override bool OnOptionsItemSelected(IMenuItem menuItem)
        {
            switch (menuItem.ItemId)
            {
                case Android.Resource.Id.Home:
                    drawerLayout.OpenDrawer(Android.Support.V4.View.GravityCompat.Start);
                    return true;
                case Resource.Id.refresh:
                    Log.Debug("detail", "attempting refresh");

                    PublicFields.UpdateDatabase();

                    PublicFields.MakeTree();
                    item = PublicFields.ItemTree.Descendants().FirstOrDefault(node => node.Value.id == itemID);
                    setupViewPager(viewPager);


                    // refresh values detail item
                    collapsingToolbar.SetTitle(item.Value.Name);
                    importance.Rating = item.Value.Importance;

                    if (item.Value.EndDate != null)
                    {
                        //String givenDateString = "Tue Apr 23 16:08:28 GMT+05:30 2013";
                        //SimpleDateFormat sdf = new SimpleDateFormat("dd-MM-yyyy");//new SimpleDateFormat("EEE MMM dd HH:mm:ss z yyyy");
                        try
                        {
                            //Date mDate = sdf.Parse(item.EndDate);

                            long timeInMilliseconds = (long)(TimeZoneInfo.ConvertTimeToUtc(item.Value.EndDate) -
                            new DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc)).TotalMilliseconds;

                            if (timeInMilliseconds >= 0)
                                txtDate.Text = DateUtils.GetRelativeTimeSpanString(Application.Context, timeInMilliseconds);
                        }
                        catch (ParseException e)
                        {
                            e.PrintStackTrace();
                        }
                    }

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

                    if (string.IsNullOrEmpty(item.Value.Notes))
                    {
                        comment.Text = "No notes";
                    }
                    else
                    {
                        comment.Text = item.Value.Notes;
                    }

                    databaseUpdated = true;

                    return true;

            }
            return base.OnOptionsItemSelected(menuItem);
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

            itemID = Intent.GetStringExtra(ITEM_ID);
            item = PublicFields.ItemTree.Descendants().FirstOrDefault(node => node.Value.id == itemID);

            if (item == null || item.Value == null)
            {
                RunOnUiThread(
                    () =>
                    {
                        new Android.Support.V7.App.AlertDialog.Builder(this)
                        .SetMessage("The item could not be found")
                        .SetCancelable(false)
                        .SetPositiveButton("OK", delegate
                        {
                            Finish();
                        })
                        .Show();
                    }
                );
            }
            else
            {
                userName.Text = PublicFields.Database.userName;
                collapsingToolbar.SetTitle(item.Value.Name);

                importance.Rating = item.Value.Importance;

                if (item.Value.EndDate != null)
                {
                    //String givenDateString = "Tue Apr 23 16:08:28 GMT+05:30 2013";
                    //SimpleDateFormat sdf = new SimpleDateFormat("dd-MM-yyyy");//new SimpleDateFormat("EEE MMM dd HH:mm:ss z yyyy");
                    try
                    {
                        long timeInMilliseconds = (long)(TimeZoneInfo.ConvertTimeToUtc(item.Value.EndDate) -
                        new DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc)).TotalMilliseconds;

                        if (timeInMilliseconds >= 0)
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

                if (string.IsNullOrEmpty(item.Value.Notes))
                {
                    comment.Text = "No notes";
                }
                else
                {
                    comment.Text = item.Value.Notes;
                }

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

                setupViewPager(viewPager);

                
            }
        }

        public override void Finish()
        {
            //itemChanged = true;

            Intent returnIntent = new Intent();
            returnIntent.PutExtra("itemChanged", itemChanged);// ("passed_item", itemYouJustCreated);
            returnIntent.PutExtra("databaseUpdated", databaseUpdated);
            if (itemChanged)
                returnIntent.PutExtra("itemID", item.Value.id);// ("passed_item", itemYouJustCreated);
            SetResult(Result.Ok, returnIntent);
            base.Finish();
        }


        void loadBackdrop() 
        {            
            var imageView = FindViewById<ImageView> (Resource.Id.backdrop);

            itemID = Intent.GetStringExtra(ITEM_ID);
            item = PublicFields.ItemTree.Descendants().FirstOrDefault(node => node.Value.id == itemID);
            if (item.Value.ImagePath != null)
            {
                BitmapFactory.Options options = new BitmapFactory.Options();
                options.InJustDecodeBounds = true;
                BitmapFactory.DecodeFile(item.Value.ImagePath, options);
                int imageHeight = options.OutHeight;
                int imageWidth = options.OutWidth;
                String imageType = options.OutMimeType;

                var sampledBitmap = PublicFields.DecodeSampledBitmapFromFile(item.Value.ImagePath, 500, 500);

                imageView.SetImageBitmap(sampledBitmap);
            }
            else if (item.Value.ImageResourceBackdrop != 0)
            {
                imageView.SetImageResource(item.Value.ImageResourceBackdrop);
            }
            else
            {
                //var r = Cheeses.GetRandomCheeseBackground();
                //imageView.SetImageResource(r);
            }
        }
    }
}

