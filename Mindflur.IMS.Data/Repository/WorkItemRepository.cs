using AutoMapper;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Mindflur.IMS.Application.Contracts.Repository;
using Mindflur.IMS.Application.Core.Constants;
using Mindflur.IMS.Application.Core.Enums;
using Mindflur.IMS.Application.Core.Exceptions;
using Mindflur.IMS.Application.DomainModel;
using Mindflur.IMS.Application.ViewModel;
using Mindflur.IMS.Application.ViewModel.Core;
using Mindflur.IMS.Application.ViewModel.View;
using Mindflur.IMS.Data.Base;
using Mindflur.IMS.Data.Extensions;
using Mindflur.IMS.Data.Models;
using NUnit.Framework.Internal.Execution;

namespace Mindflur.IMS.Data.Repository
{
	public class WorkItemRepository : BaseRepository<WorkItemMaster>, IWorkItemRepository
	{
		private readonly IMapper _mapper;

		public WorkItemRepository(IMSDEVContext dbContext, ILogger<WorkItemMaster> logger, IMapper mapper) : base(dbContext, logger)
		{
			_mapper = mapper;
		}

		public async Task<PaginatedItems<WorkItemGridView>> GetPastWorkItemList(GetWorkItemGridRequest getWorkItemList)
		{
			var rawData = (from work in _context.WorkItemMasters
						   join audit in _context.AuditPrograms on work.SourceItemId equals audit.Id
						   join catergory in _context.MasterData on work.CategoryId equals catergory.Id
						   join status in _context.MasterData on work.StatusMasterDataId equals status.Id
						   join user in _context.UserMasters on work.AssignedToUserId equals user.UserId into user
						   from subuser in user.DefaultIfEmpty()
						   join user1 in _context.UserMasters on work.AssignedToUserId equals user1.UserId into user1
						   from subuser1 in user1.DefaultIfEmpty()
						   join department in _context.DepartmentMasters on work.DepartmentId equals department.DepartmentId
						   where work.TenantId == getWorkItemList.TenantId  && work.StatusMasterDataId != (int)IMSItemStatus.Closed

						   select new WorkItemGridView()
						   {
							   SourceId = work.SourceId,
							   WorkItemId = work.WorkItemId,
							   SourceItemId = work.SourceItemId,
							   Title = work.Title,
							   CategoryId = work.CategoryId,
							   CategoryText = catergory.Items,
							   CreatedBy = $"{subuser1.FirstName} {subuser1.LastName}",
							   CreatedById = work.CreatedBy,
							   StatusText = status.Items,
							   Department = department.DepartmentName,
							   DepartmentId = work.DepartmentId,
							   AssignedToUserId = work.AssignedToUserId,
							   AssignedToUsername = $"{subuser.FirstName} {subuser.LastName}",
							   DueDate = work.DueDate,
                               AuditCreatedOn = audit.CreatedOn,
							   AuditActualStart= audit.ActualStart,
                           }).OrderByDescending(work => work.WorkItemId).AsQueryable();

			if (getWorkItemList.SourceId > 0)
				rawData = rawData.Where(log => log.SourceId == getWorkItemList.SourceId);
			if (getWorkItemList.SourceItemId > 0)
				rawData = rawData.Where(log => log.SourceItemId == getWorkItemList.SourceItemId);
			if (getWorkItemList.AssignTo > 0)
				rawData = rawData.Where(log => log.AssignedToUserId == getWorkItemList.AssignTo);
			if (getWorkItemList.CreatedBy > 0)
				rawData = rawData.Where(log => log.CreatedById == getWorkItemList.CreatedBy);
            if (getWorkItemList.AuditActualStart != null)
                rawData = rawData.Where(log => log.AuditActualStart < getWorkItemList.AuditActualStart);
            if (getWorkItemList.AuditCreatedOn != null )
				rawData = rawData.Where(log => log.AuditCreatedOn< getWorkItemList.AuditCreatedOn);
           

            var filteredData = DataExtensions.OrderBy(rawData, getWorkItemList.ListRequests.SortColumn, getWorkItemList.ListRequests.Sort == "asc")
							 .Skip(getWorkItemList.ListRequests.PerPage * (getWorkItemList.ListRequests.Page - 1))
							 .Take(getWorkItemList.ListRequests.PerPage);

			var totalItems = await rawData.LongCountAsync();

			int totalPages = (int)Math.Ceiling(totalItems / (double)getWorkItemList.ListRequests.PerPage);
			var model = new PaginatedItems<WorkItemGridView>(getWorkItemList.ListRequests.Page, getWorkItemList.ListRequests.PerPage, totalPages, filteredData);

			return await Task.FromResult(model);
		}

