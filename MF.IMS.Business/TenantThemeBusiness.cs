using Microsoft.Extensions.Options;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using Mindflur.IMS.Application.Contracts.Business;
using Mindflur.IMS.Application.Contracts.Repository;
using Mindflur.IMS.Application.Contracts.Service;
using Mindflur.IMS.Application.Core;
using Mindflur.IMS.Application.Core.Enums;
using Mindflur.IMS.Application.ViewModel.Core;
using Mindflur.IMS.Application.ViewModel.Model;
using Mindflur.IMS.Data.Models;
using Mindflur.IMS.Data.Repository;
using NUnit.Framework.Internal.Execution;

namespace Mindflur.IMS.Business
{
    public class TenantThemeBusiness : ITenantThemeBusiness
    {
        private readonly ITenantThemeRepository _tenantThemeRepository;
        private readonly IOptions<CoreSettings> _coreSettings;
        private readonly IFilesRepository _filesRepository;
        private readonly IMessageService _messageService;
		private readonly IUserRepository _userRepository;
		public TenantThemeBusiness(IOptions<CoreSettings> coreSettings, ITenantThemeRepository tenantThemeRepository, IFilesRepository filesRepository, IMessageService messageService, IUserRepository userRepository)
		{
			_tenantThemeRepository = tenantThemeRepository;
			_coreSettings = coreSettings;
			_filesRepository = filesRepository;
			_messageService = messageService;
			_userRepository = userRepository;
		}

		public async Task<GetTenantTheme> GetThemeByShortCode(string shortCode)
        {
            return await _tenantThemeRepository.GetThemeByShortCode(shortCode);
        }

