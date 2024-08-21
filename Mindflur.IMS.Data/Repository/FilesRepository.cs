using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Mindflur.IMS.Application.Contracts.Repository;
using Mindflur.IMS.Application.Core.Enums;
using Mindflur.IMS.Application.ViewModel.Core;
using Mindflur.IMS.Application.ViewModel.Model;
using Mindflur.IMS.Application.ViewModel.View;
using Mindflur.IMS.Data.Base;
using Mindflur.IMS.Data.Extensions;
using Mindflur.IMS.Data.Models;

namespace Mindflur.IMS.Data.Repository
{
    public class FilesRepository : BaseRepository<FileRepository>, IFilesRepository
    {
       
        public FilesRepository(IMSDEVContext dbContext, ILogger<FileRepository> logger) : base(dbContext, logger)
        {
            
        }

        public async Task<FileRepositoryView> GetThemeIconBySourceIdAndAttachementId(int tenantId)
        {
            var file = await (from _fileRepository in _context.FileRepositories
                              join _attachmentMaster in _context.MasterData on _fileRepository.SourceItemId equals _attachmentMaster.Id into _attachmentMaster
							  from subSourceItem in _attachmentMaster.DefaultIfEmpty()
							  join md1 in _context.MasterData on _fileRepository.SourceId equals md1.Id
                              where _fileRepository.TenantId == tenantId && _fileRepository.SourceId == (int)IMSMasterSiteContent.SiteLogo && _fileRepository.SourceItemId == (int)IMSModules.TenantTheme
                              select new FileRepositoryView()
                              {
                                  FileRepositoryId = _fileRepository.FileRepositoryId,
                                  TenantId = _fileRepository.TenantId,
                                  SourceItemId = _fileRepository.SourceItemId,
								  SourceItem = subSourceItem.Items,
								  SourceId = _fileRepository.SourceId,
								  Source = md1.Items,
                                  FilePath = _fileRepository.BlobStorageFilePath,
                                  FullName = _fileRepository.FullName,
                              }).FirstOrDefaultAsync();
            return file;
        }

        public async Task<FileRepositoryView> GetThemeLogoBySourceIdAndAttachementId(int tenantId)
        {
            var file = await (from _fileRepository in _context.FileRepositories
                              join _attachmentMaster in _context.MasterData on _fileRepository.SourceItemId equals _attachmentMaster.Id into _attachmentMaster
							  from subSourceItem in _attachmentMaster.DefaultIfEmpty()
							  join md1 in _context.MasterData on _fileRepository.SourceId equals md1.Id
                              where _fileRepository.TenantId == tenantId && _fileRepository.SourceId == (int)IMSMasterSiteContent.SiteIcon && _fileRepository.SourceItemId == (int)IMSModules.TenantTheme
							  select new FileRepositoryView()
                              {
                                  FileRepositoryId = _fileRepository.FileRepositoryId,
                                  TenantId = _fileRepository.TenantId,
								  SourceItemId = _fileRepository.SourceItemId,
								  SourceItem = subSourceItem.Items,
								  SourceId = _fileRepository.SourceId,
								  Source = md1.Items,
								  FilePath = _fileRepository.BlobStorageFilePath,
                                  FullName = _fileRepository.FullName,
                              }).FirstOrDefaultAsync();
            return file;
        }

        public async Task<FileRepositoryView> GetThemeCoverPathBySourceIdAndAttachementId(int tenantId)
        {
            var file = await (from _fileRepository in _context.FileRepositories
                              join _attachmentMaster in _context.MasterData on _fileRepository.SourceItemId equals _attachmentMaster.Id into _attachmentMaster
                              from subSourceItem in _attachmentMaster.DefaultIfEmpty()
							  join md1 in _context.MasterData on _fileRepository.SourceId equals md1.Id
                              where _fileRepository.TenantId == tenantId && _fileRepository.SourceId == (int)IMSModules.AuditFinding && _fileRepository.SourceItemId == (int)IMSModules.TenantTheme
                              select new FileRepositoryView()
                              {
                                  FileRepositoryId = _fileRepository.FileRepositoryId,
                                  TenantId = _fileRepository.TenantId,
								  SourceItemId = _fileRepository.SourceItemId,
								  SourceItem = subSourceItem.Items,
								  SourceId = _fileRepository.SourceId,
								  Source = md1.Items,
								  FilePath = _fileRepository.BlobStorageFilePath,
                                  FullName = _fileRepository.FullName,
                              }).FirstOrDefaultAsync();
            return file;
        }

