using System.ComponentModel.DataAnnotations;
using FxKit.CompilerServices;

namespace Ciandt.FlowTools.FlowPair.Chats.Models;

[EnumMatch]
public enum LlmModelType
{
    [Display(Name = "GPT 4")] Gpt4,
    [Display(Name = "GPT 4o")] Gpt4o,
    [Display(Name = "GPT 4o Mini")] Gpt4oMini,
    [Display(Name = "GPT 3.5 Turbo")] Gpt35Turbo,
    [Display(Name = "Text Embedding ADA 002")] TextEmbeddingAda002,
    [Display(Name = "Text Embedding Gecko 003")] TextEmbeddingGecko003,
    [Display(Name = "Gemini 1.5 Flash")] Gemini15Flash,
    [Display(Name = "Gemini 1.5 Pro")] Gemini15Pro,
    [Display(Name = "Claude 3 Sonnet")] Claude3Sonnet,
    [Display(Name = "Claude 3.5 Sonnet")] Claude35Sonnet,
    [Display(Name = "Llama 3 70b")] Llama370b
}
