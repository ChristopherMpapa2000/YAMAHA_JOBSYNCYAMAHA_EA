using System;
using System.Configuration;
using System.Linq;
using System.Threading;

namespace JobSyncYAMAHA_EA
{
    internal class Program
    {
        public static string _Connection = ConfigurationSettings.AppSettings["ConnectionString"];
        public static string _LogFile = ConfigurationSettings.AppSettings["LogFile"];
        public static void Log(String iText)
        {
            string pathlog = _LogFile;
            String logFolderPath = System.IO.Path.Combine(pathlog, DateTime.Now.ToString("yyyyMMdd"));

            if (!System.IO.Directory.Exists(logFolderPath))
            {
                System.IO.Directory.CreateDirectory(logFolderPath);
            }
            String logFilePath = System.IO.Path.Combine(logFolderPath, DateTime.Now.ToString("yyyyMMdd") + ".txt");

            try
            {
                using (System.IO.StreamWriter outfile = new System.IO.StreamWriter(logFilePath, true))
                {
                    System.Text.StringBuilder sbLog = new System.Text.StringBuilder();

                    String[] listText = iText.Split('|').ToArray();

                    foreach (String s in listText)
                    {
                        sbLog.AppendLine($"[{DateTime.Now:HH:mm:ss}] {s}");
                    }

                    outfile.WriteLine(sbLog.ToString());
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error writing log file: {ex.Message}");
            }
        }
        static void Main(string[] args)
        {
            var _context = new YAMAHADataContext(_Connection);
            try
            {
                Log("====== Start Process ====== : " + DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss"));
                Log(string.Format("Run batch as :{0}", System.Security.Principal.WindowsIdentity.GetCurrent().Name));
                var viewAll = _context.V_EMPLOYEE_TYMs.ToList();
                Log("V_EMPLOYEE_TYM : " + viewAll.Count());
                Console.WriteLine("V_EMPLOYEE_TYM : " + viewAll.Count());
                foreach (var viewEmp in viewAll)
                {
                    if (!string.IsNullOrEmpty(viewEmp.NAMPOSE) && !string.IsNullOrEmpty(viewEmp.NAMPOST))
                    {
                        var positionQuery = _context.MSTPositions.Where(x => x.NameEn == viewEmp.NAMPOSE || x.NameTh == viewEmp.NAMPOST);
                        if (!positionQuery.Any(x => x.NameEn == viewEmp.NAMPOSE || x.NameTh == viewEmp.NAMPOST))
                        {
                            var position = new MSTPosition();
                            position.CreatedDate = DateTime.Now;
                            position.ModifiedDate = DateTime.Now;
                            position.AccountId = 1;
                            position.IsActive = true;
                            position.NameEn = (viewEmp.NAMPOSE ?? "").Replace(Environment.NewLine, "").Trim();
                            position.NameTh = (viewEmp.NAMPOST ?? "").Replace(Environment.NewLine, "").Trim();
                            position.CreatedBy = "SYSTEM";
                            position.ModifiedBy = "SYSTEM";
                            position.CompanyCode = "TYM";
                            _context.MSTPositions.InsertOnSubmit(position);
                            _context.SubmitChanges();
                            Log("Positionid: " + position.PositionId + "|PositionNameEn: " + position.NameEn);
                            Console.WriteLine("Positionid: " + position.PositionId + "|PositionNameEn: " + position.NameEn);
                        }
                    }
                    else
                    {
                        Log($"Position is null. CODEMPID : {viewEmp.CODEMPID} , NAMEMPE : {viewEmp.NAMEMPE}");
                    }
                    if (!string.IsNullOrEmpty(viewEmp.NAMCENTENG) && !string.IsNullOrEmpty(viewEmp.NAMCENTTHA))
                    {
                        var deptQuery = _context.MSTDepartments.Where(x => x.NameEn == viewEmp.NAMCENTENG || x.NameTh == viewEmp.NAMCENTTHA);
                        if (!deptQuery.Any(x => x.NameEn == viewEmp.NAMCENTENG || x.NameTh == viewEmp.NAMCENTTHA))
                        {
                            var dept = new MSTDepartment();
                            dept.AccountId = 1;
                            dept.CreatedDate = DateTime.Now;
                            dept.ModifiedDate = DateTime.Now;
                            dept.IsActive = true;
                            dept.NameEn = viewEmp.NAMCENTENG;
                            dept.NameTh = viewEmp.NAMCENTTHA;
                            dept.CreatedBy = "SYSTEM";
                            dept.ModifiedBy = "SYSTEM";
                            dept.CompanyCode = "TYM";
                            dept.DepartmentCode = !string.IsNullOrEmpty(viewEmp.CODCOMP) ? viewEmp.CODCOMP : null;
                            _context.MSTDepartments.InsertOnSubmit(dept);
                            _context.SubmitChanges();
                            Log("Departmentid: " + dept.DepartmentId + "|DepartmentNameEn: " + dept.NameEn);
                            Console.WriteLine("Departmentid: " + dept.DepartmentId + "|DepartmentNameEn: " + dept.NameEn);
                        }
                    }
                    else
                    {
                        Log($"Departmen is null. CODEMPID : {viewEmp.CODEMPID} , NAMEMPE : {viewEmp.NAMEMPE}");
                    }

                    if (!string.IsNullOrEmpty(viewEmp.department_e) && !string.IsNullOrEmpty(viewEmp.department_t))
                    {
                        var divQuery = _context.MSTDivisions.Where(x => x.NameEn == viewEmp.department_e || x.NameTh == viewEmp.department_t);
                        if (!divQuery.Any(x => x.NameEn == viewEmp.department_e || x.NameTh == viewEmp.department_t))
                        {
                            var div = new MSTDivision();
                            div.AccountId = 1;
                            div.CreatedDate = DateTime.Now;
                            div.ModifiedDate = DateTime.Now;
                            div.IsActive = true;
                            div.NameEn = viewEmp.department_e;
                            div.NameTh = viewEmp.department_t;
                            div.CreatedBy = "SYSTEM";
                            div.ModifiedBy = "SYSTEM";
                            _context.MSTDivisions.InsertOnSubmit(div);
                            _context.SubmitChanges();
                            Log("Divisionid: " + div.DivisionId + "|DivisionNameEn: " + div.NameEn);
                            Console.WriteLine("Divisionid: " + div.DivisionId + "|DivisionNameEn: " + div.NameEn);
                        }
                    }
                    else
                    {
                        Log($"Division is null. CODEMPID : {viewEmp.CODEMPID} , NAMEMPE : {viewEmp.NAMEMPE}");
                    }
                }
                Log("---------------------------------------------------------");
                var updates = _context.V_EMPLOYEE_TYMs.ToList().Select(ss => new
                {
                    CODEMPID = !string.IsNullOrEmpty(ss.CODNATNL) && ss.CODNATNL == "01" ? "CN" + ss.CODEMPID : ss.CODEMPID,
                    ss.CODNATNL,
                    ss.NAMEMPT,
                    ss.NAMEMPE,
                    ss.EMAIL,
                    ss.NAMPOSE,
                    ss.NAMCENTENG,
                    ss.NAMPOST,
                    ss.department_e,
                    ss.department_t,
                    ss.codeHead
                }).Where(x => _context.MSTEmployees.Select(s => s.EmployeeCode).Contains(x.CODEMPID)).ToList();

                Console.WriteLine($"EMP UPDATE COUNT : {updates.Count()}");
                Log($"EMP UPDATE COUNT : {updates.Count()}");
                var usersCode = updates.Select(s => s.CODEMPID);
                var empUpdates = _context.MSTEmployees.Where(x => usersCode.Contains(x.EmployeeCode)).ToList();
                var empIsActive = _context.MSTEmployees.Where(x => !usersCode.Contains(x.EmployeeCode)).ToList();

                foreach (var update in empUpdates)
                {
                    var mapper = updates.FirstOrDefault(x => update.EmployeeCode == x.CODEMPID);
                    update.Username = mapper.CODEMPID;
                    update.EmployeeCode = mapper.CODEMPID;
                    update.NameTh = mapper.NAMEMPT;
                    update.NameEn = mapper.NAMEMPE;
                    update.Email = mapper.EMAIL;
                    update.PositionId = _context.MSTPositions.FirstOrDefault(x => x.NameEn == mapper.NAMPOSE || x.NameTh == mapper.NAMPOST)?.PositionId;
                    update.DepartmentId = _context.MSTDepartments.FirstOrDefault(x => x.NameEn == mapper.NAMCENTENG || x.NameTh == mapper.NAMCENTENG)?.DepartmentId;
                    update.DivisionId = _context.MSTDivisions.FirstOrDefault(x => x.NameEn == mapper.department_e || x.NameTh == mapper.department_t)?.DivisionId;
                    update.ModifiedBy = "SYSTEM";
                    update.ModifiedDate = DateTime.Now;

                    update.ReportToEmpCode = _context.MSTEmployees.Where(x => x.EmployeeCode == (!string.IsNullOrEmpty(mapper.CODNATNL) && mapper.CODNATNL == "01" ? "CN" + mapper.codeHead : mapper.codeHead)).FirstOrDefault()?.EmployeeId.ToString() ?? null;
                    if (update.ReportToEmpCode == null)
                    {
                        var checkempcode = _context.MSTEmployees.Where(x => x.EmployeeCode == mapper.codeHead && x.IsActive == true).FirstOrDefault();
                        if (mapper.CODNATNL == "01" && checkempcode != null)
                        {
                            checkempcode.EmployeeCode = "CN" + mapper.codeHead;
                            update.ReportToEmpCode = checkempcode.EmployeeId.ToString();
                        }
                        else
                        {
                            //กรณี Report to ไม่มี user ใน wolf
                            var emphead = updates.FirstOrDefault(x => mapper.codeHead == x.CODEMPID);
                            if (emphead != null)
                            {
                                MSTEmployee Newemp = new MSTEmployee();
                                Newemp.Username = emphead.CODEMPID;
                                Newemp.EmployeeCode = emphead.CODEMPID;
                                Newemp.NameTh = emphead.NAMEMPT;
                                Newemp.NameEn = emphead.NAMEMPT;
                                Newemp.Email = emphead.EMAIL;
                                Newemp.PositionId = _context.MSTPositions.FirstOrDefault(x => x.NameEn == emphead.NAMPOSE || x.NameTh == emphead.NAMPOST)?.PositionId;
                                Newemp.DepartmentId = _context.MSTDepartments.FirstOrDefault(x => x.NameEn == emphead.NAMCENTENG || x.NameTh == emphead.NAMCENTENG)?.DepartmentId;
                                Newemp.DivisionId = _context.MSTDivisions.FirstOrDefault(x => x.NameEn == emphead.department_e || x.NameTh == emphead.department_t)?.DivisionId;
                                Newemp.ReportToEmpCode = _context.MSTEmployees.Where(x => x.EmployeeCode == (!string.IsNullOrEmpty(emphead.CODNATNL) && emphead.CODNATNL == "01" ? "CN" + emphead.codeHead : emphead.codeHead)).FirstOrDefault()?.EmployeeId.ToString() ?? null;
                                if (Newemp.ReportToEmpCode == null)
                                {
                                    Log("Not have Report to !! : 01" + emphead.CODEMPID + "|codeHead :" + emphead.codeHead);
                                    Console.WriteLine("Not have Report to !! : 01" + emphead.CODEMPID + "|codeHead :" + emphead.codeHead);
                                }
                                Newemp.ModifiedBy = "SYSTEM";
                                Newemp.ModifiedDate = DateTime.Now;
                                Newemp.CreatedBy = "SYSTEM";
                                Newemp.CreatedDate = DateTime.Now;
                            }
                            else
                            {
                                Log("Not have Report to !! : 02" + "|codeHead" + mapper.codeHead);
                                Console.WriteLine("Not have Report to !! : 02" + "|codeHead" + mapper.codeHead);

                            }
                        }
                    }
                    _context.SubmitChanges();
                }
                Log("---------------------------------------------------------");


                Log($"EMP ISACTIVE COUNT : {empIsActive.Count()}");
                Console.WriteLine($"EMP ISACTIVE COUNT : {empIsActive.Count()}");
                foreach (var update in empIsActive)
                {
                    update.IsActive = false;
                    _context.SubmitChanges();
                }
                Log("---------------------------------------------------------");
            }

            catch (Exception ex)
            {
                Console.WriteLine("ERROR");
                Console.WriteLine("Exit ERROR");
                Log("ERROR");
                Log("message: " + ex.Message);
                Log("Exit ERROR");
            }
            finally
            {
                Log("====== End Process Process ====== : " + DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss"));
                Thread.Sleep(100000);
            }
        }
    }
}
