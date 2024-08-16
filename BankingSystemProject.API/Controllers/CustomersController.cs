using BankingSystemProject.Application.Commands;
using BankingSystemProject.Application.ViewModels;
using BankingSystemProject.Domain.Models;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace BankingSystemProject.API.Controllers;

[ApiController]
[Route("api/v{version:apiVersion}/customers")]
public class CustomersController : ControllerBase
{
    private readonly IMediator _mediator;

    public CustomersController(IMediator mediator)
    {
        _mediator = mediator;
    }
    
    [HttpGet]
    public async Task<IActionResult> getCustomers()
    {
        try
        {
            List<CustomerViewModel> employeesViewModel = await _mediator.Send(new GetAllCustomers{});
            return Ok(employeesViewModel);
        }
        catch
        {
            return NotFound();
        }
    }
    
    [HttpGet("{username}")]
    public async Task<IActionResult> getCustomer(string username)
    {
        try
        {
            CustomerViewModel cutomerViewModel = await _mediator.Send(new GetCustomer { username = username });
            return Ok(cutomerViewModel);
        }
        catch
        {
            return NotFound(username);
        }
    }
    
    [HttpPost("create-account")]
    public async Task<IActionResult> CreateAccount([FromForm] CreateAccount command)
    {
        try
        {
            await _mediator.Send(command, CancellationToken.None);
            return Ok("Account created successfully.");
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }
    
    [HttpGet("all-accounts/{username}")]
    public async Task<IActionResult> getAccounts(string username)
    {
        try
        {
            List<AccountViewModel> accountsViewModel = await _mediator.Send(new GetAllAccounts { username = username });
            return Ok(accountsViewModel);
        }
        catch
        {
            return NotFound(username);
        }
    }
}