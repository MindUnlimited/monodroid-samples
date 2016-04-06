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
using Cheesesquare.Models;

namespace Cheesesquare
{
    class SyncHandler : IMobileServiceSyncHandler
    {
        MobileServiceClient client;
        const string LOCAL_VERSION = "Use local version";
        const string SERVER_VERSION = "Use server version";

        public SyncHandler(MobileServiceClient client)
        {
            this.client = client;
        }

        public virtual Task OnPushCompleteAsync(MobileServicePushCompletionResult result)
        {
            return Task.FromResult(0);
        }

        public virtual async Task<JObject> ExecuteTableOperationAsync(IMobileServiceTableOperation operation)
        {
            MobileServiceInvalidOperationException error;
            Func<Task<JObject>> tryOperation = operation.ExecuteAsync;

            do
            {
                error = null;

                try
                {
                    JObject result = await operation.ExecuteAsync();
                    return result;
                }
                catch (MobileServiceConflictException ex)
                {
                    error = ex;
                }
                catch (MobileServicePreconditionFailedException ex)
                {
                    error = ex;
                }

                if (error != null)
                {
                    //var localItem = operation.Item.ToObject<ToDoItem>();
                    var serverValue = error.Value;
                    //if (serverValue == null) // 409 doesn't return the server item
                    //{
                    //    serverValue = await operation.Table.LookupAsync(localItem.Id) as JObject;
                    //}

                    //var serverItem = serverValue.ToObject<ToDoItem>();

                    //if (serverItem.Complete == localItem.Complete &&
                    //    serverItem.Text == localItem.Text)
                    //{
                    //    // items are same so we can ignore the conflict
                    //    return serverValue;
                    //}

                    return (JObject)serverValue;

                    //int command = await ShowConflictDialog(localItem, serverValue);

                    //if (command == 1)
                    //{
                    //    // Overwrite the server version and try the operation again by continuing the loop
                    //    operation.Item[MobileServiceSystemColumns.Version] = serverValue[MobileServiceSystemColumns.Version];
                    //    if (error is MobileServiceConflictException) // change operation from Insert to Update
                    //    {
                    //        tryOperation = async () => await operation.Table.UpdateAsync(operation.Item) as JObject;
                    //    }
                    //    continue;
                    //}
                    //else if (command == 2)
                    //{
                    //    return (JObject)serverValue;
                    //}
                    //else
                    //{
                    //    operation.AbortPush();
                    //}
                }
            } while (error != null);

            return null;
        }

        //private async Task<int> ShowConflictDialog(ToDoItem localItem, JObject serverValue)
        //{
        //    var dialog = new UIAlertView("Conflict between local and server versions",
        //            "How do you want to resolve this conflict?\n\n" + "Local item: \n" + localItem +
        //            "\n\nServer item:\n" + serverValue.ToObject<ToDoItem>(), null, "Cancel", LOCAL_VERSION, SERVER_VERSION);

        //    var clickTask = new TaskCompletionSource<int>();
        //    dialog.Clicked += (sender, e) =>
        //    {
        //        clickTask.SetResult(e.ButtonIndex);
        //    };

        //    dialog.Show();

        //    return await clickTask.Task;
        //}
    }

    public class Database 
	{
        //Mobile Service Client reference
        public MobileServiceClient client;
        public MobileServiceUser mobileServiceUser = null;

        public string userName { get; set; }
        public string email { get; set; }
        public string userID { get; set; }
        public Group defGroup { get; set; }
        public User defUser { get; set; }
        public List<User> contacts { get; set; }
        public List<Group> userGroups { get; set; }

        //Mobile Service sync table used to access data
        //private IMobileServiceSyncTable<TodoItem> toDoTable;
        private IMobileServiceSyncTable<Item> itemTable;
        private IMobileServiceSyncTable<Device> deviceTable;
        private IMobileServiceSyncTable<ItemLink> itemLinkTable;
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
                        contactsString += "'" + contact.Email.ToLower() + "'" + ",";
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
                deviceTable = client.GetSyncTable<Device>();
                userGroupMembershipTable = client.GetSyncTable<UserGroupMembership>();
                groupTable = client.GetSyncTable<Group>();
                groupGroupMembershipTable = client.GetSyncTable<GroupGroupMembership>();
                itemTable = client.GetSyncTable<Item>();
                itemLinkTable = client.GetSyncTable<ItemLink>();
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
                    user.Email = emailAddress.ToLower();
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
            store.DefineTable<Device>();
            store.DefineTable<User>();
            store.DefineTable<Group>();
            store.DefineTable<UserGroupMembership>();
            store.DefineTable<GroupGroupMembership>();
            store.DefineTable<Item>();
            store.DefineTable<ItemLink>();

