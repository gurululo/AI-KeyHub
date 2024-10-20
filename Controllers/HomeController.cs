using System.Diagnostics;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using AI_KeyHub.Models;
using AI_KeyHub.Models.ViewModels;
using AI_KeyHub.Services;

namespace AI_KeyHub.Controllers;

[Authorize]
public class HomeController : Controller
{
    private readonly IChannelService _channelService;
    private readonly IApiKeyService _apiKeyService;

    public HomeController(IChannelService channelService, IApiKeyService apiKeyService)
    {
        _channelService = channelService;
        _apiKeyService = apiKeyService;
    }

    public IActionResult Index()
    {
        var channels = _channelService.GetAllChannels();
        var channelDict = channels.ToDictionary(c => c.Id, c => c);
        var keys = _apiKeyService.GetAllApiKeys();

        var scheme = Request.Scheme;
        var host = Request.Host.Value;
        var endpoint = $"{scheme}://{host}/v1/chat/completions";

        var vm = new DashboardViewModel
        {
            TotalChannels = channels.Count,
            ActiveChannels = channels.Count(c => c.IsEnabled),
            TotalApiKeys = keys.Count,
            ActiveApiKeys = keys.Count(k => k.IsEnabled),
            ApiEndpointUrl = endpoint,
            RecentChannels = channels.Take(5).Select(c => new ChannelViewModel
            {
                Id = c.Id,
                Name = c.Name,
                BaseUrl = c.BaseUrl,
                Model = c.Model,
                MaskedApiKey = _channelService.MaskApiKey(c.ApiKey),
                IsEnabled = c.IsEnabled,
                CreatedAt = c.CreatedAt
            }).ToList(),
            RecentApiKeys = keys.Take(5).Select(k =>
            {
                var boundChannel = channelDict.TryGetValue(k.ChannelId, out var c) ? c : null;
                return new ApiKeyViewModel
                {
                    Id = k.Id,
                    Name = k.Name,
                    ChannelName = boundChannel?.Name ?? "已失效渠道",
                    BoundModel = boundChannel?.Model ?? "未知",
                    MaskedKey = k.MaskedKey,
                    IsEnabled = k.IsEnabled,
                    CreatedAt = k.CreatedAt
                };
            }).ToList()
        };

        return View(vm);
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
