using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Firebase;
using Firebase.Database;
using Java.Util;
using System.Security;
using System;
using Android.Telephony;

namespace FamBasket_3_0
{
    [Activity(Label = "Registration", Theme = "@style/AppTheme")]
    public class Registration : Activity 
    {

        private EditText etUsername, etPassword, etPhoneNum;
        private Button btnOK, btnCancel;
        private FirebaseHelper firebaseHelper;
        private ProgressBar progressBar;
        Intent intent;
        ISharedPreferences sp;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            // Create your application here

            SetContentView(Resource.Layout.signup);

            btnOK = FindViewById<Button>(Resource.Id.btn_OK1);
            btnCancel = FindViewById<Button>(Resource.Id.btn_Cancel1);
            etUsername = FindViewById<EditText>(Resource.Id.et_NewUsername);
            etPassword = FindViewById<EditText>(Resource.Id.et_NewPassword);

            btnOK.Click += Reg_Click;
            btnCancel.Click += Cancel_Click;

            progressBar = FindViewById<ProgressBar>(Resource.Id.progressBar2);
            progressBar.Visibility = ViewStates.Invisible;
            firebaseHelper = new FirebaseHelper(progressBar);

            

        }

        /// <summary>
        /// באם המשתמש מבטל את ההרשמה ולוחץ על כפתור 'ביטול', הפעולה תחזיר את המשתמש למסך התחברות - המסך הראשי
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Cancel_Click(object sender, EventArgs e)
        {
            intent = new Intent(this, typeof(Login));
            StartActivity(intent);
            Finish();
        }

        /// <summary>
        /// באם המשתמש המשיך עם ההרשמה ונרשם בהצלחה, הפעולה תיצור עבורו משתמש חדש ותעביר אותו למסך הצגת רשימות הקניות
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void Reg_Click(object sender, EventArgs e)
        {
            progressBar.Visibility = ViewStates.Visible;
            if (await firebaseHelper.CheckIfUserExist(etUsername.Text))
            {
                progressBar.Visibility = ViewStates.Invisible;
                Toast.MakeText(this, "Username taken", ToastLength.Long).Show();
                etUsername.Text = string.Empty;
                etPassword.Text = string.Empty;
            }
            else 
            {
                if (etUsername.Text != string.Empty && etPassword.Text != string.Empty && ! await firebaseHelper.CheckIfUserExist(etUsername.Text))
                {
                    DatabaseReference reference = firebaseHelper.GetDatabase().GetReference("Users").Push();


                    firebaseHelper.AddUser(reference, etUsername.Text, etPassword.Text, false);

                    sp = this.GetSharedPreferences("curruserinfo", FileCreationMode.Private);
                    ISharedPreferencesEditor spEdit = sp.Edit();
                    spEdit.PutString("currusername", etUsername.Text);
                    spEdit.Commit();

                    etUsername.Text = string.Empty;
                    etPassword.Text = string.Empty;
                    progressBar.Visibility = ViewStates.Invisible;

                    intent = new Intent(this, typeof(MyListsChoose));
                    StartActivity(intent);
                    Finish();
                }
                else
                    Toast.MakeText(this, "Please make sure username is valid and that all fields are filled!", ToastLength.Long).Show();
            }
        }

        

    }
}