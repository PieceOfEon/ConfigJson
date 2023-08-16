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

var jsonFilePath = "json.json";

app.Run(async context =>
{
    context.Response.ContentType = "text/html; charset=utf-8";

    if (context.Request.Path == "/")
    {
        
        var registrationPage = @"
    <html>
    <head>
        <title>Регистрация</title>
    </head>
    <body style='font-family: Arial, sans-serif; background-color: #f4f4f4; margin: 0; padding: 0;'>
        <div style='width: 400px; margin: 0 auto; padding: 20px; background-color: #fff; box-shadow: 0 2px 4px rgba(0, 0, 0, 0.1);'>
            <h2 style='text-align: center; margin-bottom: 20px;'>Регистрация</h2>
            <form method='post' action='/register'>
                <div style='margin-bottom: 15px;'>
                    <label style='display: block; font-weight: bold; margin-bottom: 5px;' for='name'>Имя:</label>
                    <input type='text' id='name' name='name' style='width: 100%; padding: 10px; border: 1px solid #ccc; border-radius: 4px;' required>
                </div>
                <div style='margin-bottom: 15px;'>
                    <label style='display: block; font-weight: bold; margin-bottom: 5px;' for='age'>Возраст:</label>
                    <input type='number' id='age' name='age' style='width: 100%; padding: 10px; border: 1px solid #ccc; border-radius: 4px;' required>
                </div>
                <div style='margin-bottom: 15px;'>
                    <label style='display: block; font-weight: bold; margin-bottom: 5px;' for='languages'>Языки (разделите запятыми):</label>
                    <input type='text' id='languages' name='languages' style='width: 100%; padding: 10px; border: 1px solid #ccc; border-radius: 4px;' required>
                </div>
                <div style='margin-bottom: 15px;'>
                    <label style='display: block; font-weight: bold; margin-bottom: 5px;' for='company'>Компания:</label>
                    <input type='text' id='company' name='company' style='width: 100%; padding: 10px; border: 1px solid #ccc; border-radius: 4px;' required>
                </div>
                <div style='margin-bottom: 15px;'>
                    <label style='display: block; font-weight: bold; margin-bottom: 5px;' for='city'>Город:</label>
                    <input type='text' id='city' name='city' style='width: 100%; padding: 10px; border: 1px solid #ccc; border-radius: 4px;' required>
                </div>
                <div style='margin-bottom: 15px;'>
                    <input type='submit' value='Зарегистрироваться' style='background-color: #007bff; color: #fff; cursor: pointer;'>
                </div>
            </form>
            <a href='/users' style='display: block; text-align: center; margin-top: 10px; color: #007bff; text-decoration: none;'>Посмотреть пользователей</a>
        </div>
    </body>
    </html>
    ";

        await context.Response.WriteAsync(registrationPage);
    }
    else if (context.Request.Path == "/users")
    {
        // Чтение данных из JSON-файла
        string jsonString = await File.ReadAllTextAsync(jsonFilePath);
        List<Person> people = JsonConvert.DeserializeObject<List<Person>>(jsonString);

        
        var usersPage = new StringBuilder();
        usersPage.AppendLine("<html>");
        usersPage.AppendLine("<head>");
        usersPage.AppendLine("<title>Список пользователей</title>");
        usersPage.AppendLine("<style>");
        usersPage.AppendLine("body { font-family: Arial, sans-serif; background-color: #f4f4f4; margin: 0; padding: 0; }");
        usersPage.AppendLine("h2 { text-align: center; margin-top: 20px; }");
        usersPage.AppendLine("table { width: 80%; margin: 20px auto; border-collapse: collapse; }");
        usersPage.AppendLine("th, td { border: 1px solid #ccc; padding: 8px; text-align: left; }");
        usersPage.AppendLine("th { background-color: #007bff; color: white; }");
        usersPage.AppendLine("form { display: inline; }");
        usersPage.AppendLine("a { display: block; text-align: center; margin-top: 10px; color: #007bff; text-decoration: none; }");
        usersPage.AppendLine("</style>");
        usersPage.AppendLine("</head>");
        usersPage.AppendLine("<body>");
        usersPage.AppendLine("<h2>Список пользователей</h2>");
        usersPage.AppendLine("<table>");
        usersPage.AppendLine("<tr><th>Имя</th><th>Возраст</th><th>Языки</th><th>Компания</th><th>Город</th><th>Действия</th></tr>");

        foreach (Person person in people)
        {
            usersPage.AppendLine("<tr>");
            usersPage.AppendLine($"<td>{person.Name}</td><td>{person.Age}</td><td>{string.Join(", ", person.Languages)}</td><td>{person.Company.Name}</td><td>{person.Company.City}</td>");
            usersPage.AppendLine("<td>");
            usersPage.AppendLine("<form method='post' action='/edit'>");
            usersPage.AppendLine($"<input type='hidden' name='personIndex' value='{people.IndexOf(person)}'>");
            usersPage.AppendLine("<input type='submit' value='Изменить'>");
            usersPage.AppendLine("</form>");
            usersPage.AppendLine("<form method='post' action='/delete'>");
            usersPage.AppendLine($"<input type='hidden' name='personIndex' value='{people.IndexOf(person)}'>");
            usersPage.AppendLine("<input type='submit' value='Удалить'>");
            usersPage.AppendLine("</form>");
            usersPage.AppendLine("</td>");
            usersPage.AppendLine("</tr>");
        }

        usersPage.AppendLine("</table>");
        usersPage.AppendLine("<a href='/' style='display: block; text-align: center; margin-top: 10px; color: #007bff; text-decoration: none;'>Регистрация</a>");
        usersPage.AppendLine("</body>");
        usersPage.AppendLine("</html>");

        await context.Response.WriteAsync(usersPage.ToString());
    }
    else if (context.Request.Path == "/register" && context.Request.Method == "POST")
    {
       
        var name = context.Request.Form["name"];
        var age = int.Parse(context.Request.Form["age"]);
        var languages = context.Request.Form["languages"].ToString().Split(',').ToList();
        var companyName = context.Request.Form["company"];
        var companyCity = context.Request.Form["city"];

        var newPerson = new Person
        {
            Name = name,
            Age = age,
            Languages = languages,
            Company = new Company
            {
                Name = companyName,
                City = companyCity
            }
        };

        
        string jsonString = await File.ReadAllTextAsync(jsonFilePath);
        List<Person> people = JsonConvert.DeserializeObject<List<Person>>(jsonString);
        people.Add(newPerson);

       
        string updatedJsonString = JsonConvert.SerializeObject(people, Formatting.Indented);
        await File.WriteAllTextAsync(jsonFilePath, updatedJsonString);

        // Перенаправление на страницу регистрации
        context.Response.Redirect("/");
    }
    else if (context.Request.Path == "/edit" && context.Request.Method == "POST")
    {
        
        string jsonString = await File.ReadAllTextAsync(jsonFilePath);
        List<Person> people = JsonConvert.DeserializeObject<List<Person>>(jsonString);

        int personIndex;
        if (int.TryParse(context.Request.Form["personIndex"], out personIndex) && personIndex >= 0 && personIndex < people.Count)
        {
            var person = people[personIndex];

            var editPage = new StringBuilder();
            editPage.AppendLine("<html>");
            editPage.AppendLine("<head>");
            editPage.AppendLine("<title>Редактирование пользователя</title>");
            editPage.AppendLine("<style>");
            editPage.AppendLine("body { font-family: Arial, sans-serif; background-color: #f4f4f4; margin: 0; padding: 0; }");
            editPage.AppendLine("h2 { text-align: center; margin-top: 20px; }");
            editPage.AppendLine("form { width: 60%; margin: 20px auto; padding: 20px; background-color: white; border-radius: 5px; box-shadow: 0px 0px 5px rgba(0, 0, 0, 0.2); }");
            editPage.AppendLine("label { display: block; font-weight: bold; margin-top: 10px; }");
            editPage.AppendLine("input[type='text'], input[type='number'] { width: 100%; padding: 8px; margin-top: 5px; border: 1px solid #ccc; border-radius: 3px; }");
            editPage.AppendLine("input[type='submit'] { margin-top: 10px; background-color: #007bff; color: white; border: none; padding: 10px 20px; border-radius: 3px; cursor: pointer; }");
            editPage.AppendLine("a { display: block; text-align: center; color: #007bff; text-decoration: none; margin-top: 10px; }");
            editPage.AppendLine("</style>");
            editPage.AppendLine("</head>");
            editPage.AppendLine("<body>");
            editPage.AppendLine("<h2>Редактирование пользователя</h2>");
            editPage.AppendLine("<form method='post' action='/update'>");
            editPage.AppendLine($"<input type='hidden' name='personIndex' value='{personIndex}'>");
            editPage.AppendLine("<label>Имя:</label>");
            editPage.AppendLine($"<input type='text' name='name' value='{person.Name}' required>");
            editPage.AppendLine("<label>Возраст:</label>");
            editPage.AppendLine($"<input type='number' name='age' value='{person.Age}' required>");
            editPage.AppendLine("<label>Языки (разделите запятыми):</label>");
            editPage.AppendLine($"<input type='text' name='languages' value='{string.Join(",", person.Languages)}' required>");
            editPage.AppendLine("<label>Компания:</label>");
            editPage.AppendLine($"<input type='text' name='company' value='{person.Company.Name}' required>");
            editPage.AppendLine("<label>Город:</label>");
            editPage.AppendLine($"<input type='text' name='city' value='{person.Company.City}' required>");
            editPage.AppendLine("<input type='submit' value='Сохранить изменения'>");
            editPage.AppendLine("</form>");
            editPage.AppendLine($"<a href='/users'>Вернуться к списку пользователей</a>");
            editPage.AppendLine("</body>");
            editPage.AppendLine("</html>");

            await context.Response.WriteAsync(editPage.ToString());
        }
        else
        {
            context.Response.StatusCode = 400; // Bad Request
        }
    }
    else if (context.Request.Path == "/delete" && context.Request.Method == "POST")
    {
        
        int personIndex = int.Parse(context.Request.Form["personIndex"]);
        string jsonString = await File.ReadAllTextAsync(jsonFilePath);
        List<Person> people = JsonConvert.DeserializeObject<List<Person>>(jsonString);

        if (personIndex >= 0 && personIndex < people.Count)
        {
            people.RemoveAt(personIndex);

            
            string updatedJsonString = JsonConvert.SerializeObject(people, Formatting.Indented);
            await File.WriteAllTextAsync(jsonFilePath, updatedJsonString);
        }

       
        context.Response.Redirect("/users");
    }
    else if (context.Request.Path == "/update" && context.Request.Method == "POST")
    {
        string jsonString = await File.ReadAllTextAsync(jsonFilePath);
        List<Person> people = JsonConvert.DeserializeObject<List<Person>>(jsonString);
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

           
            var validationContext = new ValidationContext(updatedPerson);
            var validationResults = new List<ValidationResult>();
            bool isValid = Validator.TryValidateObject(updatedPerson, validationContext, validationResults, true);
            if (isValid)
            {
                people[personIndex] = updatedPerson;

                string updatedJsonString = JsonConvert.SerializeObject(people, Formatting.Indented);
                await File.WriteAllTextAsync(jsonFilePath, updatedJsonString);

                
                context.Response.Redirect("/users");
            }
            else
            {
                // Выводим сообщение об ошибке валидации через JavaScript алерт
                string errorMessage = string.Join(", ", validationResults.Select(r => r.ErrorMessage));
                await context.Response.WriteAsync(@"
                <script>
                    alert('Ошибка валидации данных: " + errorMessage + @"');
                    window.location.href = '/edit?personIndex=" + personIndex + @"';
                </script>
            ");
            }
        }
        else
        {
            context.Response.StatusCode = 400; // Bad Request
        }
    }
    else
    {
        context.Response.StatusCode = 404;
    }
});

app.Run();
