using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using backend.Models;

namespace backend.Services.ClarifyGoAutoLogin
{
    public interface IClarifyGoAutoLoginService
    {
        Task<bool> LoginAsync(string username, string password);
        Task<List<AuditLog>> FetchAuditLogsAsync(string fromDate, string toDate);
    }
}
