using WebApplication1ApiTest.APP.User;
using WebApplication1ApiTest.StartingArea;


namespace StartingArea
{
    public class Program 
    {
        public static void Main(string[] args) 
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.

            builder.Services.AddControllers();
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();

            //builder.Services.AddSwaggerGen();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            //if (app.Environment.IsDevelopment())
            //{
            //    app.UseSwagger();
            //    app.UseSwaggerUI();
            //}

            app.UseHttpsRedirection();

            app.UseAuthorization();

            app.MapControllers();

            TemporaryHandler temporaryHandler = new TemporaryHandler();

            app.MapGet("/api/user", async (context) => 
            {
                await temporaryHandler.ReceivingDataForm(context);
            });

            app.MapPost("/data", async (HttpContext httpContext) =>
            {
                using StreamReader reader = new StreamReader(httpContext.Request.Body);

                string temp = await reader.ReadToEndAsync();

                if (temp == "1") 
                {
                    return app.GetType().ToString();
                }
                else
                {
                    return "0";
                }
            });
            app.MapPost("/api/user",async (HttpContext httpContext) =>
            {
                await temporaryHandler.ReceivingDataForm(httpContext);
            });
            app.MapGet("/start", async (HttpContext httpContext) =>
            {
                await temporaryHandler.ReceivingDataForm(httpContext);
            });
            
            app.Run();
        }
    }
}


