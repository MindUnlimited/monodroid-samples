using System;

using Android.App;
using Android.Content;
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
using Android.Util;
using Android.Support.V4.View;
using Microsoft.WindowsAzure.MobileServices;
using Android.Preferences;
using Newtonsoft.Json;
using System.Linq;
using Todo;
using MindSet.Models;
using Gcm.Client;
using Android.Content.PM;
using Android.Database;
using Android.Provider;
using Android.Graphics;

namespace MindSet
{
    [Activity(Name = "com.sample.mindset.MainActivity", Label = "MindSet", MainLauncher = true, ScreenOrientation = ScreenOrientation.Portrait, LaunchMode = LaunchMode.SingleTop)]
    public class MainActivity : AppCompatActivity, NavigationView.IOnNavigationItemSelectedListener
    {
        public static MainActivity instance;

        FloatingActionButton FAB;
        DrawerLayout drawerLayout;
        ViewPager viewPager;
        TabLayout tabLayout;
        NavigationView navigationView;
        TextView userName;

        private const int SHARE_CONTACT = 101;
        private const int LOGIN = 102;
        private const int ITEMDETAIL = 103;
        private const int EDIT_ITEM = 104;
        private const int PICKIMAGE = 105;

        private ListFragment currentDomainFragment;
        //private RecyclerView.AdapterDataObserver dataObserver;

        private void RegisterWithGCM()
        {
            // Check to ensure everything's set up right
            GcmClient.CheckDevice(this);
            GcmClient.CheckManifest(this);

            // Register for push notifications
            Log.Info("MainActivity", "Registering...");
            GcmClient.Register(this, Constants.SenderID);
        }

        protected async override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            instance = this;

            PublicFields.Database = new Database();
            //dataObserver = new DataObserver();

            SetContentView(Resource.Layout.activity_main);

            var toolbar = FindViewById<V7Toolbar>(Resource.Id.toolbar);
            SetSupportActionBar(toolbar);

            SupportActionBar.SetHomeAsUpIndicator(Resource.Drawable.ic_menu);
            SupportActionBar.SetDisplayHomeAsUpEnabled(true);

            drawerLayout = FindViewById<DrawerLayout>(Resource.Id.drawer_layout);

            navigationView = FindViewById<NavigationView>(Resource.Id.nav_view_main);
            if (navigationView != null)
                setupDrawerContent(navigationView);

            userName = navigationView.GetHeaderView(0).FindViewById<TextView>(Resource.Id.username_nav);

            FAB = FindViewById<FloatingActionButton>(Resource.Id.fab);
            FAB.Click += (sender, e) =>
            {
                var intent = new Intent(this, typeof(EditItemActivity));
                intent.PutExtra("newItem", true);
                intent.PutExtra("newItem", true);
                intent.PutExtra("parentItemID", currentDomainFragment.domain.Value.id);

                StartActivityForResult(intent, EDIT_ITEM);
            };

            viewPager = FindViewById<ViewPager>(Resource.Id.viewpager);
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

            if(viewPager != null)
            {
                var adapter = (MyAdapter)viewPager.Adapter;
                int index = viewPager.CurrentItem;
                if (adapter != null && index >= 0)
                {
                    var currentFragment = (ListFragment)adapter.GetItem(index);
                    var fragmentAdapter = currentFragment?.itemRecyclerViewAdapter;

                    if (fragmentAdapter != null)
                    {
                        var domain = currentFragment.domain;

                        fragmentAdapter.ChangeDateSet(domain.Children);
                        fragmentAdapter.NotifyDataSetChanged();
                    }
                }
            }


            if (navigationView != null)
                navigationView.SetCheckedItem(Resource.Id.nav_home);

            DebugSharedItemRetrievalProblems();

            //else
            //{
            //    var loginpage = new NavigationPage(new Views.SelectLoginProviderPage());
            //    await App.Navigation.PushModalAsync(loginpage);
            //}
            //await Todo.App.selectedDomainPage.Refresh();

            //RequestedOrientation = ScreenOrientation.Portrait;
        }

