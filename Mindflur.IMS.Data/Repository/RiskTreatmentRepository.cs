using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Mindflur.IMS.Application.Contracts.Repository;
using Mindflur.IMS.Application.ViewModel.Core;
using Mindflur.IMS.Application.ViewModel.View;
using Mindflur.IMS.Data.Base;
using Mindflur.IMS.Data.Extensions;
using Mindflur.IMS.Data.Models;

namespace Mindflur.IMS.Data.Repository
{
    public class RiskTreatmentRepository : BaseRepository<RiskTreatment>, IRiskTreatmentRepository
    {

        public RiskTreatmentRepository(IMSDEVContext dbContext, ILogger<RiskTreatment> logger) : base(dbContext, logger)
        {


        }
        public async Task<PaginatedItems<RiskTreatmentView>> GetRiskTreatment(GetListRequest getRiskTreatment)
        {

            string searchString = string.Empty;

            var rawData = (from rt in _context.RiskTreatments
                           join mdd in _context.MasterData on rt.ConsequenceMasterDataId equals mdd.Id
                           join mdc in _context.MasterData on rt.ProbabilityMasterDataId equals mdc.Id
                           join mds in _context.MasterData on rt.RiskRatingMasterDataId equals mds.Id
                           join mdt in _context.MasterData on rt.CurrentStatusMasterDataId equals mdt.Id
                           join mdo in _context.MasterData on rt.TreatmentOptionMasterDataId equals mdo.Id
                           join md in _context.UserMasters on rt.ResponsibleUserId equals md.UserId
                           join op in _context.OpportunitiesMasters on rt.OpportunityId equals op.Id
                           select new RiskTreatmentView
                           {
                               Id = rt.Id,
                               InherentRiskId = rt.RiskId,
                               RiskTreatmentOption = mdo.Items,
                               RiskTreatmentMitigationPlan = rt.MitigationPlan,
                               CurrentStatus = mdt.Items,
                               ResidualRiskProbability = mdc.Items,
                               ResidualRiskConsequence = mdd.Items,
                               RiskRating = mds.Items,
                               TotalRiskScore = rt.TotalRiskScore,
                               CompletionDate = rt.DueDate,
                               LastReview = rt.ReviewedOn,
                               ResponsiblePerson = $"{ md.FirstName } { md.LastName }",
                               Opportunities = rt.OpportunityId,
                               OpportunitiesDescription = op.OpportunitesDescription
                           })
            .OrderByDescending(rm => rm.CompletionDate)
            .AsQueryable();

            var filteredData = DataExtensions.OrderBy(rawData, getRiskTreatment.SortColumn, getRiskTreatment.Sort == "asc").Skip(getRiskTreatment.PerPage * (getRiskTreatment.Page - 1)).Take(getRiskTreatment.PerPage);
            var totalItems = await rawData.LongCountAsync();
            int totalPages = (int)Math.Ceiling(totalItems / (double)getRiskTreatment.PerPage);
            var model = new PaginatedItems<RiskTreatmentView>(getRiskTreatment.Page, getRiskTreatment.PerPage, totalPages, filteredData);
            return await Task.FromResult(model);
        }
        public async Task<RiskTreatmentPreviewView> GetRiskTreatmentPreview(int riskTreatmentId)
        {


            var rawData = await (from rt in _context.RiskTreatments
                                 join mdd in _context.MasterData on rt.ConsequenceMasterDataId equals mdd.Id
                                 join mdc in _context.MasterData on rt.ProbabilityMasterDataId equals mdc.Id
                                 join mds in _context.MasterData on rt.RiskRatingMasterDataId equals mds.Id
                                 join mdt in _context.MasterData on rt.CurrentStatusMasterDataId equals mdt.Id
                                 join md in _context.UserMasters on rt.ResponsibleUserId equals md.UserId
                                 join mdo in _context.MasterData on rt.TreatmentOptionMasterDataId equals mdo.Id
                                 join inherent in _context.Risks on rt.RiskId equals inherent.Id
                                 join op in _context.OpportunitiesMasters on rt.OpportunityId equals op.Id
                                 where riskTreatmentId == rt.Id
                                 select new RiskTreatmentPreviewView
                                 {
                                     Id = rt.Id,
                                     InherentRiskId = rt.RiskId,
                                    // InherentRisk = inherent.RiskDescription,
                                     RiskTreatmentOptionId = rt.TreatmentOptionMasterDataId,
                                     RiskTreatmentOption = mdo.Items,
                                     RiskTreatmentMitigationPlan = rt.MitigationPlan,
                                     CurrentStatusId = rt.CurrentStatusMasterDataId,
                                     CurrentStatus = mdt.Items,
                                     ResidualRiskProbabilityId = rt.ProbabilityMasterDataId,
                                     ResidualRiskProbability = mdc.Items,
                                     ResidualRiskConsequenceId = rt.ConsequenceMasterDataId,
                                     ResidualRiskConsequence = mdd.Items,
                                     RiskRatingId = rt.RiskRatingMasterDataId,
                                     RiskRating = mds.Items,
                                     TotalRiskScore = rt.TotalRiskScore,
                                     CompletionDate = rt.DueDate,
                                     LastReview = rt.ReviewedOn,
                                     ResponsiblePersonId = rt.ResponsibleUserId,
                                     ResponsiblePerson = $"{md.FirstName} {md.LastName}",
                                     Opportunities = rt.OpportunityId,
                                     OpportunitiesDescription = op.OpportunitesDescription
                                 }).ToListAsync();
            return rawData.FirstOrDefault();
        }
        public async Task<PaginatedItems<GetRiskTreatmentViewByInherentRisk>> GetRiskTreatmentByInherentRiskId(GetCAList getTasksByProjectId)
        {
            string searchString = string.Empty;

            var rawData = (from rt in _context.RiskTreatments

                           join mdd in _context.MasterData on rt.ConsequenceMasterDataId equals mdd.Id
                           join mdc in _context.MasterData on rt.ProbabilityMasterDataId equals mdc.Id
                           join mds in _context.MasterData on rt.RiskRatingMasterDataId equals mds.Id
                           join mdt in _context.MasterData on rt.CurrentStatusMasterDataId equals mdt.Id
                           join mdo in _context.MasterData on rt.TreatmentOptionMasterDataId equals mdo.Id
                           join md in _context.UserMasters on rt.ResponsibleUserId equals md.UserId
                           join op in _context.OpportunitiesMasters on rt.OpportunityId equals op.Id
                           where getTasksByProjectId.ActionId == rt.RiskId
                           select new GetRiskTreatmentViewByInherentRisk
                           {
                               Id = rt.Id,

                               RiskTreatmentMitigationPlan = rt.MitigationPlan,
                               RiskTreatmentOption = mdo.Items,
                               CurrentStatus = mdt.Items,
                               ResidualRiskProbability = mdc.Items,
                               ResidualRiskConsequence = mdd.Items,
                               RiskRating = mds.Items,
                               TotalRiskScore = rt.TotalRiskScore,
                               CompletionDate = rt.DueDate,
                               LastReview = rt.ReviewedOn,
                               ResponsiblePerson = string.Format("{0} {1}", md.FirstName, md.LastName),
                               Opportunities = rt.OpportunityId,
                               OpportunitiesDescription = op.OpportunitesDescription
                           })
            .OrderByDescending(rm => rm.CompletionDate)
            .AsQueryable();

            var filteredData = DataExtensions.OrderBy(rawData, getTasksByProjectId.ListRequests.SortColumn, getTasksByProjectId.ListRequests.Sort == "asc")
                                .Skip(getTasksByProjectId.ListRequests.PerPage * (getTasksByProjectId.ListRequests.Page - 1))
                                .Take(getTasksByProjectId.ListRequests.PerPage);
            var totalItems = await rawData.LongCountAsync();

            int totalPages = (int)Math.Ceiling(totalItems / (double)getTasksByProjectId.ListRequests.PerPage);
            var model = new PaginatedItems<GetRiskTreatmentViewByInherentRisk>(getTasksByProjectId.ListRequests.Page, getTasksByProjectId.ListRequests.PerPage, totalPages, filteredData);
            return await Task.FromResult(model);




        }

