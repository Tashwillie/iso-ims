using DocumentFormat.OpenXml.Wordprocessing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Mindflur.IMS.Application.Contracts.Repository;
using Mindflur.IMS.Application.Contracts.Service;
using Mindflur.IMS.Application.Core.Enums;
using Mindflur.IMS.Application.ViewModel;
using Mindflur.IMS.Application.ViewModel.Core;
using Mindflur.IMS.Application.ViewModel.Model;
using Mindflur.IMS.Data.Base;
using Mindflur.IMS.Data.Models;

namespace Mindflur.IMS.Data.Repository
{
    public class RiskManagementInherentRiskRepository : BaseRepository<Risk>, IRiskRepository
    {

        private readonly IMessageService _messageService;
        private readonly IWorkItemRepository _workItemRepository;

        public RiskManagementInherentRiskRepository(IMSDEVContext dbContext, ILogger<Risk> logger, IMessageService messageService, IWorkItemRepository workItemRepository) : base(dbContext, logger)
        {
            _messageService = messageService;
            _workItemRepository = workItemRepository;
        }

        public async Task<Risk> GetRiskByWorkItemId(int workItemId)
        {
            var risk = await _context.Risks.FirstOrDefaultAsync(t => t.WorkItemId == workItemId);
            return risk;
        }

        public async Task<GetRiskMetadataViewModel2> GetRiskMetaData(int tenantId, int workItemId)
        {
            var riskMetaData = (from risk in _context.Risks
                                join work in _context.WorkItemMasters on risk.WorkItemId equals work.WorkItemId
                                join user in _context.UserMasters on risk.UpdatedBy equals user.UserId into user
                                from subuser in user.DefaultIfEmpty()
                                join reviewuser in _context.UserMasters on risk.ReviewedBy equals reviewuser.UserId into ruser
                                from subreviewUser in ruser.DefaultIfEmpty()
                                join appuser in _context.UserMasters on risk.ApprovedBy equals appuser.UserId into auser
                                from approveUser in auser.DefaultIfEmpty()
                                join assign in _context.UserMasters on work.AssignedToUserId equals assign.UserId into assign
                                from assignuser in assign.DefaultIfEmpty()

                                where risk.WorkItemId == workItemId

                                select new GetRiskMetadataViewModel2()
                                {
                                    Id = risk.Id,
                                    WorkItemId = risk.WorkItemId,
                                    CurrentControls = risk.CurrentControls,
                                    TotalRiskScore = risk.TotalRiskScore,
                                    AssignedToId = assignuser.UserId,
                                    AssignedTo = $"{assignuser.FirstName} {assignuser.LastName}",
                                    IsApproved = risk.IsApproved,
                                    ApprovedById = risk.ApprovedBy,
                                    ApprovedBy = $"{approveUser.FirstName} {approveUser.LastName}",
                                    AppovedOn = risk.ApprovedOn,
                                    ReviewedById = risk.ReviewedBy,
                                    ReviewedBy = $"{subreviewUser.FirstName} {subreviewUser.LastName}",
                                    ReviewedOn = risk.ReviewedOn,
                                    UpdatedById = risk.UpdatedBy,
                                    UpdatedBy = $"{subuser.FirstName} {subuser.LastName}",
                                    UpdatedOn = risk.UpdatedOn,

                                }).AsQueryable();
            return riskMetaData.FirstOrDefault();
        }

