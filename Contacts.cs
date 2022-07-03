using Android.App;
using Android.Content;
using Android.OS;
using Android.Views;
using Android.Widget;
using System.Collections.Generic;
using Android.Support.V4.Widget;
using Android.Support.Design.Widget;
using Android.Support.V7.App;
using Android.Support.V4.View;
using System;
using Firebase.Database;
using Android.Provider;
using Android.Net;
using static Android.Provider.ContactsContract;

namespace FamBasket_3_0
{
    [Activity(Label = "SharedContacts", Theme = "@style/AppTheme")]
    public class Contacts : AppCompatActivity, NavigationView.IOnNavigationItemSelectedListener, ListView.IOnItemLongClickListener, View.IOnClickListener, AdapterView.IOnItemClickListener
    {
        private int user_Index1;
        private int user_Index2;
        private User user;
        private Contact contact, newFriend;
        private FriendRequestsAdapter requestsAdapter;
        private List<Contact> contacts;
        private ContactsListAdapter contactsAdapter;
        private ListView lst;
        private Button btnBack, btnAdd, btnCancel, btnRequests, btnBack2;
        private EditText etFriendName;
        private Dialog addFriend;
        private AppCompatDialog requests;
        FirebaseHelper firebaseHelper;
        Intent intent;
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            Xamarin.Essentials.Platform.Init(this, savedInstanceState);
            SetContentView(Resource.Layout.contacts);
            Android.Support.V7.Widget.Toolbar toolbar = FindViewById<Android.Support.V7.Widget.Toolbar>(Resource.Id.toolbar3);
            SetSupportActionBar(toolbar);
            firebaseHelper = new FirebaseHelper();

            //FloatingActionButton fab = FindViewById<FloatingActionButton>(Resource.Id.fab);
            //fab.Click += FabOnClick;

            DrawerLayout drawer = FindViewById<DrawerLayout>(Resource.Id.drawer_layout3);
            ActionBarDrawerToggle toggle = new ActionBarDrawerToggle(this, drawer, toolbar, Resource.String.navigation_drawer_open, Resource.String.navigation_drawer_close);
            drawer.AddDrawerListener(toggle);
            toggle.SyncState();

            NavigationView navigationView = FindViewById<NavigationView>(Resource.Id.nav_view);
            navigationView.SetNavigationItemSelectedListener(this);
            // Create your application here

            SetContentView(Resource.Layout.contacts);

            Intent intent;

            lst = FindViewById<ListView>(Resource.Id.lv_ListView2);
            btnBack = FindViewById<Button>(Resource.Id.btn_back);
            btnRequests = FindViewById<Button>(Resource.Id.btn_PendingRequests);
            btnRequests.SetOnClickListener(this);
            btnBack.Click += delegate { intent = new Intent(this, typeof(MyListsChoose)); StartActivity(intent); Finish(); };

            BuildList();

            lst.OnItemLongClickListener = this;

            FindViewById<Button>(Resource.Id.btn_AddFriend).Click += async delegate 
            {
                addFriend = new Dialog(this);
                addFriend.SetContentView(Resource.Layout.addfriend);
                addFriend.SetCanceledOnTouchOutside(true);
                btnAdd = addFriend.FindViewById<Button>(Resource.Id.btn_AddFriend);
                btnCancel = addFriend.FindViewById<Button>(Resource.Id.btn_Cancel4);
                etFriendName = addFriend.FindViewById<EditText>(Resource.Id.et_FriendName);
                btnAdd.SetOnClickListener(this);
                btnCancel.SetOnClickListener(this);
                addFriend.Show();
                //var contact = await Xamarin.Essentials.Contacts.PickContactAsync();

                //if (contact != null)
                //{
                //    var phones = contact.Phones;
                //    var name = contact.DisplayName;

                //    var phone = phones.ToArray()[0];

                //    Contact contact1 = new Contact();
                //    contact1.phoneNum = phone.ToString();
                //    contact1.contactName = name;
                //    FirebaseHelper firebaseHelper = new FirebaseHelper();
                //    DatabaseReference reference = firebaseHelper.GetDatabase().GetReference("friends").Push();
                //    await firebaseHelper.UpdateFriendsList(this.GetSharedPreferences("curruserinfo", FileCreationMode.Private
                //                                        ).GetString("currusername", ""), contact1);
                //    BuildList();
                //}
            };
        }
        protected override void OnSaveInstanceState(Bundle outState)
        {
            base.OnSaveInstanceState(outState);
        }

