using lab3_Api.Data;
using lab3_Api.Endpoints;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);


builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var connectionString = builder.Configuration.GetConnectionString("personDb");
builder.Services.AddDbContext<PersonDbContext>(opts =>
        opts.UseSqlServer(connectionString));


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();


app.MapGet("api/persons", PersonEndpoints.Get);
app.MapGet("api/person/{id:int}", PersonEndpoints.GetById);
app.MapGet("api/person/{id:int}/interests", PersonEndpoints.GetInterestsForAPerson);
app.MapGet("api/person/{id:int}/links", PersonEndpoints.GetLinksForAPerson);









app.Run();

