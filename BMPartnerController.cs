using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using AXAPartnershipWeb.Models;
using AXAPartnershipWeb.Models.BMPartner;
using AXAPartnershipWeb.Models.ViewModels;
using AXAPartnershipWeb.Services;
using AXAPartnershipWeb.Services.BrokerService;
using AXAPartnershipWeb.Services.IServices;
using AXAPartnershipWeb.Utility;
using AXAPartnershipWeb.Utility.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using OfficeOpenXml;

namespace AXAPartnershipWeb.Controllers
{
    //[Authorize(Roles = BMPartnerRoles.Dashboard + "," + BMPartnerRoles.Archive + "," + BMPartnerRoles.PartnerPackages, AuthenticationSchemes = AuthenticaticationScheme.Partners_Scheme)]
    public class BMPartnerController : Controller
    {
        private readonly IPartnerIDRespository _ptnrIDRepo;
        private readonly IBMPackageRepository _pRepo;
        private readonly ILogger<BMPartnerController> _logger;
        private readonly IBMPCKageDetailsRepository _pkgDtlsRepo;
        private readonly IBMCompanyRepository _ptnRepo;
        private readonly IHttpClientFactory _clientFactory;
        private readonly IPartnerRepository _partnerRepository;
        private string _fileName;
        private string _connString;
        private readonly IMessageSenderService _messageSender;
        private readonly IEmailSender _emailSender;
        private string _token;
        private readonly IApiCallImplementor _apiCallImplementor;
        private readonly IHttpContextAccessor _httpContextAccessor;




        public BMPartnerController(IConfiguration configuration, IPartnerIDRespository ptnrIDRepo, IBMPackageRepository pRepo, ILogger<BMPartnerController> logger,
                                    IApiCallImplementor apiCallImplementor, IBMPCKageDetailsRepository pkgDtlsRepo, IBMCompanyRepository ptnRepo, IPartnerRepository partnerRepository,
                                    IHttpContextAccessor httpContextAccessor, IEmailSender emailSender, IMessageSenderService messageSender, IHttpClientFactory clientFactory)
        {
            _connString = configuration.GetConnectionString("AXADBConnectionString");
            _ptnrIDRepo = ptnrIDRepo;
            _pRepo = pRepo;
            _logger = logger;
            _pkgDtlsRepo = pkgDtlsRepo;
            _ptnRepo = ptnRepo;
            _partnerRepository = partnerRepository;
            _clientFactory = clientFactory;
            _messageSender = messageSender;
            _emailSender = emailSender;
            _apiCallImplementor = apiCallImplementor;
            _fileName = "Excel_Sheet" + DateTime.Now.ToString("dd-MM-yyyy") + Guid.NewGuid() + ".xlsx";
            _token = httpContextAccessor.HttpContext.Session.GetString("JWToken");
        }
        public IActionResult Dashboard()
        {
            //Disable dashboard.
            //return View();
            return RedirectToAction("AllBMPartnerPackages");
        }

        public IActionResult ChangePassword() => View();


        //public async Task<IActionResult> Profile()
        //{
        //    string usrID = HttpContext.Session.GetString("sessionUserID");
        //    if (usrID == null)
        //    {
        //        return RedirectToAction("PartnerLogin", "Account");
        //    }
        //    var partnerid = await _ptnrIDRepo.GetAsync(SD.GetBMPartnerProfile, Convert.ToInt32(usrID));
        //    var partnerId = partnerid.PartnerId;

        //    BMPartnerDetails ptnrDtls = new BMPartnerDetails()
        //    {
        //        PartnerId = partnerId
        //    };

        //    return View(ptnrDtls);
        //}

        public async Task<IActionResult> Profile()
        {

            BMPartnerDetails Response = new BMPartnerDetails();
            UserIdRequest request = new UserIdRequest();
            request.UserId = userId();
            //using (var httpClient = new HttpClient())
            //{
            //    var path = SD.GetBMPartnerProfile + "/?UserId=" +
            //    var request = new HttpRequestMessage(HttpMethod.Post, path);
            //    var client = _clientFactory.CreateClient();
            //    if (_token != null && _token.Length != 0)
            //    {
            //        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _token);
            //    }
            //    HttpResponseMessage response = await client.SendAsync(request);
            //    if (response.StatusCode == System.Net.HttpStatusCode.OK)
            //    {
            //        var jsonString = await response.Content.ReadAsStringAsync();
            //        apiResponse = JsonConvert.DeserializeObject<BMPartnerDetails>(jsonString);
            //    }
            //}

            try
            {
                var response = await _apiCallImplementor.PostApiService(SD.ApiBasePath1, "api/v1/BM/GetBMPartnerDetails/", request);
                //UserDetailsResponse result = response.responseContent == String.Empty ? new UserDetailsRequest() : JsonConvert.DeserializeObject<UserDetailsResponse>(response.responseContent);
                BMPartnerDetails apiResponse = JsonConvert.DeserializeObject<BMPartnerDetails>(response.responseContent) ?? new BMPartnerDetails();
                return View(apiResponse);
            }
            catch (Exception)
            {
                //ex.LogError();

            }


            return View(Response);
        }

