using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using Newtonsoft.Json;
using WaterMeter.Config;
using WaterMeter.SimpleLog;
using WaterMeter.Messages;

namespace WaterMeter.API;

public class RefreshTokenHttpMessageHandler : DelegatingHandler
{

    private readonly HttpClient _backChannel = new HttpClient();

    private readonly int _maxRetryCount = 1;
    private string RefreshToken => _config.RefreshToken;
    private string? _accessToken;
    private readonly WaterMetterConfig _config;

    public RefreshTokenHttpMessageHandler(WaterMetterConfig config)
    {
        _config = config;
        InnerHandler = new SocketsHttpHandler();
    }

    [return:NotNull]
    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(_accessToken))
        {
            new SimpleLog.Log(LogLevel.Info, "AccessToken为空，刷新中...").Send();
            await RefreshAccessToken();
        }

        HttpResponseMessage? response = null;
        for (int i = 0; i <= _maxRetryCount; i++)
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _accessToken);
            request.Headers.UserAgent.Clear();
            request.Headers.UserAgent.Add(new ProductInfoHeaderValue("WaterMeter","1.0"));
            response = await base.SendAsync(request, cancellationToken);
            if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                new SimpleLog.Log(LogLevel.Info, "AccessToken过期，刷新中...").Send();
                await RefreshAccessToken();
                continue;
            }
            return response;
        }
        return response!;
    }

    private async Task RefreshAccessToken()
    {
        var content = new FormUrlEncodedContent([
            new KeyValuePair<string, string>("client_id", "9a1fd200-8687-44b1-4c20-08d50a96e5cd"),
            new KeyValuePair<string, string>("client_secret", "8b53f727-08e2-4509-8857-e34bf92b27f2"),
            new KeyValuePair<string, string>("grant_type", "refresh_token"),
            new KeyValuePair<string, string>("refresh_token", RefreshToken)
        ]);
        try
        {
            var response = await _backChannel.PostAsync("https://openid.cc98.org/connect/token", content);
            response.EnsureSuccessStatusCode();
            var json = await response.Content.ReadAsStringAsync();
            var result = JsonConvert.DeserializeObject<RefreshTokenResponse>(json);
            _accessToken = result!.AccessToken;
            new SimpleLog.Log(LogLevel.Info, "刷新AccessToken成功").Send();
        }
        catch (Exception e)
        {
            new SimpleLog.Log(LogLevel.Critical, $"刷新AccessToken失败,异常信息:{e}").Send();
        }

    }

}