        //The method will close the drawer navigation uppon pressing the back button (toggle button)
        public override void OnBackPressed()
        {
            DrawerLayout drawer = FindViewById<DrawerLayout>(Resource.Id.drawer_layout3);
            if (drawer.IsDrawerOpen(GravityCompat.Start))
            {
                drawer.CloseDrawer(GravityCompat.Start);
            }
            else
            {
                base.OnBackPressed();
            }
        }

        //This method is irrelevant to the functionallity of the application / activity.
        private void FabOnClick(object sender, EventArgs eventArgs)
        {
            View view = (View)sender;
            Snackbar.Make(view, "Replace with your own action", Snackbar.LengthLong)
                .SetAction("Action", (Android.Views.View.IOnClickListener)null).Show();
        }

        //The method handles the click events of all the buttons in the navigation drawer menu.
        //When the user presses a certain button, the method will take him to the proper screen.
        public bool OnNavigationItemSelected(IMenuItem item)
        {
            int id = item.ItemId;


            switch (id)
            {
                case Resource.Id.nav_mylists:
                    intent = new Intent(this, typeof(MyLists));
                    break;
                case Resource.Id.nav_settings:
                    Dialog d = new Android.App.Dialog(this);
                    d.SetContentView(Resource.Layout.settings);
                    d.SetCanceledOnTouchOutside(true);
                    EditText etNewName = FindViewById<EditText>(Resource.Id.et_NewListName);
                    Button btnApply = FindViewById<Button>(Resource.Id.btn_Apply);
                    Button btnCancel = FindViewById<Button>(Resource.Id.btn_Cancel2);
                    btnApply.SetOnClickListener(this);
                    btnCancel.SetOnClickListener(this);
                    break;
                case Resource.Id.nav_logout:
                    intent = new Intent(this, typeof(Login));
                    if (user.isGuest == true)
                        DeleteUser();
                    break;
            }

            if (Intent != null)
            {
                StartActivity(Intent);
                Finish();
            }

            DrawerLayout drawer = FindViewById<DrawerLayout>(Resource.Id.drawer_layout3);
            drawer.CloseDrawer(GravityCompat.Start);
            return true;
        }

        //The method deletes the guest details from the database (it's a shame he didn't signed up!)
        public async void DeleteUser()
        {
            await firebaseHelper.DeleteUser(user.userId);
        }

        //The method builds the list of the user's contacts.
        private async void BuildList()
        {
            contacts = new List<Contact>();

            FirebaseHelper firebaseHelper = new FirebaseHelper();
            User user = await firebaseHelper.GetUser(this.GetSharedPreferences("curruserinfo", FileCreationMode.Private).GetString
                                               ("currusername", ""));
            contacts = user.friends;
            contactsAdapter = new ContactsListAdapter(this, contacts);

            FindViewById<ListView>(Resource.Id.lv_ListView2).Adapter = contactsAdapter;
        }

        //The method handles the case in which the user long-clicks a contact
        //in the list of contacts. He will be asked wether or not he would like to delete
        //the contact from his contacts, and the proper action will be triggered.
        public bool OnItemLongClick(AdapterView parent, View view, int position, long id)
        {
            user_Index1 = position;
            contact = contacts[position];
            Android.App.AlertDialog.Builder RYS = new Android.App.AlertDialog.Builder(this);
            RYS.SetTitle("Contact Deletion");
            RYS.SetMessage("Are you sure you want to remove this contact?");
            RYS.SetPositiveButton("Yes", OK_Action);
            RYS.SetNegativeButton("No", Abort_Action);
            RYS.SetCancelable(false);
            RYS.Create();
            RYS.Show();
            return true;
        }

        //The user wants to delte the contact - the contact will be removed from the list.
        private async void OK_Action(object sender, DialogClickEventArgs e)
        {
            // Delete Report file 
            FirebaseHelper firebaseHelper = new FirebaseHelper();
            string username = this.GetSharedPreferences("curruserinfo", FileCreationMode.Private).GetString("currusername", "");
            await firebaseHelper.RemoveContactFromList(username, contact.contactId);

            contacts.RemoveAt(user_Index1);
            contactsAdapter.NotifyDataSetChanged();
            user_Index1 = -1;
            return;
        }

        //The user doesn't want to delete the contact from the list - the method will abort the dialog.
        private void Abort_Action(object sender, DialogClickEventArgs e)
        {
            user_Index1 = -1;
            return;
        }

