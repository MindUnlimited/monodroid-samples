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
using Android.Util;
using Microsoft.WindowsAzure.MobileServices;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using Xamarin.Auth;
using Android.Preferences;
using Todo;
using System.Net.Http;
using Newtonsoft.Json;

namespace Cheesesquare
{
    [Activity(Label = "LoginActivity")]
    public class LoginActivity : Activity
    {
        private Button facebookButton;
        private Button googleButton;
        private Button microsoftButton;

        private MobileServiceAuthenticationProvider provider;

        protected override async void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            var providerIntent = Intent.GetStringExtra("provider");

            if (providerIntent != null)
            {
                provider = JsonConvert.DeserializeObject<MobileServiceAuthenticationProvider>(providerIntent);
                if (providerIntent != null)
                {
                    await Authenticate(provider);

                    Intent resultIntent = new Intent();
                    SetResult(Result.Ok, resultIntent);
                    Finish();
                }
            }
            else
            {
                SetContentView(Resource.Layout.activity_login);

                facebookButton = FindViewById<Button>(Resource.Id.facebook_button);
                facebookButton.Click += FacebookButton_Click;
                googleButton = FindViewById<Button>(Resource.Id.google_button);
                googleButton.Click += GoogleButton_Click;
                microsoftButton = FindViewById<Button>(Resource.Id.microsoft_button);
                microsoftButton.Click += MicrosoftButton_Click;
            }
        }