		public async Task<PaginatedItems<WorkItemGridView>> GetWorkItemList(GetWorkItemGridRequest getWorkItemList)
		{
			var rawData = (from workItem in _context.WorkItemMasters
						   join category in _context.MasterData on workItem.CategoryId equals category.Id into temp_category
						   from temp_category_tbl in temp_category.DefaultIfEmpty()
						   join status in _context.MasterData on workItem.StatusMasterDataId equals status.Id
						   join user in _context.UserMasters on workItem.AssignedToUserId equals user.UserId into user
						   from subuser in user.DefaultIfEmpty()
						   join assignedToUser in _context.UserMasters on workItem.CreatedBy equals assignedToUser.UserId into user1
						   from subuser1 in user1.DefaultIfEmpty()
						   join department in _context.DepartmentMasters on workItem.DepartmentId equals department.DepartmentId into department
						   from subDepartment in department.DefaultIfEmpty()
						   join source in _context.MasterData on workItem.SourceId equals source.Id into source1	
						   from subsource in source1.DefaultIfEmpty()
						   where workItem.TenantId == getWorkItemList.TenantId

						   select new WorkItemGridView()
						   {
							   SourceId = workItem.SourceId,
							   Source=subsource.Items,
							   Description=workItem.Description,
							   WorkItemId = workItem.WorkItemId,
							   SourceItemId = workItem.SourceItemId,
							   Title = workItem.Title,
							   CategoryId = workItem.CategoryId,
							   CategoryText = temp_category_tbl.Items,
							   CreatedBy = $"{subuser1.FirstName} {subuser1.LastName}",
							   CreatedById = workItem.CreatedBy,
							   StatusText = status.Items,
							   Department = subDepartment.DepartmentName,
							   DepartmentId = workItem.DepartmentId,
							   AssignedToUserId = workItem.AssignedToUserId,
							   AssignedToUsername = $"{subuser.FirstName} {subuser.LastName}",
							   DueDate = workItem.DueDate,
							   WorkItemTypeId = workItem.WorkItemTypeId
						   }).OrderByDescending(work => work.WorkItemId).AsQueryable();

			if (getWorkItemList.SourceId > 0)
				rawData = rawData.Where(workitem => workitem.SourceId == getWorkItemList.SourceId);
			if (getWorkItemList.SourceItemId > 0)
				rawData = rawData.Where(workitem => workitem.SourceItemId == getWorkItemList.SourceItemId);
			if (getWorkItemList.AssignTo > 0)
				rawData = rawData.Where(workitem => workitem.AssignedToUserId == getWorkItemList.AssignTo);
			if (getWorkItemList.CreatedBy > 0)
				rawData = rawData.Where(workitem => workitem.CreatedById == getWorkItemList.CreatedBy);
			if (getWorkItemList.CategoryId > 0)
				rawData = rawData.Where(workitem => workitem.WorkItemTypeId == getWorkItemList.CategoryId);
			if (getWorkItemList.UserId > 0)
				rawData = rawData.Where(workitem => workitem.AssignedToUserId == getWorkItemList.UserId);

			var filteredData = DataExtensions.OrderBy(rawData, getWorkItemList.ListRequests.SortColumn, getWorkItemList.ListRequests.Sort == "asc")
							 .Skip(getWorkItemList.ListRequests.PerPage * (getWorkItemList.ListRequests.Page - 1))
							 .Take(getWorkItemList.ListRequests.PerPage);

			var totalItems = await rawData.LongCountAsync();

			int totalPages = (int)Math.Ceiling(totalItems / (double)getWorkItemList.ListRequests.PerPage);
			var model = new PaginatedItems<WorkItemGridView>(getWorkItemList.ListRequests.Page, getWorkItemList.ListRequests.PerPage, totalPages, filteredData);

			return await Task.FromResult(model);
		}



