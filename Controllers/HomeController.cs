using KGP.Models;
using KGP.Services;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Diagnostics;

namespace KGP.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IOrderService _orderService;

        public HomeController(ILogger<HomeController> logger, IOrderService orderService)
        {
            _logger = logger;
            _orderService = orderService;   
        }

        public IActionResult Index(string site, string workCenter = null)
        {
            try {
                if (site == null)
                {
                    throw new Exception("Please select site");
                }

                if (workCenter == null)
                {
                    throw new Exception("Please select Work Center");
                }

                string createdBy = HttpContext.Request.Headers["X-MS-CLIENT-PRINCIPAL-NAME"];
                if (string.IsNullOrEmpty(createdBy))
                {
                    createdBy = "agent@kellogggarden.com";
                }

                var workcenters = _orderService.GetWorkCenterBySite(site);
                ViewBag.Site = site;
                ViewBag.WorkCenter = workCenter;
                ViewBag.WorkCenters = workcenters;
                ViewBag.SelectedWorkCenter = workCenter;
                var jobs = new List<ProductionLine>();
                if (!string.IsNullOrEmpty(workCenter)) {
                    jobs = _orderService.GetProductionLines(null, site, workCenter, null, createdBy);
                }
                return View(jobs);
            }
            catch (Exception ex)
            {
                var jobs = new List<ProductionLine>();
                ViewBag.WorkCenters = new List<string>();
                ViewBag.ErrorMsg = "*" + ex.Message;
                return View(jobs);
            }
        }

        public async Task<IActionResult> JobDetailAsync(int id, string site, string workCenter = null)
        {
            try {
                var workcenters = _orderService.GetWorkCenterBySite(site);

                var downlogs = _orderService.GetDownTimeLog(id);

                ViewBag.DownReasons = _orderService.GetProductionDownReason();
                ViewBag.BalerDownReasons = _orderService.GetProductionDownReason().Where(x => !x.Reason.ToLower().Equals("break") && !x.Reason.ToLower().Equals("lunch")).ToList();
                ViewBag.CreatedBy = HttpContext.Request.Headers["X-MS-CLIENT-PRINCIPAL-NAME"];
                if (string.IsNullOrEmpty(ViewBag.CreatedBy))
                {
                    ViewBag.CreatedBy = "agent@kellogggarden.com";
                }
                var job = _orderService.GetProductionLines(id).FirstOrDefault();
                job.StoppedBaler = downlogs.Where(x => x.Baler != null && x.EndDownTime == null).ToList();
                
                //if (job.Status.ToLower() != "pending") {
                //    throw new Exception(string.Format("Operator {0} already worked on this job.", job.Operator));
                //}

                ViewBag.WorkCenters = workcenters;
                ViewBag.Site = site;
                ViewBag.SelectedWorkCenter = workCenter;

                ViewBag.Materials = await _orderService.GetProductionItemMaterials(job.RunID, job.ID, site, job.ProductionScheduleID, job.ItemID, job.DueDate.ToString("MM/dd/yyyy"), job.Job);

                return View(job);
            }
            catch(Exception ex) {
                var job = new ProductionLine() { Status = ""};
                ViewBag.ErrorMsg = "*" + ex.Message;
                return View(job);
            }
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}