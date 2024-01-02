using lab3_Api.Data;
using Microsoft.EntityFrameworkCore;

namespace lab3_Api.Endpoints
{
    public static class ExtensionEndPoints
    {

        public static IEndpointRouteBuilder PersonApiExtensions(
            this IEndpointRouteBuilder endPointBuilder)
        {
            var grpEndPointBuilder = endPointBuilder.MapGroup("api/person");



            endPointBuilder.MapGet("/", async (PersonDbContext personDb) =>
              TypedResults.Ok(await personDb.Persons.ToListAsync()))
                .WithTags("Get")
                .WithOpenApi(op =>
                {
                    op.Summary = "Get all persons";
                    return op;
                });





            return endPointBuilder;

        }










    }
}
