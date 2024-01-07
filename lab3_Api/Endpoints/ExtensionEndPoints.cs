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

        public static IEndpointRouteBuilder PersonApiExtensions(
            this IEndpointRouteBuilder endPointBuilder)
        {

            var grpEndPointBuilder = endPointBuilder.MapGroup("api/person");


            endPointBuilder.MapGet("/", GetAllPersonsAsync)
                .WithTags("Get")
                .WithOpenApi(op =>
                {
                    op.Summary = "Get all persons";
                    return op;
                });


            endPointBuilder.MapGet("/{id:int}/interests", GetAllInterestForAPersonByIdAsync)
                .WithName("GetInterestByPersonId")
                .WithTags("Get")
                .WithOpenApi(op =>
                {
                    op.Summary = "Get interests for a person";
                    op.Description = "Get the interests for specific person with the person id";
                    op.Parameters[0].Description = "The id of the person";

                    return op;
                });


            endPointBuilder.MapGet("/{id:int}/all-info", GetAllPersonInfoAsync)
                .WithName("GetInfoPerson")
                .WithTags("Get")
                .WithOpenApi(op =>
                {
                    op.Summary = "Get all info about person";
                    op.Description = "Get all info for a specific person with the person id";
                    op.Parameters[0].Description = "The id of the person";

                    return op;
                });


            endPointBuilder.MapGet("/{id:int}/links", GetAllLinkForAPersonByIdAsync)
                .WithName("GetLinkByPersonId")
                .WithTags("Get")
                .WithOpenApi(op =>
                {
                    op.Summary = "Get links for a person";
                    op.Description = "Get the links for specific person with the person id";
                    op.Parameters[0].Description = "The id of the person";

                    return op;
                });


            endPointBuilder.MapPost("/{id:int}/add-interest", AddInterestToAPersonAsync)
               .WithName("AddInterestToAPerson")
               .WithTags("Post")
               .WithOpenApi(op =>
                {
                    op.Summary = "Add interest to an person";
                    op.Parameters[0].Description = "The id of the person";

                    return op;
                })
                .Produces<Interest>(StatusCodes.Status201Created);


            endPointBuilder.MapPost("/{personId:int}/interest/{interestId:int}/add-link", AddLinkToAInterestAsync)
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




        // I want to move this code out and clean all this up, but i don't have time..
        // I also should check for exceptions with try catch and give back better response when some 500 error happens to the user that uses my api
        // im plan to do this with controllers for more practice in the future

        // all methods for the endpoints
        private static async Task<Results<Ok<List<PersonViewModel>>, NotFound>> GetAllPersonsAsync(PersonDbContext db)
        {

            var personList = await db.Persons.ToListAsync();

            if (personList.Count == 0)
            {
                return TypedResults.NotFound();
            }

            var persons = CreatePersonViewModel(personList);

            return persons.Count > 0 ?
            TypedResults.Ok(persons) : TypedResults.NotFound();

        }


        private static async Task<Results<Ok<List<InterestVewModel>>, NotFound>> GetAllInterestForAPersonByIdAsync(PersonDbContext db, int id)
        {

            var interestPersonList = await db.InterestRelation
            .Where(i => i.PersonId == id)
            .Select(i => i.Interest)
            .ToListAsync();

            if (interestPersonList.Count == 0)
            {
                return TypedResults.NotFound();
            }

            var interests = CreateInterestViewModel(interestPersonList);

            return interests.Count > 0 ?
            TypedResults.Ok(interests) : TypedResults.NotFound();
        }


        private static async Task<Results<Ok<PersonAllInfoViewModel>, NotFound>> GetAllPersonInfoAsync(PersonDbContext db, int id)
        {

            var person = await db.Persons
            .Include(p => p.InterestRelation)!
            .ThenInclude(i => i.Interest)
            .ThenInclude(l => l.Links)
            .FirstOrDefaultAsync(P => P.Id == id);

            if (person == null)
            {
                return TypedResults.NotFound();
            }

            var result = CreatePersonAllViewModel(person);

            return TypedResults.Ok(result);
        }


        private static async Task<Results<Ok<List<LinkViewModel>>, NotFound>> GetAllLinkForAPersonByIdAsync(PersonDbContext db, int id)
        {
            var linkPersonList = await db.InterestRelation
                   .Where(i => i.PersonId == id)
                   .SelectMany(i => i.Interest.Links)
                   .ToListAsync();

            if (linkPersonList.Count == 0)
            {
                return TypedResults.NotFound();
            }

            var links = CreateLinkViewModel(linkPersonList);

            return links.Count > 0 ?
            TypedResults.Ok(links) : TypedResults.NotFound();
        }


        private static async Task<Results<UnprocessableEntity, CreatedAtRoute<InterestsDto>, NotFound>> AddInterestToAPersonAsync
                        (PersonDbContext db, int id, [FromBody] InterestsDto interest)
        {

            var person = await db.Persons.FirstOrDefaultAsync(i => i.Id == id);

            if (person == null)
            {
                return TypedResults.NotFound();
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
              routeValues: new
              {
                  personId = person.Id,
                  interestId = addInterest.Id
              },
              value: new InterestsDto
              {
                  Title = interest.Title,
                  Description = interest.Description
              });
        }


        private static async Task<Results<UnprocessableEntity, CreatedAtRoute<LinkDto>, NotFound>> AddLinkToAInterestAsync

                        (PersonDbContext db, int personId, int interestId, [FromBody] LinkDto linkDto)
        {

            if (!ValidateUrl(linkDto.Url))
            {
                return TypedResults.UnprocessableEntity();
            }


            var person = await db.Persons
                .Include(p => p.InterestRelation)!
                .ThenInclude(ir => ir.Interest)
                .ThenInclude(i => i.Links)
                .FirstOrDefaultAsync(p => p.Id == personId);

            if (person == null)
            {
                return TypedResults.NotFound();
            }

            var interestRelation = person.InterestRelation?
                .FirstOrDefault(ir => ir.InterestId == interestId);

            if (interestRelation == null || interestRelation.Interest == null)
            {
                return TypedResults.NotFound();
            }

            var interest = interestRelation.Interest;

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
                value: new LinkDto { Url = newLink.Url });
        }




        // methods for creating the view models
        private static List<LinkViewModel> CreateLinkViewModel(List<Link> result)
        {
            return result.Select(i => new LinkViewModel
            {
                Id = i.Id,
                Url = i.Url,
            })
            .ToList();
        }


        private static List<InterestVewModel> CreateInterestViewModel(List<Interest> result)
        {
            return result.Select(i => new InterestVewModel
            {
                Id = i.Id,
                Title = i.Title,
                Description = i.Description,
            })
            .ToList();
        }


        private static List<PersonViewModel> CreatePersonViewModel(List<Person> result)
        {
            return result.Select(p => new PersonViewModel
            {
                Id = p.Id,
                FirstName = p.FirstName,
                LastName = p.LastName,
                PhoneNumber = p.PhoneNumber
            })
            .ToList();
        }


        private static PersonAllInfoViewModel CreatePersonAllViewModel(Person person)
        {
            return new PersonAllInfoViewModel
            {
                Id = person.Id,
                FirstName = person.FirstName,
                LastName = person.LastName,
                PhoneNumber = person.PhoneNumber,
                Interests = person.InterestRelation!
                .Select(i => new InterestVewModel
                {
                    Id = i.Interest.Id,
                    Title = i.Interest.Title,
                    Description = i.Interest.Description,
                })
                .ToList(),
                Links = person.InterestRelation?
                .SelectMany(il => il.Interest.Links)
                .Select(l => new LinkViewModel
                {
                    Id = l.Id,
                    Url = l.Url,
                })
                .ToList()
            };
        }



        private static bool ValidateUrl(string url)
        {
            if (url == null)
            {
                return false;
            }

            return Uri.TryCreate(url, UriKind.Absolute, out Uri? uriResult) &&
                (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);
        }


    }
}
