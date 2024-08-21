using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Mindflur.IMS.Application.Contracts.Repository;
using Mindflur.IMS.Application.ViewModel.View;
using Mindflur.IMS.Data.Models.Custom;

namespace Mindflur.IMS.Data.Repository
{
    public class ChecklistRepository : IChecklistRepository
    {
       
        private readonly IConfiguration _configuration;

        public ChecklistRepository(IConfiguration configuration, ILogger<ChartRepository> logger)
        {
            _configuration = configuration;
            
        }

        public async Task<IEnumerable<CheccklistItemData>> GetChecklistAsync(int auditProgramId)
        {
            using var conn = new SqlConnection(_configuration.GetConnectionString("DataConnectionString"));

            conn.Open();

            return await conn.QueryAsync<CheccklistItemData>(
                        @"select cm.Id as ClauseId, cm.ClauseNo as ClauseTitle, cr.ChecklistMasterId as QuestionId, cqm.Questions as QuestionText, cr.Comments as Comments, cr.MasterDataClassificationId as ClassificationId, md.Items as ClassificationTitle
                        from ChecklistMaster as cqm
                        join AuditChecklist as cr on cqm.Id = cr.ChecklistMasterId
                        left join ClauseMaster as cm ON cm.Id = cqm.ClauseMasterId
                        left join MasterData as md on md.Id = cr.MasterDataClassificationId
                        join AuditProgram as ap on ap.Id = cr.AuditProgramId
                        where ap.Id = @auditProgramId order by cm.ClauseNo", new { auditProgramId });
        }

        public async Task<ChecklistSeverityView> GetChecklistSeverityForReport(int auditId, int severityId)
        {
            using var conn = new SqlConnection(_configuration.GetConnectionString("DataConnectionString"));

            conn.Open();
            return await conn.QueryFirstOrDefaultAsync<ChecklistSeverityView>(
                        @"Select Items as Severity , COUNT(*) as Total
                                 from AuditChecklist as ac 
                                 Join MasterData as md on ac.MasterDataClassificationId = md.Id
                                 join AuditProgram as ap on ac.AuditProgramId = ap.Id
                                 where ac.AuditProgramId = @auditId and md.Id = @severityId
                                group by Items", new { auditId, severityId });
        }
    }
}