using lab3_Api.Data;
using lab3_Api.Dtos;
using lab3_Api.Models;
using lab3_Api.Models.ViewModels;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace lab3_Api.Endpoints
{
    public static class ExtensionEndPoints
    {


        private static bool ValidateUrl(string url)
        {
            return Uri.TryCreate(url, UriKind.Absolute, out Uri uriResult) &&
                   (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);
        }



        public static IEndpointRouteBuilder PersonApiExtensions(
            this IEndpointRouteBuilder endPointBuilder)
        {
            var grpEndPointBuilder = endPointBuilder.MapGroup("api/person");


            // all persons

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


            // get interest by person id

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
               .WithName("GetInterestByPersonId")
               .WithTags("Get")
               .WithOpenApi(op =>
               {
                   op.Summary = "Get interests for a person";
                   op.Description = "Get the interests for specific person with the person id";

                   op.Parameters[0].Description = "The id of the person";

                   return op;
               });




            // link by person id

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

             return interests.Count > 0 ?
             TypedResults.Ok(interests) : TypedResults.NotFound();
         })
        .WithName("GetLinkByPersonId")
        .WithTags("Get")
        .WithOpenApi(op =>
        {
            op.Summary = "Get links for a person";
            op.Description = "Get the links for specific person with the person id";

            op.Parameters[0].Description = "The id of the person";

            return op;
        });



            // add interest for a person 

            endPointBuilder.MapPost("/{id:int}/add-interest",
               async Task<Results<UnprocessableEntity, CreatedAtRoute<InterestsDto>, BadRequest>>
                        (PersonDbContext db, int id, [FromBody] InterestsDto interest) =>
               {

                   var person = await db.Persons.FirstOrDefaultAsync(i => i.Id == id);

                   if (person == null)
                   {
                       return TypedResults.BadRequest();
                   }

                   var addInterest = new Interest
                   {
                       Title = interest.Title,
                       Description = interest.Description,
                   };

                   var newRelation = new InterestRelation
                   {
                       Person = person,
                       Interest = addInterest
                   };


                   person.InterestRelation ??= [];
                   person.InterestRelation.Add(newRelation);

                   await db.SaveChangesAsync();

                   return TypedResults.CreatedAtRoute(
                     routeName: "AddInterestToAPerson",
                     routeValues: new { personId = person.Id, interestId = addInterest.Id },
                     value: new InterestsDto
                     {
                         Title = interest.Title,
                         Description = interest.Description
                     });


               })
                .WithName("AddInterestToAPerson")
                .WithTags("Post")
                  .WithOpenApi(op =>
                  {
                      op.Summary = "Add interest to an person";
                      op.Parameters[0].Description = "The id of the person";

                      return op;
                  })
                .Produces<Interest>(StatusCodes.Status201Created);





            // add link to a interest

            endPointBuilder.MapPost("/{personId:int}/interest/{interestId:int}/add-link",
                async Task<Results<UnprocessableEntity, CreatedAtRoute<LinkDto>, BadRequest>>
                        (PersonDbContext db, int personId, int interestId, [FromBody] LinkDto linkDto) =>
                {

                    if (!ValidateUrl(linkDto.Url))
                    {
                        return TypedResults.UnprocessableEntity();
                    }


                    var person = await db.Persons
                        .Include(p => p.InterestRelation)
                        .ThenInclude(ir => ir.Interest)
                        .ThenInclude(i => i.Links)
                        .FirstOrDefaultAsync(p => p.Id == personId);

                    if (person == null)
                    {
                        return TypedResults.BadRequest();
                    }

                    var interestRelation = person.InterestRelation?
                        .FirstOrDefault(ir => ir.InterestId == interestId);

                    if (interestRelation == null)
                    {
                        return TypedResults.BadRequest();
                    }

                    var interest = interestRelation.Interest;

                    if (interest == null)
                    {
                        // need to fix so i can return not found
                        return TypedResults.BadRequest();
                    }

                    var newLink = new Link
                    {
                        Url = linkDto.Url,
                        InterestId = interestId,
                    };

                    interest.Links ??= [];
                    interest.Links.Add(newLink);

                    await db.SaveChangesAsync();

                    return TypedResults.CreatedAtRoute(
                        routeName: "AddLinkToInterest",
                        routeValues: new { personId = person.Id, interestId = interest.Id },
                        value: new LinkDto
                        {
                            Url = newLink.Url
                        });
                })
                .WithName("AddLinkToInterest")
                .Produces(StatusCodes.Status201Created)
                .WithTags("Post")
                .WithOpenApi(op =>
                {
                    op.Summary = "Add link to an interest";
                    op.Parameters[0].Description = "The id of the person";
                    op.Parameters[1].Description = "The id of the interest";

                    return op;
                });






            return endPointBuilder;

        }





    }
}
