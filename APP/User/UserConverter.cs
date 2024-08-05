using System.Text.Json;
using System.Text.Json.Serialization;

namespace WebApplication1ApiTest.APP.User
{
    public class UserConverter : JsonConverter<User>
    {
        public override User? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var userId = string.Empty;

            var userFullName = string.Empty;

            var userNickName = string.Empty;

            var userEmail = string.Empty;

            while (reader.Read()) 
            {
                if (reader.TokenType == JsonTokenType.PropertyName)
                {
                    var propertyName = reader.GetString();

                    reader.Read();

                    switch (propertyName?.ToLower())
                    {
                        case "userid" when reader.TokenType == JsonTokenType.String:

                            string? id = reader.GetString();

                            if (id != null)
                            {
                                userId = id;
                            }

                            break;

                        case "userfullname" when reader.TokenType == JsonTokenType.String:

                            string? fullName = reader.GetString();

                            if (fullName != null)
                            {
                                userFullName = fullName;
                            }

                            break;

                        case "usernickname" when reader.TokenType == JsonTokenType.String:

                            string? nickName = reader.GetString();

                            if (nickName != null) 
                            {
                                userNickName = nickName;
                            }

                            break;

                        case "useremail" when reader.TokenType == JsonTokenType.String:

                            string? email = reader.GetString();

                            if (email != null)
                            {
                                userEmail = email;
                            }

                            break;

                        default:
                            break;
                    }
                }
            }
            return new User(userId, userFullName, userNickName, userEmail);
        }
        public override void Write(Utf8JsonWriter writer, User user, JsonSerializerOptions options)
        {
            writer.WriteStartObject();

            writer.WriteString("UserEmail", user.UserEmail);

            writer.WriteString("UserNickName", user.UserNickname);

            writer.WriteString("UserFullName", user.UserFullName);

            writer.WriteString("UserId", user.UserId);

            writer.WriteEndObject();
        }
    }
}
