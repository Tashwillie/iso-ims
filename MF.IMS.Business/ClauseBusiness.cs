using Mindflur.IMS.Application.Contracts.Business;
using Mindflur.IMS.Application.Contracts.Repository;
using Mindflur.IMS.Application.Core.Exceptions;
using Mindflur.IMS.Application.ViewModel.Core;
using Mindflur.IMS.Application.ViewModel.Model;
using Mindflur.IMS.Application.ViewModel.View;
using Mindflur.IMS.Data.Models;

namespace Mindflur.IMS.Business
{
    public class ClauseBusiness : IClauseBusiness
    {
        private readonly IClauseRepository _ClauseRepository;

        public ClauseBusiness(IClauseRepository clauseRepository)
        {
            _ClauseRepository = clauseRepository;

        }
        public async Task<PaginatedItems<GetClauseView>> TokenList(GetClauseListRequest getClauseListRequest )
        {
            return await _ClauseRepository.GetClausesList(getClauseListRequest);
        }

        public async Task<IList<GetClauseView>>GetClauseByParentId(int parentId , int tenantId)
        {
           return await _ClauseRepository.GetClauseByParentId(parentId, tenantId);
        }
        public async Task AddClause(PostClauseView clauseView, int tenantId)
        {
            Clause clause = new Clause();
            clause.StandardId = clauseView.StandardId;
            clause.ClauseNumberText = clauseView.ClauseNumberText;
            clause.DisplayText = clauseView.DisplayText;
            clause.ParentId = clauseView.ParentId;
            clause.SequenceNumber = clauseView.SequenceNumber;

            await _ClauseRepository.AddAsync(clause);
        }

        public async Task UpdateClauses(PostClauseView clauseView, int clauseId, int tenantId)
        {
            var clause = await _ClauseRepository.GetByIdAsync(clauseId);
            clause.StandardId = clauseView.StandardId;
            clause.ClauseNumberText = clauseView.ClauseNumberText;
            clause.DisplayText = clauseView.DisplayText;
            clause.ParentId = clauseView.ParentId;
            clause.SequenceNumber = clauseView.SequenceNumber;
            await _ClauseRepository.UpdateAsync(clause);

        }
        public async Task DeleteClauseById(int clauseId, int tenantId)
        {
            var clause = await _ClauseRepository.GetByIdAsync(clauseId);
            if (clause == null) 
            {
                throw new NotFoundException("ClauseId Not Found", clauseId);
            }
            await _ClauseRepository.DeleteAsync(clause);

        }

        public async Task<GetClauseIdView> GetClauseByClauseId(int clauseId, int tenantId)
        {
            return await _ClauseRepository.GetClauseByClauseId(clauseId, tenantId);
        }

        public async Task<IList<GetClauseDropdown>> GetDropDown(int tenant)
        {
            return await _ClauseRepository.GetDropDown(tenant);
        }

		public async Task<IList<ClauseResponseView>> GetClausesResponseAsync(int tenantId)
		{
			IList<ClauseResponseView> clauseResponseViews = new List<ClauseResponseView>();
			var rawData = await _ClauseRepository.getClausesResponseAsync(tenantId);
			foreach (var data in rawData)
			{
				clauseResponseViews.Add(new ClauseResponseView() { ClauseId = data.ClauseId, StandardId = data.StandardId, ClauseNumberText = data.ClauseNumberText, DisplayText = data.DisplayText, ParentId = data.ParentId,Parent=data.Parent });
			}
			return clauseResponseViews;
		}

        public async Task<IList<ClauseTreeResponseView>> GetClausesTreeResponseAsync(int tenantId)
        {
            var clausesList = await _ClauseRepository.getClausesResponseAsync(tenantId);

            var clausesTree = clausesList.Select(clauseItem => new ClauseTreeResponseView { ClauseId = clauseItem.ClauseId, ClauseNumberText=clauseItem.ClauseNumberText, ParentId=clauseItem.ParentId, DisplayText = clauseItem.DisplayText}).ToList();

            foreach (var treeItem in clausesTree)
                treeItem.Children = clausesTree.Where(n => n.ParentId == treeItem.ClauseId).ToList();

            return clausesTree;
        }

        public async Task<IList<GetStandardView>> GetClauseByStandardId(int standardId)
        {
            return await _ClauseRepository.GetClauseByStandardId(standardId);
        }



        public async Task<IList<GetClauseNumberTextView>> GetClausenNmberText(string clauseNumberText)
        {
            return await _ClauseRepository.GetClausenNmberText(clauseNumberText);
        }
    }
}
