using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using LegionTDServerReborn.Models;
using LegionTDServerReborn.Extensions;
using Microsoft.Extensions.Caching.Memory;
using System.IO;
using System.Text;
using System.Net;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using System;
using LegionTDServerReborn.Utils;

namespace LegionTDServerReborn.Services
{
    public class FileLogger
    {
        private readonly string _logFolder;

        public FileLogger(IConfiguration configuration)
        {
            _logFolder = configuration["logFolder"];
        }

        public async Task<string> LogToFile(string folder, IDictionary<string, object> toLog)
        {
            var now = DateTime.UtcNow;
            var dir = Path.Combine(_logFolder, folder);
            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }
            var fileName = $"{now:yyyy_MM_dd__HH_mm_ss_ff}.log";
            var filePath = Path.Combine(dir, fileName);
            if (File.Exists(filePath))
            {
                LoggingUtil.Error($"Log {filePath} already exists!");
                return null;
            }
            var fileContent = new StringBuilder();
            foreach (var (key, value) in toLog)
            {
                fileContent.Append($"{key}:{value}\n\n");
            }
            await File.WriteAllTextAsync(filePath, fileContent.ToString());
            return filePath;
        }
    }
}
