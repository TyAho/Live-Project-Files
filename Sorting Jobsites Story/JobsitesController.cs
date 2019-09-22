using ManagementPortal.Common;
using ManagementPortal.Models;
using ManagementPortal.Services;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using PagedList;
namespace ManagementPortal.Controllers
{
    public class JobSitesController : ApplicationBaseController
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: Jobsites
        public ViewResult Index(string sortOrder, string currentFilter, string searchString, int? page)
        {
            ViewBag.CurrentSort = sortOrder;
            ViewBag.SiteSortParm = string.IsNullOrEmpty(sortOrder) ? "name_desc" : "";
            ViewBag.AddressSortParm = sortOrder == "Date" ? "address_desc" : "Date";
            ViewBag.TownSortParm = string.IsNullOrEmpty(sortOrder) ? "town_desc" : "";
            ViewBag.StateSortParm = string.IsNullOrEmpty(sortOrder) ? "state_desc" : "";
            ViewBag.ZipSortParm = string.IsNullOrEmpty(sortOrder) ? "zip_desc" : "";
            
            if (searchString != null)
            {
                page = 1;
            }
            else
            {
                searchString = currentFilter;

            }

            ViewBag.CurrentFilter = searchString;

            var JobSites = from s in db.JobSites
                           select s;

            if (!String.IsNullOrEmpty(searchString))
            {
                JobSites = JobSites.Where(s => s.SiteName.Contains(searchString)
                                        || s.Address.Contains(searchString));
            }

            switch (sortOrder)
            {
                case "name_desc":
                    JobSites = JobSites.OrderByDescending(s => s.SiteName);
                    break;
                case "address_desc":
                    JobSites = JobSites.OrderByDescending(s => s.Address);
                    break;
                case "town_desc":
                    JobSites = JobSites.OrderByDescending(s => s.Town);
                    break;

                case "state_desc":
                    JobSites = JobSites.OrderByDescending(s => s.State);
                    break;

                case "zip_desc":
                    JobSites = JobSites.OrderByDescending(s => s.Zip);
                    break;

                default:
                    JobSites = JobSites.OrderBy(s => s.SiteName);
                    break;

            }
            int pageSize = 10;
            int pageNumber = (page ?? 1);
            return View(JobSites.ToPagedList(pageNumber, pageSize));
        }

        // GET: Jobsites/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var jobSite = db.JobSites.Find(id);

            if (jobSite == null)
            {
                return HttpNotFound();
            }

            // Set a default false flag, then evaluate if a map should be displayed
            ViewBag.HasLatLong = false;

            // create geocode object to use it's validation logic
            var geocode = new DecimalGeocode() { DecimalLatitude = jobSite.Lat, DecimalLongitude = jobSite.Long };

            if (geocode.IsValid())
            {
                //if valid, setup the view to display the map
                ViewBag.HasLatLong = true;
                ViewBag.Lat = geocode.DecimalLatitude;
                ViewBag.Long = geocode.DecimalLongitude;
            }