        public async Task<GetTenantTheme> GetTheme(int tenantId)
        {
            return await _tenantThemeRepository.GetTheme(tenantId);
        }
        public async Task UpdateTheme(int tenantId, PutTenantTheme putTenantTheme, int userId)
        {
            var rawData = await _tenantThemeRepository.GetThemeByTenantId(tenantId);
            if (rawData == null)
            {
                rawData = new TenantTheme();
                rawData.TenantId = tenantId;
                rawData.ApplicationTitle = putTenantTheme.ApplicationTitle;

                rawData.ApplicationDescription = putTenantTheme.ApplicationDescription;
                rawData.ApplicationFooterTextLeft = putTenantTheme.ApplicationFooterTextLeft;
                rawData.ApplicationFooterTextRight = putTenantTheme.ApplicationFooterTextRight;
                rawData.ApplicationFooterTextLinkLeft = putTenantTheme.ApplicationFooterTextLinkLeft;
                rawData.ApplicationFooterTextLinkRight = putTenantTheme.ApplicationFooterTextLinkRight;
                rawData.Primary = putTenantTheme.Primary;
                rawData.Secondory = putTenantTheme.Secondary;
                rawData.Success = putTenantTheme.Success;
                rawData.Danger = putTenantTheme.Danger;
                rawData.Warning = putTenantTheme.Warning;
                rawData.Info = putTenantTheme.Info;
                rawData.Dark = putTenantTheme.Dark;
                rawData.Light = putTenantTheme.Light;
                rawData.ThemeDarkBodyBg = putTenantTheme.ThemeDarkBodyBg;
                rawData.ThemeDarkBodyColor = putTenantTheme.ThemeDarkBodyColor;
                rawData.ThemeDarkBorderColor = putTenantTheme.ThemeDarkBorderColor;
                rawData.ThemeDarkCustomControlBorderColor = putTenantTheme.ThemeDarkCustomControlBorderColor;
                rawData.ThemeDarkHeadingsColor = putTenantTheme.ThemeDarkHeadingsColor;
                rawData.ThemeDarkLabelColor = putTenantTheme.ThemeDarkLabelColor;
                rawData.ThemeDarkTextMutedColor = putTenantTheme.ThemeDarkTextMutedColor;
                rawData.ThemeDarkCardBg = putTenantTheme.ThemeDarkCardBg;
                rawData.ThemeDarkInputBg = putTenantTheme.ThemeDarkInputBg;
                rawData.ThemeDarkInputDisabledBg = putTenantTheme.ThemeDarkInputDisabledBg;
                rawData.ThemeDarkInputDisabledBorderColor = putTenantTheme.ThemeDarkInputDisabledBorderColor;
                rawData.ThemeDarkChartBg = putTenantTheme.ThemeDarkChartBg;
                rawData.ThemeDarkSwitchBg = putTenantTheme.ThemeDarkSwitchBg;
                rawData.ThemeDarkSwitchBgDisabled = putTenantTheme.ThemeDarkSwitchBgDisabled;
                rawData.ThemeDarkTableBg = putTenantTheme.ThemeDarkTableBg;
                rawData.ThemeDarkTableHeaderBg = putTenantTheme.ThemeDarkTableHeaderBg;
                rawData.ThemeDarkTableRowBg = putTenantTheme.ThemeDarkTableRowBg;
                rawData.ThemeDarkTableHoverBg = putTenantTheme.ThemeDarkTableHoverBg;
                rawData.ThemeDarkTableStripedBg = putTenantTheme.ThemeDarkTableStripedBg;
                rawData.ThemeDarkModalHeaderBg = putTenantTheme.ThemeDarkModalHeaderBg;
                rawData.ThemeDarkPaginationBg = putTenantTheme.ThemeDarkPaginationBg;
                rawData.ThemeDarkInputPlaceholderColor = putTenantTheme.ThemeDarkInputPlaceholderColor;
                rawData.ThemeDarkInputBorderColor = putTenantTheme.ThemeDarkInputBorderColor;
                rawData.ThemeDarkWidgetBg = putTenantTheme.ThemeDarkWidgetBg;
                rawData.CreatedBy = userId;
                rawData.CreatedOn = DateTime.UtcNow;
                await _tenantThemeRepository.AddAsync(rawData);
            }
            else
            {
                rawData.TenantId = tenantId;

                rawData.ApplicationTitle = putTenantTheme.ApplicationTitle;
                rawData.ApplicationDescription = putTenantTheme.ApplicationDescription;
                rawData.ApplicationFooterTextLeft = putTenantTheme.ApplicationFooterTextLeft;
                rawData.ApplicationFooterTextRight = putTenantTheme.ApplicationFooterTextRight;
                rawData.ApplicationFooterTextLinkLeft = putTenantTheme.ApplicationFooterTextLinkLeft;
                rawData.ApplicationFooterTextLinkRight = putTenantTheme.ApplicationFooterTextLinkRight;
                rawData.Primary = putTenantTheme.Primary;
                rawData.Secondory = putTenantTheme.Secondary;
                rawData.Success = putTenantTheme.Success;
                rawData.Danger = putTenantTheme.Danger;
                rawData.Warning = putTenantTheme.Warning;
                rawData.Info = putTenantTheme.Info;
                rawData.Dark = putTenantTheme.Dark;
                rawData.Light = putTenantTheme.Light;
                rawData.ThemeDarkBodyBg = putTenantTheme.ThemeDarkBodyBg;
                rawData.ThemeDarkBodyColor = putTenantTheme.ThemeDarkBodyColor;
                rawData.ThemeDarkBorderColor = putTenantTheme.ThemeDarkBorderColor;
                rawData.ThemeDarkCustomControlBorderColor = putTenantTheme.ThemeDarkCustomControlBorderColor;
                rawData.ThemeDarkHeadingsColor = putTenantTheme.ThemeDarkHeadingsColor;
                rawData.ThemeDarkLabelColor = putTenantTheme.ThemeDarkLabelColor;
                rawData.ThemeDarkTextMutedColor = putTenantTheme.ThemeDarkTextMutedColor;
                rawData.ThemeDarkCardBg = putTenantTheme.ThemeDarkCardBg;
                rawData.ThemeDarkInputBg = putTenantTheme.ThemeDarkInputBg;
                rawData.ThemeDarkInputDisabledBg = putTenantTheme.ThemeDarkInputDisabledBg;
                rawData.ThemeDarkInputDisabledBorderColor = putTenantTheme.ThemeDarkInputDisabledBorderColor;
                rawData.ThemeDarkChartBg = putTenantTheme.ThemeDarkChartBg;
                rawData.ThemeDarkSwitchBg = putTenantTheme.ThemeDarkSwitchBg;
                rawData.ThemeDarkSwitchBgDisabled = putTenantTheme.ThemeDarkSwitchBgDisabled;
                rawData.ThemeDarkTableBg = putTenantTheme.ThemeDarkTableBg;
                rawData.ThemeDarkTableHeaderBg = putTenantTheme.ThemeDarkTableHeaderBg;
                rawData.ThemeDarkTableRowBg = putTenantTheme.ThemeDarkTableRowBg;
                rawData.ThemeDarkTableHoverBg = putTenantTheme.ThemeDarkTableHoverBg;
                rawData.ThemeDarkTableStripedBg = putTenantTheme.ThemeDarkTableStripedBg;
                rawData.ThemeDarkModalHeaderBg = putTenantTheme.ThemeDarkModalHeaderBg;
                rawData.ThemeDarkPaginationBg = putTenantTheme.ThemeDarkPaginationBg;
                rawData.ThemeDarkInputPlaceholderColor = putTenantTheme.ThemeDarkInputPlaceholderColor;
                rawData.ThemeDarkInputBorderColor = putTenantTheme.ThemeDarkInputBorderColor;
                rawData.ThemeDarkWidgetBg = putTenantTheme.ThemeDarkWidgetBg;
                rawData.CreatedBy = rawData.CreatedBy;
                rawData.CreatedOn = rawData.CreatedOn;
                rawData.UpdatedBy = userId;
                rawData.UpdatedOn = DateTime.UtcNow;
                await _tenantThemeRepository.UpdateAsync(rawData);
            }
			var userDetails = await _userRepository.GetUserDetail(userId, tenantId);
			await _messageService.SendNotificationMessage(new NotificationMessage()
            {
                EventType = NotificationEventType.GlobalMaster,
                BroadcastLevel = NotificationBroadcastLevel.None,
                SourceIdUserId = userId,
				SourceIdUser = $"{userDetails.FirstName} {userDetails.LastName}",
				TenantId = tenantId,
                Action = IMSControllerActionCategory.Edit,
                Module = IMSControllerCategory.ThemeMaster,
                ItemId = tenantId,
                Description ="Tenant theme has updated",
                Title = "Tenant theme has updated",
                Date = rawData.UpdatedOn
            });
        }


