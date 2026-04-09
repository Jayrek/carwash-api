using System.ComponentModel.DataAnnotations;

namespace CarwashApi.DTO.Request;

public class SignUpRequestDto {
  [Required]
  [EmailAddress]
  [MaxLength(256)]
  public string Email { get; set; } = string.Empty;

  [Required]
  [MinLength(8)]
  public string Password { get; set; } = string.Empty;
  
}