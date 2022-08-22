using ImportExportTest.Data;
using ImportExportTest.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Http;

namespace ImportExportTest.Controllers
{
    public class PilotGroupsController : ApiController
    {
        // GET: PailotGroups
        [HttpGet]
        [ActionName("Get")]
        [Route("api/pilotgroups")]
        public IHttpActionResult Get()
        {
            List<PilotGroups> results = new List<PilotGroups>();
            try
            {
                AppDbContext appDb = new AppDbContext();
                results = appDb.PilotGroups.ToList();

            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }

            return Json(results);
        }
        [HttpGet]
        [ActionName("Export")]
        [Route("api/pilotgroups/export")]
        public IHttpActionResult Export()
        {
            string errorMessage = "";
            HttpStatusCode httpStatusCode = 0;
            string filePath = "";
            string destFolder = HttpContext.Current.Server.MapPath($"~/PilotGroupExports/");

            //thread.Abort();
            if (!Directory.Exists(destFolder))
            {
                Directory.CreateDirectory(destFolder);
            }
            //Creating a new filename and appending date to it.
            string filename = "PilotGroup-" + Guid.NewGuid().ToString() + ".xlsx";
            try
            {
                List<PilotGroups> results = new List<PilotGroups>();
                AppDbContext appDb = new AppDbContext();
                results = appDb.PilotGroups.ToList();

                //Logic to create and write into xls file and save the file to the local machine
                filePath = Path.Combine(destFolder, filename);

                DataExportHelper dataExportHelper = new DataExportHelper(filePath);
                dataExportHelper.CreatePilotGroupExcelFile(results);

                HttpContext.Current.Response.Clear();
                HttpContext.Current.Response.Buffer = true;
                HttpContext.Current.Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                // to open file prompt Box open or Save file    
                HttpContext.Current.Response.AddHeader("content-disposition", "attachment;filename=" + filename.ToString());
                HttpContext.Current.Response.TransmitFile(filePath);

                HttpContext.Current.Response.End();

            }
            catch (Exception ex)
            {
                errorMessage = ex.Message.Replace("\r\n", "");
                httpStatusCode = HttpStatusCode.InternalServerError;
            }
            finally
            {
                //cleaning up BP Server
                try
                {
                    if (!string.IsNullOrEmpty(filePath))
                        File.Delete(filePath);

                }
                catch (Exception ex)
                {
                    errorMessage = ex.Message.Replace("\r\n", "");
                    httpStatusCode = HttpStatusCode.InternalServerError;
                }
            }
            if (errorMessage != "")
                throw new HttpResponseException(HttpResponse(errorMessage, httpStatusCode));

            return Json("PilotGroups Exported Successful");


        }
        [HttpPost]
        [ActionName("Import")]
        // [Route("api/pilotgroups/import")]
        public IHttpActionResult Import()
        {
            AppDbContext appDb = new AppDbContext();
            int count = HttpContext.Current.Request.Files.Count;
            HttpPostedFile uploadedFile = null;
            if (count > 0)
                uploadedFile = HttpContext.Current.Request.Files[0];
            string filePath = "";
            string errorMessage = "";
            HttpStatusCode httpStatusCode = 0;
            try
            {

                List<Entity> results = new List<Entity>();
                if (uploadedFile != null)
                {
                    string path = HttpContext.Current.Server.MapPath($"~/PilotGroupImports/");
                    if (!Directory.Exists(path))
                    {
                        Directory.CreateDirectory(path);
                    }

                    //Upload selected file to the BP Server
                    filePath = path + Guid.NewGuid().ToString() + "_" + Path.GetFileName(uploadedFile.FileName);
                    uploadedFile.SaveAs(filePath);

                    if (!Path.GetExtension(uploadedFile.FileName).Equals(".xlsx"))
                        return Json("Please select a excel file with .xlsx extension.");

                    //OpenXml to read data from selected file and return as DataTable
                  DataImportHelper dataImportHelper = new DataImportHelper(filePath);
                    System.Data.DataTable importedData = dataImportHelper.OpenXLSXFile();

                    //Converting datatable into user defined pilotgroup class
                    results = DataImportHelper.ConvertDataTable<Entity>(importedData);


                    if (importedData.Columns.Count <= 1)
                        return Json("Failed to read the file selected, please check if the file is not empty.\nIf error persists, contact support team.");

                    string Importstatus = "";
                    int block = 2;
                    int imported = 0;

                    for (int i = 1; i < results.Count - 1; i++)
                    {
                        Entity entity = results[i];
                        PilotGroups pilotGroup = new PilotGroups
                        {
                            NameOrDesc = entity.A,
                            StartDate = DateTime.FromOADate(double.Parse(entity.C)),
                            EndDate = DateTime.FromOADate(double.Parse(entity.D)),
                            AuditDate = DateTime.Now,
                            AuditUser = "defaultUser",
                            Active = entity.B == "Y" ? true : false
                        };
                        appDb.PilotGroups.Add(pilotGroup);
                        if (appDb.SaveChanges() > 0)
                        {
                            imported++;
                            Importstatus += $"{pilotGroup.NameOrDesc}: imported successful, record has been added.\n";
                        }
                        block++;

                    }
                    Importstatus += $"{imported} records added!";
                    return Json(Importstatus);
                }
                else
                {
                    errorMessage = "No File Selected. Please select file and try again.";
                    httpStatusCode = HttpStatusCode.BadRequest;
                }


            }
            catch (Exception ex)
            {
                errorMessage = ex.Message.Replace("\r\n", "");
                httpStatusCode = HttpStatusCode.InternalServerError;
            }
            finally
            {
                //cleaning up BP Server
                try
                {
                    if (!string.IsNullOrEmpty(filePath))
                        File.Delete(filePath);

                    if (uploadedFile != null)
                    {
                        uploadedFile.InputStream.Flush();
                        uploadedFile.InputStream.Close();
                        uploadedFile.InputStream.Dispose();
                    }

                }
                catch (Exception ex)
                {
                    errorMessage = ex.Message.Replace("\r\n", "");
                    httpStatusCode = HttpStatusCode.InternalServerError;
                }
            }

            if (errorMessage != "")
                throw new HttpResponseException(HttpResponse(errorMessage, httpStatusCode));
            return Json(errorMessage);
        }
        private HttpResponseMessage HttpResponse(string errorMessage, HttpStatusCode statusCode)
        {
            var response = new HttpResponseMessage(statusCode)
            {
                Content = new StringContent(errorMessage),
                ReasonPhrase = errorMessage
            };
            return response;
        }
    }
}