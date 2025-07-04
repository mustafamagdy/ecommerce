﻿using FSH.WebApi.Application.Common.Exceptions;
using FSH.WebApi.Application.Common.Mailing;
using FSH.WebApi.Application.Identity.Users.Password;
using FSH.WebApi.Shared.Multitenancy;
using Microsoft.AspNetCore.WebUtilities;

namespace FSH.WebApi.Infrastructure.Identity;

internal partial class UserService
{
  public async Task<string> ForgotPasswordAsync(ForgotPasswordRequest request, string origin)
  {
    EnsureValidTenant();

    var user = await _userManager.FindByEmailAsync(request.Email.Normalize());
    if (user is null || (_identitySettings.RequireConfirmedAccount && !await _userManager.IsEmailConfirmedAsync(user)))
    {
      // Don't reveal that the user does not exist or is not confirmed
      throw new InternalServerException(_t["An Error has occurred!"]);
    }

    // For more information on how to enable account confirmation and password reset please
    // visit https://go.microsoft.com/fwlink/?LinkID=532713
    string code = await _userManager.GeneratePasswordResetTokenAsync(user);
    const string route = "account/reset-password";
    var endpointUri = new Uri(string.Concat($"{origin}/", route));
    string passwordResetUrl = QueryHelpers.AddQueryString(endpointUri.ToString(), "Token", code);
    var mailRequest = new MailRequest(
      new List<string> { request.Email },
      _t["Reset Password"],
      _t[$"Your Password Reset Token is '{code}'. You can reset your password using the {endpointUri} Endpoint."]);
    _jobService.Enqueue(() => _mailService.SendAsync(mailRequest, CancellationToken.None));

    return _t["Password Reset Mail has been sent to your authorized Email."];
  }

  public async Task<string> ResetPasswordAsync(ResetPasswordRequest request)
  {
    var user = await _userManager.FindByEmailAsync(request.Email?.Normalize());

    // Don't reveal that the user does not exist
    _ = user ?? throw new InternalServerException(_t["An Error has occurred!"]);

    var result = await _userManager.ResetPasswordAsync(user, request.Token, request.Password);

    return result.Succeeded
      ? _t["Password Reset Successful!"]
      : throw new InternalServerException(_t["An Error has occurred!"]);
  }

  public async Task<string> ResetUserPasswordAsync(UserResetPasswordRequest request)
  {
    var user = await _userManager.FindByIdAsync(request.UserId.ToString());
    _ = user ?? throw new InternalServerException(_t["An Error has occurred!"]);
    var password = request.Password ?? MultitenancyConstants.TempPassword;
    var token = await _userManager.GeneratePasswordResetTokenAsync(user);
    var result = await _userManager.ResetPasswordAsync(user, token, password);

    return result.Succeeded
      ? password
      : throw new InternalServerException(_t["An Error has occurred!"]);
  }

  public async Task ChangePasswordAsync(ChangePasswordRequest model, string userId)
  {
    var user = await _userManager.FindByIdAsync(userId);

    _ = user ?? throw new NotFoundException(_t["User Not Found."]);

    var result = await _userManager.ChangePasswordAsync(user, model.Password, model.NewPassword);

    if (!result.Succeeded)
    {
      throw new InternalServerException(_t["Change password failed"], result.GetErrors(_t));
    }
  }
}