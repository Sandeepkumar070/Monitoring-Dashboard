using Dapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Spiro_Andon.Data;
using Spiro_Andon.Models;
using System.Diagnostics;
using System.Linq;


namespace Spiro_Andon.Controllers
{
    public class HomeController : Controller
    {

        private readonly string _connectionString = "Data Source=192.168.3.1,1433;Initial Catalog=Spiro_DWI;User ID=Spiro_DWI_S;Password=1234;Encrypt=True;TrustServerCertificate=True";
        private readonly ILogger<HomeController> _logger;
        private static List<Quality> reports = new List<Quality>();

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
       
        }
        public IActionResult Index()
        {
            return View();
        }
      
        [HttpPost]
        public IActionResult InsertIncident(Incident_Report incidentReport)
        {
            if (incidentReport == null)
            {
                return BadRequest("Incident report data is null.");
            }

            try
            {
                using (SqlConnection connection = new SqlConnection(_connectionString))
                {
                    connection.Open();

                    string insertQuery = @"INSERT INTO [dbo].[Incident_Report] 
                                            ([EMP_ID], [EMP_NAME], [Date_of_Incident], [Type_of_Incident], [Describe_Incident]) 
                                            VALUES (@EmpID, @EmpName, @DateOfIncident, @IncidentType, @Description)";

                    using (SqlCommand command = new SqlCommand(insertQuery, connection))
                    {
                        command.Parameters.AddWithValue("@EmpID", incidentReport.EmpID);
                        command.Parameters.AddWithValue("@EmpName", incidentReport.EmpName);
                        command.Parameters.AddWithValue("@DateOfIncident", incidentReport.DateOfIncident);
                        command.Parameters.AddWithValue("@IncidentType", incidentReport.IncidentType);
                        command.Parameters.AddWithValue("@Description", incidentReport.Description);

                        int rowsAffected = command.ExecuteNonQuery();

                        if (rowsAffected > 0)
                        {
                            //TempData["SuccessMessage"] = "Incident added successfully.";
                            return RedirectToAction("Privacy");
                        }
                        else
                        {
                            TempData["ErrorMessage"] = "Error while inserting the incident report.";
                            return RedirectToAction("Privacy");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Internal server error: {ex.Message}";
                return RedirectToAction("Privacy");
            }
        }
        //Reload Function For gridview

        [HttpGet]
        public IActionResult UpdateIncident(Incident_Report report)
        {
            if (report == null || report.Id == 0 || string.IsNullOrWhiteSpace(report.EmpName))
            {
                return Json(new { success = false, error = "Invalid input data." });
            }

            try
            {
                using (SqlConnection connection = new SqlConnection(_connectionString))
                {
                    connection.Open();

                    string updateQuery = @"
                UPDATE [dbo].[Incident_Report]
                SET [EMP_ID] = @EmpID,
                    [EMP_NAME] = @EmpName,
                    [Date_of_Incident] = @DateOfIncident,
                    [Type_of_Incident] = @IncidentType,
                    [Describe_Incident] = @Description
                WHERE [ID] = @ID";

                    using (SqlCommand command = new SqlCommand(updateQuery, connection))
                    {
                        command.Parameters.Add(new SqlParameter("@Id", System.Data.SqlDbType.Int) { Value = report.Id });
                        command.Parameters.Add(new SqlParameter("@EmpID", System.Data.SqlDbType.NVarChar, 50) { Value = report.EmpID ?? (object)DBNull.Value });
                        command.Parameters.Add(new SqlParameter("@EmpName", System.Data.SqlDbType.NVarChar, 100) { Value = report.EmpName });
                        command.Parameters.Add(new SqlParameter("@DateOfIncident", System.Data.SqlDbType.DateTime) { Value = report.DateOfIncident });
                        command.Parameters.Add(new SqlParameter("@IncidentType", System.Data.SqlDbType.NVarChar, 50) { Value = report.IncidentType ?? (object)DBNull.Value });
                        command.Parameters.Add(new SqlParameter("@Description", System.Data.SqlDbType.NVarChar, -1) { Value = report.Description ?? (object)DBNull.Value });

                        int rowsAffected = command.ExecuteNonQuery();

                        if (rowsAffected > 0)
                        {
                            return Json(new { success = true });
                        }
                        else
                        {
                            return Json(new { success = false, error = "No record updated." });
                        }
                    }
                }
            }
            catch (SqlException sqlEx)
            {
                return Json(new { success = false, error = $"Database error: {sqlEx.Message}" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, error = $"Error: {ex.Message}" });
            }
        }


        [HttpGet]
        public IActionResult Privacy(DateTime? startDate, DateTime? endDate)
        {
            List<Incident_Report> reports = new List<Incident_Report>();

            try
            {
                using (SqlConnection connection = new SqlConnection(_connectionString))
                {
                    connection.Open();

                    string selectQuery;
                    if (startDate.HasValue && endDate.HasValue)
                    {
                        selectQuery = @"
                    SELECT [ID], [EMP_ID], [EMP_NAME], [Date_of_Incident], 
                           [Type_of_Incident], [Describe_Incident]
                    FROM [dbo].[Incident_Report]
                    WHERE [Date_of_Incident] BETWEEN @StartDate AND @EndDate
                    ORDER BY [ID] DESC";
                    }
                    else
                    {
                        selectQuery = @"
                    SELECT TOP 15 [ID], [EMP_ID], [EMP_NAME], [Date_of_Incident], 
                                   [Type_of_Incident], [Describe_Incident]
                    FROM [dbo].[Incident_Report]
                    ORDER BY [ID] DESC";
                    }

                    using (SqlCommand command = new SqlCommand(selectQuery, connection))
                    {
                        if (startDate.HasValue && endDate.HasValue)
                        {
                            command.Parameters.AddWithValue("@StartDate", startDate.Value);
                            command.Parameters.AddWithValue("@EndDate", endDate.Value);
                        }

                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                reports.Add(new Incident_Report
                                {
                                    Id = Convert.ToInt32(reader["ID"]),
                                    EmpID = reader["EMP_ID"].ToString(),
                                    EmpName = reader["EMP_NAME"].ToString(),
                                    DateOfIncident = Convert.ToDateTime(reader["Date_of_Incident"]),
                                    IncidentType = reader["Type_of_Incident"].ToString(),
                                    Description = reader["Describe_Incident"].ToString()
                                });
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error fetching data: {ex.Message}";
            }

            return View(reports);
        }

        [HttpGet]
        public JsonResult GetIncidentById(int id)
        {
            Incident_Report report = null;

            try
            {
                using (SqlConnection connection = new SqlConnection(_connectionString))
                {
                    connection.Open();

                    string selectQuery = @"
                SELECT [ID], [EMP_ID], [EMP_NAME], [Date_of_Incident], 
                       [Type_of_Incident], [Describe_Incident]
                FROM [dbo].[Incident_Report]
                WHERE [ID] = @Id";

                    using (SqlCommand command = new SqlCommand(selectQuery, connection))
                    {
                        command.Parameters.AddWithValue("@Id", id);

                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                report = new Incident_Report
                                {
                                    Id = Convert.ToInt32(reader["ID"]),
                                    EmpID = reader["EMP_ID"].ToString(),
                                    EmpName = reader["EMP_NAME"].ToString(),
                                    DateOfIncident = Convert.ToDateTime(reader["Date_of_Incident"]),
                                    IncidentType = reader["Type_of_Incident"].ToString(),
                                    Description = reader["Describe_Incident"].ToString()
                                };
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                return Json(new { error = ex.Message });
            }

            return Json(report);
        }

        


        [HttpGet]
        public IActionResult GetTotalIncidentCount()
        {
           
            try
            {
                int totalIncidents;
                using (SqlConnection connection = new SqlConnection(_connectionString))
                {
                    string query = "SELECT COUNT(*) FROM [Spiro_DWI].[dbo].[Incident_Report]";
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        connection.Open();
                        totalIncidents = (int)command.ExecuteScalar();
                        connection.Close();
                    }
                }

                // Return JSON with the count
                return Json(new { success = true, totalIncidents });
            }
            catch (Exception ex)
            {
                // Handle and log exceptions
                return Json(new { success = false, message = "Error: " + ex.Message });
            }
        }

        [HttpGet]
        public IActionResult GetMinorIncidentCount()
        {
           
            int minorIncidentCount = 0;

            try
            {
                using (SqlConnection connection = new SqlConnection(_connectionString))
                {
                    string query = "SELECT COUNT(*) FROM [Spiro_DWI].[dbo].[Incident_Report] WHERE Type_of_Incident = 'Minor Incident'";
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        connection.Open();
                        minorIncidentCount = (int)command.ExecuteScalar();
                    }
                }
                return Json(new { success = true, count = minorIncidentCount });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "An error occurred: " + ex.Message });
            }
        }
        [HttpGet]
        public IActionResult GetMajorIncidentCount()
        {
           
            int majorIncidentCount = 0;

            try
            {
                using (SqlConnection connection = new SqlConnection(_connectionString))
                {
                    string query = "SELECT COUNT(*) FROM [Spiro_DWI].[dbo].[Incident_Report] WHERE Type_of_Incident = 'Major Incident'";
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        connection.Open();
                        majorIncidentCount = (int)command.ExecuteScalar();
                    }
                }
                return Json(new { success = true, count = majorIncidentCount });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "An error occurred: " + ex.Message });
            }
        }



