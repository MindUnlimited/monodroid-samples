using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using SQLite;
using System.IO;

// Azure additions
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.MobileServices;
using Microsoft.WindowsAzure.MobileServices.Sync;
using Microsoft.WindowsAzure.MobileServices.SQLiteStore;
//using Xamarin.Forms;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Xamarin.Contacts;
using System.Collections.ObjectModel;
using Android.App;
using Todo;
using Android.Util;

namespace Cheesesquare
{
	public class Database 
	{
        //Mobile Service Client reference
        public MobileServiceClient client;
        public MobileServiceUser mobileServiceUser = null;
        public string userName { get; set; }
        public string email { get; set; }
        public string userID { get; set; }
        public Group defGroup { get; set; }
        public List<User> contacts { get; set; }

        //Mobile Service sync table used to access data
        //private IMobileServiceSyncTable<TodoItem> toDoTable;
        private IMobileServiceSyncTable<Item> itemTable;
        private IMobileServiceSyncTable<User> userTable;
        private IMobileServiceSyncTable<Group> groupTable;
        private IMobileServiceSyncTable<UserGroupMembership> userGroupMembershipTable;
        private IMobileServiceSyncTable<GroupGroupMembership> groupGroupMembershipTable;


        const string applicationURL = @"https://mindunlimited.azure-mobile.net/";
        const string applicationKey = @"RMFULNJBBVHwffaZeDYYhndAjEQzoT88";

        const string localDbFilename = "localstore.db";

		//static object locker = new object ();

		//SQLiteConnection database;

		string DatabasePath {
			get { 
				var sqliteFilename = "TodoSQLite.db3";
				#if __IOS__
				string documentsPath = Environment.GetFolderPath (Environment.SpecialFolder.Personal); // Documents folder
				string libraryPath = Path.Combine (documentsPath, "..", "Library"); // Library folder
				var path = Path.Combine(libraryPath, sqliteFilename);
				#else
				#if __ANDROID__
				string documentsPath = Environment.GetFolderPath (Environment.SpecialFolder.Personal); // Documents folder
				var path = Path.Combine(documentsPath, sqliteFilename);
				#else
				// WinPhone
				//var path = Path.Combine(Windows.Storage.ApplicationData.Current.LocalFolder.Path, sqliteFilename);;
			    var path = sqliteFilename;
				#endif
				#endif
				return path;
			}
		}

        public async Task getContactsThatUseApp()
        {
            String contactsFound = "";

            if (contacts == null)
                getContacts();

            if (contacts.Count > 0 && userID != null)
            {
                int sliceSize = 25;
                List<List<User>> listOfContactLists = new List<List<User>>();
                for (int i = 0; i < contacts.Count; i += sliceSize)
                    listOfContactLists.Add(contacts.GetRange(i, Math.Min(sliceSize, contacts.Count - i)));

                foreach (List<User> contactList in listOfContactLists)
                {
                    var contactsString = "";
                    foreach (User contact in contactList)
                    {
                        contactsString += "'" + contact.Email + "'" + ",";
                    }
                    contactsString = contactsString.TrimEnd(',');

                    //var json = (Newtonsoft.Json.Linq.JObject) await client.InvokeApiAsync("userInfo", HttpMethod.Get, null);


                    var parameters = new Dictionary<string, string>
                    {
                        { "contacts", contactsString }
                    };

                    var contactsThatUseAppAPIResult = await client.InvokeApiAsync("contactsthatuseapp", HttpMethod.Get, parameters);
                    contactsFound += contactsThatUseAppAPIResult.ToString();

                    

                    //if (contactsThatUseAppAPIResult != null)
                    //{
                    //    Newtonsoft.Json.Linq.JObject calcResult = (Newtonsoft.Json.Linq.JObject)contactsThatUseAppAPIResult.Result;
                    //}
                }

            }
            //Debug.WriteLine(contactsFound);
        }

