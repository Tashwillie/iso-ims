using Mindflur.IMS.Application.Contracts.Business;
using Mindflur.IMS.Application.Contracts.Repository;
using Mindflur.IMS.Application.Core.Exceptions;
using Mindflur.IMS.Application.ViewModel;
using Mindflur.IMS.Application.ViewModel.Core;
using Mindflur.IMS.Application.ViewModel.View;
using Mindflur.IMS.Data.Models;

namespace Mindflur.IMS.Business
{
    public class CommonTokenBusiness : ICommonTokenBusiness
    {
        private readonly ICommonTokenRepository _tokenRepository;

        public CommonTokenBusiness(ICommonTokenRepository tokenRepository)
        {
            _tokenRepository = tokenRepository;
        }

        public async Task<PaginatedItems<CommonTokenView>> GetAllTokenByTenantId(GetTokenListRequest getListRequest)
        {
            return await _tokenRepository.GetAllTokenByTenantId(getListRequest);
        }

        public async Task<IList<IListTokenView>> GetTokenDetails( int parentTokenId, int tenantId)
        {
            return await _tokenRepository.GetTokenDetails( parentTokenId,tenantId);
        }

        public async Task DeleteToken(int tokenId, int tenantId)
        {
            var data = await _tokenRepository.GetByIdAsync(tokenId);
            if (data == null)
            {
                throw new NotFoundException("Token", tokenId);

            }
            await _tokenRepository.DeleteAsync(data);
        }

        public async Task<CommonTokenDetailsView> GetDetails( int tokenId, int tenantId)
        {
            return await _tokenRepository.GetDetails(tokenId,tenantId);
        }

        public async Task AddToken(AddPostView tokenView, int tenantId)
        {
          CommonToken common=new CommonToken();
             
            common.TenantId = tenantId;
            common.Weightage = tokenView.Weightage;
            common.TokenName = tokenView.TokenName;
            common.DisplayName=tokenView.DisplayName;
            common.SeqeunceNumber = tokenView.SeqeunceNumber;
            common.ParentTokenId = tokenView.ParentTokenId;
            
            await _tokenRepository.AddAsync(common);
        }

        public async Task UpdateToken(AddPostView tokenView, int tokenId, int tenantId)
        {
           
           
            var token=await _tokenRepository.GetByIdAsync(tokenId);
            if (token == null)
            {
                throw new NotFoundException("Token ", tokenId);
            }

            token.TenantId = tenantId;
            token.Weightage = tokenView.Weightage;
            token.TokenName = tokenView.TokenName;
            token.DisplayName= tokenView.DisplayName;
            token.SeqeunceNumber = tokenView.SeqeunceNumber;
            token.ParentTokenId = tokenView.ParentTokenId;

            await _tokenRepository.UpdateAsync(token);
            

        }
		public async Task<IList<TokenResponseView>> getResponseByParentTokenId( int tenantId)
        {

			IList<TokenResponseView> tokenresponseViews = new List<TokenResponseView>();
			var rawData=await _tokenRepository.getTokenResponseAsync( tenantId);
			foreach (var data in rawData)
            {
				tokenresponseViews.Add(new TokenResponseView() { TokenId = data.TokenId, Token = data.TokenName, ParentTokenId = data.ParentTokenId , Parent = data.Parent});
			}
            return tokenresponseViews;  
		}

        public async Task<IList<CommonTokenDetailsView>> GetTokenDropdown(int tenantId)
        {
            return await _tokenRepository.GetTokenDropdown(tenantId);
        }


    }

}