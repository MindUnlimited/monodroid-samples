using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.Support.V7.Widget;

using V7Toolbar = Android.Support.V7.Widget.Toolbar;
using Todo;
using Android.Support.V4.View;
using Android.Support.V7.App;
using Android.Support.V4.Widget;
using Android.Support.Design.Widget;

namespace Cheesesquare
{
    [Activity(Label = "GroupsActivity")]
    public class GroupsActivity : AppCompatActivity, NavigationView.IOnNavigationItemSelectedListener
    {
        private ViewPager viewPager;
        private DrawerLayout drawerLayout;
        private NavigationView navigationView;
        private TextView userName;

        protected async override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // Create your application here
            SetContentView(Resource.Layout.activity_groups);

            var toolbar = FindViewById<V7Toolbar>(Resource.Id.toolbar);
            SetSupportActionBar(toolbar);

            SupportActionBar.SetHomeAsUpIndicator(Resource.Drawable.ic_menu);
            SupportActionBar.SetDisplayHomeAsUpEnabled(true);
            SupportActionBar.Title = "Your Groups";

            viewPager = FindViewById<ViewPager>(Resource.Id.viewpager_groups);
            drawerLayout = FindViewById<DrawerLayout>(Resource.Id.drawer_layout_groups);

            var groups = await PublicFields.Database.getGroups();
            var adapter = new CheeseDetailActivity.MyAdapter(SupportFragmentManager);
            adapter.AddFragment(new ListFragmentGroups(groups), "Shared Items");

            viewPager.Adapter = adapter;

            navigationView = FindViewById<NavigationView>(Resource.Id.nav_view_groups);
            navigationView.SetNavigationItemSelectedListener(this);

            navigationView.SetCheckedItem(Resource.Id.groups);

            userName = navigationView.GetHeaderView(0).FindViewById<TextView>(Resource.Id.username_nav);
            userName.Text = PublicFields.Database.userName;
        }

        public bool OnNavigationItemSelected(IMenuItem menuItem)
        {
            Intent intent = null;
            switch (menuItem.ItemId)
            {
                case Resource.Id.nav_home:
                    drawerLayout.CloseDrawers();
                    Finish();
                    return true;
                case Resource.Id.shared_items:
                    intent = new Intent(this, typeof(SharedItemsActivity));
                    intent.AddFlags(ActivityFlags.ClearTop);
                    drawerLayout.CloseDrawers();
                    StartActivity(intent);
                    Finish();
                    return true;
                case Resource.Id.groups:
                    drawerLayout.CloseDrawers();
                    break;
            }

            return false;
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


    }
}