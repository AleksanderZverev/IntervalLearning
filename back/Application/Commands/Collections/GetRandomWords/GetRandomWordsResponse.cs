using DB.Models.Dictionary;
using Domain.Language;

namespace Application.Commands.Collections.GetRandomWords;

public record GetRandomWordsResponse(
    List<LanguageWord> Words, 
    Language Language
);