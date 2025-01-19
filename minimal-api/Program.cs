using minimal_api.Domain;
using minimal_api.Domain.Interfaces;
using minimal_api.Helpers;
using minimal_api.Infrastructure;
using Asp.Versioning;
using Asp.Versioning.Conventions;
using Microsoft.EntityFrameworkCore;

namespace minimal_api
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddDbContext<CollisionDbContext>(opt => opt.UseInMemoryDatabase("ColisionDb"));
            builder.Services.AddDatabaseDeveloperPageExceptionFilter();
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            //create a service to handle all logic related to exercise
            builder.Services.AddScoped<ICollisionService, CollisionService>();
            //create a simple helper service to populate dummy data
            builder.Services.AddSingleton<DummyDataHelper>();

            builder.Services.AddApiVersioning(options =>
            {
                options.DefaultApiVersion = new ApiVersion(1, 0);
                options.ReportApiVersions = true;
                options.AssumeDefaultVersionWhenUnspecified = true;
                options.ApiVersionReader = new HeaderApiVersionReader("api-version");
            });

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();


            //using new Minimal API as its very basic and ideal for simple API microservices as per Microsoft!
            var versionSet = app.NewApiVersionSet()
                    .HasApiVersion(1.0)
                    .ReportApiVersions()
                    .Build();

            //Simple minimal API to get all collision for the operator_id (please use only this to see values in InmemoryDb for that operator),
            //This would not be in production code used only to compare with returns case needed
            app.MapGet("/collisionsforoperator", async (string operator_id_invoker, ICollisionService collisionService) =>
            {
                var list = await collisionService.GetCollisionsForOperatorAsync(operator_id_invoker);
                return list.Any() ? Results.Ok(list) : Results.NoContent();
            })
            .WithName("GetCollisionsForOperator")
            .WithDescription("Return the collision of an operator (who is using the API).<br/>" +
             "Please use operator_id 001, 002 and 003 of the dummy data if no new data inserted.")
            .WithSummary("NOTE: THIS IS A HELPER API JUST TO CHECK VALUES IN MEMORYDB NOT INTENDED FOR PRODUCTION")
            .WithOpenApi()
            .WithApiVersionSet(versionSet)
            .MapToApiVersion(1.0); 

            //Simple minimal API to get all collision alerts for the operator_id
            app.MapGet("/collisionalerts", async (string operator_id_invoker, ICollisionService collisionService) =>
            {
                var list = await collisionService.GetColisionsWarningsByOperatorIdAsync(operator_id_invoker);
                return list.Any() ? Results.Ok(list) : Results.NoContent();
            })
            .WithName("GetCollisionAlerts")
            .WithDescription("Return the collision status of warning for all the satellites of an operator (who is using the API).<br/>" +
             "Please use operator_id 001, 002 and 003 of the dummy data if no new data inserted")
            .WithOpenApi()
            .WithApiVersionSet(versionSet)
            .MapToApiVersion(1.0);

            //Simple minimal API to POST a collision record
            app.MapPost("/collision", async (string operator_id_invoker, CollisionDto collisionDto, ICollisionService collisionService) =>
            {
                var dbId = await collisionService.SaveCollisionAsync(operator_id_invoker, collisionDto);
                return dbId != Guid.Empty ? Results.Created($"/colision/{dbId}", dbId) : Results.UnprocessableEntity();
            })
            .WithName("PostCollision")
            .WithDescription("Insert new collision data, please notice the mandatory fields.<br/>")
            .WithOpenApi()
            .WithApiVersionSet(versionSet)
            .MapToApiVersion(1.0);

            //Simple minimal API to PATCH a collision record with IsCanceled
            app.MapPatch("/collisioncancel", async (string operator_id_invoker, CollisionDto collisionDto, ICollisionService collisionService) =>
            {
                var dbId = await collisionService.CancelCollisionAsync(operator_id_invoker, collisionDto);
                return dbId != Guid.Empty ? Results.Accepted($"/collisioncancel/{dbId}", dbId) : Results.UnprocessableEntity();
            })
            .WithName("CancelCollision")
            .WithDescription("Cancels a collision data, please notice the mandatory fields.<br/>")
            .WithOpenApi()
            .WithApiVersionSet(versionSet)
            .MapToApiVersion(1.0);

            //just for the sake of simplicity generate static data for demo on app run
            var dummyData = app.Services.GetRequiredService<DummyDataHelper>();
            Task.FromResult(dummyData.GenerateData());

            app.Run();

        }
    }
}

