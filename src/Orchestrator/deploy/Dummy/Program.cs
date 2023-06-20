var builder = WebApplication.CreateBuilder(args);

// var dbHost = "";

// using var client = new HttpClient();
// var body = await File.ReadAllTextAsync("pg-connector.conf.json");
// body = body.Replace("${db_host}", dbHost);
// // Console.WriteLine(body);

// using var request = new HttpRequestMessage(HttpMethod.Post, "http://localhost:8083/connectors");
// request.Content = new StringContent(body, System.Text.Encoding.UTF8, "application/json");
// var response = await client.SendAsync(request);
// if (!(response.IsSuccessStatusCode || response.StatusCode == System.Net.HttpStatusCode.Conflict))
// {
//     Console.WriteLine($"Error registering debezium connector: {response.ReasonPhrase}");
//     return;
// }

// Console.WriteLine("Registered debezium connector!");

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer(); // @@NOCHECKIN
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapControllers();

app.Run();
