namespace CarwashApi.DTO.Request;

public class UpdateProfileRequestDto {
  public string FirstName { get; set; } = string.Empty;
  public string LastName { get; set; } = string.Empty;
  public string Phone { get; set; } = string.Empty;
  public string ProfileImageUrl { get; set; } = string.Empty;
}