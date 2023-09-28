using KGP.Models;
using KGP.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualBasic;

namespace KGP.Controllers
{
    public class AdminController : Controller
    {
        private readonly IOrderService _orderService;
        public AdminController(IOrderService orderService)
        {
            _orderService = orderService;
        }

        public async Task<IActionResult> IndexAsync(string site, string filter, string direction, int? showModalId = null, int? showDownModalId = null, int? ShowMaterialModalId = null)
        {
            try {
                if (site == null)
                {
                    throw new Exception("Please select site");
                }

                ViewBag.CreatedBy = HttpContext.Request.Headers["X-MS-CLIENT-PRINCIPAL-NAME"];
                if (string.IsNullOrEmpty(ViewBag.CreatedBy))
                {
                    ViewBag.CreatedBy = "agent@kellogggarden.com";
                }

                ViewBag.Site = site;
                ViewBag.Filter = filter == null ? string.Empty : filter;
                ViewBag.Direction = direction == null ? string.Empty : direction;
                ViewBag.WorkCenters = _orderService.GetWorkCenterBySite(site);
                ViewBag.CSIService = Config.CSIRestService;
                ViewBag.Operators = _orderService.GetOperators(site);
                ViewBag.DownReasons = _orderService.GetProductionDownReason();

                var jobs = _orderService.GetAllSubmittedProductHeaders(site, filter, direction);

                if (showModalId != null)
                {
                    var job = jobs.Find(x => x.ID == showModalId);
                    ViewBag.UM = job.UM;
                    ViewBag.SelectedProduct = job;
                    ViewBag.ProductionLines = job.ProductionLines;
                    ViewBag.ShowMoadl = true;
                    ViewBag.HeaderId = jobs.Find(x => x.ID == showModalId).ID;
                    ViewBag.DueDate = jobs.Find(x => x.ID == showModalId).DueDate;
                    ViewBag.CompletedBag = job.ProductionLines.Sum(x => x.BagCount + x.PalletCount * x.BagsPerPallet);
                    ViewBag.Remainig = job.BagQty - job.ProductionLines.Sum(x => x.Qty) <= 0 ? 0 : job.BagQty - job.ProductionLines.Sum(x => x.Qty);
                    ViewBag.Schedule = job.ProductionScheduleID;
                    ViewBag.Item = job.ItemID;
                }
                else {
                    ViewBag.ShowMoadl = false;
                    ViewBag.SelectedProduct = new ProductionHeader() { 
                        ItemID = "111",
                        DueDate = new DateTime(),
                        ProductionScheduleID = "Default"
                    };
                    ViewBag.Schedule = "";
                    ViewBag.Item = "";
                    ViewBag.UM = "";
                }

                if (showDownModalId != null)
                {
                    ViewBag.IsBaler = jobs.Find(x => x.ID == showModalId).ProductionLines.Where(x => x.ID == showDownModalId).FirstOrDefault().IsBaler;
                    ViewBag.DownLogs = _orderService.GetDownTimeLog((int)showDownModalId);
                    ViewBag.LineId = showDownModalId;
                    ViewBag.DisableLine = jobs.Find(x => x.ID == showModalId).ProductionLines.Where(x => x.ID == showDownModalId).FirstOrDefault().Status.ToLower() == "complete";
                    ViewBag.ShowDownModal = true;
                }
                else {
                    ViewBag.ShowDownModal = false;
                }

                if (ShowMaterialModalId != null)
                {
                    var job = jobs.Find(x => x.ID == showModalId);
                    ViewBag.Materials = await _orderService.GetProductionItemMaterials((int)showModalId, (int)ShowMaterialModalId, site, job.ProductionLines[0].ProductionScheduleID, job.ItemID, job.DueDate.ToString("MM/dd/yyyy"), job.Job);
                    ViewBag.LineId = ShowMaterialModalId;
                    ViewBag.ShowMaterialModal = true;
                    ViewBag.DisableLine = jobs.Find(x => x.ID == showModalId).ProductionLines.Where(x => x.ID == ShowMaterialModalId).FirstOrDefault().Status.ToLower() == "complete";
                }
                else
                {
                    ViewBag.ShowMaterialModal = false;
                }


                return View(jobs);
            }
            catch(Exception ex)
            {
                var jobs = new List<ProductionHeader>();
                ViewBag.ErrorMsg = "*" + ex.Message;
                return View(jobs);
            }
        }

        public IActionResult ProductionSchedule(string id, string site) {
            if (Request.Cookies["site"] == null)
            {
                if (site == null)
                {
                    throw new Exception("Please select site");
                }
                else
                {
                    Response.Cookies.Append("site", site);
                }
            }
            else
            {
                site = Request.Cookies["site"].ToString();
            }

            var schedules = new List<ProductionSchedule>();

            if (id != null) { 
                schedules = _orderService.GetProductionSchedulebyID(id);
                ViewBag.FetchId = id;
            }
            ViewBag.Site = site;
            ViewBag.WorkCenters = _orderService.GetWorkCenterBySite(site);
            return View(schedules);
        }

        public IActionResult Detail(int id, string site) {
            return View();
        }

        public async Task<IActionResult> ScheduleAsync(string site, DateTime dueDate) {
            try
            {
                if (site == null)
                {
                    throw new Exception("Please select site");
                }

                ViewBag.Site = site;
                var schedule = new List<ProductionLine>();
                schedule = _orderService.GetAllProductionLines(null, site, null, null, null);

                for (int i = 0; i < schedule.Count; i++)
                {
                    if (schedule[i].DueDate != dueDate)
                    {
                        schedule.RemoveAt(i);
                        i--;
                    }
                    else
                    {
                        schedule[i].Materials = new List<ProductionMaterial>();
                        schedule[i].Materials = await _orderService.GetAllProductionItemMaterials(site, schedule[i].ProductionScheduleID, schedule[i].ItemID, schedule[i].HeaderDueDate.ToString("MM/dd/yyyy"));

                    }
                }

                return View(schedule);
            }
            catch (Exception ex)
            {
                var schedule = new List<ProductionLine>();
                ViewBag.ErrorMsg = "*" + ex.Message;
                return View(schedule);
            }
        }
    }
}
