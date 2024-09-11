using HandUp.ServiceComposition;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using System.Text.RegularExpressions;
using Xunit;

namespace HandUp.UnitTests.ServiceComposition;

public class ServiceComposerTests
{
    private readonly TestLogger logger;
    private readonly HandUpConfiguration configuration;
    private readonly TheRequest request;
    private readonly TheResponse response;

    public ServiceComposerTests()
    {
        logger = new TestLogger();
        configuration = new HandUpConfiguration();
        request = new TheRequest();
        response = new TheResponse();
    }

    [Fact]
    public async Task ComposeAsync_WhenThereAreNoParticipators_ShouldThrow()
    {
        // Arrange
        var serviceCollection = new ServiceCollection();

        await using var scopedServiceProvider = serviceCollection.BuildServiceProvider();

        var sut = new ServiceComposer(logger, configuration, scopedServiceProvider);

        // Act
        var ex = await Record.ExceptionAsync(() => sut.ComposeAsync(request, response));

        // Assert
        Assert.NotNull(ex);
        Assert.IsType<InvalidOperationException>(ex);
        Assert.Equal($"No participators are available for request {request} / response {response}. There must be at least one.", ex.Message);
    }

    [Fact]
    public async Task ComposeAsync_WhenThereIsMoreThanOneStructureInitializer_ShouldThrow()
    {
        // Arrange
        var serviceCollection = new ServiceCollection()
            .AddScoped<RequestParticipator<TheRequest, TheResponse>, InitializerParticipator1>()
            .AddScoped<RequestParticipator<TheRequest, TheResponse>, InitializerParticipator2>();

        await using var scopedServiceProvider = serviceCollection.BuildServiceProvider();

        var sut = new ServiceComposer(logger, configuration, scopedServiceProvider);

        // Act
        var ex = await Record.ExceptionAsync(() => sut.ComposeAsync(request, response));

        // Assert
        Assert.NotNull(ex);
        Assert.IsType<InvalidOperationException>(ex);
        Assert.Equal($"More than one participator (2) has "
                     + $"'{nameof(RequestParticipator<object, object>.IsStructureInitializer)}' set to true "
                     + $"for request {request} / response {response}. There can only be one per composition.", ex.Message);
    }

    [Fact]
    public async Task ComposeAsync_WhenThereIsASingleStructureInitializer_ShouldRunThatParticipator_AndReturnResult()
    {
        // Arrange
        var serviceCollection = new ServiceCollection()
            .AddScoped<RequestParticipator<TheRequest, TheResponse>, InitializerParticipator1>();

        await using var scopedServiceProvider = serviceCollection.BuildServiceProvider();

        var sut = new ServiceComposer(logger, configuration, scopedServiceProvider);

        request.ValueForStructureInitializer = Guid.NewGuid();

        // Act
        var result = await sut.ComposeAsync(request, response);

        // Assert
        Assert.False(result.HasErrors);
        Assert.Empty(result.Errors);
        Assert.False(result.NotFoundOrNoResults);
        Assert.True(result.StructureInitialized);
        Assert.Equal(request.ValueForStructureInitializer, result.Response.ValueSetByStructureInitializer);
    }