        public async Task<IList<RiskTreatmentPreviewView>> GetRiskTreatmentForInherant(int inherentRiskId)
        {
            var rawData = await (from rt in _context.RiskTreatments
                                 join mdd in _context.MasterData on rt.ConsequenceMasterDataId equals mdd.Id
                                 join mdc in _context.MasterData on rt.ProbabilityMasterDataId equals mdc.Id
                                 join mds in _context.MasterData on rt.RiskRatingMasterDataId equals mds.Id
                                 join mdt in _context.MasterData on rt.CurrentStatusMasterDataId equals mdt.Id
                                 join md in _context.UserMasters on rt.ResponsibleUserId equals md.UserId
                                 join mdo in _context.MasterData on rt.TreatmentOptionMasterDataId equals mdo.Id
                                 join inherent in _context.Risks on rt.RiskId equals inherent.Id
                                 join op in _context.OpportunitiesMasters on rt.OpportunityId equals op.Id
                                 where inherentRiskId == rt.RiskId
                                 select new RiskTreatmentPreviewView
                                 {
                                     Id = rt.Id,
                                     InherentRiskId = rt.RiskId,
                                    // InherentRisk = inherent.RiskDescription,
                                     RiskTreatmentOptionId = rt.TreatmentOptionMasterDataId,
                                     RiskTreatmentOption = mdo.Items,
                                     RiskTreatmentMitigationPlan = rt.MitigationPlan,
                                     CurrentStatusId = rt.CurrentStatusMasterDataId,
                                     CurrentStatus = mdt.Items,
                                     ResidualRiskProbabilityId = rt.ProbabilityMasterDataId,
                                     ResidualRiskProbability = mdc.Items,
                                     ResidualRiskConsequenceId = rt.ConsequenceMasterDataId,
                                     ResidualRiskConsequence = mdd.Items,
                                     RiskRatingId = rt.RiskRatingMasterDataId,
                                     RiskRating = mds.Items,
                                     TotalRiskScore = rt.TotalRiskScore,
                                     CompletionDate = rt.DueDate,
                                     LastReview = rt.ReviewedOn,
                                     ResponsiblePersonId = rt.ResponsibleUserId,
                                     ResponsiblePerson = $"{md.FirstName} {md.LastName}",
                                     Opportunities = rt.OpportunityId,
                                     OpportunitiesDescription = op.OpportunitesDescription
                                 }).ToListAsync();
            return await Task.FromResult(rawData);
        }
    }
}
