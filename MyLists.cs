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
using Android.Graphics;
using Android.Runtime;
using Android.Provider;
using Java.IO;
using Android.Util;
using Android.Graphics.Drawables;
using System.IO;
using FamBasket_3_0.Adapters;
using Android.Content.PM;
using Uri = Android.Net.Uri;
using static Android.Provider.ContactsContract;
using Android;

namespace FamBasket_3_0
{
    [Activity(Label = "", Theme = "@style/AppTheme")]
    public class MyLists : AppCompatActivity, NavigationView.IOnNavigationItemSelectedListener, ListView.IOnItemLongClickListener, View.IOnClickListener, ListView.IOnItemClickListener
    {
        private const int REQUEST_SENDSMS = 1;
        private const int CAMERA_REQUEST = 5, PICK_IMAGE_REQUEST = 8;
        private Spinner spMeasureUnits;
        private SearchView svSearch;
        private TextView tvHeadline;
        private Dialog d1, d2, ShowMembers, AddMembers, editGrocery;
        private Intent intent;
        private ListView lst;
        private EditText etNewName, etItemName;
        private Button btnApply, btnCancel1, btnCancel2, btnDone, btnAddItem;
        private List<Grocery> groceries = null;
        private List<string> currListMemberIds = null;
        private List<string> currListMemberNames = null;
        private List<Contact> userFriends = null;
        private GroceryListAdapter groceriesAdapter;
        private int currListPos;
        private AvailableFriendsAdapter availableContactsAdapter;
        private ArrayAdapter membersAdapter;
        private Grocery grocery;
        private GroceryList currList;
        Android.App.AlertDialog.Builder RY;
        int user_Index;
        int temp;
        private string username;
        User user;
        FirebaseHelper firebaseHelper;
        ISharedPreferences sp;
        ISharedPreferencesEditor spEdit;
        protected override async void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            Xamarin.Essentials.Platform.Init(this, savedInstanceState);
            SetContentView(Resource.Layout.mylists);
            firebaseHelper = new FirebaseHelper();

            lst = FindViewById<ListView>(Resource.Id.lv_ListView1);
            tvHeadline = FindViewById<TextView>(Resource.Id.tv_NameOfList);
            btnAddItem = FindViewById<Button>(Resource.Id.btn_AddNewitem);

            sp = this.GetSharedPreferences("curruserinfo", FileCreationMode.Private);
            username = sp.GetString("currusername", "");
            user = await firebaseHelper.GetUser(username);
            UpdateTitle();

            currListPos = Intent.GetIntExtra("currlistposition", -1);
            currList = user.groceryLists[currListPos];
            groceries = user.groceryLists[currListPos].groceries;
            currListMemberIds = user.groceryLists[currListPos].members;
            currListMemberNames = await firebaseHelper.GetListMembersNames(user.username, currList.listId);
            userFriends = user.friends;


            groceriesAdapter = new GroceryListAdapter(this, groceries);

            lst.Adapter = groceriesAdapter;

            lst.OnItemClickListener = this;
            lst.OnItemLongClickListener = this;
            btnAddItem.Click += BtnAddItem_Click;


            Android.Support.V7.Widget.Toolbar toolbar = FindViewById<Android.Support.V7.Widget.Toolbar>(Resource.Id.toolbar2);
            SetSupportActionBar(toolbar);

            //FloatingActionButton fab = FindViewById<FloatingActionButton>(Resource.Id.fab);
            //fab.Click += FabOnClick;

            DrawerLayout drawer = FindViewById<DrawerLayout>(Resource.Id.drawer_layout2);
            ActionBarDrawerToggle toggle = new ActionBarDrawerToggle(this, drawer, toolbar, Resource.String.navigation_drawer_open, Resource.String.navigation_drawer_close);
            drawer.AddDrawerListener(toggle);
            toggle.SyncState();

            NavigationView navigationView = FindViewById<NavigationView>(Resource.Id.nav_view);
            navigationView.SetNavigationItemSelectedListener(this);
        }

        //The method handles the click event of the "Add item" button. It will take him to the groceries addition screen.
        private void BtnAddItem_Click(object sender, EventArgs e)
        {
            intent = new Intent(this, typeof(AddGrocery));
            intent.PutExtra("currlistposition", Intent.GetIntExtra("currlistposition", -1));
            StartActivity(intent);
            Finish();
        }

        //The method imports the current choosed list name, and changes the title of the window accordingly.
        private void UpdateTitle()
        {
            tvHeadline.Text = user.groceryLists[Intent.GetIntExtra("currlistposition", -1)].listName;
        }

