using Android.App;
using Android.OS;
using Android.Support.V7.App;
using Android.Runtime;
using Android.Widget;
using Android.Views;
using Android;
using System;
using Android.Content;
using Firebase;
using Firebase.Database;

namespace FamBasket_3_0
{
    [Activity(Label = "@string/app_name", Theme = "@style/AppTheme", MainLauncher = true)]
    public class Login : AppCompatActivity, View.IOnClickListener
    {

        private EditText etUsername, etPassword;
        private Button btnLogin, btnSignUp, btnGuest;
        ProgressBar progressBar;
        Intent intent;
        FirebaseHelper firebaseHelper;
        ISharedPreferences sp;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            firebaseHelper = new FirebaseHelper();
            // Create your application here

            SetContentView(Resource.Layout.login);

            etUsername = FindViewById<EditText>(Resource.Id.et_Username);
            etPassword = FindViewById<EditText>(Resource.Id.et_Password);
            btnLogin = FindViewById<Button>(Resource.Id.btn_Login);
            btnSignUp = FindViewById<Button>(Resource.Id.btn_SignUp1);
            btnGuest = FindViewById<Button>(Resource.Id.btn_Guest);
            progressBar = FindViewById<ProgressBar>(Resource.Id.progressBar1);

            progressBar.Visibility = ViewStates.Invisible;
            firebaseHelper = new FirebaseHelper(progressBar);

            btnLogin.SetOnClickListener(this);
            btnSignUp.SetOnClickListener(this);
            btnGuest.SetOnClickListener(this);

        }
        /// <summary>
        /// הפעולה מטפלת באירועי הלחיצה של כפתור ההתחברות וכפתור ההרשמה 
        /// </summary>
        /// <param name="v">מייצג את משתנה העיצוב של האוייבקט שהפעולה מקבלת כפרמטר</param>
        public async void OnClick(View v)
        {

            if (v == btnLogin)
            {
                progressBar.Visibility = ViewStates.Visible;
                if (await firebaseHelper.CheckIfUserExist(etUsername.Text) && await firebaseHelper.CheckIfPasswordCorrect(etUsername.Text, etPassword.Text))
                {
                    sp = this.GetSharedPreferences("curruserinfo", FileCreationMode.Private);
                    ISharedPreferencesEditor spEdit = sp.Edit();
                    spEdit.PutString("currusername", etUsername.Text);
                    spEdit.Commit();

                    await firebaseHelper.ImportParticipatedLists(etUsername.Text);

                    progressBar.Visibility = ViewStates.Invisible;
                    intent = new Intent(this, typeof(MyListsChoose));
                    StartActivity(intent);
                    Finish();
                }
                else if (etUsername.Text == string.Empty || etPassword.Text != string.Empty)
                    Toast.MakeText(this, "Please make sure you filled all fields! ", ToastLength.Short).Show();
                else if (!await firebaseHelper.CheckIfUserExist(etUsername.Text) || !await firebaseHelper.CheckIfPasswordCorrect(etUsername.Text, etPassword.Text))
                    Toast.MakeText(this, "User not exists or password is incorrect! ", ToastLength.Short).Show();
                
            }

            if (v == btnSignUp)
            {


                intent = new Intent(this, typeof(Registration));

                StartActivity(intent);
                Finish();

            }

            if (v == btnGuest)
            {
                DatabaseReference reference = firebaseHelper.GetDatabase().GetReference("Users").Push();

                Classes.Guest guest = new Classes.Guest();

                sp = this.GetSharedPreferences("curruserinfo", FileCreationMode.Private);
                ISharedPreferencesEditor spEdit = sp.Edit();
                spEdit.PutString("currusername", guest.username);
                spEdit.Commit();

                firebaseHelper.AddUser(reference, guest.username, guest.password, guest.isGuest);

                intent = new Intent(this, typeof(MyListsChoose));
                StartActivity(intent);
                Finish();
            }

        }

    }
}