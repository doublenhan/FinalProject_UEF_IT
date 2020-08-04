using HeThongQuanLyDoAnCNTT.CustomAuthentication;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace HeThongQuanLyDoAnCNTT.Controllers
{
    [CustomAuthorize(Roles = "Admin,Teacher,Student,Secretary,HeaderFaulity")]
    public class HomeController : Controller
    {        
        public ActionResult Index()
        {
            return View();
        }
    }
}