        public IActionResult ContactUs() => View();

        [HttpPost]
        public async Task<IActionResult> ContactUs(CONTACT_US ContactUsDetails)
        {
            var code = PARTNER_HELPER.RandomString(ContactUsDetails.EmailAddress);
            AttachmentValues attValues = new AttachmentValues();
            List<AttachmentValues> emailAtt = new List<AttachmentValues>();
            string apiResponse = String.Empty;
            ContactUsDetails.UserName = userName();
            ContactUsDetails.RefCode = "REF-" + code;
            try
            {
                if (ModelState.IsValid)
                {
                    using (var httpClient = new HttpClient())
                    {
                        StringContent content = new StringContent(JsonConvert.SerializeObject(ContactUsDetails),
                            Encoding.UTF8, "application/json");
                        if (_token != null && _token.Length != 0)
                        {
                            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _token);
                        }
                        using (var response = await httpClient.PostAsync(SD.ContactUsMessageBM, content))
                        {
                            // Deserialize                          
                            apiResponse = await response.Content.ReadAsStringAsync();
                            var emailMessage = JsonConvert.DeserializeObject<BMEmailResponse>(apiResponse);
                            if (emailMessage.message.Equals(EmailMessage.MailSent))
                            {

                                ContactUsDetails.Message = $"New message from {ContactUsDetails.FirstName} {ContactUsDetails.LastName}:<br/>" +
                                                $"<br/>Email Address: {ContactUsDetails.EmailAddress} <br/>" +
                                                $"<br/>Message Reference Code: {ContactUsDetails.RefCode} <br/>" +
                                                $"<br/>---------------------------------------<br/>" +
                                                $"{ContactUsDetails.Message}.<br/>";
                                EmailSenderModel emailObj = new EmailSenderModel()
                                {
                                    //EmailReciever = SD.EmailReceiver,
                                    EmailReciever = "FIPartners@axamansard.com",
                                    EmailSubject = ContactUsDetails.TitleOfMessage,
                                    EmailBody = ContactUsDetails.Message,
                                    EmailCc = "",
                                    EmailBcc = ""
                                };
                                EmailSenderVM apiMessage1 = new EmailSenderVM();
                                var sendEmail = _messageSender.SendEmail(emailObj, emailAtt).Result;
                                if (sendEmail == "Email sent successfully!")
                                {
                                    string returnMessage = $"Dear {ContactUsDetails.FirstName} {ContactUsDetails.LastName} <br/>" +
                                            $"Thank you for contacting us.<br/>" +
                                            $"<br/>" +
                                            $" We have recieved your message and a personnel will get back to you shortly <br/>" +
                                            $"Thank you for choosing AXA Mansard.<br/>";
                                    emailObj = new EmailSenderModel()
                                    {
                                        EmailReciever = ContactUsDetails.EmailAddress,
                                        EmailSubject = ContactUsDetails.TitleOfMessage,
                                        EmailBody = returnMessage,
                                        EmailCc = "",
                                        EmailBcc = ""
                                    };
                                    sendEmail = _messageSender.SendEmail(emailObj, emailAtt).Result;
                                    switch (sendEmail)
                                    {
                                        case "Email sent successfully!":
                                            TempData["MessageSent"] = "Thanks for reaching out. We will review and get back to you in few minutes!";
                                            //return RedirectToAction(nameof(AllBMPartnerPackages));
                                            return View();
                                        default:
                                            TempData["MessageError"] = "error processing request";
                                            return View();
                                    }
                                }
                            }
                            TempData["MessageError"] = "error processing request";
                            return View();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                await ExceptionLogger.WriteToFile("An Exception has occurred");
                await ExceptionLogger.WriteToFile(ex.Message);
                await ExceptionLogger.WriteToFile(ex.StackTrace);
                ex.Message.ToString();
                _logger.LogError(ex, ex.Message);
            }
            TempData["MessageError"] = "error processing request";
            return View();
        }

        public async Task<IActionResult> AllBMPartnerPackages()
        {
            string usrID = HttpContext.Session.GetString("sessionUserID");
            if (usrID == null)
            {
                return RedirectToAction("PartnerLogin", "Account");
            }
            var partnerid = await _ptnrIDRepo.GetAsync(SD.bmPtnrIDPath, Convert.ToInt32(usrID));
            var partnerId = partnerid.PartnerId;

            PARTNERDTLS ptnrDtls = new PARTNERDTLS()
            {
                PartnerId = partnerId
            };

            return View(ptnrDtls);
        }

        [HttpGet]
        public async Task<IActionResult> GetAllPartnerPackages(int partnerId)
        {
            string usrID = HttpContext.Session.GetString("sessionUserID");
            if (usrID == null)
            {
                return RedirectToAction("PartnerLogin", "Account");
            }

            //var objPckgs = await _pRepo.GetAsyncList(SD.bmPtnerPckgPath, partnerId);
            return Json(new { data = await _pRepo.GetAsyncList(SD.bmPtnerPckgPath, partnerId) });
            // return Json(objPckgs);
        }

        [HttpGet]
        public async Task<IActionResult> PckgDetails(int? id)
        {
            string usrID = HttpContext.Session.GetString("sessionUserID");
            string usrNme = HttpContext.Session.GetString("sessionUName");
            if (usrID == null)
            {
                return RedirectToAction("PartnerLogin", "Account");
            }

            var objPckgVM = new PckageDtailsViewModel();
            if (id == null)
            {
                return NotFound();
            }

            try
            {
                var pckgDtls = await _pkgDtlsRepo.GetAsyncList(SD.bmpckgDtlsPath, id.GetValueOrDefault());

                IEnumerable<BMCOMPANIES> partnerList = await _ptnRepo.GetAllAsync(SD.bmcPath);

                foreach (var item in pckgDtls)
                {
                    objPckgVM.PckageDetails.Add(new PCKAGE
                    {
                        PackageId = item.PackageId,
                        PartnerId = item.PartnerId,
                        PartnerName = item.PartnerName,
                        Activated = item.Activated,
                        PackageDescription = item.PackageDescription,
                        Background = item.Background,
                        MaxEntryAge = item.MaxEntryAge,
                        MaxSumAssured = item.MaxSumAssured,
                        Tenure = item.Tenure,
                        ProductFeatures = item.ProductFeatures,
                        Request = item.Request,
                        TranStatus = item.TranStatus,
                        PartnerCom = item.PartnerCom,
                        FI = item.FI,
                        Retail = item.Retail,
                        Username = usrNme,
                        ProductCode = item.ProductCode,
                        BundleName = item.BundleName,
                        EntityId = item.EntityId,
                        EntityName = item.EntityName,
                        ChargeTypeId = item.ChargeTypeId,
                        Description = item.Description,
                        ComponentName = item.ComponentName,
                        Amount = item.Amount,
                        Rate = item.Rate,

                        PartnerList = partnerList.Select(i => new SelectListItem
                        {
                            Text = i.PartnerName,
                            Value = i.PartnerId.ToString()
                        })
                    });
                }
                if (pckgDtls[0].TranStatus > 0)
                {
                    return RedirectToAction("PckgViewDetails", "BMPartner", new { id });
                }
                return View(objPckgVM);
            }
            catch (Exception ex)
            {
                await ExceptionLogger.WriteToFile("An Exception Occured.");
                await ExceptionLogger.WriteToFile(ex.Message);
                await ExceptionLogger.WriteToFile(ex.StackTrace);
                ex.Message.ToString();
                _logger.LogError(ex, ex.Message);
            }

            return View(objPckgVM);
        }

        [HttpGet]
        public async Task<IActionResult> PckgViewDetails(int? id)
        {
            string usrID = HttpContext.Session.GetString("sessionUserID");
            string usrNme = HttpContext.Session.GetString("sessionUName");
            if (usrID == null)
            {
                return RedirectToAction("PartnerLogin", "Account");
            }

            var objPckgVM = new PckageDtailsViewModel();
            if (id == null)
            {
                return NotFound();
            }

            try
            {
                var pckgDtls = await _pkgDtlsRepo.GetAsyncList(SD.bmpckgVWDtlsPath, id.GetValueOrDefault());

                IEnumerable<BMCOMPANIES> partnerList = await _ptnRepo.GetAllAsync(SD.bmcPath);

                foreach (var item in pckgDtls)
                {
                    objPckgVM.PckageDetails.Add(new PCKAGE
                    {
                        PackageId = item.PackageId,
                        PartnerId = item.PartnerId,
                        PartnerName = item.PartnerName,
                        Activated = item.Activated,
                        PackageDescription = item.PackageDescription,
                        Background = item.Background,
                        MaxEntryAge = item.MaxEntryAge,
                        MaxSumAssured = item.MaxSumAssured,
                        Tenure = item.Tenure,
                        ProductFeatures = item.ProductFeatures,
                        Request = item.Request,
                        TranStatus = item.TranStatus,
                        PartnerCom = item.PartnerCom,
                        FI = item.FI,
                        Retail = item.Retail,
                        Username = usrNme,
                        ProductCode = item.ProductCode,
                        BundleName = item.BundleName,
                        EntityId = item.EntityId,
                        EntityName = item.EntityName,
                        ChargeTypeId = item.ChargeTypeId,
                        Description = item.Description,
                        ComponentName = item.ComponentName,
                        Amount = item.Amount,
                        Rate = item.Rate,

                        PartnerList = partnerList.Select(i => new SelectListItem
                        {
                            Text = i.PartnerName,
                            Value = i.PartnerId.ToString()
                        })
                    });
                }
                return View(objPckgVM);
            }
            catch (Exception ex)
            {
                await ExceptionLogger.WriteToFile("An Exception Occured.");
                await ExceptionLogger.WriteToFile(ex.Message);
                await ExceptionLogger.WriteToFile(ex.StackTrace);
                ex.Message.ToString();
                _logger.LogError(ex, ex.Message);
            }

            return View(objPckgVM);
        }

        public IActionResult BMUBatch() => View();

        [HttpPost]
        public async Task<IActionResult> BMUBatch(SomeForm someForm)
        {
            string fileName = FileUpload.GenerateFileName("BMBatch");

            var filepath = new PhysicalFileProvider(Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "BMEVUploads")).Root;

            var excelPath = Path.Combine(filepath, $"excel{userId() + "_" + fileName}.xls");
            var recieptPath = Path.Combine(filepath, $"reciept{userId() + "_" + fileName}.png");

            //using (var fileStream = new FileStream(excelPath, FileMode.Create, FileAccess.Write))
            //{
            //    someForm.Reciept.CopyTo(fileStream);
            //}
            //using (var fileStream = new FileStream(recieptPath, FileMode.Create, FileAccess.Write))
            //{
            //    someForm.Excel.CopyTo(fileStream);
            //}


            // COPY TO DIRECTORY
            var excelUpload = await FileUpload.UploadFileAsync(someForm.Excel, fileName, filepath);

            if (!excelUpload.success)
            {
                TempData["UploadError"] = "Error processing Excel Document";
            }


            var recieptUpload = await FileUpload.UploadFileAsync(someForm.Reciept, fileName, filepath);

            if (!recieptUpload.success)
            {
                TempData["UploadError"] = "Error processing Reciept";
            }

            // WRITE TO DB
            //BMBatchUploadDTO
            var batch = new BMBatchUploadDTO
            {
                BatchCode = fileName,
                BatchFileName = fileName,
                BundleId = Int32.Parse(someForm.BundleId),
                UserId = userId(),
                PartnerId = Int32.Parse(someForm.PartnerId),
                BundleName = someForm.BundleName,
                UserName = userName(),
                PartnerName = someForm.PartnerName,
                ExcelFilePath = excelUpload.uploadPath,
                RecieptFilePath = recieptUpload.uploadPath
            };

            var saveBatch = SaveNewBatchUpload(batch);

            if (saveBatch)
            {
                TempData["UploadSuccess"] = "Request Successful";
            }
            else
                TempData["UploadFailed"] = "Upload Failed";


            return RedirectToAction(nameof(BMUBatch));
        }



        public bool SaveNewBatchUpload(BMBatchUploadDTO uploadBatch)
        {
            try
            {
                string Result = String.Empty;
                using (SqlConnection con = new SqlConnection(_connString))
                {
                    using (SqlCommand cmd = new SqlCommand("STP_BM_ADD_BMBatchUpload", con))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.CommandTimeout = 12000;

                        cmd.Parameters.Add(new SqlParameter("@BatchCode", SqlDbType.Int));
                        cmd.Parameters["@BatchCode"].Value = uploadBatch.BatchCode;

                        cmd.Parameters.Add(new SqlParameter("@BundleID", SqlDbType.Int));
                        cmd.Parameters["@BundleID"].Value = uploadBatch.BundleId;

                        cmd.Parameters.Add(new SqlParameter("@PartnerID", SqlDbType.Int));
                        cmd.Parameters["@PartnerID"].Value = uploadBatch.PartnerId;

                        cmd.Parameters.Add(new SqlParameter("@ExcelFilePath", SqlDbType.VarChar));
                        cmd.Parameters["@ExcelFilePath"].Value = uploadBatch.ExcelFilePath;

                        cmd.Parameters.Add(new SqlParameter("@RecieptFilePath", SqlDbType.VarChar));
                        cmd.Parameters["@RecieptFilePath"].Value = uploadBatch.RecieptFilePath;

                        cmd.Parameters.Add(new SqlParameter("@UserName", SqlDbType.VarChar));
                        cmd.Parameters["@UserName"].Value = uploadBatch.UserName;

                        cmd.Parameters.Add(new SqlParameter("@PartnerName", SqlDbType.VarChar));
                        cmd.Parameters["@PartnerName"].Value = uploadBatch.PartnerName;

                        cmd.Parameters.Add(new SqlParameter("@BundleName", SqlDbType.VarChar));
                        cmd.Parameters["@BundleName"].Value = uploadBatch.BundleName;

                        cmd.Parameters.Add(new SqlParameter("@BatchFileName", SqlDbType.VarChar));
                        cmd.Parameters["@BatchFileName"].Value = uploadBatch.BatchFileName;

                        cmd.Parameters.Add(new SqlParameter("@DateCreated", SqlDbType.DateTime));
                        cmd.Parameters["@DateCreated"].Value = DateTime.UtcNow;

                        //cmd.Parameters.Add("@retValue", SqlDbType.Int).Direction = ParameterDirection.ReturnValue;
                        con.Open();
                        cmd.ExecuteNonQuery();
                        Result = Convert.ToString(cmd.Parameters["@Status"].Value);

                        //int retval = (int)cmd.Parameters["@retValue"].Value;
                        con.Close();
                        switch (Result)
                        {
                            case "Success": return true;
                            //case "User Exists": return Result;
                            case "Error": return false;
                        }
                        return false;
                    }
                }
            }
            catch (Exception ex)
            {

                throw;
            }
        }

