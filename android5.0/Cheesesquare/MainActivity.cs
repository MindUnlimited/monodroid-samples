﻿using System;

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
        private const int LOGIN = 102;
        private const int ITEMDETAIL = 103;


        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            PublicFields.Database = new Database();

            SetContentView(Resource.Layout.activity_main);

            var toolbar = FindViewById<V7Toolbar>(Resource.Id.toolbar);
            SetSupportActionBar(toolbar);

            SupportActionBar.SetHomeAsUpIndicator(Resource.Drawable.ic_menu);
            SupportActionBar.SetDisplayHomeAsUpEnabled(true);

            drawerLayout = FindViewById<DrawerLayout>(Resource.Id.drawer_layout);

            var navigationView = FindViewById<NavigationView>(Resource.Id.nav_view);
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
                StartActivity(intent);
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

        protected override void OnActivityResult(int requestCode, Result resultCode,
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

                            var item = PublicFields.allItems.Find(it => it.ID == ItemID);

                            fragmentAdapter.UpdateValue(item);
                            fragmentAdapter.ApplyChanges();
                        }
                            
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
            IEnumerable<string> groups_ids = from grp in PublicFields.userGroups select grp.ID;
            foreach(Todo.Item item in items)
            {
                var directSubItems = PublicFields.allItems.Where(it => groups_ids.Contains(it.OwnedBy) && it.Parent != null && it.Parent == item.ID).ToList();
                //var directSubItems = (from it in PublicFields.allItems where groups_ids.Contains(it.OwnedBy) && it.Parent != null && it.Parent == item.ID select it).ToList<Todo.Item>();
                item.SubItems = directSubItems;

                //if (directSubItems != null && directSubItems.Count != 0)
                //    AddSubTasks(item.SubItems);
            }
        }

        async void setupViewPager(Android.Support.V4.View.ViewPager viewPager)
        {
            PublicFields.allItems = (List<Todo.Item>) await PublicFields.Database.GetItems();
            PublicFields.userGroups = await PublicFields.Database.getGroups();           
            AddSubTasks(PublicFields.allItems); // links subtasks to the items

            PublicFields.domains = PublicFields.allItems.Where(it => it.Type == 1).ToList();
            
            var adapter = new MyAdapter(SupportFragmentManager);

            foreach (Todo.Item domain in PublicFields.domains)
            {
                adapter.AddFragment(new CheeseListFragment(domain.SubItems), domain.Name);
            }
            viewPager.Adapter = adapter;
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