            // Uses the default conflict handler, which fails on conflict
            // To use a different conflict handler, pass a parameter to InitializeAsync. For more details, see http://go.microsoft.com/fwlink/?LinkId=521416
            await client.SyncContext.InitializeAsync(store, new SyncHandler(client));


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
                await deviceTable.PullAsync("Devices", deviceTable.CreateQuery());
                await itemLinkTable.PullAsync("ItemLinks", itemLinkTable.CreateQuery());
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

	    private void CreateAndShowDialog(Exception exception, String title)
	    {
            Debug.WriteLine(exception.InnerException.Message);
	    }

        public async Task<Group> getDefaultGroup(User user)
        {
            if (user == null)
                return null;

            var parameters = new Dictionary<string, string>
                {
                    { "userid", user.ID }
                };

            var defGroup = await PublicFields.Database.client.InvokeApiAsync<Group>("getdefaultgroup", HttpMethod.Get, parameters); // also gather extra user information

            return defGroup;
        }


        public async Task<Group> GetGroupByName(string groupName)
        {
            if (userID == null)
                return null;
            else
            {
                if (userGroups == null)
                    userGroups = await getGroups();

                foreach (Group group in userGroups)
                {
                    if (group.Name == groupName)
                        return group;
                }
            }
            return null;
        }

        public async Task<Group> GetGroupByID(string groupID)
        {
            if (userID == null)
                return null;
            else
            {
                if (userGroups == null)
                    userGroups = await getGroups();

                return userGroups.Find(grp => grp.id == groupID); 
            }
        }


        public async Task<List<Group>> getGroups()
        {
            if (userID == null)
                return null;
            else
            {
                var resultGroups = await groupTable.ToListAsync();
                return resultGroups;
            }
        }

        public async Task<List<Device>> getDevices()
        {
            if (userID == null)
                return null;
            else
            {
                var devices = await deviceTable.ToListAsync();
                return devices;
            }
        }


        public async Task SaveDevice(Device device)
        {
            try
            {
                await SyncAsync(); // offline sync, push and pull changes. Maybe results in conflict with the item to be saved

                if (defGroup == null)
                    defGroup = await getDefaultGroup(defUser);

                if(device.OwnerId == null)
                {
                    device.OwnerId = defGroup.id;
                }

                var devices_in_db_like_this = await deviceTable.Where(x => x.MachineId == device.MachineId && device.OwnerId == x.OwnerId && device.OwnerId != null).ToListAsync();

                // if id is not null then the item is already in the local db if it has version as well then it is also in the cloud
                if (devices_in_db_like_this.Count == 1)
                {
                    Log.Debug("DB", "Device already in db, doing nothing");
                    //await deviceTable.UpdateAsync(device);
                }
                else if (devices_in_db_like_this.Count == 0)
                {
                    Log.Debug("DB", "Device not yet in db adding field");
                    await deviceTable.InsertAsync(device);
                    await client.SyncContext.PushAsync();
                }
                else
                {
                    Log.Error("DB", "Something went wrong, multiple devices in db with the same machine id and owner");
                }
            }
            catch (Exception e)
            {
                CreateAndShowDialog(e, "Error");
            }
        }



        public async Task<bool> MemberOfGroup(User user, Group group)
        {
            var defGroupUser = await getDefaultGroup(user);
            List<UserGroupMembership> ugms = await userGroupMembershipTable.Where(ugm => ugm.ID == user.ID && ugm.MembershipID == group.id).ToListAsync();

            return ugms.Count == 1; // there should be only one item satisfying this query
        }

        public async Task<List<Group>> MembersOfGroup(Group group)
        {
            var user = defUser;

            var parameters = new Dictionary<string, string>
                {
                    { "userid", user.ID }, {"groupid", group.id }
                };

            var memberIDs = new List<string>();
            var members = new List<Group>();
            var groupMembers = await client.InvokeApiAsync<List<Group>>("getgroupmembers", HttpMethod.Get, parameters);
            return groupMembers;
        }