	    public Database()
	    {
            try
            {

                // Create the Mobile Service Client instance, using the provided
                // Mobile Service URL and key
                client = new MobileServiceClient(applicationURL, applicationKey);
                
                InitLocalStoreAsync();
                                
                userTable = client.GetSyncTable<User>();
                userGroupMembershipTable = client.GetSyncTable<UserGroupMembership>();
                groupTable = client.GetSyncTable<Group>();
                groupGroupMembershipTable = client.GetSyncTable<GroupGroupMembership>();
                itemTable = client.GetSyncTable<Item>();
            }
            catch (Exception e)
            {
                CreateAndShowDialog(e, "Error");
            }            
	    }

        public void getContacts()
        {
            contacts = new List<User>();

            #if __ANDROID__
            var book = new AddressBook(Application.Context);
            #else
            var book = new AddressBook();
            #endif

            if (!book.RequestPermission().Result)
            {
                Console.WriteLine("Permission denied by user or manifest");
            }

            foreach (Xamarin.Contacts.Contact contact in book)
            {
                //Console.WriteLine("{0} {1}", contact.FirstName, contact.LastName);

                //enum emailAddressImportance {EmailType.Home, EmailType.Work, EmailType.Other};

                List<EmailType> emailImportance = new List<EmailType>(new EmailType[]
	            {
	                EmailType.Home,
	                EmailType.Work,     // River 2
	                EmailType.Other
	            });

                string emailAddress = null;
                int index = 0;
                while (emailAddress == null)
                {
                    if (index >= emailImportance.Count)
                        break;
                    emailAddress = (from email in contact.Emails
                                    where email.Address != null && email.Type == emailImportance[index]
                                    select email.Address).FirstOrDefault();
                    index += 1;
                }

                if (emailAddress != null)
                {
                    User user = new User();
                    user.Email = emailAddress;
                    user.Name = contact.FirstName + " " + contact.LastName;
                    contacts.Add(user);
                }
            }
        }

        public async Task InitLocalStoreAsync()
        {
            // new code to initialize the SQLite store
            string path = DatabasePath;

            if (!File.Exists(path))
            {
                File.Create(path).Dispose();
            }

            var store = new MobileServiceSQLiteStore(path);
            store.DefineTable<TodoItem>();
            store.DefineTable<User>();
            store.DefineTable<Group>();
            store.DefineTable<UserGroupMembership>();
            store.DefineTable<GroupGroupMembership>();
            store.DefineTable<Item>();

            // Uses the default conflict handler, which fails on conflict
            // To use a different conflict handler, pass a parameter to InitializeAsync. For more details, see http://go.microsoft.com/fwlink/?LinkId=521416
            await client.SyncContext.InitializeAsync(store);

            String errorString = null;
            try
            {
                await client.SyncContext.PushAsync();
            }
            catch (MobileServicePushFailedException ex)
            {
                errorString = "Push failed because of sync errors: " +
                  ex.PushResult.Errors.Count + " errors, message: " + ex.Message + '\t' + ex.PushResult.Status;
            }
            catch (Exception ex)
            {
                errorString = "Pull failed: " + ex.Message +
                  "\n\nIf you are still in an offline scenario, " +
                  "you can try your Pull again when connected with your Mobile Serice.";
            }

            if (errorString != null)
            {
                Log.Debug("DataBase", errorString);
            }
            
        }

        public async Task SyncAsync()
        {
            String errorString = null;

            try
            {
                await client.SyncContext.PushAsync();


                await userTable.PullAsync("Users", userTable.CreateQuery());// first param is query ID, used for incremental sync
                await userGroupMembershipTable.PullAsync("UserGroupMemberships", userGroupMembershipTable.CreateQuery()); // first param is query ID, used for incremental sync
                await groupTable.PullAsync("Groups", groupTable.CreateQuery()); // first param is query ID, used for incremental sync
                await groupGroupMembershipTable.PullAsync("GroupGroupMemberships", groupGroupMembershipTable.CreateQuery()); // first param is query ID, used for incremental sync
                await itemTable.PullAsync("Items", itemTable.CreateQuery());// first param is query ID, used for incremental sync
            }

            catch (MobileServicePushFailedException ex)
            {
                errorString = "Push failed because of sync errors: " +
                  ex.PushResult.Errors.Count + " errors, message: " + ex.Message;
            }
            catch (Exception ex)
            {
                errorString = "Pull failed: " + ex.Message +
                  "\n\nIf you are still in an offline scenario, " +
                  "you can try your Pull again when connected with your Mobile Serice.";
            }

            if (errorString != null)
            {
                //MessageDialog d = new MessageDialog(errorString);
                //await d.ShowAsync();
            }
        }

