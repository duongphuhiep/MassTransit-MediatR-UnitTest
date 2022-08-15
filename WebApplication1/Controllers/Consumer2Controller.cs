using MassTransit;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System.Transactions;

namespace WebApplication1.Controllers;

[ApiController]
[Route("[controller]")]
public class Consumer2Controller : ControllerBase
{
    private readonly IRequestClient<Input2> _requestClient;
    private readonly IConfiguration _configuration;
    private readonly IScopedSample21 _sampleScoped21;
    private readonly IScopedSample22 _sampleScoped22;

    public Consumer2Controller(IRequestClient<Input2> requestClient, IConfiguration configuration, IScopedSample21 sampleScoped21, IScopedSample22 sampleScoped22)
    {
        _requestClient = requestClient;
        _configuration = configuration;
        _sampleScoped21 = sampleScoped21;
        _sampleScoped22 = sampleScoped22;
    }

    [HttpGet(Name = "Consumer2")]
    public async Task<Output2> Get()
    {
        var connectionString = _configuration.GetValue<string>("App:ConnectionString");

        //https://docs.microsoft.com/en-us/ef/core/saving/transactions#using-systemtransactions
        using (var tx = new CommittableTransaction(new TransactionOptions { IsolationLevel = IsolationLevel.ReadCommitted }))
        {
            using (var dbConnection = new SqlConnection(connectionString))
            {
                await dbConnection.OpenAsync();
                HttpContext.Items["CurrentDbConnection"] = dbConnection;
                dbConnection.EnlistTransaction(tx);

                var response = await _requestClient.GetResponse<Output2>(new Input2 { Info = $"Input2 {_sampleScoped21.GetId()}/{_sampleScoped22.GetId()}" });
                //tx.Commit();
                return response.Message;
            }
        }
    }
}