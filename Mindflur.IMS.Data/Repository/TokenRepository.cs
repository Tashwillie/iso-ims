using Microsoft.Extensions.Logging;
using Mindflur.IMS.Application.Contracts.Repository;
using Mindflur.IMS.Application.ViewModel.Core;
using Mindflur.IMS.Application.ViewModel.View;
using Mindflur.IMS.Data.Base;
using Mindflur.IMS.Data.Models;

namespace Mindflur.IMS.Data.Repository
{
    public class TokenRepository : BaseRepository<CommonToken>, ITokenRepository
    {
        private readonly ILogger _logger;

        public TokenRepository(IMSDEVContext dbContext, ILogger<CommonToken> logger) : base(dbContext, logger)
        {
            
            _logger = logger;
        }

        public Task<CommonToken> GetDetail(int tenantId, int Id)
        {
            throw new NotImplementedException();
        }

        public Task<IList<CommonToken>> GetTokens(int tenantId, int parentTokenId)
        {
            throw new NotImplementedException();
        }

        public Task<PaginatedItems<TokenListView>> GetTokens(GetTokenListRequest getTokenListRequest)
        {
            throw new NotImplementedException();
        }
    }
}