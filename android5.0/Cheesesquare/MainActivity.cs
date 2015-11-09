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
using Xamarin.Auth;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.WindowsAzure.MobileServices;
using System.Net.Http;

namespace Cheesesquare
{
    public static class Constants
    {
        // Azure app specific URL and key
        public const string ApplicationURL = @"https://mindunlimited.azure-mobile.net/";
        public const string ApplicationKey = @"RMFULNJBBVHwffaZeDYYhndAjEQzoT88";
    }

    [Activity (Name = "com.sample.cheesesquare.MainActivity", Label = "MindSet", MainLauncher = true)]
    public class MainActivity : AppCompatActivity, NavigationView.IOnNavigationItemSelectedListener
    {        
        DrawerLayout drawerLayout;
        private AccountStore accountStore;
        private MobileServiceUser mobileServiceUser;
        private MobileServiceClient client;
        private bool justAuthenticated;
        private Account currentAccount;

        //private async Task Authenticate()
        //{
        //    //mobServiceUser = Todo.App.Database.mobileServiceUser;
        //    accountStore = AccountStore.Create(this);
        //    currentAccount = null;
        //    bool useToken = true;

        //    try
        //    {
        //        var accounts = accountStore.FindAccountsForService("Microsoft").ToArray();
        //        // Log in 
        //        if (useToken)
        //        {
        //            if (accounts.Length != 0)
        //            {
        //                mobileServiceUser = new MobileServiceUser(accounts[0].Username);
        //                mobileServiceUser.MobileServiceAuthenticationToken = accounts[0].Properties["token"];

        //                client.CurrentUser = mobileServiceUser;
        //            }
        //        }
        //        if (mobileServiceUser != null) // Set the user from the stored credentials.
        //        {
        //            client.CurrentUser = mobileServiceUser;
        //            //App.MobileService.CurrentUser = user;

        //            try
        //            {
        //                // Try to return an item now to determine if the cached credential has expired.
        //                //var test = await Todo.App.Database.client.GetTable<Item>().Take(1).ToListAsync();
        //                var userInfo = await client.InvokeApiAsync("userInfo", HttpMethod.Get, null);

        //                //CreateAndShowDialog(string.Format("you are now logged in - {0}", Todo.App.Database.mobileServiceUser.UserId), "Logged in!");
        //                justAuthenticated = true;
        //                //currentAccount = accounts[0];

        //                //await Todo.App.Database.InitLocalStoreAsync();
        //                //await Todo.App.Database.newUser(Todo.App.Database.mobileServiceUser.UserId);
        //                //await Todo.App.Database.OnRefreshItemsSelected(); // pull database tables                 
        //            }
        //            catch (MobileServiceInvalidOperationException ex)
        //            {
        //                //System.Diagnostics.Debug.WriteLine(ex.InnerException.ToString());
        //                if (ex.Response.StatusCode == System.Net.HttpStatusCode.Unauthorized || ex.Response.StatusCode == System.Net.HttpStatusCode.InternalServerError)
        //                {
        //                    // Remove the credential with the expired token.
        //                    accountStore.Delete(accounts[0], "Microsoft");
        //                    mobileServiceUser = null;
        //                    client.CurrentUser = null;
        //                    justAuthenticated = false;
        //                }
        //            }
        //        }
        //        else // Regular login flow
        //        {
        //            //user = new MobileServiceuser( await client
        //            //    .LoginAsync(MobileServiceAuthenticationProvider.Facebook, token);
        //            //var token = new JObject();
        //            //// Replace access_token_value with actual value of your access token
        //            //token.Add("access_token", "access_token_value");

        //            mobileServiceUser = await client.LoginAsync(this, MobileServiceAuthenticationProvider.MicrosoftAccount);

        //            if (useToken)
        //            {
        //                // After logging in
        //                currentAccount = new Account(mobileServiceUser.UserId, new Dictionary<string, string> { { "token", mobileServiceUser.MobileServiceAuthenticationToken } });
        //                accountStore.Save(currentAccount, "Microsoft");
        //            }

        //            //CreateAndShowDialog(string.Format("you are now logged in - {0}", Todo.App.Database.mobileServiceUser.UserId), "Logged in!");
        //            justAuthenticated = true;

        //            //await Todo.App.Database.InitLocalStoreAsync();
        //            //await Todo.App.Database.newUser(Todo.App.Database.mobileServiceUser.UserId);
        //            //await Todo.App.Database.OnRefreshItemsSelected(); // pull database tables
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        //CreateAndShowDialog(ex, "Authentication failed");
        //    }


        //    //// Log out
        //    //client.Logout();
        //    //accountStore.Delete(account, "Facebook");
        //}

        private async Task Authenticate()
        {
            try
            {
                mobileServiceUser = await client.LoginAsync(this, MobileServiceAuthenticationProvider.MicrosoftAccount);
                //CreateAndShowDialog(string.Format("you are now logged in - {0}", user.UserId), "Logged in!");
            }
            catch (Exception ex)
            {
                Log.Debug("Main", ex.Message);//CreateAndShowDialog(ex, "Authentication failed");
            }
        }

        protected override async void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            SetContentView(Resource.Layout.activity_main);

            Log.Debug("CheeseDetailAcitivity", this.ComponentName.ToString());

            var toolbar = FindViewById<V7Toolbar>(Resource.Id.toolbar);
            SetSupportActionBar(toolbar);

            SupportActionBar.SetHomeAsUpIndicator(Resource.Drawable.ic_menu);
            SupportActionBar.SetDisplayHomeAsUpEnabled(true);

            drawerLayout = FindViewById<DrawerLayout>(Resource.Id.drawer_layout);

            var navigationView = FindViewById<NavigationView>(Resource.Id.nav_view);
            if (navigationView != null)
                setupDrawerContent(navigationView);

            var viewPager = FindViewById<Android.Support.V4.View.ViewPager>(Resource.Id.viewpager);
            if (viewPager != null)
                setupViewPager(viewPager);

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

            var tabLayout = FindViewById<TabLayout>(Resource.Id.tabs);
            tabLayout.SetupWithViewPager(viewPager);

            // Initialize the progress bar
            //requestWindowFeature(Window.FEATURE_INDETERMINATE_PROGRESS);
            SetProgressBarIndeterminateVisibility(true);
            //progressBar = FindViewById<ProgressBar>(Resource.Id.loadingProgressBar);
            //progressBar.Visibility = ViewStates.Gone;

            //// Create ProgressFilter to handle busy state
            //var progressHandler = new ProgressHandler();
            //progressHandler.BusyStateChange += (busy) => {
            //    if (progressBar != null)
            //        progressBar.Visibility = busy ? ViewStates.Visible : ViewStates.Gone;
            //};

            try
            {
                CurrentPlatform.Init();
                // Create the Mobile Service Client instance, using the provided
                // Mobile Service URL and key
                client = new MobileServiceClient(
                    Constants.ApplicationURL,
                    Constants.ApplicationKey);//, progressHandler);

                await Authenticate();
                //await CreateTable();
            }
            catch (Java.Net.MalformedURLException)
            {
                //CreateAndShowDialog(new Exception("There was an error creating the Mobile Service. Verify the URL"), "Error");
            }
            catch (Exception e)
            {
                //CreateAndShowDialog(e, "Error");
            }
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
                    drawerLayout.CloseDrawers();
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