        public Task<List<BMBatchUploadVM>> GetBMBatchUpload(string UserID)
        {
            List<BMBatchUploadVM> kycBatchList = new List<BMBatchUploadVM>();
            var res = new BMBatchUploadVM();
            try
            {
                using SqlConnection sql = new SqlConnection(_connString);
                using (SqlCommand compCmd = new SqlCommand("STP_BM_GET_BMBatchUploads", sql))
                {
                    compCmd.CommandTimeout = 12000;
                    compCmd.CommandType = CommandType.StoredProcedure;

                    compCmd.Parameters.Add(new SqlParameter("@UserId", SqlDbType.VarChar, 4000));
                    compCmd.Parameters["@UserId"].Value = UserID;

                    sql.Open();
                    SqlDataReader rd = compCmd.ExecuteReader();

                    if (rd.HasRows)
                    {

                        while (rd.Read())
                        {
                            string batchCode = rd.GetString("BatchCode");
                            string batchFileName = rd.GetString("BatchFileName");
                            int bundleId = rd.GetInt32("BundleId");
                            int partnerId = rd.GetInt32("PartnerId");
                            int userId = rd.GetInt32("UserId");
                            string bundleName = rd.GetString("BundleName");
                            string partnerName = rd.GetString("PartnerName");
                            string userName = rd.GetString("UserName");
                            string excelFilePath = rd.GetString("ExcelFilePath");
                            string recieptFilePath = rd.GetString("RecieptFilePath");
                            string dateUploadedString = rd.GetDateTime("DateCreated").ToString("yyyy-MM-dd");
                            //DateTime dateUploaded = rd.GetDateTime("DateCreated");
                            var batchUpload = new BMBatchUploadVM
                            {
                                UserId = userId,
                                PartnerId = partnerId,
                                BundleId = bundleId,
                                BatchCode = batchCode,
                                BatchFileName = batchFileName,
                                BundleName = bundleName,
                                PartnerName = partnerName,
                                UserName = userName,
                                ExcelFilePath = excelFilePath,
                                RecieptFilePath = recieptFilePath,
                                DateCreated = dateUploadedString,
                            };
                            kycBatchList.Add(batchUpload);
                        }
                    }

                    if (kycBatchList.Count > 0)
                    {
                        var cmpTypeResponse = new List<BMBatchUploadVM>();
                        cmpTypeResponse = kycBatchList;


                        return Task.FromResult(cmpTypeResponse);
                    }
                    else
                    {
                        return null;
                    }

                }
            }
            catch (Exception ex)
            {
                //res.Response = ex.Message;
            }
            return Task.FromResult(kycBatchList);
        }
        private static ExcelPackage ByteArrayToObject(byte[] arrBytes)
        {
            using (MemoryStream memStream = new MemoryStream(arrBytes))
            {
                ExcelPackage package = new ExcelPackage(memStream);
                return package;
            }
        }

