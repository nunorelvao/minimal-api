using minimal_api.Domain;
using minimal_api.Domain.Interfaces;
using minimal_api.Helpers;
using minimal_api.Infrastructure;
using Asp.Versioning;
using Asp.Versioning.Conventions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace minimal_api
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddDbContext<CollisionDbContext>(opt => opt.UseInMemoryDatabase("CollisionsDb"));
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

            //Simple minimal API to get all collision for the operator_id (please use only this to see values in InMemoryDb for that operator),
            //This would not be in production code used only to compare with returns case needed
            app.MapGet("/collisions/{operatorid}", async ([FromRoute]string operatorid, ICollisionService collisionService, CancellationToken ct = default) =>
            {
                var list = await collisionService.GetCollisionsForOperatorAsync(operatorid, ct);
                if (ct.IsCancellationRequested)
                {
                    return Results.StatusCode(499); // Client closed the request
                }
                return list.Any() ? Results.Ok(list) : Results.NoContent();
            })
            .WithName("GetCollisionsForOperator")
            .WithDescription("Return the collisions of an operator.<br/>" +
             "Please use operator_id 001, 002 and 003 of the dummy data if no new data inserted.")
            .WithSummary("NOTE: THIS IS A HELPER API JUST TO CHECK VALUES IN MEMORY DB NOT INTENDED FOR PRODUCTION")
            .WithOpenApi()
            .WithApiVersionSet(versionSet)
            .MapToApiVersion(1.0); 
            
            //Simple minimal API to get collision by id
            app.MapGet("/collision/{id}", async ([FromRoute]Guid id, ICollisionService collisionService,  CancellationToken ct = default) =>
                {
                    var collision = await collisionService.GetCollisionByIdAsync(id, ct);
                    if (ct.IsCancellationRequested)
                    {
                        return Results.StatusCode(499); // Client closed the request
                    }
                    return collision != null ? Results.Ok(collision) : Results.NoContent();
                })
                .WithName("GetCollisionById")
                .WithDescription("Return the collision by Id.")
                .WithOpenApi()
                .WithApiVersionSet(versionSet)
                .MapToApiVersion(1.0);

            //Simple minimal API to get all collision alerts for the operator_id
            app.MapGet("/collisions/alerts/{operatorid}", async ([FromRoute]string operatorid, ICollisionService collisionService,  CancellationToken ct = default) =>
            {
                var list = await collisionService.GetCollisionsWarningsByOperatorIdAsync(operatorid, ct);
                if (ct.IsCancellationRequested)
                {
                    return Results.StatusCode(499); // Client closed the request
                }
                return list.Any() ? Results.Ok(list) : Results.NoContent();
            })
            .WithName("GetCollisionsAlerts")
            .WithDescription("Return the collision status of warning for all the satellites of an operator.<br/>" +
             "Please use operatorid 001, 002 and 003 of the dummy data if no new data inserted")
            .WithOpenApi()
            .WithApiVersionSet(versionSet)
            .MapToApiVersion(1.0);

            //Simple minimal API to POST a collision record
            app.MapPost("/collision/{operatorid}", async ([FromRoute]string operatorid, [FromBody]CollisionDto collisionDto, ICollisionService collisionService, CancellationToken ct = default) =>
            {
                var (success, result) = await collisionService.SaveCollisionAsync(operatorid, collisionDto, ct);
                return  success ? Results.Created($"/collision/{result}", result) : Results.UnprocessableEntity(result);
            })
            .WithName("PostCollision")
            .WithDescription("Insert new collision data, please notice the mandatory fields.<br/>")
            .WithOpenApi()
            .WithApiVersionSet(versionSet)
            .MapToApiVersion(1.0);
            
            //Simple minimal API to PATCH a collision record with IsCanceled
            app.MapPatch("/collision/{operatorid}", async ([FromRoute]string operatorid, [FromBody]CollisionDto collisionDto, ICollisionService collisionService,  CancellationToken ct = default) =>
            {
                var (success, result) = await collisionService.CancelCollisionAsync(operatorid, collisionDto, ct);
                if (ct.IsCancellationRequested)
                {
                    return Results.StatusCode(499); // Client closed the request
                }
                return  success ? Results.Accepted($"/collision/{result}", result) : Results.UnprocessableEntity(result);
            })
            .WithName("PatchCollision")
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

