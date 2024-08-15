using AutoMapper;
using BankingSystemProject.Application.Commands;
using BankingSystemProject.Application.ViewModels;
using BankingSystemProject.Persistence.Data;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace BankingSystemProject.Application.Handlers;

public class GetCustomerHandler : IRequestHandler<GetCustomer, CustomerViewModel>
{
    private readonly BankingSystemContext _context;
    private readonly IMapper _mapper;
    private readonly IMemoryCache _cache;
    private readonly TimeSpan _cacheDuration = TimeSpan.FromMinutes(30);

    public GetCustomerHandler(BankingSystemContext context, IMapper mapper, IMemoryCache cache)
    {
        _context = context;
        _mapper = mapper;
        _cache = cache;
    }

    public async Task<CustomerViewModel> Handle(GetCustomer request, CancellationToken cancellationToken)
    {
        var username = request.username;
        
        if (!_cache.TryGetValue($"Customer_{username}", out CustomerViewModel cutomerViewModel))
        {
            var customer = await _context.Users
                .Where(u => u.Role == "Customer" && u.Username == username)
                .SingleOrDefaultAsync(cancellationToken);
            
            if (customer == null)
            {
                throw new Exception("No customer with this username found");
            }
            var customerView = _mapper.Map<CustomerViewModel>(customer);
            _cache.Set($"Customer_{username}", customerView, _cacheDuration);
            Console.WriteLine("added to cache");
            return customerView;
        }
        Console.WriteLine("From cache");
        return cutomerViewModel;
    }
}