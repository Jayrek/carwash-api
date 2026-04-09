namespace CarwashApi.DTO.Response;

public class UserResponseDto {
  public Guid Id { get; set; }
  public string Email { get; set; } = string.Empty;
  public string FirstName { get; set; } = string.Empty;
  public string LastName { get; set; } = string.Empty;
  public string Phone { get; set; } = string.Empty;
  public string Role { get; set; } = string.Empty;
  public bool IsActive { get; set; } = true;
  public string ProfileImageUrl { get; set; } = string.Empty;
}