        //// Called when the refresh menu option is selected
        //public async Task OnRefreshItemsSelected()
        //{
        //    await SyncAsync(); // get changes from the mobile service
        //}


        public async Task CheckItem(Item item)
        {
            if (client == null)
            {
                return;
            }

            // Set the item as completed and update it in the table
            item.Status = 7;
            try
            {
                await itemTable.UpdateAsync(item); // update the new item in the local database
                await SyncAsync(); // send changes to the mobile service

                //if (item.Done)
                //    adapter.Remove(item);

            }
            catch (Exception e)
            {
                CreateAndShowDialog(e, "Error");
            }
        }



        //public async void AddItem(View view)
        //{
        //    if (client == null)// || string.IsNullOrWhiteSpace(textNewToDo.Text))
        //    {
        //        return;
        //    }

        //    // Create a new item
        //    var item = new Item
        //    {
                
        //        //Name = textNewToDo.Text,

        //        Status = 0
        //    };

        //    try
        //    {
        //        await itemTable.InsertAsync(item); // insert the new item into the local database
        //        await SyncAsync(); // send changes to the mobile service


        //        //if (!item.Done)
        //        //{
        //        //    adapter.Add(item);
        //        //}
        //    }
        //    catch (Exception e)
        //    {
        //        CreateAndShowDialog(e, "Error: " + e.Message);
        //    }

        //    //textNewToDo.Text = "";
        //}

	    private void CreateAndShowDialog(Exception exception, String title)
	    {
            Debug.WriteLine(exception.InnerException.Message);
	    }

        public async Task createDomains(string OwnedByID)
        {
            if (defGroup == null)
                defGroup = await getDefaultGroup();

            Item personal = new Item { Name = "Personal", Type = 1, OwnedBy = OwnedByID, CreatedBy = defGroup.ID};
            Item friends = new Item { Name = "Friends & Family", Type = 1, OwnedBy = OwnedByID, CreatedBy = defGroup.ID };
            Item work = new Item { Name = "Work", Type = 1, OwnedBy = OwnedByID, CreatedBy = defGroup.ID };
            Item community = new Item { Name = "Community", Type = 1, OwnedBy = OwnedByID, CreatedBy = defGroup.ID };

            await itemTable.InsertAsync(personal);
            await itemTable.InsertAsync(friends);
            await itemTable.InsertAsync(work);
            await itemTable.InsertAsync(community);
                
            await client.SyncContext.PushAsync();
        }

        //private void CreateAndShowDialog(Exception exception, String title)
        //{
        //    CreateAndShowDialog(exception.Message, title);
        //}

        //private void CreateAndShowDialog(string message, string title)
        //{
        //    AlertDialog.Builder builder = new AlertDialog.Builder(this);

        //    builder.SetMessage(message);
        //    builder.SetTitle(title);
        //    builder.Create().Show();
        //}

        public async Task<Group> getDefaultGroup()
        {
            var defGroupMembershipList = await userGroupMembershipTable.Where(ugm => ugm.ID == userID).ToListAsync();
            UserGroupMembership defUserGroupMembership = defGroupMembershipList.FirstOrDefault();

            if (defUserGroupMembership != null)
            {
                string defGroupID = defUserGroupMembership.MembershipID;
                // default's group ID to it's actual group
                List<Group> defGroupList = await groupTable.Where(grp => grp.ID == defGroupID).ToListAsync();
                Group defGroup = defGroupList.FirstOrDefault();
                return defGroup;
            }
            return null;
        }


