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

public static class UpdateStudentFunction
{
    [FunctionName("UpdateStudent")]
    public static async Task<IActionResult> Run(
        [HttpTrigger(AuthorizationLevel.Function, "put", Route = "students/{id}")] HttpRequest req,
        int id,
        ILogger log)
    {
        try
        {
            // Leer y analizar el cuerpo de la solicitud JSON
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            var updatedStudent = JsonConvert.DeserializeObject<Student>(requestBody);

            

            // Conexión nueva a la bdd
            string connectionString = Environment.GetEnvironmentVariable("SqlConnectionString");

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                await connection.OpenAsync();

                // Verificar si el alumno existe
                using (SqlCommand checkCommand = new SqlCommand(
                    "SELECT COUNT(*) FROM Alumnos WHERE Id = @Id",
                    connection))
                {
                    checkCommand.Parameters.AddWithValue("@Id", id);

                    int existingStudentCount = Convert.ToInt32(await checkCommand.ExecuteScalarAsync());

                    if (existingStudentCount == 0)
                    {
                        return new NotFoundResult(); // Alumno no encontrado ó no existe
                    }
                }

                // Actualizar información de alumno en  bdd
                using (SqlCommand updateCommand = new SqlCommand(
                    "UPDATE Alumnos SET Nombres = @Nombres, Apellidos = @Apellidos, Dirección = @Dirección, Teléfono = @Teléfono, Foto = @Foto " +
                    "WHERE Id = @Id",
                    connection))
                {
                    updateCommand.Parameters.AddWithValue("@Id", id);
                    updateCommand.Parameters.AddWithValue("@Nombres", updatedStudent.Nombres);
                    updateCommand.Parameters.AddWithValue("@Apellidos", updatedStudent.Apellidos);
                    updateCommand.Parameters.AddWithValue("@Dirección", updatedStudent.Dirección);
                    updateCommand.Parameters.AddWithValue("@Teléfono", updatedStudent.Teléfono);
                    updateCommand.Parameters.AddWithValue("@Foto", updatedStudent.Foto);

                    await updateCommand.ExecuteNonQueryAsync();

                    return new OkResult(); // Actualización finalizada correctamente
                }
            }
        }
        catch (Exception ex)
        {
            log.LogError($"Se encontró un error al actualizar el alumno: {ex.Message}");
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