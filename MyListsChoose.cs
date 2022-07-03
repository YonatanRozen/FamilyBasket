using Android.App;
using Android.Content;
using Android.OS;
using Android.Views;
using Android.Widget;
using System.Collections.Generic;
using Android.Support.V4.Widget;
using Android.Support.Design.Widget;
using Android.Support.V7.App;
using System;
using Firebase.Database;
using Android.Support.V4.View;
using System.Threading.Tasks;

namespace FamBasket_3_0
{
    [Activity(Label = "MyListsChoose")]
    public class MyListsChoose : AppCompatActivity , NavigationView.IOnNavigationItemSelectedListener, View.IOnClickListener, ListView.IOnItemClickListener, AdapterView.IOnItemLongClickListener
    {
        private Button btnAddList, btnDone, btnCancel, btnGoTo;
        private List<GroceryList> userGroceryLists = null;
        private List<GroceryList> friendGroceryLists = null;
        private List<Contact> userFriends = null;
        private GroceryListsAdapter groceryListsAdapter;
        private GroceryList groceryList;
        FirebaseHelper firebaseHelper;
        private EditText etNewListName;
        private Dialog createListDia;
        private ListView listView;
        private int user_Index;
        private User user;      
        ISharedPreferences sp;
        Intent intent;
        protected override async void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.mylists_choose);
            firebaseHelper = new FirebaseHelper();
            sp = this.GetSharedPreferences("curruserinfo", FileCreationMode.Private);
            user = await firebaseHelper.GetUser(sp.GetString("currusername", ""));
            

            Android.Support.V7.Widget.Toolbar toolbar = FindViewById<Android.Support.V7.Widget.Toolbar>(Resource.Id.toolbar3);
            SetSupportActionBar(toolbar);
            //FloatingActionButton fab = FindViewById<FloatingActionButton>(Resource.Id.fab);
            //fab.Click += FabOnClick;
            DrawerLayout drawer = FindViewById<DrawerLayout>(Resource.Id.drawer_layout4);
            ActionBarDrawerToggle toggle = new ActionBarDrawerToggle(this, drawer, toolbar, Resource.String.navigation_drawer_open, Resource.String.navigation_drawer_close);
            drawer.AddDrawerListener(toggle);
            toggle.SyncState();
            NavigationView navigationView = FindViewById<NavigationView>(Resource.Id.nav_view);
            navigationView.SetNavigationItemSelectedListener(this);
            btnAddList = FindViewById<Button>(Resource.Id.btn_AddNewList);
            listView = FindViewById<ListView>(Resource.Id.lv_ListView3);
           
            userGroceryLists = user.groceryLists;

            groceryListsAdapter = new GroceryListsAdapter(this, userGroceryLists);

            listView.Adapter = groceryListsAdapter;

            listView.OnItemClickListener = this;
            listView.OnItemLongClickListener = this;

