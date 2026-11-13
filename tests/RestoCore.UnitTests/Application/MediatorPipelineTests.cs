namespace RestoCore.UnitTests.Application;

using FluentAssertions;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using RestoCore.Application.Common.Behaviors;
using RestoCore.Application.Common.Mediator;
using Xunit;

public class MediatorPipelineTests
{
    public record SampleCommand(string Name, decimal Price) : IRequest<string>;

    public class SampleCommandValidator : AbstractValidator<SampleCommand>
    {
        public SampleCommandValidator()
        {
            RuleFor(x => x.Name).NotEmpty();
            RuleFor(x => x.Price).GreaterThan(0);
        }
    }

    public class SampleCommandHandler : IRequestHandler<SampleCommand, string>
    {
        public Task<string> Handle(SampleCommand request, CancellationToken cancellationToken)
            => Task.FromResult($"{request.Name}:{request.Price}");
    }

    public class TrackingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
        where TRequest : notnull
    {
        public List<string> ExecutionLog { get; }

        public TrackingBehavior(List<string> executionLog)
        {
            ExecutionLog = executionLog;
        }

        public async Task<TResponse> Handle(
            TRequest request,
            RequestHandlerDelegate<TRequest, TResponse> next,
            CancellationToken cancellationToken)
        {
            ExecutionLog.Add("BeforeHandler");
            var response = await next(request, cancellationToken);
            ExecutionLog.Add("AfterHandler");
            return response;
        }
    }

    [Fact]
    public async Task Mediator_Send_ShouldExecutePipelineBehaviorsAndCachedTypedInvoker()
    {
        // Arrange
        var executionLog = new List<string>();
        var services = new ServiceCollection();
        services.AddSingleton(executionLog);
        services.AddScoped<IMediator, Mediator>();
        services.AddScoped<IRequestHandler<SampleCommand, string>, SampleCommandHandler>();
        services.AddScoped<IValidator<SampleCommand>, SampleCommandValidator>();
        services.AddScoped(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
        services.AddScoped(typeof(IPipelineBehavior<,>), typeof(TrackingBehavior<,>));

        await using var provider = services.BuildServiceProvider();
        var mediator = provider.GetRequiredService<IMediator>();

        // Act - invoke twice to exercise InvokerCache hit path
        var first = await mediator.Send(new SampleCommand("Bife", 15000m));
        var second = await mediator.Send(new SampleCommand("Empanada", 2500m));

        // Assert
        first.Should().Be("Bife:15000");
        second.Should().Be("Empanada:2500");
        executionLog.Should().Equal("BeforeHandler", "AfterHandler", "BeforeHandler", "AfterHandler");
    }

    [Fact]
    public async Task Mediator_Send_ShouldThrowValidationExceptionWhenValidationBehaviorFails()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddScoped<IMediator, Mediator>();
        services.AddScoped<IRequestHandler<SampleCommand, string>, SampleCommandHandler>();
        services.AddScoped<IValidator<SampleCommand>, SampleCommandValidator>();
        services.AddScoped(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));

        await using var provider = services.BuildServiceProvider();
        var mediator = provider.GetRequiredService<IMediator>();

        // Act
        var act = async () => await mediator.Send(new SampleCommand("", -10m));

        // Assert
        await act.Should().ThrowAsync<ValidationException>();
    }
}
