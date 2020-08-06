namespace HeThongQuanLyDoAnCNTT.Controllers
{
    using HeThongQuanLyDoAnCNTT.CustomAuthentication;
    using HeThongQuanLyDoAnCNTT.DataAccess;
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Data.Entity;
    using System.Linq;
    using System.Net;
    using System.Security.Cryptography;
    using System.Web.Mvc;
    using HeThongQuanLyDoAnCNTT.Models;
    using System.Web;

    [CustomAuthorize(Roles = "Teacher,Student,Secretary,HeaderFaulity")]
    public class SubjectsController : Controller
    {
        #region Khởi tạo
        protected virtual CustomPrincipal CurrentUser
        {
            get { return User as CustomPrincipal; }
        }
        private DataEntities db = new DataEntities();
        #endregion

        #region DateTime
        private string day = string.Format("{0:00}", DateTime.Now.Day);
        private string month = string.Format("{0:00}", DateTime.Now.Month);
        private string year = DateTime.Now.Year.ToString();
        private string hour = string.Format("{0:00}", DateTime.Now.Hour);
        private string minute = string.Format("{0:00}", DateTime.Now.Minute);
        private string second = string.Format("{0:00}", DateTime.Now.Second);
        #endregion

        #region GetDaTa
        public Student GetCurrentStudent()
        {
            return db.Students.SingleOrDefault(student => student.ID_Account == CurrentUser.ID);
        }

        public Teacher GetCurrentTeacher()
        {
            return db.Teachers.SingleOrDefault(teacher => teacher.ID_Account == CurrentUser.ID);
        }

        public Teacher GetTeacherByID(int ID)
        {
            return db.Teachers.SingleOrDefault(teacher => teacher.ID == ID);
        }

        public bool IsStudentDoingSubject()
        {
            return db.Students.FirstOrDefault(student => student.ID_Account == CurrentUser.ID && student.ID_Subject != null) != null;
        }

        public bool IsCurSubjectInStuList(int IDSubject)
        {
            List<Student> ListStudent = db.Students.ToList();
            foreach(Student S in ListStudent)
            {
                if (S.ID_Subject == IDSubject)
                    return true;
            }
            return false;
        }

        public List<T_Subject> GetListSubjectEnable()
        {
            return db.T_Subject.Where(sub => sub.isDelete != true).ToList();
        }

        public bool IsSubjectNameinTSubject(string Subject_Name)
        {
            return db.T_Subject.Find(Subject_Name) != null;
        }
        public bool IsSubjectNameinSubject(string Subject_Name)
        {
            return db.Subjects.Find(Subject_Name) != null;
        }
        #endregion

        #region Hiển thị danh sách đề tài
        public ActionResult Index()
        {
            List<Subject> SubjectList = new List<Subject>();
            /* Danh sách đề tài giảng viên đang hướng dẫn */
            if (User.IsInRole("Teacher") == true)
            {
                SubjectList = db.Subjects.Where(s => s.Teacher.ID_Account == CurrentUser.ID && s.isDelete != true && s.isSubmit == true).ToList();
                return View(SubjectList);
            }
            /* Danh sách đề tài sinh viên đã và đang thực hiện */
            else if (User.IsInRole("Student") == true)
            {
                List<SubjectDetail> detail = db.SubjectDetails.Where(s => s.Student.ID_Account == CurrentUser.ID).ToList();
                if (detail != null)
                {
                    foreach (SubjectDetail item in detail)
                    {
                        Subject sub = db.Subjects.Find(item.ID_Subject);
                        SubjectList.Add(sub);
                    }
                }
                return View(SubjectList);
            }
            else
            {
                return View(db.Subjects.Include(s => s.Teacher).ToList());
            }
        }
        #endregion

        #region Hiển thị tên giảng viên theo đề tài được chọn
        public JsonResult GetTeacherName(string ID_Subject)
        {
            db.Configuration.ProxyCreationEnabled = false;
            T_Subject TeacherSubject = db.T_Subject.FirstOrDefault(sub => sub.ID_Subject == ID_Subject);
            string TeacherName = null;
            if (TeacherSubject != null)
            {
                TeacherName = db.Teachers.FirstOrDefault(teacher => teacher.ID == TeacherSubject.ID_Teacher).FullName;
            }
            return Json(TeacherName, JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region Thêm đề tài dựa trên đề tài có sẵn
        public bool SubjectAvaiable(Subject sub)
        {
            bool Result = false;
            if (sub != null)
            {
                if(sub.isDelete == true)
                {
                    Result = true;
                }
            }
            else
            {
                Result = true;
            }
            return Result;
        }
        public ActionResult CreateExist()
        {
            if (!IsStudentDoingSubject())
            {
                ViewBag.TeacherSubject = new SelectList(GetListSubjectEnable(), "ID_Subject", "T_SubjectName");
                return View();
            }
            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CreateExist([Bind(Include = "ID,ID_Subject,SubjectName,Contents,Description,ID_Teacher,DateBegin,isActive,IDSubjectType")]  Subject subject)
        {
            if (subject.ID_Subject != null)
            {
                Subject findsubject = db.Subjects.FirstOrDefault(s => s.ID_Subject == subject.ID_Subject);
                SubjectDetail Detail = new SubjectDetail();
                Student CurrentStudent = db.Students.FirstOrDefault(s => s.ID_Account == CurrentUser.ID);
                T_Subject TSubject = db.T_Subject.FirstOrDefault(s => s.ID_Subject == subject.ID_Subject);

                if (SubjectAvaiable(findsubject)) /* Tạo mới subject nếu nó đã bị xóa */
                {
                    subject.SubjectName = TSubject.T_SubjectName;
                    subject.DateBegin = month + "/" + day + "/" + year;
                    subject.ID_Teacher = int.Parse(TSubject.ID_Teacher.ToString());
                    subject.IDSubjectType = 1;
                    subject.isActive = true;

                    TSubject.isActive = false;
                    if (ModelState.IsValid)
                    {
                        db.Subjects.Add(subject);
                        db.SaveChanges();
                        CurrentStudent.ID_Subject = subject.ID;
                        db.SaveChanges();
                    }

                    Detail.ID_Student = CurrentStudent.ID;
                    Detail.ID_Subject = subject.ID;
                    Detail.PrecentComplete = 0;
                    if (ModelState.IsValid)
                    {
                        db.SubjectDetails.Add(Detail);
                        db.SaveChanges();
                    }
                }
                else if (findsubject.isSubmit != true) /* Nếu đề tài đã được người khác đăng ký nhưng chưa submit thì có thể đăng ký chung nhóm */
                {
                    Detail.ID_Subject = findsubject.ID;
                    Detail.ID_Student = CurrentStudent.ID;
                    Detail.PrecentComplete = 0;

                    CurrentStudent.ID_Subject = subject.ID;
                    if (ModelState.IsValid)
                    {
                        db.SubjectDetails.Add(Detail);
                        db.SaveChanges();
                    }
                }
                return RedirectToAction("Index");
            }
            return RedirectToAction("CreateExist");
        }
        #endregion

        #region Thêm đề tài sinh viên
        public ActionResult CreateNew()
        {
            if (!IsStudentDoingSubject())
            {
                GetTeacherList();
                return View();
            }
            return RedirectToAction("Index");
        }
        [HttpPost]
        public ActionResult CreateNew([Bind(Include = "ID,ID_Subject,SubjectName,Contents,Description,ID_Teacher,DateBegin,isActive,IDSubjectType")]  Subject subject)
        {
            if (subject.ID_Teacher != 0 && subject.SubjectName != null) 
            {
                if (!IsSubjectNameinSubject(subject.SubjectName) && !IsSubjectNameinTSubject(subject.SubjectName))
                {
                    String ID_Student = GetCurrentStudent().MSSV;
                    subject.DateBegin = month + "/" + day + "/" + year;
                    subject.ID_Subject = "DTTC";
                    subject.isActive = true;
                    subject.IDSubjectType = 1;
                    if (ModelState.IsValid)
                    {
                        db.Subjects.Add(subject);
                        db.SaveChanges();
                        subject.ID_Subject += string.Format("{0:0000}",subject.ID);
                        db.SaveChanges();
                    }
                }
                SubjectDetail Detail = new SubjectDetail();
                Detail.ID_Subject = db.Subjects.FirstOrDefault(s => s.ID_Subject == subject.ID_Subject).ID;
                Detail.ID_Student = db.Students.FirstOrDefault(student => student.ID_Account == CurrentUser.ID).ID;
                Detail.PrecentComplete = 0;
                Student CurrentStudent = GetCurrentStudent();
                CurrentStudent.ID_Subject = subject.ID;
                if (ModelState.IsValid)
                {
                    db.SubjectDetails.Add(Detail);
                    db.SaveChanges();
                }
                return RedirectToAction("Index");
            }
            return RedirectToAction("CreateNew");
        }
        #endregion
        
        #region Cập nhật thông tin đề tài
        public ActionResult Edit(int? id)
        {
            if (id != null)
            {
                Subject subject = db.Subjects.Find(id);
                if (subject != null)
                {
                    List<SubjectDetail> subdetail = db.SubjectDetails.Where(s => s.ID_Subject == id).ToList();
                    List<string> ListStudent = new List<string>();
                    foreach(SubjectDetail item in subdetail)
                    {
                        ListStudent.Add(item.Student.FullName);
                    }
                    ViewBag.StuList = ListStudent;
                    return View(subject);
                }
                return HttpNotFound();
            }
            return RedirectToAction("PageNotFound", "Error");
        }

        [HttpPost]
        public ActionResult Edit([Bind(Include = "ID,ID_Subject,SubjectName,Contents,Description,ID_Teacher,DateBegin,SubjectTarget,Result,isActive,isDelete,Feedback,isSubmit,isDone,IDSubjectType")] Subject subject)
        {
            if (ModelState.IsValid)
            {
                subject.DateBegin = string.Format("{0}/{1}/{2}", month, day, year);
                db.Entry(subject).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(subject);
        }
        #endregion

        #region Hủy đăng ký đề tài
        public bool CanDelete(Subject subject)
        {
            return (subject != null && subject.isDone != true && subject.isSubmit != true);
        }
        public ActionResult Delete(int? id)
        {
            if(id!=null)
            {
                Subject CurrentSubject = db.Subjects.Find(id);
                if (CanDelete(CurrentSubject))
                {
                    Student student = GetCurrentStudent();
                    student.ID_Subject = null;
                    SubjectDetail Detail = db.SubjectDetails.FirstOrDefault(detail => detail.ID_Student == student.ID && detail.ID_Subject == CurrentSubject.ID);
                    db.SubjectDetails.Remove(Detail);
                    if (!IsCurSubjectInStuList(CurrentSubject.ID))
                    {
                        CurrentSubject.isDelete = true;
                    }
                    db.SaveChanges();
                    return RedirectToAction("Index", "Home");
                }
                return RedirectToAction("Index");
            }
            return RedirectToAction("Index");
        }
        #endregion

        #region Submit đề tài
        public ActionResult SubmitSujectToTeacher(int? id)
        {
            if (id != null)
            {
                Subject subject = db.Subjects.Find(id);
                if(subject != null)
                {
                    subject.isSubmit = true;
                    db.SaveChanges();
                    Uri SubjectURL = new Uri("https://localhost:44351/Subjects/edit/" + subject.ID);
                    string Mail = GetCurrentStudent().Email;
                    //string Mail = GetTeacherWithSubjectID(Subject.ID_Teacher).Email;
                    string TeacherName = GetTeacherByID(subject.ID_Teacher).FullName;
                    string MailSubject = "Mail thông báo sinh viên đăng ký đề tài";
                    string MailBody = string.Format("Dear Thầy/Cô {0},\n\tSinh viên {1} đã Đăng ký đề tài\n\tTên đề tài: {2}\n\tLink đề tài: {3}\n\nVui lòng xác nhận\n\n\tThanks and Best Regards", TeacherName, GetCurrentStudent().FullName, subject.SubjectName, SubjectURL);
                    Mail NewEmail = new Mail(Mail, MailSubject, MailBody);
                    NewEmail.SendMail();
                    return View();
                }
                return HttpNotFound();
            }
            return RedirectToAction("PageNotFound", "Error");
        }
        #endregion

        #region Hoàn thành đề tài
        public bool SetSubjectDone(int ?ID_Subject)
        {
            Subject CurrentSubject = db.Subjects.Find(ID_Subject);
            if (CurrentSubject != null)
            {
                List<TrackingPaper> ListTrackingPapers = db.TrackingPapers.Where(s => s.ID_Subject == ID_Subject).ToList();
                List<SubjectDetail> ListStudentSubject = db.SubjectDetails.Where(s => s.ID_Subject == ID_Subject).ToList();
                if (ListTrackingPapers.Count >= 5)
                {
                    foreach (SubjectDetail Student in ListStudentSubject)
                    {
                        Student StudentSubject = db.Students.FirstOrDefault(s => s.ID == Student.ID_Student);
                        StudentSubject.ID_Subject = null;
                    }
                    CurrentSubject.isDone = true;
                    db.SaveChanges();
                    return true;
                }
                return false;
            }
            return false;
        }

        public ActionResult SubjectDone(int ?ID_Subject)
        {
            if (ID_Subject != null)
            {
                Subject CurrentSubject = db.Subjects.Find(ID_Subject);
                if (CurrentSubject != null)
                {
                    if (SetSubjectDone(ID_Subject))
                        return View();
                }
                return HttpNotFound();
            }
            return RedirectToAction("PageNotFound","Error");
        }
        #endregion

        #region Tìm kiếm đề tài
        public void GetTeacherList ()
        {
            List<Teacher> LstTeacher = db.Teachers.ToList();
            ViewBag.LstTeacherName = new SelectList(LstTeacher, "ID", "FullName");
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
