using HandUp.ServiceComposition;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;

namespace HandUp.UnitTests.ServiceComposition;

public class ServiceComposerTests
{
    private readonly ILogger<ServiceComposer> logger;
    private readonly HandUpConfiguration configuration;
    private readonly TheRequest request;
    private readonly TheResponse response;
    
    public ServiceComposerTests()
    {
        logger = Substitute.For<ILogger<ServiceComposer>>();
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
            .AddScoped<RequestParticipator<TheRequest, TheResponse>, InitializerParticipator1>();

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
    public async Task ComposeAsync_WhenThereIsASingleStructureInitializer_AndAnotherTwoInitializers_ShouldRunTheInitialParticipatorFirst_ThenTheOthers_AndReturnResult()
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
}

public record TheRequest
{
    public Guid ValueForStructureInitializer { get; set; }
    public Guid ValueForSubsequentButSetByInitializer { get; set; }
    public Guid Value2ForSubsequentButSetByInitializer { get; set; }
}

public record TheResponse
{
    public Guid ValueSetByStructureInitializer { get; set; }
    public Guid ValueForSubsequentButSetByInitializer { get; set; }
    public Guid Value2ForSubsequentButSetByInitializer { get; set; }
    public Guid SubsequentValue { get; set; }
    public Guid SubsequentValue2 { get; set; }
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
    }
}