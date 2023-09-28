using KGP.Models;
using KGP.Services;
using Microsoft.AspNetCore.Mvc;
using System.Data.SqlClient;

namespace KGP.Controllers
{
    public class OpenOrderController : Controller
    {
        private readonly ILogger<OpenOrderController> _logger;
        private readonly IOrderService _orderService;

        public OpenOrderController(ILogger<OpenOrderController> logger, IOrderService orderService)
        {
            _logger = logger;
            _orderService = orderService;
        }

        public IActionResult Index(string warehouse = "all", string status = "all", int page = 1, string startDate = "", string endDate = "")
        {
            List<OpenOrder> list = new List<OpenOrder>();
            try
            {
                if (string.IsNullOrEmpty(status))
                {
                    status = "ordered";
                }
                else
                {
                    if (status == "all")
                        status = "%";
                }

                if (string.IsNullOrEmpty(warehouse))
                {
                    warehouse = "%";
                }
                else
                {
                    if (warehouse == "all")
                        warehouse = "%";
                }

                if (string.IsNullOrEmpty(startDate))
                {
                    startDate = DateTime.Today.ToString("MM/dd/yyyy");
                }

                if (string.IsNullOrEmpty(endDate))
                {
                    endDate = DateTime.Today.ToString("MM/dd/yyyy");
                }

                var filter = new Filter()
                {
                    Warehouse = warehouse,
                    StartDate= startDate,
                    Status = status,
                    EndDate= endDate,
                    Page = page
                };

                list = _orderService.GetOrders(filter);

                ViewBag.Status = status;
                ViewBag.Warehouse = warehouse;
                ViewBag.StartDate = DateTime.Parse(startDate).ToString("yyyy-MM-dd");
                ViewBag.EndDate = DateTime.Parse(endDate).ToString("yyyy-MM-dd");

                ViewBag.TotalRecord = list.Count == 0 ? 0 : list[0].Total;
                ViewBag.Page = page;
                ViewBag.Count = list.Count == 0 ? 0 : list[0].Count;
                ViewBag.ErrorMsg = "";
                return View(list);
            }
            catch (Exception ex) {
                var todate = DateTime.Now;
                ViewBag.ErrorMsg = ex.Message;
                ViewBag.TotalRecord = 0;
                ViewBag.Status = "%";
                ViewBag.Warehouse = "%";
                ViewBag.StartDate = todate.ToString("yyyy-MM-dd");
                ViewBag.EndDate = todate.AddDays(1).ToString("yyyy-MM-dd");
                return View(list);
            }   
        }
    }
}
