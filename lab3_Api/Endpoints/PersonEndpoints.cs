using lab3_Api.Data;
using Microsoft.EntityFrameworkCore;

namespace lab3_Api.Endpoints
{
    public static class PersonEndpoints
    {


        public static async Task<IResult> Get(PersonDbContext db)
        {
            return TypedResults.Ok(await db.Persons.ToArrayAsync());
        }


        public static async Task<IResult> GetById(PersonDbContext db, int id)
        {
            var person = await db.Persons.FirstOrDefaultAsync(p => p.Id == id);

            return person != null ? TypedResults.Ok(person) : TypedResults.NotFound();
        }


        public static async Task<IResult> GetInterestsForAPerson(PersonDbContext db, int id)
        {
            var personInterests = await db.InterestRelation
                .Where(i => i.PersonId == id)
                .Select(p => p.Interest)
                .ToListAsync();

            return personInterests != null ? TypedResults.Ok(personInterests) : TypedResults.NotFound();
        }



        public static async Task<IResult> GetLinksForAPerson(PersonDbContext db, int id)
        {
            var personLinks = await db.InterestRelation
                .Where(l => l.PersonId == id)
                .SelectMany(p => p.Interest.Links)
                .ToListAsync();

            return personLinks != null ? TypedResults.Ok(personLinks) : TypedResults.NotFound();
        }

    }
}
