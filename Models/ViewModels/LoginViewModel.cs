using System.ComponentModel.DataAnnotations;

namespace AI_KeyHub.Models.ViewModels;

public class LoginViewModel
{
    [Required(ErrorMessage = "请输入用户名")]
    [Display(Name = "用户名")]
    public string Username { get; set; } = string.Empty;

    [Required(ErrorMessage = "请输入密码")]
    [DataType(DataType.Password)]
    [Display(Name = "密码")]
    public string Password { get; set; } = string.Empty;

    [Display(Name = "保持登录")]
    public bool RememberMe { get; set; }
}
