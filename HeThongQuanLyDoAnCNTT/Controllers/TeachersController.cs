using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using HeThongQuanLyDoAnCNTT.CustomAuthentication;
using HeThongQuanLyDoAnCNTT.DataAccess;

namespace HeThongQuanLyDoAnCNTT.Controllers
{
    [CustomAuthorize(Roles = "Admin,Teacher,Secretary,HeaderFaulity")]
    public class TeachersController : Controller
    {
        private DataEntities db = new DataEntities();

        #region List giảng viên
        // GET: Teachers
        [CustomAuthorize(Roles = "HeaderFaulity,Admin,Secretary")]
        public ActionResult Index()
        {
            return View(db.Teachers.ToList());
        }
        #endregion

        #region Thêm mới giảng viên
        [CustomAuthorize(Roles = "HeaderFaulity,Admin")]
        // GET: Teachers/Create
        public ActionResult Create()
        {
            return View();
        }

        [CustomAuthorize(Roles = "HeaderFaulity,Admin")]     
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "ID,FullName,MSGV,NumberPhone,Email,DateofBirth,Address,DateBegin,ID_Roles,ID_Subjects,IdentificationCard,ID_T_Subjects")] Teacher teacher)
        {
            if (ModelState.IsValid)
            {
                db.Teachers.Add(teacher);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(teacher);
        }
        #endregion

        #region Chỉnh sử thông tin giảng viên
        [CustomAuthorize(Roles = "Teacher")]
        // GET: Teachers/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Teacher teacher = db.Teachers.Find(id);
            if (teacher == null)
            {
                return HttpNotFound();
            }
            return View(teacher);
        }
        [CustomAuthorize(Roles = "Teacher")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "ID,FullName,MSGV,NumberPhone,Email,DateofBirth,Address,DateBegin,ID_Roles,ID_Subjects,IdentificationCard,ID_T_Subjects")] Teacher teacher)
        {
            if (ModelState.IsValid)
            {
                db.Entry(teacher).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(teacher);
        }
        #endregion

        #region Dispose
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
        #endregion
    }
}