        //The method closes the navigation drawer uppon pressing the "Back" toggle button.
        public override void OnBackPressed()
        {
            DrawerLayout drawer = FindViewById<DrawerLayout>(Resource.Id.drawer_layout2);
            if (drawer.IsDrawerOpen(GravityCompat.Start))
            {
                drawer.CloseDrawer(GravityCompat.Start);
            }
            else
            {
                base.OnBackPressed();
            }
        }
        private void FabOnClick(object sender, EventArgs eventArgs)
        {
            View view = (View)sender;
            Snackbar.Make(view, "Replace with your own action", Snackbar.LengthLong)
                .SetAction("Action", (Android.Views.View.IOnClickListener)null).Show();
        }

        //The method handles the click events of all the items in the drawer navigation menu and takes the user to the proper screen accrodingly.
        public bool OnNavigationItemSelected(IMenuItem item)
        {
            int id = item.ItemId;


            switch (id)
            {
                case Resource.Id.nav_home:
                    intent = new Intent(this, typeof(MyListsChoose));
                    break;
                case Resource.Id.nav_friends:
                    if (user.isGuest == false)
                        intent = new Intent(this, typeof(Contacts));
                    else
                        Toast.MakeText(this, "Sorry, but only registered users can add friends!", ToastLength.Long).Show();
                    break;
                case Resource.Id.nav_settings:
                    d2 = new Android.App.Dialog(this);
                    d2.SetContentView(Resource.Layout.settings);
                    d2.SetCanceledOnTouchOutside(true);
                    etNewName = d2.FindViewById<EditText>(Resource.Id.et_NewListName);
                    btnApply = d2.FindViewById<Button>(Resource.Id.btn_Apply);
                    btnCancel2 = d2.FindViewById<Button>(Resource.Id.btn_Cancel2);
                    btnApply.SetOnClickListener(this);
                    btnCancel2.SetOnClickListener(this);
                    d2.Show();
                    break;
                case Resource.Id.nav_logout:
                    intent = new Intent(this, typeof(Login));
                    if (user.isGuest == true)
                        DeleteUser();
                    break;
            }

            if (intent != null)
            {
                StartActivity(intent); Finish();
            }

            DrawerLayout drawer = FindViewById<DrawerLayout>(Resource.Id.drawer_layout2);
            drawer.CloseDrawer(GravityCompat.Start);
            return true;
        }

        //The method deletes the guest details from the database (it's a shame he didn't signed up!)
        public async void DeleteUser()
        {
            await firebaseHelper.DeleteUser(user.userId);
        }

        //The method is triggered uppon a long click on a grocery.
        //The user will be asked wether he would like to delete, cancel the dialog, or edit the grocery. The user will be transfered to the proper
        //screen accordingly.
        public bool OnItemLongClick(AdapterView parent, View view, int position, long id)
        {
            user_Index = position;
            grocery = groceries[position];
            Android.App.AlertDialog.Builder RYS = new Android.App.AlertDialog.Builder(this);
            RYS.SetTitle("Choose Action");
            RYS.SetMessage("Choose wether to delete, or edit this grocery");
            RYS.SetPositiveButton("Delete", OK_Action);
            RYS.SetNeutralButton("Edit", Edit_Action);
            RYS.SetNegativeButton("Cancel", Abort_Action);
            RYS.SetCancelable(false);
            RYS.Create();
            RYS.Show();
            return true;
        }

        //The method is triggered when the user clicks the "delete" button. The method deletes the grocery from the list.
        private async void OK_Action(object sender, DialogClickEventArgs e)
        {
            // Delete Report file            
            string username = this.GetSharedPreferences("curruserinfo", FileCreationMode.Private).GetString("currusername", "");
            await firebaseHelper.RemoveGroceryFromList(username, grocery.GroceryId, user.groceryLists[Intent.GetIntExtra("currlistposition", -1)].listId);
            if (currListMemberIds != null)
                await firebaseHelper.RemoveGroceryFromListForAllMembers(currListMemberIds, currList.listId, grocery.GroceryId);

            groceries.RemoveAt(user_Index);
            groceriesAdapter.NotifyDataSetChanged();
            user_Index = -1;
            return;
        }

        //The method closes the dialog, since it is triggered when the user aborts the dialog.
        private void Abort_Action(object sender, DialogClickEventArgs e)
        {
            user_Index = -1;
            return;
        }

