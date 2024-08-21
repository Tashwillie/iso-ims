using DocumentFormat.OpenXml.Office2010.Excel;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Mindflur.IMS.Application.Contracts.Repository;
using Mindflur.IMS.Application.ViewModel.Core;
using Mindflur.IMS.Application.ViewModel.View;
using Mindflur.IMS.Data.Base;
using Mindflur.IMS.Data.Extensions;
using Mindflur.IMS.Data.Models;

namespace Mindflur.IMS.Data.Repository
{
    public class SurveySupplierMappingRepository : BaseRepository<SurveySupplierMapping>, ISurveySupplierMappingRepository
    {
        public SurveySupplierMappingRepository(IMSDEVContext context, ILogger<SurveySupplierMapping> logger) : base(context, logger)
        {
        }
        public async Task<PaginatedItems<SurveyMasterDataGridView>> GetAllSurveyForSupplier(GetListSurveySupplier getListRequest)
        {
            var data = (from ssm in _context.SurveySupplierMappings
                        join survey in _context.SurveyMasterData on ssm.SurveyMasterId equals survey.SurveyId
                        join md in _context.MasterData on survey.MasterDataSurveyStatusId equals md.Id
                        join supplier in _context.SupplierMasters on ssm.SupplierMasterId equals supplier.SupplierId
                        join tm in _context.TenanttMasters on survey.TenantId equals tm.TenantId

                        where tm.TenantId == getListRequest.TenantId && supplier.SupplierId == getListRequest.SupplierId
                        select new SurveyMasterDataGridView
                        {
                            SurveyId = survey.SurveyId,
                            Title = survey.Title,
                            Description = survey.Description,
                            StartDate = survey.StartDate,
                            EndTime = survey.EndTime,
                            MasterDataSurveyStatusId = md.Items,
                            CreatedBy = survey.CreatedBy,
                            AssignedToUserId = survey.AssignedToUserId,


                        }).OrderByDescending(survey  => survey.SurveyId).AsQueryable();
			if (getListRequest.AssignedById > 0)
				data = data.Where(log => log.AssignedToUserId == getListRequest.AssignedById);
			if (getListRequest.CreatedById > 0)
				data = data.Where(log => log.CreatedBy == getListRequest.CreatedById);

			var filteredData = DataExtensions.OrderBy(data, getListRequest.ListRequests.SortColumn, getListRequest.ListRequests.Sort == "asc").Skip(getListRequest.ListRequests.PerPage * (getListRequest.ListRequests.Page - 1)).Take(getListRequest.ListRequests.PerPage);
            var totalItems = await data.LongCountAsync();
            int totalPages = (int)Math.Ceiling(totalItems / (double)getListRequest.ListRequests.PerPage);
            var model = new PaginatedItems<SurveyMasterDataGridView>(getListRequest.ListRequests.Page, getListRequest.ListRequests.PerPage, totalPages, filteredData);
            return await Task.FromResult(model);
        }
        public async Task<IList<SurveyDataView>> GetSurveys(int supplierId)
        {
            var survey = await (from ssm in _context.SurveySupplierMappings
                                join sm in _context.SurveyMasterData on ssm.SurveyMasterId equals sm.SurveyId
                                join supplier in _context.SupplierMasters on ssm.SupplierMasterId equals supplier.SupplierId
                                where ssm.SupplierMasterId == supplierId
                                select new SurveyDataView
                                {
                                    SurveyId = sm.SurveyId,
                                    SurveyTitle = sm.Title

                                }).ToListAsync();
            return await Task.FromResult(survey);
        }

		public async Task<SupplierIdFromSurveyId> getSupplierId(int surveyId, int tenantId)
        {
			var rawData = (from survey in _context.SurveyMasterData
						   join supplier in _context.SurveySupplierMappings on survey.SurveyId equals supplier.SurveyMasterId
						   join tm in _context.TenanttMasters on survey.TenantId equals tm.TenantId
                           where survey.SurveyId == surveyId
						 
						   select new SupplierIdFromSurveyId()
						   {
							   SupplierId = supplier.SupplierMasterId,
							  

						   }).AsQueryable();
			return rawData.FirstOrDefault();
		}


	}
}
