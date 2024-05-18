using System.ComponentModel.DataAnnotations;

namespace AI_KeyHub.Models.ViewModels;

public class ChannelViewModel
{
    public string? Id { get; set; }

    [Required(ErrorMessage = "请输入渠道名称")]
    [Display(Name = "渠道名称")]
    [StringLength(50, ErrorMessage = "渠道名称长度不能超过 50 个字符")]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "请输入 API 地址")]
    [Display(Name = "API 地址")]
    [Url(ErrorMessage = "请输入合法的 URL 地址，如 https://api.openai.com")]
    public string BaseUrl { get; set; } = string.Empty;

    [Display(Name = "上游 API Key")]
    public string? ApiKey { get; set; }

    [Required(ErrorMessage = "请输入模型名称")]
    [Display(Name = "模型名称")]
    [StringLength(50, ErrorMessage = "模型名称长度不能超过 50 个字符")]
    public string Model { get; set; } = string.Empty;

    [Display(Name = "启用状态")]
    public bool IsEnabled { get; set; } = true;

    public string? MaskedApiKey { get; set; }
    public DateTime? CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
