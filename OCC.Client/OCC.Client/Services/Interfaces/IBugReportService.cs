using OCC.Shared.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OCC.Client.Services.Interfaces
{
    public interface IBugReportService
    {
        Task SubmitBugAsync(BugReport report);
        Task<List<BugReport>> GetBugReportsAsync();
        Task AddCommentAsync(Guid bugId, string comment, string? status);
        Task DeleteBugAsync(Guid bugId);
    }
}
