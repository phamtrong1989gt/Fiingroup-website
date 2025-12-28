using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using PT.Base;
using PT.Domain.Seedwork;
using PT.Domain.Model;
using PT.Infrastructure;
using PT.Infrastructure.Interfaces;
using PT.Shared;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PT.BE.Areas.Base.Controllers
{
    /// <summary>
    /// Database Query Management Tool - Công cụ quản lý và thực thi câu lệnh SQL
    /// Chỉ dành cho Super Admin với PIN bảo mật
    /// </summary>
    [Area("Base")]
    [IsSupperAdminAuthorizePermission]
    public class DatabaseQueryController : BaseController
    {
        private readonly ApplicationContext _context;
        private readonly IMemoryCache _memoryCache;
        private readonly ILogRepository _logRepository;
        private readonly IConfiguration _configuration;
        private const string PIN_CACHE_KEY = "DBQuery_PIN_";
        private const string DEFAULT_PIN = "DB_ADMIN_2025"; // PIN cố định trong code
        private const int MAX_QUERY_TIMEOUT = 300; // 5 phút
        private const int MAX_ROWS_RETURN = 5000; // Giới hạn số dòng trả về

        public DatabaseQueryController(
            ApplicationContext context,
            IMemoryCache memoryCache,
            ILogRepository logRepository,
            IConfiguration configuration)
        {
            _context = context;
            _memoryCache = memoryCache;
            _logRepository = logRepository;
            _configuration = configuration;
            controllerName = "DatabaseQuery";
            tableName = "DatabaseQuery";
        }

        /// <summary>
        /// Trang chính - Hiển thị giao diện query tool
        /// </summary>
        [HttpGet]
        public IActionResult Index()
        {
            // Kiểm tra xem user đã xác thực PIN chưa
            var userId = DataUserInfo.UserId;
            var isPinVerified = _memoryCache.TryGetValue($"{PIN_CACHE_KEY}{userId}", out bool _);
            
            ViewBag.IsPinVerified = isPinVerified;
            ViewBag.UserName = DataUserInfo.DisplayName;
            
            try
            {
                // Lấy database name từ connection string
                var connectionString = _configuration.GetConnectionString("DefaultConnection");
                var builder = new SqlConnectionStringBuilder(connectionString);
                ViewBag.DatabaseName = builder.InitialCatalog;
            }
            catch (Exception ex)
            {
                ViewBag.DatabaseName = "Unknown";
                // Log lỗi nếu cần
                System.Diagnostics.Debug.WriteLine($"Error getting database name: {ex.Message}");
            }
            
            return View();
        }

        /// <summary>
        /// Xác thực PIN code để sử dụng query tool
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> VerifyPin([FromBody] PinVerifyModel model)
        {
            try
            {
                var userId = DataUserInfo.UserId;
                
                if (model.Pin != DEFAULT_PIN)
                {
                    // Log failed attempt
                    await AddLog(new LogModel
                    {
                        ObjectId = userId,
                        Name = "PIN xác thực thất bại - Database Query Tool",
                        Type = LogType.Control
                    });
                    
                    return Json(CreateResponse(-1, "PIN không chính xác!", "danger"));
                }

                // Lưu trạng thái đã xác thực trong cache (hết hạn sau 2 giờ)
                _memoryCache.Set($"{PIN_CACHE_KEY}{userId}", true, TimeSpan.FromHours(2));

                // Log successful verification
                await AddLog(new LogModel
                {
                    ObjectId = userId,
                    Name = "PIN xác thực thành công - Database Query Tool",
                    Type = LogType.Control
                });

                return Json(CreateResponse(1, "Xác thực thành công!", "success"));
            }
            catch (Exception ex)
            {
                return Json(CreateResponse(-1, $"Lỗi: {ex.Message}", "danger"));
            }
        }

        /// <summary>
        /// Thực thi câu lệnh SQL query
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> ExecuteQuery([FromBody] QueryExecuteModel model)
        {
            try
            {
                var userId = DataUserInfo.UserId;
                
                // Kiểm tra xác thực PIN
                var isPinVerified = _memoryCache.TryGetValue($"{PIN_CACHE_KEY}{userId}", out bool _);
                if (!isPinVerified)
                {
                    return Json(CreateResponse(-1, "Vui lòng xác thực PIN trước!", "danger"));
                }

                if (string.IsNullOrWhiteSpace(model.Query))
                {
                    return Json(CreateResponse(-1, "Câu lệnh SQL không được để trống!", "warning"));
                }

                // Validate query (chặn các câu lệnh nguy hiểm)
                var dangerousKeywords = new[] { "DROP DATABASE", "DROP SCHEMA", "sp_", "xp_" };
                var upperQuery = model.Query.ToUpper();
                
                if (dangerousKeywords.Any(keyword => upperQuery.Contains(keyword)))
                {
                    await AddLog(new LogModel
                    {
                        ObjectId = userId,
                        Name = $"Cố gắng thực thi lệnh nguy hiểm: {model.Query.Substring(0, Math.Min(100, model.Query.Length))}",
                        Type = LogType.Control
                    });
                    
                    return Json(CreateResponse(-1, "Câu lệnh chứa từ khóa nguy hiểm bị chặn!", "danger"));
                }

                var startTime = DateTime.Now;
                var result = new QueryResultModel();

                // Sử dụng SqlConnection trực tiếp
                var connectionString = _configuration.GetConnectionString("DefaultConnection");
                using (var connection = new SqlConnection(connectionString))
                {
                    await connection.OpenAsync();
                    
                    using (var command = new SqlCommand(model.Query, connection))
                    {
                        command.CommandTimeout = model.Timeout > 0 && model.Timeout <= MAX_QUERY_TIMEOUT 
                            ? model.Timeout 
                            : 30; // Default 30s

                        // Kiểm tra loại query (SELECT vs. DML)
                        if (upperQuery.TrimStart().StartsWith("SELECT") || 
                            upperQuery.TrimStart().StartsWith("WITH") ||
                            upperQuery.TrimStart().StartsWith("EXEC") && upperQuery.Contains("SELECT"))
                        {
                            // Query trả về kết quả
                            using (var reader = await command.ExecuteReaderAsync())
                            {
                                result.Columns = new List<string>();
                                for (int i = 0; i < reader.FieldCount; i++)
                                {
                                    result.Columns.Add(reader.GetName(i));
                                }

                                result.Rows = new List<List<object>>();
                                int rowCount = 0;
                                
                                while (await reader.ReadAsync() && rowCount < MAX_ROWS_RETURN)
                                {
                                    var row = new List<object>();
                                    for (int i = 0; i < reader.FieldCount; i++)
                                    {
                                        row.Add(reader.IsDBNull(i) ? null : reader.GetValue(i));
                                    }
                                    result.Rows.Add(row);
                                    rowCount++;
                                }

                                result.RowCount = rowCount;
                                result.IsLimitedResult = rowCount >= MAX_ROWS_RETURN;
                            }
                        }
                        else
                        {
                            // DML commands (INSERT, UPDATE, DELETE)
                            var affectedRows = await command.ExecuteNonQueryAsync();
                            result.RowCount = affectedRows;
                            result.Message = $"Thực thi thành công. {affectedRows} dòng bị ảnh hưởng.";
                        }
                    }
                }

                var executionTime = (DateTime.Now - startTime).TotalSeconds;
                result.ExecutionTime = Math.Round(executionTime, 3);

                // Log query execution
                await AddLog(new LogModel
                {
                    ObjectId = userId,
                    Name = $"Thực thi query ({executionTime:F3}s): {model.Query.Substring(0, Math.Min(200, model.Query.Length))}",
                    Type = LogType.Control
                });

                return Json(new
                {
                    output = 1,
                    message = "Thực thi thành công!",
                    type = "success",
                    data = result
                });
            }
            catch (SqlException sqlEx)
            {
                await AddLog(new LogModel
                {
                    ObjectId = DataUserInfo.UserId,
                    Name = $"Lỗi SQL: {sqlEx.Message} - Query: {model.Query.Substring(0, Math.Min(100, model.Query.Length))}",
                    Type = LogType.Control
                });
                
                return Json(new
                {
                    output = -1,
                    message = $"Lỗi SQL: {sqlEx.Message}",
                    type = "danger",
                    errorLine = sqlEx.LineNumber
                });
            }
            catch (Exception ex)
            {
                return Json(CreateResponse(-1, $"Lỗi: {ex.Message}", "danger"));
            }
        }

        /// <summary>
        /// Lấy danh sách tables trong database
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetTables()
        {
            try
            {
                var userId = DataUserInfo.UserId;
                var isPinVerified = _memoryCache.TryGetValue($"{PIN_CACHE_KEY}{userId}", out bool _);
                
                if (!isPinVerified)
                {
                    return Json(CreateResponse(-1, "Vui lòng xác thực PIN trước!", "danger"));
                }

                var query = @"
                    SELECT 
                        s.name AS SchemaName,
                        t.name AS TableName,
                        SUM(p.rows) AS [RowCount]
                    FROM sys.tables t
                    INNER JOIN sys.schemas s ON t.schema_id = s.schema_id
                    INNER JOIN sys.partitions p ON t.object_id = p.object_id
                    WHERE p.index_id IN (0,1)
                    GROUP BY s.name, t.name
                    ORDER BY s.name, t.name";

                var tables = new List<TableInfoModel>();
                
                // Sử dụng SqlConnection trực tiếp
                var connectionString = _configuration.GetConnectionString("DefaultConnection");
                using (var connection = new SqlConnection(connectionString))
                {
                    await connection.OpenAsync();
                    
                    using (var command = new SqlCommand(query, connection))
                    {
                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                tables.Add(new TableInfoModel
                                {
                                    SchemaName = reader.GetString(0),
                                    TableName = reader.GetString(1),
                                    RowCount = reader.IsDBNull(2) ? 0 : reader.GetInt64(2)
                                });
                            }
                        }
                    }
                }

                return Json(new { output = 1, data = tables });
            }
            catch (Exception ex)
            {
                return Json(CreateResponse(-1, $"Lỗi: {ex.Message}", "danger"));
            }
        }

        /// <summary>
        /// Gen script CREATE TABLE cho một table cụ thể
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GenerateTableScript(string schemaName, string tableName)
        {
            try
            {
                var userId = DataUserInfo.UserId;
                var isPinVerified = _memoryCache.TryGetValue($"{PIN_CACHE_KEY}{userId}", out bool _);
                
                if (!isPinVerified)
                {
                    return Json(CreateResponse(-1, "Vui lòng xác thực PIN trước!", "danger"));
                }

                var script = new StringBuilder();
                
                // Lấy connection string từ configuration
                var connectionString = _configuration.GetConnectionString("DefaultConnection");
                
                using (var connection = new SqlConnection(connectionString))
                {
                    await connection.OpenAsync();
                    
                    // Generate CREATE TABLE script
                    var createTableQuery = $@"
                        DECLARE @TableName NVARCHAR(MAX) = '[{schemaName}].[{tableName}]'
                        DECLARE @SQL NVARCHAR(MAX) = ''
                        
                        SELECT @SQL = 'CREATE TABLE ' + @TableName + ' (' + CHAR(13) + CHAR(10)
                        
                        SELECT @SQL = @SQL + '    [' + c.name + '] ' + t.name + 
                            CASE 
                                WHEN t.name IN ('varchar', 'char', 'nvarchar', 'nchar') 
                                THEN '(' + CASE WHEN c.max_length = -1 THEN 'MAX' 
                                          WHEN t.name LIKE 'n%' THEN CAST(c.max_length/2 AS VARCHAR(10))
                                          ELSE CAST(c.max_length AS VARCHAR(10)) END + ')'
                                WHEN t.name IN ('decimal', 'numeric')
                                THEN '(' + CAST(c.precision AS VARCHAR(10)) + ',' + CAST(c.scale AS VARCHAR(10)) + ')'
                                ELSE ''
                            END +
                            CASE WHEN c.is_nullable = 0 THEN ' NOT NULL' ELSE ' NULL' END + ',' + CHAR(13) + CHAR(10)
                        FROM sys.columns c
                        INNER JOIN sys.types t ON c.user_type_id = t.user_type_id
                        WHERE c.object_id = OBJECT_ID(@TableName)
                        ORDER BY c.column_id
                        
                        SELECT @SQL = LEFT(@SQL, LEN(@SQL) - 3) + CHAR(13) + CHAR(10) + ');'
                        SELECT @SQL AS CreateScript";

                    using (var command = new SqlCommand(createTableQuery, connection))
                    {
                        var result = await command.ExecuteScalarAsync();
                        script.AppendLine(result?.ToString() ?? "");
                    }
                }

                await AddLog(new LogModel
                {
                    ObjectId = userId,
                    Name = $"Gen CREATE script cho table: [{schemaName}].[{tableName}]",
                    Type = LogType.Control
                });

                return Json(new { output = 1, data = script.ToString() });
            }
            catch (Exception ex)
            {
                return Json(CreateResponse(-1, $"Lỗi: {ex.Message}", "danger"));
            }
        }

        /// <summary>
        /// Gen script INSERT DATA cho một table
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> GenerateDataScript([FromBody] GenerateScriptModel model)
        {
            try
            {
                var userId = DataUserInfo.UserId;
                var isPinVerified = _memoryCache.TryGetValue($"{PIN_CACHE_KEY}{userId}", out bool _);
                
                if (!isPinVerified)
                {
                    return Json(CreateResponse(-1, "Vui lòng xác thực PIN trước!", "danger"));
                }

                var script = new StringBuilder();
                var tableName = $"[{model.SchemaName}].[{model.TableName}]";
                
                // Lấy connection string từ configuration
                var connectionString = _configuration.GetConnectionString("DefaultConnection");
                
                using (var connection = new SqlConnection(connectionString))
                {
                    await connection.OpenAsync();
                    
                    // Get data
                    var selectQuery = $"SELECT TOP {Math.Min(model.RowLimit, 10000)} * FROM {tableName}";
                    
                    using (var command = new SqlCommand(selectQuery, connection))
                    {
                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            // Get column names
                            var columns = new List<string>();
                            var columnTypes = new List<string>();
                            
                            for (int i = 0; i < reader.FieldCount; i++)
                            {
                                columns.Add(reader.GetName(i));
                                columnTypes.Add(reader.GetFieldType(i).Name);
                            }
                            
                            // Generate INSERT statements
                            while (await reader.ReadAsync())
                            {
                                script.Append($"INSERT INTO {tableName} (");
                                script.Append(string.Join(", ", columns.Select(c => $"[{c}]")));
                                script.Append(") VALUES (");
                                
                                var values = new List<string>();
                                for (int i = 0; i < reader.FieldCount; i++)
                                {
                                    if (reader.IsDBNull(i))
                                    {
                                        values.Add("NULL");
                                    }
                                    else
                                    {
                                        var value = reader.GetValue(i);
                                        var type = columnTypes[i];
                                        
                                        if (type == "String" || type == "Guid")
                                        {
                                            values.Add($"N'{value.ToString().Replace("'", "''")}'");
                                        }
                                        else if (type == "DateTime")
                                        {
                                            var dt = (DateTime)value;
                                            values.Add($"'{dt:yyyy-MM-dd HH:mm:ss.fff}'");
                                        }
                                        else if (type == "Boolean")
                                        {
                                            values.Add((bool)value ? "1" : "0");
                                        }
                                        else if (type == "Byte[]")
                                        {
                                            var bytes = (byte[])value;
                                            values.Add($"0x{BitConverter.ToString(bytes).Replace("-", "")}");
                                        }
                                        else
                                        {
                                            values.Add(value.ToString());
                                        }
                                    }
                                }
                                
                                script.Append(string.Join(", ", values));
                                script.AppendLine(");");
                            }
                        }
                    }
                }

                await AddLog(new LogModel
                {
                    ObjectId = userId,
                    Name = $"Gen INSERT script cho table: {tableName} ({model.RowLimit} rows)",
                    Type = LogType.Control
                });

                return Json(new { output = 1, data = script.ToString() });
            }
            catch (Exception ex)
            {
                return Json(CreateResponse(-1, $"Lỗi: {ex.Message}", "danger"));
            }
        }

        /// <summary>
        /// Backup database (chỉ generate script BACKUP)
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GenerateBackupScript()
        {
            try
            {
                var userId = DataUserInfo.UserId;
                var isPinVerified = _memoryCache.TryGetValue($"{PIN_CACHE_KEY}{userId}", out bool _);
                
                if (!isPinVerified)
                {
                    return Json(CreateResponse(-1, "Vui lòng xác thực PIN trước!", "danger"));
                }

                string dbName = "Unknown";
                
                try
                {
                    // Lấy connection string và parse database name
                    var connectionString = _configuration.GetConnectionString("DefaultConnection");
                    var builder = new SqlConnectionStringBuilder(connectionString);
                    dbName = builder.InitialCatalog;
                }
                catch
                {
                    // Fallback: thử lấy từ EF context
                    var connection = _context.Database.GetDbConnection();
                    if (connection.State != ConnectionState.Open)
                    {
                        await connection.OpenAsync();
                    }
                    dbName = connection.Database;
                }

                var backupPath = $"C:\\Backups\\{dbName}_{DateTime.Now:yyyyMMddHHmmss}.bak";
                
                var script = $@"-- =============================================
-- Database Backup Script
-- Database: {dbName}
-- Generated: {DateTime.Now:yyyy-MM-dd HH:mm:ss}
-- =============================================

-- Lưu ý: 
-- 1. Đường dẫn backup phải tồn tại trên SQL Server
-- 2. Tài khoản SQL Server phải có quyền ghi vào thư mục backup
-- 3. Thay đổi đường dẫn phù hợp với môi trường của bạn

-- Tạo thư mục backup (nếu chưa tồn tại)
EXEC xp_create_subdir 'C:\Backups';
GO

-- Thực hiện backup
BACKUP DATABASE [{dbName}] 
TO DISK = N'{backupPath}'
WITH FORMAT, 
     MEDIANAME = '{dbName}_Backup',
     NAME = '{dbName} - Full Database Backup {DateTime.Now:yyyy-MM-dd HH:mm:ss}',
     STATS = 10,
     COMPRESSION;
GO

-- Kiểm tra backup
RESTORE VERIFYONLY FROM DISK = N'{backupPath}';
GO

-- =============================================
-- Thông tin backup
-- =============================================
-- File backup: {backupPath}
-- Để restore backup, sử dụng lệnh:
-- RESTORE DATABASE [{dbName}] FROM DISK = N'{backupPath}' WITH REPLACE;
-- =============================================";

                await AddLog(new LogModel
                {
                    ObjectId = userId,
                    Name = $"Gen backup script cho database: {dbName}",
                    Type = LogType.Control
                });

                return Json(new { output = 1, data = script, fileName = $"{dbName}_backup.sql" });
            }
            catch (Exception ex)
            {
                return Json(CreateResponse(-1, $"Lỗi: {ex.Message}", "danger"));
            }
        }

        /// <summary>
        /// Lấy cấu trúc của table
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetTableSchema(string schemaName, string tableName)
        {
            try
            {
                var userId = DataUserInfo.UserId;
                var isPinVerified = _memoryCache.TryGetValue($"{PIN_CACHE_KEY}{userId}", out bool _);
                
                if (!isPinVerified)
                {
                    return Json(CreateResponse(-1, "Vui lòng xác thực PIN trước!", "danger"));
                }

                var query = @"
                    SELECT 
                        c.name AS ColumnName,
                        t.name AS DataType,
                        c.max_length AS MaxLength,
                        c.is_nullable AS IsNullable,
                        CAST(CASE WHEN pk.column_id IS NOT NULL THEN 1 ELSE 0 END AS BIT) AS IsPrimaryKey
                    FROM sys.columns c
                    INNER JOIN sys.types t ON c.user_type_id = t.user_type_id
                    LEFT JOIN sys.index_columns pk ON c.object_id = pk.object_id AND c.column_id = pk.column_id
                        AND pk.index_id = (SELECT index_id FROM sys.indexes WHERE object_id = c.object_id AND is_primary_key = 1)
                    WHERE c.object_id = OBJECT_ID(@tableName)
                    ORDER BY c.column_id";

                var columns = new List<ColumnInfoModel>();
                
                // Sử dụng SqlConnection trực tiếp
                var connectionString = _configuration.GetConnectionString("DefaultConnection");
                using (var connection = new SqlConnection(connectionString))
                {
                    await connection.OpenAsync();
                    
                    using (var command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@tableName", $"{schemaName}.{tableName}");

                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                columns.Add(new ColumnInfoModel
                                {
                                    ColumnName = reader.GetString(0),
                                    DataType = reader.GetString(1),
                                    MaxLength = reader.IsDBNull(2) ? (int?)null : reader.GetInt16(2),
                                    IsNullable = reader.GetBoolean(3),
                                    IsPrimaryKey = reader.GetBoolean(4)
                                });
                            }
                        }
                    }
                }

                return Json(new { output = 1, data = columns });
            }
            catch (Exception ex)
            {
                return Json(CreateResponse(-1, $"Lỗi: {ex.Message}", "danger"));
            }
        }

        /// <summary>
        /// Export kết quả ra CSV
        /// </summary>
        [HttpPost]
        public IActionResult ExportCsv([FromBody] ExportCsvModel model)
        {
            try
            {
                var csv = new StringBuilder();
                
                // Header
                csv.AppendLine(string.Join(",", model.Columns.Select(c => $"\"{c}\"")));
                
                // Rows
                foreach (var row in model.Rows)
                {
                    csv.AppendLine(string.Join(",", row.Select(cell => 
                        $"\"{(cell?.ToString() ?? "NULL").Replace("\"", "\"\"")}\"")));
                }

                var bytes = Encoding.UTF8.GetBytes(csv.ToString());
                var fileName = $"QueryResult_{DateTime.Now:yyyyMMddHHmmss}.csv";
                
                return File(bytes, "text/csv", fileName);
            }
            catch (Exception ex)
            {
                return Json(CreateResponse(-1, $"Lỗi: {ex.Message}", "danger"));
            }
        }

        /// <summary>
        /// Lấy lịch sử query
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetQueryHistory()
        {
            try
            {
                var userId = DataUserInfo.UserId;
                var isPinVerified = _memoryCache.TryGetValue($"{PIN_CACHE_KEY}{userId}", out bool _);
                
                if (!isPinVerified)
                {
                    return Json(CreateResponse(-1, "Vui lòng xác thực PIN trước!", "danger"));
                }

                var logs = await _logRepository.SearchAsync(
                    false,
                    1,
                    50,
                    x => x.ObjectType == "DatabaseQuery" && x.Type == LogType.Control
                );

                var history = logs.Select(l => new
                {
                    l.ActionTime,
                    l.Name,
                    User = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, object>>(l.AcctionUser ?? "{}")
                }).OrderByDescending(x => x.ActionTime).ToList();

                return Json(new { output = 1, data = history });
            }
            catch (Exception ex)
            {
                return Json(CreateResponse(-1, $"Lỗi: {ex.Message}", "danger"));
            }
        }

        /// <summary>
        /// Cập nhật một row trong table
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> UpdateRow([FromBody] UpdateRowModel model)
        {
            try
            {
                var userId = DataUserInfo.UserId;
                var isPinVerified = _memoryCache.TryGetValue($"{PIN_CACHE_KEY}{userId}", out bool _);
                
                if (!isPinVerified)
                {
                    return Json(CreateResponse(-1, "Vui lòng xác thực PIN trước!", "danger"));
                }

                var tableName = $"[{model.SchemaName}].[{model.TableName}]";
                var connectionString = _configuration.GetConnectionString("DefaultConnection");
                
                using (var connection = new SqlConnection(connectionString))
                {
                    await connection.OpenAsync();
                    
                    // Build UPDATE query
                    var setClauses = new List<string>();
                    foreach (var kvp in model.Values)
                    {
                        setClauses.Add($"[{kvp.Key}] = @{kvp.Key}");
                    }
                    
                    var whereClauses = new List<string>();
                    foreach (var kvp in model.WhereConditions)
                    {
                        whereClauses.Add($"[{kvp.Key}] = @Where_{kvp.Key}");
                    }
                    
                    var updateQuery = $@"
                        UPDATE {tableName} 
                        SET {string.Join(", ", setClauses)}
                        WHERE {string.Join(" AND ", whereClauses)}";
                    
                    using (var command = new SqlCommand(updateQuery, connection))
                    {
                        // Add SET parameters
                        foreach (var kvp in model.Values)
                        {
                            command.Parameters.AddWithValue($"@{kvp.Key}", kvp.Value ?? DBNull.Value);
                        }
                        
                        // Add WHERE parameters
                        foreach (var kvp in model.WhereConditions)
                        {
                            command.Parameters.AddWithValue($"@Where_{kvp.Key}", kvp.Value ?? DBNull.Value);
                        }
                        
                        var affectedRows = await command.ExecuteNonQueryAsync();
                        
                        await AddLog(new LogModel
                        {
                            ObjectId = userId,
                            Name = $"Update row trong table: {tableName}",
                            Type = LogType.Control
                        });
                        
                        return Json(CreateResponse(1, $"Cập nhật thành công {affectedRows} dòng!", "success"));
                    }
                }
            }
            catch (Exception ex)
            {
                return Json(CreateResponse(-1, $"Lỗi: {ex.Message}", "danger"));
            }
        }

        /// <summary>
        /// Xóa một row trong table
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> DeleteRow([FromBody] DeleteRowModel model)
        {
            try
            {
                var userId = DataUserInfo.UserId;
                var isPinVerified = _memoryCache.TryGetValue($"{PIN_CACHE_KEY}{userId}", out bool _);
                
                if (!isPinVerified)
                {
                    return Json(CreateResponse(-1, "Vui lòng xác thực PIN trước!", "danger"));
                }

                var tableName = $"[{model.SchemaName}].[{model.TableName}]";
                var connectionString = _configuration.GetConnectionString("DefaultConnection");
                
                using (var connection = new SqlConnection(connectionString))
                {
                    await connection.OpenAsync();
                    
                    var whereClauses = new List<string>();
                    foreach (var kvp in model.WhereConditions)
                    {
                        whereClauses.Add($"[{kvp.Key}] = @{kvp.Key}");
                    }
                    
                    var deleteQuery = $@"
                        DELETE FROM {tableName} 
                        WHERE {string.Join(" AND ", whereClauses)}";
                    
                    using (var command = new SqlCommand(deleteQuery, connection))
                    {
                        foreach (var kvp in model.WhereConditions)
                        {
                            command.Parameters.AddWithValue($"@{kvp.Key}", kvp.Value ?? DBNull.Value);
                        }
                        
                        var affectedRows = await command.ExecuteNonQueryAsync();
                        
                        await AddLog(new LogModel
                        {
                            ObjectId = userId,
                            Name = $"Delete row trong table: {tableName}",
                            Type = LogType.Control
                        });
                        
                        return Json(CreateResponse(1, $"Xóa thành công {affectedRows} dòng!", "success"));
                    }
                }
            }
            catch (Exception ex)
            {
                return Json(CreateResponse(-1, $"Lỗi: {ex.Message}", "danger"));
            }
        }

        /// <summary>
        /// Generate INSERT scripts cho nhiều tables (Backup Database)
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> GenerateFullBackupScript([FromBody] FullBackupModel model)
        {
            try
            {
                var userId = DataUserInfo.UserId;
                var isPinVerified = _memoryCache.TryGetValue($"{PIN_CACHE_KEY}{userId}", out bool _);
                
                if (!isPinVerified)
                {
                    return Json(CreateResponse(-1, "Vui lòng xác thực PIN trước!", "danger"));
                }

                var script = new StringBuilder();
                var connectionString = _configuration.GetConnectionString("DefaultConnection");
                
                // Header
                script.AppendLine("-- =============================================");
                script.AppendLine("-- FULL DATABASE BACKUP SCRIPT");
                script.AppendLine($"-- Generated: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
                script.AppendLine($"-- Tables: {model.Tables.Count}");
                script.AppendLine("-- =============================================");
                script.AppendLine();
                
                using (var connection = new SqlConnection(connectionString))
                {
                    await connection.OpenAsync();
                    
                    foreach (var table in model.Tables)
                    {
                        var tableName = $"[{table.SchemaName}].[{table.TableName}]";
                        
                        script.AppendLine($"-- =============================================");
                        script.AppendLine($"-- Table: {tableName}");
                        script.AppendLine($"-- =============================================");
                        
                        // Disable constraints
                        script.AppendLine($"ALTER TABLE {tableName} NOCHECK CONSTRAINT ALL;");
                        script.AppendLine();
                        
                        // Get data
                        var selectQuery = $"SELECT TOP {table.RowLimit} * FROM {tableName}";
                        
                        using (var command = new SqlCommand(selectQuery, connection))
                        {
                            using (var reader = await command.ExecuteReaderAsync())
                            {
                                var columns = new List<string>();
                                var columnTypes = new List<string>();
                                
                                for (int i = 0; i < reader.FieldCount; i++)
                                {
                                    columns.Add(reader.GetName(i));
                                    columnTypes.Add(reader.GetFieldType(i).Name);
                                }
                                
                                int rowCount = 0;
                                while (await reader.ReadAsync())
                                {
                                    script.Append($"INSERT INTO {tableName} (");
                                    script.Append(string.Join(", ", columns.Select(c => $"[{c}]")));
                                    script.Append(") VALUES (");
                                    
                                    var values = new List<string>();
                                    for (int i = 0; i < reader.FieldCount; i++)
                                    {
                                        if (reader.IsDBNull(i))
                                        {
                                            values.Add("NULL");
                                        }
                                        else
                                        {
                                            var value = reader.GetValue(i);
                                            var type = columnTypes[i];
                                            
                                            if (type == "String" || type == "Guid")
                                            {
                                                values.Add($"N'{value.ToString().Replace("'", "''")}'");
                                            }
                                            else if (type == "DateTime")
                                            {
                                                var dt = (DateTime)value;
                                                values.Add($"'{dt:yyyy-MM-dd HH:mm:ss.fff}'");
                                            }
                                            else if (type == "Boolean")
                                            {
                                                values.Add((bool)value ? "1" : "0");
                                            }
                                            else if (type == "Byte[]")
                                            {
                                                var bytes = (byte[])value;
                                                if (bytes.Length > 0)
                                                {
                                                    values.Add($"0x{BitConverter.ToString(bytes).Replace("-", "")}");
                                                }
                                                else
                                                {
                                                    values.Add("NULL");
                                                }
                                            }
                                            else
                                            {
                                                values.Add(value.ToString());
                                            }
                                        }
                                    }
                                    
                                    script.Append(string.Join(", ", values));
                                    script.AppendLine(");");
                                    rowCount++;
                                }
                                
                                script.AppendLine($"-- Inserted {rowCount} rows");
                            }
                        }
                        
                        // Enable constraints
                        script.AppendLine($"ALTER TABLE {tableName} CHECK CONSTRAINT ALL;");
                        script.AppendLine();
                        script.AppendLine();
                    }
                }
                
                script.AppendLine("-- =============================================");
                script.AppendLine("-- BACKUP COMPLETED");
                script.AppendLine("-- =============================================");

                await AddLog(new LogModel
                {
                    ObjectId = userId,
                    Name = $"Generate full backup script cho {model.Tables.Count} tables",
                    Type = LogType.Control
                });

                return Json(new { 
                    output = 1, 
                    data = script.ToString(),
                    fileName = $"FullBackup_{DateTime.Now:yyyyMMddHHmmss}.sql"
                });
            }
            catch (Exception ex)
            {
                return Json(CreateResponse(-1, $"Lỗi: {ex.Message}", "danger"));
            }
        }

        /// <summary>
        /// Revoke PIN (đăng xuất khỏi query tool)
        /// </summary>
        [HttpPost]
        public IActionResult RevokePin()
        {
            var userId = DataUserInfo.UserId;
            _memoryCache.Remove($"{PIN_CACHE_KEY}{userId}");
            return Json(CreateResponse(1, "Đã đăng xuất khỏi Database Query Tool", "success"));
        }
    }

    #region Models
    public class PinVerifyModel
    {
        public string Pin { get; set; }
    }

    public class QueryExecuteModel
    {
        public string Query { get; set; }
        public int Timeout { get; set; } = 30;
    }

    public class QueryResultModel
    {
        public List<string> Columns { get; set; }
        public List<List<object>> Rows { get; set; }
        public int RowCount { get; set; }
        public double ExecutionTime { get; set; }
        public string Message { get; set; }
        public bool IsLimitedResult { get; set; }
    }

    public class TableInfoModel
    {
        public string SchemaName { get; set; }
        public string TableName { get; set; }
        public long RowCount { get; set; }
    }

    public class ColumnInfoModel
    {
        public string ColumnName { get; set; }
        public string DataType { get; set; }
        public int? MaxLength { get; set; }
        public bool IsNullable { get; set; }
        public bool IsPrimaryKey { get; set; }
    }

    public class ExportCsvModel
    {
        public List<string> Columns { get; set; }
        public List<List<object>> Rows { get; set; }
    }

    public class GenerateScriptModel
    {
        public string SchemaName { get; set; }
        public string TableName { get; set; }
        public int RowLimit { get; set; } = 1000;
    }

    public class UpdateRowModel
    {
        public string SchemaName { get; set; }
        public string TableName { get; set; }
        public Dictionary<string, object> Values { get; set; }
        public Dictionary<string, object> WhereConditions { get; set; }
    }

    public class DeleteRowModel
    {
        public string SchemaName { get; set; }
        public string TableName { get; set; }
        public Dictionary<string, object> WhereConditions { get; set; }
    }

    public class FullBackupModel
    {
        public List<TableBackupInfo> Tables { get; set; }
    }

    public class TableBackupInfo
    {
        public string SchemaName { get; set; }
        public string TableName { get; set; }
        public int RowLimit { get; set; } = 10000;
    }
    #endregion
}
