using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ASPSnippets.GoogleAPI;
using System.Web.Script.Serialization;
using System.Net;
using HeThongQuanLyDoAnCNTT.Models;
using HeThongQuanLyDoAnCNTT.DataAccess;
using System.Web.Security;
using HeThongQuanLyDoAnCNTT.CustomAuthentication;
using Newtonsoft.Json;

namespace HeThongQuanLyDoAnCNTT.Controllers
{
    [AllowAnonymous]
    public class LoginController : Controller
    {
        private DataEntities db = new DataEntities();
        // GET: Login
        #region Index Login
        [AllowAnonymous]
        public ActionResult Index()
        {
            return View();
        }
        #endregion

        #region Logout
        public ActionResult LogOut()
        {
            HttpCookie cookie = new HttpCookie("Cookie1", "");
            cookie.Expires = DateTime.Now.AddYears(-1);
            Response.Cookies.Add(cookie);

            FormsAuthentication.SignOut();
            return RedirectToAction("Index", "Login", null);
        }
        #endregion

        #region Login Google uing Google API
        [HttpPost]
        [ValidateAntiForgeryToken]
        public void LoginWithGooglePlus()
        {
            GoogleConnect.ClientId = "164802556545-rvmd1u9vu1es5ah51tvte7554t9kh4h6.apps.googleusercontent.com";
            GoogleConnect.ClientSecret = "ak0LpmTv8wZI3AVLnyI-1sg3";
            GoogleConnect.RedirectUri = Request.Url.AbsoluteUri.Split('?')[0];
            GoogleConnect.Authorize("profile","email");
        }

        [ActionName("LoginWithGooglePlus")]
        public ActionResult LoginWithGooglePlusConfirmed()
        {
            if (!string.IsNullOrEmpty(Request.QueryString["code"]))
            {
                string code = Request.QueryString["code"];
                string json = GoogleConnect.Fetch("me", code);
                GoogleProfile profile = new JavaScriptSerializer().Deserialize<GoogleProfile>(json);

                DataEntities db = new DataEntities();

                Account item = db.Accounts.FirstOrDefault(S => S.Email == profile.email);

                if (item != null)
                {
                    Account itemset = new Account();
                    itemset.Email = item.Email;
                    itemset.PassWord = item.PassWord;
                    LoginGoogle(itemset, "/");
                }
                else
                {
                    return RedirectToAction("Index");
                }
            }
            if (Request.QueryString["error"] == "access_denied")
            {
                return Content("access_denied");
            }
            return RedirectToAction("Index", "Home");
        }
        #endregion

        #region Login Google 
        public ActionResult LoginGoogle(Account acc, string ReturnUrl)
        {
            try
            {
                if (Membership.ValidateUser(acc.Email, acc.PassWord))
                {
                    var user = (CustomMembershipUser)Membership.GetUser(acc.Email, false);
                    if (user != null)
                    {
                        CustomSerializeModel userModel = new Models.CustomSerializeModel()
                        {
                            ID = user.ID,
                            FirstName = user.FirstName,
                            LastName = user.LastName,
                            RoleName = user.Roles.Select(s => s.ID_Role.ToString()).ToList()
                        };

                        userModel.RoleName = db.Roles.Where(S => userModel.RoleName.Contains(S.ID.ToString())).Select(x => x.Role_Name).ToList();

                        string userData = JsonConvert.SerializeObject(userModel);
                        FormsAuthenticationTicket authTicket = new FormsAuthenticationTicket
                            (
                           1, acc.Email, DateTime.Now, DateTime.Now.AddMinutes(15), false, userData
                            );
                        string enTicket = FormsAuthentication.Encrypt(authTicket);
                        HttpCookie faCookie = new HttpCookie("Cookie1", enTicket);
                        Response.Cookies.Add(faCookie);
                    }
                    if (Url.IsLocalUrl(ReturnUrl))
                    {
                        return Redirect(ReturnUrl);
                    }
                    else
                    {
                        return RedirectToAction("Index", "Home");
                    }
                }
                return View("Index");
            }
            catch (Exception)
            {
                return View();
            }
        }
        #endregion

        #region Login Email & Password      
        public ActionResult Login(Account acc, string ReturnUrl)
        {
            try
            {
                if (Membership.ValidateUser(acc.Email, acc.PassWord))
                {

                    var user = (CustomMembershipUser)Membership.GetUser(acc.Email, false);
                    if (user != null)
                    {

                        CustomSerializeModel userModel = new Models.CustomSerializeModel()
                        {
                            ID = user.ID,
                            FirstName = user.FirstName,
                            LastName = user.LastName,
                            RoleName = user.Roles.Select(s => s.ID_Role.ToString()).ToList()
                        };

                        userModel.RoleName = db.Roles.Where(S => userModel.RoleName.Contains(S.ID.ToString())).Select(x => x.Role_Name).ToList();

                        string userData = JsonConvert.SerializeObject(userModel);
                        FormsAuthenticationTicket authTicket = new FormsAuthenticationTicket
                            (
                           1, acc.Email, DateTime.Now, DateTime.Now.AddMinutes(15), false, userData
                            );

                        string enTicket = FormsAuthentication.Encrypt(authTicket);
                        HttpCookie faCookie = new HttpCookie("Cookie1", enTicket);
                        Response.Cookies.Add(faCookie);

                    }
                    if (Url.IsLocalUrl(ReturnUrl))
                    {
                        return Redirect(ReturnUrl);
                    }
                    else
                    {
                        return RedirectToAction("Index", "Home");
                    }
                }
                return View("Index");
            }
            catch (Exception)
            {
                return View();
            }
        }
        #endregion
      
    }
}