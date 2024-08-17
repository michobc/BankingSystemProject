namespace BankingSystemProject.Common.Services;

public class CalculatNextTransactionDate
{
    public DateTime CalculateNextTransactionDate(string frequency, DateTime date)
    {
        return frequency.ToLower() switch
        {
            "daily" => date.AddDays(1),
            "weekly" => date.AddDays(7),
            "monthly" => date.AddMonths(1),
            _ => throw new Exception("Invalid frequency.")
        };
    }
}