        #region Get OTP
        [HttpPost]
        public async Task<IActionResult> GetOtp()
        {

            string responseApi = "";
            try
            {
                using (var httpClient = new HttpClient())
                {
                    using (var response = await httpClient.PostAsync(SD.ForgotPassword + userName(), null))
                    {
                        var apiResponse = await response.Content.ReadAsStringAsync();
                        var loginMessage = JsonConvert.DeserializeObject<string>(apiResponse);
                        if (loginMessage == "username does not exist")
                        {
                            responseApi = "Username Does not Exist";
                        }
                        if (loginMessage == "Mail Sent")
                        {
                            responseApi = "Mail Sent Successfuly";
                        }
                        if (loginMessage == "Mail not Sent")
                        {
                            responseApi = "Mail Not Sent";
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                await ExceptionLogger.WriteToFile("An Exception has occurred");
                await ExceptionLogger.WriteToFile(ex.Message);
                await ExceptionLogger.WriteToFile(ex.StackTrace);
                ex.Message.ToString();
                _logger.LogError(ex, ex.Message);
            }
            return Json(new { status = responseApi, message = "Success" });
        }
        #endregion

        #region Change Password

        [HttpPost]
        //[ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(ChangePassword changePassword)
        {
            string apiResponse = String.Empty;
            string loginRequestResponse = String.Empty;
            MESSAGE apiMessage = new MESSAGE();
            changePassword.Username = userName();
            try
            {
                using (var httpClient = new HttpClient())
                {
                    StringContent content = new StringContent(JsonConvert.SerializeObject(changePassword), Encoding.UTF8, "application/json");
                    using (var response = await httpClient.PostAsync(SD.ResetPartnersPassword, content))
                    {
                        apiResponse = await response.Content.ReadAsStringAsync();
                        loginRequestResponse = JsonConvert.DeserializeObject<string>(apiResponse);
                        if (loginRequestResponse == "Password update successful")
                        {
                            TempData["PasswordUpdateSuccess"] = "Password Changed Successfully";
                        }
                        if (loginRequestResponse == "Username or Password does not exist")
                        {
                            TempData["ViewBag.PasswordChangeFailed"] = "User does not exist";
                        }
                        if (loginRequestResponse == "OTP cannot be found")
                        {
                            TempData["OTPNOTFOUND"] = "Otp could not be found";
                        }

                    }
                }
            }
            catch (Exception ex)
            {
                await ExceptionLogger.WriteToFile("An Exception has occurred");
                await ExceptionLogger.WriteToFile(ex.Message);
                await ExceptionLogger.WriteToFile(ex.StackTrace);
                ex.Message.ToString();
                _logger.LogError(ex, ex.Message);
            }
            return View("ChangePassword");
        }
        #endregion
        private string userName()
        {

            //ClaimsIdentity claimsIdentity = User.Identity as ClaimsIdentity;
            //var UserName = User.Identity.IsAuthenticated ? claimsIdentity.FindFirst(ClaimTypes.Email).Value : "";
            var UserName = HttpContext.Session.GetString("sessionUserName");
            return UserName;
        }

        private int userId()
        {
            ClaimsIdentity claimsIdentity = User.Identity as ClaimsIdentity;
            var UserId = HttpContext.Session.GetString("sessionUserID");
            return Convert.ToInt32(UserId);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> BatchUpload(BMUUPLOAD bmuBatchUpload)
        {
            try
            {
                //string errorMessage = string.Empty;
                //string tempFileName = string.Empty;
                int k = 0;
                using var ms1 = new MemoryStream();
                using var ms2 = new MemoryStream();
                using (var httpClient = new HttpClient())
                {
                    var files = HttpContext.Request.Form.Files;
                    if (files.Count > 0)
                    {
                        var fileName = files[k];
                        var uniqueFileName = Convert.ToString(Guid.NewGuid());
                        var fileExtension = Path.GetExtension(fileName.FileName);
                        if (files[k].Name == "Excel" && fileExtension != ".xlsx")
                        {
                            TempData["FileExtension"] = "Invalid file extension";
                            return RedirectToAction(nameof(BMUBatch));
                        }
                        var filepath = new PhysicalFileProvider(Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "BMEVUploads")).Root + $@"\{fileName}";
                        //tempFileName = filepath;
                        for (int i = 0; i < files.Count; i++)
                        {
                            using (var fs1 = files[i].OpenReadStream())
                            {
                                if (i == 0)
                                {
                                    fs1.CopyTo(ms1);
                                    bmuBatchUpload.Excel = ms1.ToArray();

                                    //if (bmuBatchUpload.Excel != null)
                                    //{
                                    //    if (ValidateExcel(tempFileName, out errorMessage))
                                    //    {
                                    //        //save the data

                                    //        //spreadsheet is valid, show success message or any logic here
                                    //    }
                                    //    else
                                    //    {
                                    //        //set error message to show in front end
                                    //        ViewBag.ErrorMessage = errorMessage;
                                    //    }
                                    //}

                                }
                                else if (i == 1)
                                {
                                    fs1.CopyTo(ms2);
                                    bmuBatchUpload.Reciept = ms2.ToArray();
                                    bmuBatchUpload.RecieptName = files[1].FileName;
                                    bmuBatchUpload.ContentType = files[1].ContentType;
                                }

                            }
                        }
                        k++;
                    }

                    #region - Handles upload and validation of Excel sheet data (not completed)

                    StringContent content = new StringContent(JsonConvert.SerializeObject(bmuBatchUpload), Encoding.UTF8, "application/json");
                    using (var response = await httpClient.PostAsync(SD.BatchUploadAPIPath, content))
                    {
                        BMU_RESPONSE_MESSAGE upload = new BMU_RESPONSE_MESSAGE();
                        string apiResponse = await response.Content.ReadAsStringAsync();
                        upload = JsonConvert.DeserializeObject<BMU_RESPONSE_MESSAGE>(apiResponse);
                        if (upload.PassedEntries == " Batch upload successful")
                        {
                            if (upload.RecieptExists != null)
                            {
                                TempData["RecieptDuplicate"] = "Sorry! This evidence of payment exist already";
                            }
                            else
                            {
                                TempData["PassedEntries"] = upload.PassedEntries;
                                TempData["Failed"] = null;
                                TempData["Counts"] = 3;
                            }
                            var errorMessage = "None!";
                            var sFileName = "success";
                            return Json(new { sFileName, errorMessage });
                        }
                        if (upload.count != 3)
                        {
                            //ViewBag.Passed = upload.PassedEntries;
                            //ViewBag.Failed = upload.FailedEntries;

                            TempData["Passed"] = upload.PassedEntries;
                            TempData["Failed"] = upload.FailedEntries;
                            TempData["Counts"] = 1;
                            TempData["PassedEntries"] = null;
                            //return RedirectToAction("KycBatch");
                        }
                        if (upload.count == 1 || upload.count == 2 || upload.count == 3)
                        {
                            ExcelPackage.LicenseContext = OfficeOpenXml.LicenseContext.NonCommercial;
                            var stream = new System.IO.MemoryStream();
                            using (var pck = new ExcelPackage(stream))
                            {
                                var excelFile = ByteArrayToObject(upload.Excel);
                                var wds = pck.Workbook.Worksheets.Add("Worksheets-Name", excelFile.Workbook.Worksheets[0]);
                                var filepaths = new PhysicalFileProvider(Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "BMTemp")).Root;
                                string sFileName = _fileName;
                                string fullPath = Path.Combine(filepaths, _fileName);
                                FileInfo fi = new FileInfo(fullPath);
                                //pck.Save();
                                var errorMessage = "Some Errors";
                                pck.SaveAs(fi);
                                return Json(new { sFileName, errorMessage });
                            }
                        }
                    }


                    #endregion
                }
            }
            catch (Exception ex)
            {
                return StatusCode(403, "Error Processing request" + ex.Message);
            }
            return RedirectToAction(nameof(BMUBatch));
        }





        //[HttpPost]
        //public ActionResult BulkUserUpload(HttpPostedFileBase uploadFile)
        //{
        //    string tempFileName = string.Empty;
        //    string errorMessage = string.Empty;

        //    if (uploadFile != null && uploadFile.ContentLength > 0)
        //    {
        //        //ExcelExtension contains array of excel extension types
        //        if (Config.ExcelExtension.Any(p => p.Trim() == Path.GetExtension(uploadFile.FileName)))
        //        {
        //            //save the uploaded excel file to temp location
        //            SaveExcelTemp(uploadFile, out tempFileName);
        //            //validate the excel sheet
        //            if (ValidateExcel(tempFileName, out errorMessage))
        //            {
        //                //save the data
        //                SaveExcelDataToDatabase(tempFileName);
        //                //spreadsheet is valid, show success message or any logic here
        //            }
        //            else
        //            {
        //                //set error message to show in front end
        //                ViewBag.ErrorMessage = errorMessage;
        //            }
        //        }
        //        else
        //        {
        //            //excel sheet is not uploaded, show error message
        //        }
        //    }
        //    else
        //    {
        //        //file is not uploaded, show error message
        //    }

        //    return View();
        //}


        /*
        private bool ValidateExcel(string tempFileName, out string errorMessage)
        {
            bool result = true;
            string error = string.Empty;
            string filePath = tempFileName;
            FileInfo fileInfo = new FileInfo(filePath);
            ExcelPackage excelPackage = new ExcelPackage(fileInfo);
            ExcelWorksheet worksheet = excelPackage.Workbook.Worksheets.First();
            int totalRows = worksheet.Dimension == null ? -1 : worksheet.Dimension.End.Row; //worksheet total rows
            int totalCols = worksheet.Dimension == null ? -1 : worksheet.Dimension.End.Column; // total columns

            //check spread sheet has rows (empty spreadsheet uploaded)
            if (totalRows == -1)
            {
                result = false;
                error = "Empty spread sheet uploaded.";
            }
            //check rows are more than or equal 2 (spread sheet has only header row)
            else if (totalRows < 2)
            {
                result = false;
                error = "Spread sheet does not contain any data";
            }
            //check total columns equal to headers defined (less columns)
            else if (totalCols > 0 && totalCols != 15) //GetColumnHeaders().Count)
            {
                result = false;
                error = "Spread sheet column header value mismatch.";
            }

            if (result)
            {
                //validate header columns
                result &= ValidateHeaderColumns(worksheet, totalCols);

                if (result)
                {
                    //validate data rows, skip the header row (data rows start from 2)
                    result &= ValidateRows(worksheet, totalRows, totalCols);
                }

                if (!result)
                {
                    error = "There are some errors in the uploaded file. Please correct them and upload again.";
                }
            }

            errorMessage = error;
            worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();
            excelPackage.Save();
            return result;
        }

        private bool SetError(ExcelRange cell, string errorComment)
        {
            var fill = cell.Style.Fill;
            fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
            fill.BackgroundColor.SetColor(System.Drawing.Color.Red);
            cell.AddComment(errorComment, "");

            return false;
        }

        private bool ValidateHeaderColumns(ExcelWorksheet worksheet, int totlaColumns)
        {
            bool result = true;
            List<string> listColumnHeaders = GetColumnHeaders();

            for (int i = 1; i < totlaColumns; i++)
            {
                var cell = worksheet.Cells[1, i]; //header columns are in first row

                if (cell.Value != null)
                {
                    //column header has a value
                    if (!listColumnHeaders.Contains(cell.Value.ToString()))
                    {
                        result &= SetError(cell, "Invalid header. Please correct.");
                    }
                }
                else
                {
                    //empty header
                    result &= SetError(cell, "Empty header. Remove the column.");
                }
            }

            return result;
        }

        private bool ValidateRows(ExcelWorksheet worksheet, int totalRows, int totalCols)
        {
            bool result = true;

            for (int i = 2; i <= totalRows; i++) //data rows start from 2`
            {
                for (int j = 1; j <= totalCols; j++)
                {
                    var cell = worksheet.Cells[i, j];

                    switch (j)
                    {
                        //email address
                        case 1:
                            {
                                result &= ValidateEmailAddress(cell, "Email address");
                                break;
                            }
                        //first name
                        case 2:
                            {
                                result &= ValidateText(cell, "First name");
                                break;
                            }
                        //last name
                        case 3:
                            {
                                result &= ValidateText(cell, "Last name");
                                break;
                            }
                        //address line 1
                        case 4:
                            {
                                result &= ValidateText(cell, "Address line 1");
                                break;
                            }
                        //address line 2
                        case 5:
                            {
                                result &= ValidateText(cell, "Address line 2");
                                break;
                            }
                        //city
                        case 6:
                            {
                                result &= ValidateText(cell, "City");
                                break;
                            }
                        //telephone number
                        case 7:
                            {
                                result &= ValidateText(cell, "Telephone number");
                                break;
                            }
                        //mobile number
                        case 8:
                            {
                                result &= ValidateText(cell, "Mobile number");
                                break;
                            }
                        //job title
                        case 9:
                            {
                                result &= ValidateJobTitle(cell, "Job title");
                                break;
                            }
                        //salary
                        case 10:
                            {
                                result &= ValidateNumber(cell, "Salary");
                                break;
                            }
                        //role
                        case 11:
                            {
                                result &= ValidateRole(cell, "Role");
                                break;
                            }
                        //branch
                        case 12:
                            {
                                result &= ValidateBranch(cell, "Branch");
                                break;
                            }
                        //joined date
                        case 13:
                            {
                                result &= ValidateDate(cell, "Joined date");
                                break;
                            }
                    }
                }
            }

            return result;
        }

        private bool ValidateEmailAddress(ExcelRange cell, string columnName)
        {
            bool result = true;
            result = ValidateText(cell, columnName); //validate if empty or not

            if (result)
            {
                if (!ValidateEmail(cell.Value.ToString())) //ValidateEmail => true, if email format is correct
                {
                    result = SetError(cell, "Email address format is invalid.");
                }
                else if (cell.Value.ToString().Length > 150)
                {
                    result = SetError(cell, "Email address too long. Max characters 150.");
                }
            }
            return result;
        }

        private bool ValidateText(ExcelRange cell, string columnName)
        {
            bool result = true;
            string error = string.Format("{0} is empty", columnName);

            if (cell.Value != null)
            {
                //check if cell value has a value
                if (string.IsNullOrWhiteSpace(cell.Value.ToString()))
                {
                    result = SetError(cell, error);
                }
            }
            else
            {
                result = SetError(cell, error);
            }
            return result;
        }


        private bool ValidateNumber(ExcelRange cell, string columnName)
        {
            bool result = true;
            double value = 0.0;
            string error = string.Format("{0} format is incorrect.", columnName);
            result = ValidateText(cell, columnName);

            if (result)
            {
                if (!double.TryParse(cell.Value.ToString(), out value))
                {
                    result = SetError(cell, error);
                }
            }
            return result;
        }

        private bool ValidateDate(ExcelRange cell, string columnName)
        {
            bool result = true;
            DateTime date = DateTime.MinValue;
            string error = string.Format("{0} format is incorrect.", columnName);
            result = ValidateText(cell, columnName);

            if (result)
            {
                if (!DateTime.TryParse(cell.Value.ToString(), out date))
                {
                    result = SetError(cell, error);
                }
            }
            return result;
        }

        private bool ValidateJobTitle(ExcelRange cell, string columnName)
        {
            bool result = true;
            string error = "Job title does not exist.";
            List<JobTitle> listJobTitle = JobTitle.GetJobTitles((int)JobTitle.JobTitleStatus.Active);
            result = ValidateText(cell, columnName);

            if (result)
            {
                if (!listJobTitle.Any(x => x.Name.ToLowerInvariant() == cell.Value.ToString().ToLowerInvariant()))
                {
                    result = SetError(cell, error);
                }
            }

            return result;
        }

        */

        public async Task<IActionResult> GetAllKycPartners()
        {
            try
            {
                var _partnerName = HttpContext.Session.GetString("sessionPartnerName");
                var data = await _partnerRepository.GetAllAsyncPartner(SD.PartnerBatchPath, _partnerName);
                return Json(data);
            }
            catch (Exception ex)
            {
                return Json(ex.Message);
            }
        }

        public async Task<ActionResult> getPartner()
        {
            string usrID = HttpContext.Session.GetString("sessionUserID");
            if (usrID == null)
            {
                return RedirectToAction("PartnerLogin", "Account");
            }
            var partnerid = await _ptnrIDRepo.GetAsync(SD.bmPtnrIDPath, Convert.ToInt32(usrID));
            var partnerId = partnerid.PartnerId;

            BMCOMPANIES partnerList = await _ptnRepo.GetAsync(SD.bmcGetIdPath, partnerId);
            //var _partnerName = HttpContext.Session.GetString("sessionPartnerName");
            //var request = new HttpRequestMessage(HttpMethod.Get, SD.GetPartnerAPIPath + _partnerName);
            //var client = _clientFactory.CreateClient();
            //HttpResponseMessage response = await client.SendAsync(request);
            if (partnerList != null)
            {
                //var jsonString = await response.Content.ReadAsStringAsync();
                //var partner = JsonConvert.DeserializeObject<BMCOMPANIES>(jsonString);
                return Json(partnerList);
            }
            return null;
        }

        public async Task<ActionResult> getBundles(string partnerName)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, SD.bmListPartnersPath + partnerName);
            var client = _clientFactory.CreateClient();
            HttpResponseMessage response = await client.SendAsync(request);
            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                var jsonString = await response.Content.ReadAsStringAsync();
                var bundles = JsonConvert.DeserializeObject<List<Bundle>>(jsonString);
                return Json(bundles.ToList());
            }
            return null;
        }
    }
}
