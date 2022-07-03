using System;
using System.Collections.Generic;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Widget;

namespace FamBasket_3_0
{
    [Activity(Label = "EditGrocery")]
    public class EditGrocery : Activity
    {
        private Button btnDoneEditingItem, btnDeleteGrocery, btnFewer, btnMore, btnAbortEdit;
        private List<string> currListMemberIds = null;
        private List<string> currListMemberNames = null;
        private List<Grocery> groceries = null;
        private EditText etNewName, etItemName, etQuantityEdit;
        private int index, currListPos;
        private FirebaseHelper firebaseHelper;
        private Spinner spM;
        private GroceryList currList;
        ISharedPreferences sp;
        private User user;
        private string username;
        private string un;
        private int finalQuantity;
        Intent intent;
        protected override async void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.edit_item);
            firebaseHelper = new FirebaseHelper();
            sp = this.GetSharedPreferences("curruserinfo", FileCreationMode.Private);
            username = sp.GetString("currusername", "");
            user = await firebaseHelper.GetUser(username);
            currListPos = Intent.GetIntExtra("currlistposition", -1);
            currList = user.groceryLists[currListPos];
            groceries = user.groceryLists[currListPos].groceries;
            currListMemberIds = user.groceryLists[currListPos].members;
            currListMemberNames = await firebaseHelper.GetListMembersNames(user.username, currList.listId);
            index = Intent.GetIntExtra("currgroceryposition", -1);
            groceries = currList.groceries;


            spM = FindViewById<Spinner>(Resource.Id.spinner1);
            btnDoneEditingItem = FindViewById<Button>(Resource.Id.btn_DoneEdit);
            btnDeleteGrocery = FindViewById<Button>(Resource.Id.btn_DeleteGrocery);
            etItemName = FindViewById<EditText>(Resource.Id.et_itemName);
            etQuantityEdit = FindViewById<EditText>(Resource.Id.et_quantity);
            btnFewer = FindViewById<Button>(Resource.Id.btn_fewer);
            btnMore = FindViewById<Button>(Resource.Id.btn_more);
            btnAbortEdit = FindViewById<Button>(Resource.Id.btn_abortEdit);
            

            var adapter = ArrayAdapter.CreateFromResource(this, Resource.Array.sp_MeasureUnits, Android.Resource.Layout.SimpleSpinnerItem);
            adapter.SetDropDownViewResource(Android.Resource.Layout.SimpleSpinnerDropDownItem);
            spM.Adapter = adapter;
            spM.ItemSelected += SpMeasureUnits_ItemSelected;
            spM.SetSelection(adapter.GetPosition(currList.groceries[index].units));
            un = "";

            etItemName.Text = groceries[index].GroceryName;
            etQuantityEdit.Text = groceries[index].Quantity;

            btnFewer.Click += BtnFewer_Click;
            btnMore.Click += BtnMore_Click;
            btnDoneEditingItem.Click += BtnDoneEditingItem_Click;
            btnDeleteGrocery.Click += BtnDeleteGrocery_Click;
            btnAbortEdit.Click += BtnAbortEdit_Click;
        }

        private void SpMeasureUnits_ItemSelected(object sender, AdapterView.ItemSelectedEventArgs e)
        {
            un = spM.GetItemAtPosition(e.Position).ToString();
        }

        //המשתמש סיים לערוך את הפריט, ולוחץ סיים על אישור, הפעולה מעדכנת את השינויים ברשימת המוצרים
        private async void BtnDoneEditingItem_Click(object sender, EventArgs e)
        {
            string formerN = groceries[index].GroceryName;
            string formerQ = groceries[index].Quantity;
            if (etItemName.Text != string.Empty)
            {
                
                if (etItemName.Text != groceries[index].GroceryName)
                    await firebaseHelper.UpdateGroceryName(user.username, currList.listId, groceries[index].GroceryId, etItemName.Text);
            }
            if (etQuantityEdit.Text != string.Empty && int.TryParse(etQuantityEdit.Text, out int a))
            {
                
                await firebaseHelper.UpdateGroceryQuantity(user.username, currList.listId, groceries[index].GroceryId, a);
                if (!string.IsNullOrEmpty(un))
                    await firebaseHelper.UpdateGroceryUnits(user.username, currList.listId, groceries[index].GroceryId, un);
            }
            if (!int.TryParse(etQuantityEdit.Text, out int b))
            {
                Toast.MakeText(this, "Please make sure the quantity field contains numbers - only / empty text!", ToastLength.Long);
                await firebaseHelper.UpdateGroceryName(user.username, currList.listId, groceries[index].GroceryId, formerN);
                await firebaseHelper.UpdateGroceryQuantity(user.username, currList.listId, groceries[index].GroceryId, int.Parse(formerQ));
                return;
            }

            intent = new Intent(this, typeof(MyLists));
            intent.PutExtra("currlistposition", currListPos);
            StartActivity(intent);
            Finish();
        }

        //The user aborted the grocery editing.
        //the method will take him back to shopping list view screen and no changes will be made.
        private void BtnAbortEdit_Click(object sender, EventArgs e)
        {
            intent = new Intent(this, typeof(MyLists));
            intent.PutExtra("currlistposition", Intent.GetIntExtra("currlistposition", -1));
            StartActivity(intent);
            Finish();
        }

        //The user wants to subtract 1 point from the grocery quantity.
        //This method handles the click event of "fewer" button.
        private async void BtnFewer_Click(object sender, EventArgs e)
        {
            if (int.TryParse(etQuantityEdit.Text, out int a))
            {
                if (a > 1)
                    etQuantityEdit.Text = (a - 1).ToString();
                else
                    Toast.MakeText(this, "Less than 1 equals nothing!", ToastLength.Long);

            }
            else
                Toast.MakeText(this, "You can only subtract quantity from a number!", ToastLength.Long);
        }

        //The user clicks on the "delete" button at the top - right corner of the editing screen.
        //The method will delete the grocery from the list, regardless of any changes been made before.
        private async void BtnDeleteGrocery_Click(object sender, EventArgs e)
        {
            string groceryID = groceries[index].GroceryId;
            await firebaseHelper.RemoveGroceryFromList(user.username, groceryID, currList.listId);
            if (currListMemberIds != null)
                await firebaseHelper.RemoveGroceryFromListForAllMembers(currListMemberIds, currList.listId, groceryID);

            intent = new Intent(this, typeof(MyLists));
            intent.PutExtra("currlistposition", Intent.GetIntExtra("currlistposition", -1));
            StartActivity(intent);
            Finish();

        }

        //The user wants to add 1 point to the grocery quantity.
        //This method handles the click event of "more" button.
        private async void BtnMore_Click(object sender, EventArgs e)
        {
            if (int.TryParse(etQuantityEdit.Text, out int a) && (a > 1 || a == 1))
            {
                a++;
                etQuantityEdit.Text = a.ToString();
            }
            else
                Toast.MakeText(this, "You can only add quantity to a number (1 or bigger) !", ToastLength.Long);

        }
    }
}