        public async Task<Group> getGroup(string groupName)
        {
            if (userID == null)
                return null;
            else
            {
                var groups = await getGroups();
                foreach (Group group in groups)
                {
                    if (group.Name == groupName)
                        return group;
                }
            }
            return null;
        }


        public async Task<List<Group>> getGroups()
        {
            if (userID == null)
                return null;
            else
            {
                // from userID to GroupID from the users default group (defGroup)
                List<Group> resultGroups = new List<Group>();
                List<Group> groups = await groupTable.ToListAsync();

                if (defGroup == null)
                    defGroup = await getDefaultGroup();

                var queue = new Queue<Group>();
                queue.Enqueue(defGroup);

                while (queue.Count > 0)
                {
                    // Take the next node from the front of the queue
                    var node = queue.Dequeue();

                    // Process the node 'node'
                    if (resultGroups.Contains(node) == false)
                        resultGroups.Add(node);

                    List<GroupGroupMembership> ggms = await groupGroupMembershipTable.Where(ggm => ggm.MemberID == node.ID).ToListAsync();

                    IEnumerable<Group> children = from g in groups
                                                  where ggms.Any(ggm => ggm.MembershipID == g.ID)
                                                  select g;

                    //List<Group> children = await groupTable.Where(group => ids.Contains(group.ID)).ToListAsync();

                    //List<Group> groups = await groupTable.ToListAsync();
                    //List<GroupGroupMembership> ggms = await groupGroupMembershipTable.ToListAsync();

                    //var childrrren = from g in groups
                    //                 join ggm in ggms
                    //                     on g.ID equals ggm.
                    //                 select g;

                    // Add the node’s children to the back of the queue
                    foreach (var child in children)
                        queue.Enqueue(child);
                }

                return resultGroups;
            }
        }

        public async Task<IEnumerable<Item>> GetDomains()
        {
            IEnumerable<Item> _domains = null;
            List<Item> sortedDomains = new List<Item>();
            if (userID != null)
            {
                List<Group> groups = await getGroups();
                IEnumerable<string> groups_ids = from grp in groups select grp.ID;

                try
                {
                    _domains = await itemTable.Where(it => groups_ids.Contains(it.OwnedBy) && it.Type == 1).ToListAsync();
                    //List<Item> domains = new List<Item>();

                    Item[] domains = new Item[_domains.Count()];
                    List<Item> remainder = new List<Item>();

                    foreach (Item dom in _domains)
                    {
                        //StackLayout head = new StackLayout { Padding = 2, Spacing = 1 };

                        switch (dom.Name)
                        {
                            case "Personal":
                                domains[0] = dom;
                                break;
                            case "Friends & Family":
                                domains[1] = dom;
                                break;
                            case "Work":
                                domains[2] = dom;
                                break;
                            case "Community":
                                domains[3] = dom;
                                break;
                            default:
                                remainder.Add(dom);
                                break;
                        }
                    }

                    // DOES THIS WORK WITH NULL??

                    sortedDomains.AddRange(domains);
                    sortedDomains.AddRange(remainder);
                }
                catch (Exception e)
                {
                    CreateAndShowDialog(e, "Error");
                }
            }

            return sortedDomains;
        }

        public async Task<IEnumerable<Item>> GetChildItems(Item parent)
        {
            List<Item> items = new List<Item>();
            if (userID != null)
            {
                List<Group> groups = await getGroups();
                IEnumerable<string> groups_ids = from grp in groups select grp.ID;

                try
                {
                    var goals = await itemTable.Where(it => groups_ids.Contains(it.OwnedBy) && it.Parent == parent.id).ToListAsync();
                    IEnumerable<string> goal_ids = from goal in goals select goal.id;

                    if (goals.Count > 0)
                    {
                        items.AddRange(goals);

                        var projects = await itemTable.Where(it => groups_ids.Contains(it.OwnedBy) && goal_ids.Contains(it.Parent)).ToListAsync();
                        IEnumerable<string> project_ids = from proj in projects select proj.id;

                        if (projects.Count > 0)
                        {
                            items.AddRange(projects);

                            var tasks = await itemTable.Where(it => groups_ids.Contains(it.OwnedBy) && project_ids.Contains(it.Parent)).ToListAsync();

                            if (tasks.Count > 0)
                                items.AddRange(tasks);
                        }
                    }                   
                }
                catch (Exception e)
                {
                    CreateAndShowDialog(e, "Error");
                }
            }

            return items;
        }