    [Fact]
    public async Task ComposeAsync_WhenThereIsASingleStructureInitializerParticipator_AndAnotherTwoParticipators_ShouldRunTheInitialParticipatorFirst_ThenTheOthers_AndReturnResult()
    {
        // Arrange
        var serviceCollection = new ServiceCollection()
            .AddScoped<RequestParticipator<TheRequest, TheResponse>, InitializerParticipator1>()
            .AddScoped<RequestParticipator<TheRequest, TheResponse>, SubsequentParticipator1>()
            .AddScoped<RequestParticipator<TheRequest, TheResponse>, SubsequentParticipator2>();

        await using var scopedServiceProvider = serviceCollection.BuildServiceProvider();

        var sut = new ServiceComposer(logger, configuration, scopedServiceProvider);

        request.ValueForStructureInitializer = Guid.NewGuid();
        request.ValueForSubsequentButSetByInitializer = Guid.NewGuid();
        request.Value2ForSubsequentButSetByInitializer = Guid.NewGuid();

        // Act
        var result = await sut.ComposeAsync(request, response);

        // Assert
        Assert.False(result.HasErrors);
        Assert.Empty(result.Errors);
        Assert.False(result.NotFoundOrNoResults);
        Assert.True(result.StructureInitialized);
        Assert.Equal(request.ValueForStructureInitializer, result.Response.ValueSetByStructureInitializer);
        Assert.Equal(request.ValueForSubsequentButSetByInitializer, result.Response.ValueForSubsequentButSetByInitializer);
        Assert.Equal(request.ValueForSubsequentButSetByInitializer, result.Response.SubsequentValue);
        Assert.Equal(request.Value2ForSubsequentButSetByInitializer, result.Response.Value2ForSubsequentButSetByInitializer);
        Assert.Equal(request.Value2ForSubsequentButSetByInitializer, result.Response.SubsequentValue2);
    }

    [Fact]
    public async Task ComposeAsync_WhenJustToTwoNonInitializerParticipators_ShouldRunTheParticipators_AndReturnResult()
    {
        // Arrange
        var serviceCollection = new ServiceCollection()
            .AddScoped<RequestParticipator<TheRequest, TheResponse>, NonSubsequentParticipator1>()
            .AddScoped<RequestParticipator<TheRequest, TheResponse>, NonSubsequentParticipator2>();

        await using var scopedServiceProvider = serviceCollection.BuildServiceProvider();

        var sut = new ServiceComposer(logger, configuration, scopedServiceProvider);

        request.ValueForParticipator1 = Guid.NewGuid();
        request.ValueForParticipator2 = Guid.NewGuid();

        // Act
        var result = await sut.ComposeAsync(request, response);

        // Assert
        Assert.False(result.HasErrors);
        Assert.Empty(result.Errors);
        Assert.False(result.NotFoundOrNoResults);
        Assert.False(result.StructureInitialized);
        Assert.Equal(Guid.Empty, result.Response.ValueSetByStructureInitializer);
        Assert.Equal(Guid.Empty, result.Response.ValueForSubsequentButSetByInitializer);
        Assert.Equal(Guid.Empty, result.Response.SubsequentValue);
        Assert.Equal(Guid.Empty, result.Response.Value2ForSubsequentButSetByInitializer);
        Assert.Equal(Guid.Empty, result.Response.SubsequentValue2);
        Assert.Equal(request.ValueForParticipator1, result.Response.ValueForParticipator1);
        Assert.Equal(request.ValueForParticipator2, result.Response.ValueForParticipator2);
    }

