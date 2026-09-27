namespace TechServe.Web.Services;

public sealed class SalesService
{
    public Task<decimal> GetTotalRevenueAsync()
    {
        return Task.FromResult(24680m);
    }

    public Task<int> GetSalesCountAsync()
    {
        return Task.FromResult(126);
    }
}
