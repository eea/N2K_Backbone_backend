using N2K_BackboneBackEnd.Models.ViewModel;
using System.Text.Json;
using System.Text.Json.Serialization;
namespace N2K_BackboneBackEnd.Helpers
{
    public class CodeChangeDetailSerialiser : JsonConverter<CodeChangeDetail>
    {
        enum TypeDiscriminator
        {
            CodeChangeDetailModify = 1,
            CodeChangeDetailAddedRemovedSpecies = 2,
            CodeChangeDetailAddedRemovedHabitats =3

        }

        public override bool CanConvert(Type typeToConvert) =>
                typeof(CodeChangeDetail).IsAssignableFrom(typeToConvert);


        public override CodeChangeDetail Read(
            ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType != JsonTokenType.StartObject)
            {
                throw new JsonException();
            }

            reader.Read();
            if (reader.TokenType != JsonTokenType.PropertyName)
            {
                throw new JsonException();
            }

            string? propertyName = reader.GetString();
            if (propertyName != "TypeDiscriminator")
            {
                throw new JsonException();
            }

            reader.Read();
            if (reader.TokenType != JsonTokenType.Number)
            {
                throw new JsonException();
            }

            TypeDiscriminator typeDiscriminator = (TypeDiscriminator)reader.GetInt32();
            CodeChangeDetail person = typeDiscriminator switch
            {
                TypeDiscriminator.CodeChangeDetailModify => new CodeChangeDetailModify(),
                TypeDiscriminator.CodeChangeDetailAddedRemovedSpecies => new CodeChangeDetailAddedRemovedSpecies(),
                TypeDiscriminator.CodeChangeDetailAddedRemovedHabitats => new CodeChangeDetailAddedRemovedHabitats(),

                _ => throw new JsonException()
            };

            while (reader.Read())
            {
                if (reader.TokenType == JsonTokenType.EndObject)
                {
                    return person;
                }

                if (reader.TokenType == JsonTokenType.PropertyName)
                {
                    propertyName = reader.GetString();
                    reader.Read();
                    switch (propertyName)
                    {
                        case "CreditLimit":
                            string? creditLimit = reader.GetString();
                            ((CodeChangeDetailModify)person).Reported = creditLimit;
                            break;
                        case "OfficeNumber":
                            string? officeNumber = reader.GetString();
                            ((CodeChangeDetailAddedRemovedHabitats)person).CoverHa = officeNumber;
                            break;

                    }
                }
            }

            throw new JsonException();
        }

        public override void Write(
            Utf8JsonWriter writer, CodeChangeDetail person, JsonSerializerOptions options)
        {

            /*
            TypeDiscriminator.CodeChangeDetailModify => new CodeChangeDetailModify(),
                TypeDiscriminator.CodeChangeDetailAddedRemovedSpecies => new CodeChangeDetailAddedRemovedSpecies(),
                TypeDiscriminator.CodeChangeDetailAddedRemovedHabitats => new CodeChangeDetailAddedRemovedHabitats(),
            */

            writer.WriteStartObject();

            if (person is CodeChangeDetailModify employee0)
            {
                writer.WriteNumber("TypeDiscriminator", (int)TypeDiscriminator.CodeChangeDetailModify);
                writer.WriteString("CreditLimit", employee0.Reported);
            }
            else if (person is CodeChangeDetailAddedRemovedSpecies employee1)
            {
                writer.WriteNumber("TypeDiscriminator", (int)TypeDiscriminator.CodeChangeDetailAddedRemovedSpecies);
                writer.WriteString("OfficeNumber", employee1.Population);
            }
            else if (person is CodeChangeDetailAddedRemovedHabitats employee2)
            {
                writer.WriteNumber("TypeDiscriminator", (int)TypeDiscriminator.CodeChangeDetailAddedRemovedHabitats);
                writer.WriteString("OfficeNumber", employee2.CoverHa);
            }


            writer.WriteString("Name", person.Name);

            writer.WriteEndObject();
        }
    }
}