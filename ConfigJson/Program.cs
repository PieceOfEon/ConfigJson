using ConfigJson;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

app.Run(async (context) =>
{
    context.Response.ContentType = "text/html; charset=utf-8";
    string jsonFilePath = "json.json";
    string jsonString = await File.ReadAllTextAsync(jsonFilePath);
    List<Person> people = JsonConvert.DeserializeObject<List<Person>>(jsonString);

    if (context.Request.Method == "POST" && context.Request.Path == "/update")
    {
        int personIndex;
        if (int.TryParse(context.Request.Form["personIndex"], out personIndex) && personIndex >= 0 && personIndex < people.Count)
        {
            Person updatedPerson = new Person();
            updatedPerson.Name = context.Request.Form["name"];
            updatedPerson.Age = int.Parse(context.Request.Form["age"]);
            updatedPerson.Languages = context.Request.Form["languages"].ToString().Split(',').ToList();
            updatedPerson.Company = new Company
            {
                Name = context.Request.Form["company"],
                City = context.Request.Form["city"]
            };

            // ��������� ������
            var validationContext = new ValidationContext(updatedPerson);
            var validationResults = new List<ValidationResult>();
            bool isValid = Validator.TryValidateObject(updatedPerson, validationContext, validationResults, true);
            if (isValid)
            {
                people[personIndex] = updatedPerson;

                string updatedJsonString = JsonConvert.SerializeObject(people, Formatting.Indented);
                await File.WriteAllTextAsync(jsonFilePath, updatedJsonString);

                // ������� ��������� �� �������� ���������� ����� JavaScript �����
                await context.Response.WriteAsync(@"
                    <script>
                        alert('������ ������� ��������� ��� �������� � ��������: " + personIndex + @"');
                        window.location.href = '/';
                    </script>
                ");
            }
            else
            {
                // ������� ��������� �� ������ ��������� ����� JavaScript �����
                string errorMessage = string.Join(", ", validationResults.Select(r => r.ErrorMessage));
                await context.Response.WriteAsync(@"
                    <script>
                        alert('������ ��������� ������: " + errorMessage + @"');
                        window.location.href = '/';
                    </script>
                ");
            }

            return;
        }
        else
        {
            await context.Response.WriteAsync("�������� ������ ��������.");
            return;
        }
    }

    if (context.Request.Method == "POST" && context.Request.Path == "/add")
    {
        Person newPerson = new Person();
        newPerson.Name = context.Request.Form["name"];
        newPerson.Age = int.Parse(context.Request.Form["age"]);
        newPerson.Languages = context.Request.Form["languages"].ToString().Split(',').ToList();
        newPerson.Company = new Company
        {
            Name = context.Request.Form["company"],
            City = context.Request.Form["city"]
        };

        // ��������� ������
        var validationContext = new ValidationContext(newPerson);
        var validationResults = new List<ValidationResult>();
        bool isValid = Validator.TryValidateObject(newPerson, validationContext, validationResults, true);
        if (isValid)
        {
            people.Add(newPerson);

            string updatedJsonString = JsonConvert.SerializeObject(people, Formatting.Indented);
            await File.WriteAllTextAsync(jsonFilePath, updatedJsonString);

            // ������� ��������� �� �������� ���������� ����� JavaScript �����
            await context.Response.WriteAsync(@"
                <script>
                    alert('������ ������� ���������.');
                    window.location.href = '/';
                </script>
            ");
        }
        else
        {
            // ������� ��������� �� ������ ��������� ����� JavaScript �����
            string errorMessage = string.Join(", ", validationResults.Select(r => r.ErrorMessage));
            await context.Response.WriteAsync(@"
                <script>
                    alert('������ ��������� ������: " + errorMessage + @"');
                    window.location.href = '/';
                </script>
            ");
        }

        return;
    }

    if (context.Request.Method == "POST" && context.Request.Path == "/delete")
    {
        int personIndex;
        if (int.TryParse(context.Request.Form["personIndex"], out personIndex) && personIndex >= 0 && personIndex < people.Count)
        {
            people.RemoveAt(personIndex);

            string updatedJsonString = JsonConvert.SerializeObject(people, Formatting.Indented);
            await File.WriteAllTextAsync(jsonFilePath, updatedJsonString);

            // ������� ��������� �� �������� �������� ����� JavaScript �����
            await context.Response.WriteAsync(@"
                <script>
                    alert('������ ������� �������.');
                    window.location.href = '/';
                </script>
            ");
        }
        else
        {
            await context.Response.WriteAsync("�������� ������ ��������.");
            return;
        }
    }

    StringBuilder output = new StringBuilder();

    output.AppendLine("<html>");
    output.AppendLine("<head>");
    output.AppendLine("<title>People Data</title>");
    output.AppendLine("<style>");
    output.AppendLine("body { font-family: Arial, sans-serif; margin: 20px; }");
    output.AppendLine("h2 { margin-bottom: 5px; }");
    output.AppendLine("p { margin-top: 0; }");
    output.AppendLine("ul { margin-top: 0; padding-left: 20px; }");
    output.AppendLine("form { margin-bottom: 10px; }");
    output.AppendLine("label { display: block; font-weight: bold; margin-top: 10px; }");
    output.AppendLine("input[type='text'], input[type='number'] { width: 200px; }");
    output.AppendLine("input[type='submit'] { margin-top: 10px; }");
    output.AppendLine("</style>");
    output.AppendLine("</head>");
    output.AppendLine("<body>");

    foreach (Person person in people)
    {
        output.AppendLine("<div style='border: 1px solid #ccc; padding: 10px; margin-bottom: 10px;'>");
        output.AppendLine("<h2>���: " + person.Name + "</h2>");
        output.AppendLine("<p>�������: " + person.Age + "</p>");

        output.AppendLine("<p>�����:</p><ul>");
        foreach (string language in person.Languages)
        {
            output.AppendLine("<li>" + language + "</li>");
        }
        output.AppendLine("</ul>");

        output.AppendLine("<p>��������: " + person.Company.Name + " (" + person.Company.City + ")</p>");

        output.AppendLine("<form method='post' action='/update'>");
        output.AppendLine("<input type='hidden' name='personIndex' value='" + people.IndexOf(person) + "'>");
        output.AppendLine("<label>���:</label>");
        output.AppendLine("<input type='text' name='name' value='" + person.Name + "' required><br>");
        output.AppendLine("<label>�������:</label>");
        output.AppendLine("<input type='number' name='age' value='" + person.Age + "' required><br>");
        output.AppendLine("<label>����� (��������� ��������):</label>");
        output.AppendLine("<input type='text' name='languages' value='" + string.Join(",", person.Languages) + "' required><br>");
        output.AppendLine("<label>��������:</label>");
        output.AppendLine("<input type='text' name='company' value='" + person.Company.Name + "' required><br>");
        output.AppendLine("<label>�����:</label>");
        output.AppendLine("<input type='text' name='city' value='" + person.Company.City + "' required><br>");
        output.AppendLine("<input type='submit' value='�������� ������'>");
        output.AppendLine("</form>");

        output.AppendLine("<form method='post' action='/delete'>");
        output.AppendLine("<input type='hidden' name='personIndex' value='" + people.IndexOf(person) + "'>");
        output.AppendLine("<input type='submit' value='������� ������'>");
        output.AppendLine("</form>");

        output.AppendLine("</div>");
    }

    output.AppendLine("<div style='border: 1px solid #ccc; padding: 10px; margin-bottom: 10px;'>");
    output.AppendLine("<h2>�������� ������ ��������:</h2>");
    output.AppendLine("<form method='post' action='/add'>");
    output.AppendLine("<label>���:</label>");
    output.AppendLine("<input type='text' name='name' value='' required><br>");
    output.AppendLine("<label>�������:</label>");
    output.AppendLine("<input type='number' name='age' value='' required><br>");
    output.AppendLine("<label>����� (��������� ��������):</label>");
    output.AppendLine("<input type='text' name='languages' value='' required><br>");
    output.AppendLine("<label>��������:</label>");
    output.AppendLine("<input type='text' name='company' value='' required><br>");
    output.AppendLine("<label>�����:</label>");
    output.AppendLine("<input type='text' name='city' value='' required><br>");
    output.AppendLine("<input type='submit' value='�������� ������'>");
    output.AppendLine("</form>");
    output.AppendLine("</div>");

    output.AppendLine("</body>");
    output.AppendLine("</html>");

    context.Response.ContentType = "text/html; charset=utf-8";
    await context.Response.WriteAsync(output.ToString());
});

app.Run();