    [Fact]
    public async Task ComposeAsync_WhenThereIsASingleStructureInitializerParticipatorWhichThrows_AndAnotherTwoParticipators_ShouldRunTheInitialParticipatorFirst_ThenFail_ThenRollbackOnlyTheInitializer()
    {
        // Arrange
        var serviceCollection = new ServiceCollection()
            .AddScoped<RequestParticipator<TheRequest, TheResponse>, InitializerParticipator1>()
            .AddScoped<RequestParticipator<TheRequest, TheResponse>, SubsequentParticipator1>()
            .AddScoped<RequestParticipator<TheRequest, TheResponse>, SubsequentParticipator2>();

        await using var scopedServiceProvider = serviceCollection.BuildServiceProvider();

        var sut = new ServiceComposer(logger, configuration, scopedServiceProvider);

        request.ValueForStructureInitializer = Guid.NewGuid();
        request.ValueForSubsequentButSetByInitializer = Guid.NewGuid();
        request.Value2ForSubsequentButSetByInitializer = Guid.NewGuid();
        request.InitializerShouldThrow = true;
        request.InitializerErrorMessage = Guid.NewGuid().ToString();
        request.InitializerRollbackValue = Guid.NewGuid();
        request.SubsequentRollbackValue = Guid.NewGuid();
        request.SubsequentRollbackValue2 = Guid.NewGuid();

        // Act
        var result = await sut.ComposeAsync(request, response);

        // Assert
        Assert.True(result.HasErrors);
        Assert.Single(result.Errors);
        Assert.Equal("An exception was handled during service participation. See logs", result.Errors.Single());
        Assert.False(result.NotFoundOrNoResults);
        Assert.True(result.StructureInitialized);
        Assert.Equal(request.ValueForStructureInitializer, result.Response.ValueSetByStructureInitializer);
        Assert.Equal(request.ValueForSubsequentButSetByInitializer, result.Response.ValueForSubsequentButSetByInitializer);
        Assert.Equal(Guid.Empty, result.Response.SubsequentValue);
        Assert.Equal(request.Value2ForSubsequentButSetByInitializer, result.Response.Value2ForSubsequentButSetByInitializer);
        Assert.Equal(Guid.Empty, result.Response.SubsequentValue2);
        Assert.Equal(request.InitializerRollbackValue, result.Response.InitializerRollbackValue);
        Assert.Equal(Guid.Empty, result.Response.SubsequentRollbackValue);
        Assert.Equal(Guid.Empty, result.Response.SubsequentRollbackValue2);

        Assert.True(
            logger.LoggedException<Exception>(
                request.InitializerErrorMessage,
                "An exception was handled during request handling for request {requestType} and response {responseType}. Attempting rollback",
                typeof(TheRequest), 
                typeof(TheResponse)
            )
        );
    }

    [Fact]
    public async Task ComposeAsync_WhenThereIsASingleStructureInitializerParticipatorWhichThrows_AndAnotherTwoParticipators_ShouldRunTheInitialParticipatorFirst_ThenFailRunningTheOthers_ThenRollbackAll()
    {
        // Arrange
        var serviceCollection = new ServiceCollection()
            .AddScoped<RequestParticipator<TheRequest, TheResponse>, InitializerParticipator1>()
            .AddScoped<RequestParticipator<TheRequest, TheResponse>, SubsequentParticipator1>()
            .AddScoped<RequestParticipator<TheRequest, TheResponse>, SubsequentParticipator2>();

        await using var scopedServiceProvider = serviceCollection.BuildServiceProvider();

        var sut = new ServiceComposer(logger, configuration, scopedServiceProvider);

        request.ValueForStructureInitializer = Guid.NewGuid();
        request.ValueForSubsequentButSetByInitializer = Guid.NewGuid();
        request.Value2ForSubsequentButSetByInitializer = Guid.NewGuid();
        request.SubsequentRollbackValue = Guid.NewGuid();
        request.SubsequentRollbackValue2 = Guid.NewGuid();
        request.Subsequent2ShouldError = true;
        request.Subsequent2Error = Guid.NewGuid().ToString();

        // Act
        var result = await sut.ComposeAsync(request, response);

        // Assert
        Assert.True(result.HasErrors);
        Assert.Single(result.Errors);
        Assert.Equal("An exception was handled during service participation. See logs", result.Errors.Single());
        Assert.False(result.NotFoundOrNoResults);
        Assert.True(result.StructureInitialized);
        Assert.Equal(request.ValueForStructureInitializer, result.Response.ValueSetByStructureInitializer);
        Assert.Equal(request.ValueForSubsequentButSetByInitializer, result.Response.ValueForSubsequentButSetByInitializer);
        Assert.Equal(request.ValueForSubsequentButSetByInitializer, result.Response.SubsequentValue);
        Assert.Equal(request.Value2ForSubsequentButSetByInitializer, result.Response.Value2ForSubsequentButSetByInitializer);
        Assert.Equal(request.Value2ForSubsequentButSetByInitializer, result.Response.SubsequentValue2);
        Assert.Equal(request.InitializerRollbackValue, result.Response.InitializerRollbackValue);
        Assert.Equal(request.SubsequentRollbackValue, result.Response.SubsequentRollbackValue);
        Assert.Equal(request.SubsequentRollbackValue2, result.Response.SubsequentRollbackValue2);

        Assert.True(
            logger.LoggedException<Exception>(
                request.Subsequent2Error,
                "An exception was handled during request handling for request {requestType} and response {responseType}. Attempting rollback",
                typeof(TheRequest),
                typeof(TheResponse)
            )
        );
    }
}

