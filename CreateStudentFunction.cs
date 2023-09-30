using System;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

public static class CreateStudentFunction
{
    [FunctionName("CreateStudent")]
    public static async Task<IActionResult> Run(
        [HttpTrigger(AuthorizationLevel.Function, "post", Route = "students")] HttpRequest req,
        ILogger log)
    {
        try
        {
            // Leer y analizar el cuerpo de la solicitud JSON
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            var newStudent = JsonConvert.DeserializeObject<Student>(requestBody);
             // Validar los datos del alumno


            //Conexión bdd
            string connectionString = "Server=sql-tucambista-evaluacion.database.windows.net;Database=sqldb-tucambista-evaluacion;User Id=LeonardoSolimano;Password=7sK48@I9l%%1_";
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                await connection.OpenAsync();

                // Insertar alumno en bdd
                using (SqlCommand command = new SqlCommand(
                    "INSERT INTO Alumnos (Nombres, Apellidos, Dirección, Teléfono, Foto) " +
                    "VALUES (@Nombres, @Apellidos, @Dirección, @Teléfono, @Foto); SELECT SCOPE_IDENTITY()",
                    connection))
                {
                    command.Parameters.AddWithValue("@Nombres", newStudent.Nombres);
                    command.Parameters.AddWithValue("@Apellidos", newStudent.Apellidos);
                    command.Parameters.AddWithValue("@Dirección", newStudent.Dirección);
                    command.Parameters.AddWithValue("@Teléfono", newStudent.Teléfono);
                    command.Parameters.AddWithValue("@Foto", newStudent.Foto);

                    // Realizar inserción y obtener el id del nuevo alumno
                    int newStudentId = Convert.ToInt32(await command.ExecuteScalarAsync());

                    // Devolver resultado con el id del nuevo alumno
                    return new OkObjectResult(new { Id = newStudentId });
                }
            }
        }
        catch (Exception ex)
        {
            log.LogError($"Se encontró un error al crear un nuevo alumno: {ex.Message}");
            return new StatusCodeResult(StatusCodes.Status500InternalServerError);
        }
    }

    public class Student
    {
        public string Nombres { get; set; }
        public string Apellidos { get; set; }
        public string Dirección { get; set; }
        public string Teléfono { get; set; }
        public string Foto { get; set; }
    }
}