		public async Task<IList<GetAllTokenView>> GetAllTokens(int workItemId)
		{
			var rawData = await(from w1 in _context.Tokens
						  join w2 in _context.WorkItemWorkItemTokens on w1.TokenId equals w2.TokenId
						  join w3 in _context.Tokens on w1.ParentTokenId equals w3.TokenId
						  join workItem in _context.WorkItemMasters on w2.WorkItemId equals workItem.WorkItemId
						   where workItem.WorkItemId == workItemId

						   select new GetAllTokenView()
						   {
							   
							   WorkItemId = workItem.WorkItemId,
							   TokenId=w2.TokenId,
							   TokenName=w1.TokenName,
							   ParentTokenId=w1.ParentTokenId,
							   ParentTokenName=w3.TokenName

							  
						   }).OrderByDescending(work => work.WorkItemId).ToListAsync();

			
			

			return await Task.FromResult(rawData);
		}



		public async Task<GetAllTokenView> GetTokenDetailsForRisk(int workitemId, int parentTokenId)
		{
			var rawData = (from w1 in _context.Tokens
						   join w2 in _context.WorkItemWorkItemTokens on w1.TokenId equals w2.TokenId
						   join w3 in _context.Tokens on w1.ParentTokenId equals w3.TokenId
						   join workItem in _context.WorkItemMasters on w2.WorkItemId equals workItem.WorkItemId
						   where workItem.WorkItemId == workitemId && w1.ParentTokenId == parentTokenId

						   select new GetAllTokenView()
						   {

							   WorkItemId = workItem.WorkItemId,
							   TokenId = w2.TokenId,
							   TokenName = w1.TokenName,
							   Weightage = w1.Weightage,
							   ParentTokenId = w1.ParentTokenId,
							   ParentTokenName = w3.TokenName


						   }).AsQueryable();
			return rawData.FirstOrDefault();
        }


		
		public async Task<WorkItemPreview> GetPreviewWorkItemById(int workItemId, int tenantId)
		{
			var rawData = (from work in _context.WorkItemMasters
						   join category in _context.MasterData on work.CategoryId equals category.Id into temp_category
						   from temp_category_tbl in temp_category.DefaultIfEmpty()
						   join status in _context.MasterData on work.StatusMasterDataId equals status.Id into temp_status
						   from subStatus in temp_status.DefaultIfEmpty()
						   join source in _context.MasterData on work.SourceId equals source.Id into md3
						   from subItem in md3.DefaultIfEmpty()
						   join assignedToUser in _context.UserMasters on work.AssignedToUserId equals assignedToUser.UserId into user
						   from subuser in user.DefaultIfEmpty()
						   join createdByUser in _context.UserMasters on work.CreatedBy equals createdByUser.UserId into user1
						   from subuser1 in user1.DefaultIfEmpty()
						   join updatedByUser in _context.UserMasters on work.UpdatedBy equals updatedByUser.UserId into user2
						   from subuser2 in user2.DefaultIfEmpty()
						   join responsibleUser in _context.UserMasters on work.ResponsibleUserId equals responsibleUser.UserId into user3
						   from subuser3 in user3.DefaultIfEmpty()
						   join department in _context.DepartmentMasters on work.DepartmentId equals department.DepartmentId into department
						   from subDepartment in department.DefaultIfEmpty()
						   join auditfindingMapping in _context.AuditFindingsMappings on work.WorkItemId equals auditfindingMapping.WorkItemId into finding
						   from subFinding in finding.DefaultIfEmpty()
						   join auditableItem in _context.AuditableItems on subFinding.AuditableItemId equals auditableItem.Id into item
						   from subItem1 in item.DefaultIfEmpty()
						   join workItem in _context.WorkItemMasters on work.SourceItemId equals workItem.WorkItemId into item1
						   from subitem1 in item1.DefaultIfEmpty()
						   where work.WorkItemId == workItemId

						   select new WorkItemPreview()
						   {
							   WorkItemId = work.WorkItemId,
							   WorkitemTypeId=work.WorkItemTypeId,
							   SourceId = work.SourceId,
							   Source = subItem.Items,
							   SourceItemId = work.SourceItemId,
							   SourceItem= subitem1.Title,
							   AuditableItemId = subFinding.AuditableItemId,
							   //AuditableItem = subItem1.AuditableItems,
							   Title = work.Title,
							   Description = work.Description,
							   CategoryId = work.CategoryId,
							   Category = temp_category_tbl.Items,
							   TenantId = work.TenantId,
							   DepartmentId = work.DepartmentId,
							   Department = subDepartment.DepartmentName,
							   StatusId = work.StatusMasterDataId,
							   Status = subStatus.Items,
							   AssignedToUserId = work.AssignedToUserId,
							   AssignedToUser = $"{subuser.FirstName} {subuser.LastName}",
							   ResponsibleUserId = work.ResponsibleUserId,
							   ResponsibleUser = $"{subuser3.FirstName} {subuser3.LastName}",
							   DueDate = work.DueDate,
							   CreatedBy = $"{subuser1.FirstName} {subuser1.LastName}",
							   CreatedById = work.CreatedBy,
							   UpdatedBy = $"{subuser2.FirstName} {subuser2.LastName}",
							   UpdatedById = work.UpdatedBy,
							   CreatedOn = work.CreatedOn,
							   UpdatedOn = work.UpdatedOn,
							   ParentWorkItemId = work.ParentWorkItemId,
						   }).AsQueryable();
			return rawData.FirstOrDefault();
		}

