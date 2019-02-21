﻿using Com.Danliris.Service.Sales.Lib.BusinessLogic.Facades.GarmentBookingOrderFacade;
using Com.Danliris.Service.Sales.Lib.Services;
using Com.Danliris.Service.Sales.WebApi.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Com.Danliris.Service.Sales.WebApi.Controllers
{
    [Produces("application/json")]
    [ApiVersion("1.0")]
    [Route("v{version:apiVersion}/sales/garment-booking-orders-report")]
    [Authorize]
    public class GarmentBookingOrderReportController : Controller
    {
        private string ApiVersion = "1.0.0";
        private readonly GarmentBookingOrderReportFacade _facade;

        public GarmentBookingOrderReportController(GarmentBookingOrderReportFacade facade)
        {
            _facade = facade;
        }

        [HttpGet]
        public IActionResult GetReportAll(string no, string buyerCode, string statusCancel, DateTime? dateFrom, DateTime? dateTo, int page, int size, string Order = "{}")
        {
            int offset = Convert.ToInt32(Request.Headers["x-timezone-offset"]);
            string accept = Request.Headers["Accept"];

            try
            {

                var data = _facade.GetReport(no, buyerCode, statusCancel, dateFrom, dateTo, page, size, Order, offset);

                return Ok(new
                {
                    apiVersion = ApiVersion,
                    data = data.Item1,
                    info = new { total = data.Item2 },
                    message = General.OK_MESSAGE,
                    statusCode = General.OK_STATUS_CODE
                });
            }
            catch (Exception e)
            {
                Dictionary<string, object> Result =
                    new ResultFormatter(ApiVersion, General.INTERNAL_ERROR_STATUS_CODE, e.Message)
                    .Fail();
                return StatusCode(General.INTERNAL_ERROR_STATUS_CODE, Result);
            }
        }

        [HttpGet("download")]
        public IActionResult GetXlsAll(string no, string buyerCode, string statusCancel, DateTime? dateFrom, DateTime? dateTo)
        {

            try
            {
                byte[] xlsInBytes;
                int offset = Convert.ToInt32(Request.Headers["x-timezone-offset"]);
                DateTime DateFrom = dateFrom == null ? new DateTime(1970, 1, 1) : Convert.ToDateTime(dateFrom);
                DateTime DateTo = dateTo == null ? DateTime.Now : Convert.ToDateTime(dateTo);

                var xls = _facade.GenerateExcel(no, buyerCode, statusCancel, dateFrom, dateTo, offset);

                string filename = String.Format("Laporan Canceled Booking Order - {0}.xlsx", DateTime.UtcNow.ToString("ddMMyyyy"));

                xlsInBytes = xls.ToArray();
                var file = File(xlsInBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", filename);
                return file;

            }
            catch (Exception e)
            {
                Dictionary<string, object> Result =
                    new ResultFormatter(ApiVersion, General.INTERNAL_ERROR_STATUS_CODE, e.Message)
                    .Fail();
                return StatusCode(General.INTERNAL_ERROR_STATUS_CODE, Result);
            }
        }
    }
}
