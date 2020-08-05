using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Web;
using System.Web.Mvc;
using HeThongQuanLyDoAnCNTT.CustomAuthentication;
using HeThongQuanLyDoAnCNTT.DataAccess;
using System.Data.SqlClient;

namespace HeThongQuanLyDoAnCNTT.Controllers
{
    [CustomAuthorize(Roles = "Teacher,Student,Secretary,HeaderFaulity")]
    public class T_SubjectController : Controller
    {
        #region Khởi tạo
        protected virtual CustomPrincipal CurrentUser
        {
            get { return User as CustomPrincipal; }
        }
        private DataEntities db = new DataEntities();
        #endregion

        #region List Đề tài giảng viên

        [CustomAuthorize(Roles = "Teacher,Student,Secretary,HeaderFaulity")]
        // GET: T_Subject
        public ActionResult Index(int ?TeacherID, string SubjectID, string SubjectName)
        {
            if (CurrentUser.IsInRole("Teacher") == true)
            {
                var CheckID = db.T_Subject.Where(s => s.Teacher.ID_Account == CurrentUser.ID);
                return View(CheckID.ToList());
            }
            if (CurrentUser.IsInRole("Student") == true)
            {
                GetTeacherList();
                return View(SearchSubject(TeacherID,SubjectID,SubjectName));
            }
            else
            {
                var t_Subject = db.T_Subject.Include(t => t.Teacher);
                return View(t_Subject.ToList());
            }
        }
        #endregion

        #region Kiểm tra đề tài có thể xóa hay không
        #endregion

        #region Thêm mới đề tài giảng viên
        public class SubjectType
        {
            public string ID;
            public string Name;

            public SubjectType(string id, string name)
            {
                this.ID = id;
                this.Name = name;
            }

        }

        [CustomAuthorize(Roles = "Teacher")]
        public ActionResult Create()
        {
            List<SubjectType> lstType = new List<SubjectType>() {
                new SubjectType("TMDT","Thương mại điện tử"),
                new SubjectType("CNTT","Công nghệ thông tin")
            };
            ViewBag.SubjectType = new SelectList(lstType, "ID", "Name");
            ViewBag.ID_Teacher = new SelectList(db.Teachers, "ID", "FullName");

            return View();
        }

        [CustomAuthorize(Roles = "Teacher")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "ID,ID_Subject,T_SubjectName,Contents,Description,ID_Teacher,isActive,isDelete")] T_Subject t_Subject)
        {
            bool SubjectNameIsExist = db.T_Subject.FirstOrDefault(s => s.T_SubjectName == t_Subject.T_SubjectName) != null;
            if (t_Subject.ID_Subject != null)
            {
                if (!SubjectNameIsExist)
                {
                    t_Subject.ID_Teacher = db.Teachers.FirstOrDefault(s => s.ID_Account == CurrentUser.ID).ID;
                    db.T_Subject.Add(t_Subject);
                    db.SaveChanges();
                    t_Subject.ID_Subject += string.Format("{0:0000}", t_Subject.ID);
                    db.SaveChanges();
                    TempData["SuccessMessage"] = " Thêm đề tài mới thành công!";
                    return RedirectToAction("Index");
                }
                TempData["WarningMessage"] = "Tên đề tài đã tồn tại, vui lòng chọn tên khác!";
                return RedirectToAction("Create");
            }
            TempData["WarningMessage"] = "Vui lòng chọn loại đề tài!";
            return RedirectToAction("Create");
        }
        #endregion

        #region Chỉnh sửa đề tài giảng viên
        [CustomAuthorize(Roles = "Teacher")]
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return RedirectToAction("PageNotFound", "Error");
            }
            T_Subject t_Subject = db.T_Subject.Find(id);
            if (t_Subject == null)
            {
                return HttpNotFound();
            }
            return View(t_Subject);
        }

        [CustomAuthorize(Roles = "Teacher")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "ID,ID_Subject,T_SubjectName,Contents,Description,ID_Teacher,isActive,isDelete")] T_Subject t_Subject)
        {
            try
            {
                db.Entry(t_Subject).State = EntityState.Modified;
                db.SaveChanges();
                TempData["SuccessMessage"] = " Chỉnh Sửa Thành Công";
                return RedirectToAction("Index");
            }
            catch (Exception)
            {
                TempData["ErrorMessage"] = "Không thể chỉnh sửa";
            }
            ViewBag.ID_Teacher = new SelectList(db.Teachers, "ID", "FullName", t_Subject.ID_Teacher);
            return View(t_Subject);
        }
        #endregion
        
        #region Xóa đề tài Giảng viên
        public bool DeleteOrNot(string ID_Subject)
        {
            if (db.Subjects.FirstOrDefault(s => s.ID_Subject == ID_Subject) == null)
                return true;
            return false;
        }

        [CustomAuthorize(Roles = "Teacher,HeaderFaulity")]
        public ActionResult Delete(int? id)
        {
            if (id != null)
            {
                T_Subject t_Subject = db.T_Subject.Find(id);
                if (t_Subject != null)
                {
                    if(DeleteOrNot(t_Subject.ID_Subject))
                    {
                        db.T_Subject.Remove(t_Subject);
                        db.SaveChanges();
                        return RedirectToAction("Index");
                    }
                    TempData["WarningMessage"] = "Đề tài đã được đăng ký, không thể xóa!";
                    return RedirectToAction("Index");
                }
                TempData["WarningMessage"] = "Đề tài không tồn tại hoặc đã bị xóa!";
                return RedirectToAction("Index");
            }
            return RedirectToAction("PageNotFound", "Error");
        }
        #endregion

        #region Tìm kiếm đề tài
        public void GetTeacherList()
        {
            List<Teacher> LstTeacher = db.Teachers.ToList();
            ViewBag.LstTeacherName = /*new SelectList(LstTeacher, "ID", "FullName")*/db.Teachers.ToList();
        }
        public List<T_Subject> SearchSubject(int ?IDTeacher, string IDSubject, string SubjectName)
        {
            List<T_Subject> LstTSubject = new List<T_Subject>();
            string SQLQuery = "Select * from T_Subject Where (isDelete = 0 or isDelete is null) and isactive = 1";
            if(IDTeacher != null)
            {
                SQLQuery += " and ID_Teacher = " + IDTeacher;
            }
            if(IDSubject != null)
            {
                SQLQuery += " and ID_Subject like '" + IDSubject + "%'";
            }
            if(SubjectName != null)
            {
                SQLQuery += " and T_SubjectName like '" + SubjectName + "%'";
            }
            LstTSubject = db.T_Subject.SqlQuery(SQLQuery).ToList();
            return LstTSubject;
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