		public async Task UpdateWorkItem(WorkItemMaster workItemMaster)
		{
			await UpdateAsync(workItemMaster);
		}

		public async Task AddStandardToWorkItem(WorkItemPutView work, int workItemId)
		{
			var workItem = await _context.WorkItemMasters.FindAsync(workItemId);
			if (workItem.WorkItemId == workItemId)
			{
				var standards = _context.WorkItemStandards.Where(s => s.WorkItemId == workItem.WorkItemId).ToList();
				_context.WorkItemStandards.RemoveRange(standards);

				//ToDo- Add using ranges for standards
				var standardForWorkItem = new List<WorkItemStandard>();
				foreach (int standard in work.StandardId)
				{
					var newStandard = new WorkItemStandard
					{
						WorkItemId = workItem.WorkItemId,
						StandardId = standard,
					};
					standardForWorkItem.Add(newStandard);
				}

				await _context.WorkItemStandards.AddRangeAsync(standardForWorkItem);
				await _context.SaveChangesAsync();
			}
			else
			{
				throw new NotFoundException(string.Format(RepositoryConstant.IdNotFoundErrorMessage), workItem.WorkItemId);
			}
		}

		public async Task AddAuditableItem(int? auditableItemId, bool? followup, int workItemId, int? auditChecklistId)
		{
			AuditFindingsMapping audititems = new AuditFindingsMapping();
			audititems.AuditableItemId = auditableItemId;
			audititems.WorkItemId = workItemId;
			audititems.FollowUp = followup;
			audititems.AuditChecklistId = auditChecklistId;
			await _context.AuditFindingsMappings.AddAsync(audititems);
			await _context.SaveChangesAsync();
		}

		public async Task<WorkItemMaster> AddWorkItem(WorkItemDomainModel workitemDomainModel)
		{
			var workitemMaster = _mapper.Map<WorkItemMaster>(workitemDomainModel);
			if (workitemMaster.WorkItemTypeId == (int)IMSModules.CorrectiveAction)
			{
				workitemMaster.StatusMasterDataId = (int)IMSItemStatus.Assigned;
			}
			else
			{
				workitemMaster.StatusMasterDataId = (int)IMSItemStatus.New;
			}
			
			workitemMaster.AssignedToUserId = workitemDomainModel.AssignedToUserId;

			await _context.WorkItemMasters.AddAsync(workitemMaster);
			await _context.SaveChangesAsync();

			var workItemPutView = _mapper.Map<WorkItemPutView>(workitemDomainModel);
			await AddStandardToWorkItem(workItemPutView, workitemMaster.WorkItemId);
			return workitemMaster;
		}

        public async Task<IList<GetNcEmailDetails>> OverDueRemiderForNc()
		{
			var nc = await (from wi in _context.WorkItemMasters
							join us in _context.UserMasters on wi.ResponsibleUserId equals us.UserId
							join md in _context.MasterData on wi.CategoryId equals md.Id
							join md1 in _context.MasterData on wi.StatusMasterDataId equals md1.Id
							where wi.DueDate < DateTime.UtcNow && wi.StatusMasterDataId == (int)IMSItemStatus.Open && wi.TenantId != 12 && wi.WorkItemTypeId == (int)IMSModules.NonConformity && wi.StatusMasterDataId != (int)IMSItemStatus.Closed
                            select new GetNcEmailDetails
							{

								Id = wi.WorkItemId,
								Name = us.FirstName,
								Title = wi.Title,
								Description = wi.Description,
								Status = md1.Items,
								NCType = md.Items,
								EmailAddress = us.EmailId

							}).ToListAsync();
			return nc;
		}