        //The method handles the click events of "Add Friend" & "Friend Requests" buttons.
        //According to the button, it creates a dialog.
        public async void OnClick(View v)
        {
            if (v == btnAdd)
            {
                ISharedPreferences sp = this.GetSharedPreferences("curruserinfo", FileCreationMode.Private);
                FirebaseHelper firebaseHelper = new FirebaseHelper();
                User currUser = await firebaseHelper.GetUser(sp.GetString("currusername", ""));
                if (etFriendName.Text != string.Empty && etFriendName.Text != currUser.username)
                {

                    if (await firebaseHelper.CheckIfUserExist(etFriendName.Text))
                    {
                        User friend = await firebaseHelper.GetUser(etFriendName.Text);
                        Contact contact = new Contact();
                        contact.contactName = currUser.username;
                        contact.contactId = currUser.userId;
                        await firebaseHelper.AddFriendRequest(friend.username, contact);
                        Toast.MakeText(this,"Friend request has been successfully sent to " + friend.username + "!", ToastLength.Long).Show();
                        addFriend.Dismiss();
                    }
                }
                else if (etFriendName.Text == string.Empty)
                    Toast.MakeText(this, "Please enter username to search for! ", ToastLength.Short).Show();
                else if (etFriendName.Text == currUser.username)
                    Toast.MakeText(this, "You can't add yourself as a friend :) ! ", ToastLength.Short).Show();
            }

            if (v == btnRequests)
            {
                requests = new AppCompatDialog(this);
                requests.SetContentView(Resource.Layout.friend_requests);
                requests.SetCanceledOnTouchOutside(true);
                ListView friendRequests = requests.FindViewById<ListView>(Resource.Id.lv_ListView4);
                friendRequests.OnItemClickListener = this;
                btnBack2 = requests.FindViewById<Button>(Resource.Id.btn_back2);
                btnBack2.Click += delegate { requests.Dismiss(); };
                ISharedPreferences sp = this.GetSharedPreferences("curruserinfo", FileCreationMode.Private);
                FirebaseHelper firebaseHelper = new FirebaseHelper();
                User user = await firebaseHelper.GetUser(sp.GetString("currusername", ""));
                requestsAdapter = new FriendRequestsAdapter(this, user.friendRquests);
                friendRequests.Adapter = requestsAdapter;
                requests.Show();
            }

            else if (v == btnCancel)
            {
                addFriend.Dismiss();
            }

        }

        //The method handles the event in which the user clicked a request in the friend requests dialog.
        //He will be asked wether or not he would like to accept the request, and the appropriate action will be called.
        public async void OnItemClick(AdapterView parent, View view, int position, long id)
        {
            ISharedPreferences sp = this.GetSharedPreferences("curruserinfo", FileCreationMode.Private);
            FirebaseHelper firebaseHelper = new FirebaseHelper();
            user = await firebaseHelper.GetUser(sp.GetString("currusername", ""));
            User friend = await firebaseHelper.GetUser(user.friendRquests[position].contactName);
            user_Index2 = position;
            newFriend = new Contact();
            newFriend.contactName = user.friendRquests[position].contactName;
            newFriend.contactId = friend.userId;
            Android.App.AlertDialog.Builder RYS = new Android.App.AlertDialog.Builder(this);
            RYS.SetTitle("Friend request");
            RYS.SetMessage("Accept or decline? ");
            RYS.SetPositiveButton("Accept", Accept_Action);
            RYS.SetNegativeButton("Decline", Decline_Action);
            RYS.SetCancelable(false);
            RYS.Create();
            RYS.Show();
            requests.Dismiss();
        }

        //The user accepted the friend request - the friend will be added to his friends list, and the user himself, will
        //also be added to the friend's friends list.
        private async void Accept_Action(object sender, DialogClickEventArgs e)
        {
            Contact curr = new Contact();
            curr.contactName = user.username;
            curr.contactId = user.userId;
            FirebaseHelper firebaseHelper = new FirebaseHelper();
            await firebaseHelper.UpdateFriendsList(user.username, newFriend);
            await firebaseHelper.UpdateFriendsList(newFriend.contactName, curr);
            await firebaseHelper.RemoveFriendRequest(user.username, newFriend);
           
            user.friendRquests.RemoveAt(user_Index2);
            BuildList();
            user_Index2 = -1;

            return;
        }

        //The user declined the friend request - the friend request dialog will be cancled. 
        private async void Decline_Action(object sender, DialogClickEventArgs e)
        {
            FirebaseHelper firebaseHelper = new FirebaseHelper();
            await firebaseHelper.RemoveFriendRequest(user.username, newFriend);
            user.friendRquests.RemoveAt(user_Index2);
            BuildList();
            user_Index2 = -1;
            return;
        }
    }
}