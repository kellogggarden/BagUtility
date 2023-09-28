using KGP.Models;
using KGP.Services;
using Microsoft.AspNetCore.Cors.Infrastructure;
using System.Reflection.PortableExecutable;

namespace KGP
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddHttpContextAccessor();
            services.AddControllersWithViews();
            services.AddSingleton<IOrderService, OrderService>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            var MyConfig = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build();

            Config.AzureDBConnectionString = MyConfig.GetValue<string>("AzureDBConnectionString");
            Config.CSIReportDBConnectionString = MyConfig.GetValue<string>("CSIReportDBConnectionString");
            Config.CSIRestService = MyConfig.GetValue<string>("CSIRestService");

            //if (env.IsDevelopment())
            //{
            //    app.UseDeveloperExceptionPage();
            //}
            //else
            //{
            //    app.UseExceptionHandler("/Home/Error");
            //    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
            //    app.UseHsts();
            //}
            app.UseDeveloperExceptionPage();
            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");

                endpoints.MapMethods("/api/order", new[] { "POST", "PUT" }, (Filter filter, IOrderService service) =>
                {
                    try
                    {
                        Results.Ok(service.GetOrders(filter));
                        return Results.Ok();
                    }
                    catch (Exception ex)
                    {
                        return Results.Problem(ex.Message);
                    }
                });

                endpoints.MapMethods("/api/order/save", new[] { "POST", "PUT" }, (List<OpenOrder> orders, IOrderService service) =>
                {
                    try
                    {
                        service.Save(orders);
                        return Results.Ok();
                    }
                    catch (Exception ex)
                    {
                        return Results.Problem(ex.Message);
                    }
                });

                endpoints.MapMethods("/api/bagjob/add", new[] { "POST", "PUT" }, (ProductionLine bag, IOrderService service) =>
                {
                    try
                    {
                        service.InsertProductionLine(bag);
                        return Results.Ok();
                    }
                    catch (Exception ex)
                    {
                        return Results.Problem(ex.Message);
                    }
                });

                endpoints.MapMethods("/api/bagjob/save", new[] { "POST", "PUT" }, (ProductionLine bag, IOrderService service) =>
                {
                    try
                    {
                        service.SaveProductionLine(bag);
                        return Results.Ok();
                    }
                    catch (Exception ex)
                    {
                        return Results.Problem(ex.Message);
                    }
                });

                endpoints.MapMethods("/api/bagjob/saveBulk", new[] { "POST", "PUT" }, (ProductionHeader header, IOrderService service) =>
                {
                    try
                    {
                        service.SaveProductionLines(header);
                        return Results.Ok();
                    }
                    catch (Exception ex)
                    {
                        return Results.Problem(ex.Message);
                    }
                });

                endpoints.MapMethods("/api/bagjob/validateJob", new[] { "POST", "PUT" }, (ProductionHeader header, IOrderService service) =>
                {
                    try
                    {
                        var validation = service.ValidateJob(header);
                        return Results.Ok(validation);
                    }
                    catch (Exception ex)
                    {
                        return Results.Problem(ex.Message);
                    }
                });

                endpoints.MapMethods("/api/bagjob/assign", new[] { "POST", "PUT" }, (List<ScheduleAssign> schedules, IOrderService service) =>
                {
                    try
                    {
                        service.AssignProductionSchedules(schedules);
                        return Results.Ok();
                    }
                    catch (Exception ex)
                    {
                        return Results.Problem(ex.Message);
                    }
                });

                endpoints.MapMethods("/api/admin/insertLine", new[] { "POST", "PUT" }, (AddProductLineParam param, IOrderService service) =>
                {
                    try
                    {
                        int id = service.AssingLineToProductionHeader(param);
                        return Results.Ok(id);
                    }
                    catch (Exception ex)
                    {
                        return Results.Problem(ex.Message);
                    }
                });

                endpoints.MapMethods("/api/admin/deletLine", new[] { "POST", "PUT" }, (int lineId, IOrderService service) =>
                {
                    try
                    {
                        service.DeleteProductionLine(lineId);
                        return Results.Ok();
                    }
                    catch (Exception ex)
                    {
                        return Results.Problem(ex.Message);
                    }
                });

                endpoints.MapMethods("/api/admin/deleteHeader", new[] { "POST", "PUT" }, (int id, IOrderService service) =>
                {
                    try
                    {
                        service.DeleteProductionHeader(id);
                        return Results.Ok();
                    }
                    catch (Exception ex)
                    {
                        return Results.Problem(ex.Message);
                    }
                });

                endpoints.MapMethods("/api/admin/saveDownLogs", new[] { "POST", "PUT" }, (List<ProductionDownTimeLog> logs, IOrderService service) =>
                {
                    try
                    {
                        service.SaveDownLogs(logs);
                        return Results.Ok();
                    }
                    catch (Exception ex)
                    {
                        return Results.Problem(ex.Message);
                    }
                });

                endpoints.MapMethods("/api/admin/deleteDownLog", new[] { "POST", "PUT" }, (int id, IOrderService service) =>
                {
                    try
                    {
                        service.DeleteDownLog(id);
                        return Results.Ok();
                    }
                    catch (Exception ex)
                    {
                        return Results.Problem(ex.Message);
                    }
                });

                endpoints.MapMethods("/api/admin/insertDownLog", new[] { "POST", "PUT" }, (ProductionDownTimeLog log, IOrderService service) =>
                {
                    try
                    {
                        service.InsertDownLog(log);
                        return Results.Ok();
                    }
                    catch (Exception ex)
                    {
                        return Results.Problem(ex.Message);
                    }
                });

                endpoints.MapMethods("/api/admin/getDownTimes", new[] { "GET" }, (int lineId, IOrderService service) =>
                {
                    try
                    {
                        var result = service.GetDownTimeLog(lineId);
                        return Results.Ok(result);
                    }
                    catch (Exception ex)
                    {
                        return Results.Problem(ex.Message);
                    }
                });

                endpoints.MapMethods("/api/csi/loadPendingProdSchedule", new[] { "POST", "PUT" }, async (LoadScheduleParam sParam, IOrderService service) =>
                {
                    try
                    {
                        await service.LoadProuctScheduelToSql(sParam);
                        return Results.Ok();
                    }
                    catch (Exception ex) {
                        return Results.Problem(ex.Message);
                    }
                });

                endpoints.MapMethods("/api/bagjob/insertBalerDownLog", new[] { "POST", "PUT" }, (ProductionLine line, IOrderService service) =>
                {
                    try
                    {
                        service.InsertOrUpdateBalerDownLog(line);
                        return Results.Ok();
                    }
                    catch (Exception ex)
                    {
                        return Results.Problem(ex.Message);
                    }
                });

                endpoints.MapMethods("/api/bagjob/insertMaterial", new[] { "POST", "PUT" }, (List<ProductionMaterial> materials, IOrderService service) =>
                {
                    try
                    {
                        service.InsertProductionMaterial(materials);
                        return Results.Ok();
                    }
                    catch (Exception ex)
                    {
                        return Results.Problem(ex.Message);
                    }
                });

                endpoints.MapMethods("/api/admin/postLine", new[] { "POST", "PUT" }, (ProductionHeader header, IOrderService service) =>
                {
                    try
                    {
                        service.PostLine(header);
                        return Results.Ok();
                    }
                    catch (Exception ex)
                    {
                        return Results.Problem(ex.Message);
                    }
                });

                endpoints.MapMethods("/api/admin/postHeader", new[] { "POST", "PUT" }, (ProductionHeader header, IOrderService service) =>
                {
                    try
                    {
                        service.PostHeader(header);
                        return Results.Ok();
                    }
                    catch (Exception ex)
                    {
                        return Results.Problem(ex.Message);
                    }
                });
            });
        }
    }
}
