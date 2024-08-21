using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Mindflur.IMS.Application.Contracts.Repository;
using Mindflur.IMS.Application.Core.Constants;
using Mindflur.IMS.Application.Core.Enums;
using Mindflur.IMS.Application.Core.Exceptions;
using Mindflur.IMS.Application.ViewModel;
using Mindflur.IMS.Application.ViewModel.Core;
using Mindflur.IMS.Application.ViewModel.Model;
using Mindflur.IMS.Application.ViewModel.View;
using Mindflur.IMS.Data.Base;
using Mindflur.IMS.Data.Extensions;
using Mindflur.IMS.Data.Models;

namespace Mindflur.IMS.Data.Repository
{
    public class IncidentMasterRepository : BaseRepository<IncidentMetaData>, IIncidentMasterRepository
    {
       private readonly IWorkItemRepository _workItemRepository;
        public IncidentMasterRepository(IMSDEVContext dbContext, ILogger<IncidentMetaData> logger,IWorkItemRepository workItemRepository) : base(dbContext, logger)
        {
            _workItemRepository = workItemRepository;
            

        }

        public async Task<IncidentMetaData> UpdateIncidentDescription(IncidentMasterDescriptionPutView incidentMaster, int userId, int incidentId, int tenantId)
        {
            var updateIncident = await _context.IncidentMetaDatas.FindAsync(incidentMaster.Id);
            if (updateIncident.IncidentMetadataId == incidentId )
            {
                var existingIncidentTags = _context.IncidentMasterClassificationTags.Where(ps => ps.IncidentId == incidentMaster.Id).ToList();
                _context.IncidentMasterClassificationTags.RemoveRange(existingIncidentTags);
                await _context.SaveChangesAsync();

                var tagsForIncidentTags = new List<IncidentMasterClassificationTag>();


                foreach (int incidentTag in incidentMaster.IncidentClassificationTags)
                {
                    var newIncidentTags = new IncidentMasterClassificationTag
                    {
                        IncidentId = incidentMaster.Id,
                        IncidentClassificationTags = incidentTag,
                    };
                    tagsForIncidentTags.Add(newIncidentTags);
                }

                await _context.IncidentMasterClassificationTags.AddRangeAsync(tagsForIncidentTags);
                await _context.SaveChangesAsync();

                updateIncident.WorkResumed = incidentMaster.WorkResumed;
                updateIncident.WearedPpe = incidentMaster.WearedPpe;
                updateIncident.HowItOccured = incidentMaster.DescriptionHowInjuryOccurred;
                updateIncident.InjuryDescription = incidentMaster.DescriptionOfInjury;
                updateIncident.MedicalTreatment = incidentMaster.DescriptionOfMedicalTreatment;
                updateIncident.ClassificationDescription = incidentMaster.ClassificationDescription;
                updateIncident.UpdatedBy = userId;
                updateIncident.UpdatedOn = DateTime.UtcNow;
                await _context.SaveChangesAsync();
                ActivityLog activityLog = new ActivityLog();
                activityLog.TenantId = tenantId;
                activityLog.ControllerId = (int)IMSControllerCategory.IncidentManagement;
                activityLog.EntityId = incidentId;
                activityLog.ModuleAction = (int)IMSControllerActionCategory.Edit;
                activityLog.Description = "Incident  Has Been Updated";
                activityLog.Details = System.Text.Json.JsonSerializer.Serialize(updateIncident);
                activityLog.Status = true;
                activityLog.CreatedBy = userId;
                activityLog.CreatedOn = DateTime.UtcNow;
                await _context.ActivityLogs.AddAsync(activityLog);
                await _context.SaveChangesAsync();

            }
            else
            {
                throw new NotFoundException(string.Format(RepositoryConstant.IdNotFoundErrorMessage), incidentId);
            }
            return updateIncident;
        }
        public async Task<GetIncidentMetaDataView> GetIncidentMetaData( int workItemId, int tenantId)
        {
            var data =  (from incident in _context.IncidentMetaDatas
                              join mt in _context.WorkItemMasters on incident.WorkItemId equals mt.WorkItemId
                             join um in _context.UserMasters on mt.ResponsibleUserId equals um.UserId into person
                             from subperson in person.DefaultIfEmpty()
                              join user in _context.UserMasters on incident.UpdatedBy equals user.UpdatedBy into user
                              from subuser in user.DefaultIfEmpty()
                              join md in _context.MasterData on incident.OccupationId equals md.Id into master
                              from submaster in master.DefaultIfEmpty()
                              join assignedUser in _context.UserMasters on mt.AssignedToUserId equals assignedUser.UserId into assignedUser
                              from subAssignedUser in assignedUser.DefaultIfEmpty()
						
						 join reviewuser in _context.UserMasters on incident.ReviewedBy equals reviewuser.UserId into ruser
						 from subreviewUser in ruser.DefaultIfEmpty()
						 join appuser in _context.UserMasters on incident.ApprovedBy equals appuser.UserId into auser
						 from approveUser in auser.DefaultIfEmpty()
						 where incident.WorkItemId == workItemId
                              select new GetIncidentMetaDataView()
                              {
                                  IncidentId = incident.IncidentMetadataId,
                                  WorkItemId = incident.WorkItemId,
                                  WorkItemText = mt.Title,
                                  EmployeeId = mt.ResponsibleUserId,
                                  EmployeeName = $"{subperson.FirstName} {subperson.LastName}",
                                  DateOfIncident = incident.DateOfIncident,
                                  Occupation = incident.OccupationId,
                                  OccupationName = submaster.Items,
                                  AllowedToBeClosed = incident.AllowedToBeClosed,
                                  WorkResumed = incident.WorkResumed,
                                  WearedPpe = incident.WearedPpe,
                                  DescriptionOfInjury = incident.InjuryDescription,
                                  DescriptionOfHowInjuryOccured = incident.HowItOccured,
                                  DescriptionOfMedicalTreatment = incident.MedicalTreatment,
                                  ClassificationDescription = incident.ClassificationDescription,
                                  UpdatedBy= $"{subuser.FirstName} {subuser.LastName}",
                                  UpdatedById=incident.UpdatedBy,
                                  UpdatedOn=incident.UpdatedOn,
                                  AssignedUserId=mt.AssignedToUserId,
                                  AssignedUser=$"{subAssignedUser.FirstName} {subAssignedUser.LastName}",
								  IsApproved = incident.IsApproved,
								  ApprovedById = incident.ApprovedBy,
								  ApprovedBy = $"{approveUser.FirstName} {approveUser.LastName}",
								  ApprovedOn = incident.ApprovedOn,
								  ReviewedById = incident.ReviewedBy,
								  ReviewedBy = $"{subreviewUser.FirstName} {subreviewUser.LastName}",
								  ReviewedOn =  incident.ReviewedOn,


							  }).AsQueryable();
            return data.FirstOrDefault();
        }
        public async Task<IncidentMetaData> GetIncidentByWorkItemId(int workItemId)
        {
            var incident = await _context.IncidentMetaDatas.FirstOrDefaultAsync(t => t.WorkItemId == workItemId);
            return incident;
        }

