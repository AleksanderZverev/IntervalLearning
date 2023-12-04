using Domain.Dictionary.Word.ValueObjects;

namespace Application.Commands.Dictionary.SearchWords;

public record SearchWordsRequest(
    WordText Text,
    SearchWordType Type,
    int Count
);