        public void DebugSharedItemRetrievalProblems()
        {
            //var allItems = PublicFields.Database.GetItems();
            //var allItemsResult = allItems.Result;

            //var test = PublicFields.Database.GetItem("2cb157ed-f525-4b19-b378-1963a257056e"); // shared item
            //var result = test.Result;

            //var testCapitalized = PublicFields.Database.GetItem("2cb157ed-f525-4b19-b378-1963a257056e".ToUpper()); // shared item
            //var resultCapitalized = test.Result;

            //var test2 = PublicFields.Database.GetItem("ea9ff1e8-23e8-4e2f-b613-55430e2684b0"); // my own item (google account)
            //var result2 = test2.Result;
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

        /**
         * Get a file path from a Uri. This will get the the path for Storage Access
         * Framework Documents, as well as the _data field for the MediaStore and
         * other file-based ContentProviders.
         *
         * @param context The context.
         * @param uri The Uri to query.
         * @author paulburke
         */
        public static String GetPath(Context context, Android.Net.Uri uri)
        {

            bool isKitKat = Build.VERSION.SdkInt >= Android.OS.BuildVersionCodes.Kitkat;

            // DocumentProvider
            if (isKitKat && DocumentsContract.IsDocumentUri(context, uri))
            {
                // ExternalStorageProvider
                if (isExternalStorageDocument(uri))
                {
                    String docId = DocumentsContract.GetDocumentId(uri);
                    String[] split = docId.Split(':');
                    String type = split[0];

                    if ("primary" == type.ToLower())
                    {
                        return Android.OS.Environment.ExternalStorageDirectory + "/" + split[1];
                    }

                    // TODO handle non-primary volumes
                }
                // DownloadsProvider
                else if (isDownloadsDocument(uri))
                {

                    String id = DocumentsContract.GetDocumentId(uri);
                    Android.Net.Uri contentUri = ContentUris.WithAppendedId(
                            Android.Net.Uri.Parse("content://downloads/public_downloads"), long.Parse(id));

                    return getDataColumn(context, contentUri, null, null);
                }
                // MediaProvider
                else if (isMediaDocument(uri))
                {
                    String docId = DocumentsContract.GetDocumentId(uri);
                    String[] split = docId.Split(':');
                    String type = split[0];

                    Android.Net.Uri contentUri = null;
                    if ("image".Equals(type))
                    {
                        contentUri = MediaStore.Images.Media.ExternalContentUri;
                    }
                    else if ("video".Equals(type))
                    {
                        contentUri = MediaStore.Video.Media.ExternalContentUri;
                    }
                    else if ("audio".Equals(type))
                    {
                        contentUri = MediaStore.Audio.Media.ExternalContentUri;
                    }

                    String selection = "_id=?";
                    String[] selectionArgs = new String[] {
                    split[1]
            };

                    return getDataColumn(context, contentUri, selection, selectionArgs);
                }
            }
            // MediaStore (and general)
            else if ("content".Equals(uri.Scheme.ToLower()))
            {
                return getDataColumn(context, uri, null, null);
            }
            // File
            else if ("file".Equals(uri.Scheme.ToLower()))
            {
                return uri.Path;
            }

            return null;
        }

        /**
         * Get the value of the data column for this Uri. This is useful for
         * MediaStore Uris, and other file-based ContentProviders.
         *
         * @param context The context.
         * @param uri The Uri to query.
         * @param selection (Optional) Filter used in the query.
         * @param selectionArgs (Optional) Selection arguments used in the query.
         * @return The value of the _data column, which is typically a file path.
         */
        public static String getDataColumn(Context context, Android.Net.Uri uri, String selection,
                String[] selectionArgs)
        {

            ICursor cursor = null;
            String column = "_data";
            String[] projection = {
                column
            };

            try
            {
                cursor = context.ContentResolver.Query(uri, projection, selection, selectionArgs,
                        null);
                if (cursor != null && cursor.MoveToFirst())
                {
                    int column_index = cursor.GetColumnIndexOrThrow(column);
                    return cursor.GetString(column_index);
                }
            }
            finally
            {
                if (cursor != null)
                    cursor.Close();
            }
            return null;
        }


        /**
         * @param uri The Uri to check.
         * @return Whether the Uri authority is ExternalStorageProvider.
         */
        public static bool isExternalStorageDocument(Android.Net.Uri uri)
        {
            return "com.android.externalstorage.documents".Equals(uri.Authority);
        }

        /**
         * @param uri The Uri to check.
         * @return Whether the Uri authority is DownloadsProvider.
         */
        public static bool isDownloadsDocument(Android.Net.Uri uri)
        {
            return "com.android.providers.downloads.documents".Equals(uri.Authority);
        }

        /**
         * @param uri The Uri to check.
         * @return Whether the Uri authority is MediaProvider.
         */
        public static bool isMediaDocument(Android.Net.Uri uri)
        {
            return "com.android.providers.media.documents".Equals(uri.Authority);
        }

        protected async override void OnActivityResult(int requestCode, Result resultCode,
        Intent intent)
        {
            base.OnActivityResult(requestCode, resultCode, intent);

            if (resultCode == Result.Ok)
            {
                List<Todo.User> selectedContacts = new List<User>();
                string groupName = null;

                switch (requestCode)
                {
                    case LOGIN:
                        await PublicFields.Database.SyncAsync();

                        if (viewPager != null)
                            setupViewPager(viewPager);

                        tabLayout.SetupWithViewPager(viewPager);

                        if(userName!=null)
                            userName.Text = PublicFields.Database.userName;

                        RegisterWithGCM();

                        break;
                    case PICKIMAGE:
                        if (intent != null)
                        {

                            

                            Android.Net.Uri uri = intent.Data;
                            string ItemID = intent.GetStringExtra(DetailActivity.ITEM_ID);
                            string path = intent.GetStringExtra("path");
                            int resourceID = intent.GetIntExtra("resourceID", 0);
                            int resourceBackdropID = intent.GetIntExtra("resourceBackdropID", 0);

                            if (resourceID != 0)
                            {
                                int index = viewPager.CurrentItem;
                                var adapter = (MyAdapter)viewPager.Adapter;
                                var currentFragment = (ListFragment)adapter.GetItem(index);
                                var fragmentAdapter = currentFragment.itemRecyclerViewAdapter;

                                var item = PublicFields.ItemTree.Descendants().FirstOrDefault(node => node.Value.id == ItemID);
                                item.Value.ImageResource = resourceID;
                                item.Value.ImageResourceBackdrop = resourceBackdropID;

                                switch (resourceID)
                                {
                                    case 1:
                                        //domain
                                        break;
                                    case Resource.Drawable.Goal256:
                                        // goal
                                        item.Value.ResourceUrl = "Goal";
                                        break;
                                    case Resource.Drawable.Project256:
                                        // project
                                        item.Value.ResourceUrl = "Project";
                                        break;
                                    case Resource.Drawable.Task256:
                                        // task
                                        item.Value.ResourceUrl = "Task";
                                        break;
                                    default:
                                        // handled same as task
                                        item.Value.ResourceUrl = "";
                                        break;
                                }

                                fragmentAdapter.UpdateValue(item);
                                fragmentAdapter.ApplyChanges();

                                await PublicFields.Database.SaveItem(item.Value);
                            }
                            else
                            {
                                int index = viewPager.CurrentItem;
                                var adapter = (MyAdapter)viewPager.Adapter;
                                var currentFragment = (ListFragment)adapter.GetItem(index);
                                var fragmentAdapter = currentFragment.itemRecyclerViewAdapter;

                                var item = PublicFields.ItemTree.Descendants().FirstOrDefault(node => node.Value.id == ItemID);
                                item.Value.ImagePath = path;

                                fragmentAdapter.UpdateValue(item);
                                fragmentAdapter.ApplyChanges();
                            }
                        }
                        break;
                    case
                        ITEMDETAIL:
                        if (intent.GetBooleanExtra("databaseUpdated", false))
                        {
                            PublicFields.UpdateDatabase();
                            PublicFields.MakeTree();

                            int currentIndex = viewPager.CurrentItem;
                            var adapter = (MyAdapter)viewPager.Adapter;
                            var currentFragment = (ListFragment)adapter.GetItem(currentIndex);

                            int indexes = adapter.Count;
                            for (int i = 0; i < indexes; i++)
                            {
                                var domainFragment = (ListFragment)adapter.GetItem(i);
                                var domainItem = PublicFields.ItemTree.Descendants().FirstOrDefault(node => node.Value.id == domainFragment.domain.Value.id);
                                if (domainItem.Children != null && domainFragment.itemRecyclerViewAdapter != null)
                                {
                                    domainFragment.domain = domainItem;
                                    domainFragment.itemRecyclerViewAdapter.ChangeDateSet(domainItem.Children);
                                    domainFragment.itemRecyclerViewAdapter.NotifyDataSetChanged();
                                }
                            }

                            viewPager.PageSelected += ViewPager_PageSelected;
                            viewPager.CurrentItem = currentIndex;
                        }
                        else if (intent.GetBooleanExtra("itemChanged", false))
                        {
                            string ItemID = intent.GetStringExtra("itemID");
                            Log.Debug("MainActivity", "Item changed");
                            int index = viewPager.CurrentItem;
                            var adapter = (MyAdapter)viewPager.Adapter;
                            var currentFragment = (ListFragment) adapter.GetItem(index);
                            var fragmentAdapter = currentFragment.itemRecyclerViewAdapter;

                            var item = PublicFields.ItemTree.Descendants().FirstOrDefault(node => node.Value.id == ItemID);

                            fragmentAdapter.UpdateValue(item);
                            fragmentAdapter.ApplyChanges();
                        }
                            
                        break;

                    case EDIT_ITEM:// new item
                        var itemID = intent.GetStringExtra("itemID");
                        bool groupChanged = intent.GetBooleanExtra("groupChanged", false);
                        var tempID = "temporaryID";

                        TreeNode<Item> newItem;
                        if (itemID == tempID)
                        {
                            var test = PublicFields.ItemTree.Descendants().ToList();
                            newItem = PublicFields.ItemTree.Descendants().FirstOrDefault(node => node.Value.id == itemID);

                            if (newItem == null)
                            {
                                //PublicFields.UpdateDatabase(); don't want it to push the item with a temporary id
                                PublicFields.MakeTree();

                                newItem = PublicFields.ItemTree.Descendants().FirstOrDefault(node => node.Value.id == itemID);

                                // still null
                                if(newItem == null)
                                {
                                    Log.Error("Main", string.Format("can not find the item with id: {0}", itemID));
                                }
                                else
                                {
                                    newItem.Value.id = null;

                                    await PublicFields.Database.SaveItem(newItem.Value);

                                    itemID = newItem.Value.id; // the newly acquired item id

                                    // check if the subitems of the new card are new as well, if so save them
                                    for (int i = 0; i < newItem.Children.Count; i++)
                                    {
                                        var it = newItem.Children[i];
                                        it.Value.Parent = itemID; // change the parent id to the new one

                                        if (string.IsNullOrEmpty(it.Value.id))
                                            await PublicFields.Database.SaveItem(it.Value);

                                        newItem.Children[i] = it; // store with newly acquired id
                                    }
                                }
                            }
                            else
                            {
                                newItem.Value.id = null;

                                await PublicFields.Database.SaveItem(newItem.Value);

                                itemID = newItem.Value.id; // the newly acquired item id

                                // check if the subitems of the new card are new as well, if so save them
                                for (int i = 0; i < newItem.Children.Count; i++)
                                {
                                    var it = newItem.Children[i];
                                    it.Value.Parent = itemID; // change the parent id to the new one

                                    if (string.IsNullOrEmpty(it.Value.id))
                                        await PublicFields.Database.SaveItem(it.Value);

                                    newItem.Children[i] = it; // store with newly acquired id
                                }
                            }
                        }


                        if (groupChanged) // sharing an item
                        {
                            selectedContacts = JsonConvert.DeserializeObject<List<Todo.User>>(intent.GetStringExtra("selectedContacts"));
                            groupName = intent.GetStringExtra("groupName"); // only has a name if user generated group
                            //TODO: check if group already exists

                            var editedToBeShared = PublicFields.ItemTree.Descendants().FirstOrDefault(node => node.Value.id == itemID);
                            if (selectedContacts != null && selectedContacts.Count > 0) // contacts selected so make a group for them
                            {
                                var ownedByGroupResult = await PublicFields.Database.SaveGroup(selectedContacts, groupName);
                                if (ownedByGroupResult != null)
                                {
                                    editedToBeShared.Value.OwnedBy = ownedByGroupResult.id;
                                    PublicFields.ItemTree.FindAndReplace(editedToBeShared.Value.id, editedToBeShared);
                                    await PublicFields.Database.SaveItem(editedToBeShared.Value); // update the item with the new ownedBy group
                                }

                                var groupMembers = await PublicFields.Database.MembersOfGroup(ownedByGroupResult);

                                foreach (var grp in groupMembers)
                                {
                                    if (grp != null)
                                    {
                                        var link = new ItemLink { ItemID = editedToBeShared.Value.id, Parent = null, OwnedBy = grp.id };
                                        if (grp.id == PublicFields.Database.defGroup.id) // own item
                                        {
                                            link.Parent = editedToBeShared.Value.Parent;
                                        }
                                        await PublicFields.Database.SaveItemLink(link);
                                    }
                                }
                            }



                        }
                        //newItem = PublicFields.ItemTree.Descendants().FirstOrDefault(node => node.Value.id == itemID);

                        //currentDomainFragment.itemRecyclerViewAdapter.NotifyItemInserted(currentDomainFragment.itemRecyclerViewAdapter.ItemCount);


                        PublicFields.UpdateDatabase();
                        PublicFields.MakeTree();

                        var domain = PublicFields.ItemTree.Descendants().FirstOrDefault(node => node.Value.id == currentDomainFragment.domain.Value.id);
                        currentDomainFragment.itemRecyclerViewAdapter.ChangeDateSet(domain.Children);
                        //currentDomainFragment.itemRecyclerViewAdapter.NotifyDataSetChanged();
                        //currentDomainFragment.itemRecyclerViewAdapter.AddItem(newItem);
                        //currentDomainFragment.itemRecyclerViewAdapter.ApplyChanges();

                        break;

                    case SHARE_CONTACT:
                        var sharedItemID = intent.GetStringExtra("itemID");
                        groupName = null;
                        selectedContacts = new List<User>();
                        User selectedContact = null;

                        if (intent.GetStringExtra("members") != null)
                        {
                            var members = intent.GetStringExtra("members");
                            groupName = intent.GetStringExtra("groupname");
                            selectedContacts = JsonConvert.DeserializeObject<List<Todo.User>>(members);
                            selectedContacts = CircleBitmap.addPhotoThumbs(selectedContacts); // add all the photo thumbnails to the contacts
                        }
                        else if (intent.GetStringExtra("member") != null)
                        {
                            var member = intent.GetStringExtra("member");
                            selectedContact = JsonConvert.DeserializeObject<Todo.User>(member);
                            selectedContact = CircleBitmap.addPhotoThumbs(selectedContact); // add the photo thumbnails to the contact
                        }

                        if (selectedContacts != null && selectedContacts.Count >= 2) // need at least two other users to make a group
                        {
                            if (selectedContacts.Count > 0)
                            {
                                // add the current user to the group if not already in there
                                if (selectedContacts.Find(ct => ct.Email == PublicFields.Database.defUser.Email) == null)
                                    selectedContacts.Insert(0, PublicFields.Database.defUser);
                                else //replace the current user with the selected user if the email address is the same
                                {
                                    var index = selectedContacts.FindIndex(ct => ct.Email == PublicFields.Database.defUser.Email);
                                    selectedContacts[index] = PublicFields.Database.defUser;
                                }
                            }
                        }
                        else if (selectedContact != null) // invisible group containing only two members (selected user and self)
                        {
                            selectedContacts.Clear();
                            selectedContacts.Add(PublicFields.Database.defUser);
                            selectedContacts.Add(selectedContact);
                        }

                        var parent = PublicFields.ItemTree.Descendants().FirstOrDefault(it => it.Value.id == sharedItemID);


                        var newShareItem = PublicFields.ItemTree.Descendants().FirstOrDefault(node => node.Value.id == sharedItemID);
                        if (selectedContacts != null && selectedContacts.Count >= 2) // contacts selected so make a group for them
                        {
                            var ownedByGroupResult = await PublicFields.Database.SaveGroup(selectedContacts, groupName);
                            if (ownedByGroupResult != null)
                            {
                                newShareItem.Value.OwnedBy = ownedByGroupResult.id;
                                PublicFields.ItemTree.FindAndReplace(newShareItem.Value.id, newShareItem);
                                await PublicFields.Database.SaveItem(newShareItem.Value); // update the item with the new ownedBy group
                            }

                            var groupMembers = await PublicFields.Database.MembersOfGroup(ownedByGroupResult);

                            foreach (var grp in groupMembers)
                            {
                                if (grp != null)
                                {
                                    var link = new ItemLink { ItemID = newShareItem.Value.id, Parent = null, OwnedBy = grp.id };
                                    if (grp.id == PublicFields.Database.defGroup.id) // own item
                                    {
                                        link.Parent = newShareItem.Value.Parent;
                                    }
                                    await PublicFields.Database.SaveItemLink(link);
                                }
                            }
                        }

                        var sharedItemDomain = PublicFields.ItemTree.Descendants().FirstOrDefault(node => node.Value.id == currentDomainFragment.domain.Value.id);
                        currentDomainFragment.itemRecyclerViewAdapter.ChangeDateSet(sharedItemDomain.Children);

                        var sharedItemAdapter = currentDomainFragment.itemRecyclerViewAdapter;
                        var sharedItem = PublicFields.ItemTree.Descendants().FirstOrDefault(node => node.Value.id == newShareItem.Value.id);
                        sharedItemAdapter.UpdateValue(sharedItem);
                        sharedItemAdapter.ApplyChanges();

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
                case Resource.Id.refresh:
                    Log.Debug("main", "attempting refresh");

                    // update db
                    //Task.Run(async () => await PublicFields.Database.SyncAsync());
                    PublicFields.UpdateDatabase();
                    PublicFields.MakeTree();

                    int currentIndex = viewPager.CurrentItem;
                    var adapter = (MyAdapter)viewPager.Adapter;
                    var currentFragment = (ListFragment)adapter.GetItem(currentIndex);

                    int indexes = adapter.Count;
                    for (int i = 0; i < indexes; i++)
                    {
                        var domainFragment = (ListFragment)adapter.GetItem(i);
                        var domain = PublicFields.ItemTree.Descendants().FirstOrDefault(node => node.Value.id == domainFragment.domain.Value.id);
                        if (domain.Children != null && domainFragment.itemRecyclerViewAdapter != null)
                        {
                            domainFragment.domain = domain;
                            domainFragment.itemRecyclerViewAdapter.ChangeDateSet(domain.Children);
                            domainFragment.itemRecyclerViewAdapter.NotifyDataSetChanged();
                        }
                    }

                    viewPager.PageSelected += ViewPager_PageSelected;
                    viewPager.CurrentItem = currentIndex;

                    return true;
            }
            return base.OnOptionsItemSelected(item);
        }

        //public void AddSubTasks(List<Todo.Item> items)
        //{
        //    IEnumerable<string> groups_ids = from grp in PublicFields.Database.userGroups select grp.ID;
        //    foreach(Todo.Item item in items)
        //    {
        //        var directSubItems = PublicFields.allItems.Where(it.Value.Parent != null && it.Value.Parent == item.id).ToList();
        //        //var directSubItems = (from it in PublicFields.allItems where groups_ids.Contains(it.OwnedBy) && it.Parent != null && it.Parent == item.ID select it).ToList<Todo.Item>();
        //        item.SubItems = directSubItems;

        //        //if (directSubItems != null && directSubItems.Count != 0)
        //        //    AddSubTasks(item.SubItems);
        //    }
        //}


        public void setupViewPager(Android.Support.V4.View.ViewPager viewPager)
        {
            PublicFields.MakeTree();
            
            var adapter = new MyAdapter(SupportFragmentManager);

            foreach (TreeNode<Item> domain in PublicFields.domains)
            {
                adapter.AddFragment(new ListFragment(domain), domain.Value.Name);
            }

            viewPager.Adapter = adapter;
            adapter.NotifyDataSetChanged();
            currentDomainFragment = ((ListFragment)((MyAdapter)viewPager.Adapter).GetItem(viewPager.CurrentItem));

            viewPager.PageSelected += ViewPager_PageSelected;
        }

        private void ViewPager_PageSelected(object sender, ViewPager.PageSelectedEventArgs e)
        {
            var vp = (ViewPager) sender;
            currentDomainFragment = ((ListFragment)((MyAdapter)vp.Adapter).GetItem(vp.CurrentItem));
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
            Intent intent = null;
            switch (menuItem.ItemId)
            {
                case Resource.Id.nav_home:
                    drawerLayout.CloseDrawers();
                    return true;
                case Resource.Id.shared_items:
                    //NavUtils.NavigateUpTo
                    intent = new Intent(this, typeof(SharedItemsActivity));
                    intent.AddFlags(ActivityFlags.ClearTop);
                    drawerLayout.CloseDrawers();
                    StartActivity(intent);
                    return true;
                case Resource.Id.groups:
                    intent = new Intent(this, typeof(GroupsActivity));
                    intent.AddFlags(ActivityFlags.ClearTop);
                    drawerLayout.CloseDrawers();
                    StartActivity(intent);
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
                if (fragments.Count > position)
                    return fragments[position];
                else
                    return null;
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


