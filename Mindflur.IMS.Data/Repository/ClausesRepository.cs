using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Identity.Client;
using Mindflur.IMS.Application.Contracts.Repository;
using Mindflur.IMS.Application.ViewModel.Core;
using Mindflur.IMS.Application.ViewModel.Model;
using Mindflur.IMS.Application.ViewModel.View;
using Mindflur.IMS.Data.Base;
using Mindflur.IMS.Data.Extensions;
using Mindflur.IMS.Data.Models;
using Mindflur.IMS.Data.Models.Custom;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mindflur.IMS.Data.Repository
{
    public class ClausesRepository : BaseRepository<Clause>, IClauseRepository
    {
       
        private readonly IConfiguration _configuration; 

        public ClausesRepository(IMSDEVContext dbContext, ILogger<Clause> logger,IConfiguration configuration) : base(dbContext, logger)
        {
            
            _configuration = configuration;
        }
        public async Task<PaginatedItems<GetClauseView>> GetClausesList(GetClauseListRequest getListRequest)
        {
            var rawData = (from clause in _context.Clauses
                           join clause2 in _context.Clauses on clause.ParentId equals clause2.ClauseId  into clause3
                           from Subclause in clause3.DefaultIfEmpty()
                           join md in _context.MasterData on clause.StandardId equals md.Id into standards
                           from substandard in standards.DefaultIfEmpty()                         
                           select new GetClauseView()
                           {
                               ClauseId = clause.ClauseId,
                               StandardId = clause.StandardId,
                               StandardName = substandard.Items,
                               ClauseNumberText = clause.ClauseNumberText,
                               DisplayText = clause.DisplayText,
                               ParentId = clause.ParentId,
                               Parent= clause.ClauseNumberText, 
							   SequenceNumber = clause.SequenceNumber,
                           }).OrderByDescending(si => si.ClauseId).AsQueryable();

            if (getListRequest.ParentId > 0)
                rawData = rawData.Where(log => log.ParentId == getListRequest.ParentId);

            if (getListRequest.StandardId > 0)
                rawData = rawData.Where(log => log.StandardId == getListRequest.StandardId);

            var filteredData = DataExtensions.OrderBy(rawData, getListRequest.ListRequests.SortColumn, getListRequest.ListRequests.Sort == "asc")
            .Skip(getListRequest.ListRequests.PerPage * (getListRequest.ListRequests.Page - 1))
                              .Take(getListRequest.ListRequests.PerPage);

            var totalItems = await rawData.LongCountAsync();

            int totalPages = (int)Math.Ceiling(totalItems / (double)getListRequest.ListRequests.PerPage);
            var model = new PaginatedItems<GetClauseView>(getListRequest.ListRequests.Page, getListRequest.ListRequests.PerPage, totalPages, filteredData);
            return await Task.FromResult(model);


        }

        public async Task<IList<GetClauseView>> GetClauseByParentId(int parentId, int tenantId)
        {
            var rawData = await (from clause in _context.Clauses
                                 join md in _context.MasterData on clause.StandardId equals md.Id into standards
                                 from substandard in standards.DefaultIfEmpty()
                                 where clause.ParentId == parentId
                                 select new GetClauseView()
                                 {
                                     ClauseId = clause.ClauseId,
                                     StandardId = clause.StandardId,
                                     StandardName = substandard.Items,
                                     ClauseNumberText = clause.ClauseNumberText,
                                     DisplayText = clause.DisplayText,
                                     ParentId = clause.ParentId,
                                     SequenceNumber = clause.SequenceNumber,

                                 }).ToListAsync();
            return await Task.FromResult(rawData);


        }

        public async Task<GetClauseIdView> GetClauseByClauseId(int clauseId, int tenantId)
        {
            {
                var data = (from clause in _context.Clauses
                                  join md in _context.MasterData on clause.StandardId equals md.Id into standards
                            from substandard in standards.DefaultIfEmpty()
                            where clause.ClauseId == clauseId
                                  select new GetClauseIdView()
                                  {
                                      ClauseId = clause.ClauseId,
                                      StandardId = clause.StandardId,
                                      Standard = substandard.Items,
                                      ClauseNumberText = clause.ClauseNumberText,
                                      DisplayText = clause.DisplayText,
                                      ParentId = clause.ParentId,
                                      Parent = $"{clause.ClauseNumberText}  {clause.DisplayText}",
                                      SequenceNumber = clause.SequenceNumber,

                                  }).AsQueryable();
                return data.FirstOrDefault();
            }

        }

        public async Task<IList<GetClauseDropdown>> GetDropDown(int tenant)
        {
            var data = await (from clause in _context.Clauses
                        
                       
                      
                        select new GetClauseDropdown()
                        {
                            Id = clause.ClauseId,
                            Name = clause.ClauseNumberText,
                            ParentId = clause.ParentId,
                        }).ToListAsync();
            return await Task.FromResult(data);
        }
		public async Task<IEnumerable<ClausesListItemData>> getClausesResponseAsync(int tenantId)
		{
			using var conn = new SqlConnection(_configuration.GetConnectionString("DataConnectionString"));

			conn.Open();
			return await conn.QueryAsync<ClausesListItemData>(
                @"with  cte (ClauseId, ClauseNumberText,DisplayText,ParentId,Parent) as 
(  select c.ClauseId, c.ClauseNumberText,c.DisplayText, c.ParentId ,c2.ClauseNumberText as Parent from Clauses as c
join Clauses as c2 on c.ParentId=c2.ClauseId where c.ParentId = 109
union all select p.ClauseId, p.ClauseNumberText,p.DisplayText,p.ParentId,cte.ClauseNumberText as Parent  from Clauses p 
inner join cte on p.ParentId = cte.ClauseId
)select * from cte Order by ClauseId;", new { tenantId }

				);
		}

        public async Task<IList<GetStandardView>> GetClauseByStandardId(int standardId)
        {
            var rawData =  await (from clause in _context.Clauses
                           where clause.StandardId == standardId
                                  select new GetStandardView()
                           {
                                  ClauseId= clause.ClauseId,
                                  StandardId = clause.StandardId,
                                  ClauseNumberText = $"{clause.ClauseNumberText}   {clause.DisplayText}",
                                       

                           }).ToListAsync();
            return  await Task.FromResult(rawData);

        }

        public async Task<IList<GetClauseNumberTextView>> GetClausenNmberText(string clauseNumberText)
        {
            var rawData = await (from clause in _context.Clauses
                                 where clause.ClauseNumberText == clauseNumberText
                                 select new GetClauseNumberTextView()
                                 {
                                     ClauseId = clause.ClauseId,
                                     ClauseNumberText = clause.ClauseNumberText,
                                     DisplayText = clause.DisplayText,
                                     ParentId = clause.ParentId,
                                 }).ToListAsync();
            return await Task.FromResult(rawData);
        }
    }
}