        public async Task Authenticate(MobileServiceAuthenticationProvider provider)
        {
            string message;
            string providerName = provider.ToString();

            // Authorization credential.
            MobileServiceUser user = null;

            var accountStore = AccountStore.Create(Application.Context); // Xamarin.Android

            // Sharedpreferences (local storage) for storing the last used oauth provider
            var preferences = PreferenceManager.GetDefaultSharedPreferences(Application.Context);
            var editor = preferences.Edit();

            while (user == null)
            {
                Account accountFound = accountStore.FindAccountsForService(providerName).FirstOrDefault();
                // Try to get an existing encrypted credential from isolated storage.        
                if (accountFound != null)
                {
                    user = new MobileServiceUser(accountFound.Username);
                    user.MobileServiceAuthenticationToken = accountFound.Properties["token"];
                }
                if (user != null)
                {
                    // Set the user from the stored credentials.
                    PublicFields.Database.client.CurrentUser = user;

                    //Todo.App.Current.MainPage.IsBusy = true;

                    try
                    {
                        // Try to return an item now to determine if the cached credential has expired.
                        await PublicFields.Database.client.GetTable<Item>().Take(1).ToListAsync();
                        JToken userInfo = await PublicFields.Database.client.InvokeApiAsync("userInfo", HttpMethod.Get, null); // also gather extra user information

                        JObject response = (JObject)await PublicFields.Database.client.InvokeApiAsync("getcontacts", HttpMethod.Get, null);
                        List<Todo.Models.Contact> contactList = new List<Todo.Models.Contact>();
                        if (provider == MobileServiceAuthenticationProvider.MicrosoftAccount)
                        {
                            foreach (JObject usr in response["data"])
                            {
                                string name = usr["name"].ToString();
                                string id = usr["id"].ToString();
                                var pictureUrl = string.Format("https://apis.live.net/v5.0/{0}/picture", usr["id"]);

                                contactList.Add(new Todo.Models.Contact { Id = id, Name = name, PictureUrl = pictureUrl });
                            }
                        }
                        else if (provider == MobileServiceAuthenticationProvider.Google)
                        {
                            JToken identity = await PublicFields.Database.client.InvokeApiAsync("getIdentities", HttpMethod.Get, null);
                            string accessToken = identity["google"]["accessToken"].ToString();

                            foreach (JObject usr in response["feed"]["entry"])
                            {
                                string name = usr["title"]["$t"].ToString();
                                string id = usr["id"]["$t"].ToString().Split('/').Last();
                                var pictureUrl = string.Format("https://www.google.com/m8/feeds/photos/media/default/{0}?access_token={1}", id, accessToken); // may not exist

                                contactList.Add(new Todo.Models.Contact { Id = id, Name = name, PictureUrl = pictureUrl });
                            }
                        }
                        else if (provider == MobileServiceAuthenticationProvider.Facebook)
                        {
                            JToken identity = await PublicFields.Database.client.InvokeApiAsync("getIdentities", HttpMethod.Get, null);
                            string accessToken = identity["facebook"]["accessToken"].ToString();

                            JArray contacts = (JArray)response["data"];
                            for (int i = 0; i < contacts.Count; i++)
                            {
                                JObject contact = (JObject)contacts.ElementAt(i);
                                string name = contact["name"].ToString();
                                string id = contact["id"].ToString();
                                //var pictureUrl = string.Format(contact["picture"]["data"]["url"].ToString() + "?access_token={0}", accessToken);
                                var pictureUrl = String.Format("https://graph.facebook.com/{0}/picture?type=large&access_token={1}", id, accessToken);

                                contactList.Add(new Todo.Models.Contact { Id = id, Name = name, PictureUrl = pictureUrl });
                            }
                        }
                    }
                    catch (MobileServiceInvalidOperationException ex)
                    {
                        if (ex.Response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                        {
                            // Remove the credential with the expired token.
                            accountStore.Delete(accountFound, providerName);
                            user = null;
                            continue;
                        }
                    }

                    //Todo.App.Current.MainPage.IsBusy = false;
                }
                else
                {
                    try
                    {
                        // Login with the identity provider.
                        user = await PublicFields.Database.client
                            .LoginAsync(this, provider);

                        // Store the encrypted user credentials in local settings.
                        Account currentAccount = new Account(user.UserId, new Dictionary<string, string> { { "token", user.MobileServiceAuthenticationToken } });
                        accountStore.Save(currentAccount, providerName);
                    }
                    catch (MobileServiceInvalidOperationException ex)
                    {
                        message = "You must log in. Login Required";
                    }
                    catch (Exception ex)
                    {
                        if (ex.Message.Contains("Authentication was cancelled by the user"))
                        {
                            // user probably pushed back button, return to select login page
                            //await Todo.App.Navigation.PushModalAsync(new Views.SelectLoginProviderPage());
                            return;
                        }
                    }
                }

                //Todo.App.Current.MainPage.IsBusy = true;

                // add last user provider, so that you don't have to click on which login provider you want to use
                editor.PutString("LastUsedProvider", providerName);
                editor.Apply();

                PublicFields.Database.mobileServiceUser = user;
                PublicFields.Database.userID = user.UserId;

                //JToken contacts2 = await App.Database.client.InvokeApiAsync("getContacts", HttpMethod.Get, null); // also gather extra user information
                //Debug.WriteLine(contacts2.ToString());


                //RetrieveFriends.retrieveFacebookFriends(user.MobileServiceAuthenticationToken, (string)userInfo2["name"]);

                await PublicFields.Database.InitLocalStoreAsync();
                await PublicFields.Database.newUser(PublicFields.Database.mobileServiceUser.UserId, provider);
                await PublicFields.Database.SyncAsync(); // pull database tables


                var parameters = new Dictionary<string, string>
                {
                    { "userid", "96090072-0234-4570-898a-5e9ec9d398c3" }
                };

                JToken defGroup = await PublicFields.Database.client.InvokeApiAsync("getdefaultgroup", HttpMethod.Get, parameters); // also gather extra user information
                //await PublicFields.Database.getContactsThatUseApp();

                message = string.Format("You are now logged in - {0}", user.UserId);
                //Debug.WriteLine(message);
                //MessageBox.Show(message);

                //Todo.App.Current.MainPage.IsBusy = false;
            }
        }

        private async void MicrosoftButton_Click(object sender, EventArgs e)
        {
            Log.Debug("LoginActivity", "Microsoft");
            await Authenticate(MobileServiceAuthenticationProvider.MicrosoftAccount);
            Intent resultIntent = new Intent();
            SetResult(Result.Ok, resultIntent);
            Finish();
        }

        private async void GoogleButton_Click(object sender, EventArgs e)
        {
            Log.Debug("LoginActivity", "Google");
            await Authenticate(MobileServiceAuthenticationProvider.Google);
            Intent resultIntent = new Intent();
            SetResult(Result.Ok, resultIntent);
            Finish();
        }

        private async void FacebookButton_Click(object sender, EventArgs e)
        {
            Log.Debug("LoginActivity", "Facebook");
            await Authenticate(MobileServiceAuthenticationProvider.Facebook);
            Intent resultIntent = new Intent();
            SetResult(Result.Ok, resultIntent);
            Finish();
        }
    }
}