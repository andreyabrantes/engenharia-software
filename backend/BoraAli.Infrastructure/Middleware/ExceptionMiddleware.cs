using System;
using System.Net;
using System.Text.Json;
using BoraAli.Core.Exceptions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace BoraAli.Infrastructure.Middleware;

/// <summary>
/// Middleware global para tratamento de exceções
/// Segurança: em produção, nunca expõe detalhes internos (stack traces, nomes de tabelas, etc.)
/// </summary>
public class ExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionMiddleware> _logger;
    private readonly IHostEnvironment _environment;

    public ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger, IHostEnvironment environment)
    {
        _next = next;
        _logger = logger;
        _environment = environment;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (NotFoundException ex)
        {
            _logger.LogWarning(ex, "Recurso não encontrado");
            await HandleExceptionAsync(context, HttpStatusCode.NotFound, ex.Message);
        }
        catch (BadRequestException ex)
        {
            _logger.LogWarning(ex, "Requisição inválida");
            await HandleExceptionAsync(context, HttpStatusCode.BadRequest, ex.Message);
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning(ex, "Acesso não autorizado");
            await HandleExceptionAsync(context, HttpStatusCode.Unauthorized, "Acesso não autorizado");
        }
        catch (OperationCanceledException ex)
        {
            _logger.LogWarning(ex, "Requisição cancelada pelo cliente (timeout)");
            // Não retorna resposta — o cliente já desconectou
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro interno do servidor");

            // Em produção, retorna mensagem genérica para não expor detalhes internos
            var message = _environment.IsProduction()
                ? "Ocorreu um erro interno no servidor"
                : $"Erro interno: {ex.Message}";

            await HandleExceptionAsync(context, HttpStatusCode.InternalServerError, message);
        }
    }

    private static async Task HandleExceptionAsync(HttpContext context, HttpStatusCode statusCode, string message)
    {
        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)statusCode;

        var response = new
        {
            success = false,
            message,
            data = (object?)null,
            errors = (List<string>?)null
        };

        var jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        await context.Response.WriteAsync(JsonSerializer.Serialize(response, jsonOptions));
    }
}
