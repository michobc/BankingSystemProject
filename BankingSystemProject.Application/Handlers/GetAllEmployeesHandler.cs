using AutoMapper;
using BankingSystemProject.Application.Commands;
using BankingSystemProject.Application.ViewModels;
using BankingSystemProject.Persistence.Data;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BankingSystemProject.Application.Handlers;

public class GetAllEmployeesHandler: IRequestHandler<GetAllEmployees, List<EmployeeViewModel>>
{
    private readonly BankingSystemContext _context;
    private readonly IMapper _mapper;

    public GetAllEmployeesHandler(BankingSystemContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<List<EmployeeViewModel>> Handle(GetAllEmployees request, CancellationToken cancellationToken)
    {
        var employees = await _context.Users
            .Where(u => u.Role == "Employee")
            .ToListAsync(cancellationToken);

        if (employees == null || !employees.Any())
        {
            throw new Exception("No employees found in this branch");
        }

        // Map entities to view models
        var employeeViewModels = _mapper.Map<List<EmployeeViewModel>>(employees);

        return employeeViewModels;
    }
}