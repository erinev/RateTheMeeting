using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using RateTheMeeting.Models;

namespace RateTheMeeting.Controllers
{
    public class RateTheMeetingController : Controller
    {
        private RateTheMeetingEntities db = new RateTheMeetingEntities();

        //
        // GET: /RateTheMeeting/

        public ActionResult Index()
        {
            return View(db.Meetings.ToList());
        }
    }
}