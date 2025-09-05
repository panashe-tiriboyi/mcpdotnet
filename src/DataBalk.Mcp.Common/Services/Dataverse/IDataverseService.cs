namespace DataBalk.Mcp.Common.Services.CRM
{

    public interface IDataverseService
    {

        Task<string> GetAccounts(string query, CancellationToken cancellationToken);
    }

}
