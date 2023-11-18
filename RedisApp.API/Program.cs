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

//Uygulamanýn herhangi bir yerinde IProductRepository ile karþýlaþýrsak ProductRepsitoryWithCacheDecorator den nesne verecek.
// ProductRepsitoryWithCacheDecorator (bu sýnýf) içerisinde karþýlaþýrsa ProductRepository'den nesne örneði vermesi gerekli.
builder.Services.AddScoped<IProductRepository>(sp=>
{

    var appDbContext = sp.GetRequiredService<AppDbContext>();

    var productRepository = new ProductRepository(appDbContext);

    var redisService = sp.GetRequiredService<RedisService>();


    return new ProductRepsitoryWithCacheDecorator(productRepository, redisService);

    // ProductController IProductRepository interface ile karþýlaþtýðý zaman ProductRepsitoryWithCacheDecorator veriyorum. Artýk datalar cache'den gelecek.
}
);


//DB kurulumu
builder.Services.AddDbContext<AppDbContext>(options =>
{
    options.UseInMemoryDatabase("myDatabase"); 
    // Memory'e kaydediyoruz bu yüzden migration yapmamýza gerek yok. Aç kapa yaparsak verilerimiz hafýzada olacaðý için silinir.
});


// CacheOption:Url --> aldýk bunu ekledik.
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



// InMemory database'si için uygulanýr sadece diðer Databaseler için böyle bir þey uygulanmaz. 
// Bunu yapma sebebimiz: Bunun içerisine yazdýðýmýz kodlar scopeler bittikten sonra memoryden düþsün.
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
