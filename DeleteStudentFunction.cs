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

public static class DeleteStudentFunction
{
    //Código de estado 200 si se eliminó correctamente 
    //Código de estado 404 Not Found si no se encontró el alumno. 
    //500 en caso de errores
    [FunctionName("DeleteStudent")]
    public static async Task<IActionResult> Run(
        [HttpTrigger(AuthorizationLevel.Function, "delete", Route = "students/{id}")] HttpRequest req,
        string id,
        ILogger log)
    {
        try
        {
            // Validar el ID del alumno
            if (string.IsNullOrEmpty(id) || !Guid.TryParse(id, out _))
            {
                return new BadRequestResult();
            }

            string connectionString = "Server=sql-tucambista-evaluacion.database.windows.net;Database=sqldb-tucambista-evaluacion;User Id=LeonardoSolimano;Password=7sK48@I9l%%1_";

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                await connection.OpenAsync();

                using (SqlCommand command = new SqlCommand("DELETE FROM Alumnos WHERE Id = @Id", connection))
                {
                    command.Parameters.AddWithValue("@Id", id);

                    int rowsAffected = await command.ExecuteNonQueryAsync();

                    if (rowsAffected > 0)
                    {
                        return new OkResult(); // Se eliminó el Alumno
                    }
                    else
                    {
                        return new NotFoundResult(); // No se encontró el alumno
                    }
                }
            }
        }
        catch (Exception ex)
        {
            log.LogError($"Se encontró un error al eliminar el alumno: {ex.Message}");
            return new StatusCodeResult(StatusCodes.Status500InternalServerError);
        }
    }
}