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
using Todo;
using Android.Support.V4.View;
using Android.Support.V7.App;
using Android.Support.V4.Widget;

namespace Cheesesquare
{
    [Activity(Label = "SharedItemsActivity")]
    public class SharedItemsActivity : AppCompatActivity
    {
        private ViewPager viewPager;
        private DrawerLayout drawerLayout;

        protected async override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // Create your application here
            SetContentView(Resource.Layout.activity_shared_items);

            viewPager = FindViewById<ViewPager>(Resource.Id.viewpager_shared_items);
            drawerLayout = FindViewById<DrawerLayout>(Resource.Id.drawer_layout_shared);
            var itemLinks = await PublicFields.Database.GetItemLinks();

            List<Item> sharedItems = new List<Item>();
            List<TreeNode<Item>> sharedItemsTreeNode = new List<TreeNode<Item>>();
            foreach (var itl in itemLinks)
            {
                Item it = await PublicFields.Database.GetItem(itl.ItemID);
                it.Parent = itl.Parent;
                sharedItems.Add(it);
                sharedItemsTreeNode.Add(new TreeNode<Item>(it));
            }

            Todo.TreeNode<Todo.Item> root = new TreeNode<Item>(null);
            root.Children.AddRange(sharedItemsTreeNode);

            var adapter = new CheeseDetailActivity.MyAdapter(SupportFragmentManager);

            //adapter.AddFragment(new CheeseListFragmentDetail(item, dataObserver), item.Value.Name);
            adapter.AddFragment(new CheeseListFragment(root), "Shared Items");
            viewPager.Adapter = adapter;

        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            switch (item.ItemId)
            {
                case Android.Resource.Id.Home:
                    //drawerLayout.OpenDrawer(Android.Support.V4.View.GravityCompat.Start);
                    Finish();
                    return true;
            }
            return base.OnOptionsItemSelected(item);
        }
    }
}