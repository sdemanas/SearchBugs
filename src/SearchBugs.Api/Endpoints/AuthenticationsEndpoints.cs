
using MediatR;
using Microsoft.AspNetCore.Mvc;
using SearchBugs.Application.Authentications.Impersonate;
using SearchBugs.Application.Authentications.Login;
using SearchBugs.Application.Authentications.Register;
using SearchBugs.Application.Authentications.StopImpersonate;
using SearchBugs.Application.Users.ForgotPassword;
using SearchBugs.Application.Users.ResetPassword;
using Shared.Results;
using SearchBugs.Api.Extensions;

namespace SearchBugs.Api.Endpoints;

public static class AuthenticationsEndpoints
{
    public record LoginRequest(string Email, string Password);

    public record RefreshTokenRequest(string Token);

    public record RegisterRequest(string Email, string Password, string FirstName, string LastName);

    public record ImpersonateRequest(Guid UserIdToImpersonate);

    public record ForgotPasswordRequest(string Email);

    public record ResetPasswordRequest(string Email, string Token, string NewPassword);

    public static void MapAuthenticationsEndpoints(this IEndpointRouteBuilder app)
    {
        var auth = app.MapGroup("api/auth");
        auth.MapPost("/login", Login);
        auth.MapPost("/register", Register);
        auth.MapPost("/impersonate", Impersonate).RequireAuthorization();
        auth.MapPost("/stop-impersonate", StopImpersonate).RequireAuthorization();
        auth.MapPost("/forgot-password", ForgotPassword);
        auth.MapPost("/reset-password", ResetPassword);
    }

    public static async Task<IResult> Login([FromBody] LoginRequest req, ISender sender)
    {
        var command = new LoginCommand(req.Email, req.Password);
        var result = await sender.Send(command);
        return result!.ToHttpResult();
    }
    public static async Task<IResult> Register([FromBody] RegisterRequest req, ISender sender)
    {
        var command = new RegisterCommand(req.Email, req.Password, req.FirstName, req.LastName);
        var result = await sender.Send(command);
        return result!.ToHttpResult();
    }

    public static async Task<IResult> Impersonate([FromBody] ImpersonateRequest req, ISender sender)
    {
        var command = new ImpersonateCommand(req.UserIdToImpersonate);
        var result = await sender.Send(command);
        return result!.ToHttpResult();
    }

    public static async Task<IResult> StopImpersonate(ISender sender)
    {
        var command = new StopImpersonateCommand();
        var result = await sender.Send(command);
        return result!.ToHttpResult();
    }

    public static async Task<IResult> ForgotPassword([FromBody] ForgotPasswordRequest req, ISender sender)
    {
        var command = new ForgotPasswordCommand(req.Email);
        var result = await sender.Send(command);
        return result!.ToHttpResult();
    }

    public static async Task<IResult> ResetPassword([FromBody] ResetPasswordRequest req, ISender sender)
    {
        var command = new ResetPasswordCommand(req.Email, req.Token, req.NewPassword);
        var result = await sender.Send(command);
        return result!.ToHttpResult();
    }
}