        //The method handles the click events of the "Apply changes" (changing the list name), and "Cancel" (cancel changes the list name).
        public async void OnClick(View v)
        {
            if (v == btnApply)
            {
                if (etNewName.Text != string.Empty)
                {
                    tvHeadline.Text = etNewName.Text;
                    sp = this.GetSharedPreferences("curruserinfo", FileCreationMode.Private);
                    spEdit = sp.Edit();
                    spEdit.PutString("currlistname", tvHeadline.Text);
                    spEdit.Commit();
                    user.groceryLists[Intent.GetIntExtra("currlistposition", -1)].listName = etNewName.Text;
                    await firebaseHelper.ChangeListName(user.userId, currList.listId, etNewName.Text);
                    d2.Dismiss();
                }
                else
                {
                    Toast.MakeText(this, "Make sure you entered a new name for the list!", ToastLength.Short).Show();
                }
            }

            else if (v == btnCancel2)
            {
                d2.Dismiss();
            }
            else
            {
                d1.Dismiss();
            }

        }

        //The method creates the menu that is opened uppon pressing the three dots at the top-right corner of the screen.
        public override bool OnCreateOptionsMenu(Android.Views.IMenu menu)
        {
            MenuInflater.Inflate(Resource.Menu.list_options, menu);
            return true;
        }

