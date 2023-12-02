using GoThataway;

using Application.Subject.Commands.AddNewSubject;
using Application.Subject.Queries.GetSubject;
using Application.Subject.Queries.GetSubjects;
using Application.Subject.Queries.GetThingsList;

namespace API.Endpoints;

public static class SubjectEndpoints
{
    public static RouteGroupBuilder MapSubjectEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/subjects");

        group.MapPost(
            "/add",
            (HttpRequest request, Thataway thataway) =>
                thataway.Send(new AddNewSubjectCommand(request))
        );

        group.MapGet(
            "/",
            (Thataway thataway) => thataway.Send(new GetSubjectsQuery())
        );

        group.MapGet(
            "/{id}",
            ([AsParameters] GetSubjectQuery query, Thataway thataway) =>
                thataway.Send(query)
        );

        group.MapGet(
            "/{subjectId}/things",
            ([AsParameters] GetThingsListQuery query, Thataway thataway) =>
                thataway.Send(query)
        );

        group.AddEndpointFilter(Filters.ConvertHandleResult);

        return group;
    }
}
