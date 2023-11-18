using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using RedisApp.API.Models;
using RedisApp.API.Repository;
using RedisApp.API.Services;
using RedisExampleApp.Cache;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddScoped<IProductService, ProductService>();

//Uygulaman�n herhangi bir yerinde IProductRepository ile kar��la��rsak ProductRepsitoryWithCacheDecorator den nesne verecek.
// ProductRepsitoryWithCacheDecorator (bu s�n�f) i�erisinde kar��la��rsa ProductRepository'den nesne �rne�i vermesi gerekli.
builder.Services.AddScoped<IProductRepository>(sp=>
{

    var appDbContext = sp.GetRequiredService<AppDbContext>();

    var productRepository = new ProductRepository(appDbContext);

    var redisService = sp.GetRequiredService<RedisService>();


    return new ProductRepsitoryWithCacheDecorator(productRepository, redisService);

    // ProductController IProductRepository interface ile kar��la�t��� zaman ProductRepsitoryWithCacheDecorator veriyorum. Art�k datalar cache'den gelecek.
}
);


//DB kurulumu
builder.Services.AddDbContext<AppDbContext>(options =>
{
    options.UseInMemoryDatabase("myDatabase"); 
    // Memory'e kaydediyoruz bu y�zden migration yapmam�za gerek yok. A� kapa yaparsak verilerimiz haf�zada olaca�� i�in silinir.
});


// CacheOption:Url --> ald�k bunu ekledik.
builder.Services.AddSingleton<RedisService>(sp =>
{
    return new RedisService(builder.Configuration["CacheOption:Url"]);
}
    );

builder.Services.AddSingleton<IDatabase>(sp =>
{

    var redisService = sp.GetRequiredService<RedisService>();

    return (IDatabase)redisService.GetDb(0);

});


var app = builder.Build();



// InMemory database'si i�in uygulan�r sadece di�er Databaseler i�in b�yle bir �ey uygulanmaz. 
// Bunu yapma sebebimiz: Bunun i�erisine yazd���m�z kodlar scopeler bittikten sonra memoryden d��s�n.
using(var scope = app.Services.CreateScope())
{

    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

    dbContext.Database.EnsureCreated();

}


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
