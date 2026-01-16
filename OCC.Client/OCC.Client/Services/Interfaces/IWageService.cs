using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using OCC.Shared.Models;

namespace OCC.Client.Services.Interfaces
{
    public interface IWageService
    {
        Task<IEnumerable<WageRun>> GetWageRunsAsync();
        Task<WageRun?> GetWageRunByIdAsync(Guid id);
        Task<WageRun> GenerateDraftRunAsync(DateTime startDate, DateTime endDate, string? notes = null);
        Task FinalizeRunAsync(Guid id);
        Task DeleteRunAsync(Guid id);
    }
}
