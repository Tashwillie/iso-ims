using DocumentFormat.OpenXml.Office2010.Excel;
using DocumentFormat.OpenXml.Spreadsheet;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Mindflur.IMS.Application.Contracts.Repository;
using Mindflur.IMS.Application.Core.Constants;
using Mindflur.IMS.Application.Core.Enums;
using Mindflur.IMS.Application.Core.Exceptions;
using Mindflur.IMS.Application.ViewModel.Core;
using Mindflur.IMS.Application.ViewModel.Model;
using Mindflur.IMS.Application.ViewModel.View;
using Mindflur.IMS.Data.Base;
using Mindflur.IMS.Data.Extensions;
using Mindflur.IMS.Data.Models;

namespace Mindflur.IMS.Data.Repository
{
	public class RiskAssesmentMasterRepository : BaseRepository<RiskAssesmentMaster>, IRiskAssesmentMasterRepository
	{
		public RiskAssesmentMasterRepository(IMSDEVContext dbContext, ILogger<RiskAssesmentMaster> logger) : base(dbContext, logger)
		{
		}
		public async Task<PaginatedItems<RiskAssesmentMasterGridView>> GetRiskAssessmentList(GetRiskAssesmentRequestList riskList)
		{
			var rawData  = ( from ram in _context.RiskAssesmentMasters
							 join risk in _context.Risks on ram.RiskId equals risk.WorkItemId
							 join wt in _context.Tokens on ram.SourceImpact equals wt.TokenId into wt
							 from source in wt.DefaultIfEmpty()
							 join probability in _context.Tokens on ram.SourceProbrability equals probability.TokenId into probability
							 from proba in probability.DefaultIfEmpty()
							 join impact in _context.Tokens on ram.CurrentImpact equals impact.TokenId into impact
							 from imp in impact.DefaultIfEmpty()
							 join current in _context.Tokens on ram.CurrentProbability equals current.TokenId into current 
							 from currentProba in current.DefaultIfEmpty()
							 where ram.RiskId == riskList.RiskId
							
							 select new RiskAssesmentMasterGridView
							 {
								 Id = ram.Id,
								 RiskId = ram.RiskId,
								 SourceImpact = source.TokenName,
								 SourceProbrability = proba.TokenName,
							     CurrentImpact = imp.TokenName,
								 CurrentProbability = currentProba.TokenName,
								
							 }).AsQueryable();


			var filteredData = DataExtensions.OrderBy(rawData, riskList.ListRequests.SortColumn, riskList.ListRequests.Sort == "asc")
						  .Skip(riskList.ListRequests.PerPage * (riskList.ListRequests.Page - 1))
						  .Take(riskList.ListRequests.PerPage);

			var totalItems = await rawData.LongCountAsync();

			int totalPages = (int)Math.Ceiling(totalItems / (double)riskList.ListRequests.PerPage);
			var model = new PaginatedItems<RiskAssesmentMasterGridView>(riskList.ListRequests.Page, riskList.ListRequests.PerPage, totalPages, filteredData);
			return await Task.FromResult(model);
		}

