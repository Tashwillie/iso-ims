using Mindflur.IMS.Application.Contracts.Business;
using Mindflur.IMS.Application.Contracts.Repository;
using Mindflur.IMS.Application.Contracts.Service;
using Mindflur.IMS.Application.Core.Constants;
using Mindflur.IMS.Application.Core.Enums;
using Mindflur.IMS.Application.Core.Exceptions;
using Mindflur.IMS.Application.ViewModel.Core;
using Mindflur.IMS.Application.ViewModel.Model;
using Mindflur.IMS.Application.ViewModel.View;
using Mindflur.IMS.Data.Models;
using Mindflur.IMS.Data.Repository;
using NUnit.Framework.Internal.Execution;

namespace Mindflur.IMS.Business
{
    public class SupplierMasterBusiness : ISupplierMasterBusiness
    {
        private readonly ISupplierMasterRepository _supplierMasterRepository;
        private readonly IActivityLogRepository _activityLogRepository;
        private readonly IMessageService _messageService;
		private readonly IUserRepository _userRepository;
		public SupplierMasterBusiness(ISupplierMasterRepository supplierMasterRepository, IActivityLogRepository activityLogRepository, IMessageService messageService, IUserRepository userRepository)
		{
			_supplierMasterRepository = supplierMasterRepository;
			_activityLogRepository = activityLogRepository;
			_messageService = messageService;
			_userRepository = userRepository;
		}

		public async Task<PaginatedItems<GetSupplierGridView>> getAllSupplierDetails(GetSupplierListRequest getListRequest)
        {
            return await _supplierMasterRepository.getAllSupplierDetails(getListRequest);
        }
        public async Task<SupplierPreview> getSupplierPreviewById(int Id, int tenantId)
        {
            return await _supplierMasterRepository.getSupplierPreviewById(Id, tenantId);
        }
        public async Task<SupplierMaster> getAllSupplierDetailById(int id, int tenantId)
        {
            var rawData = await _supplierMasterRepository.GetByIdAsync(id);
            return rawData.SupplierId == id && rawData.TenantId == tenantId ? rawData : throw new NotFoundException("Id NOt Found", id);


        }
        public async Task deleteSupplierDetails(int Id, int userId, int tenantId)
        {
            var rawData = await _supplierMasterRepository.GetByIdAsync(Id);
            if (rawData == null)
            {
                throw new NotFoundException(string.Format(ConstantsBusiness.IdNotFoundErrorMessage), Id);
            }
            else if (rawData.SupplierId == Id && rawData.TenantId == tenantId)
            {
                await _supplierMasterRepository.DeleteAsync(rawData);
                ActivityLog activityLog = new ActivityLog();
                activityLog.TenantId = rawData.TenantId;
                activityLog.ControllerId = (int)IMSControllerCategory.SupplierManagement;
                activityLog.EntityId = rawData.SupplierId;
                activityLog.ModuleAction = (int)IMSControllerActionCategory.Edit;
                activityLog.Description = "Supplier Master Has Been Deleted";
                activityLog.Details = System.Text.Json.JsonSerializer.Serialize(rawData);
                activityLog.Status = true;
                activityLog.CreatedBy = userId;
                activityLog.CreatedOn = DateTime.UtcNow;
                await _activityLogRepository.AddAsync(activityLog);
            }
            else
            {
                throw new FileNotFoundException();
            }


        }
        public async Task<IList<SupplierDropDownView>> GetSupplierDropDown(int tenantId)
        {
            return await _supplierMasterRepository.GetSupplierDropDown(tenantId);
        }

