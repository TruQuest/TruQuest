using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

using Domain.Aggregates;
using Infrastructure.Persistence;

var app = API.Program.CreateWebApplication(args);
using var scope = app.Services.CreateScope();

var appDbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
appDbContext.Database.Migrate();

appDbContext.Tags.Add(new Tag("Politics"));
appDbContext.SaveChanges();

var eventDbContext = scope.ServiceProvider.GetRequiredService<EventDbContext>();
eventDbContext.Database.Migrate();