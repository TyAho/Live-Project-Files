using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using ManagementPortal.Models;

namespace ManagementPortal.Controllers
{
    public class PersonalProfilesController : ApplicationBaseController
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: PersonalProfiles
        public ActionResult Index()
        {
            var personalProfiles = db.Profile.Include(p => p.Person);
            return View(personalProfiles.ToList());
        }

        // GET: PersonalProfiles/Details/5
        public ActionResult Details(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            PersonalProfile PersonalProfiles = db.Profile.Find(id);
            if (PersonalProfiles == null)
            {
                return HttpNotFound();
            }
            return View(PersonalProfiles);
        }

        // GET: PersonalProfiles/Create
        public ActionResult Create()
        {
            ViewBag.ProfileID = new SelectList(db.Users, "Id", "DisplayName");
            return View();
        }

        // POST: PersonalProfiles/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "ProfileID,AboutMe,TagLine")] PersonalProfile PersonalProfiles)
        {
            if (ModelState.IsValid)
            {
                db.Profile.Add(PersonalProfiles);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.ProfileID = new SelectList(db.Users, "Id", "DisplayName", PersonalProfiles.ProfileID);
            return View(PersonalProfiles);
        }

        // GET: PersonalProfiles/Edit/5
        public ActionResult Edit(string id, string ProfileID)
        {
            //var ProfileID = Request.QueryString["id"];
            //return View();



            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            PersonalProfile PersonalProfiles = db.Profile.Find(id);
            if (PersonalProfiles == null)
            {
                return HttpNotFound();
            }
            ViewBag.ProfileID = new SelectList(db.Users, "ProfileID", "DisplayName", PersonalProfiles.ProfileID);
            return View(PersonalProfiles);
        }

        // POST: PersonalProfiles/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "ProfileID,AboutMe,TagLine")] PersonalProfile PersonalProfiles, IndexViewModel ProfileID)
        {
            //var ProfileID = Request["ProfileID"];
            //var AboutMe = Request["About Me"];
            //var TagLine = Request["TagLine"];

            //return RedirectToAction(ProfileID);


            if (ModelState.IsValid)
            {
                db.Entry(PersonalProfiles).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.ProfileID = new SelectList(db.Users, "ProfileID", "DisplayName", PersonalProfiles.ProfileID);
            return View(PersonalProfiles);
        }

        // GET: PersonalProfiles/Delete/5
        public ActionResult Delete(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            PersonalProfile PersonalProfiles = db.Profile.Find(id);
            if (PersonalProfiles == null)
            {
                return HttpNotFound();
            }
            return View(PersonalProfiles);
        }

        // POST: PersonalProfiles/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(string id)
        {
            PersonalProfile PersonalProfiles = db.Profile.Find(id);
            db.Profile.Remove(PersonalProfiles);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
