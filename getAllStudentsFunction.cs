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


public static class getAllStudentFunction
{
    [FunctionName("GetAllStudents")]
    public static async Task<IActionResult> GetAllStudents(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "students")] HttpRequest req,
        ILogger log)
    {
        try
        {
            string connectionString = "Server=sql-tucambista-evaluacion.database.windows.net;Database=sqldb-tucambista-evaluacion;User Id=LeonardoSolimano;Password=7sK48@I9l%%1_";

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                await connection.OpenAsync();

                using (SqlCommand command = new SqlCommand("SELECT * FROM Alumnos", connection))
                {
                    DataTable dataTable = new DataTable();
                    using (SqlDataAdapter adapter = new SqlDataAdapter(command))
                    {
                        adapter.Fill(dataTable);
                    }

                    // Lista de alumnos en formato de arreglo JSON
                    string studentList = JsonConvert.SerializeObject(dataTable);

                    return new OkObjectResult(studentList);
                }
            }
        }
        catch (Exception ex)
        {
            log.LogError($"Se encontró un error al obtener la lista de alumnos: {ex.Message}");
            return new StatusCodeResult(StatusCodes.Status500InternalServerError);
        }
    }
}