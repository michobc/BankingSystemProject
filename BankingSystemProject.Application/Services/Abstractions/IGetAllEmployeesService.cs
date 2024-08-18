using BankingSystemProject.Application.ViewModels;

namespace BankingSystemProject.Application.Services.Abstractions;

public interface IGetAllEmployeesService
{
    public Task<List<EmployeeViewModel>> GetAllEmployees();
    public Task<EmployeeViewModel> GetMyEmployee(string username);
}