        public async Task<IEnumerable<Item>> GetItems()
        {
            IEnumerable<Item> items = null;
            if (userID != null)
            {
                List<Group> groups = await getGroups();
                IEnumerable<string> groups_ids = from grp in groups select grp.ID;

                try
                {
                    items = await itemTable.Where(it => groups_ids.Contains(it.OwnedBy)).ToListAsync();
                }
                catch (Exception e)
                {
                    CreateAndShowDialog(e, "Error");
                }
            }

            return items;
        }

        public async Task<IEnumerable<Item>> GetItemsNotDone()
        {
            IEnumerable<Item> list = null;
            try
            {
                // Get the items that weren't marked as completed and add them in the adapter
                //await SyncAsync(); // offline sync
                list = await itemTable.Where(item => item.Status != 7).ToListAsync();
            }
            catch (Exception e)
            {
                CreateAndShowDialog(e, "Error");
            }

            return list;
        }

        public async Task<Item> GetItem(string id)
        {
            //lock (locker)
            //{
            //    return database.Table<TodoItem>().FirstOrDefault(x => x.ID == id);
            //}

            try
            {
                //await SyncAsync();
                return await itemTable.LookupAsync(id);
            }
            catch (MobileServiceInvalidOperationException msioe)
            {
                Debug.WriteLine(@"INVALID {0}", msioe.Message);
            }
            catch (Exception e)
            {
                Debug.WriteLine(@"ERROR {0}", e.Message);
            }
            return null;
        }

        public async Task<User> existingUser(User user)
        {
            try
            {
                var userTable = client.GetSyncTable<User>();
                await userTable.PullAsync(null, userTable.CreateQuery());

                //var users = await userTable.ToListAsync();
                //var all_users = await userTable.ToListAsync();

                var existingUserList = await userTable.Where(u => u.MicrosoftID == user.Email).ToListAsync();
                User foundUser = existingUserList.FirstOrDefault();
                return foundUser;
            }
            catch (Exception e)
            {
                CreateAndShowDialog(e, "Error: " + e.Message);
            }

            return null;
        }

