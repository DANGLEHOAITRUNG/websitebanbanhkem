using CakeShop_BE.Data;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddDbContext<CakeShopContext>(options =>
	options.UseSqlServer(builder.Configuration.GetConnectionString("CakeShop")));
builder.Services.AddCors(options =>
{
	options.AddPolicy("AllowAll",
		policy =>
		{
			policy.WithOrigins("http://localhost:4400").
			 AllowAnyOrigin()
				  .AllowAnyHeader()
				  .AllowAnyMethod();
		});
});

// JSON Serializer
builder.Services.AddControllersWithViews().AddNewtonsoftJson(option => option.SerializerSettings.ReferenceLoopHandling = Newtonsoft
.Json.ReferenceLoopHandling.Ignore)
	.AddNewtonsoftJson(option =>
	option.SerializerSettings.ContractResolver = new DefaultContractResolver());

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
	app.UseSwagger();
	app.UseSwaggerUI();
}
app.UseCors("AllowAll");
app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseAuthorization();

app.MapControllers();

app.Run();
