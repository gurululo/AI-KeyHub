using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using AI_KeyHub.Models.ViewModels;
using AI_KeyHub.Services;

namespace AI_KeyHub.Controllers;

[Authorize]
public class ApiKeyController : Controller
{
    private readonly IApiKeyService _apiKeyService;
    private readonly IChannelService _channelService;

    public ApiKeyController(IApiKeyService apiKeyService, IChannelService channelService)
    {
        _apiKeyService = apiKeyService;
        _channelService = channelService;
    }

    [HttpGet]
    public IActionResult Index()
    {
        var channels = _channelService.GetAllChannels();
        var channelDict = channels.ToDictionary(c => c.Id, c => c);

        var keys = _apiKeyService.GetAllApiKeys();
        var viewModels = keys.Select(k =>
        {
            var channel = channelDict.TryGetValue(k.ChannelId, out var c) ? c : null;
            return new ApiKeyViewModel
            {
                Id = k.Id,
                Name = k.Name,
                Remarks = k.Remarks,
                ChannelId = k.ChannelId,
                ChannelName = channel?.Name ?? "已失效/已删除渠道",
                BoundModel = channel?.Model ?? "未知",
                ChannelEnabled = channel?.IsEnabled ?? false,
                MaskedKey = k.MaskedKey,
                IsEnabled = k.IsEnabled,
                CreatedAt = k.CreatedAt
            };
        }).ToList();

        ViewBag.Channels = channels;
        return View(viewModels);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Create(CreateApiKeyViewModel viewModel)
    {
        if (string.IsNullOrWhiteSpace(viewModel.ChannelId))
        {
            ModelState.AddModelError("ChannelId", "请选择要绑定的 AI 渠道");
        }

        var channel = _channelService.GetChannelById(viewModel.ChannelId);
        if (channel == null)
        {
            ModelState.AddModelError("ChannelId", "所选渠道不存在");
        }

        if (!ModelState.IsValid)
        {
            TempData["ErrorMessage"] = "创建 API Key 失败，请检查填写内容。";
            return RedirectToAction(nameof(Index));
        }

        var (entity, plainKey) = _apiKeyService.CreateApiKey(
            viewModel.Name,
            viewModel.Remarks ?? string.Empty,
            viewModel.ChannelId);

        TempData["NewKey_Name"] = entity.Name;
        TempData["NewKey_Plain"] = plainKey;
        TempData["NewKey_Channel"] = channel!.Name;
        TempData["NewKey_Model"] = channel.Model;
        TempData["SuccessMessage"] = $"内部 API Key「{entity.Name}」创建成功！";

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Toggle(string id)
    {
        var key = _apiKeyService.GetApiKeyById(id);
        if (key == null)
        {
            TempData["ErrorMessage"] = "找不到指定的 API Key。";
            return RedirectToAction(nameof(Index));
        }

        var newState = _apiKeyService.ToggleApiKeyStatus(id);
        var stateText = newState ? "启用" : "禁用";
        TempData["SuccessMessage"] = $"API Key「{key.Name}」已{stateText}。";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Delete(string id)
    {
        var key = _apiKeyService.GetApiKeyById(id);
        if (key != null)
        {
            _apiKeyService.DeleteApiKey(id);
            TempData["SuccessMessage"] = $"API Key「{key.Name}」已成功删除。";
        }
        else
        {
            TempData["ErrorMessage"] = "找不到要删除的 API Key。";
        }

        return RedirectToAction(nameof(Index));
    }
}
