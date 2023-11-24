using Application.Commands.Cards.CreateCard;
using Application.Commands.Cards.UpdateCard;
using Domain.Card.ValueObjects;
using Domain.Collection.ValueObjects;
using IntervalLearningApi.Constants;
using IntervalLearningApi.Extensions;
using IntervalLearningApi.Models.ByUser;
using Microsoft.AspNetCore.Mvc;

namespace IntervalLearningApi.Controllers;

public partial class CardsController
{
        [HttpPost(ApiRoutes.Cards.Post_CreateCard)]
        public async Task<ActionResult<CardDto>> CreateCard(short collectionId, [FromBody]CreateCardItem item)
        {
            if (item.Examples != null && item.Examples.Any(e => e.Length > 255))
            {
                return BadRequest();
            }

            var userId = HttpContext.GetUserId();

            if (userId.IsFailed)
                return BadRequest();

            var collectionIdDomain = CollectionId.Create(collectionId).Value;

            if (item.CardId == null)
            {
                var createdResult = await createCardCommand.Handle(new CreateCardRequest()
                {
                    ParentUserId = userId.Value,
                    ParentCollectionId = collectionIdDomain,
                    RememberingText = CardText.Create(item.FrontText).Value,
                    PromptText = item.PromptText == null ? null : CardText.Create(item.PromptText).Value,
                    MeaningText = CardText.Create(item.BackText).Value,
                    Description = item.Description != null ? CardDescription.Create(item.Description).Value : null,
                    Examples = item.Examples != null
                        ? item.Examples.Select(e => CardExample.Create(e).Value).ToList()
                        : new List<CardExample>()
                });
                
                return createdResult.ToActionResult(c => mapper.Map<CardDto>(c));
            }

            var cardResult = await updateCardCommand.Handle(new UpdateCardRequest(){
                CardId = CardId.Create(item.CardId.Value).Value,
                ParentUserId = userId.Value,
                ParentCollectionId = collectionIdDomain,
                RememberingText = CardText.Create(item.FrontText).Value,
                PromptText = item.PromptText == null ? null : CardText.Create(item.PromptText).Value,
                MeaningText = CardText.Create(item.BackText).Value,
                Description = item.Description != null ? CardDescription.Create(item.Description).Value : null,
                Examples = item.Examples != null
                    ? item.Examples.Select(e => CardExample.Create(e).Value).ToList()
                    : new List<CardExample>()
            });
            
            return cardResult.ToActionResult(card => mapper.Map<CardDto>(card));
        }
}