        public async Task<User> GetUser(string email)
        {
            var parameters = new Dictionary<string, string>
                {
                    { "email", email }
                };

            var user = await PublicFields.Database.client.InvokeApiAsync<User>("contactusesapp", HttpMethod.Get, parameters); // also gather extra user information

            return user;
        }

        public async Task<User> GetUser(Group defGroup)
        {
            List<UserGroupMembership> ugms = await userGroupMembershipTable.Where(ugm => ugm.MembershipID == defGroup.id).ToListAsync();
            var ugmExtracted = ugms.FirstOrDefault();
            if (ugmExtracted != null)
            {
                var user = await userTable.Where(usr => usr.ID.ToLower() == ugmExtracted.ID.ToLower()).ToListAsync();
                return user[0];
            }
            else return null;
        }

        public async Task<Group> GroupExists(List<User> groupmembers, string name = null)
        {
            Group groupWithCorrectName;

            if (userGroups == null)
            {
                userGroups = await getGroups();
            }

            if (name != null) // real group
            {
                groupWithCorrectName = userGroups.Find(grp => grp.Name == name);

                if (groupWithCorrectName == null)
                    return null;
            }  
            else if (groupmembers.Count == 2) // group invisible to the users containing only two members
            {
                var user1 = await GetUser(groupmembers[0].Email.ToLower());
                var user2 = await GetUser(groupmembers[1].Email.ToLower());

                var defGroupUser1 = await getDefaultGroup(user1);
                var defGroupUser2 = await getDefaultGroup(user2);

                if (defGroupUser1 == null || defGroupUser2 == null)
                    return null;

                groupWithCorrectName = userGroups.Find(grp => (grp.Name == defGroupUser1.id + "+" + defGroupUser2.id) || (grp.Name == defGroupUser2.id + "+" + defGroupUser1.id));
                return groupWithCorrectName;
            }
            else // something has gone wrong
            {
                Log.Debug("Database", string.Format("Group with name {} was not found", name));
                groupWithCorrectName = null;
            }
                
            var membersInGroup = true;
            foreach (var member in groupmembers)
            {
                if (! await (MemberOfGroup(member, groupWithCorrectName)))
                {
                    membersInGroup = false;
                    break;
                }
            }

            if (membersInGroup)
                return groupWithCorrectName;

            return null;
        }


        public async Task newUser(User user)
        {
            try
            {
                // insert new user
                await userTable.InsertAsync(user);
                await SyncAsync(); // offline sync, retrieve the group and ugm relationship to the local sqlite db

                var defGroup = await getDefaultGroup(user);
            }
            catch (Exception e)
            {
                CreateAndShowDialog(e, "Error: " + e.Message);
            }
        }

        public async Task<Group> SaveGroup(List<User> users, String name)
        {
            Group newGroup;

            // check whether the group already exists, if so return with that group
            var groupFound = await GroupExists(users, name);
            if (groupFound != null)
                return groupFound;

            var usersUpdated = new List<User>();
            // check whether the users in users already use the app
            foreach (var usr in users)
            {
                if (usr != defUser) // on all users except for the one currently logged in
                {
                    var retrievedUser = await GetUser(usr.Email.ToLower());
                    if (retrievedUser == null) // not in db (sqlite and azure)
                    {
                        await newUser(usr);
                        usersUpdated.Add(usr);
                    }
                    else
                        usersUpdated.Add(retrievedUser);
                }
                else
                    usersUpdated.Add(usr);
            }

            users = usersUpdated;


            // no name and a group with two users signalizes that two users are sharing with each other, the fact that that makes a group is unknown to them
            // the name of this bond will be the default groups id's of the two users concatenated with a plus sign in the middle
            if (string.IsNullOrEmpty(name) && users.Count == 2)
            {
                var currentUser = await getDefaultGroup(users[0]);
                var shareWithUser = await getDefaultGroup(users[1]);

                //if (currentUser == null) // make new temporary user for 1
                //{
                //    await newUser(users[0]);
                //    currentUser = await getDefaultGroup(users[0]);
                //}
                //else if (shareWithUser == null) //make new temporary user for 2
                //{
                //    await newUser(users[1]);
                //    shareWithUser = await getDefaultGroup(users[1]);
                //}
                //else // if users are already using app
                //{

                var groupGUIDCombined = currentUser.id + "+" + shareWithUser.id;
                newGroup = new Group { Name = groupGUIDCombined };
                //}
            }
            else
                newGroup = new Group { Name = name };

            await groupTable.InsertAsync(newGroup);

            try
            {
                await client.SyncContext.PushAsync();
            }
            catch(MobileServicePushFailedException ex)
            {
                Log.Debug("Database", "push of new group failed");
            }

            foreach (User usr in users)
            {
                Group usrDefGroup = await getDefaultGroup(usr);
                if (!string.IsNullOrEmpty(usrDefGroup.id) && !string.IsNullOrEmpty(newGroup.id))
                {
                    var ggm = new GroupGroupMembership { MemberID = usrDefGroup.id, MembershipID = newGroup.id };
                    await groupGroupMembershipTable.InsertAsync(ggm);
                }

            }

            try
            {
                await client.SyncContext.PushAsync();
            }
            catch (MobileServicePushFailedException ex)
            {
                Log.Debug("Database", "push of groupgroupmembership failed");
            }
            return newGroup;
        }