        public async Task<IList<GetNcEmailDetails>> NightlyRemiderForNc()
        {
            var nc = await (from wi in _context.WorkItemMasters
                            join us in _context.UserMasters on wi.ResponsibleUserId equals us.UserId
                            join md in _context.MasterData on wi.CategoryId equals md.Id
                            join md1 in _context.MasterData on wi.StatusMasterDataId equals md1.Id
                            where wi.DueDate < DateTime.UtcNow.Date.AddDays(2) && wi.StatusMasterDataId == (int)IMSItemStatus.Open && wi.TenantId != 12 && wi.WorkItemTypeId == (int)IMSModules.NonConformity && wi.StatusMasterDataId != (int)IMSItemStatus.Closed
                            select new GetNcEmailDetails
                            {

                                Id = wi.WorkItemId,
                                Name = us.FirstName,
                                Title = wi.Title,
                                Description = wi.Description,
                                Status = md1.Items,
                                NCType = md.Items,
                                EmailAddress = us.EmailId

                            }).ToListAsync();
            return nc;
        }

        public async Task<IList<GetCaEmailDetails>> OverDueRemiderForCA()
        {
			var correctiveAction = await (from wi in _context.WorkItemMasters
										  join wi2 in _context.WorkItemMasters on wi.SourceItemId equals wi2.WorkItemId
										  join um in _context.UserMasters on wi.ResponsibleUserId equals um.UserId
										  where wi.DueDate < DateTime.UtcNow  && wi.WorkItemTypeId == (int)IMSModules.CorrectiveAction && wi.StatusMasterDataId != (int)IMSItemStatus.Closed && wi.TenantId != 12
                                          select new GetCaEmailDetails
										  {
											  CaId = wi.WorkItemId,
											  NcId = wi.SourceItemId,
											  NcTitle = wi2.Title,
											  Name = um.FirstName,
											  CaDescription = wi.Description,
											  CaTitle = wi.Title,
											  EmailAddress = um.EmailId

                                          }).ToListAsync();
            return correctiveAction;
        }
        public async Task<IList<GetCaEmailDetails>> NightlyRemiderForCA()
        {
            var correctiveAction = await (from wi in _context.WorkItemMasters
                                          join wi2 in _context.WorkItemMasters on wi.SourceItemId equals wi2.WorkItemId
                                          join um in _context.UserMasters on wi.ResponsibleUserId equals um.UserId
                                          where wi.DueDate < DateTime.UtcNow.Date.AddDays(2) &&  wi.WorkItemTypeId == (int)IMSModules.CorrectiveAction && wi.StatusMasterDataId != (int)IMSItemStatus.Closed && wi.TenantId != 12
                                          select new GetCaEmailDetails
                                          {
											  CaId = wi.WorkItemId,
                                              NcId = wi.SourceItemId,
                                              NcTitle = wi2.Title,
                                              Name = um.FirstName,
                                              CaDescription = wi.Description,
                                              CaTitle = wi.Title,
                                              EmailAddress = um.EmailId

                                          }).ToListAsync();
            return correctiveAction;
        }

        public async Task<IList<GetRiskEmailDetails>> OverDueRemiderForRisk()
        {
			var risk = await (from wi in _context.WorkItemMasters
							  join r in _context.Risks on wi.WorkItemId equals r.WorkItemId
							 
							  join um in _context.UserMasters on wi.ResponsibleUserId equals um.UserId
							  where wi.DueDate < DateTime.UtcNow && wi.WorkItemTypeId == (int)IMSModules.RiskManagement && wi.StatusMasterDataId != (int)IMSItemStatus.Closed && wi.TenantId != 12
                              select new GetRiskEmailDetails
							  {
								RiskId = wi.WorkItemId,
								RiskOwner = um.FirstName,
								Name = um.FirstName,
								RiskDate = r.InitialDate,
								Description = wi.Description,
								EmailAddress = um.EmailId,
								
								TotalScore = r.TotalRiskScore

							  } ).ToListAsync();
            return risk;
        }
        public async Task<IList<GetRiskEmailDetails>> NightlyRemiderForRisk()
        {
            var risk = await (from wi in _context.WorkItemMasters
                              join r in _context.Risks on wi.WorkItemId equals r.WorkItemId
                            
                              join um in _context.UserMasters on wi.ResponsibleUserId equals um.UserId
                              where wi.DueDate < DateTime.UtcNow.Date.AddDays(2) && wi.WorkItemTypeId == (int)IMSModules.RiskManagement && wi.StatusMasterDataId != (int)IMSItemStatus.Closed && wi.TenantId != 12 
                              select new GetRiskEmailDetails
                              {
                                  RiskId = wi.WorkItemId,
                                  RiskOwner = um.FirstName,
                                  Name = um.FirstName,
                                  RiskDate = r.InitialDate,
                                  Description = wi.Description,
                                  EmailAddress = um.EmailId,
                                
                                  TotalScore = r.TotalRiskScore

                              }).ToListAsync();
            return risk; 
        }

