﻿using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Duo;

namespace DuoWebTests
{
    [TestClass]
    public class DuoWebTests
    {
        /* Dummy IKEY and SKEY values */
        const string IKEY = "DIXXXXXXXXXXXXXXXXXX";
        const string SKEY = "deadbeefdeadbeefdeadbeefdeadbeefdeadbeef";
        const string AKEY = "useacustomerprovidedapplicationsecretkey";

        /* Dummy username */
        const string USER = "testuser";

        /* Dummy response signatures */
        const string INVALID_RESPONSE = "AUTH|INVALID|SIG";
        const string EXPIRED_RESPONSE = "AUTH|dGVzdHVzZXJ8RElYWFhYWFhYWFhYWFhYWFhYWFh8MTMwMDE1Nzg3NA==|cb8f4d60ec7c261394cd5ee5a17e46ca7440d702";
        const string FUTURE_RESPONSE = "AUTH|dGVzdHVzZXJ8RElYWFhYWFhYWFhYWFhYWFhYWFh8MTYxNTcyNzI0Mw==|d20ad0d1e62d84b00a3e74ec201a5917e77b6aef";
        const string OLD_REQUEST_APP_SIG = "APP|dGVzdHVzZXJ8RElYWFhYWFhYWFhYWFhYWFhYWFh8MTMwMDE2MTM5OQ==|c3648befd92041b26197af8a976300542f00cd5a";
        const string OLD_REQUEST = "TX|dGVzdHVzZXJ8RElYWFhYWFhYWFhYWFhYWFhYWFh8MTMwMDE1ODA5OQ==|815423f20909dbff2bc4962fdc3031d5f673bc1b:APP|dGVzdHVzZXJ8RElYWFhYWFhYWFhYWFhYWFhYWFh8MTMwMDE2MTM5OQ==|c3648befd92041b26197af8a976300542f00cd5a";

        private string valid_app_sig;
        private string invalid_app_sig;

        [TestInitialize()]
        public void SetUp()
        {
            string request_sig;
            string[] sigs;

            request_sig = Duo.Web.SignRequest(IKEY, SKEY, AKEY, USER);
            sigs = request_sig.Split(':');
            valid_app_sig = sigs[1];

            request_sig = Duo.Web.SignRequest(IKEY, SKEY, "invalidinvalidinvalidinvalidinvalidinvalid", USER);
            sigs = request_sig.Split(':');
            invalid_app_sig = sigs[1];
        }

        [TestMethod]
        public void TestSign()
        {
            string request_sig = Duo.Web.SignRequest(IKEY, SKEY, AKEY, USER);
            Assert.IsNotNull(request_sig);
        }

        [TestMethod]
        public void TestSignWithCustomTime()
        {
            Int64 fake_current_time = 1300157874 - 75;
            DateTime fake_current_dt = new DateTime(1970, 1, 1).AddSeconds(fake_current_time);
            string request_sig = Duo.Web.SignRequest(IKEY, SKEY, AKEY, USER, fake_current_dt);
            Assert.AreEqual(request_sig, OLD_REQUEST);
        }

        [TestMethod]
        public void TestSignEmptyUsername()
        {
            string request_sig = Duo.Web.SignRequest(IKEY, SKEY, AKEY, "");
            Assert.AreEqual(request_sig, Duo.Web.ERR_USER);
        }

        [TestMethod]
        public void TestSignBadIkey()
        {
            string request_sig = Duo.Web.SignRequest("invalid", SKEY, AKEY, USER);
            Assert.AreEqual(request_sig, Duo.Web.ERR_IKEY);
        }

        [TestMethod]
        public void TestSignBadSkey()
        {
            string request_sig = Duo.Web.SignRequest(IKEY, "invalid", AKEY, USER);
            Assert.AreEqual(request_sig, Duo.Web.ERR_SKEY);
        }

        [TestMethod]
        public void TestSignBadAkey()
        {
            string request_sig = Duo.Web.SignRequest(IKEY, SKEY, "invalid", USER);
            Assert.AreEqual(request_sig, Duo.Web.ERR_AKEY);
        }

        [TestMethod]
        public void TestVerifyInvalidUser()
        {
            string invalid_user = Duo.Web.VerifyResponse(IKEY, SKEY, AKEY, INVALID_RESPONSE + ":" + valid_app_sig);
            Assert.IsNull(invalid_user);
        }

        [TestMethod]
        public void TestVerifyExpiredUser()
        {
            string expired_user = Duo.Web.VerifyResponse(IKEY, SKEY, AKEY, EXPIRED_RESPONSE + ":" + valid_app_sig);
            Assert.IsNull(expired_user);
        }

        [TestMethod]
        public void TestVerifyFutureUserInvalidAppSig()
        {
            string future_user = Duo.Web.VerifyResponse(IKEY, SKEY, AKEY, FUTURE_RESPONSE + ":" + invalid_app_sig);
            Assert.IsNull(future_user);
        }

        [TestMethod]
        public void TestVerifyFutureUserValidAppSig()
        {
            string future_user = Duo.Web.VerifyResponse(IKEY, SKEY, AKEY, FUTURE_RESPONSE + ":" + valid_app_sig);
            Assert.AreEqual(future_user, USER);
        }

        [TestMethod]
        public void TestVerifyExpiredUserWithCustomTime()
        {
            Int64 fake_current_unixtime = 1300157874 - 60;
            DateTime fake_current_dt = new DateTime(1970, 1, 1).AddSeconds(fake_current_unixtime);

            string expired_user = Duo.Web.VerifyResponse(IKEY, SKEY, AKEY, EXPIRED_RESPONSE + ":" + OLD_REQUEST_APP_SIG, fake_current_dt);
            Assert.AreEqual(expired_user, USER);
        }
    }
}