        public async Task SaveGroup(List<Group> groupItems, String name)
        {
            Group newGroup = new Group { Name = name };
            await groupTable.InsertAsync(newGroup);

            foreach (Group grp in groupItems)
            {
                var ggm = new GroupGroupMembership { MemberID = grp.id, MembershipID = newGroup.id };
                await groupGroupMembershipTable.InsertAsync(ggm);
            }

            await client.SyncContext.PushAsync();
        }

        public async Task<IEnumerable<Item>> GetDomains()
        {
            IEnumerable<Item> _domains = null;
            List<Item> sortedDomains = new List<Item>();
            if (userID != null)
            {
                if (userGroups == null)
                    userGroups = await getGroups();

                IEnumerable<string> groups_ids = from grp in userGroups select grp.id;

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
                if (userGroups == null)
                    userGroups = await getGroups();

                IEnumerable<string> groups_ids = from grp in userGroups select grp.id;

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
                try
                {
                    items = await itemTable.ToListAsync();
                }
                catch (Exception e)
                {
                    CreateAndShowDialog(e, "Error");
                }
            //}

            return items;
        }

        public async Task<IEnumerable<ItemLink>> GetItemLinks()
        {
            IEnumerable<ItemLink> itemLinks = null;

            try
            {
                itemLinks = await itemLinkTable.ToListAsync();
            }
            catch (Exception e)
            {
                CreateAndShowDialog(e, "Error");
            }

            return itemLinks;
        }

        public async Task<Item> GetItem(string id)
        {
            try
            {
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

                var existingUserList = await userTable.Where(u => u.Email.ToLower() == user.Email.ToLower()).ToListAsync();
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
                //await SyncAsync();

                var usr = client.CurrentUser;
                JObject userObject = (JObject) await client.InvokeApiAsync("userInfo", HttpMethod.Get, null);

                // properly logged in?
                if (userObject != null)
                {
                    userName = (string)userObject["name"];

                    var userTable = client.GetSyncTable<User>();
                    await userTable.PullAsync(null, userTable.CreateQuery());

                    switch (provider)
                    {
                        case MobileServiceAuthenticationProvider.Facebook:
                            email = (string)userObject["email"];
                            break;
                        case MobileServiceAuthenticationProvider.Google:
                            email = (string)userObject["email"];
                            break;
                        case MobileServiceAuthenticationProvider.MicrosoftAccount:
                            email = (string)userObject["emails"]["account"];
                            break;
                        case MobileServiceAuthenticationProvider.Twitter:
                            break;
                        case MobileServiceAuthenticationProvider.WindowsAzureActiveDirectory:
                            break;
                        default:
                            break;
                    }

                    List<User> existing_user = new List<User>();
                    existing_user = await userTable.Where(u => u.Email.ToLower() == email.ToLower()).ToListAsync();

                    if (existing_user.Count == 0)
                    {
                        User user = new User
                        {
                            Name = userName,
                            Email = email.ToLower()
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
                        await newUser(user);

                        userID = user.ID;

                        defGroup = await getDefaultGroup(user);

                        defUser = user;
                    }
                    else if (existing_user.Count == 1) // found a user with the right email address but do the providers match?
                    {
                        var user = existing_user[0];

                        // if first time logging in with account we already made for sharing then store the provider that the user uses
                        // or add another provider on the same email account
                        switch (provider)
                        {
                            case MobileServiceAuthenticationProvider.Facebook:
                                if (string.IsNullOrEmpty(user.FacebookID))
                                {
                                    user.FacebookID = providerID;
                                    Debug.WriteLine("adding new provider ID to user: " + providerID);
                                    await userTable.UpdateAsync(user);
                                    await client.SyncContext.PushAsync();
                                }
                                break;
                            case MobileServiceAuthenticationProvider.Google:
                                if (string.IsNullOrEmpty(user.GoogleID))
                                {
                                    user.GoogleID = providerID;
                                    Debug.WriteLine("adding new provider ID to user: " + providerID);
                                    await userTable.UpdateAsync(user);
                                    await client.SyncContext.PushAsync();
                                }
                                break;
                            case MobileServiceAuthenticationProvider.MicrosoftAccount:
                                if (string.IsNullOrEmpty(user.MicrosoftID))
                                {
                                    user.MicrosoftID = providerID;
                                    Debug.WriteLine("adding new provider ID to user: " + providerID);
                                    await userTable.UpdateAsync(user);
                                    await client.SyncContext.PushAsync();
                                }
                                break;
                            case MobileServiceAuthenticationProvider.Twitter:
                                break;
                            case MobileServiceAuthenticationProvider.WindowsAzureActiveDirectory:
                                break;
                            default:
                                break;
                        }
                        Debug.WriteLine("user exists, ID found: " + user.ID);

                        userID = user.ID;
                        defUser = user;
                        defGroup = await getDefaultGroup(defUser);
                    }
                    else
                    {
                        Debug.WriteLine("something weird happened, more than one user with the same ID found");
                        Debugger.Break();
                    }

                    //await SyncAsync(); // offline sync
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

                    ggm.MembershipID = group.id;

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

                    ggm.MembershipID = group.id;
                    await groupGroupMembershipTable.InsertAsync(ggm);
                    await client.SyncContext.PushAsync();

                    foreach (User user in usersToAdd)
                    {
                        User userInDB = await existingUser(user);
                        if (userInDB != null)
                        {
                            GroupGroupMembership ggmUser = new GroupGroupMembership();
                            ggmUser.MembershipID = group.id;
                            ggmUser.MemberID = userInDB.ID.ToLower();
                            await groupGroupMembershipTable.InsertAsync(ggmUser);
                            await client.SyncContext.PushAsync();
                        }
                        // else email them?
                    }
                }
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
                // item is shared from another user
                if(item.SharedLink != null)
                {
                    // retrieve shared item
                    var sharedItem = await GetItem(item.id);

                    // undo share specific changes
                    item.Parent = sharedItem.Parent;
                }

                await SyncAsync(); // offline sync, push and pull changes. Maybe results in conflict with the item to be saved

                // if id is not null then the item is already in the local db if it has version as well then it is also in the cloud
                if (item.id != null)
                {
                    await itemTable.UpdateAsync(item);
                }
                else
                {
                    if(string.IsNullOrEmpty(item.CreatedBy))
                    {
                        if (defGroup == null)
                            defGroup = await getDefaultGroup(defUser);

                        item.CreatedBy = defGroup.id;
                    }
                    await itemTable.InsertAsync(item);
                }

                await client.SyncContext.PushAsync();
            }
            catch (Exception e)
            {
                CreateAndShowDialog(e, "Error");
            }
        }

        public async Task SaveItemLink(ItemLink itemLink)
        {
            try
            {
                await SyncAsync(); // offline sync, push and pull changes. Maybe results in conflict with the item to be saved

                // if id is not null then the item is already in the local db if it has version as well then it is also in the cloud
                if (itemLink.id != null)
                {
                    await itemLinkTable.UpdateAsync(itemLink);
                }
                else
                {
                    await itemLinkTable.InsertAsync(itemLink);
                }

                await client.SyncContext.PushAsync();
            }
            catch (Exception e)
            {
                CreateAndShowDialog(e, "Error");
            }
        }

        public async Task DeleteItem(Item item)
        {
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
	}
}