        public async Task<IList<GetCAListByIncidentId>>GetCaListByIncidentId(int tenantId,int workItemId)
        {
            var rawData = await (from incident in _context.WorkItemMasters
                                 join metaData in _context.IncidentMetaDatas on incident.WorkItemId equals metaData.WorkItemId
                                 join ca in _context.WorkItemMasters on incident.WorkItemId equals ca.SourceItemId
                                 join status in _context.MasterData on incident.StatusMasterDataId equals status.Id into status
                                 from subStatus in status.DefaultIfEmpty()
                                 where incident.WorkItemId == workItemId && incident.TenantId==tenantId && incident.WorkItemTypeId==(int)IMSModules.IncidentManagement && ca.WorkItemTypeId==(int)IMSModules.CorrectiveAction
                                 select new GetCAListByIncidentId()
                                 {
                                     Id = incident.WorkItemId,
                                     StatusId = ca.StatusMasterDataId
                                 }).ToListAsync();
            return await Task.FromResult(rawData);  
        }

        public async Task UpadteIncidentMetaData(PutIncidentMasterView putIncidentMasterView, int workItemId, int UserId, int tenantId)
        {
             var incident = await GetIncidentByWorkItemId(workItemId);
            var incidentData = await _workItemRepository.GetByIdAsync(workItemId);

            if(incident == null)
            {
                var tokentags = new List<WorkItemWorkItemToken>();
                foreach (int tokens in putIncidentMasterView.Token)
                {
                    var newtokens = new WorkItemWorkItemToken
                    {
                        WorkItemId = workItemId,
                        TokenId = tokens
                    };
                    tokentags.Add(newtokens);
                }
                await _context.WorkItemWorkItemTokens.AddRangeAsync(tokentags);
                await _context.SaveChangesAsync();

                incident = new IncidentMetaData();
                incident.WorkItemId= workItemId;               
                incident.DateOfIncident = putIncidentMasterView.DateOfIncident;
                incident.OccupationId = putIncidentMasterView.Occupation;
                incident.AllowedToBeClosed= putIncidentMasterView.AllowedToBeClosed;
                incident.WorkResumed= putIncidentMasterView.WorkResumed;
                incident.WearedPpe = putIncidentMasterView.WearedPpe;
                incident.InjuryDescription = putIncidentMasterView.DescriptionOfInjury;
                incident.HowItOccured = putIncidentMasterView.DescriptionOfHowInjuryOccured;
                incident.MedicalTreatment = putIncidentMasterView.DescriptionOfMedicalTreatment;
                incident.ClassificationDescription= putIncidentMasterView.ClassificationDescription;
                incident.UpdatedBy = UserId;
                incident.UpdatedOn = DateTime.UtcNow;
                await AddAsync(incident);
               
            }
            else 
            {
                var existingtokens = _context.WorkItemWorkItemTokens.Where(t => t.WorkItemId == workItemId).ToList();
                _context.WorkItemWorkItemTokens.RemoveRange(existingtokens);
                var tokentags = new List<WorkItemWorkItemToken>();
                foreach (int tokens in putIncidentMasterView.Token)
                {
                    var newtokens = new WorkItemWorkItemToken
                    {
                        WorkItemId = workItemId,
                        TokenId = tokens
                    };
                    tokentags.Add(newtokens);
                }
                await _context.WorkItemWorkItemTokens.AddRangeAsync(tokentags);
                await _context.SaveChangesAsync();
                incident.DateOfIncident = putIncidentMasterView.DateOfIncident;
                incident.OccupationId = putIncidentMasterView.Occupation;
                incident.AllowedToBeClosed = putIncidentMasterView.AllowedToBeClosed;
                incident.WorkResumed = putIncidentMasterView.WorkResumed;
                incident.WearedPpe = putIncidentMasterView.WearedPpe;
                incident.InjuryDescription = putIncidentMasterView.DescriptionOfInjury;
                incident.HowItOccured = putIncidentMasterView.DescriptionOfHowInjuryOccured;
                incident.MedicalTreatment = putIncidentMasterView.DescriptionOfMedicalTreatment;
                incident.ClassificationDescription = putIncidentMasterView.ClassificationDescription;
                incident.UpdatedBy = UserId;
                incident.UpdatedOn = DateTime.UtcNow;
                await UpdateAsync(incident);
            }

			incidentData.AssignedToUserId = putIncidentMasterView.AssignToUserId;
            incidentData.StatusMasterDataId = (int)IMSItemStatus.Assigned;
			await _workItemRepository.UpdateAsync(incidentData);
			await _context.SaveChangesAsync();
		}
    }
}
