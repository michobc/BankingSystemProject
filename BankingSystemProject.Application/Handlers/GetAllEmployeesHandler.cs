using AutoMapper;
using BankingSystemProject.Application.Commands;
using BankingSystemProject.Application.Services.Abstractions;
using BankingSystemProject.Application.ViewModels;
using BankingSystemProject.Persistence.Data;
using BankingSystemProject.Persistence.Services.Abstractions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BankingSystemProject.Application.Handlers;

public class GetAllEmployeesHandler: IRequestHandler<GetAllEmployees, List<EmployeeViewModel>>
{
    private readonly BankingSystemContext _context;
    private readonly IMapper _mapper;
    private readonly ITenantService _tenantService;
    private readonly IGetAllEmployeesService _getAllEmployeesService;

    public GetAllEmployeesHandler(BankingSystemContext context, IMapper mapper, ITenantService tenantService, IGetAllEmployeesService getAllEmployeesService)
    {
        _context = context;
        _mapper = mapper;
        _tenantService = tenantService;
        _getAllEmployeesService = getAllEmployeesService;
    }

    public async Task<List<EmployeeViewModel>> Handle(GetAllEmployees request, CancellationToken cancellationToken)
    {
        if (_tenantService.getRole() == "admin")
        {
            var employees = await _getAllEmployeesService.GetAllEmployees();
            return employees;
        }
        else
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
}