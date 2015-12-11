using System;

using Android.App;
using Android.Content;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.OS;
using Android.Support.V7.App;
using V7Toolbar = Android.Support.V7.Widget.Toolbar;
using V4Fragment = Android.Support.V4.App.Fragment;
using V4FragmentManager = Android.Support.V4.App.FragmentManager;
using Android.Support.V4.Widget;
using Android.Support.Design.Widget;
using System.Collections.Generic;
using Android.Support.V4.App;
using Android.Util;
using Android.Support.V4.View;
using Android.Graphics;
using Microsoft.WindowsAzure.MobileServices;
using Android.Preferences;
using Newtonsoft.Json;
using System.Linq;
using Android.Support.V7.Widget;
using Todo;

namespace Cheesesquare
{
    public static class PublicFields
    {
        public static Database Database;

        public static List<Todo.Item> allItems { get; set; }
        public static List<Todo.Item> domains { get; set; }
        public static List<Todo.Group> userGroups { get; set; }
    }


    [Activity(Name = "com.sample.cheesesquare.MainActivity", Label = "MindSet", MainLauncher = true)]
    public class MainActivity : AppCompatActivity, NavigationView.IOnNavigationItemSelectedListener
    {
        DrawerLayout drawerLayout;
        ViewPager viewPager;
        TabLayout tabLayout;
        NavigationView navigationView;

        private const int LOGIN = 102;
        private const int ITEMDETAIL = 103;
        private const int EDIT_ITEM = 104;

        private CheeseListFragment currentDomainFragment;
        //private RecyclerView.AdapterDataObserver dataObserver;


        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            Tree<string> root = new Tree<string>(null);
            root.Value = "zero";
            int d0 = root.Depth;
            TreeNode<string> one = root.Children.Add("one");
            int d1 = one.Depth;
            TreeNode<string> twoa = one.Children.Add("two - a");
            TreeNode <string> twob = one.Children.Add("two - b");
            TreeNode <string> twoc = one.Children.Add("two - c");
            string twocstr = twoc.Value;
            int d2 = twoa.Depth;

            root.

            PublicFields.Database = new Database();
            //dataObserver = new DataObserver();

            SetContentView(Resource.Layout.activity_main);

            var toolbar = FindViewById<V7Toolbar>(Resource.Id.toolbar);
            SetSupportActionBar(toolbar);

            SupportActionBar.SetHomeAsUpIndicator(Resource.Drawable.ic_menu);
            SupportActionBar.SetDisplayHomeAsUpEnabled(true);

            drawerLayout = FindViewById<DrawerLayout>(Resource.Id.drawer_layout);

            navigationView = FindViewById<NavigationView>(Resource.Id.nav_view);
            if (navigationView != null)
                setupDrawerContent(navigationView);


            var fab = FindViewById<FloatingActionButton>(Resource.Id.fab);
            fab.Click += (sender, e) =>
            {
                //Snackbar.Make (fab, "Here's a snackbar!", Snackbar.LengthLong).SetAction ("Action",
                //    new ClickListener (v => {
                //        Console.WriteLine ("Action handler");
                //    })).Show ();
                var intent = new Intent(this, typeof(EditItemActivity));
                intent.PutExtra("newItem", true);
                intent.PutExtra("newItem", true);
                intent.PutExtra("parentItemID", currentDomainFragment.domain.id);

                StartActivityForResult(intent, EDIT_ITEM);
            };

            viewPager = FindViewById<Android.Support.V4.View.ViewPager>(Resource.Id.viewpager);
            tabLayout = FindViewById<TabLayout>(Resource.Id.tabs);

            // not logged in
            if (PublicFields.Database.userID == null)
            {
                // Shared Preferences are the local saved value for the app. Used here to access the last used provider
                var preferences = PreferenceManager.GetDefaultSharedPreferences(this);


                var loginIntent = new Intent(this, typeof(LoginActivity));
                // Try to use the latest used oauth provider           
                if (preferences.Contains("LastUsedProvider"))
                {
                    string providerName = preferences.GetString("LastUsedProvider", "");
                    MobileServiceAuthenticationProvider provider;

                    switch (providerName)
                    {
                        case "Facebook":
                            provider = MobileServiceAuthenticationProvider.Facebook;
                            loginIntent.PutExtra("provider", JsonConvert.SerializeObject(provider));
                            //await auth.Authenticate(provider);
                            break;
                        case "Google":
                            provider = MobileServiceAuthenticationProvider.Google;
                            loginIntent.PutExtra("provider", JsonConvert.SerializeObject(provider));
                            //await auth.Authenticate(provider);
                            break;
                        case "MicrosoftAccount":
                            provider = MobileServiceAuthenticationProvider.MicrosoftAccount;
                            loginIntent.PutExtra("provider", JsonConvert.SerializeObject(provider));
                            //await auth.Authenticate(provider);
                            break;
                        case "Twitter":
                            provider = MobileServiceAuthenticationProvider.Twitter;
                            loginIntent.PutExtra("provider", JsonConvert.SerializeObject(provider));
                            //await auth.Authenticate(provider);
                            break;
                        case "WindowsAzureActiveDirectory":
                            provider = MobileServiceAuthenticationProvider.WindowsAzureActiveDirectory;
                            loginIntent.PutExtra("provider", JsonConvert.SerializeObject(provider));
                            //await auth.Authenticate(provider);
                            break;
                        default:
                            break;
                    }
                }

                StartActivityForResult(loginIntent, LOGIN);
            }


        }

