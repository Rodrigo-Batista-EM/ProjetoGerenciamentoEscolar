using System.Globalization;
using EM.Repository.Banco;
using Microsoft.AspNetCore.Localization;
using EM.Web.Interfaces;
using EM.Web.Services;
using EM.Repository;

var builder = WebApplication.CreateBuilder(args);

// iText 8 requer especificar o adapter de Bouncy Castle
Environment.SetEnvironmentVariable("ITEXT_BOUNCY_CASTLE_FACTORY_NAME", "bouncy-castle");

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();
builder.Services.AddScoped<IRelatorioService, RelatorioService>();
builder.Services.AddScoped<RepositorioAluno>();
builder.Services.AddScoped<RepositorioCidade>();

// Configure database connection
DBHelper.Configure(builder.Configuration.GetConnectionString("FirebirdConnection"));

// Configure localization
builder.Services.Configure<RequestLocalizationOptions>(options =>
{
    var supportedCultures = new[] { new CultureInfo("pt-BR") };
    options.DefaultRequestCulture = new RequestCulture("pt-BR");
    options.SupportedCultures = supportedCultures;
    options.SupportedUICultures = supportedCultures;
});

// Configure session
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// Configure MVC
builder.Services.AddMvc(options =>
{
    options.ModelBindingMessageProvider.SetValueMustNotBeNullAccessor(
        _ => "O campo {0} é obrigatório.");
    options.ModelBindingMessageProvider.SetAttemptedValueIsInvalidAccessor(
        (value, field) => $"O valor '{value}' não é válido para o campo {field}.");
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// Configure localization
app.UseRequestLocalization();

app.UseAuthorization();
app.UseSession();

// Configure routing
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapRazorPages();

app.Run();
