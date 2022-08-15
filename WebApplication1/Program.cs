using MassTransit;
using WebApplication1;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseDefaultServiceProvider(o =>
{
    o.ValidateOnBuild = true;
    o.ValidateScopes = true;
});
// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddMediator(cfg =>
{

    cfg.AddConsumers(typeof(Consumer21).Assembly);
    cfg.AddRequestClient<Input1>();
    cfg.AddRequestClient<Input2>();
});

builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<IScopedSample1, ScopedSample1>();
builder.Services.AddScoped<IScopedSample21, ScopedSample21>();
builder.Services.AddScoped<IScopedSample22, ScopedSample22>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();