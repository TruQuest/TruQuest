using Application.Common.Interfaces;
using Application.Subject.Commands.AddNewSubject;
using Application.Subject.Queries.GetSubject;
using Application.Subject.Queries.GetSubjects;
using Application.Subject.Queries.GetThingsList;

namespace API.Endpoints;

public static class SubjectEndpoints
{
    public static RouteGroupBuilder MapSubjectEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/subjects");

        group.MapPost(
            "/add",
            (HttpRequest request, ISenderWrapper sender, HttpContext context) => sender.Send(
                new AddNewSubjectCommand(request),
                serviceProvider: context.RequestServices
            )
        );

        group.MapGet(
            "/",
            (ISenderWrapper sender, HttpContext context) =>
                sender.Send(new GetSubjectsQuery(), serviceProvider: context.RequestServices)
        );

        group.MapGet(
            "/{id}",
            ([AsParameters] GetSubjectQuery query, ISenderWrapper sender, HttpContext context) =>
                sender.Send(query, serviceProvider: context.RequestServices)
        );

        group.MapGet(
            "/{subjectId}/things",
            ([AsParameters] GetThingsListQuery query, ISenderWrapper sender, HttpContext context) =>
                sender.Send(query, serviceProvider: context.RequestServices)
        );

        group.AddEndpointFilter(Filters.ConvertHandleResult);

        return group;
    }
}