            btnAddList.Click += BtnAddList_Click;

            
        }

        /// <summary>
        /// בעת לחיצה על כפתור 'הוספת רשימה חדשה', הפעולה תיצור תיבת דו - שיח שתשמש ליצירת רשימה חדשה, ותראה אותה למשתמש
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnAddList_Click(object sender, EventArgs e)
        {
            createListDia = new Android.App.Dialog(this);
            createListDia.SetContentView(Resource.Layout.addnewlist);
            createListDia.SetCanceledOnTouchOutside(true);
            etNewListName = createListDia.FindViewById<EditText>(Resource.Id.et_ListName);
            btnDone = createListDia.FindViewById<Button>(Resource.Id.btn_Done1);
            btnCancel = createListDia.FindViewById<Button>(Resource.Id.btn_Cancel1);
            btnDone.SetOnClickListener(this);
            btnCancel.SetOnClickListener(this);
            createListDia.Show();
        }


        //The method closes the navigation drawer uppon pressing the "back" toggle button
        public override void OnBackPressed()
        {
            DrawerLayout drawer = FindViewById<DrawerLayout>(Resource.Id.drawer_layout4);
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

        /// <summary>
        /// הפעולה מטפלת בכל אירועי הלחיצה של הכפתורים שכלולים בתפריט המגירה, הנפתח מצדי המסך, ומעבירה 
        /// את המשתמש למסך המתאים על פי הכפתור שעליו לחץ
        /// </summary>
        /// <param name="item">מייצג את הפריט שעליו המשתמש לחץ בתפריט, אותו מקבלת הפעולה כפרמטר</param>
        /// <returns>הפעולה תחזיר 'אמת' באם היא הצליחה ו'שקר' אחרת</returns>
        public bool OnNavigationItemSelected(IMenuItem item)
        {
            int id = item.ItemId;


            switch (id)
            {

                case Resource.Id.nav_friends:
                    if (user.isGuest == false)
                        intent = new Intent(this, typeof(Contacts));
                    else
                        Toast.MakeText(this, "Sorry, but only registered users can add friends!", ToastLength.Long).Show();
                    break;
                case Resource.Id.nav_logout:
                    intent = new Intent(this, typeof(Login));
                    sp = this.GetSharedPreferences("curruserinfo", FileCreationMode.Private);
                    sp.Dispose();
                    if (user.isGuest == true)
                        DeleteUser();
                    break;
            }

            if (intent != null)
            {
                StartActivity(intent);
                Finish();
            }

            DrawerLayout drawer = FindViewById<DrawerLayout>(Resource.Id.drawer_layout4);
            drawer.CloseDrawer(GravityCompat.Start);
            return true;
        }

        //The method deletes the guest details from the database (it's a shame he didn't signed up!)
        public async void DeleteUser ()
        {
            await firebaseHelper.DeleteUser(user.userId);
        }

        /// <summary>
        /// הפעולה מטפלת באירועי הלחיצה של כפתורי 'בטל' ו'סיים' בתיבת דו - שיח של יצירת 
        ///  רשימה חדשה. באם נלחץ 'סיים', הפעולה תיצור רשימה חדשה, באם נלחץ 'בטל', הפעולה 
        ///  תבטל את תיבת דו השיח של יצירת רשימה חדשה
        ///  באם המשתמש לא הכניס שם לרשימה החדשה
        /// </summary>
        /// <param name="v"></param>
        public async void OnClick (View v)
        {
            if (v == btnDone)
            {
                if (!await firebaseHelper.CheckIfListExists(user.username, etNewListName.Text))
                {
                    GroceryList newList = new GroceryList();
                    newList.groceries = new List<Grocery>();
                    if (etNewListName.Text != string.Empty)
                    {
                        newList.listName = etNewListName.Text;
                    }

                    await firebaseHelper.AddGroceryList(user.username, newList);
                    userGroceryLists.Add(newList);
                    groceryListsAdapter.NotifyDataSetChanged();

                    createListDia.Dismiss();
                }
                if (await firebaseHelper.CheckIfListExists(user.username, etNewListName.Text))
                    Toast.MakeText(this, "There's aready a list with the same name, choose a different one!", ToastLength.Long);
                else if (etNewListName.Text == string.Empty)
                    Toast.MakeText(this, "You have to provide a name for the list!", ToastLength.Long);
            }

            else if (v == btnCancel)
            {
                createListDia.Dismiss();
            }
            else
                Toast.MakeText(this, "Please make sure you have provided a grocery name and quantity! ", ToastLength.Long).Show();
        }       

        //הפעולה מטפלת במקרה בו משתמש לוחץ לחיצה ארוכה על פריט ברשימת הקניות ואז הוא נשאל אם למחוק או לא
        public bool OnItemLongClick(AdapterView parent, View view, int position, long id)
        {
            user_Index = position;
            groceryList = userGroceryLists[position];
            Android.App.AlertDialog.Builder RYS = new Android.App.AlertDialog.Builder(this);
            RYS.SetTitle("List deletion");
            RYS.SetMessage("Are you sure you want to remove this list?");
            RYS.SetPositiveButton("Yes", OK_Action);
            RYS.SetNegativeButton("No", Abort_Action);
            RYS.SetCancelable(false);
            RYS.Create();
            RYS.Show();
            return true;

            
        }

        //במקרה והמשתמש אכן רוצה למחוק, הפעולה הזו תמחק את המוצר מרשימת הקניות
        private async void OK_Action(object sender, DialogClickEventArgs e)
        {
            // Delete Report file 
            await firebaseHelper.RemoveGroceryList(user.username, groceryList.listId);
            if (user.userId == groceryList.ownerId)
            {
                if (groceryList.members != null)
                    await firebaseHelper.RemoveGroceryListForAllMembers(groceryList.members, groceryList.listId);
            }
            userGroceryLists.RemoveAt(user_Index);
            groceryListsAdapter.NotifyDataSetChanged();
            user_Index = -1;
            return;
        }

        //המשתמש אינו רוצה למחוק את הפריט
        private void Abort_Action(object sender, DialogClickEventArgs e)
        {
            user_Index = -1;
            return;
        }

        //המשתמש לוחץ על אחד מן המוצרים - הפעולה תוביל אותו למסך עריכת הפריט
        public void OnItemClick(AdapterView parent, View view, int position, long id)
        {
            intent = new Intent(this, typeof(MyLists));
            intent.PutExtra("currlistposition", position);
            StartActivity(intent);
            Finish();
        }
    }
}