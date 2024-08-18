using BankingSystemProject.Application.Commands;
using BankingSystemProject.Application.ViewModels;
using MediatR;
using Microsoft.AspNetCore.Authorization;
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
    
    [Authorize(Roles = "admin, Employee")]
    [HttpGet("{branch}")]
    public async Task<IActionResult> getCustomers(string branch)
    {
        try
        {
            List<CustomerViewModel> employeesViewModel = await _mediator.Send(new GetAllCustomers{ branch = branch });
            return Ok(employeesViewModel);
        }
        catch
        {
            return NotFound();
        }
    }
    
    [Authorize(Roles = "admin, Employee")]
    [HttpGet("{branch}/{username}")]
    public async Task<IActionResult> getCustomer(string username, string branch)
    {
        try
        {
            CustomerViewModel cutomerViewModel = await _mediator.Send(new GetCustomer { username = username, branch = branch });
            return Ok(cutomerViewModel);
        }
        catch
        {
            return NotFound(username);
        }
    }
    
    [Authorize(Roles = "admin, Employee")]
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
    
    [Authorize(Roles = "admin, Employee")]
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
    
    [Authorize(Roles = "Customer")]
    [HttpGet("all-my-accounts")]
    public async Task<IActionResult> getMyAccounts()
    {
        try
        {
            List<AccountViewModel> accountsViewModel = await _mediator.Send(new GetAllMyAccounts {} );
            return Ok(accountsViewModel);
        }
        catch
        {
            return NotFound();
        }
    }
    
    [Authorize(Roles = "admin, Customer")]
    [HttpPost("create-transaction")]
    public async Task<IActionResult> CreateTransaction([FromForm] CreateTransaction command)
    {
        try
        {
            var transactionViewModel = await _mediator.Send(command, CancellationToken.None);
            return Ok(transactionViewModel);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }
    
    [Authorize(Roles = "admin, Employee")]
    [HttpPost("create-recurrent-transaction")]
    public async Task<IActionResult> CreateRecurrentTransaction([FromForm] CreateRecurrentTransaction command)
    {
        try
        {
            var recurrentTransactionViewModel = await _mediator.Send(command, CancellationToken.None);
            return Ok(recurrentTransactionViewModel);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }
    
    [Authorize(Roles = "Customer")]
    [HttpPost("{branch}/transactions/{accountid}")]
    public async Task<IActionResult> ViewTransaction(int accountid, string branch)
    {
        try
        {
            var transactionViewModel = await _mediator.Send(new GetTransactions{ AccountId = accountid, Branch = branch});
            return Ok(transactionViewModel);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }
}