        public async Task<IList<EmailDetailsForOpportunity>> NightlyRemiderForOpportunity()
        {
            var opportunities = await (from wi in _context.WorkItemMasters
									   join um in _context.UserMasters on wi.ResponsibleUserId equals um.UserId
									   join dm in _context.DepartmentMasters on wi.DepartmentId equals dm.DepartmentId
                                       where wi.DueDate < DateTime.UtcNow.Date.AddDays(2) && wi.WorkItemTypeId == (int)IMSModules.Opportunity && wi.StatusMasterDataId != (int)IMSItemStatus.Closed && wi.TenantId != 12
                                       select new EmailDetailsForOpportunity
									  {
										  OpportunityId = wi.WorkItemId,
										  Name = um.FirstName,
										  Title = wi.Title,
										  Description = wi.Description,
										  EmailAddress = um.EmailId,
										  Department = dm.DepartmentName,
										  DueDate = wi.DueDate
                                 

									  }).ToListAsync();
            return opportunities; 
        }

        public async Task<IList<EmailDetailsForOpportunity>> OverDueRemiderForOpportunity()
        {
            var opportunities = await (from wi in _context.WorkItemMasters
                                       join um in _context.UserMasters on wi.ResponsibleUserId equals um.UserId
                                       join dm in _context.DepartmentMasters on wi.DepartmentId equals dm.DepartmentId
                                       where wi.DueDate < DateTime.UtcNow && wi.WorkItemTypeId == (int)IMSModules.Opportunity && wi.StatusMasterDataId != (int)IMSItemStatus.Closed && wi.TenantId != 12
                                       select new EmailDetailsForOpportunity
                                       {
                                           OpportunityId = wi.WorkItemId,
                                           Name = um.FirstName,
                                           Title = wi.Title,
                                           Description = wi.Description,
                                           EmailAddress = um.EmailId,
                                           Department = dm.DepartmentName,
                                           DueDate = wi.DueDate


                                       }).ToListAsync();
            return opportunities;
        }

        public async Task<IList<EmailDetailsForObservation>> NightlyRemiderForObservation()
        {
            var observation = await (from wi in _context.WorkItemMasters
                                       join um in _context.UserMasters on wi.ResponsibleUserId equals um.UserId
                                       join dm in _context.DepartmentMasters on wi.DepartmentId equals dm.DepartmentId
                                       where wi.DueDate < DateTime.UtcNow.Date.AddDays(2) && wi.WorkItemTypeId == (int)IMSModules.Observation && wi.StatusMasterDataId != (int)IMSItemStatus.Closed && wi.TenantId != 12
                                       select new EmailDetailsForObservation
                                       {
                                           ObservationId = wi.WorkItemId,
                                           Name = um.FirstName,
                                           Title = wi.Title,
                                           Description = wi.Description,
                                           EmailAddress = um.EmailId,
                                           Department = dm.DepartmentName,
                                           DueDate = wi.DueDate


                                       }).ToListAsync();
            return observation;
        }
		
        public async Task<IList<EmailDetailsForObservation>> OverDueRemiderForObservation()
        {
            var observation = await (from wi in _context.WorkItemMasters
                                       join um in _context.UserMasters on wi.ResponsibleUserId equals um.UserId
                                       join dm in _context.DepartmentMasters on wi.DepartmentId equals dm.DepartmentId
                                       where wi.DueDate < DateTime.UtcNow && wi.WorkItemTypeId == (int)IMSModules.Observation && wi.StatusMasterDataId != (int)IMSItemStatus.Closed && wi.TenantId != 12
                                       select new EmailDetailsForObservation
                                       {
                                           ObservationId = wi.WorkItemId,
                                           Name = um.FirstName,
                                           Title = wi.Title,
                                           Description = wi.Description,
                                           EmailAddress = um.EmailId,
                                           Department = dm.DepartmentName,
                                           DueDate = wi.DueDate


                                       }).ToListAsync();
            return observation;
        }


