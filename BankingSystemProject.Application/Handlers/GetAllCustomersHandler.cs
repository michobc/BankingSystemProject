using AutoMapper;
using BankingSystemProject.Application.Commands;
using BankingSystemProject.Application.Services.Abstractions;
using BankingSystemProject.Application.ViewModels;
using BankingSystemProject.Persistence.Data;
using BankingSystemProject.Persistence.Services.Abstractions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BankingSystemProject.Application.Handlers;

public class GetAllCustomersHandler: IRequestHandler<GetAllCustomers, List<CustomerViewModel>>
{
    private readonly IGetAllCustomersService _getAllCustomersService;

    public GetAllCustomersHandler(IGetAllCustomersService getAllCustomersService)
    {
        _getAllCustomersService = getAllCustomersService;
    }

    public async Task<List<CustomerViewModel>> Handle(GetAllCustomers request, CancellationToken cancellationToken)
    {
        var customers = await _getAllCustomersService.GetAllCustomers(request.branch);
        return customers;
    }
}