        public IActionResult QualityForm()
        {
            return View();
        }



     




        [HttpPost]
        public async Task<IActionResult> QualityForm(string Textbox1, string Textbox2, string Combobox, string Datepicker, string Description)
        {
            if (string.IsNullOrEmpty(Textbox1) || string.IsNullOrEmpty(Textbox2) ||
                string.IsNullOrEmpty(Combobox) || string.IsNullOrEmpty(Datepicker) || string.IsNullOrEmpty(Description))
            {
                ModelState.AddModelError("", "All fields are required.");
                return View(); // Return to the form view with validation errors
            }

            bool isSaved = false;

            try
            {
                using (SqlConnection connection = new SqlConnection(_connectionString))
                {
                    string query = @"INSERT INTO [Spiro_DWI].[dbo].[Quality] 
                            ([VIN], [Checked_by], [Defacut_Type], [Date], [Description])
                            VALUES (@VIN, @CheckedBy, @DefectType, @Date, @Description)";

                    SqlCommand command = new SqlCommand(query, connection);
                    command.Parameters.AddWithValue("@VIN", Textbox1);
                    command.Parameters.AddWithValue("@CheckedBy", Textbox2);
                    command.Parameters.AddWithValue("@DefectType", Combobox);
                    command.Parameters.AddWithValue("@Date", Datepicker);
                    command.Parameters.AddWithValue("@Description", Description);

                    connection.Open();
                    await command.ExecuteNonQueryAsync();
                }

                isSaved = true;
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "An error occurred while saving data: " + ex.Message);
            }