        public async Task UpdateRiskMetaData(PutRiskMetadataViewModel riskView, int workItemId, int userId, int tenantId)
        {
            var metaData = await _context.Risks.Where(t => t.WorkItemId == workItemId).FirstOrDefaultAsync();
            if (metaData != null)
            {
                var existingToken = _context.WorkItemWorkItemTokens.Where(ps => ps.WorkItemId == workItemId).ToList();
                _context.WorkItemWorkItemTokens.RemoveRange(existingToken);
                await _context.SaveChangesAsync();

                var tokens = new List<WorkItemWorkItemToken>();

                foreach (int token in riskView.Tokens)
                {
                    var newTokens = new WorkItemWorkItemToken
                    {
                        WorkItemId = workItemId,
                        TokenId = token,
                    };
                    tokens.Add(newTokens);
                }

                await _context.WorkItemWorkItemTokens.AddRangeAsync(tokens);
                await _context.SaveChangesAsync();

                var impact = await _workItemRepository.GetTokenDetailsForRisk(workItemId, (int)IMSRiskDetails.ImapctLevel);
                var probability = await _workItemRepository.GetTokenDetailsForRisk(workItemId, (int)IMSRiskDetails.ProbabilityLevel);



                metaData.CurrentControls = riskView.CurrentControls;
                metaData.WorkItemId = workItemId;
                metaData.TotalRiskScore = impact.Weightage * probability.Weightage;
                metaData.UpdatedBy = userId;
                metaData.UpdatedOn = DateTime.UtcNow;
                await _context.SaveChangesAsync();

                var riskRating = new WorkItemWorkItemToken();
				if ( impact.Weightage >= 5 && probability.Weightage ==1)
				{
					riskRating.TokenId = (int)IMSRiskRatingTokenID.Medium;
				}
				 else if (probability.Weightage >= 5 && impact.Weightage == 1)
				{
					riskRating.TokenId = (int)IMSRiskRatingTokenID.Medium;
				}

				else if (metaData.TotalRiskScore <=6)
                {
                    riskRating.TokenId = (int)IMSRiskRatingTokenID.Low;
                }
                else if(metaData.TotalRiskScore == 7 || metaData.TotalRiskScore<= 16)
                {
                    riskRating.TokenId = (int)IMSRiskRatingTokenID.Medium;
                }
                else if(metaData.TotalRiskScore == 17 || metaData.TotalRiskScore <= 36 )
                {
                    riskRating.TokenId = (int)IMSRiskRatingTokenID.High;
                }
                else
                {
                    riskRating.TokenId = (int)IMSRiskRatingTokenID.VeryHigh;
                }

                riskRating.WorkItemId = workItemId;
                await _context.WorkItemWorkItemTokens.AddAsync(riskRating);

                var rawData = await _context.WorkItemMasters.FindAsync(workItemId);
                rawData.AssignedToUserId = riskView.AssignedToUserId;
                rawData.StatusMasterDataId = (int)IMSItemStatus.Assigned;
                await _context.SaveChangesAsync();

                await _messageService.SendNotificationMessage(new NotificationMessage()
                {
                    SourceIdUserId = userId,
                    BroadcastLevel = NotificationBroadcastLevel.User,
                    EventType = NotificationEventType.BusinessMaster,
                    TenantId = tenantId,
                    Action = IMSControllerActionCategory.Edit,
                    Module = IMSControllerCategory.RiskManagement,
                    ItemId = workItemId,
                    Description = rawData.Description,
                    Title = rawData.Title,
                });
            }
            else
            {
                
               

               

                var workItem = new List<WorkItemWorkItemToken>();

                foreach (int a in riskView.Tokens)
                {
                    var newWorkItem = new WorkItemWorkItemToken
                    {
                        WorkItemId = workItemId,
                        TokenId = a,
                    };
                    workItem.Add(newWorkItem);
                }
                await _context.WorkItemWorkItemTokens.AddRangeAsync(workItem);
                await _context.SaveChangesAsync();

                var impact = await _workItemRepository.GetTokenDetailsForRisk(workItemId, (int)IMSRiskDetails.ImapctLevel);
                var probability = await _workItemRepository.GetTokenDetailsForRisk(workItemId, (int)IMSRiskDetails.ProbabilityLevel);
                metaData = new Risk();
                metaData.CurrentControls = riskView.CurrentControls;
                metaData.WorkItemId = workItemId;
                metaData.TotalRiskScore = impact.Weightage * probability.Weightage;
                metaData.UpdatedBy = userId;
                metaData.UpdatedOn = DateTime.UtcNow;

                await _context.Risks.AddAsync(metaData);
                await _context.SaveChangesAsync();
                var riskRating = new WorkItemWorkItemToken();

                if (impact.Weightage >= 5 && probability.Weightage == 1)
                {
                    riskRating.TokenId = (int)IMSRiskRatingTokenID.Medium;
                }
                else if (probability.Weightage >= 5 && impact.Weightage == 1)
                {
                    riskRating.TokenId = (int)IMSRiskRatingTokenID.Medium;
                }

                else if (metaData.TotalRiskScore <= 6)
                {
                    riskRating.TokenId = (int)IMSRiskRatingTokenID.Low;
                }
                else if (metaData.TotalRiskScore == 7 || metaData.TotalRiskScore <= 16)
                {
                    riskRating.TokenId = (int)IMSRiskRatingTokenID.Medium;
                }
                else if (metaData.TotalRiskScore == 17 || metaData.TotalRiskScore <= 36)
                {
                    riskRating.TokenId = (int)IMSRiskRatingTokenID.High;
                }
                else
                {
                    riskRating.TokenId = (int)IMSRiskRatingTokenID.VeryHigh;
                }

                riskRating.WorkItemId = workItemId;
                await _context.WorkItemWorkItemTokens.AddAsync(riskRating);

                var rawData = await _context.WorkItemMasters.FindAsync(workItemId);
                rawData.AssignedToUserId = riskView.AssignedToUserId;
                rawData.StatusMasterDataId = (int)IMSItemStatus.Assigned;
                await _context.SaveChangesAsync();
                await _messageService.SendNotificationMessage(new NotificationMessage()
                {
                    SourceIdUserId = userId,
                    BroadcastLevel = NotificationBroadcastLevel.User,
                    EventType = NotificationEventType.BusinessMaster,
                    TenantId = tenantId,
                    Action = IMSControllerActionCategory.Edit,
                    Module = IMSControllerCategory.RiskManagement,
                    ItemId = workItemId,
                    Description = rawData.Description,
                    Title = rawData.Title,
                });
            }
        }


		public async Task<IList<GetRiskTreatment>> GetAallRiskTreament(int tenantId, int workItemId)
		{
			var rawDAta = await (from wt in _context.WorkItemMasters
								 join risk in _context.WorkItemMasters on wt.WorkItemId equals risk.SourceItemId

								 where risk.SourceItemId == workItemId && risk.SourceId == (int)IMSModules.RiskManagement
								 select new GetRiskTreatment()
								 {
									 StatusId = risk.StatusMasterDataId
								 }).ToListAsync();
			return await Task.FromResult(rawDAta);
		}
	}
}