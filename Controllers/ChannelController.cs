using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using AI_KeyHub.Models;
using AI_KeyHub.Models.ViewModels;
using AI_KeyHub.Services;

namespace AI_KeyHub.Controllers;

[Authorize]
public class ChannelController : Controller
{
    private readonly IChannelService _channelService;

    public ChannelController(IChannelService channelService)
    {
        _channelService = channelService;
    }

    [HttpGet]
    public IActionResult Index()
    {
        var channels = _channelService.GetAllChannels();
        var viewModels = channels.Select(c => new ChannelViewModel
        {
            Id = c.Id,
            Name = c.Name,
            BaseUrl = c.BaseUrl,
            MaskedApiKey = _channelService.MaskApiKey(c.ApiKey),
            Model = c.Model,
            IsEnabled = c.IsEnabled,
            CreatedAt = c.CreatedAt,
            UpdatedAt = c.UpdatedAt
        }).ToList();

        return View(viewModels);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Create(ChannelViewModel viewModel)
    {
        if (string.IsNullOrWhiteSpace(viewModel.ApiKey))
        {
            ModelState.AddModelError("ApiKey", "创建渠道时上游 API Key 不能为空");
        }

        if (!ModelState.IsValid)
        {
            TempData["ErrorMessage"] = "创建渠道失败，请检查填写内容。";
            return RedirectToAction(nameof(Index));
        }

        var channel = new Channel
        {
            Name = viewModel.Name.Trim(),
            BaseUrl = viewModel.BaseUrl.Trim().TrimEnd('/'),
            ApiKey = viewModel.ApiKey!.Trim(),
            Model = viewModel.Model.Trim(),
            IsEnabled = viewModel.IsEnabled
        };

        _channelService.AddChannel(channel);
        TempData["SuccessMessage"] = $"渠道「{channel.Name}」创建成功！";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Edit(ChannelViewModel viewModel)
    {
        if (string.IsNullOrWhiteSpace(viewModel.Id))
        {
            TempData["ErrorMessage"] = "无效的渠道编号。";
            return RedirectToAction(nameof(Index));
        }

        if (!ModelState.IsValid)
        {
            TempData["ErrorMessage"] = "编辑渠道失败，请检查填写内容。";
            return RedirectToAction(nameof(Index));
        }

        var channel = new Channel
        {
            Id = viewModel.Id,
            Name = viewModel.Name.Trim(),
            BaseUrl = viewModel.BaseUrl.Trim().TrimEnd('/'),
            ApiKey = viewModel.ApiKey?.Trim() ?? string.Empty,
            Model = viewModel.Model.Trim(),
            IsEnabled = viewModel.IsEnabled
        };

        _channelService.UpdateChannel(channel, keepExistingApiKeyIfEmpty: true);
        TempData["SuccessMessage"] = $"渠道「{channel.Name}」更新成功！";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Toggle(string id)
    {
        var channel = _channelService.GetChannelById(id);
        if (channel == null)
        {
            TempData["ErrorMessage"] = "找不到指定的渠道。";
            return RedirectToAction(nameof(Index));
        }

        var newState = _channelService.ToggleChannelStatus(id);
        var stateText = newState ? "启用" : "禁用";
        TempData["SuccessMessage"] = $"渠道「{channel.Name}」已{stateText}。";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Delete(string id)
    {
        var channel = _channelService.GetChannelById(id);
        if (channel != null)
        {
            _channelService.DeleteChannel(id);
            TempData["SuccessMessage"] = $"渠道「{channel.Name}」已成功删除。";
        }
        else
        {
            TempData["ErrorMessage"] = "找不到要删除的渠道。";
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public IActionResult GetDetail(string id)
    {
        var channel = _channelService.GetChannelById(id);
        if (channel == null)
        {
            return NotFound();
        }

        return Json(new
        {
            id = channel.Id,
            name = channel.Name,
            baseUrl = channel.BaseUrl,
            model = channel.Model,
            isEnabled = channel.IsEnabled
        });
    }
}
