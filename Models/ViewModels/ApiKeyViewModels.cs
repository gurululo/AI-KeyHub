using System.ComponentModel.DataAnnotations;

namespace AI_KeyHub.Models.ViewModels;

public class ApiKeyViewModel
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Remarks { get; set; } = string.Empty;
    public string ChannelId { get; set; } = string.Empty;
    public string ChannelName { get; set; } = string.Empty;
    public string BoundModel { get; set; } = string.Empty;
    public bool ChannelEnabled { get; set; }
    public string MaskedKey { get; set; } = string.Empty;
    public bool IsEnabled { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class CreateApiKeyViewModel
{
    [Required(ErrorMessage = "请输入 API Key 名称")]
    [Display(Name = "Key 名称")]
    [StringLength(50, ErrorMessage = "名称长度不能超过 50 个字符")]
    public string Name { get; set; } = string.Empty;

    [Display(Name = "备注信息")]
    [StringLength(200, ErrorMessage = "备注长度不能超过 200 个字符")]
    public string? Remarks { get; set; }

    [Required(ErrorMessage = "请选择绑定的 AI 渠道")]
    [Display(Name = "绑定的 AI 渠道")]
    public string ChannelId { get; set; } = string.Empty;
}