            ViewBag.IsSaved = isSaved;
            return View();
        }









        [HttpGet]
        public IActionResult GetQualityReports(DateTime? startDate, DateTime? endDate)
        {
            List<Quality> qualityReports = new List<Quality>();

            try
            {
                using (SqlConnection connection = new SqlConnection(_connectionString))
                {
                    connection.Open();

                    string selectQuery;

                    // If startDate and endDate are provided, filter by date range
                    if (startDate.HasValue && endDate.HasValue)
                    {
                        selectQuery = @"
                SELECT [ID], [VIN], [Checked_by], [Defacut_Type], [Date], [Description]
                FROM [Spiro_DWI].[dbo].[Quality]
                WHERE [Date] BETWEEN @StartDate AND @EndDate
                ORDER BY [ID] DESC";
                    }
                    else
                    {
                        // If no dates are provided, fetch the top 10 reports
                        selectQuery = @"
                SELECT TOP 10 [ID], [VIN], [Checked_by], [Defacut_Type], [Date], [Description]
                FROM [Spiro_DWI].[dbo].[Quality]
                ORDER BY [ID] DESC";
                    }

                    using (SqlCommand command = new SqlCommand(selectQuery, connection))
                    {
                        // Add parameters for date range if provided
                        if (startDate.HasValue && endDate.HasValue)
                        {
                            command.Parameters.AddWithValue("@StartDate", startDate.Value);
                            command.Parameters.AddWithValue("@EndDate", endDate.Value);
                        }

                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                qualityReports.Add(new Quality
                                {
                                    Id = Convert.ToInt32(reader["ID"]),
                                    VIN = reader["VIN"].ToString(),
                                    CheckedBy = reader["Checked_by"].ToString(),
                                    DefectType = reader["Defacut_Type"].ToString(),
                                    Date = Convert.ToDateTime(reader["Date"]),
                                    Description = reader["Description"].ToString()
                                });
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while fetching quality reports.");
                TempData["ErrorMessage"] = $"Error fetching data: {ex.Message}";
            }

            return Json(qualityReports);
        }






        [HttpPost]
        public IActionResult Update(int id, QualityReport report)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    using (SqlConnection connection = new SqlConnection(_connectionString))
                    {
                        connection.Open();

                        string query = @"
                    UPDATE [Spiro_DWI].[dbo].[Quality]
                    SET 
                        [VIN] = @VIN,
                        [Checked_by] = @CheckedBy,
                        [Defacut_Type] = @DefectType,
                        [Date] = @Date,
                        [Description] = @Description
                    WHERE 
                        [ID] = @ID";

                        using (SqlCommand command = new SqlCommand(query, connection))
                        {
                            // Add parameters to the command
                            command.Parameters.AddWithValue("@VIN", report.VIN ?? (object)DBNull.Value);
                            command.Parameters.AddWithValue("@CheckedBy", report.CheckedBy ?? (object)DBNull.Value);
                            command.Parameters.AddWithValue("@DefectType", report.DefectType ?? (object)DBNull.Value);
                            command.Parameters.AddWithValue("@Date", report.Date ?? (object)DBNull.Value); // Assuming Date is nullable
                            command.Parameters.AddWithValue("@Description", report.Description ?? (object)DBNull.Value);
                            command.Parameters.AddWithValue("@ID", id);

                            int rowsAffected = command.ExecuteNonQuery();

                            if (rowsAffected > 0)
                            {
                                TempData["Message"] = "Record updated successfully.";
                            }
                            else
                            {
                                TempData["Message"] = "No record found to update.";
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    // Handle any database-related exceptions
                    TempData["Error"] = $"An error occurred: {ex.Message}";
                }
            }
            else
            {
                TempData["Error"] = "Please correct the errors in the form and try again.";
            }

            // Return the form view with the model to preserve the form data
            return View("QualityForm", report);
        }

        //Production Planning Code start fron hear

        public IActionResult Production()
        {
            return View();
        }

        public IActionResult Cost()
        {
            return View();
        }
        //Delevery


       
        public IActionResult Delevery()
        {
            return View();
        }
        public IActionResult People()
        {
            return View();
        }
        //Pro_Report
        public IActionResult Pro_Report()
        {
            return View();
        }
        public IActionResult Login()
        {
            return View();
        }
        public IActionResult Loss_analysis()
        {
            return View();
        }
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
 

}
