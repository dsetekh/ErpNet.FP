﻿using ErpNet.FP.Core;
using ErpNet.FP.Core.Service;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ErpNet.FP.Server.Controllers
{
    // PrintersController, example: //host/printers/[controller]
    [Route("[controller]")]
    [ApiController]
    public class PrintersController : ControllerBase
    {
        private readonly IServiceController context;

        public PrintersController(IServiceController context)
        {
            this.context = context;
        }

        // GET /
        [HttpGet()]
        public ActionResult<Dictionary<string, DeviceInfo>> Printers()
        {
            if (!context.IsReady)
            {
                return StatusCode(StatusCodes.Status405MethodNotAllowed);
            }
            return context.PrintersInfo;
        }

        // GET {id}
        [HttpGet("{id}")]
        public ActionResult<DeviceInfo> Info(string id)
        {
            if (!context.IsReady)
            {
                return StatusCode(StatusCodes.Status405MethodNotAllowed);
            }
            if (context.PrintersInfo.TryGetValue(id, out DeviceInfo deviceInfo))
            {
                return deviceInfo;
            }
            return NotFound();
        }

        // GET {id}/status
        [HttpGet("{id}/status")]
        public ActionResult<DeviceStatusWithDateTime> Status(string id)
        {
            if (!context.IsReady)
            {
                return StatusCode(StatusCodes.Status405MethodNotAllowed);
            }
            if (context.Printers.TryGetValue(id, out IFiscalPrinter printer))
            {
                return printer.CheckStatus();
            }
            return NotFound();
        }

        // GET {id}/cash
        [HttpGet("{id}/cash")]
        public async Task<IActionResult> Cash(
            string id,
            [FromQuery] int asyncTimeout = PrintJob.DefaultTimeout)
        {
            if (!context.IsReady)
            {
                return StatusCode(StatusCodes.Status405MethodNotAllowed);
            }
            if (context.Printers.TryGetValue(id, out IFiscalPrinter printer))
            {
                var result = await context.RunAsync(
                    printer,
                    PrintJobAction.Cash,
                    null,
                    asyncTimeout);
                return Ok(result);
            }
            return NotFound();
        }

        // GET taskinfo
        [HttpGet("taskinfo")]
        public ActionResult<TaskInfoResult> TaskInfo([FromQuery]string id)
        {
            return context.GetTaskInfo(id);
        }

        // POST {id}/receipt
        [HttpPost("{id}/receipt")]
        public async Task<IActionResult> PrintReceipt(
            string id,
            [FromBody] Receipt receipt,
            [FromQuery] int asyncTimeout = PrintJob.DefaultTimeout)
        {
            if (!context.IsReady)
            {
                return StatusCode(StatusCodes.Status405MethodNotAllowed);
            }
            if (context.Printers.TryGetValue(id, out IFiscalPrinter printer))
            {
                var result = await context.RunAsync(
                    printer,
                    PrintJobAction.Receipt,
                    receipt,
                    asyncTimeout);
                return Ok(result);
            }
            return NotFound();
        }

        // POST {id}/reversalreceipt
        [HttpPost("{id}/reversalreceipt")]
        public async Task<IActionResult> PrintReversalReceipt(
            string id,
            [FromBody] ReversalReceipt reversalReceipt,
            [FromQuery] int asyncTimeout = PrintJob.DefaultTimeout)
        {
            if (!context.IsReady)
            {
                return StatusCode(StatusCodes.Status405MethodNotAllowed);
            }
            if (context.Printers.TryGetValue(id, out IFiscalPrinter printer))
            {
                var result = await context.RunAsync(
                    printer,
                    PrintJobAction.ReversalReceipt,
                    reversalReceipt,
                    asyncTimeout);
                return Ok(result);
            }
            return NotFound();
        }

        // POST {id}/withdraw
        [HttpPost("{id}/withdraw")]
        public async Task<IActionResult> PrintWithdraw(
            string id,
            [FromBody] TransferAmount withdraw,
            [FromQuery] int asyncTimeout = PrintJob.DefaultTimeout)
        {
            if (!context.IsReady)
            {
                return StatusCode(StatusCodes.Status405MethodNotAllowed);
            }
            if (context.Printers.TryGetValue(id, out IFiscalPrinter printer))
            {
                var result = await context.RunAsync(
                    printer,
                    PrintJobAction.Withdraw,
                    withdraw,
                    asyncTimeout);
                return Ok(result);
            }
            return NotFound();
        }

        // POST {id}/deposit
        [HttpPost("{id}/deposit")]
        public async Task<IActionResult> PrintDeposit(
            string id,
            [FromBody] TransferAmount deposit,
            [FromQuery] int asyncTimeout = PrintJob.DefaultTimeout)
        {
            if (!context.IsReady)
            {
                return StatusCode(StatusCodes.Status405MethodNotAllowed);
            }
            if (context.Printers.TryGetValue(id, out IFiscalPrinter printer))
            {
                var result = await context.RunAsync(
                    printer,
                    PrintJobAction.Deposit,
                    deposit,
                    asyncTimeout);
                return Ok(result);
            }
            return NotFound();
        }

        // POST {id}/datetime
        [HttpPost("{id}/datetime")]
        public async Task<IActionResult> SetDateTime(
            string id,
            [FromBody] CurrentDateTime datetime,
            [FromQuery] int asyncTimeout = PrintJob.DefaultTimeout)
        {
            if (!context.IsReady)
            {
                return StatusCode(StatusCodes.Status405MethodNotAllowed);
            }
            if (context.Printers.TryGetValue(id, out IFiscalPrinter printer))
            {
                var result = await context.RunAsync(
                    printer,
                    PrintJobAction.SetDateTime,
                    datetime,
                    asyncTimeout);
                return Ok(result);
            }
            return NotFound();
        }

        // POST {id}/zreport
        [HttpPost("{id}/zreport")]
        public async Task<IActionResult> PrintZReport(
            string id,
            [FromQuery] int asyncTimeout = PrintJob.DefaultTimeout)
        {
            if (!context.IsReady)
            {
                return StatusCode(StatusCodes.Status405MethodNotAllowed);
            }
            if (context.Printers.TryGetValue(id, out IFiscalPrinter printer))
            {
                var result = await context.RunAsync(
                    printer,
                    PrintJobAction.ZReport,
                    null,
                    asyncTimeout);
                return Ok(result);
            }
            return NotFound();
        }

        // POST {id}/xreport
        [HttpPost("{id}/xreport")]
        public async Task<IActionResult> PrintXReport(
            string id,
            [FromQuery] int asyncTimeout = PrintJob.DefaultTimeout)
        {
            if (!context.IsReady)
            {
                return StatusCode(StatusCodes.Status405MethodNotAllowed);
            }
            if (context.Printers.TryGetValue(id, out IFiscalPrinter printer))
            {
                var result = await context.RunAsync(
                    printer,
                    PrintJobAction.XReport,
                    null,
                    asyncTimeout);
                return Ok(result);
            }
            return NotFound();
        }
    }
}