		public async Task<RiskAssesmentMasterview> GetRiskByRiskId(int riskId)
		{
			var metaData = (from ram in _context.RiskAssesmentMasters
							join risk in _context.Risks on ram.RiskId equals risk.WorkItemId
							join wt in _context.Tokens on ram.SourceImpact equals wt.TokenId
							join wt1 in _context.Tokens on ram.SourceProbrability equals wt1.TokenId
							join wt2 in _context.Tokens on ram.CurrentImpact equals wt2.TokenId
							join wt3 in _context.Tokens on ram.CurrentProbability equals wt3.TokenId
							join wt4 in _context.Tokens on ram.ResidualRiskRating equals wt4.TokenId
							join user in _context.UserMasters on ram.CreatedBy equals user.UserId
							where  ram.RiskId == riskId
							select new RiskAssesmentMasterview
							{
								Id = ram.Id,
								RiskId = ram.RiskId,
								SourceImpactId = ram.SourceImpact,
								SourceImpact = wt.TokenName,
								SourceProbrabilityId = ram.SourceProbrability,
								SourceProbrability = wt1.TokenName,
								ResidualImpactId = ram.CurrentImpact,
								ResidualImpact = wt2.TokenName,
								ResidualProbabilityId = ram.CurrentProbability,
								ResidualProbability = wt3.TokenName,
								ResidualRiskRtingId = ram.ResidualRiskRating,
								ResidualRiskRating = wt4.TokenName,
								ResidualRiskScore = ram.ResidualRiskScore,
								CreatedById = ram.CreatedBy,
								CreatedBy = $"{user.FirstName}{user.LastName}",
								CreatedOn = ram.CreatedOn,
							}).AsQueryable();
			return metaData.FirstOrDefault();
       
		}
        public async Task UpsertRiskAssessment(PostRiskAssesmentView editRiskAssessmentView, int sourceImpact, int sourceProbability, int userId, int riskId)
		{
			var riskAssesment = await _context.RiskAssesmentMasters.Where(t => t.RiskId == riskId).FirstOrDefaultAsync();
			if(riskAssesment != null)
			{


                riskAssesment.SourceProbrability = sourceProbability;
                riskAssesment.SourceImpact = sourceImpact;
                riskAssesment.CurrentImpact = editRiskAssessmentView.CurrentImpact;
                riskAssesment.CurrentProbability = editRiskAssessmentView.CurrentProbability;
                riskAssesment.UpdatedBy = userId;
                riskAssesment.UpdatedOn = DateTime.UtcNow;
                await _context.SaveChangesAsync();
				var currentImpact = await _context.Tokens.Where(t => t.TokenId == editRiskAssessmentView.CurrentImpact).FirstOrDefaultAsync();
				var currentprobability = await _context.Tokens.Where(t => t.TokenId == editRiskAssessmentView.CurrentProbability).FirstOrDefaultAsync();

				riskAssesment.ResidualRiskScore = currentImpact.Weightage * currentprobability.Weightage;
                if (currentImpact.Weightage >= 5 && currentprobability.Weightage == 1) 
                {
                    riskAssesment.ResidualRiskRating = (int)IMSRiskRatingTokenID.Medium;
                }
                else if (currentprobability.Weightage >= 5 && currentImpact.Weightage == 1)
                {
                    riskAssesment.ResidualRiskRating = (int)IMSRiskRatingTokenID.Medium;
                }

                else if (riskAssesment.ResidualRiskScore <= 6)
                {
                    riskAssesment.ResidualRiskRating = (int)IMSRiskRatingTokenID.Low;
                }
                else if (riskAssesment.ResidualRiskScore == 7 || riskAssesment.ResidualRiskScore <= 16)
                {
                    riskAssesment.ResidualRiskRating = (int)IMSRiskRatingTokenID.Medium;
                }
                else if (riskAssesment.ResidualRiskScore == 17 || riskAssesment.ResidualRiskScore <= 36)
                {
                    riskAssesment.ResidualRiskRating = (int)IMSRiskRatingTokenID.High;
                }
                else
                {
                    riskAssesment.ResidualRiskRating = (int)IMSRiskRatingTokenID.VeryHigh;
                }
				await _context.SaveChangesAsync();
            }
			else
			{
                RiskAssesmentMaster newriskAssesment = new RiskAssesmentMaster();
                newriskAssesment.RiskId = riskId;
                newriskAssesment.SourceImpact = sourceImpact;
                newriskAssesment.SourceProbrability = sourceProbability;
                newriskAssesment.CurrentImpact = editRiskAssessmentView.CurrentImpact;
                newriskAssesment.CurrentProbability = editRiskAssessmentView.CurrentProbability;
                newriskAssesment.CreatedBy = userId;
                newriskAssesment.CreatedOn = DateTime.UtcNow;
				

                var currentImpact = await _context.Tokens.Where(t => t.TokenId == editRiskAssessmentView.CurrentImpact).FirstOrDefaultAsync();
                var currentprobability = await _context.Tokens.Where(t => t.TokenId == editRiskAssessmentView.CurrentProbability).FirstOrDefaultAsync();

                newriskAssesment.ResidualRiskScore = currentImpact.Weightage * currentprobability.Weightage;
                if (currentImpact.Weightage >= 5 && currentprobability.Weightage == 1)
                {
                    newriskAssesment.ResidualRiskRating = (int)IMSRiskRatingTokenID.Medium;
                }
                else if (currentprobability.Weightage >= 5 && currentImpact.Weightage == 1)
                {
                    newriskAssesment.ResidualRiskRating = (int)IMSRiskRatingTokenID.Medium;
                }

                else if (newriskAssesment.ResidualRiskScore <= 6)
                {
                    newriskAssesment.ResidualRiskRating = (int)IMSRiskRatingTokenID.Low;
                }
                else if (newriskAssesment.ResidualRiskScore == 7 || newriskAssesment.ResidualRiskScore <= 16)
                {
                    newriskAssesment.ResidualRiskRating = (int)IMSRiskRatingTokenID.Medium;
                }
                else if (newriskAssesment.ResidualRiskScore == 17 || newriskAssesment.ResidualRiskScore <= 36)
                {
                    newriskAssesment.ResidualRiskRating = (int)IMSRiskRatingTokenID.High;
                }
                else
                {
                    newriskAssesment.ResidualRiskRating = (int)IMSRiskRatingTokenID.VeryHigh;
                }
                await _context.RiskAssesmentMasters.AddAsync(newriskAssesment);
                await _context.SaveChangesAsync();
            }
		}

    }
}

