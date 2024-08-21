using Azure;
using Azure.Storage.Blobs;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using Mindflur.IMS.Application.Contracts.Business;
using Mindflur.IMS.Application.Contracts.Repository;
using Mindflur.IMS.Application.Core;
using Mindflur.IMS.Application.Core.Constants;
using Mindflur.IMS.Application.Core.Exceptions;
using Mindflur.IMS.Application.ViewModel.Core;
using Mindflur.IMS.Application.ViewModel.Model;
using Mindflur.IMS.Application.ViewModel.View;
using Mindflur.IMS.Data.Models;

namespace Mindflur.IMS.Business
{
	public class FileRepsoitoryBusiness : IFileRepositoryBusiness
	{
		private readonly IFilesRepository _filesRepository;
		private readonly IOptions<CoreSettings> _coreSettings;

		private readonly ILogger<FileRepsoitoryBusiness> _logger;

		public FileRepsoitoryBusiness(IOptions<CoreSettings> coreSettings, ILogger<FileRepsoitoryBusiness> logger, IFilesRepository filesRepository)
		{
			_logger = logger;
			_filesRepository = filesRepository;
			_coreSettings = coreSettings;
		}

		public async Task<PaginatedItems<FileRepositoryView>> GetFiles(GetFileListRequest getListRequest)
		{
			return await _filesRepository.GetFiles(getListRequest);
		}

		public async Task DeleteFile(int Id, int tenantId)
		{
			var file = await _filesRepository.GetByIdAsync(Id);
			if (file.FileRepositoryId == Id && file.TenantId == tenantId)
			{
				file.DeletedOn = DateTime.UtcNow;
				file.DeletedBy = 4;
				await _filesRepository.UpdateAsync(file);
			}
			else
			{
				throw new NotFoundException(string.Format(ConstantsBusiness.InvaliedTenantOrIdErrorMessage), Id);
			}
		}

		public async Task<IList<FileRepositoryView>> GetFilesBySourceId(int SourceId, int tenantId)
		{
			return await _filesRepository.GetFilesBySourceId(tenantId, SourceId);
		}

		public async Task<IList<FileRepositoryView>> GetFilesByAttachmentMasterId(int sourceItemId, int sourceId, int tenantId)
		{
			return await _filesRepository.GetFilesByAttachmentMasterId(sourceItemId, sourceId, tenantId);
		}

		public async Task<FileRepository> AddFile(IFormFile file, int tenantId, PostFileRepositoryView postFileRepositoryView, int userId)
		{
			if (file.Length > 15728640)
			{
				throw new Exception("File size exceeds the maximum allowed limit.");
			}
			string fileExtension = Path.GetExtension(file.FileName).ToLower();
			if (fileExtension != ".png" && fileExtension != ".jpg" && fileExtension != ".jpeg" && fileExtension != ".pdf")
			{
				throw new Exception("Only PNG, JPEG, and PDF files are allowed.");
			}
			FileRepository fileEntity = new FileRepository
			{
				TenantId = tenantId,
				SourceItemId = postFileRepositoryView.SourceItemId,
				SourceId = postFileRepositoryView.SourceId,
				Description = postFileRepositoryView.Description,
				CreatedBy = userId,
				CreatedOn = DateTime.UtcNow,
				FullName = file.FileName,
				BlobFileName = $"{Guid.NewGuid()}-{file.FileName}" //Use following name to upload the file, this would make sure if the file with same name already existed then it would not override it.
			};

			CloudStorageAccount cloudStorageAccount = CloudStorageAccount.Parse(_coreSettings.Value.BlobConnectionString);
			CloudBlobClient blobClient = cloudStorageAccount.CreateCloudBlobClient();
			string containerName = postFileRepositoryView.IsPrivate ? tenantId.ToString().PadLeft(6, '0') : "assets";

			CloudBlobContainer container = blobClient.GetContainerReference(containerName);
			if (!await container.ExistsAsync())
			{
				await CreateContainer(containerName);
			}

			CloudBlockBlob blockBlob = container.GetBlockBlobReference(fileEntity.BlobFileName);

			await using (var source = file.OpenReadStream())
			{
				await blockBlob.UploadFromStreamAsync(source);
			}

			fileEntity.BlobStorageFilePath = blockBlob.StorageUri.PrimaryUri.AbsoluteUri;
			await _filesRepository.AddAsync(fileEntity);

			return fileEntity;
		}

		public async Task<FileDownloadDomain> DownloadFile(int fileId, int tenantId)
		{
			var file = await _filesRepository.GetFileDetail(fileId: fileId, tenantId: tenantId);

			if (file != null)
			{
				FileDownloadDomain fileDownload = new FileDownloadDomain();
				string containerName = "assets"; //ToDo: Replace this name to be picked from configuration. This is a public container location

				if (file.IsPrivate==false)
				{
					containerName = tenantId.ToString().PadLeft(6, '0'); //This is a private container reference.
				}

				CloudBlockBlob blockBlob;
				await using (MemoryStream memoryStream = new MemoryStream())
				{
					CloudStorageAccount cloudStorageAccount = CloudStorageAccount.Parse(_coreSettings.Value.BlobConnectionString);
					CloudBlobClient cloudBlobClient = cloudStorageAccount.CreateCloudBlobClient();
					CloudBlobContainer cloudBlobContainer = cloudBlobClient.GetContainerReference(containerName);
					blockBlob = cloudBlobContainer.GetBlockBlobReference(file.BlobFileName);
					await blockBlob.DownloadToStreamAsync(memoryStream);
				}

				fileDownload.Content = await blockBlob.OpenReadAsync();
				fileDownload.FileName = file.FullName;
				fileDownload.ContentType = "application/pdf";

				return fileDownload;
			}
			else
			{
				var fileDownload = new FileDownloadDomain();
				return fileDownload;
			}
		}

		public async Task CreateContainer(string containerName)
		{
			try
			{
				BlobServiceClient blobServiceClient = new BlobServiceClient(_coreSettings.Value.BlobConnectionString);
				BlobContainerClient container = await blobServiceClient.CreateBlobContainerAsync(containerName);
				if (await container.ExistsAsync())
				{
					_logger.LogInformation($"New storage container '{containerName}' has been created");
				}
			}
			catch (RequestFailedException e)
			{
				_logger.LogError($"Unable to create container '{containerName}'", e);
			}
		}
	}
}