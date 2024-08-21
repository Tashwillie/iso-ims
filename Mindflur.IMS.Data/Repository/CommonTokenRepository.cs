using Microsoft.Extensions.Logging;
using Mindflur.IMS.Application.Contracts.Repository;
using Mindflur.IMS.Application.ViewModel.Core;
using Mindflur.IMS.Application.ViewModel.View;
using Mindflur.IMS.Application.ViewModel;
using Mindflur.IMS.Data.Base;
using Mindflur.IMS.Data.Models;
using Mindflur.IMS.Data.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Mindflur.IMS.Data.Models.Custom;
using Dapper;
using Stripe;
using Mindflur.IMS.Application.Core.Enums;

namespace Mindflur.IMS.Data.Repository
{
    public class CommonTokenRepository : BaseRepository<CommonToken>, ICommonTokenRepository
    {

        private readonly IConfiguration _configuration;
        public CommonTokenRepository(IConfiguration configuration, IMSDEVContext dbContext, ILogger<CommonToken> logger) : base(dbContext, logger)
        {

            _configuration = configuration;
        }
        public async Task<PaginatedItems<CommonTokenView>> GetAllTokenByTenantId(GetTokenListRequest getListRequest)
        {
            var rawData = (from tn in _context.Tokens
                           join workItem in _context.Tokens on tn.ParentTokenId equals workItem.TokenId
                           select new CommonTokenView()
                           {
                               TokenId = tn.TokenId,

                               Weightage = tn.Weightage,
                               TokenName = tn.TokenName,
                               DisplayName = tn.DisplayName,
                               SequenceNumber = tn.SeqeunceNumber,
                               ParentTokenId = tn.ParentTokenId,
                               ParentToken = workItem.TokenName

                           }).OrderByDescending(token => token.TokenId).AsQueryable();
            if (getListRequest.ParentTokenId > 0)
            {
                rawData = rawData.Where(log => log.ParentTokenId == getListRequest.ParentTokenId);
            }

            var filteredData = DataExtensions.OrderBy(rawData, getListRequest.ListRequests.SortColumn, getListRequest.ListRequests.Sort == "asc")
            .Skip(getListRequest.ListRequests.PerPage * (getListRequest.ListRequests.Page - 1))
                              .Take(getListRequest.ListRequests.PerPage);

            var totalItems = await rawData.LongCountAsync();

            int totalPages = (int)Math.Ceiling(totalItems / (double)getListRequest.ListRequests.PerPage);
            var model = new PaginatedItems<CommonTokenView>(getListRequest.ListRequests.Page, getListRequest.ListRequests.PerPage, totalPages, filteredData);
            return await Task.FromResult(model);
        }
        public async Task<IList<IListTokenView>> GetTokenDetails(int parentTokenId, int tenantId)
        {
            var rawData = await (from tn in _context.Tokens

                                 where tn.ParentTokenId == parentTokenId
                                 select new IListTokenView()
                                 {
                                     TokenId = tn.TokenId,
                                     TokenName = tn.TokenName,
                                     DisplayName = tn.DisplayName,
                                     SequenceNumber = tn.SeqeunceNumber,
                                     Weightage = tn.Weightage,
                                     ParentTokenId = tn.ParentTokenId
                                 }

                           ).ToListAsync();
            return await Task.FromResult(rawData);
        }

        public async Task<CommonTokenDetailsView> GetDetails(int tokenId, int tenantId)
        {
            var rawData = (from tn in _context.Tokens
                           join token in _context.Tokens on tn.ParentTokenId equals token.TokenId
                           where tn.TokenId == tokenId
                           select new CommonTokenDetailsView()
                           {
                               TokenId = tn.TokenId,

                               Weightage = tn.Weightage,
                               TokenName = tn.TokenName,
                               DisplayName = tn.DisplayName,
                               SequenceNumber = tn.SeqeunceNumber,
                               ParentTokenId = tn.ParentTokenId,
                               ParentToken = token.TokenName
                           }).AsQueryable();
            return rawData.FirstOrDefault();
        }

        public async Task<IEnumerable<TokenListItemData>> getTokenResponseAsync(int tenantId)
        {

            using var conn = new SqlConnection(_configuration.GetConnectionString("DataConnectionString"));

            conn.Open();
            return await conn.QueryAsync<TokenListItemData>(
                @"with  cte
            (TokenId, TokenName, ParentTokenId,Parent)
            as (
                select w.TokenId, w.TokenName,w.ParentTokenId ,w2.TokenName as Parent from WorkItem_Tokens as w
                join WorkItem_Tokens as w2 on w.ParentTokenId=w2.TokenId
                where w.ParentTokenId = 89 and w.TenantId=1 
                union all select p.TokenId, p.TokenName, p.ParentTokenId,cte.TokenName as Parent from WorkItem_Tokens p 
                inner join cte on p.ParentTokenId = cte.TokenId ) select * from cte order by TokenId"

                );
        }

        public async Task<IList<CommonTokenDetailsView>> GetTokenDropdown(int tenantId)
        {
            var rawData = await (from tn in _context.Tokens
                                 join token in _context.Tokens on tn.ParentTokenId equals token.TokenId
                                 where tn.ParentTokenId == 28 && tn.TokenId!=(int)IMSTokenMaster.AuditFinding && tn.TokenId !=(int)IMSTokenMaster.AuditSystem
                                 select new CommonTokenDetailsView()
                                 {
                                     TokenId = tn.TokenId,

                                     Weightage = tn.Weightage,
                                     TokenName = tn.TokenName,
                                     DisplayName = tn.DisplayName,
                                     SequenceNumber = tn.SeqeunceNumber,
                                     ParentTokenId = tn.ParentTokenId,
                                     ParentToken = token.TokenName
                                 }).ToListAsync();
            return await Task.FromResult(rawData);


        }
    }
}
