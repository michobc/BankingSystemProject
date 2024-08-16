using AutoMapper;
using BankingSystemProject.Application.Commands;
using BankingSystemProject.Application.ViewModels;
using BankingSystemProject.Persistence.Data;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace BankingSystemProject.Application.Handlers;

public class GetEmployeeHandler: IRequestHandler<GetEmployee, EmployeeViewModel>
{
    private readonly BankingSystemContext _context;
    private readonly IMapper _mapper;
    private readonly IMemoryCache _cache;
    private readonly TimeSpan _cacheDuration = TimeSpan.FromMinutes(30);

    public GetEmployeeHandler(BankingSystemContext context, IMapper mapper, IMemoryCache cache)
    {
        _context = context;
        _mapper = mapper;
        _cache = cache;
    }

    public async Task<EmployeeViewModel> Handle(GetEmployee request, CancellationToken cancellationToken)
    {
        var username = request.username;
        //
        // if (!_cache.TryGetValue($"Employee_{username}", out EmployeeViewModel employeeViewModel))
        // {
            var Employee = await _context.Users
                .Where(u => u.Role == "Employee" && u.Username == username)
                .SingleOrDefaultAsync(cancellationToken);
            
            if (Employee == null)
            {
                throw new Exception("No Employee found");
            }
            var employeeView = _mapper.Map<EmployeeViewModel>(Employee);
            // _cache.Set($"Employee_{username}", employeeView, _cacheDuration);
            // Console.WriteLine("added to cache");
            return employeeView;
        // }
        // Console.WriteLine("From cache");
        // return employeeViewModel;
    }
}