        protected override void OnStart()
        {
            base.OnStart();


            //else
            //{
            //    var loginpage = new NavigationPage(new Views.SelectLoginProviderPage());
            //    await App.Navigation.PushModalAsync(loginpage);
            //}
            //await Todo.App.selectedDomainPage.Refresh();

            //RequestedOrientation = ScreenOrientation.Portrait;
        }

        public override void OnBackPressed()
        {
            new Android.Support.V7.App.AlertDialog.Builder(this)
                .SetMessage("Are you sure you want to exit?")
                .SetCancelable(false)
                .SetPositiveButton("Yes", delegate
                {
                    Finish();
                })
               .SetNegativeButton("No", delegate
               {
                   return;
               })
               .Show();
        }

protected async override void OnActivityResult(int requestCode, Result resultCode,
        Intent intent)
        {
            base.OnActivityResult(requestCode, resultCode, intent);

            if (resultCode == Result.Ok)
            {
                switch (requestCode)
                {
                    case LOGIN:
                        if (viewPager != null)
                            setupViewPager(viewPager);

                        tabLayout.SetupWithViewPager(viewPager);

                        var username = navigationView.FindViewById<TextView>(Resource.Id.username_nav);
                        username.Text = PublicFields.Database.userName;
                        break;
                    case
                        ITEMDETAIL:
                        if (intent.GetBooleanExtra("itemChanged", false))
                        {
                            string ItemID = intent.GetStringExtra("itemID");
                            Log.Debug("MainActivity", "Item changed");
                            int index = viewPager.CurrentItem;
                            var adapter = (MyAdapter)viewPager.Adapter;
                            var currentFragment = (CheeseListFragment) adapter.GetItem(index);
                            var fragmentAdapter = currentFragment.itemRecyclerViewAdapter;

                            var item = PublicFields.allItems.Find(it => it.id == ItemID);
                            var itemIndex = PublicFields.allItems.FindIndex(it => it.id == ItemID);

                            //await PublicFields.Database.SaveItem(item);
                            //for (int i = 0; i < item.SubItems.Count; i++)// Todo.Item it in subItem.SubItems) // check if the subitems of the new card are new as well, if so save them
                            //{
                            //    var it = item.SubItems[i];
                            //    it.Parent = item.id; // change the parent id to the new one
                            //    if (string.IsNullOrEmpty(it.id))
                            //        await PublicFields.Database.SaveItem(it);
                            //    item.SubItems[i] = it; // store with newly acquired id
                            //}


                            //PublicFields.allItems[itemIndex] = item; 
                            //changes happen on cheesedetail niveau not here

                            fragmentAdapter.UpdateValue(item);
                            fragmentAdapter.ApplyChanges();


                        }
                            
                        break;

                    case EDIT_ITEM:// new item
                        var itemID = intent.GetStringExtra("itemID");

                        if (itemID == "temporaryID")
                        {
                            var newItem = PublicFields.allItems.Find(it => it.id == itemID);
                            var indexSubItem = PublicFields.allItems.FindIndex(it => it.id == newItem.id);
                            newItem.id = null;

                            await PublicFields.Database.SaveItem(newItem);
                            itemID = newItem.id;

                            for (int i = 0; i < newItem.SubItems.Count; i++)// Todo.Item it in subItem.SubItems) // check if the subitems of the new card are new as well, if so save them
                            {
                                var it = newItem.SubItems[i];
                                it.Parent = itemID; // change the parent id to the new one
                                if (string.IsNullOrEmpty(it.id))
                                    await PublicFields.Database.SaveItem(it);
                                newItem.SubItems[i] = it; // store with newly acquired id
                            }

                            PublicFields.allItems[indexSubItem] = newItem;

                        }

                        currentDomainFragment.itemRecyclerViewAdapter.addItem(PublicFields.allItems.Find(it => it.id == itemID));
                        currentDomainFragment.itemRecyclerViewAdapter.ApplyChanges();
                        break;
                    default:
                        break;
                }
            }
        }

