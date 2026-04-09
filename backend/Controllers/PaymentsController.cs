using Microsoft.AspNetCore.Mvc;
using RestaurantManagement.Api.Common.Enums;
using RestaurantManagement.Api.DTOs.Common;
using RestaurantManagement.Api.DTOs.Payments;
using RestaurantManagement.Api.Helpers;
using RestaurantManagement.Api.Interfaces.Services;

namespace RestaurantManagement.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[AppAuthorize(Roles = $"{AppRoles.Admin},{AppRoles.Manager},{AppRoles.Cashier}")]
public class PaymentsController(IPaymentService paymentService) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetPayments([FromQuery] QueryParameters query)
    {
        return Ok(await paymentService.GetPaymentsAsync(query));
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var payment = await paymentService.GetByIdAsync(id);
        return payment is null ? NotFound() : Ok(payment);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreatePaymentDto request)
    {
        var payment = await paymentService.CreateAsync(request);
        return CreatedAtAction(nameof(GetById), new { id = payment.Id }, payment);
    }
}
