using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Speech;
using Android.Views;
using Android.Widget;

namespace FamBasket_3_0
{
    [Activity(Label = "AddGrocery")]
    public class AddGrocery : Activity , AdapterView.IOnItemClickListener
    {
        private Button btnBack, btnRecord, btnAdd;
        private ListView lvHistory, lvSuggestions;
        private ArrayAdapter historyAdapter;
        private ArrayAdapter suggestionsAdapter;
        private Android.App.AlertDialog.Builder RYS;
        private List<string> groceryHistory = null;
        private List<string> suggestions = null;
        private List<string> currListMembers = null;
        private List<Grocery> groceries;
        private GroceryList currList;
        private readonly int VOICE = 10;
        FirebaseHelper firebaseHelper;
        private EditText etGroceryName;
        private string listClicked;
        private string currListId;
        private string temp;
        private int currListPos;
        private bool isRecording;
        ISharedPreferences sp;        
        Intent intent;
        User user;
        protected override async void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.addnewitem);
            firebaseHelper = new FirebaseHelper(); 

            etGroceryName = FindViewById<EditText>(Resource.Id.et_AddThis);
            btnBack = FindViewById<Button>(Resource.Id.btn_backToLists);
            btnRecord = FindViewById<Button>(Resource.Id.btn_Record);
            btnAdd = FindViewById<Button>(Resource.Id.btn_Add);
            lvSuggestions = FindViewById<ListView>(Resource.Id.lv_Suggestions);

            sp = this.GetSharedPreferences("curruserinfo", FileCreationMode.Private);
            user = await firebaseHelper.GetUser(sp.GetString("currusername", ""));
            currListPos = Intent.GetIntExtra("currlistposition", -1);
            currListId = user.groceryLists[currListPos].listId;
            currListMembers = user.groceryLists[currListPos].members;
            groceries = user.groceryLists[currListPos].groceries;
            currList = user.groceryLists[currListPos];

            suggestions = await firebaseHelper.GetGroceryHistory(user.username);
            suggestions.AddRange(GetSuggestions());

            suggestionsAdapter = new ArrayAdapter(this, Android.Resource.Layout.SimpleListItem1, suggestions);

            lvSuggestions.Adapter = suggestionsAdapter;
            lvSuggestions.OnItemClickListener = this;