        public override bool OnCreateOptionsMenu(IMenu menu)
        {
            MenuInflater.Inflate(Resource.Menu.sample_actions, menu);
            return true;
        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            switch (item.ItemId) {
                case Android.Resource.Id.Home:
                    drawerLayout.OpenDrawer(Android.Support.V4.View.GravityCompat.Start);
                    return true;
            }
            return base.OnOptionsItemSelected(item);
        }

        public void AddSubTasks(List<Todo.Item> items)
        {
            IEnumerable<string> groups_ids = from grp in PublicFields.Database.userGroups select grp.ID;
            foreach(Todo.Item item in items)
            {
                var directSubItems = PublicFields.allItems.Where(it => groups_ids.Contains(it.OwnedBy) && it.Parent != null && it.Parent == item.id).ToList();
                //var directSubItems = (from it in PublicFields.allItems where groups_ids.Contains(it.OwnedBy) && it.Parent != null && it.Parent == item.ID select it).ToList<Todo.Item>();
                item.SubItems = directSubItems;

                //if (directSubItems != null && directSubItems.Count != 0)
                //    AddSubTasks(item.SubItems);
            }
        }

        async void setupViewPager(Android.Support.V4.View.ViewPager viewPager)
        {
            PublicFields.allItems = (List<Todo.Item>) await PublicFields.Database.GetItems();
            if (PublicFields.Database.userGroups == null)
                PublicFields.Database.userGroups = await PublicFields.Database.getGroups();           
            AddSubTasks(PublicFields.allItems); // links subtasks to the items

            PublicFields.domains = PublicFields.allItems.Where(it => it.Type == 1).ToList();
            
            var adapter = new MyAdapter(SupportFragmentManager);

            foreach (Todo.Item domain in PublicFields.domains)
            {
                adapter.AddFragment(new CheeseListFragment(domain), domain.Name);
            }
            viewPager.Adapter = adapter;
            currentDomainFragment = ((CheeseListFragment)((MyAdapter)viewPager.Adapter).GetItem(viewPager.CurrentItem));


            viewPager.PageSelected += ViewPager_PageSelected;
        }

        private void ViewPager_PageSelected(object sender, ViewPager.PageSelectedEventArgs e)
        {
            var vp = (ViewPager) sender;
            currentDomainFragment = ((CheeseListFragment)((MyAdapter)vp.Adapter).GetItem(vp.CurrentItem));
        }

        void setupDrawerContent(NavigationView navigationView) 
        {
            navigationView.SetNavigationItemSelectedListener(this);

            //navigationView.NavigationItemSelected += (sender, e) => {
            //    switch (e.MenuItem.ItemId)
            //    {
            //        case Android.Resource.Id.Home:
            //            drawerLayout.OpenDrawer(Android.Support.V4.View.GravityCompat.Start);
            //            break;
            //    }
            //    e.MenuItem.SetChecked(true);
            //    //e.P0.SetChecked (true);
            //    drawerLayout.CloseDrawers ();
            //};
        }

        public bool OnNavigationItemSelected(IMenuItem menuItem)
        {
            switch (menuItem.ItemId)
            {
                case Resource.Id.nav_home:
                    drawerLayout.CloseDrawers();
                    return true;
            }

            return false;
        }

        class MyAdapter : Android.Support.V4.App.FragmentPagerAdapter 
        {
            List<V4Fragment> fragments = new List<V4Fragment> ();
            List<string> fragmentTitles = new List<string> ();

            public MyAdapter(V4FragmentManager fm) : base (fm)
            {
            }

            public void AddFragment (V4Fragment fragment, String title) 
            {
                fragments.Add(fragment);
                fragmentTitles.Add(title);
            }
                
            public override V4Fragment GetItem(int position) 
            {
                return fragments [position];
            }

            public override int Count {
                get { return fragments.Count; }
            }

            public override Java.Lang.ICharSequence GetPageTitleFormatted (int position)
            {
                return new Java.Lang.String (fragmentTitles [position]);
            }

        }
    }

    public class ClickListener : Java.Lang.Object, View.IOnClickListener
    {
        public ClickListener(Action<View> handler)
        {
            Handler = handler;
        }

        public Action<View> Handler { get; set; }

        public void OnClick(View v)
        {
            var h = Handler;
            if (h != null)
                h(v);
        }
    }
}


