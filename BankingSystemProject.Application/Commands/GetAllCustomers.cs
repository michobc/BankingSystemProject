using BankingSystemProject.Application.ViewModels;
using MediatR;

namespace BankingSystemProject.Application.Commands;

public class GetAllCustomers: IRequest<List<CustomerViewModel>>
{
    
}