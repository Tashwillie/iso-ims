using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Mindflur.IMS.Application.Contracts.Repository;
using Mindflur.IMS.Application.SupplierReports.Model;
using Mindflur.IMS.Application.ViewModel.Core;
using Mindflur.IMS.Application.ViewModel.View;
using Mindflur.IMS.Data.Base;
using Mindflur.IMS.Data.Extensions;
using Mindflur.IMS.Data.Models;

namespace Mindflur.IMS.Data.Repository
{
	public class SurveyMasterDatumRepository : BaseRepository<SurveyMasterDatum>, ISurveyMasterDatumRepository
	{
		public SurveyMasterDatumRepository(IMSDEVContext dbContext, ILogger<SurveyMasterDatum> logger) : base(dbContext, logger)
		{
		}

		public async Task<PaginatedItems<SurveyMasterDataGridView>> getAllSurveyData(GetSurveyListRequest getListRequest)
		{
			var data = (from survey in _context.SurveyMasterData
						join md in _context.MasterData on survey.MasterDataSurveyStatusId equals md.Id
						join tm in _context.TenanttMasters on survey.TenantId equals tm.TenantId
						where tm.TenantId == getListRequest.TenantId
						select new SurveyMasterDataGridView()
						{
							SurveyId = survey.SurveyId,
							Title = survey.Title,
							Description = survey.Description,
							StartDate = survey.StartDate,
							EndTime = survey.EndTime,
							MasterDataSurveyStatusId = md.Items,
							CreatedBy = survey.CreatedBy,
						}).AsQueryable();
			if (getListRequest.CreatedBy > 0)
				data = data.Where(log => log.CreatedBy == getListRequest.CreatedBy);
			var filteredData = DataExtensions.OrderBy(data, getListRequest.ListRequests.SortColumn, getListRequest.ListRequests.Sort == "asc").Skip(getListRequest.ListRequests.PerPage * (getListRequest.ListRequests.Page - 1)).Take(getListRequest.ListRequests.PerPage);
			var totalItems = await data.LongCountAsync();
			int totalPages = (int)Math.Ceiling(totalItems / (double)getListRequest.ListRequests.PerPage);
			var model = new PaginatedItems<SurveyMasterDataGridView>(getListRequest.ListRequests.Page, getListRequest.ListRequests.PerPage, totalPages, filteredData);
			return await Task.FromResult(model);
		}

		public async Task<SurveyDataPreview> GetSurveyDataPreviewById(int tenantId, int Id)
		{
			var data = (from survey in _context.SurveyMasterData
						join md in _context.MasterData on survey.MasterDataSurveyStatusId equals md.Id
						join tm in _context.TenanttMasters on survey.TenantId equals tm.TenantId
						join user in _context.UserMasters on survey.AssignedToUserId equals user.UserId
						join user1 in _context.UserMasters on survey.ResponsiblePersonId equals user1.UserId
						where survey.SurveyId == Id && survey.TenantId == tenantId

						select new SurveyDataPreview()
						{
							SurveyId = survey.SurveyId,
							MasterDataSurveyStatusId = survey.MasterDataSurveyStatusId,
							MasterDataSurveyStatus = md.Items,
							Title = survey.Title,
							Description = survey.Description,
							StartDate = survey.StartDate,
							EndTime = survey.EndTime,
							AssignedPersonId = survey.AssignedToUserId,
							AssignedPerson = $"{user.FirstName} {user.LastName}",
							ResponsiblePersonId = survey.ResponsiblePersonId,
							ResponsiblePerson = $"{user1.FirstName} {user1.LastName}",
						}).AsQueryable();
			return data.FirstOrDefault();
		}

		public async Task<SupplierDetails> GetSupplierDetails(int surveyId, int tenantId)
		{
			var rawData = (from survey in _context.SurveyMasterData
						   join Sup in _context.SurveySupplierMappings on survey.SurveyId equals Sup.SurveyMasterId
						   join supplier in _context.SupplierMasters on Sup.SupplierMasterId equals supplier.SupplierId
						   join user in _context.UserMasters on survey.ResponsiblePersonId equals user.UserId
						   where survey.SurveyId == surveyId && survey.TenantId == tenantId
						   select new SupplierDetails()
						   {
							   Supplier = supplier.SupplierName,
							   SupplierLocation = supplier.SupplierLocation,
							   SupplierContactDetails = supplier.ContactNumber,
							   ReviewedBy = $"{user.FirstName} {user.LastName}",
							   ReviewedDate = $"{survey.EndTime:dd MMMM yyyy}",
							   OverAllComments = survey.OverAllComments
						   }).AsQueryable();
			return rawData.FirstOrDefault();
		}

		public async Task<IList<SupplierSurveyDetails>> GetSupplierSurveyDetails(int surveyId, int tenantId)
		{
			var rawData = await (from surveyQuestion in _context.SurveyQuestions
								 join survey in _context.SurveyMasterData on surveyQuestion.SurveyId equals survey.SurveyId
								 join smd in _context.SurveySupplierMappings on surveyQuestion.SurveyId equals smd.SurveyMasterId
								 join offeredam in _context.OfferedAnswerMaster on surveyQuestion.OfferedAnswerId equals offeredam.SurveyOfferedAnswerId
								 join sqm in _context.QuestionMasters on surveyQuestion.QuestionId equals sqm.QuestionId
								 where surveyQuestion.SurveyId == surveyId
								 select new SupplierSurveyDetails()
								 {
									 Crieteria = sqm.Title,
									 Ratings = offeredam.Title,
									 Comments = surveyQuestion.Comments,
								 }).ToListAsync();
			return await Task.FromResult(rawData);
		}
	}
}