        public async Task<FileRepositoryDomainModel> GetFileDetail(int fileId, int tenantId)
        {
            return await (from fileEntity in _context.FileRepositories
                          join _attachmentMaster in _context.MasterData on fileEntity.SourceItemId equals _attachmentMaster.Id into _attachmentMaster
                          from  subSourceItemId in _attachmentMaster.DefaultIfEmpty()
                          join source in _context.MasterData on fileEntity.SourceId equals source.Id into source 
                          from subsource in source.DefaultIfEmpty()
                              where fileEntity.TenantId == tenantId && fileEntity.FileRepositoryId == fileId
                              select new FileRepositoryDomainModel()
                              {
                                  FileRepositoryId = fileEntity.FileRepositoryId,
                                  TenantId = fileEntity.TenantId,
                                  SourceItemId = subSourceItemId.Items,
                                  SourceId = subsource.Items,
                                  IsPrivate = fileEntity.IsPrivate,
                                  BlobFileName = fileEntity.BlobFileName,
                                 
                                  FilePath = fileEntity.BlobStorageFilePath,
                                  FullName = fileEntity.FullName,
                              }).FirstOrDefaultAsync();
        }

        public async Task<PaginatedItems<FileRepositoryView>> GetFiles(GetFileListRequest getListRequest)
        {
            var rawData = (from file in _context.FileRepositories
                           join md in _context.MasterData on file.SourceItemId equals md.Id into master
                           from subSourceItem in master.DefaultIfEmpty()

                           join md1 in _context.MasterData on file.SourceId equals md1.Id into md1
                           from subSourceId in md1.DefaultIfEmpty()

                           where file.TenantId == getListRequest.TenantId && file.DeletedBy == null
						   select new FileRepositoryView()
                           {
                               FileRepositoryId = file.FileRepositoryId,
                               TenantId = file.TenantId,
                               SourceItemId = file.SourceItemId,
                               SourceItem= subSourceItem.Items,
                               Description= file.Description,
                               SourceId = file.SourceId,
                               Source= subSourceId.Items,
                               FilePath = file.BlobStorageFilePath,
                               FullName = file.FullName,
                           }
                         ).OrderByDescending(file=>file.FileRepositoryId).AsQueryable();
            if (getListRequest.SourceItemId > 0)
            {
                rawData=rawData.Where(log=>log.SourceItemId==getListRequest.SourceItemId);
            }
			if (getListRequest.SourceId > 0)
			{
				rawData = rawData.Where(log => log.SourceId == getListRequest.SourceId);
			}
			var filteredData = DataExtensions.OrderBy(rawData, getListRequest.ListRequests.SortColumn, getListRequest.ListRequests.Sort == "asc")
                               .Skip(getListRequest.ListRequests.PerPage * (getListRequest.ListRequests.Page - 1))
                               .Take(getListRequest.ListRequests.PerPage);

            var totalItems = await rawData.LongCountAsync();

            int totalPages = (int)Math.Ceiling(totalItems / (double)getListRequest.ListRequests.PerPage);
            var model = new PaginatedItems<FileRepositoryView>(getListRequest.ListRequests.Page, getListRequest.ListRequests.PerPage, totalPages, filteredData);
            return await Task.FromResult(model);
        }

        public async Task<IList<FileRepositoryView>> GetFilesBySourceId(int SourceId, int tenantId)
        {
            var rawData = await (from file in _context.FileRepositories
                                 join md in _context.MasterData on file.SourceItemId equals md.Id into md
								 from subSourceItem in md.DefaultIfEmpty()
								 join md1 in _context.MasterData on file.SourceId equals md1.Id

                                 where file.SourceId == SourceId && file.TenantId == tenantId
                                 select new FileRepositoryView()
                                 {
                                     FileRepositoryId = file.FileRepositoryId,
                                     TenantId = file.TenantId,
									 SourceItemId = file.SourceItemId,
									 SourceItem = subSourceItem.Items,
									 SourceId = file.SourceId,
									 Source = md1.Items,
									 FilePath = file.BlobStorageFilePath,
                                     FullName = file.FullName,
                                 }

                         ).ToListAsync();
            return await Task.FromResult(rawData);
        }
		public async Task<IList<FileRepositoryView>> GetFilesByAttachmentMasterId(int sourceItemId,int sourceId, int tenantId)
		{
			var rawData = await (from file in _context.FileRepositories
								 join md in _context.MasterData on file.SourceItemId equals md.Id into md from subSourceItem in md.DefaultIfEmpty()
								 join md1 in _context.MasterData on file.SourceId equals md1.Id

								 where file.SourceItemId == sourceItemId && file.TenantId == tenantId && file.SourceId == sourceId && file.DeletedBy==null
								 select new FileRepositoryView()
								 {
									 FileRepositoryId = file.FileRepositoryId,
									 TenantId = file.TenantId,
									 SourceItemId = file.SourceItemId,
									 SourceItem = subSourceItem.Items,
                                     Description = file.Description,
                                     SourceId = file.SourceId,
									 Source = md1.Items,
									 FilePath = file.BlobStorageFilePath,
									 FullName = file.FullName,
								 }

						 ).ToListAsync();
			return await Task.FromResult(rawData);
		}
	}
}