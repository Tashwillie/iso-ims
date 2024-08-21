using Mindflur.IMS.Application.Contracts.Business;
using Mindflur.IMS.Application.Contracts.Repository;
using Mindflur.IMS.Application.Contracts.Service;
using Mindflur.IMS.Application.Core.Constants;
using Mindflur.IMS.Application.Core.Enums;
using Mindflur.IMS.Application.Core.Exceptions;
using Mindflur.IMS.Application.ViewModel.Core;
using Mindflur.IMS.Application.ViewModel.View;
using Mindflur.IMS.Data.Models;

namespace Mindflur.IMS.Business
{
    public class MasterDataBusiness : IMasterDataBusiness
    {

        private readonly IMasterDataRepository _masterDataRepository;
        private readonly IMessageService _messageService;


        public MasterDataBusiness(IMasterDataRepository masterDataRepository, IMessageService messageService)
        {
            _masterDataRepository = masterDataRepository;
            _messageService = messageService;

        }

        public async Task<MasterDatum> GetMasterDataById(int masterId, int tenantId)
        {
            var masterData = await _masterDataRepository.GetByIdAsync(masterId);
            if (masterData == null)
            {
                throw new NotFoundException(string.Format(ConstantsBusiness.MasterDataNotFoundErrorMessage), masterId);
            }
            else
            {
                return masterData.Id == masterId ? masterData : throw new FileNotFoundException();
            }


        }

        public async Task<MasterDatum> UpdateMasterData(MasterDataView masterData, int masterId, int tenantId)
        {
            var masterDatas = await _masterDataRepository.GetByIdAsync(masterId);
            if (masterDatas == null)
            {
                throw new NotFoundException(string.Format(ConstantsBusiness.MasterDataNotFoundErrorMessage), masterId);
            }
            else if (masterDatas.Id == masterId)
            {
                masterDatas.Items = masterData.Items;

                masterDatas.MasterDataGroupId = masterData.MasterDataGroupId;
                masterDatas.ParentId = masterData.ParentId;
                masterDatas.OrderId = masterData.OrderId;
                masterDatas.Active = masterData.Active;
                await _masterDataRepository.UpdateAsync(masterDatas);

                await _messageService.SendNotificationMessage(new NotificationMessage()
                {
                    SourceIdUserId = masterDatas.Id, //ToDo: Replace this with a valid userId
                    EventType = NotificationEventType.TenantMaster,
                    BroadcastLevel = NotificationBroadcastLevel.None,
                    TenantId = tenantId,
                    Action = IMSControllerActionCategory.Edit,
                    Module = IMSControllerCategory.MasterData,
                    ItemId = tenantId,
                    Description = masterDatas.Items + " has Updated ",
                    Title = masterDatas.Items,
                    Date = DateTime.UtcNow
                });

                return masterDatas;
            }
            else
            {
                throw new FileNotFoundException(); //ToDo: replace this exception with throw item not found exception.
            }
        }

        public async Task DeleteMasterData(int masterId, int tenantId)
        {
            var masterDatas = await _masterDataRepository.GetByIdAsync(masterId);
            if (masterDatas == null)
            {
                throw new NotFoundException(string.Format(ConstantsBusiness.MasterDataNotFoundErrorMessage), masterId);
            }
            else if (masterDatas.Id == masterId)
            {
                await _masterDataRepository.DeleteAsync(masterDatas);
                await _messageService.SendNotificationMessage(new NotificationMessage()
                {
                    SourceIdUserId = masterDatas.Id, //ToDo: Replace this with a valid userId
                    EventType = NotificationEventType.TenantMaster,
                    BroadcastLevel = NotificationBroadcastLevel.None,
                    TenantId = tenantId,
                    Action = IMSControllerActionCategory.Delete,
                    Module = IMSControllerCategory.MasterData,
                    ItemId = tenantId,
                    Description = masterDatas.Items + " is Created ",
                    Title = masterDatas.Items,
                    Date = DateTime.UtcNow
                });
            }
            else
            {
                throw new FileNotFoundException();
            }
        }

        public async Task<IList<SelectView>> GetMasterData(int tenantId)
        {
            return await _masterDataRepository.GetMasterData(tenantId);

        }
        public async Task<IList<SelectView>> getDataByMasterGroupId(int tenantId, int masterDataGroupId)

        {
            return await _masterDataRepository.getDataByMasterGroupId(tenantId, masterDataGroupId);
        }
        public async Task addMasterData(MasterDataView masterDataView, int tenantId)
        {
            MasterDatum masterDatas = new MasterDatum();
            masterDatas.Items = masterDataView.Items;

            masterDatas.MasterDataGroupId = masterDataView.MasterDataGroupId;
            masterDatas.ParentId = masterDataView.ParentId;
            masterDatas.OrderId = masterDataView.OrderId;
            masterDatas.Active = masterDataView.Active;
            await _masterDataRepository.AddAsync(masterDatas);

            await _messageService.SendNotificationMessage(new NotificationMessage()
            {
                SourceIdUserId = masterDatas.Id, //ToDo: Replace this with a valid userId
                EventType = NotificationEventType.TenantMaster,
                BroadcastLevel = NotificationBroadcastLevel.None,
                TenantId = tenantId,
                Action = IMSControllerActionCategory.Create,
                Module = IMSControllerCategory.MasterData,
                ItemId = tenantId,
                Description = masterDatas.Items + " is Created ",
                Title = masterDatas.Items,
                Date = DateTime.UtcNow
            });
        }
        public async Task<PaginatedItems<MasterDataListView>> getMasterDataList(GetMasterDataList getMasterDataList)
        {
            return await _masterDataRepository.getMasterDataList(getMasterDataList);
        }
        public async Task<MasterDataPreView> getPreviewData(int tenantId, int Id)
        {
            return await _masterDataRepository.getPreviewData(tenantId, Id);
        }
    }
}