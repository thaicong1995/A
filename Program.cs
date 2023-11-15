using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;
using WebApi.Controllers.View;
using WebApi.MyDbContext;
using WebApi.Sevice.Interface;
using WebApi.Sevice.Service;
using WebApi.TokenConfig;
using WebApi.VNpay;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddJsonFile("appsettings.json");

// Đọc cấu hình từ builder.Configuration
builder.Services.AddDbContext<MyDb>(x => x.UseSqlServer(builder.Configuration.GetConnectionString("DB")));
// Add services to the container.
builder.Configuration.AddJsonFile("appsettings.json");
builder.Services.AddScoped<IUserService,UserService>();
builder.Services.AddScoped<IProductService,ProductService>();
builder.Services.AddScoped<ICartItemService,CartItemService>();
builder.Services.AddScoped<IOrderService,OrderService>();
builder.Services.AddScoped<IWalletService,WalletService>();
builder.Services.AddScoped<IReveneuService,RevenueService>();
builder.Services.AddScoped<IDiscountService,DiscountService>();
builder.Services.AddScoped<IShopService,ShopService>();
builder.Services.AddScoped<Token>();
builder.Services.AddSingleton<BackGroundService>();
builder.Services.AddTransient<EmailService>();
builder.Services.AddScoped<ResetService>();
builder.Services.AddTransient<ResetService>();
builder.Services.AddScoped<VnPayLibrary>();
builder.Services.AddScoped<VnPayService>();
builder.Services.AddControllers();
builder.Services.AddControllersWithViews();
builder.Services.Configure<RazorViewEngineOptions>(options =>
{
    options.ViewLocationFormats.Add("~/Views/{0}.cshtml");
});
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(option =>
{
    option.SwaggerDoc("v1", new OpenApiInfo { Title = "Demo API", Version = "v1" });
    option.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        In = ParameterLocation.Header,
        Description = "Please enter a valid token",
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        BearerFormat = "JWT",
        Scheme = "Bearer"
    });
    option.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type=ReferenceType.SecurityScheme,
                    Id="Bearer"
                }
            },
            new string[]{}
        }
    });
});


//Token Expire Time
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"],   // Lấy giá trị Issuer từ cấu hình
        ValidAudience = builder.Configuration["Jwt:Audience"],
        ValidateIssuerSigningKey = true,
        // Replace "Jwt:Token512" with the correct configuration key for your token
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Token512"]))
    };
})
.AddCookie(CookieAuthenticationDefaults.AuthenticationScheme, options =>
{
    options.Cookie.Name = "authenticationToken"; // Tên của cookie
    options.Cookie.HttpOnly = true;
    options.LoginPath = "/account/login"; // Đường dẫn đến trang đăng nhập
});

//.
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowSpecificOrigin", builder =>
    {
        builder.WithOrigins("https://localhost:7212")
            .AllowAnyHeader()
            .WithMethods("POST"); // Thêm AllowMethods("POST") để cho phép POST requests
    });
});


var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var serviceProvider = scope.ServiceProvider;

    // Truy cập dịch vụ TokenExpirationService trong scope này
    var tokenExpirationService = serviceProvider.GetRequiredService<BackGroundService>();

    // Khởi động dịch vụ TokenExpirationService
    tokenExpirationService.StartAsync(CancellationToken.None).Wait();
}


// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "abc");

    });
}
app.UseRouting();
app.UseCors("AllowSpecificOrigin");
app.UseAuthentication();
app.UseAuthorization(); // Đảm bảo đặt UseAuthorization ở đây

app.UseEndpoints(endpoints =>
{
    endpoints.MapControllerRoute(
        name: "default",
        pattern: "{controller=Home}/{action=Index}/{id?}");

    endpoints.MapControllerRoute(
        name: "UpdatePassword",
        pattern: "api/ResetPasswordForEmail/UpdatePassword",
        defaults: new { controller = "ResetPasswordForEmail", action = "Reset-Password" }
    );
});

app.UseHttpsRedirection();

app.UseAuthentication();

app.MapControllers();

app.Run();
