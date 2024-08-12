using BankingSystemProject.Application.Commands;
using BankingSystemProject.Application.ViewModels;
using MediatR;
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
    
    [HttpGet("{username}")]
    public async Task<IActionResult> getEmployee(string username)
    {
        EmployeeViewModel employeeViewModel = await _mediator.Send(new GetEmployee { username = username });
        return Ok(employeeViewModel);
    }
}