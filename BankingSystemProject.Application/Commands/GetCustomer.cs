using BankingSystemProject.Application.ViewModels;
using MediatR;

namespace BankingSystemProject.Application.Commands;

public class GetCustomer: IRequest<CustomerViewModel>
{
    public string username { get; set; }
    public string branch { get; set; }
}