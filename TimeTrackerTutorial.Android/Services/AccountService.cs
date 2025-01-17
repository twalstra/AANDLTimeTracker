﻿using System;
using System.Threading.Tasks;
using Firebase;
using Firebase.Auth;
using Firebase.Firestore;
using Java.Util.Concurrent;
using TimeTrackerTutorial.Droid.ServiceListeners;
using TimeTrackerTutorial.Droid.Services;
using TimeTrackerTutorial.Models;
using TimeTrackerTutorial.Services.Account;
using Xamarin.Essentials;
using Xamarin.Forms;

[assembly: Dependency(typeof(AccountService))]
namespace TimeTrackerTutorial.Droid.Services
{
    public class AccountService : PhoneAuthProvider.OnVerificationStateChangedCallbacks, IAccountService
    {
        const int OTP_TIMEOUT = 30; // seconds
        private TaskCompletionSource<bool> _phoneAuthTcs;
        private string _verificationId;

        public AccountService()
        {
        }

        public Task<double> GetCurrentPayRateAsync()
        {
            return Task.FromResult(10d);
        }

        public Task<LoginResult> LoginAsync(string username, string password)
        {
            var tcs = new TaskCompletionSource<LoginResult>();
            FirebaseAuth.Instance.SignInWithEmailAndPasswordAsync(username.Trim(), password)
                .ContinueWith((task) => OnAuthCompleted(task, tcs));
            return tcs.Task;
        }

        public override void OnVerificationCompleted(PhoneAuthCredential credential)
        {
            System.Diagnostics.Debug.WriteLine("PhoneAuthCredential created Automatically");
        }

        public override void OnVerificationFailed(FirebaseException exception)
        {
            System.Diagnostics.Debug.WriteLine("Verification Failed: " + exception.Message);
            _phoneAuthTcs?.TrySetResult(false);
        }

        public override void OnCodeSent(string verificationId, PhoneAuthProvider.ForceResendingToken forceResendingToken)
        {
            base.OnCodeSent(verificationId, forceResendingToken);
            _verificationId = verificationId;
            _phoneAuthTcs?.TrySetResult(true);
        }

        public Task<bool> SendOtpCodeAsync(string phoneNumber)
        {
            _phoneAuthTcs = new TaskCompletionSource<bool>();
            PhoneAuthProvider.Instance.VerifyPhoneNumber(
                phoneNumber,
                OTP_TIMEOUT,
                TimeUnit.Seconds,
                Platform.CurrentActivity,
                this);
            return _phoneAuthTcs.Task;
        }

        private void OnAuthCompleted(Task task, TaskCompletionSource<LoginResult> tcs)
        {
            if (task.IsCanceled || task.IsFaulted)
            {
                // something went wrong
                tcs.SetResult(LoginResult.FromError(task.Exception?.Message ?? "Canceled, look into this (TODO)"));
                return;
            }
            _verificationId = null;
            tcs.SetResult(LoginResult.FromSuccess());
        }

        public Task<LoginResult> VerifyOtpCodeAsync(string code)
        {
            if (!string.IsNullOrWhiteSpace(_verificationId))
            {
                var credential = PhoneAuthProvider.GetCredential(_verificationId, code);
                var tcs = new TaskCompletionSource<LoginResult>();
                FirebaseAuth.Instance.SignInWithCredentialAsync(credential)
                    .ContinueWith((task) => OnAuthCompleted(task, tcs));
                return tcs.Task;
            }
            return Task.FromResult(LoginResult.FromError("Verfication ID is null"));
        }

        public Task<AuthenticatedUser> GetUserAsync()
        {
            var tcs = new TaskCompletionSource<AuthenticatedUser>();

            FirebaseFirestore.Instance
                .Collection("users")
                .Document(FirebaseAuth.Instance.CurrentUser.Uid)
                .Get()
                .AddOnCompleteListener(new OnCompleteListener(tcs));

            return tcs.Task;
        }
    }
}
