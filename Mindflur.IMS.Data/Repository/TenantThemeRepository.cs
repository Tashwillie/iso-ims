using DocumentFormat.OpenXml.Spreadsheet;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Mindflur.IMS.Application.Contracts.Repository;
using Mindflur.IMS.Application.Contracts.Service;
using Mindflur.IMS.Application.Core.Constants;
using Mindflur.IMS.Application.ViewModel.Model;
using Mindflur.IMS.Data.Base;
using Mindflur.IMS.Data.Models;

namespace Mindflur.IMS.Data.Repository
{
    public class TenantThemeRepository : BaseRepository<TenantTheme>, ITenantThemeRepository
    {
        private readonly ICacheService _cacheService;
        public TenantThemeRepository(IMSDEVContext dbContext, ILogger<TenantTheme> logger, ICacheService cacheService) : base(dbContext, logger)
        {
            _cacheService = cacheService;
        }
        public async Task<GetTenantTheme> GetTheme(int tenantId)
        {
            var rawData = (from theme in _context.TenantThemes
                           join file in _context.FileRepositories on theme.CoverPathId equals file.FileRepositoryId into file
                           from subfile in file.DefaultIfEmpty()
                           join file1 in _context.FileRepositories on theme.SiteIconPathId equals file1.FileRepositoryId into file1
                           from subfile1 in file1.DefaultIfEmpty()
                           join file2 in _context.FileRepositories on theme.SiteLogoPathId equals file2.FileRepositoryId into file2
                           from subfile2 in file2.DefaultIfEmpty()
                           where theme.TenantId == tenantId
                           select new GetTenantTheme()
                           {
                               ApplicationDescription = theme.ApplicationDescription,
                               ApplicationTitle = theme.ApplicationTitle,
                               ApplicationFooterTextLeft = theme.ApplicationFooterTextLeft,
                               ApplicationFooterTextLinkLeft = theme.ApplicationFooterTextLinkLeft,
                               ApplicationFooterTextLinkRight = theme.ApplicationFooterTextLinkRight,
                               ApplicationFooterTextRight = theme.ApplicationFooterTextRight,
                               SiteIconPath = subfile1.BlobStorageFilePath,
                               SiteLogoPath = subfile2.BlobStorageFilePath,
                               SiteCoverPath = subfile.BlobStorageFilePath,
                               Primary = theme.Primary,
                               Secondary = theme.Secondory,
                               Success = theme.Success,
                               Danger = theme.Danger,
                               Warning = theme.Warning,
                               Info = theme.Info,
                               Dark = theme.Dark,
                               Light = theme.Light,
                               ThemeDarkBodyBg = theme.ThemeDarkBodyBg,
                               ThemeDarkBodyColor = theme.ThemeDarkBodyColor,
                               ThemeDarkBorderColor = theme.ThemeDarkBorderColor,
                               ThemeDarkCustomControlBorderColor = theme.ThemeDarkCustomControlBorderColor,
                               ThemeDarkHeadingsColor = theme.ThemeDarkHeadingsColor,
                               ThemeDarkLabelColor = theme.ThemeDarkLabelColor,
                               ThemeDarkTextMutedColor = theme.ThemeDarkTextMutedColor,
                               ThemeDarkCardBg = theme.ThemeDarkCardBg,
                               ThemeDarkInputBg = theme.ThemeDarkInputBg,
                               ThemeDarkInputPlaceholderColor = theme.ThemeDarkInputPlaceholderColor,
                               ThemeDarkInputBorderColor = theme.ThemeDarkInputBorderColor,
                               ThemeDarkInputDisabledBg = theme.ThemeDarkInputDisabledBg,
                               ThemeDarkInputDisabledBorderColor = theme.ThemeDarkInputDisabledBorderColor,
                               ThemeDarkSwitchBg = theme.ThemeDarkSwitchBg,
                               ThemeDarkSwitchBgDisabled = theme.ThemeDarkSwitchBgDisabled,
                               ThemeDarkTableBg = theme.ThemeDarkTableBg,
                               ThemeDarkTableHeaderBg = theme.ThemeDarkTableHeaderBg,
                               ThemeDarkTableRowBg = theme.ThemeDarkTableRowBg,
                               ThemeDarkTableHoverBg = theme.ThemeDarkTableHoverBg,
                               ThemeDarkTableStripedBg = theme.ThemeDarkTableStripedBg,
                               ThemeDarkModalHeaderBg = theme.ThemeDarkModalHeaderBg,
                               ThemeDarkPaginationBg = theme.ThemeDarkPaginationBg,
                               ThemeDarkChartBg = theme.ThemeDarkChartBg,
                               ThemeDarkWidgetBg = theme.ThemeDarkWidgetBg
                           }).FirstOrDefault();
            return rawData;
        }
        public async Task<TenantTheme> GetThemeByTenantId(int tenantId)
        {
            return await _context.TenantThemes.FirstOrDefaultAsync(theme => theme.TenantId == tenantId);
        }

