using CryptoWise.BlazorWasmClient.Components.PageComponents.SignUp;
using CryptoWise.BlazorWasmClient.Entities;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Update;

namespace CryptoWise.BlazorWasmClient.Validators;

public class DecentralizedSignUpValidator : AbstractValidator<InitiateSignUpData>
{
    public DecentralizedSignUpValidator()
    {
        RuleFor(x => x.UserIdentificator)
            .NotEmpty()
            .WithMessage("Field username should not be empty");
    }

    public Func<object, string, Task<IEnumerable<string>>> ValidateValue => async (model, propertyName) =>
    {
        var result = await ValidateAsync(ValidationContext<InitiateSignUpData>
            .CreateWithOptions((InitiateSignUpData)model, x =>
                x.IncludeProperties(propertyName)));
        return result.IsValid ? Array.Empty<string>() : result.Errors.Select(e => e.ErrorMessage);
    };
}