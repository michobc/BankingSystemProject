using BankingSystemProject.Application.ViewModels;
using MediatR;

namespace BankingSystemProject.Application.Commands;

public class GetAllEmployees: IRequest<List<EmployeeViewModel>>
{
    
}