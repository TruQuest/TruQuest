using MediatR;

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
            (HttpRequest request, ISender mediator) =>
                mediator.Send(new AddNewSubjectCommand { Request = request })
        );

        group.MapGet(
            "/",
            (ISender mediator) => mediator.Send(new GetSubjectsQuery())
        );

        group.MapGet(
            "/{id}",
            ([AsParameters] GetSubjectQuery query, ISender mediator) =>
                mediator.Send(query)
        );

        group.MapGet(
            "/{subjectId}/things",
            ([AsParameters] GetThingsListQuery query, ISender mediator) =>
                mediator.Send(query)
        );

        return group;
    }
}