public record TheRequest
{
    public Guid ValueForStructureInitializer { get; set; }
    public Guid ValueForSubsequentButSetByInitializer { get; set; }
    public Guid Value2ForSubsequentButSetByInitializer { get; set; }
    public Guid ValueForParticipator1 { get; set; }
    public Guid ValueForParticipator2 { get; set; }
    public bool InitializerShouldThrow { get; set; }
    public string InitializerErrorMessage { get; set; } = string.Empty;
    public Guid InitializerRollbackValue { get; set; }
    public Guid SubsequentRollbackValue { get; set; }
    public Guid SubsequentRollbackValue2 { get; set; }
    public bool Subsequent2ShouldError { get; set; }
    public string Subsequent2Error { get; set; } = string.Empty;
}

public record TheResponse
{
    public Guid ValueSetByStructureInitializer { get; set; }
    public Guid ValueForSubsequentButSetByInitializer { get; set; }
    public Guid Value2ForSubsequentButSetByInitializer { get; set; }
    public Guid SubsequentValue { get; set; }
    public Guid SubsequentValue2 { get; set; }
    public Guid ValueForParticipator1 { get; set; }
    public Guid ValueForParticipator2 { get; set; }
    public Guid InitializerRollbackValue { get; set; }
    public Guid SubsequentRollbackValue { get; set; }
    public Guid SubsequentRollbackValue2 { get; set; }
}

public class InitializerParticipator1 : RequestParticipator<TheRequest, TheResponse>
{
    public override bool IsStructureInitializer => true;

    public override async Task ParticipateAsync(TheRequest request, ComposeResult<TheResponse> ongoingComposeResult)
    {
        await Task.CompletedTask;

        if (
            ongoingComposeResult.Response.ValueSetByStructureInitializer != Guid.Empty ||
            ongoingComposeResult.Response.ValueForSubsequentButSetByInitializer != Guid.Empty ||
            ongoingComposeResult.Response.Value2ForSubsequentButSetByInitializer != Guid.Empty ||
            ongoingComposeResult.StructureInitialized
        )
        {
            throw new Exception("Values already set");
        }

        ongoingComposeResult.Response.ValueSetByStructureInitializer = request.ValueForStructureInitializer;
        ongoingComposeResult.Response.ValueForSubsequentButSetByInitializer = request.ValueForSubsequentButSetByInitializer;
        ongoingComposeResult.Response.Value2ForSubsequentButSetByInitializer = request.Value2ForSubsequentButSetByInitializer;

        ongoingComposeResult.StructureInitialized = true;

        if (request.InitializerShouldThrow)
        {
            throw new Exception(request.InitializerErrorMessage);
        }
    }

    public override async Task RollbackAsync(TheRequest request, ComposeResult<TheResponse> ongoingComposeResult)
    {
        await Task.CompletedTask;

        ongoingComposeResult.Response.InitializerRollbackValue = request.InitializerRollbackValue;
    }
}

public class InitializerParticipator2 : RequestParticipator<TheRequest, TheResponse>
{
    public override bool IsStructureInitializer => true;

    public override async Task ParticipateAsync(TheRequest request, ComposeResult<TheResponse> ongoingComposeResult) => await Task.CompletedTask;
}

public class SubsequentParticipator1 : RequestParticipator<TheRequest, TheResponse>
{
    public override bool Ready(ComposeResult<TheResponse> ongoingComposeResult) => ongoingComposeResult.StructureInitialized;

    public override async Task ParticipateAsync(TheRequest request, ComposeResult<TheResponse> ongoingComposeResult)
    {
        await Task.CompletedTask;

        if (ongoingComposeResult.Response.SubsequentValue != Guid.Empty)
        {
            throw new Exception("Values already set");
        }

        ongoingComposeResult.Response.SubsequentValue = ongoingComposeResult.Response.ValueForSubsequentButSetByInitializer;
    }

