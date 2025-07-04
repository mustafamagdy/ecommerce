﻿namespace FSH.WebApi.Infrastructure.Identity;

public sealed class RegisterUserEmailModel
{
  public string UserName { get; set; } = default!;
  public string Email { get; set; } = default!;
  public string Url { get; set; } = default!;
}