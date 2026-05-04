using ErrorOr;
using FluentValidation;
using GymSaaS.Application.Abstractions.Auth;
using GymSaaS.Application.Features.Auth;
using MediatR;

public sealed record RegisterTenantCommand(
    string TenantSlug,
    string DisplayName,
    string Email,
    string Password) : IRequest<ErrorOr<AuthResponse>>;

public sealed class RegisterTenantCommandValidator : AbstractValidator<RegisterTenantCommand>
{
    public RegisterTenantCommandValidator()
    {
        RuleFor(x => x.TenantSlug).NotEmpty().MaximumLength(160);
        RuleFor(x => x.DisplayName).NotEmpty().MaximumLength(260);
        RuleFor(x => x.Email).NotEmpty().EmailAddress().MaximumLength(256);
        RuleFor(x => x.Password).MinimumLength(8).MaximumLength(128);
    }
}

public sealed class RegisterTenantCommandHandler(IAuthService auth) : IRequestHandler<RegisterTenantCommand, ErrorOr<AuthResponse>>
{
    public Task<ErrorOr<AuthResponse>> Handle(RegisterTenantCommand request, CancellationToken cancellationToken) => 
        auth.RegisterTenantAsync(
            request.TenantSlug,
            request.DisplayName,
            request.Email,
            request.Password,
            cancellationToken
        );
    
}

public sealed record LoginCommand(string Email, string Password, string TenantSlug)
    : IRequest<ErrorOr<AuthResponse>>;

public sealed class LoginCommandValidator : AbstractValidator<LoginCommand>
{
    public LoginCommandValidator()
    {
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
        RuleFor(x => x.Password).NotEmpty();
        RuleFor(x => x.TenantSlug).NotEmpty().MaximumLength(160);
    }
}

public sealed class LoginCommandHandler(IAuthService auth)
    : IRequestHandler<LoginCommand, ErrorOr<AuthResponse>>
{
    public Task<ErrorOr<AuthResponse>> Handle(LoginCommand request, CancellationToken cancellationToken) =>
        auth.LoginAsync(request.Email, request.Password, request.TenantSlug, cancellationToken);
}