        public async Task UpdateSupplierDetails(int Id, PutSupplierView supplierMaster, int userId, int tenantId)
        {
            var rawData = await _supplierMasterRepository.GetByIdAsync(Id);
            if (rawData == null)
            {
                throw new NotFoundException(string.Format(ConstantsBusiness.IdNotFoundErrorMessage), Id);
            }
            else if (rawData.SupplierId == Id && rawData.TenantId == tenantId)
            {
                rawData.SupplierName = supplierMaster.SupplierName;
                rawData.EmailAddress = supplierMaster.EmailAddress;
                rawData.ContactPerson = supplierMaster.ContactPerson;
                rawData.ContactNumber = supplierMaster.ContactNumber;
                rawData.SupplierLocation = supplierMaster.SupplierLocation;
                rawData.DepartmentId = supplierMaster.DepartmentId;
                rawData.MasterDataSupplierStatusId = supplierMaster.MasterDataSupplierStatusId;
                rawData.UpdatedBy = userId;
                rawData.UpdatedOn = DateTime.UtcNow;
                await _supplierMasterRepository.UpdateAsync(rawData);

				var userDetails = await _userRepository.GetUserDetail(userId, tenantId);
				await _messageService.SendNotificationMessage(new NotificationMessage()
                {
                    SourceIdUserId = userId,
					SourceIdUser = $"{userDetails.FirstName} {userDetails.LastName}",
					Action = IMSControllerActionCategory.Create,
                    Module = IMSControllerCategory.SupplierManagement,
                    ItemId = rawData.SupplierId,
                    Description = null,
                    Title = null,
					Date = rawData.CreatedOn
				});

                ActivityLog activityLog = new ActivityLog();
                activityLog.TenantId = rawData.TenantId;
                activityLog.ControllerId = (int)IMSControllerCategory.SupplierManagement;
                activityLog.EntityId = rawData.SupplierId;
                activityLog.ModuleAction = (int)IMSControllerActionCategory.Edit;
                activityLog.Description = "Supplier Master Has Been Updated";
                activityLog.Details = System.Text.Json.JsonSerializer.Serialize(rawData);
                activityLog.Status = true;
                activityLog.CreatedBy = userId;
                activityLog.CreatedOn = DateTime.UtcNow;
                await _activityLogRepository.AddAsync(activityLog);
            }
            else
            {

                throw new FileNotFoundException();

            }

        }
        public async Task AddSupplierDetails(PostSupplierView postSupplierView, int userId, int tenantId)
        {

            SupplierMaster supplierMaster = new SupplierMaster();
            supplierMaster.TenantId = tenantId;
            supplierMaster.SupplierName = postSupplierView.SupplierName;
            supplierMaster.EmailAddress = postSupplierView.EmailAddress;
            supplierMaster.ContactPerson = postSupplierView.ContactPerson;
            supplierMaster.ContactNumber = postSupplierView.ContactNumber;
            supplierMaster.SupplierLocation = postSupplierView.SupplierLocation;
            supplierMaster.DepartmentId= postSupplierView.DepartmentId;
            supplierMaster.MasterDataSupplierStatusId = 15;
            supplierMaster.CreatedBy = userId;
            supplierMaster.CreatedOn = DateTime.UtcNow;
            await _supplierMasterRepository.AddAsync(supplierMaster);

			var userDetails = await _userRepository.GetUserDetail(userId, tenantId);
			await _messageService.SendNotificationMessage(new NotificationMessage()
            {
                SourceIdUserId = userId,
				SourceIdUser = $"{userDetails.FirstName} {userDetails.LastName}",
				Action = IMSControllerActionCategory.Create,
                Module = IMSControllerCategory.SupplierManagement,
                ItemId = supplierMaster.SupplierId,
                Description = null,
                Title = null,
				Date = supplierMaster.CreatedOn
			});

            ActivityLog activityLog = new ActivityLog();
            activityLog.TenantId = supplierMaster.TenantId;
            activityLog.ControllerId = (int)IMSControllerCategory.SupplierManagement;
            activityLog.EntityId = supplierMaster.SupplierId;
            activityLog.ModuleAction = (int)IMSControllerActionCategory.Create;
            activityLog.Description = "Supplier Master Has Been Created";
            activityLog.Details = System.Text.Json.JsonSerializer.Serialize(supplierMaster);
            activityLog.Status = true;
            activityLog.CreatedBy = userId;
            activityLog.CreatedOn = DateTime.UtcNow;
            await _activityLogRepository.AddAsync(activityLog);
        }
    }
}
