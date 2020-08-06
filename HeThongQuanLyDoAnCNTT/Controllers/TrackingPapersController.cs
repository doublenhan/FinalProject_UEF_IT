using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using HeThongQuanLyDoAnCNTT.CustomAuthentication;
using HeThongQuanLyDoAnCNTT.DataAccess;

namespace HeThongQuanLyDoAnCNTT.Controllers
{
    [CustomAuthorize(Roles = "Teacher,Student,Secretary,HeaderFaulity")]
    public class TrackingPapersController : Controller
    {
        protected virtual CustomPrincipal CurrentUser
        {
            get { return User as CustomPrincipal; }
        }

        private DataEntities db = new DataEntities();

        #region Ngày tháng năm
        public string GetCurrentDay()
        {
            string Day, Month, Year;
            Day = DateTime.Now.Day.ToString();
            Month = DateTime.Now.Month.ToString();
            Year = DateTime.Now.Year.ToString();
            return string.Format("{0}/{1}/{2}", Month, Day, Year);
        }
        #endregion

        #region Xem danh sách báo cáo
        [CustomAuthorize(Roles = "Teacher,Student,Secretary,HeaderFaulity")]
        public ActionResult Index()
        {
            if(User.IsInRole("Teacher,Student,Secretary,HeaderFaulity") == true)
            {
                var TrackingPaper = db.TrackingPapers.Include(t => t.Student).Include(t => t.Subject);
                return View(TrackingPaper.ToList());
            }          
             return RedirectToAction("AccessDenied", "Error");         
        }
        #endregion

        #region Download báo cáo
        [CustomAuthorize(Roles = "Teacher,Student,Secretary,HeaderFaulity")]
        public FileResult Download(string fileName)
        {
            string fullPath = Path.Combine(Server.MapPath("/FileUpload"), fileName);
            byte[] fileBytes = System.IO.File.ReadAllBytes(fullPath);
            return File(fileBytes, System.Net.Mime.MediaTypeNames.Application.Octet, fileName);
        }
        #endregion

        #region Nộp báo cáo mới
        [CustomAuthorize(Roles = "Teacher,Student,Secretary,HeaderFaulity")]
        [HandleError]
        [HttpGet]
        public ActionResult Create()
        {
            int ID_Student = db.Students.SingleOrDefault(student => student.ID_Account == CurrentUser.ID).ID;
            if (CheckSubjectID(db.SubjectDetails.FirstOrDefault(subject => subject.ID_Student == ID_Student).ID_Subject))
            {
                return View();
            }
            TempData["WarningMessage"] = "Đề tài chưa được kiểm duyệt hoặc đã hoàn thành, không thể nộp báo cáo!";
            return RedirectToAction("Index");
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(TrackingPaper trackingPaper, HttpPostedFileBase file)
        {
            trackingPaper.ID_Student = db.Students.SingleOrDefault(student => student.ID_Account == CurrentUser.ID).ID;
            trackingPaper.ID_Subject = db.SubjectDetails.FirstOrDefault(subject => subject.ID_Student == trackingPaper.ID_Student).ID_Subject;
            var pro = db.TrackingPapers.SingleOrDefault(c => c.ID.Equals(trackingPaper.ID));
            if (file != null)
            {
                if (file.ContentLength > 0)
                {
                    try
                    {
                        string nameFile = Path.GetFileName(file.FileName);
                        file.SaveAs(Path.Combine(Server.MapPath("/FileUpload"), nameFile));
                        trackingPaper.FileUpload = nameFile;
                    }
                    catch (Exception)
                    {
                        ViewBag.CreateProError = "Không thể chọn file.";
                    }
                }
                trackingPaper.TrackingTime = GetCurrentDay();              
                try
                {
                    if (pro != null)
                    {
                        ViewBag.CreateProError = "Mã đề tài ko tồn tại.";
                    }
                    else
                    {
                        db.TrackingPapers.Add(trackingPaper);
                        db.SaveChanges();
                        TempData["SuccessMessage"] = " Nộp báo cáo thành công";
                        return RedirectToAction("Index");
                    }
                }
                catch (Exception)
                {
                    return View(trackingPaper);
                }
            }
            else
            {
                ViewBag.FileUpload = "Chọn file đính kèm";
            }
            return View(trackingPaper);
        }
        #endregion

        #region Kiểm tra mã đề tài
        public bool CheckSubjectID (int ?SubjectID)
        {
            return db.Subjects.FirstOrDefault(s => s.ID == SubjectID && s.isSubmit == true && s.isDone != true) != null;
        }
        #endregion

        #region Chỉnh sửa báo cáo
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            TrackingPaper trackingPaper = db.TrackingPapers.Find(id);
            if (trackingPaper == null)
            {
                return HttpNotFound();
            }
            ViewBag.ID_Student = new SelectList(db.Students, "ID", "FullName", trackingPaper.ID_Student);
            ViewBag.ID_Subject = new SelectList(db.Subjects, "ID", "ID_Subject", trackingPaper.ID_Subject);
            return View(trackingPaper);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "ID,Contents,Description,TrackingTime,ID_Student,ID_Subject,FileUpload")] TrackingPaper trackingPaper)
        {
            if (ModelState.IsValid)
            {
                db.Entry(trackingPaper).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.ID_Student = new SelectList(db.Students, "ID", "FullName", trackingPaper.ID_Student);
            ViewBag.ID_Subject = new SelectList(db.Subjects, "ID", "ID_Subject", trackingPaper.ID_Subject);
            return View(trackingPaper);
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