            return View(jobSite);
        }

        // GET: Jobsites/Create
        public ActionResult Create()
        {
            var newJobSite = new JobSite();
            return View(newJobSite);
        }

        // POST: Jobsites/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "SiteName,JobSiteID,Address,Town,State,Zip,Lat,Long")] JobSite jobSite)
        {
            if (ModelState.IsValid)
            {
                var validateZipCode = FormAPIZipCodeService.ValidateZipCode(jobSite.Zip, jobSite.State.GetDisplayName(), jobSite.Town);

                if (validateZipCode.Successful)
                {
                    var geoCode = OpenGeocodingService.GetGeocode(jobSite.Address, jobSite.Town, jobSite.State.GetDisplayName(), jobSite.Zip);

                    jobSite.Lat = geoCode.Return.Geocode.DecimalLatitude;
                    jobSite.Long = geoCode.Return.Geocode.DecimalLongitude;

                    db.JobSites.Add(jobSite);
                    db.SaveChanges();
                    return RedirectToAction("Index");
                }
                else
                {
                    ModelState.AddModelError("Zip", validateZipCode.Return.ErrorMessage);
                }
            }
            
            return View(jobSite);
        }

        /// <summary>
        /// Ajax endpoint for the Create new Job Site on the Create Jobs form.
        /// </summary>
        /// <param name="jobSite"></param>
        /// <returns></returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CreateAjax([Bind(Include = "SiteName,JobSiteID,Address,Town,State,Zip,Lat,Long")] JobSite jobSite)
        {
            if (ModelState.IsValid)
            {
                var validateZipCode = FormAPIZipCodeService.ValidateZipCode(jobSite.Zip, jobSite.State.GetDisplayName(), jobSite.Town);

                // the following logic is setup to bypass the 509 error code from the FormAPIZipCodeService.
                // It may seem counter intutative, but it first checks if it was unsucessful, and if so, if it was a 509
                // error. if it's a 509, then it adds to the database anyway, if not, it reports the error to the form,
                // and it if it was successful, then it jumps to the add database label using the goto/label.

                if (!validateZipCode.Successful)
                {
                    if (validateZipCode.Return.ErrorMessage.Contains("509"))
                    {
                        //509 error is bandwidth limit exceeded or rather, that the api server itself is busy.
                        var geoCode = OpenGeocodingService.GetGeocode(jobSite.Address, jobSite.Town, jobSite.State.GetDisplayName(), jobSite.Zip);

                        jobSite.Lat = geoCode.Return.Geocode.DecimalLatitude;
                        jobSite.Long = geoCode.Return.Geocode.DecimalLongitude;

                        db.JobSites.Add(jobSite);
                        db.SaveChanges();
                        return Json(new { success = "true", jobSite_Text = jobSite.SiteName, jobSite_Value = jobSite.JobSiteID });
                    }
                    else
                    {
                        // invalid zip code
                        // add error to zip field
                        ModelState.AddModelError("Zip", validateZipCode.Return.ErrorMessage);

                        // return json with false success, and pass in the field name and errors
                        return Json(new { success = "false", errors = ExtractErrors(ViewData.ModelState) });
                    }
                }
                else
                {
                    var geoCode = OpenGeocodingService.GetGeocode(jobSite.Address, jobSite.Town, jobSite.State.GetDisplayName(), jobSite.Zip);

                    jobSite.Lat = geoCode.Return.Geocode.DecimalLatitude;
                    jobSite.Long = geoCode.Return.Geocode.DecimalLongitude;

                    db.JobSites.Add(jobSite);
                    db.SaveChanges();
                    return Json(new { success = "true", jobSite_Text = jobSite.SiteName, jobSite_Value = jobSite.JobSiteID });
                }
            }
            else
            {
                // invalid form
                // return json with false success, and pass in the field name and errors
                return Json(new { success = "false", errors = ExtractErrors(ViewData.ModelState) });
            }
        }

        /// <summary>
        /// Helper function to iterate over the modelState dictionary and extract the errors
        /// </summary>
        /// <param name="modelState">ViewData.ModelState</param>
        /// <returns>an array of anonymous type, with fieldName and errorMessage </returns>
        private object[] ExtractErrors(ModelStateDictionary modelState)
        {
            // list of errors to return, of type object to accept the anonymous type to pass in.
            List<object> errors = new List<object>();

            foreach (KeyValuePair<string, ModelState> kvp in modelState)
            {
                // check if there are errors for the current key
                if (kvp.Value.Errors.Count > 0)
                {
                    // add each error with the key to the return list
                    foreach (var err in kvp.Value.Errors)
                    {
                        // add a new anonymous type item with fieldName and errorMessage keys
                        errors.Add(new { fieldName = kvp.Key, errorMessage = err.ErrorMessage });
                    }
                }
            }

            // return any errors
            return errors.ToArray();
        }

        // GET: Jobsites/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var jobSite = db.JobSites.Find(id);

            if (jobSite != null)
            {
                return View(jobSite);
            }

            else
            {
                return HttpNotFound();
            }
        }

        // POST: Jobsites/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "SiteName,JobSiteID,Address,Town,State,Zip,Lat,Long")] JobSite jobSite)
        {
            if (ModelState.IsValid)
            {
                db.Entry(jobSite).State = EntityState.Modified;

                //Query the API for new coordinates upon changes to any properties that actually change the address.
                var entry = db.Entry(jobSite);
                var changed = entry.CurrentValues.PropertyNames.Where(x => entry.Property(x).IsModified);
                if (changed.Contains("Address") || changed.Contains("Town") || changed.Contains("State") || changed.Contains("Zip"))
                {
                    var validateZipCode = FormAPIZipCodeService.ValidateZipCode(jobSite.Zip, jobSite.State.GetDisplayName(), jobSite.Town);

                    if (validateZipCode.Successful)
                    {
                        var geoCode = OpenGeocodingService.GetGeocode(jobSite.Address, jobSite.Town, jobSite.State.GetDisplayName(), jobSite.Zip);

                        jobSite.Lat = geoCode.Return.Geocode.DecimalLatitude;
                        jobSite.Long = geoCode.Return.Geocode.DecimalLongitude;

                        db.SaveChanges();
                        return RedirectToAction("Index");
                    }
                    else
                    {
                        ModelState.AddModelError("Zip", validateZipCode.Return.ErrorMessage);
                    }   
                }
            }
            return View(jobSite);
        }

        // GET: Jobsites/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var jobSite = db.JobSites.Find(id);

            if (jobSite == null)
            {
                return HttpNotFound();
            }

            return View(jobSite);
        }

        // POST: Jobsites/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            var jobSite = db.JobSites.Find(id);

            db.JobSites.Remove(jobSite);
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