        //The method handles the click event of all the items in the options menu:
        //Show Members - shows the members of the current list.
        //Add member to list - allows the user to add members to the list from his current friends.
        //Send list - allows the user to send the list as a string to a specific contact in whatsapp, email and etc.
        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            if (item.ItemId == Resource.Id.action_ShowMembers)
            {
                if (user.isGuest != true)
                {
                    ShowMembers = new Android.App.Dialog(this);
                    ShowMembers.SetCanceledOnTouchOutside(true);
                    ShowMembers.SetContentView(Resource.Layout.list_members);
                    ListView members = ShowMembers.FindViewById<ListView>(Resource.Id.lv_ListView5);
                    membersAdapter = new ArrayAdapter(this, Android.Resource.Layout.SimpleListItem1, currListMemberNames);
                    members.Adapter = membersAdapter;
                    members.ItemLongClick += Members_ItemLongClick1;
                    ShowMembers.Show();
                }
                else
                    Toast.MakeText(this, "Sorry, but only registered users can do that!", ToastLength.Long).Show();
                return true;

            }
            else if (item.ItemId == Resource.Id.action_AddMembers)
            {
                if (user.isGuest != true)
                {
                    AddMembers = new Android.App.Dialog(this);
                    AddMembers.SetCanceledOnTouchOutside(true);
                    AddMembers.SetContentView(Resource.Layout.add_list_members);
                    AddMembers.FindViewById<Button>(Resource.Id.btn_back4).Click += delegate { AddMembers.Dismiss(); };
                    ListView availableContacts = AddMembers.FindViewById<ListView>(Resource.Id.lv_ListView6);
                    availableContactsAdapter = new AvailableFriendsAdapter(this, userFriends);
                    availableContacts.Adapter = availableContactsAdapter;
                    availableContacts.ItemClick += AvailableContacts_ItemClick1;
                    AddMembers.Show();
                }
                else
                    Toast.MakeText(this, "Sorry, but only registered users can do that!", ToastLength.Long).Show();
                return true;
            }
            else if (item.ItemId == Resource.Id.action_SendList)
            {
                OpenWhatsapp();
                return true;
            }
            return base.OnOptionsItemSelected(item);


        }

        //The method is triggered uppon a long click on a member in the members list dialog - the user will be asked 
        // wether or not he would like to remove the member from the list. If he clicks "yes", the member will be removed only if he
        //is the current list's owner. If he clicks not, the dialog will be cancled regardless of any ownership of the list.
        private async void Members_ItemLongClick1(object sender, AdapterView.ItemLongClickEventArgs e)
        {
            temp = e.Position;
            RY = new Android.App.AlertDialog.Builder(this);
            RY.SetTitle("Member Removal");
            RY.SetMessage("Are you sure you want to remove " + currListMemberNames[e.Position] + " from " + currList.listName + "?");
            RY.SetPositiveButton("Yes", Remove_Member_Action);
            RY.SetNegativeButton("No", Abort_Member_Removal_Action);
            RY.SetCancelable(false);
            RY.Create();
            RY.Show();

        }

        //The user wants to remove the member from the list.
        private async void Remove_Member_Action(object sender, DialogClickEventArgs e)
        {
            if (user.userId == currList.ownerId)
            {
                string selectedContact = user.friends[temp].contactId;
                await firebaseHelper.RemoveMemberFromList(user.username, currList.listId, selectedContact);
                await firebaseHelper.RemoveMemberFromListForAllMembers(currListMemberIds, currList.listId, selectedContact);
                await firebaseHelper.RemoveGroceryList(currListMemberNames[temp], currList.listId);

                currListMemberNames.RemoveAt(temp);
                membersAdapter.NotifyDataSetChanged();
                ShowMembers.Dismiss();
            }
            else
                Toast.MakeText(this, "You are not authorized to remove members from " + currList.listName + "!", ToastLength.Long).Show();
        }

        //The member - removal is aborted.
        private async void Abort_Member_Removal_Action(object sender, DialogClickEventArgs e)
        {
            RY.Dispose();
        }

        //The user clicks on an available contact on available friends list, and he is added to the members list.
        private async void AvailableContacts_ItemClick1(object sender, AdapterView.ItemClickEventArgs e)
        {
            string selectedContact = user.friends[e.Position].contactName;
            User temp = await firebaseHelper.GetUser(selectedContact);
            if (!await firebaseHelper.CheckIfMemberInList(user.username, currList.listId, temp.userId))
            {
                await firebaseHelper.AddMemberToList(user.username, currList.listId, temp.userId);
                currListMemberIds.Add(temp.userId);
                await firebaseHelper.AddMemberToListForAllMembers(currListMemberIds, currList.listId, temp.userId);
                currListMemberNames.Add(temp.username);
 
                Toast.MakeText(this, temp.username + " been added successfully to " + currList.listName + " members! ", ToastLength.Long).Show();
                if (membersAdapter != null) membersAdapter.NotifyDataSetChanged();
                AddMembers.Dismiss();
            }
            else
                Toast.MakeText(this, temp.username + " is already in " + currList.listName + " members! ", ToastLength.Long).Show();
        }

        //The method gives the user an option to share the list as a string, in whatsapp, email and etc.
        private async void OpenWhatsapp()
        {
            if (groceries  != null)
            {
                string s = currList.listName + ":" + "\n";

                for (int i = 0; i < groceries.Count; i++)
                {
                    s += groceries[i].ToString() + "\n";
                }

                s += "Created by FamilyBasket";

                if (Android.Support.V4.Content.ContextCompat.CheckSelfPermission(this, Manifest.Permission.SendSms) == (int)Permission.Granted)
                {
                    Intent sendIntent = new Intent();
                    sendIntent.SetAction(Intent.ActionSend);
                    sendIntent.PutExtra(Intent.ExtraText, s);
                    sendIntent.SetType("text/plain");

                    Intent shareIntent = Intent.CreateChooser(sendIntent, "");
                    StartActivity(shareIntent);

                }
                else if (Android.Support.V4.App.ActivityCompat.ShouldShowRequestPermissionRationale(this, Manifest.Permission.SendSms))
                {
                    Android.App.AlertDialog.Builder builder = new Android.App.AlertDialog.Builder(this);
                    builder.SetTitle("Sms Permission Needed");
                    builder.SetMessage("Can not share stuff without sms permission");
                    builder.SetCancelable(false);
                    builder.SetPositiveButton("Allow", (g, e) =>
                    {
                        Android.Support.V4.App.ActivityCompat.RequestPermissions(this, new string[] { Manifest.Permission.SendSms }, REQUEST_SENDSMS);
                    });
                    builder.SetNegativeButton("Deny", (g, e) =>
                    {
                        builder.Dispose();
                    });

                    Android.App.AlertDialog d = builder.Create();
                    d.Show();
                }
                else
                {
                    Android.Support.V4.App.ActivityCompat.RequestPermissions(this, new string[] { Manifest.Permission.SendSms }, REQUEST_SENDSMS);
                }


            }

            else
                Toast.MakeText(this, "Empty list? Really?", ToastLength.Long).Show();
        }

        //The method checks the clicked grocery with a strike-through line that indicated it's been crossed off, or unchecks
        //it if it's already crossed-off.
        public async void OnItemClick(AdapterView parent, View view, int position, long id)
        {
            if (lst.GetChildAt(position).FindViewById<TextView>(Resource.Id.tv_GroceryName1).PaintFlags == PaintFlags.StrikeThruText)
            {
                lst.GetChildAt(position).FindViewById<TextView>(Resource.Id.tv_GroceryName1).PaintFlags = PaintFlags.AntiAlias;
                await firebaseHelper.UpdateGroceryIsFlags(user.username, currList.listId, groceries[position].GroceryId, "false");
            }
            else
            {
                lst.GetChildAt(position).FindViewById<TextView>(Resource.Id.tv_GroceryName1).PaintFlags = PaintFlags.StrikeThruText;
                await firebaseHelper.UpdateGroceryIsFlags(user.username, currList.listId, groceries[position].GroceryId, "true");
            }
        }

        //The method takes the user to the grocery editing screen.
        public void Edit_Action(object sender, EventArgs e)
        {
            intent = new Intent(this, typeof(EditGrocery));
            intent.PutExtra("currlistposition", currListPos);
            intent.PutExtra("currgroceryposition", user_Index);
            StartActivity(intent);
            Finish();
        }
    }
}