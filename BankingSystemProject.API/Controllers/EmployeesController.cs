using BankingSystemProject.Application.Commands;
using BankingSystemProject.Application.ViewModels;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BankingSystemProject.API.Controllers;

[ApiController]
[Route("api/v{version:apiVersion}/employees")]
public class EmployeesController : ControllerBase
{
    private readonly IMediator _mediator;

    public EmployeesController(IMediator mediator)
    {
        _mediator = mediator;
    }
    
    // admin can view Employees on all branches 
    // Employees are restricted only to there branch
    [Authorize(Roles = "admin, Employee")]
    [HttpGet]
    public async Task<IActionResult> getEmployees()
    {
        try
        {
            List<EmployeeViewModel> employeesViewModel = await _mediator.Send(new GetAllEmployees{});
            return Ok(employeesViewModel);
        }
        catch
        {
            return NotFound();
        }
    }
    
    [Authorize(Roles = "admin, Employee")]
    [HttpGet("{username}")]
    public async Task<IActionResult> getEmployee(string username)
    {
        try
        {
            EmployeeViewModel employeeViewModel = await _mediator.Send(new GetEmployee { username = username });
            return Ok(employeeViewModel);
        }
        catch
        {
            return NotFound(username);
        }
    }
}