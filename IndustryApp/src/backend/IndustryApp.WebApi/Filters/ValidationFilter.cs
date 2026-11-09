using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Mvc.Filters;

namespace IndustryApp.WebApi.Filters;

/// <summary>
/// İstek gövdesini FluentValidation ile doğrular. Validator'lar interface üzerine yazıldığı için (ör.
/// AbstractValidator&lt;IRegisterCustomerRequest&gt;) argümanın kendi tipi ve uyguladığı tüm interface'ler için kayıtlı
/// validator'lar çalıştırılır. Hata varsa ValidationException → GlobalExceptionHandler → 400 VALIDATION_FAILED.
/// </summary>
public sealed class ValidationFilter(IServiceProvider services) : IAsyncActionFilter
{
    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var failures = new List<ValidationFailure>();

        foreach (var argument in context.ActionArguments.Values)
        {
            if (argument is null || argument is CancellationToken || argument.GetType().IsPrimitive || argument is string or Guid)
                continue;

            var type = argument.GetType();
            foreach (var target in type.GetInterfaces().Prepend(type))
            {
                var validatorType = typeof(IValidator<>).MakeGenericType(target);
                foreach (var validator in services.GetServices(validatorType).OfType<IValidator>())
                {
                    var result = await validator.ValidateAsync(new ValidationContext<object>(argument), context.HttpContext.RequestAborted);
                    failures.AddRange(result.Errors);
                }
            }
        }

        if (failures.Count > 0)
            throw new ValidationException(failures);

        await next();
    }
}
