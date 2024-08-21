using Mindflur.IMS.Application.Contracts.Business;
using Mindflur.IMS.Application.Contracts.Repository;
using Mindflur.IMS.Application.Core.Constants;
using Mindflur.IMS.Application.Core.Exceptions;
using Mindflur.IMS.Application.ViewModel.View;
using Mindflur.IMS.Data.Models;

namespace Mindflur.IMS.Business
{
    public class SurveyQuestionAnswerBusiness : ISurveyQuetionAnswerBusiness
    {
        private readonly ISurveyQuestionAnswerRepository _surveyQuestionAnswerRepository;
        public SurveyQuestionAnswerBusiness(ISurveyQuestionAnswerRepository surveyQuestionAnswerRepository)
        {
            _surveyQuestionAnswerRepository = surveyQuestionAnswerRepository;
        }

        public async Task<IList<GetSurveyQuestionAnswer>> GetAllSurveyQuestionAnswe()
        {
            return await _surveyQuestionAnswerRepository.GetAllSurveyQuestionAnswe();

        }

        public async Task AddSurveyQuestionAnswe(SurveyQuestionAnswer surveyQuestionAnswer)
        {
            await _surveyQuestionAnswerRepository.AddAsync(surveyQuestionAnswer);
        }
        public async Task UpdateSurveyQuestionAnswer(SurveyQuestionAnswer surveyQuestionAnswer, int Id)
        {
            var questionAnswer = await _surveyQuestionAnswerRepository.GetByIdAsync(Id);
            if (questionAnswer == null)
            {
                throw new NotFoundException(string.Format(ConstantsBusiness.IdNotFoundErrorMessage), Id);
            }
            else
            {
                questionAnswer.SurveyQuestionId = surveyQuestionAnswer.SurveyQuestionId;
                questionAnswer.OfferedAnswerId = surveyQuestionAnswer.OfferedAnswerId;
                questionAnswer.SequenceId = surveyQuestionAnswer.SequenceId;
                await _surveyQuestionAnswerRepository.UpdateAsync(questionAnswer);
            }
        }
        public async Task<IList<ListSurveyQuestionAnswers>> GetListForQuestionnsAndOptions(int surveyId, int tenantId)
        {
            IList<ListSurveyQuestionAnswers> surveyCollection = new List<ListSurveyQuestionAnswers>();
            var surveyListQuestionAnswer = await _surveyQuestionAnswerRepository.GetAllQuestionsForSurvey(surveyId, tenantId);
            var surveys = surveyListQuestionAnswer.DistinctBy(survey => survey.SurveyId);
            foreach (var survey in surveys)
            {
                var surveyListForQuestions = surveyListQuestionAnswer.Where(surveyListQuestionAnswer => surveyListQuestionAnswer.SurveyId == survey.SurveyId);
                var questions = surveyListForQuestions.DistinctBy(survey => survey.QuestionId);
                var questionsList = new List<Questions>();
                foreach (var question in questions)
                {
                    var surveyListForAnswers = surveyListForQuestions.Where(surveyListQuestionAnswer => surveyListQuestionAnswer.QuestionId == question.QuestionId);
                    var answers = new List<Answers>();
                    foreach (var answer in surveyListForAnswers)
                    {
                        var answerOptions = new Answers
                        {
                            AnswerId = answer.AnswerId,
                            AnswerText = answer.AnswerText,
                        };
                        answers.Add(answerOptions);

                    }
                    var quetionItems = new Questions
                    {
                        QuestionId = question.QuestionId,
                        QuestionText = question.QuestionText,
                        Options = answers

                    };
                    questionsList.Add(quetionItems);
                }
                var surveyList = new ListSurveyQuestionAnswers
                {
                    SurveyId = survey.SurveyId,
                    SurveyTitle = survey.SurveyTitle,
                    SurveyQuestions = questionsList
                };
                surveyCollection.Add(surveyList);
            }
            return surveyCollection;
        }

    }
}
