using System;
using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

public static class getStudentByIdFunction
{
    [FunctionName("getStudentById")]
    public static async Task<IActionResult> GetStudentById(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "students/{studentId}")] HttpRequest req,
        string studentId,
        ILogger log)
    {
        try
        {
            string connectionString = "Server=sql-tucambista-evaluacion.database.windows.net;Database=sqldb-tucambista-evaluacion;User Id=LeonardoSolimano;Password=7sK48@I9l%%1_;";

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                await connection.OpenAsync();

                // Consulta SQL para obtener un alumno por su ID
                string sqlQuery = "SELECT * FROM Alumnos WHERE Id = @StudentId";
                using (SqlCommand command = new SqlCommand(sqlQuery, connection))
                {
                    command.Parameters.AddWithValue("@StudentId", studentId);

                    DataTable dataTable = new DataTable();
                    using (SqlDataAdapter adapter = new SqlDataAdapter(command))
                    {
                        adapter.Fill(dataTable);
                    }

                    // Verificar si se encontró un alumno
                    if (dataTable.Rows.Count == 0)
                    {
                        return new NotFoundResult(); //Código 404 - Alumno no encontrado
                    }

                    // Formatear el alumno como un objeto JSON
                    string studentJson = JsonConvert.SerializeObject(dataTable.Rows[0]);

                    return new OkObjectResult(studentJson);
                }
            }
        }
        catch (Exception ex)
        {
            log.LogError($"Se encontró un error al obtener el alumno por id: {ex.Message}");
            return new StatusCodeResult(StatusCodes.Status500InternalServerError);
        }
    }
}