using System.Reflection;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddSwaggerGen(c =>
    {
        //TODO: Replace empty strings.
        // c.SwaggerDoc("v1", new OpenApiInfo
        // {
        //     Version = "v1",
        //     Title = "minitwit API",
        //     Description = "The API for the minitwit backend application.",
        //     TermsOfService = new Uri(string.Empty),
        //     Contact = new OpenApiContact
        //     {
        //         Name = string.Empty,
        //         Email = string.Empty,
        //         Url = new Uri(string.Empty),
        //     },
        //     License = new OpenApiLicense
        //     {
        //         Name = string.Empty,
        //         Url = new Uri(string.Empty),
        //     }
        // });

        // Set the comments path for the Swagger JSON and UI.
        var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
        var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
        c.IncludeXmlComments(xmlPath);
    });

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
} else {
    app.UseDeveloperExceptionPage();
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
