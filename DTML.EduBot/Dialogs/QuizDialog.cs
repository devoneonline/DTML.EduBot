﻿namespace DTML.EduBot.Dialogs
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using AdaptiveCards;
    using DTML.EduBot.LessonPlan;
    using Microsoft.Bot.Builder.Dialogs;
    using Microsoft.Bot.Connector;
    using Models;
    using System.Collections.ObjectModel;

    [Serializable]
    public class QuizDialog : IDialog<string>
    {
        private Quiz quiz;

        public QuizDialog(Quiz quiz)
        {
            this.quiz = quiz;
        }

        public async Task StartAsync(IDialogContext context)
        {
            var wasSuccess = await PostAdaptiveCard(context);
            if (!wasSuccess)
            {
                context.Done(Constants.Shared.NoMoreQuizesMessage);
                return;
            }

            context.Wait(this.CheckAnswerOptionsAsync);
        }

        private async Task<bool> PostAdaptiveCard(IDialogContext context)
        {
            var nextQuestion = quiz.Questions.ElementAtOrDefault(quiz.currentQuestion);
            if (nextQuestion == null)
            {
                return false;
            }

            AdaptiveCard adaptiveCard = new AdaptiveCard()
            {
                Body = new List<CardElement>()
                {
                    new TextBlock()
                    {
                        Text = nextQuestion.Question
                    },
                    new Image()
                    {
                        Size = ImageSize.Large,
                        Url  = nextQuestion.ImageUrl
                    }
                }
            };

            Attachment attachment = new Attachment()
            {
                ContentType = AdaptiveCard.ContentType,
                Content = adaptiveCard
            };

            var reply = context.MakeMessage();
            reply.Attachments.Add(attachment);

            await context.PostAsync(reply, CancellationToken.None);
            return true;
        }

        private async Task CheckAnswerOptionsAsync(IDialogContext context, IAwaitable<IMessageActivity> result)
        {
            var studentAnswer = (await result).Text;

            var question = quiz.Questions.ElementAtOrDefault(quiz.currentQuestion);
            if (question == null)
            {
                return;
            }

            // if the answer given is correct
            if (studentAnswer != null && studentAnswer.Equals(question.CorrectAnswer, StringComparison.InvariantCultureIgnoreCase))
            {
                await context.PostAsync(question.CorrectAnswerBotResponse);

                // check if run out of questions
                if (quiz.currentQuestion > quiz.Questions.Count - 1)
                {
                    context.Done("Congrats! You passed the quiz.");
                }
                else
                {
                    quiz.currentQuestion++;
                    await this.StartAsync(context);
                }
            }
            else
            {
                await context.PostAsync(question.WrongAnswerBotResponse);
                await this.StartAsync(context);
            }
        }

    }
}