            btnAdd.Click += BtnAdd_Click;
            btnBack.Click += BtnBack_Click;
            btnRecord.Click += BtnRecord_Click;
        }

        //בעת לחיצה על פריט מההיסטוריה או מההצעות, הוא יוסף לרשימה
        public async void OnItemClick(AdapterView parent, View view, int position, long id)
        {
            Grocery grocery = new Grocery();
            grocery.GroceryName = suggestions[position];
            grocery.isFlags = "false";

            await firebaseHelper.UpdateGroceryList(user.userId, currListId, grocery);
            intent = new Intent(this, typeof(MyLists));
            intent.PutExtra("currlistposition", currListPos);
            StartActivity(intent);
            Finish();
        }

        //המשתמש רוצה להשתמש בפונקציית הקלטה כדי לשנות את הטקסט בשורת החיפוש, הפעולה 
        //מתרגמת את השמע לטקטס ומכניסה אותו לשורת החיפוש
        private void BtnRecord_Click(object sender, EventArgs e)
        {
            string rec = Android.Content.PM.PackageManager.FeatureMicrophone;
            if (rec != "android.hardware.microphone")
            {
                // no microphone, no recording. Disable the button and output an alert
                var alert = new AlertDialog.Builder(btnRecord.Context);
                alert.SetTitle("You don't seem to have a microphone to record with");
                alert.SetPositiveButton("OK", (Sender, E) =>
                {
                    btnRecord.Enabled = false;
                    return;
                });

                alert.Show();
            }
            else
                btnRecord.Click += delegate
                {
                    isRecording = !isRecording;
                    if (isRecording)
                    {
                        // create the intent and start the activity
                        var voiceIntent = new Intent(RecognizerIntent.ActionRecognizeSpeech);
                        voiceIntent.PutExtra(RecognizerIntent.ExtraLanguageModel, RecognizerIntent.LanguageModelFreeForm);


                        // if there is more then 1.5s of silence, consider the speech over
                        voiceIntent.PutExtra(RecognizerIntent.ExtraSpeechInputCompleteSilenceLengthMillis, 1500);
                        voiceIntent.PutExtra(RecognizerIntent.ExtraSpeechInputPossiblyCompleteSilenceLengthMillis, 1500);
                        voiceIntent.PutExtra(RecognizerIntent.ExtraSpeechInputMinimumLengthMillis, 15000);
                        voiceIntent.PutExtra(RecognizerIntent.ExtraMaxResults, 1);

                        // you can specify other languages recognised here, for example
                        // voiceIntent.PutExtra(RecognizerIntent.ExtraLanguage, Java.Util.Locale.German);
                        // if you wish it to recognise the default Locale language and German
                        // if you do use another locale, regional dialects may not be recognised very well

                        voiceIntent.PutExtra(RecognizerIntent.ExtraLanguage, Java.Util.Locale.Default);
                        StartActivityForResult(voiceIntent, VOICE);
                    }
                };
        }

        //כאשר המשתמש לוחץ על כפתור חזרה למסך הקודם, הפעולה תחזיר אותו למסך הצגת הרשימה
        private void BtnBack_Click(object sender, EventArgs e)
        {
            intent = new Intent(this, typeof(MyLists));
            intent.PutExtra("currlistposition", currListPos);
            StartActivity(intent);
            Finish();
        }

        //כאשר המשתמש לוחץ על כפתור 'הוספה', הפעולה תוסיף לרשימת הקניות פריט חדש
        //ששמו שווה לטקסט בשורת החיפוש (אם הוא לא ריק)י
        private async void BtnAdd_Click(object sender, EventArgs e)
        {
            if (etGroceryName.Text != string.Empty)
            {
                Grocery grocery = new Grocery();
                grocery.GroceryName = etGroceryName.Text;
                grocery.isFlags = "false";

                await firebaseHelper.UpdateGroceryList(user.userId, currListId, grocery);
                //if (currList.members != null)
                //    await firebaseHelper.UpdateGroceryListForAllMembers(currList.members, currListId, grocery);
                intent = new Intent(this, typeof(MyLists));
                intent.PutExtra("currlistposition", currListPos);
                StartActivity(intent);
                Finish();
            }
            else
                Toast.MakeText(this, "A grocery with no name?!", ToastLength.Short).Show();
        }

        //הפעולה מטפלת בתרגום השמע שחוזר כטקסט מפונקציית ההקלטה ושואלת האם להוסיף אותו כפריט לרשימה
        protected override void OnActivityResult(int requestCode, Result resultVal, Intent data)
        {
            if (requestCode == VOICE)
            {
                if (resultVal == Result.Ok)
                {
                    var matches = data.GetStringArrayListExtra(RecognizerIntent.ExtraResults);

                    if (matches.Count != 0)
                    {
                        temp = matches[0];
                        RYS = new Android.App.AlertDialog.Builder(this);
                        RYS.SetTitle("Grocery Addition");
                        RYS.SetMessage("Do you want to add " + matches[0] + " to " + user.groceryLists[currListPos].listName + "?");                      
                        RYS.SetCancelable(false);
                        RYS.SetPositiveButton("Yes", OK_Action);
                        RYS.SetNegativeButton("No", Abort_Action);
                        RYS.Create();
                        RYS.Show();
                    }
                    else
                        Toast.MakeText(this, "No speech was recognised", ToastLength.Short).Show();
                }
            }

            base.OnActivityResult(requestCode, resultVal, data);
        }

        //המשתמש אינו רוצה להוסיף מוצר עם שם כזה לרשימת הקניות
        private void Abort_Action(object sender, DialogClickEventArgs e)
        {
            RYS.Dispose();
        }

        //המשתמש כן רוצה להוסיף מוצר עם שם כזה לרשימת הקניות
        private async void OK_Action(object sender, DialogClickEventArgs e)
        {
            Grocery grocery = new Grocery();
            grocery.GroceryName = temp;
            grocery.isFlags = "false";


            await firebaseHelper.UpdateGroceryList(user.userId, currListId, grocery);
            intent = new Intent(this, typeof(MyLists));
            intent.PutExtra("currlistposition", currListPos);
            StartActivity(intent);
            Finish();
        }

        //הפעולה מכניסה מחרוזות לרשימת ההצעות של פריטים
        private List<string> GetSuggestions ()
        {
            List<string> suggestions = new List<string>();

            suggestions.Add("Banana");
            suggestions.Add("Dark Chocolate");
            suggestions.Add("Chicken Breast");
            suggestions.Add("Pecan Halves");
            suggestions.Add("Almonds");
            suggestions.Add("Watermelon");
            suggestions.Add("Lemon");
            suggestions.Add("Carrots");
            suggestions.Add("Cucumbers");
            suggestions.Add("Tomatoes");
            suggestions.Add("White Chocolate");
            suggestions.Add("Milk");
            suggestions.Add("Water");
            suggestions.Add("Coca Cola");
            suggestions.Add("Sprite");
            suggestions.Add("Ground Black Coffe");
            suggestions.Add("Peanut Butter");
            suggestions.Add("Hummus");
            suggestions.Add("Cottage Cheese");
            suggestions.Add("Cream Cheese");
            suggestions.Add("Cooking Oil");
            suggestions.Add("Olive Oil");
            suggestions.Add("Vodka");
            suggestions.Add("Soda");

            return suggestions;
        }
    }
}