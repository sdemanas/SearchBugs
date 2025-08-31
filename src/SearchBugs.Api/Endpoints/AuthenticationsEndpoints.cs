
using MediatR;
using SearchBugs.Application.Authentications.Login;
using SearchBugs.Application.Authentications.Register;
using SearchBugs.Application.Authentications.Impersonate;
using SearchBugs.Application.Authentications.StopImpersonate;
using Shared.Results;
using SearchBugs.Api.Extensions;

namespace SearchBugs.Api.Endpoints;

public static class AuthenticationsEndpoints
{
    public record LoginRequest(string Email, string Password);

    public record RefreshTokenRequest(string Token);

    public record RegisterRequest(string Email, string Password, string FirstName, string LastName);

    public record ImpersonateRequest(Guid UserIdToImpersonate);

    public static void MapAuthenticationsEndpoints(this IEndpointRouteBuilder app)
    {
        var auth = app.MapGroup("api/auth");
        auth.MapPost("login", Login).WithName(nameof(Login));
        auth.MapPost("register", Register).WithName(nameof(Register));
        auth.MapPost("impersonate", Impersonate).WithName(nameof(Impersonate)).RequireAuthorization();
        auth.MapPost("stop-impersonate", StopImpersonate).WithName(nameof(StopImpersonate)).RequireAuthorization();
    }

    public static async Task<IResult> Login(LoginRequest req, ISender sender)
    {
        var command = new LoginCommand(req.Email, req.Password);
        var result = await sender.Send(command);
        return result!.ToHttpResult();
    }
    public static async Task<IResult> Register(RegisterRequest req, ISender sender)
    {
        var command = new RegisterCommand(req.Email, req.Password, req.FirstName, req.LastName);
        var result = await sender.Send(command);
        return result!.ToHttpResult();
    }

    public static async Task<IResult> Impersonate(ImpersonateRequest req, ISender sender)
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
}