        public async Task newUser(string providerID, MobileServiceAuthenticationProvider provider)
        {
            try
            {
                await SyncAsync();
                //userTable = client.GetSyncTable<User>();
                //await userTable.PullAsync(null, userTable.CreateQuery());
                // does the user already exist?

                // fetch username not just the id
                //Newtonsoft.Json.Linq.JObject userInfo = (Newtonsoft.Json.Linq.JObject) await client.InvokeApiAsync("userInfo", HttpMethod.Get, null);
                //userName = userInfo.Value<string>("name");
                //var emails = userInfo.Value<strting>("emails");

                var usr = client.CurrentUser;
                JObject userObject = (JObject) await client.InvokeApiAsync("userInfo", HttpMethod.Get, null);

                if (userObject != null)
                {
                    userName = (string)userObject["name"];

                    var userTable = client.GetSyncTable<User>();
                    await userTable.PullAsync(null, userTable.CreateQuery());

                    //var users = await userTable.ToListAsync();
                    //var all_users = await userTable.ToListAsync();

                    List<User> existing_user = new List<User>();
                    switch (provider)
                    {
                        case MobileServiceAuthenticationProvider.Facebook:
                            email = (string)userObject["email"];
                            existing_user = await userTable.Where(u => u.FacebookID == providerID).ToListAsync();
                            break;
                        case MobileServiceAuthenticationProvider.Google:
                            email = (string)userObject["email"];
                            existing_user = await userTable.Where(u => u.GoogleID == providerID).ToListAsync();
                            break;
                        case MobileServiceAuthenticationProvider.MicrosoftAccount:
                            email = (string)userObject["emails"]["account"];
                            existing_user = await userTable.Where(u => u.MicrosoftID == providerID).ToListAsync();
                            break;
                        case MobileServiceAuthenticationProvider.Twitter:
                            break;
                        case MobileServiceAuthenticationProvider.WindowsAzureActiveDirectory:
                            break;
                        default:
                            break;
                    }

                    //var existing_user = await userTable.Where(u => u.MicrosoftID == userID).ToListAsync();
                    //var groups = await groupTable.ToListAsync();

                    if (existing_user.Count == 0)
                    {
                        User user = new User
                        {
                            Name = userName,
                            Email = email
                        };

                        switch (provider)
                        {
                            case MobileServiceAuthenticationProvider.Facebook:
                                user.FacebookID = providerID;
                                break;
                            case MobileServiceAuthenticationProvider.Google:
                                user.GoogleID = providerID;
                                break;
                            case MobileServiceAuthenticationProvider.MicrosoftAccount:
                                user.MicrosoftID = providerID;
                                break;
                            case MobileServiceAuthenticationProvider.Twitter:
                                break;
                            case MobileServiceAuthenticationProvider.WindowsAzureActiveDirectory:
                                break;
                            default:
                                break;
                        }

                        // insert new user
                        await userTable.InsertAsync(user);
                        await client.SyncContext.PushAsync();

                        userID = user.ID;

                        Group group = new Group
                        {
                            Name = userName
                        };

                        // add default group voor user
                        await groupTable.InsertAsync(group);
                        await client.SyncContext.PushAsync();

                        defGroup = group;

                        UserGroupMembership ugm = new UserGroupMembership
                        {
                            ID = user.ID,
                            MembershipID = group.ID
                        };

                        await userGroupMembershipTable.InsertAsync(ugm);
                        await client.SyncContext.PushAsync();

                        await createDomains(defGroup.ID);
                    }
                    else if (existing_user.Count == 1)
                    {
                        string ID = existing_user.FirstOrDefault<User>().ID;
                        Debug.WriteLine("user exists, ID found: " + ID);
                        userID = ID;
                        defGroup = await getDefaultGroup();
                    }
                    else
                    {
                        Debug.WriteLine("something weird happened, more than one user with the same ID found");
                        Debugger.Break();
                    }

                    await SyncAsync(); // offline sync
                    //adapter.Clear();
                }

            }
            catch (Exception e)
            {
                CreateAndShowDialog(e, "Error: " + e.Message);
            }
        }

        public async Task SaveItem(Group group, GroupGroupMembership ggm)
        {
            try
            {
                await SyncAsync(); // offline sync, push and pull changes. Maybe results in conflict with the item to be saved

                // if version is not null then the item already exists, so update is needed instead of insert
                if (group.Version != null)
                {
                    await groupTable.UpdateAsync(group);
                    await groupGroupMembershipTable.UpdateAsync(ggm);
                }
                else
                {

                    await groupTable.InsertAsync(group);
                    await client.SyncContext.PushAsync();

                    ggm.MembershipID = group.ID;

                    await groupGroupMembershipTable.InsertAsync(ggm);
                }


                await client.SyncContext.PushAsync();
            }
            catch (Exception e)
            {
                CreateAndShowDialog(e, "Error");
            }
        }

