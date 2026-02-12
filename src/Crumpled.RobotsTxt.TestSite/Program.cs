using Crumpled.RobotsTxt;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.CreateUmbracoBuilder()
    .AddBackOffice()
    .AddWebsite()
    .AddDeliveryApi()
    .AddComposers()
  //  .AddCrumpledRobotsTxt()
    .Build();


WebApplication app = builder.Build();

await app.BootUmbracoAsync();

app.UseUmbraco()
    .WithMiddleware(u =>
    {
        u.UseBackOffice();
        u.UseWebsite();
    })
    .WithEndpoints(u =>
    {
	    u.UseBackOfficeEndpoints();
	    u.UseWebsiteEndpoints();
	});

await app.RunAsync();

// Make Program class accessible to test projects
public partial class Program { }
