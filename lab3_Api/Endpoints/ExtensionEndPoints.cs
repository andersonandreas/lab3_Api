using lab3_Api.Data;
using lab3_Api.Models.ViewModels;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;

namespace lab3_Api.Endpoints
{
    public static class ExtensionEndPoints
    {

        public static IEndpointRouteBuilder PersonApiExtensions(
            this IEndpointRouteBuilder endPointBuilder)
        {
            var grpEndPointBuilder = endPointBuilder.MapGroup("api/person");



            endPointBuilder.MapGet("/",
                async Task<Results<Ok<List<PersonViewModel>>, NotFound>> (PersonDbContext db) =>
            {

                var result = await db.Persons.ToListAsync();

                var persons = result.Select(p => new PersonViewModel
                {
                    Id = p.Id,
                    FirstName = p.FirstName,
                    LastName = p.LastName,
                    PhoneNumber = p.PhoneNumber
                })
                .ToList();

                return persons.Count > 0 ? TypedResults.Ok(persons) : TypedResults.NotFound();
            })

                .WithTags("Get")
                .WithOpenApi(op =>
                {
                    op.Summary = "Get all persons";
                    return op;
                });



            endPointBuilder.MapGet("/{id:int}/interests",
                async Task<Results<Ok<List<InterestVewModel>>, NotFound>> (PersonDbContext db, int id) =>
            {
                var result = await db.InterestRelation
                .Where(i => i.PersonId == id)
                .Select(i => i.Interest)
                .ToListAsync();

                var interests = result.Select(i => new InterestVewModel
                {
                    Id = i.Id,
                    Title = i.Title,
                    Description = i.Description,
                })
                .ToList();

                return interests.Count > 0 ? TypedResults.Ok(interests) : TypedResults.NotFound();
            })
               .WithName("Get a interest by person id")
               .Produces(StatusCodes.Status500InternalServerError)
               .WithTags("Get")
               .WithOpenApi(op =>
               {
                   op.Summary = "Get a interests for a person";
                   op.Description = "Get the interests for specific person with the person id";

                   op.Parameters[0].Description = "The id of the person";

                   return op;
               });


            endPointBuilder.MapGet("/{id:int}/links",
                async Task<Results<Ok<List<LinkViewModel>>, NotFound>> (PersonDbContext db, int id) =>
         {
             var result = await db.InterestRelation
                    .Where(i => i.PersonId == id)
                    .SelectMany(i => i.Interest.Links)
                    .ToListAsync();

             var interests = result.Select(i => new LinkViewModel
             {
                 Id = i.Id,
                 Url = i.Url,
             })
             .ToList();

             return interests.Count > 0 ? TypedResults.Ok(interests) : TypedResults.NotFound();
         })
        .WithName("Get links by person id")
        .Produces(StatusCodes.Status500InternalServerError)
        .WithTags("Get")
        .WithOpenApi(op =>
        {
            op.Summary = "Get a links for a person";
            op.Description = "Get the links for specific person with the person id";

            op.Parameters[0].Description = "The id of the person";

            return op;
        });





            return endPointBuilder;

        }










    }
}
