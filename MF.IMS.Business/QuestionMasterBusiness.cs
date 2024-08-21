using Mindflur.IMS.Application.Contracts.Business;
using Mindflur.IMS.Application.Contracts.Repository;
using Mindflur.IMS.Application.Core.Constants;
using Mindflur.IMS.Application.Core.Exceptions;
using Mindflur.IMS.Application.ViewModel.Model;
using Mindflur.IMS.Data.Models;

namespace Mindflur.IMS.Business
{
    public class QuestionMasterBusiness : IQuestionMasterBusiness
    {
        private readonly IQuestionMasterRepository _questionMasterRepository;
        public QuestionMasterBusiness(IQuestionMasterRepository questionMasterRepository)
        {
            _questionMasterRepository = questionMasterRepository;
        }
        public async Task<IList<QuestionMaster>> GetAllQuestionDetails()
        {
            return await _questionMasterRepository.GetAllQuestionDetails();
        }
        public async Task AddQuestionMasterDetails(QuestionMaster questionMaster)
        {
            await _questionMasterRepository.AddAsync(questionMaster);
        }
        public async Task UpdateQuestionMasterDetails(PutQuestionMasterView questionMaster, int Id)
        {
            var question = await _questionMasterRepository.GetByIdAsync(Id);
            if (question == null)
            {
                throw new NotFoundException(String.Format(ConstantsBusiness.IdNotFoundErrorMessage), Id);
            }
            else
            {
                question.Title = questionMaster.Title;
                await _questionMasterRepository.UpdateAsync(question);
            }


        }
        public async Task DeleteQuestionMasterDetails(int Id)
        {
            var question = await _questionMasterRepository.GetByIdAsync(Id);
            if (question == null)

                throw new NotFoundException(string.Format(ConstantsBusiness.IdNotFoundErrorMessage), Id);



            await _questionMasterRepository.DeleteAsync(question);


        }
        public async Task<QuestionMaster> GetQuestionDetailsById(int Id)
        {
            var question = await _questionMasterRepository.GetByIdAsync(Id);
            return question == null ? throw new NotFoundException(string.Format(ConstantsBusiness.IdNotFoundErrorMessage), Id) : question;
        }
    }
}
