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
    [Activity(Label = "SharedItemsActivity")]
    public class SharedItemsActivity : AppCompatActivity, NavigationView.IOnNavigationItemSelectedListener
    {
        private ViewPager viewPager;
        private DrawerLayout drawerLayout;
        private NavigationView navigationView;
        private TextView userName;

        protected async override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // Create your application here
            SetContentView(Resource.Layout.activity_shared_items);

            var toolbar = FindViewById<V7Toolbar>(Resource.Id.toolbar);
            SetSupportActionBar(toolbar);

            SupportActionBar.SetHomeAsUpIndicator(Resource.Drawable.ic_menu);
            SupportActionBar.SetDisplayHomeAsUpEnabled(true);
            SupportActionBar.Title = "Shared with me";

            viewPager = FindViewById<ViewPager>(Resource.Id.viewpager_shared_items);
            drawerLayout = FindViewById<DrawerLayout>(Resource.Id.drawer_layout_shared);
            var itemLinks = await PublicFields.Database.GetItemLinks();
            itemLinks = itemLinks.Where(x => x.OwnedBy.ToLower() == PublicFields.Database.defGroup.ID.ToLower()); // filter so that we only see the ones we own not the ones we are part of the group

            List<Item> sharedItems = new List<Item>();
            List<TreeNode<Item>> sharedItemsTreeNode = new List<TreeNode<Item>>();
            foreach (var itl in itemLinks)
            {
                Item it = await PublicFields.Database.GetItem(itl.ItemID);
                it.SharedLink = itl;

                if (itl.Parent == null) // to be assigned to a parent so shows up in list
                {
                    it.Parent = itl.Parent;
                    sharedItems.Add(it);
                    sharedItemsTreeNode.Add(new TreeNode<Item>(it));
                }
            }

            Todo.TreeNode<Todo.Item> root = new TreeNode<Item>(null);
            root.Children.AddRange(sharedItemsTreeNode);

            var adapter = new CheeseDetailActivity.MyAdapter(SupportFragmentManager);

            //adapter.AddFragment(new CheeseListFragmentDetail(item, dataObserver), item.Value.Name);
            adapter.AddFragment(new CheeseListFragmentSharedItems(root), "Shared Items");
            viewPager.Adapter = adapter;

            navigationView = FindViewById<NavigationView>(Resource.Id.nav_view_shared);
            navigationView.SetNavigationItemSelectedListener(this);

            navigationView.SetCheckedItem(Resource.Id.shared_items);

            userName = navigationView.GetHeaderView(0).FindViewById<TextView>(Resource.Id.username_nav);
            userName.Text = PublicFields.Database.userName;
        }

        public bool OnNavigationItemSelected(IMenuItem menuItem)
        {
            switch (menuItem.ItemId)
            {
                case Resource.Id.nav_home:
                    Finish();
                    drawerLayout.CloseDrawers();
                    return true;
                case Resource.Id.shared_items:
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