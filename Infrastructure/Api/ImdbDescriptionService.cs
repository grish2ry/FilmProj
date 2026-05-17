using System;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Infrostruture.Loggers;
namespace Infrostruture.Api;

public class ImdbDescriptionService : IDescriptionService
{
    private string apiKey_;
    private readonly HttpClient client_;
    private readonly int retries_;
    private readonly int delay_;

    private readonly IAppLogger logger;

    public ImdbDescriptionService(HttpClient client, IAppLogger logger, string apiKey, int retries = 3, int delay = 100)
    {
        client_ = client;
        apiKey_ = apiKey;
        retries_ = retries;
        delay_ = delay;
        this.logger = logger;
    }

    public async Task<string?> GetDescription(string imdbId, CancellationToken ct = default)
    {
        OMDbResponse? r = new OMDbResponse();
        for(int i = 0; i < retries_; i++){
            try{
                var url = $"http://www.omdbapi.com/?apikey={apiKey_}&i={imdbId}&plot=full";
                string json = await client_.GetStringAsync(url, ct);
                r = JsonSerializer.Deserialize<OMDbResponse>(json);
            }
            catch(Exception ex)
            {
                logger.LogErrorMessage(ex.Message);
            }
            if(r != null && !string.IsNullOrEmpty(r?.Plot))
            {
                return r?.Plot;
            }
            await Task.Delay(delay_);
        }
        return null;

    }
}
