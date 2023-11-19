using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using DB.Models.Dictionary.ValueObjects;
using Domain.Language;
using Domain.Language.ValueObjects;

namespace DB.Models.Dictionary;

public class LanguageWord
{
    public int Id { get; init; }
    public WordText Word { get; set; }
    public WordPronunciation? Pronunciation { get; set; }


    public LanguageId LanguageId { get; set; }
    public Language? Language { get; set; }
}