		public async Task<IList<WorkItemDropDownView>> GetNcDropDown(int tenantId)
		{
			var rawData = await (from nc in _context.WorkItemMasters
								 where nc.TenantId == tenantId && nc.WorkItemTypeId == 214
								 select new WorkItemDropDownView()
								 {
									 Value = nc.WorkItemId,
									 Label = nc.Title,
									 CategoryId = nc.CategoryId,

								 }).ToListAsync();
			return await Task.FromResult(rawData);
		}
		public async Task<IList<WorkItemDropDownView>> GetIncidentDropDown(int tenantId)
		{
			var rawData = await (from incident in _context.WorkItemMasters
								 where incident.TenantId == tenantId && incident.WorkItemTypeId == 219
								 select new WorkItemDropDownView()
								 {
									 Value = incident.WorkItemId,
									 Label = incident.Title,
									 CategoryId = incident.WorkItemTypeId,
								 }).ToListAsync();
			return await Task.FromResult(rawData);
		}

        public async Task<IList<EmailDetailsForIncident>> NightlyRemiderForIncident()
        {
            var incidents = await (from wi in _context.WorkItemMasters
                                     join um in _context.UserMasters on wi.ResponsibleUserId equals um.UserId
                                     join dm in _context.DepartmentMasters on wi.DepartmentId equals dm.DepartmentId
									 join incident in _context.IncidentMetaDatas on wi.WorkItemId equals incident.WorkItemId
                                     where wi.DueDate < DateTime.UtcNow.Date.AddDays(2) && wi.WorkItemTypeId == (int)IMSModules.IncidentManagement && wi.StatusMasterDataId != (int)IMSItemStatus.Closed && wi.TenantId != 12
                                     select new EmailDetailsForIncident
                                     {
                                         IncidentId = wi.WorkItemId,
                                         Name = um.FirstName,
                                         EmailAddress = um.EmailId,
                                         Title = wi.Title,
                                         IncidentDate = incident.DateOfIncident,
                                         Department = dm.DepartmentName,
                                         Description = wi.Description,
										 InjuryDescription  = incident.InjuryDescription,
										 HowItOccured =incident.HowItOccured,
                                         MedicalTreatment = incident.MedicalTreatment,
										 ClassificationDescription = incident.ClassificationDescription

                                     }).ToListAsync();
            return incidents;
        }
		
        public async Task<IList<EmailDetailsForIncident>> OverDueRemiderForIncident()
        {
            var incidents = await (from wi in _context.WorkItemMasters
                                     join um in _context.UserMasters on wi.ResponsibleUserId equals um.UserId
                                     join dm in _context.DepartmentMasters on wi.DepartmentId equals dm.DepartmentId
                                     join incident in _context.IncidentMetaDatas on wi.WorkItemId equals incident.WorkItemId
                                     where wi.DueDate < DateTime.UtcNow && wi.WorkItemTypeId == (int)IMSModules.IncidentManagement && wi.StatusMasterDataId != (int)IMSItemStatus.Closed && wi.TenantId != 12
                                     select new EmailDetailsForIncident
                                     {

                                         IncidentId = wi.WorkItemId,
                                         Name = um.FirstName,
                                         EmailAddress = um.EmailId,
                                         Title = wi.Title,
                                         IncidentDate = incident.DateOfIncident,
                                         Department = dm.DepartmentName,
                                         Description = wi.Description,
                                         InjuryDescription = incident.InjuryDescription,
                                         HowItOccured = incident.HowItOccured,
                                         MedicalTreatment = incident.MedicalTreatment,
                                         ClassificationDescription = incident.ClassificationDescription


                                     }).ToListAsync();
            return incidents;
        }
		public async Task<GetRiskByNc> GetRiskBy(int workItemId)
		{
			var risk = (from risks in _context.WorkItemMasters 
						where risks.SourceItemId == workItemId && risks.SourceId == (int)IMSModules.NonConformity
						select new GetRiskByNc()
						{
							Id = risks.WorkItemId,
							CreatedOn = risks.CreatedOn
						}).AsQueryable();
			return risk.FirstOrDefault();
		}

	}
}