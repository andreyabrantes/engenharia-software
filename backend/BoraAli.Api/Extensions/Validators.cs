using BoraAli.Api.DTOs;
using FluentValidation;

namespace BoraAli.Api.Extensions;

/// <summary>
/// Validadores FluentValidation para os DTOs
/// </summary>
public class CreateEventValidator : AbstractValidator<CreateEventDto>
{
    public CreateEventValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("O título do evento é obrigatório")
            .MaximumLength(200).WithMessage("O título deve ter no máximo 200 caracteres");

        RuleFor(x => x.Description)
            .NotEmpty().WithMessage("A descrição é obrigatória")
            .MaximumLength(500).WithMessage("A descrição deve ter no máximo 500 caracteres");

        RuleFor(x => x.EventDate)
            .GreaterThan(DateTime.UtcNow).WithMessage("A data do evento deve ser futura");

        RuleFor(x => x.Time)
            .NotEmpty().WithMessage("O horário é obrigatório");

        RuleFor(x => x.Location)
            .NotEmpty().WithMessage("O local é obrigatório")
            .MaximumLength(200).WithMessage("O local deve ter no máximo 200 caracteres");

        RuleFor(x => x.Address)
            .NotEmpty().WithMessage("O endereço é obrigatório")
            .MaximumLength(500).WithMessage("O endereço deve ter no máximo 500 caracteres");

        RuleFor(x => x.City)
            .NotEmpty().WithMessage("A cidade é obrigatória")
            .MaximumLength(100).WithMessage("A cidade deve ter no máximo 100 caracteres");

        RuleFor(x => x.Cep)
            .MaximumLength(8).WithMessage("O CEP deve ter no máximo 8 caracteres");

        RuleFor(x => x.Street)
            .MaximumLength(200).WithMessage("O logradouro deve ter no máximo 200 caracteres");

        RuleFor(x => x.Neighborhood)
            .MaximumLength(100).WithMessage("O bairro deve ter no máximo 100 caracteres");

        RuleFor(x => x.State)
            .MaximumLength(2).WithMessage("O estado deve ter 2 caracteres");

        RuleFor(x => x.AddressNumber)
            .MaximumLength(20).WithMessage("O número deve ter no máximo 20 caracteres");

        RuleFor(x => x.CategoryId)
            .GreaterThan(0).WithMessage("A categoria é obrigatória");

        RuleFor(x => x.Tickets)
            .NotEmpty().WithMessage("Pelo menos um tipo de ingresso é obrigatório");

        RuleForEach(x => x.Tickets).SetValidator(new CreateTicketTypeValidator());
    }
}

public class UpdateEventValidator : AbstractValidator<UpdateEventDto>
{
    public UpdateEventValidator()
    {
        When(x => x.Title != null, () =>
        {
            RuleFor(x => x.Title!)
                .NotEmpty().WithMessage("O título não pode ser vazio")
                .MaximumLength(200).WithMessage("O título deve ter no máximo 200 caracteres");
        });

        When(x => x.Description != null, () =>
        {
            RuleFor(x => x.Description!)
                .NotEmpty().WithMessage("A descrição não pode ser vazia")
                .MaximumLength(500).WithMessage("A descrição deve ter no máximo 500 caracteres");
        });

        When(x => x.EventDate != null, () =>
        {
            RuleFor(x => x.EventDate!)
                .GreaterThan(DateTime.UtcNow).WithMessage("A data do evento deve ser futura");
        });

        When(x => x.Status != null, () =>
        {
            RuleFor(x => x.Status!)
                .Must(s => new[] { "Draft", "Published", "Cancelled", "Finished" }.Contains(s))
                .WithMessage("Status inválido. Use: Draft, Published, Cancelled ou Finished");
        });
    }
}

public class CreateTicketTypeValidator : AbstractValidator<CreateTicketTypeDto>
{
    public CreateTicketTypeValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("O nome do ingresso é obrigatório")
            .MaximumLength(100).WithMessage("O nome deve ter no máximo 100 caracteres");

        RuleFor(x => x.Price)
            .GreaterThanOrEqualTo(0).WithMessage("O preço deve ser maior ou igual a zero");

        RuleFor(x => x.TotalQuantity)
            .GreaterThan(0).WithMessage("A quantidade deve ser maior que zero");
    }
}

public class RegisterUserValidator : AbstractValidator<RegisterUserDto>
{
    public RegisterUserValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("O nome é obrigatório")
            .MaximumLength(100).WithMessage("O nome deve ter no máximo 100 caracteres");

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("O email é obrigatório")
            .EmailAddress().WithMessage("Email inválido")
            .MaximumLength(200).WithMessage("O email deve ter no máximo 200 caracteres");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("A senha é obrigatória")
            .MinimumLength(6).WithMessage("A senha deve ter no mínimo 6 caracteres")
            .MaximumLength(100).WithMessage("A senha deve ter no máximo 100 caracteres");

        RuleFor(x => x.Cpf)
            .NotEmpty().WithMessage("O CPF é obrigatório")
            .Must(BeValidCpf).WithMessage("CPF inválido. Informe 11 dígitos numéricos.");

        RuleFor(x => x.Role)
            .NotEmpty().WithMessage("O perfil é obrigatório")
            .Must(role => role == "Cliente" || role == "Organizador")
            .WithMessage("Perfil inválido. Escolha 'Cliente' ou 'Organizador'.");
    }

    private static bool BeValidCpf(string? cpf)
    {
        if (string.IsNullOrWhiteSpace(cpf)) return false;
        var numbers = cpf.Replace(".", "").Replace("-", "").Trim();
        if (numbers.Length != 11 || numbers.All(c => c == numbers[0])) return false;

        int[] multiplier1 = { 10, 9, 8, 7, 6, 5, 4, 3, 2 };
        int[] multiplier2 = { 11, 10, 9, 8, 7, 6, 5, 4, 3, 2 };

        var temp = numbers[..9];
        var sum = temp.Select((t, i) => int.Parse(t.ToString()) * multiplier1[i]).Sum();
        var remainder = sum % 11;
        var digit1 = remainder < 2 ? 0 : 11 - remainder;

        temp += digit1;
        sum = temp.Select((t, i) => int.Parse(t.ToString()) * multiplier2[i]).Sum();
        remainder = sum % 11;
        var digit2 = remainder < 2 ? 0 : 11 - remainder;

        return numbers.EndsWith($"{digit1}{digit2}");
    }
}

public class LoginValidator : AbstractValidator<LoginDto>
{
    public LoginValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("O email é obrigatório")
            .EmailAddress().WithMessage("Email inválido");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("A senha é obrigatória");
    }
}

public class CreateOrderValidator : AbstractValidator<CreateOrderDto>
{
    public CreateOrderValidator()
    {
        RuleFor(x => x.EventId)
            .GreaterThan(0).WithMessage("O evento é obrigatório");

        RuleFor(x => x.Items)
            .NotEmpty().WithMessage("Pelo menos um item é obrigatório");

        RuleForEach(x => x.Items).SetValidator(new CreateOrderItemValidator());
    }
}

public class CreateOrderItemValidator : AbstractValidator<CreateOrderItemDto>
{
    public CreateOrderItemValidator()
    {
        RuleFor(x => x.TicketTypeId)
            .GreaterThan(0).WithMessage("O tipo de ingresso é obrigatório");

        RuleFor(x => x.Quantity)
            .GreaterThan(0).WithMessage("A quantidade deve ser maior que zero")
            .LessThanOrEqualTo(10).WithMessage("Máximo de 10 ingressos por tipo por pedido");
    }
}