        public async Task SaveItem(Group group, List<User> usersToAdd, GroupGroupMembership ggm)
        {
            try
            {
                await SyncAsync(); // offline sync, push and pull changes. Maybe results in conflict with the item to be saved

                // if version is not null then the item already exists, so update is needed instead of insert
                if (group.Version != null)
                {
                    await groupTable.UpdateAsync(group);
                    await groupGroupMembershipTable.UpdateAsync(ggm);



                    //TODO! 
                }
                else
                {

                    await groupTable.InsertAsync(group);
                    await client.SyncContext.PushAsync();

                    ggm.MembershipID = group.ID;
                    await groupGroupMembershipTable.InsertAsync(ggm);
                    await client.SyncContext.PushAsync();

                    foreach (User user in usersToAdd)
                    {
                        User userInDB = await existingUser(user);
                        if (userInDB != null)
                        {
                            GroupGroupMembership ggmUser = new GroupGroupMembership();
                            ggmUser.MembershipID = group.ID;
                            ggmUser.MemberID = userInDB.ID;
                            await groupGroupMembershipTable.InsertAsync(ggmUser);
                            await client.SyncContext.PushAsync();
                        }
                        // else email them?
                    }
                }


                //await client.SyncContext.PushAsync();
            }
            catch (Exception e)
            {
                CreateAndShowDialog(e, "Error");
            }
        }

        public async Task SaveItem(Item item)
        {
            try
            {
                await SyncAsync(); // offline sync, push and pull changes. Maybe results in conflict with the item to be saved

                // if version is not null then the item already exists, so update is needed instead of insert
                if (item.Version != null)
                {
                    await itemTable.UpdateAsync(item);
                }
                else
                {
                    if (defGroup == null)
                        defGroup = await getDefaultGroup();

                    item.CreatedBy = defGroup.ID;
                    //var jObject = JObject.FromObject(item);
                    //var retJObject = await itemTable.InsertAsync(jObject);// (item);
                    //item = retJObject.ToObject<Item>();
                    await itemTable.InsertAsync(item);
                }

                await client.SyncContext.PushAsync();

                //adapter.Clear();

                //foreach (ToDoItem current in list)
                //    adapter.Add(current);

            }
            catch (Exception e)
            {
                CreateAndShowDialog(e, "Error");
            }
        }

        public async Task DeleteItem(Item item)
        {
            //lock (locker)
            //{
            //    return database.Delete<TodoItem>(id);
            //}

            try
            {
                await itemTable.DeleteAsync(item);
                await SyncAsync(); // offline sync
            }
            catch (MobileServiceInvalidOperationException msioe)
            {
                CreateAndShowDialog(msioe, msioe.Message);
            }
            catch (Exception e)
            {
                CreateAndShowDialog(e, "Error");
            }
        }

        public async Task DeleteItem(Group group)
        {
            //lock (locker)
            //{
            //    return database.Delete<TodoItem>(id);
            //}

            try
            {
                await groupTable.DeleteAsync(group);
                await SyncAsync(); // offline sync
            }
            catch (MobileServiceInvalidOperationException msioe)
            {
                CreateAndShowDialog(msioe, msioe.Message);
            }
            catch (Exception e)
            {
                CreateAndShowDialog(e, "Error");
            }
        }

        //public async Task DeleteItem(string id)
        //{
        //    //lock (locker)
        //    //{
        //    //    return database.Delete<TodoItem>(id);
        //    //}

        //    try 
        //    {
        //        Item to_be_deleted = GetItem(id).Result;
        //        await itemTable.DeleteAsync(to_be_deleted);
        //        await SyncAsync(); // offline sync
        //    } 
        //    catch (MobileServiceInvalidOperationException msioe)
        //    {
        //       CreateAndShowDialog(msioe, msioe.Message);
        //    }
        //    catch (Exception e)
        //    {
        //        CreateAndShowDialog(e, "Error");
        //    }
        //}

        //public async Task DeleteTaskAsync(TodoItem item)
        //{
        //    try
        //    {
        //        await todoTable.DeleteAsync(item);
        //        //await SyncAsync ();
        //    }
        //    catch (MobileServiceInvalidOperationException msioe)
        //    {
        //        Debug.WriteLine(@"INVALID {0}", msioe.Message);
        //    }
        //    catch (Exception e)
        //    {
        //        Debug.WriteLine(@"ERROR {0}", e.Message);
        //    }
        //}
	}
}