        public async Task<Stream?> GetIcon(int tenantId)
        {
            var fileDetails = await _filesRepository.GetThemeIconBySourceIdAndAttachementId(tenantId);

            var file = await _filesRepository.GetFileDetail(fileId: fileDetails.FileRepositoryId, tenantId: tenantId);

            if (file == null)
                return null;

            CloudBlockBlob blockBlob;
            await using (MemoryStream memoryStream = new MemoryStream())
            {
                CloudStorageAccount cloudStorageAccount = CloudStorageAccount.Parse(_coreSettings.Value.BlobConnectionString);
                CloudBlobClient cloudBlobClient = cloudStorageAccount.CreateCloudBlobClient();
                CloudBlobContainer cloudBlobContainer = cloudBlobClient.GetContainerReference("manish");
                blockBlob = cloudBlobContainer.GetBlockBlobReference(file.FileName.ToString());
                await blockBlob.DownloadToStreamAsync(memoryStream);
            }
            return await blockBlob.OpenReadAsync();


        }
        public async Task<Stream?> GetLogo(int tenantId)
        {
            var fileDetails = await _filesRepository.GetThemeLogoBySourceIdAndAttachementId(tenantId);

            var file = await _filesRepository.GetFileDetail(fileId: fileDetails.FileRepositoryId, tenantId: tenantId);

            if (file == null)
                return null;

            CloudBlockBlob blockBlob;
            await using (MemoryStream memoryStream = new MemoryStream())
            {
                CloudStorageAccount cloudStorageAccount = CloudStorageAccount.Parse(_coreSettings.Value.BlobConnectionString);
                CloudBlobClient cloudBlobClient = cloudStorageAccount.CreateCloudBlobClient();
                CloudBlobContainer cloudBlobContainer = cloudBlobClient.GetContainerReference("manish");
                blockBlob = cloudBlobContainer.GetBlockBlobReference(file.FileName.ToString());
                await blockBlob.DownloadToStreamAsync(memoryStream);
            }
            return await blockBlob.OpenReadAsync();


        }
        public async Task<Stream?> GetCoverPath(int tenantId)
        {
            var fileDetails = await _filesRepository.GetThemeCoverPathBySourceIdAndAttachementId(tenantId);

            var file = await _filesRepository.GetFileDetail(fileId: fileDetails.FileRepositoryId, tenantId: tenantId);

            if (file == null)
                return null;

            CloudBlockBlob blockBlob;
            await using (MemoryStream memoryStream = new MemoryStream())
            {
                CloudStorageAccount cloudStorageAccount = CloudStorageAccount.Parse(_coreSettings.Value.BlobConnectionString);
                CloudBlobClient cloudBlobClient = cloudStorageAccount.CreateCloudBlobClient();
                CloudBlobContainer cloudBlobContainer = cloudBlobClient.GetContainerReference("manish");
                blockBlob = cloudBlobContainer.GetBlockBlobReference(file.FileName.ToString());
                await blockBlob.DownloadToStreamAsync(memoryStream);
            }
            return await blockBlob.OpenReadAsync();


        }
        public async Task UpdateIconTenantTheme(int tenantId, PutIconTenantTheme putTenantTheme, int userId)
        {
            var tenantTheme = await _tenantThemeRepository.GetThemeByTenantId(tenantId);
            if (tenantTheme == null)
            {
                tenantTheme = new TenantTheme();
            }
            tenantTheme.SiteIconPathId = putTenantTheme.SiteIconPath;

            await _tenantThemeRepository.UpdateAsync(tenantTheme);

			var userDetails = await _userRepository.GetUserDetail(userId, tenantId);
			await _messageService.SendNotificationMessage(new NotificationMessage()
            {
                EventType = NotificationEventType.GlobalMaster,
                BroadcastLevel = NotificationBroadcastLevel.None,
                SourceIdUserId = userId,
				SourceIdUser = $"{userDetails.FirstName} {userDetails.LastName}",
				TenantId = tenantId,
                Action = IMSControllerActionCategory.Edit,
                Module = IMSControllerCategory.ThemeMaster,
                ItemId = tenantId,
                Description = "Tenant theme has updated",
                Title = "Tenant theme has updated",
				Date = tenantTheme.UpdatedOn
			});
        }
        public async Task UpdateLogoTenantTheme(int tenantId, PutIconTenantTheme putTenantTheme, int userId)
        {
            var tenantTheme = await _tenantThemeRepository.GetThemeByTenantId(tenantId);
            if (tenantTheme == null)
            {
                tenantTheme = new TenantTheme();
            }

            tenantTheme.SiteLogoPathId = putTenantTheme.SiteLogoPath;

            await _tenantThemeRepository.UpdateAsync(tenantTheme);
			var userDetails = await _userRepository.GetUserDetail(userId, tenantId);
			await _messageService.SendNotificationMessage(new NotificationMessage()
            {
                EventType = NotificationEventType.GlobalMaster,
                BroadcastLevel = NotificationBroadcastLevel.None,
                SourceIdUserId = userId,
				SourceIdUser = $"{userDetails.FirstName} {userDetails.LastName}",
				TenantId = tenantId,
                Action = IMSControllerActionCategory.Edit,
                Module = IMSControllerCategory.ThemeMaster,
                ItemId = tenantId,
                Description = "Tenant theme has updated",
                Title = "Tenant theme has updated",
				Date = tenantTheme.UpdatedOn
			});
        }
        public async Task UpdateCoverTenantTheme(int tenantId, PutIconTenantTheme putTenantTheme, int userId)
        {
            var tenantTheme = await _tenantThemeRepository.GetThemeByTenantId(tenantId);
            if (tenantTheme == null)
            {
                tenantTheme = new TenantTheme();
            }

            tenantTheme.CoverPathId = putTenantTheme.SiteCoverPath;
            await _tenantThemeRepository.UpdateAsync(tenantTheme);
			var userDetails = await _userRepository.GetUserDetail(userId, tenantId);
			await _messageService.SendNotificationMessage(new NotificationMessage()
            {
                EventType = NotificationEventType.GlobalMaster,
                BroadcastLevel = NotificationBroadcastLevel.None,
                SourceIdUserId = userId,
				SourceIdUser = $"{userDetails.FirstName} {userDetails.LastName}",
				TenantId = tenantId,
                Action = IMSControllerActionCategory.Edit,
                Module = IMSControllerCategory.ThemeMaster,
                ItemId = tenantId,
                Description = "Tenant theme has updated",
                Title = "Tenant theme has updated",
                Date = DateTime.UtcNow
            });
        }
		public async Task<GetIconByTenantId> GetTenantIconBytenant(int tenantId)
		{
			return await _tenantThemeRepository.GetTenantIconBytenant(tenantId);
		}
	}


}
