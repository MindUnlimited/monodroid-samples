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

namespace Cheesesquare
{
    [Activity (Name = "com.sample.cheesesquare.MainActivity", Label = "MindSet", MainLauncher = true)]
    public class MainActivity : AppCompatActivity, NavigationView.IOnNavigationItemSelectedListener
    {        
        DrawerLayout drawerLayout;
               
        protected override void OnCreate(Bundle savedInstanceState) 
        {
            base.OnCreate (savedInstanceState);

            SetContentView (Resource.Layout.activity_main);

            Log.Debug("CheeseDetailAcitivity", this.ComponentName.ToString());

            var toolbar = FindViewById<V7Toolbar>(Resource.Id.toolbar);
            SetSupportActionBar (toolbar);

            SupportActionBar.SetHomeAsUpIndicator (Resource.Drawable.ic_menu);
            SupportActionBar.SetDisplayHomeAsUpEnabled (true);

            drawerLayout = FindViewById<DrawerLayout> (Resource.Id.drawer_layout);

            var navigationView = FindViewById<NavigationView> (Resource.Id.nav_view);
            if (navigationView != null)
                setupDrawerContent(navigationView);

            var viewPager = FindViewById<Android.Support.V4.View.ViewPager> (Resource.Id.viewpager);
            if (viewPager != null)
                setupViewPager(viewPager);

            var fab = FindViewById<FloatingActionButton> (Resource.Id.fab);
            fab.Click += (sender, e) =>
            {
                //Snackbar.Make (fab, "Here's a snackbar!", Snackbar.LengthLong).SetAction ("Action",
                //    new ClickListener (v => {
                //        Console.WriteLine ("Action handler");
                //    })).Show ();
                var intent = new Intent(this, typeof(EditItemActivity));
                StartActivity(intent);
            };

            var tabLayout = FindViewById<TabLayout> (Resource.Id.tabs);
            tabLayout.SetupWithViewPager (viewPager);
        }
            
        public override bool OnCreateOptionsMenu (IMenu menu) 
        {
            MenuInflater.Inflate(Resource.Menu.sample_actions, menu);
            return true;
        }
            
        public override bool OnOptionsItemSelected (IMenuItem item) 
        {
            switch (item.ItemId) {
            case Android.Resource.Id.Home:
                drawerLayout.OpenDrawer (Android.Support.V4.View.GravityCompat.Start);
                return true;
            }
            return base.OnOptionsItemSelected (item);
        }

        void setupViewPager (Android.Support.V4.View.ViewPager viewPager) 
        {
            var adapter = new Adapter (SupportFragmentManager);
            adapter.AddFragment (new CheeseListFragment (), "Friends");
            adapter.AddFragment (new CheeseListFragment (), "Family");
            adapter.AddFragment (new CheeseListFragment (), "Work");
            adapter.AddFragment(new CheeseListFragment(), "Other");
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
                    drawerLayout.OpenDrawer(Android.Support.V4.View.GravityCompat.Start);

                    NavUtils.NavigateUpFromSameTask(this);
                    return true;
            }

            return false;
        }

        class Adapter : Android.Support.V4.App.FragmentPagerAdapter 
        {
            List<V4Fragment> fragments = new List<V4Fragment> ();
            List<string> fragmentTitles = new List<string> ();

            public Adapter (V4FragmentManager fm) : base (fm)
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


