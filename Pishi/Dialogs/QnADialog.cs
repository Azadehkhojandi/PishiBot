using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using Microsoft.Bot.Builder.CognitiveServices.QnAMaker;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;

namespace PishiBot.Dialogs
{
    [Serializable]
    [QnAMaker("252109f1e2014244855272236c999905", "56c47bcd-0170-42e3-b25a-302ca6e63d58")]
    public class QnADialog: QnAMakerDialog
    {
        private bool acceptableResult(QnAMakerResults results)
        {
            return results.Answers?.Count > 0 && results.Answers.FirstOrDefault()?.Score > 0.5;
        }

        // Override to also include the knowledge base question with the answer on confident matches
        protected override async Task RespondFromQnAMakerResultAsync(IDialogContext context, IMessageActivity message, QnAMakerResults qnaMakerResults)
        {
            if (acceptableResult(qnaMakerResults))
            {
                var chosenQnAPair = qnaMakerResults.Answers.First();
                var response = "Here is the match from FAQ:  \r\n  Q: " + chosenQnAPair.Questions.First() + "  \r\n A: " + qnaMakerResults.Answers.First().Answer;
                await context.PostAsync(response);

            }
            else
            {
                await context.PostAsync("Unfortunately, I don't know the answer of your question.");
            }
           
        }

        protected override async Task DefaultWaitNextMessageAsync(IDialogContext context, IMessageActivity message, QnAMakerResults qnaMakerResults)
        {
            message.Value = acceptableResult(qnaMakerResults);
            context.Done(message);
            await Task.CompletedTask;
        }

        protected override Task QnAFeedbackStepAsync(IDialogContext context, QnAMakerResults qnaMakerResults)
        {
            return base.QnAFeedbackStepAsync(context, qnaMakerResults);
        }

        protected override bool IsConfidentAnswer(QnAMakerResults qnaMakerResults)
        {
            
            return base.IsConfidentAnswer(qnaMakerResults);
        }
        

    }
}