using System.Text.Json;
using WebApplication1ApiTest.APP.User;

namespace WebApplication1ApiTest.StartingArea
{
    public class TemporaryHandler
    {
        public async Task ReceivingDataForm(HttpContext context)
        {
            var response = context.Response;

            var request = context.Request;

            if (request.Path == "/api/user")
            {
                var message = "Некорректные данные";   // содержание сообщения по умолчанию

                if (request.HasJsonContentType())
                {
                    try
                    {
                        var jsoptions = new JsonSerializerOptions();

                        jsoptions.Converters.Add(new UserConverter());

                        var user = await request.ReadFromJsonAsync<User>(jsoptions);

                        //// пытаемся получить данные json
                        //var user = await request.ReadFromJsonAsync<User>();

                        if (user != null) // если данные сконвертированы в User
                            message = $"UserFullName: {user.UserFullName}  UserEmail: {user.UserEmail}  UserId: {user.UserId}  UserNickName: {user.UserNickname}";
                    }
                    catch { }
                    // отправляем пользователю данные
                    await response.WriteAsJsonAsync(new { text = message });
                }
            }
            else
            {
                response.ContentType = "text/html; charset=utf-8";

                await response.SendFileAsync("html/index.html");
            }
        }
        public void ReturnSimpleJson(WebApplication app)
        {
            app.Run(async (context) =>
            {
                User user = new User("123", "ДДВ", "NK", "dongaev@inbox.ru");

                await context.Response.WriteAsJsonAsync(user);
            });
        }
    }
}
