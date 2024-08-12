using BankingSystemProject.Application.ViewModels;
using MediatR;

namespace BankingSystemProject.Application.Commands;

public class GetEmployee : IRequest<EmployeeViewModel>
{
    public string username { get; set; }
}