    public override async Task RollbackAsync(TheRequest request, ComposeResult<TheResponse> ongoingComposeResult)
    {
        await Task.CompletedTask;

        ongoingComposeResult.Response.SubsequentRollbackValue = request.SubsequentRollbackValue;
    }
}

public class SubsequentParticipator2 : RequestParticipator<TheRequest, TheResponse>
{
    public override bool Ready(ComposeResult<TheResponse> ongoingComposeResult) => ongoingComposeResult.StructureInitialized;

    public override async Task ParticipateAsync(TheRequest request, ComposeResult<TheResponse> ongoingComposeResult)
    {
        await Task.CompletedTask;

        if (ongoingComposeResult.Response.SubsequentValue2 != Guid.Empty)
        {
            throw new Exception("Values already set");
        }

        ongoingComposeResult.Response.SubsequentValue2 = ongoingComposeResult.Response.Value2ForSubsequentButSetByInitializer;

        if (request.Subsequent2ShouldError)
        {
            throw new Exception(request.Subsequent2Error);
        }
    }

    public override async Task RollbackAsync(TheRequest request, ComposeResult<TheResponse> ongoingComposeResult)
    {
        await Task.CompletedTask;

        ongoingComposeResult.Response.SubsequentRollbackValue2 = request.SubsequentRollbackValue2;
    }
}

public class NonSubsequentParticipator1 : RequestParticipator<TheRequest, TheResponse>
{
    public override async Task ParticipateAsync(TheRequest request, ComposeResult<TheResponse> ongoingComposeResult)
    {
        await Task.CompletedTask;

        if (ongoingComposeResult.Response.ValueForParticipator1 != Guid.Empty)
        {
            throw new Exception("Values already set");
        }

        ongoingComposeResult.Response.ValueForParticipator1 = request.ValueForParticipator1;
    }
}

public class NonSubsequentParticipator2 : RequestParticipator<TheRequest, TheResponse>
{
    public override async Task ParticipateAsync(TheRequest request, ComposeResult<TheResponse> ongoingComposeResult)
    {
        await Task.CompletedTask;

        if (ongoingComposeResult.Response.ValueForParticipator2 != Guid.Empty)
        {
            throw new Exception("Values already set");
        }

        ongoingComposeResult.Response.ValueForParticipator2 = request.ValueForParticipator2;
    }
}

public class TestLogger : ILogger<ServiceComposer>
{
    private readonly List<(LogLevel LogLevel, string FinalMessage, Exception? Exception)> logs = [];

    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
    {
        var finalMessage = formatter.Invoke(state, exception);

        logs.Add((logLevel, finalMessage, exception));
    }

    public bool IsEnabled(LogLevel logLevel) => true;

    public IDisposable? BeginScope<TState>(TState state)
        where TState : notnull => null;

    public bool LoggedException<TException>(string exceptionMessage, string template, params object[] args)
        where TException : Exception
    {
        var foundByException = logs.Where(x => 
            x.Exception != null && 
            x.Exception.GetType() == typeof(TException) &&
            x.Exception.Message == exceptionMessage
        ).ToArray();

        if (foundByException.Length != 1)
        {
            return false;
        }

        var log = foundByException.Single();

        var expectedFinalMessage = FormatLogMessage(template, args);

        return log.FinalMessage == expectedFinalMessage && log.LogLevel == LogLevel.Error;
    }

    private static string FormatLogMessage(string template, params object[] args)
    {
#pragma warning disable SYSLIB1045
        var regex = new Regex(@"\{(\w+)\}");
#pragma warning restore SYSLIB1045
        var matches = regex.Matches(template);

        if (matches.Count != args.Length)
        {
            throw new ArgumentException("The number of placeholders does not match the number of arguments.");
        }

        for (var i = 0; i < matches.Count; i++)
        {
            template = template.Replace(matches[i].Value, args[i].ToString());
        }

        return template;
    }
}