using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using System.Reflection;
using System.Text;
using Microsoft.AspNetCore.Mvc;

namespace HandUp.AspNet;

public static class WebApplicationExtensions
{
    public static WebApplication MapHandUpConfigurationUi(this WebApplication app)
    {
        app.MapGet("hand-up/configuration", Handler);

        return app;
    }

    [AllowAnonymous]
    private static async Task<IResult> Handler([FromServices] HandUpConfiguration configuration)
    {
        var assembly = Assembly.GetAssembly(typeof(WebApplicationExtensions));

        if (assembly == null)
        {
            return Results.NotFound();
        }

        await using var stream = assembly.GetManifestResourceStream($"{typeof(WebApplicationExtensions).Namespace}.config-ui.html");

        if (stream == null)
        {
            return Results.NotFound();
        }

        using var reader = new StreamReader(stream);

        var html = await reader
                       .ReadToEndAsync()
                       .ConfigureAwait(false);

        html = ReplaceHtml(html, configuration);

        return Results.Text(html, contentType: "text/html", Encoding.UTF8);
    }

    private static string ReplaceHtml(string html, HandUpConfiguration configuration)
    {
        var sb = new StringBuilder();

        foreach (var implementorSet in configuration.ImplementorSets.OrderBy(x => x.Key.ToString()))
        {
            var request = implementorSet.Key.GenericTypeArguments[0];
            var response = implementorSet.Key.GenericTypeArguments[1];

            sb.Append("<tr><td>");
            sb.Append(request);
            sb.Append("</td><td>");
            sb.Append(response);
            sb.Append("</td><td>");

            if (implementorSet.Value.Count < 1)
            {
                sb.Append("<em>NO IMPLEMENTATIONS</em></td>");
            }
            else
            {
                sb.Append("<ul>");

                foreach (var type in implementorSet.Value.OrderBy(x => x.ToString()))
                {
                    sb.Append("<li>");
                    sb.Append(type);
                    sb.Append("</li>");
                }

                sb.Append("</ul></td>");
            }

            sb.Append("</tr>");
        }

        return html.Replace("{ROWS}", sb.ToString());
    }
}