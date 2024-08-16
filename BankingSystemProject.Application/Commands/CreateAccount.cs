using MediatR;

namespace BankingSystemProject.Application.Commands;

public class CreateAccount : IRequest
{
    public string CustomerUsername { get; set; }
    public decimal InitialBalance { get; set; }
    public string BranchId { get; set; }
}