        public async Task<GetTenantTheme> GetThemeByShortCode(string shortCode)
        {
            if (!_cacheService.TryGet(CacheKeysConstants.GetAllTheme(), out IList<GetTenantTheme> cachedItem))
            {
                cachedItem = await GetTheme();

                _cacheService.Set(CacheKeysConstants.GetAllTheme(), cachedItem);
            }

            return cachedItem.Where(predicate: theme => theme.ShortCode == shortCode).SingleOrDefault();
        }

        public async Task<IList<GetTenantTheme>> GetTheme()
        {
            return await (from tenant in _context.TenanttMasters
                          join theme in _context.TenantThemes on tenant.TenantId equals theme.TenantId
                          join file in _context.FileRepositories on theme.CoverPathId equals file.FileRepositoryId into file
                          from subfile in file.DefaultIfEmpty()
                          join file1 in _context.FileRepositories on theme.SiteIconPathId equals file1.FileRepositoryId into file1
                          from subfile1 in file1.DefaultIfEmpty()
                          join file2 in _context.FileRepositories on theme.SiteLogoPathId equals file2.FileRepositoryId into file2
                          from subfile2 in file2.DefaultIfEmpty()
                          select new GetTenantTheme()
                          {
                              ShortCode = tenant.ShortCode,
                              ApplicationDescription = theme.ApplicationDescription,
                              ApplicationTitle = theme.ApplicationTitle,
                              ApplicationFooterTextLeft = theme.ApplicationFooterTextLeft,
                              ApplicationFooterTextLinkLeft = theme.ApplicationFooterTextLinkLeft,
                              ApplicationFooterTextLinkRight = theme.ApplicationFooterTextLinkRight,
                              ApplicationFooterTextRight = theme.ApplicationFooterTextRight,
                              SiteIconPath = subfile1.BlobStorageFilePath,
                              SiteLogoPath = subfile2.BlobStorageFilePath,
                              SiteCoverPath = subfile.BlobStorageFilePath,
                              Primary = theme.Primary,
                              Secondary = theme.Secondory,
                              Success = theme.Success,
                              Danger = theme.Danger,
                              Warning = theme.Warning,
                              Info = theme.Info,
                              Dark = theme.Dark,
                              Light = theme.Light,
                              ThemeDarkBodyBg = theme.ThemeDarkBodyBg,
                              ThemeDarkBodyColor = theme.ThemeDarkBodyColor,
                              ThemeDarkBorderColor = theme.ThemeDarkBorderColor,
                              ThemeDarkCustomControlBorderColor = theme.ThemeDarkCustomControlBorderColor,
                              ThemeDarkHeadingsColor = theme.ThemeDarkHeadingsColor,
                              ThemeDarkLabelColor = theme.ThemeDarkLabelColor,
                              ThemeDarkTextMutedColor = theme.ThemeDarkTextMutedColor,
                              ThemeDarkCardBg = theme.ThemeDarkCardBg,
                              ThemeDarkInputBg = theme.ThemeDarkInputBg,
                              ThemeDarkInputPlaceholderColor = theme.ThemeDarkInputPlaceholderColor,
                              ThemeDarkInputBorderColor = theme.ThemeDarkInputBorderColor,
                              ThemeDarkInputDisabledBg = theme.ThemeDarkInputDisabledBg,
                              ThemeDarkInputDisabledBorderColor = theme.ThemeDarkInputDisabledBorderColor,
                              ThemeDarkSwitchBg = theme.ThemeDarkSwitchBg,
                              ThemeDarkSwitchBgDisabled = theme.ThemeDarkSwitchBgDisabled,
                              ThemeDarkTableBg = theme.ThemeDarkTableBg,
                              ThemeDarkTableHeaderBg = theme.ThemeDarkTableHeaderBg,
                              ThemeDarkTableRowBg = theme.ThemeDarkTableRowBg,
                              ThemeDarkTableHoverBg = theme.ThemeDarkTableHoverBg,
                              ThemeDarkTableStripedBg = theme.ThemeDarkTableStripedBg,
                              ThemeDarkModalHeaderBg = theme.ThemeDarkModalHeaderBg,
                              ThemeDarkPaginationBg = theme.ThemeDarkPaginationBg,
                              ThemeDarkChartBg = theme.ThemeDarkChartBg,
                              ThemeDarkWidgetBg = theme.ThemeDarkWidgetBg
                          })
                    .ToListAsync();
        }


        public async Task<GetIconByTenantId> GetTenantIconBytenant(int tenantId)
        {
            var rawData = (from theme in _context.TenantThemes
                                 join file in _context.FileRepositories on theme.SiteLogoPathId equals file.FileRepositoryId into file
                                 from subfile in file.DefaultIfEmpty()
                                                             where theme.TenantId == tenantId
                                 select new GetIconByTenantId
                                 {
                                     IconPath = subfile.BlobStorageFilePath,

								 }).AsQueryable();
			return rawData.FirstOrDefault();
		}

	}
}
